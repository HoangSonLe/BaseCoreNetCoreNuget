using BaseNetCore.Core.src.Main.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseNetCore.Core.src.Main.Extensions
{
    /// <summary>
    /// Unified extension methods for configuring all BaseNetCore features.
    /// </summary>
    public static class BaseNetCoreExtensions
    {
        /// <summary>
        /// Adds all BaseNetCore features with recommended configuration:
        /// - Automatic model validation with ApiErrorResponse format
        /// - Controllers with JSON options
        /// 
        /// This is the recommended way to configure BaseNetCore features.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>IMvcBuilder for further MVC configuration</returns>
        public static IMvcBuilder AddBaseNetCoreFeatures(this IServiceCollection services)
        {
            // Add automatic model validation with ApiErrorResponse format (recommended)
            services.AddAutomaticModelValidation();

            // Add and return MvcBuilder for further configuration
            return services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
        }

        /// <summary>
        /// Adds BaseNetCore features with JWT authentication.
        /// Includes:
        /// - JWT authentication and authorization
        /// - Automatic model validation with ApiErrorResponse format
        /// - Controllers with JSON options
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration containing TokenSettings</param>
        /// <param name="tokenSettingsSectionName">Section name for TokenSettings (default: "TokenSettings")</param>
        /// <returns>IMvcBuilder for further MVC configuration</returns>
        public static IMvcBuilder AddBaseNetCoreFeaturesWithAuth(
            this IServiceCollection services,
            IConfiguration configuration,
            string tokenSettingsSectionName = "TokenSettings")
        {
            // Add JWT authentication
            services.AddJwtAuthentication(configuration, tokenSettingsSectionName);

            // Add base features
            return services.AddBaseNetCoreFeatures();
        }

        /// <summary>
        /// Adds BaseNetCore middleware to the application pipeline.
        /// Includes:
        /// - Global exception handling middleware
        /// 
        /// Note: This should be called early in the middleware pipeline.
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder for chaining</returns>
        public static IApplicationBuilder UseBaseNetCoreMiddleware(this IApplicationBuilder app)
        {
            // Add global exception handling
            app.UseMiddleware<GlobalExceptionMiddleware>();

            return app;
        }

        /// <summary>
        /// Adds complete BaseNetCore middleware pipeline with authentication.
        /// Includes:
        /// - Global exception handling
        /// - Authentication
        /// - Authorization
        /// 
        /// Note: This should be called after UseRouting() and before UseEndpoints().
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder for chaining</returns>
        public static IApplicationBuilder UseBaseNetCoreMiddlewareWithAuth(this IApplicationBuilder app)
        {
            // Add global exception handling
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Add authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}
