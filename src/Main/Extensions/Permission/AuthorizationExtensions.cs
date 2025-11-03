using BaseNetCore.Core.src.Main.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BaseNetCore.Core.src.Main.Extensions.Permission
{
    /// <summary>
    /// Provides extension methods for registering dynamic authorization services in the dependency injection container.
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Registers core dynamic authorization services, including dynamic permission provider, permission repository, and user permission service.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddBaseCoreDynamicAuthorization(this IServiceCollection services)
        {
            // Provider for dynamic rules (defaults to IConfiguration-based provider)
            services.AddSingleton<IDynamicPermissionProvider, DefaultDynamicPermissionProvider>();

            // Repository: default in-memory repo from core (application can replace with EF-backed repo)
            DIUntils.AddAutoRegisterDI<ICorePermissionService>(services);

            // IUserPermissionService backed by IDistributedCache (app must register IDistributedCache implementation)
            services.AddScoped<IUserPermissionService, CachedUserPermissionService>();

            return services;
        }

        public static IApplicationBuilder UseBaseCoreDynamicPermissionMiddleware(this IApplicationBuilder app)
        {
            // Ensure app.UseAuthentication() is called before this middleware in Program.cs
            return app.UseMiddleware<DynamicPermissionMiddleware>();
        }

    }

}