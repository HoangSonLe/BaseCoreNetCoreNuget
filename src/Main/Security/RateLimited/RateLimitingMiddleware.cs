using BaseNetCore.Core.src.Main.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.RateLimiting;

namespace BaseNetCore.Core.src.Main.Security.RateLimited
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimiter _rateLimiter;
        private readonly RateLimitOptions _options;
        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimiter rateLimiter, RateLimitOptions rateLimiterOptions)
        {
            _next = next;
            _logger = logger;
            _rateLimiter = rateLimiter;
            _options = rateLimiterOptions;

        }
        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu rate limiting bị tắt
            // Bỏ qua rate limiting cho các endpoint được whitelist
            if (!_options.Enabled || IsWhitelisted(context))
            {
                await _next(context);
                return;
            }

            var identifier = GetIdentifier(context);

            using (var lease = await _rateLimiter.AcquireAsync(1, context.RequestAborted))
            {
                if (lease.IsAcquired)
                {
                    // Thêm rate limit headers nếu được cấu hình
                    if (_options.IncludeRateLimitHeaders)
                    {
                        AddRateLimitHeaders(context, lease);
                    }
                    // Cho phép request tiếp tục
                    await _next(context);
                }
                else
                {
                    await HandleRateLimitExceeded(context, identifier);
                }
            }


        }
        private bool IsWhitelisted(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            return _options.WhitelistedPaths.Any(p => path.StartsWith(p.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
        }
        private void AddRateLimitHeaders(HttpContext context, RateLimitLease lease)
        {
            if (lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.Response.Headers["X-RateLimit-Retry-After"] = retryAfter.ToString();
            }
            context.Response.Headers["X-RateLimit-Limit"] = _options.PermitLimit.ToString();
            if (!string.IsNullOrEmpty(_options.PolicyName))
            {
                context.Response.Headers["X-RateLimit-Policy"] = _options.PolicyName;
            }
        }
        private async Task HandleRateLimitExceeded(HttpContext context, string identifier)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            var retryAfterSeconds = _options.RetryAfterSeconds;
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = _options.PermitLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddSeconds(retryAfterSeconds).ToUnixTimeSeconds().ToString();

            var message = !string.IsNullOrEmpty(_options.RateLimitExceededMessage) ? _options.RateLimitExceededMessage : "Too many requests. Please try again later.";
            var response = new ApiErrorResponse()
            {
                Guid = Guid.NewGuid().ToString(),
                Code = context.Response.StatusCode.ToString(),
                Message = message,
                Method = context.Request.Method,
                Path = context.Request.Path,
                Timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsJsonAsync(response);

            _logger.LogWarning(
               "Rate limit exceeded for identifier: {Identifier}, Path: {Path}, Method: {Method}",
               identifier,
               context.Request.Path,
               context.Request.Method);

        }
        private string GetIdentifier(HttpContext context)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                // Sử dụng user ID làm identifier nếu user đã authenticate
                var userId = context.User.FindFirst("sub")?.Value  // Giả sử sử dụng claim "sub" làm user ID
                    ?? context.User.FindFirst("userId")?.Value
                    ?? context.User.Identity.Name;
                if (!string.IsNullOrEmpty(userId))
                {
                    return $"user:{userId}";
                }
            }
            // Sử dụng IP của client làm identifier
            var ipAddress = context.Connection.RemoteIpAddress?.ToString()
                ?? context.Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault()
            ?? "unknown";
            return $"ip:{ipAddress}";
        }

    }
}
