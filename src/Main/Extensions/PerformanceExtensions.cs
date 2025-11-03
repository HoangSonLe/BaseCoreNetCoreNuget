using BaseNetCore.Core.src.Main.Common.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseNetCore.Core.src.Main.Extensions
{
    /// <summary>
    /// Performance optimization extensions for ASP.NET Core applications
    /// Provides Response Compression, Caching, and Kestrel configuration
    /// </summary>
    public static class PerformanceExtensions
    {
        /// <summary>
        /// Add performance optimization services
        /// Configuration section "PerformanceOptimization" is OPTIONAL - uses defaults if missing
        /// </summary>
        public static IServiceCollection AddBaseNetCorePerformanceOptimization(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<PerformanceOptions>? configureOptions = null)
        {
            // Read options from config (or use defaults)
            var options = new PerformanceOptions();
            var configSection = configuration.GetSection(PerformanceOptions.SectionName);

            if (configSection.Exists())
            {
                configSection.Bind(options);
            }

            // Allow programmatic override
            configureOptions?.Invoke(options);

            // Register options for DI
            services.Configure<PerformanceOptions>(opt =>
            {
                opt.EnableResponseCompression = options.EnableResponseCompression;
                opt.CompressionEnableForHttps = options.CompressionEnableForHttps;
                opt.BrotliCompressionLevel = options.BrotliCompressionLevel;
                opt.GzipCompressionLevel = options.GzipCompressionLevel;
                opt.EnableResponseCaching = options.EnableResponseCaching;
                opt.ResponseCacheMaxBodySize = options.ResponseCacheMaxBodySize;
                opt.ResponseCacheCaseSensitive = options.ResponseCacheCaseSensitive;
                opt.EnableOutputCache = options.EnableOutputCache;
                opt.OutputCacheExpirationSeconds = options.OutputCacheExpirationSeconds;
            });

            // === Response Compression ===
            if (options.EnableResponseCompression)
            {
                services.AddResponseCompression(compressionOptions =>
                {
                    compressionOptions.EnableForHttps = options.CompressionEnableForHttps;
                    compressionOptions.Providers.Add<BrotliCompressionProvider>();
                    compressionOptions.Providers.Add<GzipCompressionProvider>();
                });

                services.Configure<BrotliCompressionProviderOptions>(brotliOptions =>
                {
                    brotliOptions.Level = options.GetBrotliCompressionLevel();
                });

                services.Configure<GzipCompressionProviderOptions>(gzipOptions =>
                {
                    gzipOptions.Level = options.GetGzipCompressionLevel();
                });
            }

            // === Response Caching ===
            if (options.EnableResponseCaching)
            {
                services.AddResponseCaching(cachingOptions =>
                {
                    cachingOptions.MaximumBodySize = options.ResponseCacheMaxBodySize;
                    cachingOptions.UseCaseSensitivePaths = options.ResponseCacheCaseSensitive;
                });
            }

            // === Output Cache (.NET 7+) ===
            if (options.EnableOutputCache)
            {
                services.AddOutputCache(outputCacheOptions =>
                {
                    outputCacheOptions.AddBasePolicy(policyBuilder =>
                        policyBuilder.Expire(TimeSpan.FromSeconds(options.OutputCacheExpirationSeconds)));
                });
            }

            return services;
        }

        /// <summary>
        /// Configure Kestrel server limits for high performance
        /// Configuration section "KestrelLimits" is OPTIONAL - uses defaults if missing
        /// </summary>
        public static IServiceCollection AddBaseNetCoreKestrelOptimization(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<KestrelLimitsOptions>? configureOptions = null)
        {
            var options = new KestrelLimitsOptions();
            var configSection = configuration.GetSection(KestrelLimitsOptions.SectionName);

            if (configSection.Exists())
            {
                configSection.Bind(options);
            }

            configureOptions?.Invoke(options);

            // Configure Kestrel (must be called on WebHostBuilder, not here)
            // This is just for registering options
            services.Configure<KestrelLimitsOptions>(opt =>
            {
                opt.MaxConcurrentConnections = options.MaxConcurrentConnections;
                opt.MaxConcurrentUpgradedConnections = options.MaxConcurrentUpgradedConnections;
                opt.MaxRequestBodySize = options.MaxRequestBodySize;
                opt.KeepAliveTimeoutMinutes = options.KeepAliveTimeoutMinutes;
                opt.RequestHeadersTimeoutSeconds = options.RequestHeadersTimeoutSeconds;
            });

            return services;
        }

        /// <summary>
        /// Use performance optimization middleware
        /// </summary>
        public static IApplicationBuilder UseBaseNetCorePerformanceOptimization(
            this IApplicationBuilder app,
            IConfiguration? configuration = null)
        {
            // Get options from DI or use defaults
            var options = new PerformanceOptions();
            if (configuration != null)
            {
                var configSection = configuration.GetSection(PerformanceOptions.SectionName);
                if (configSection.Exists())
                {
                    configSection.Bind(options);
                }
            }

            // Response Compression - MUST be before UseStaticFiles
            if (options.EnableResponseCompression)
            {
                app.UseResponseCompression();
            }

            // Response Caching - After UseRouting
            if (options.EnableResponseCaching)
            {
                app.UseResponseCaching();
            }

            // Output Cache
            if (options.EnableOutputCache)
            {
                app.UseOutputCache();
            }

            return app;
        }

        /// <summary>
        /// Configure WebHostBuilder with Kestrel optimization
        /// Call this from Program.cs: builder.WebHost.UseBaseNetCoreKestrelOptimization(builder.Configuration)
        /// </summary>
        public static IWebHostBuilder UseBaseNetCoreKestrelOptimization(
            this IWebHostBuilder webHostBuilder,
            IConfiguration configuration,
            Action<KestrelLimitsOptions>? configureOptions = null)
        {
            var options = new KestrelLimitsOptions();
            var configSection = configuration.GetSection(KestrelLimitsOptions.SectionName);

            if (configSection.Exists())
            {
                configSection.Bind(options);
            }

            configureOptions?.Invoke(options);

            webHostBuilder.ConfigureKestrel(serverOptions =>
            {
                if (options.MaxConcurrentConnections.HasValue)
                {
                    serverOptions.Limits.MaxConcurrentConnections = options.MaxConcurrentConnections;
                }

                if (options.MaxConcurrentUpgradedConnections.HasValue)
                {
                    serverOptions.Limits.MaxConcurrentUpgradedConnections = options.MaxConcurrentUpgradedConnections;
                }

                if (options.MaxRequestBodySize.HasValue)
                {
                    serverOptions.Limits.MaxRequestBodySize = options.MaxRequestBodySize;
                }

                serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(options.KeepAliveTimeoutMinutes);
                serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(options.RequestHeadersTimeoutSeconds);
            });

            return webHostBuilder;
        }
    }
}