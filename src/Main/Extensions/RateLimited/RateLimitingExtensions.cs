using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace BaseNetCore.Core.src.Main.Extensions.RateLimited
{
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// Đăng ký Rate Limiting với PartitionedRateLimiter (Enterprise-grade)
        /// Sử dụng partitioning để tránh memory leak và improve performance
        /// </summary>
        public static IServiceCollection AddBaseRateLimiting(
            this IServiceCollection services,
            IConfiguration? configuration = null,
            Action<RateLimitOptions>? configure = null)
        {
            // Tạo options với giá trị mặc định
            var options = new RateLimitOptions();

            // Bind từ configuration nếu có
            if (configuration != null)
            {
                var section = configuration.GetSection(RateLimitOptions.SectionName);
                if (section.Exists())
                {
                    section.Bind(options);
                }
            }

            // Override bằng action nếu có
            configure?.Invoke(options);

            // Đăng ký options
            services.AddSingleton(options);

            // Tạo và đăng ký PartitionedRateLimiter - ENTERPRISE OPTIMIZATION
            services.AddSingleton<PartitionedRateLimiter<HttpContext>>(sp =>
            {
                var opts = sp.GetRequiredService<RateLimitOptions>();
                return CreatePartitionedRateLimiter(opts);
            });

            return services;
        }

        public static IApplicationBuilder UseBaseRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingMiddleware>();
        }

        /// <summary>
        /// Tạo PartitionedRateLimiter với automatic cleanup và memory optimization
        /// </summary>
        private static PartitionedRateLimiter<HttpContext> CreatePartitionedRateLimiter(RateLimitOptions options)
        {
            return PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                // Extract identifier from context
                var identifier = GetIdentifier(context);

                // Create partition based on rate limit type
                return options.Type switch
                {
                    RateLimitType.Fixed => RateLimitPartition.GetFixedWindowLimiter(identifier, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = options.PermitLimit,
                            Window = TimeSpan.FromSeconds(options.WindowSeconds),
                            QueueLimit = options.QueueLimit,
                            AutoReplenishment = true
                        }),

                    RateLimitType.Sliding => RateLimitPartition.GetSlidingWindowLimiter(identifier, _ =>
                        new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = options.PermitLimit,
                            Window = TimeSpan.FromSeconds(options.WindowSeconds),
                            SegmentsPerWindow = 10,
                            QueueLimit = options.QueueLimit,
                            AutoReplenishment = true
                        }),

                    RateLimitType.TokenBucket => RateLimitPartition.GetTokenBucketLimiter(identifier, _ =>
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = options.PermitLimit,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(options.WindowSeconds),
                            TokensPerPeriod = options.PermitLimit,
                            QueueLimit = options.QueueLimit,
                            AutoReplenishment = true
                        }),

                    RateLimitType.Concurrency => RateLimitPartition.GetConcurrencyLimiter(identifier, _ =>
                        new ConcurrencyLimiterOptions
                        {
                            PermitLimit = options.PermitLimit,
                            QueueLimit = options.QueueLimit
                        }),

                    _ => throw new ArgumentException($"Unsupported rate limit type: {options.Type}")
                };
            });
        }

        /// <summary>
        /// Extract identifier từ HttpContext (User ID hoặc IP Address)
        /// PERFORMANCE: Reuse logic từ middleware để consistent
        /// </summary>
        private static string GetIdentifier(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Sử dụng user ID làm identifier nếu user đã authenticate
                var userId = context.User.FindFirst("sub")?.Value
                   ?? context.User.FindFirst("userId")?.Value
                   ?? context.User.Identity.Name;

                if (!string.IsNullOrEmpty(userId))
                {
                    return $"user:{userId}";
                }
            }

            // Sử dụng IP của client làm identifier
            var ipAddress = context.Connection.RemoteIpAddress?.ToString()
             ?? context.Request.Headers["X-Forwarded-For"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim()
                    ?? "unknown";

            return $"ip:{ipAddress}";
        }
    }
}