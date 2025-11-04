using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BaseNetCore.Core.src.Main.Extensions
{
    /// <summary>
    /// Extension methods for integrating Serilog with BaseNetCore.Core
    /// These are OPTIONAL - consumers can choose to use Serilog or not.
    /// Configuration from appsettings.json is also OPTIONAL - works with defaults if not provided.
    /// </summary>
    public static class SerilogExtensions
    {
        /// <summary>
        /// Adds Serilog with recommended configuration for BaseNetCore applications.
        /// Configuration from appsettings.json is OPTIONAL - if "Serilog" section is missing, uses sensible defaults.
        /// Call this BEFORE builder.Build() in Program.cs
        /// </summary>
        /// <param name="builder">WebApplicationBuilder instance</param>
        /// <param name="configureLogger">Optional: Additional logger configuration</param>
        /// <returns>WebApplicationBuilder for chaining</returns>
        /// <example>
        /// // Minimal usage (no appsettings.json required)
        /// var builder = WebApplication.CreateBuilder(args);
        /// builder.AddBaseNetCoreSerilog();
        /// 
        /// // With custom configuration
        /// builder.AddBaseNetCoreSerilog(config => config.MinimumLevel.Debug());
        /// </example>
        public static WebApplicationBuilder AddBaseNetCoreSerilog(
            this WebApplicationBuilder builder,
            Action<LoggerConfiguration>? configureLogger = null)
        {
            var loggerConfig = new LoggerConfiguration();

            // Try to read from appsettings.json if available
            try
            {
                var serilogSection = builder.Configuration.GetSection("Serilog");
                if (serilogSection.Exists())
                {
                    loggerConfig.ReadFrom.Configuration(builder.Configuration);
                }
                else
                {
                    // Use default configuration if no Serilog section in appsettings.json
                    ApplyDefaultConfiguration(loggerConfig, builder.Environment, builder.Configuration);
                }
            }
            catch
            {
                // Fallback to defaults if configuration reading fails
                ApplyDefaultConfiguration(loggerConfig, builder.Environment, builder.Configuration);
            }

            // Always add these enrichers regardless of config
            loggerConfig
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName ?? "BaseNetCoreApp")
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

            // Allow custom configuration to override everything
            configureLogger?.Invoke(loggerConfig);

            // Create and set the global logger
            Log.Logger = loggerConfig.CreateLogger();

            // Replace default ILogger with Serilog
            builder.Host.UseSerilog();

            return builder;
        }

        /// <summary>
        /// Adds Serilog request logging middleware with BaseNetCore recommended settings.
        /// Call this AFTER app.UseRouting() and BEFORE app.UseBaseNetCoreMiddlewareWithAuth()
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="configureOptions">Optional: Customize request logging options</param>
        /// <returns>IApplicationBuilder for chaining</returns>
        /// <example>
        /// app.UseRouting();
        /// app.UseBaseNetCoreSerilogRequestLogging();
        /// app.UseBaseNetCoreMiddlewareWithAuth();
        /// </example>
        public static IApplicationBuilder UseBaseNetCoreSerilogRequestLogging(
            this IApplicationBuilder app,
            Action<Serilog.AspNetCore.RequestLoggingOptions>? configureOptions = null)
        {
            app.UseSerilogRequestLogging(options =>
            {
                // Default message template
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

                // Customize log level based on response
                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    if (ex != null) return LogEventLevel.Error;
                    if (elapsed > 1000) return LogEventLevel.Warning; // Slow requests
                    if (httpContext.Response.StatusCode > 499) return LogEventLevel.Error;
                    if (httpContext.Response.StatusCode > 399) return LogEventLevel.Warning;
                    return LogEventLevel.Debug; // Normal requests at Debug level
                };

                // Enrich logs with additional properties
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                    diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

                    // Add user info if authenticated
                    if (httpContext.User.Identity?.IsAuthenticated == true)
                    {
                        var userId = httpContext.User.FindFirst("sub")?.Value
                                  ?? httpContext.User.FindFirst("userId")?.Value
                                  ?? httpContext.User.Identity.Name;

                        if (!string.IsNullOrEmpty(userId))
                        {
                            diagnosticContext.Set("UserId", userId);
                        }

                        var username = httpContext.User.Identity.Name;
                        if (!string.IsNullOrEmpty(username))
                        {
                            diagnosticContext.Set("Username", username);
                        }
                    }
                };

                // Allow custom configuration to override
                configureOptions?.Invoke(options);
            });

            return app;
        }

        /// <summary>
        /// Ensures Serilog is properly flushed when application shuts down.
        /// Call this in Program.cs finally block.
        /// </summary>
        /// <param name="app">WebApplication instance (can be null)</param>
        /// <example>
        /// try
        /// {
        ///     var app = builder.Build();
        ///     app.Run();
        /// }
        /// finally
        /// {
        ///     app.FlushBaseNetCoreSerilog();
        /// }
        /// </example>
        public static void FlushBaseNetCoreSerilog(this WebApplication? app)
        {
            Log.CloseAndFlush();
        }

        #region Private Helper Methods

        /// <summary>
        /// Apply default Serilog configuration when no appsettings.json config is found
        /// </summary>
        private static void ApplyDefaultConfiguration(
            LoggerConfiguration loggerConfig,
            IHostEnvironment environment,
            IConfiguration configuration)
        {
            // Set minimum level based on environment
            var defaultLevel = environment.IsDevelopment()
                ? LogEventLevel.Debug
                : LogEventLevel.Information;

            loggerConfig
                .MinimumLevel.Is(defaultLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("BaseNetCore.Core", LogEventLevel.Debug);

            // Write to Console by default
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}");

            // Read log path from configuration or use default
            var logPath = configuration.GetValue<string>("Logging:FilePath");

            if (string.IsNullOrWhiteSpace(logPath))
            {
                // Use ContentRootPath for default path
                logPath = Path.Combine(environment.ContentRootPath, "logs", "log-.txt");
            }
            else if (!Path.IsPathRooted(logPath))
            {
                // If relative path, combine with ContentRootPath
                logPath = Path.Combine(environment.ContentRootPath, logPath);
            }

            // In production, write to File
            if (environment.IsProduction())
            {
                // Read additional file logging settings
                var retainedFileCountLimit = configuration.GetValue<int?>("Logging:RetainedFileCountLimit") ?? 30;
                var fileSizeLimitBytes = configuration.GetValue<long?>("Logging:FileSizeLimitBytes") ?? 10_485_760; // 10MB
                var rollingInterval = configuration.GetValue<string>("Logging:RollingInterval");

                var interval = rollingInterval?.ToLowerInvariant() switch
                {
                    "hour" => RollingInterval.Hour,
                    "day" => RollingInterval.Day,
                    "month" => RollingInterval.Month,
                    "year" => RollingInterval.Year,
                    _ => RollingInterval.Day
                };

                loggerConfig.WriteTo.File(
                    path: logPath,
                    rollingInterval: interval,
                    retainedFileCountLimit: retainedFileCountLimit,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
            }
        }

        #endregion
    }
}