using BaseNetCore.Core.src.Main.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.RateLimiting;

namespace BaseNetCore.Core.src.Main.Security.RateLimited
{
    /// <summary>
    /// Enterprise-grade Rate Limiting Middleware with PartitionedRateLimiter
    /// Optimized for performance and memory efficiency
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly PartitionedRateLimiter<HttpContext> _rateLimiter;
        private readonly RateLimitOptions _options;

        public RateLimitingMiddleware(
  RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
       PartitionedRateLimiter<HttpContext> rateLimiter,
     RateLimitOptions rateLimiterOptions)
        {
            _next = next;
            _logger = logger;
            _rateLimiter = rateLimiter;
            _options = rateLimiterOptions;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu rate limiting bị tắt hoặc path được whitelist
            if (!_options.Enabled || IsWhitelisted(context))
            {
                await _next(context);
                return;
            }

            // OPTIMIZATION: Sử dụng PartitionedRateLimiter.AcquireAsync trực tiếp
            // Automatic partition cleanup khi không còn sử dụng
            using var lease = await _rateLimiter.AcquireAsync(context, 1, context.RequestAborted);

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
                // OPTIMIZATION: Extract identifier chỉ khi cần log
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    var identifier = GetIdentifierForLogging(context);
                    await HandleRateLimitExceeded(context, identifier);
                }
                else
                {
                    await HandleRateLimitExceeded(context, null);
                }
            }
        }

        private bool IsWhitelisted(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // OPTIMIZATION: Cache lowercase path để tránh allocation
            var lowerPath = path.ToLowerInvariant();

            // OPTIMIZATION: Sử dụng Span-based comparison nếu .NET 8+
            foreach (var whitelistedPath in _options.WhitelistedPaths)
            {
                if (lowerPath.StartsWith(whitelistedPath.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddRateLimitHeaders(HttpContext context, RateLimitLease lease)
        {
            // OPTIMIZATION: Avoid boxing với ToString() nếu có thể
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

        private async Task HandleRateLimitExceeded(HttpContext context, string? identifier)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            var retryAfterSeconds = _options.RetryAfterSeconds;
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = _options.PermitLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddSeconds(retryAfterSeconds).ToUnixTimeSeconds().ToString();

            var message = !string.IsNullOrEmpty(_options.RateLimitExceededMessage)
             ? _options.RateLimitExceededMessage
            : "Too many requests. Please try again later.";

            var response = new ApiErrorResponse
            {
                Guid = context.TraceIdentifier, // Reuse TraceIdentifier thay vì tạo mới Guid
                Code = StatusCodes.Status429TooManyRequests.ToString(),
                Message = message,
                Method = context.Request.Method,
                Path = context.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);

            // OPTIMIZATION: Chỉ log khi có identifier
            if (!string.IsNullOrEmpty(identifier))
            {
                _logger.LogWarning(
            "Rate limit exceeded for identifier: {Identifier}, Path: {Path}, Method: {Method}",
             identifier,
           context.Request.Path,
           context.Request.Method);
            }
        }

        /// <summary>
        /// OPTIMIZATION: Extract identifier chỉ khi cần log (lazy evaluation)
        /// </summary>
        private static string GetIdentifierForLogging(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("sub")?.Value
        ?? context.User.FindFirst("userId")?.Value
          ?? context.User.Identity.Name;

                if (!string.IsNullOrEmpty(userId))
                {
                    return $"user:{userId}";
                }
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString()
                        ?? context.Request.Headers["X-Forwarded-For"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim()
            ?? "unknown";

            return $"ip:{ipAddress}";
        }
    }
}
