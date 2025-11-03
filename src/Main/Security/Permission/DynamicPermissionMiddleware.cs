using BaseNetCore.Core.src.Main.Common.Contants;
using BaseNetCore.Core.src.Main.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BaseNetCore.Core.src.Main.Security.Permission
{
    /// <summary>
    /// Enterprise-grade Dynamic Permission Middleware
    /// Optimized to reuse HttpContext scope instead of creating new scope
    /// </summary>
    public class DynamicPermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDynamicPermissionProvider _provider;
        private readonly ILogger<DynamicPermissionMiddleware> _logger;

        // OPTIMIZATION: Remove IServiceProvider injection - use HttpContext.RequestServices
        public DynamicPermissionMiddleware(
            RequestDelegate next,
            IDynamicPermissionProvider provider,
            ILogger<DynamicPermissionMiddleware> logger)
        {
            _next = next;
            _provider = provider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // If the endpoint is marked with [AllowAnonymous], skip permission checks entirely.
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? "/";
            var method = context.Request.Method.ToUpperInvariant();

            // Check PermitAll first (fast path)
            var permitAll = await _provider.GetPermitAllAsync();
            if (permitAll.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Find matching rule
            var rules = await _provider.GetRulesAsync();
            var rule = rules.FirstOrDefault(r => r.HttpMethod == method && r.PathRegex.IsMatch(path));

            if (rule == null)
            {
                // No dynamic rule -> deny access
                _logger.LogWarning("No permission rule found for {Method} {Path}", method, path);
                await WriteError(context, StatusCodes.Status403Forbidden, CoreErrorCodes.FORBIDDEN);
                return;
            }

            // Check authentication
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await WriteError(context, StatusCodes.Status401Unauthorized, CoreErrorCodes.SYSTEM_AUTHORIZATION);
                return;
            }

            // OPTIMIZATION: Reuse HttpContext scope instead of creating new one
            var userPermissionService = context.RequestServices.GetService<IUserPermissionService>();
            if (userPermissionService == null)
            {
                _logger.LogError("IUserPermissionService not registered in DI container");
                await WriteError(context, StatusCodes.Status500InternalServerError, CoreErrorCodes.SYSTEM_ERROR, "Internal server error");
                return;
            }

            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                await WriteError(context, StatusCodes.Status401Unauthorized, CoreErrorCodes.SYSTEM_AUTHORIZATION);
                return;
            }

            // OPTIMIZATION: Use HashSet with case-insensitive comparer from start
            var userPerms = await userPermissionService.GetPermissionsAsync(userId);
            var userPermSet = new HashSet<string>(userPerms, StringComparer.OrdinalIgnoreCase);

            // Check permissions
            var hasPermission = rule.RequiredPermissions.Count == 0 || rule.RequiredPermissions.Any(rp => userPermSet.Contains(rp));

            if (!hasPermission)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Authorization failed for {Path} {Method}. Required: {Required}, UserPerms: {UserPerms}",
                        path, method, string.Join(",", rule.RequiredPermissions), string.Join(",", userPerms));
                }

                await WriteError(context, StatusCodes.Status403Forbidden, CoreErrorCodes.FORBIDDEN);
                return;
            }

            await _next(context);
        }

        private static Task WriteError(HttpContext context, int status, CoreErrorCodes code, string? message = null)
        {
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";

            var payload = new ApiErrorResponse
            {
                Guid = context.TraceIdentifier,
                Code = code.Code,
                Message = message ?? code.Message,
                Path = context.Request.Path,
                Method = context.Request.Method,
                Timestamp = DateTime.UtcNow
            };

            // OPTIMIZATION: Use pre-configured JsonSerializerOptions
            var json = JsonSerializer.Serialize(payload, JsonSerializerOptionsCache.CamelCase);
            return context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// Cache JsonSerializerOptions to avoid recreation
    /// </summary>
    internal static class JsonSerializerOptionsCache
    {
        public static readonly JsonSerializerOptions CamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultBufferSize = 1024 // Reduce allocation
        };
    }
}
