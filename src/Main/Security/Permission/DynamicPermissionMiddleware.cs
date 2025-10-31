using BaseNetCore.Core.src.Main.Common.Contants;
using BaseNetCore.Core.src.Main.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BaseNetCore.Core.src.Main.Security.Permission
{
    public class DynamicPermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDynamicPermissionProvider _provider;
        private readonly ILogger<DynamicPermissionMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        public DynamicPermissionMiddleware(RequestDelegate next, IDynamicPermissionProvider provider,
            IServiceProvider serviceProvider,
            ILogger<DynamicPermissionMiddleware> logger)
        {
            _next = next;
            _provider = provider;
            _serviceProvider = serviceProvider;
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
            var permitAll = await _provider.GetPermitAllAsync();
            if (permitAll.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }
            var rules = await _provider.GetRulesAsync();
             var rule = rules.FirstOrDefault(r => r.HttpMethod == method && r.PathRegex.IsMatch(path));
            if (rule == null)
            {
                // No dynamic rule -> continue (choose default-deny if you prefer)
                //await _next(context);

                _logger.LogError("IUserPermissionService not registered in DI container");
                await WriteError(context, StatusCodes.Status403Forbidden, CoreErrorCodes.FORBIDDEN);

                return;
            }
            if (!context.User.Identity?.IsAuthenticated ?? false)
            {
                await WriteError(context, StatusCodes.Status401Unauthorized, CoreErrorCodes.SYSTEM_AUTHORIZATION);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var _userPermissionService = scope.ServiceProvider.GetService<IUserPermissionService>();
            if (_userPermissionService == null)
            {
                _logger.LogError("IUserPermissionService not registered in DI container");
                await WriteError(context, StatusCodes.Status500InternalServerError, CoreErrorCodes.SYSTEM_ERROR, "Internal server error");
                return;
            }

            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userPerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(userId))
            {
                var perms = await _userPermissionService.GetPermissionsAsync(userId);
                foreach (var p in perms) userPerms.Add(p);
            }

            var ok = rule.RequiredPermissions.Count == 0 || rule.RequiredPermissions.Any(rp => userPerms.Contains(rp));
            if (!ok)
            {
                _logger.LogInformation("Authorization failed for {Path} {Method}. Required: {Req}, UserPerms: {UserPerms}", path, method, string.Join(",", rule.RequiredPermissions), string.Join(",", userPerms));
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
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(payload, options);
            return context.Response.WriteAsync(json);
        }
    }
}
