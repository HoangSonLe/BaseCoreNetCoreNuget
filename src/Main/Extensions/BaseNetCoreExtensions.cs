using BaseNetCore.Core.src.Main.Cache;
using BaseNetCore.Core.src.Main.Extensions.Token;
using BaseNetCore.Core.src.Main.GlobalMiddleware;
using BaseNetCore.Core.src.Main.Security.Algorithm;
using BaseNetCore.Core.src.Main.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        /// - Optional: Serilog logging
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="builder">WebApplicationBuilder for Serilog configuration (optional)</param>
        /// <param name="configureSerilog">Optional: Custom Serilog configuration</param>
        /// <returns>IMvcBuilder for further MVC configuration</returns>
        public static IMvcBuilder AddBaseNetCoreFeatures(
            this IServiceCollection services,
            IConfiguration configuration,
            WebApplicationBuilder? builder = null,
            Action<LoggerConfiguration>? configureSerilog = null)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            // If WebApplicationBuilder is provided, configure Serilog
            if (builder != null)
            {
                builder.AddBaseNetCoreSerilog(configureSerilog);
            }

            // Add automatic model validation with ApiErrorResponse format (recommended)
            services.AddBaseAutomaticModelValidation();
            services.AddBaseServiceDependencies();
            services.AddAesAlgorithmConfiguration(configuration);

            // Add and return MvcBuilder for further configuration
            return ConfigureControllers(services);
        }

        /// <summary>
        /// Adds BaseNetCore features with JWT authentication.
        /// Includes:
        /// - JWT authentication and authorization
        /// - Automatic model validation with ApiErrorResponse format
        /// - Controllers with JSON options
        /// - Optional: Serilog logging
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration containing TokenSettings</param>
        /// <param name="tokenSettingsSectionName">Section name for TokenSettings (default: "TokenSettings")</param>
        /// <param name="isUseMemoryCache">Enable memory cache (default: true)</param>
        /// <param name="builder">WebApplicationBuilder for Serilog configuration (optional)</param>
        /// <param name="configureSerilog">Optional: Custom Serilog configuration</param>
        /// <returns>IMvcBuilder for further MVC configuration</returns>
        public static IMvcBuilder AddBaseNetCoreFeaturesWithAuth(
            this IServiceCollection services,
            IConfiguration configuration,
            string tokenSettingsSectionName = "TokenSettings",
            bool isUseMemoryCache = true,
            WebApplicationBuilder? builder = null,
            Action<LoggerConfiguration>? configureSerilog = null)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(tokenSettingsSectionName))
                throw new ArgumentException("Token settings section name must be provided.", nameof(tokenSettingsSectionName));

            // Add JWT authentication
            services.AddBaseJwtAuthentication(configuration, tokenSettingsSectionName);

            // Auto-register application-provided ITokenValidator (if any implementation exists in loaded assemblies)
            DIUntils.AddAutoRegisterDI<ITokenValidator>(services);

            // Add memory cache for token-related caching scenarios
            if (isUseMemoryCache)
            {
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, MemoryCacheService>();
            }

            // Add base features (with optional Serilog)
            return services.AddBaseNetCoreFeatures(configuration, builder, configureSerilog);
        }

        /// <summary>
        /// Registers AES algorithm and its settings.
        /// Validates configuration immediately and registers a singleton AesAlgorithm instance.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration containing AES settings</param>
        /// <param name="aesSettingsSectionName">Section name for AES settings (default: "Aes")</param>
        /// <returns>IServiceCollection for chaining</returns>
        public static IServiceCollection AddAesAlgorithmConfiguration(
            this IServiceCollection services,
            IConfiguration configuration,
            string aesSettingsSectionName = "Aes")
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(aesSettingsSectionName))
                throw new ArgumentException("AES settings section name must be provided.", nameof(aesSettingsSectionName));

            // Bind settings so they are available via IOptions if needed elsewhere
            services.Configure<AesSettings>(configuration.GetSection(aesSettingsSectionName));

            // Read bound values now and fail fast if SecretKey is missing
            var bound = configuration.GetSection(aesSettingsSectionName).Get<AesSettings>();
            if (bound == null || string.IsNullOrWhiteSpace(bound.SecretKey))
            {
                throw new InvalidOperationException($"Configuration section '{aesSettingsSectionName}' is missing or does not contain a valid SecretKey. Set '{aesSettingsSectionName}:SecretKey' in configuration.");
            }

            // Register a pre-constructed singleton using the validated secret to avoid timing/order issues
            services.AddSingleton(new AesAlgorithm(bound.SecretKey));

            return services;
        }

        /// <summary>
        /// Registers BaseService dependencies.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddBaseServiceDependencies(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            services.AddHttpContextAccessor();
            return services;
        }

        /// <summary>
        /// Adds BaseNetCore middleware to the application pipeline.
        /// Includes:
        /// - Correlation ID tracking (X-Correlation-ID header)
        /// - Serilog request logging (if Serilog is configured)
        /// - Global exception handling middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="configureSerilogRequestLogging">Optional: Customize Serilog request logging options</param>
        /// <returns>Application builder for chaining</returns>
        public static IApplicationBuilder UseBaseNetCoreMiddleware(
            this IApplicationBuilder app,
            Action<RequestLoggingOptions>? configureSerilogRequestLogging = null)
        {
            if (app is null) throw new ArgumentNullException(nameof(app));

            // Add Correlation ID tracking
            app.UseMiddleware<CorrelationIdMiddleware>();

            // Add Serilog request logging if Serilog is configured
            if (Log.Logger != null && Log.Logger != Serilog.Core.Logger.None)
            {
                app.UseBaseNetCoreSerilogRequestLogging(configureSerilogRequestLogging);
            }

            // Add global exception handling
            app.UseMiddleware<GlobalExceptionMiddleware>();

            return app;
        }

        /// <summary>
        /// Adds complete BaseNetCore middleware pipeline with authentication.
        /// Includes:
        /// - Correlation ID tracking (X-Correlation-ID header)
        /// - Serilog request logging (if Serilog is configured)
        /// - Global exception handling
        /// - Authentication
        /// - Authorization
        /// Note: Should be called after UseRouting() and before UseEndpoints().
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="configureSerilogRequestLogging">Optional: Customize Serilog request logging options</param>
        /// <returns>Application builder for chaining</returns>
        public static IApplicationBuilder UseBaseNetCoreMiddlewareWithAuth(
            this IApplicationBuilder app,
            Action<RequestLoggingOptions>? configureSerilogRequestLogging = null)
        {
            if (app is null) throw new ArgumentNullException(nameof(app));

            // Add Correlation ID tracking
            app.UseMiddleware<CorrelationIdMiddleware>();

            // Add Serilog request logging if Serilog is configured
            if (Log.Logger != null && Log.Logger != Serilog.Core.Logger.None)
            {
                app.UseBaseNetCoreSerilogRequestLogging(configureSerilogRequestLogging);
            }

            // Add global exception handling
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Add authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        #region PRIVATE

        /// <summary>
        /// Private helper to centralize controller setup and JSON options to avoid duplication.
        /// </summary>
        private static IMvcBuilder ConfigureControllers(IServiceCollection services)
        {
            return services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
        }

        #endregion
    }
}