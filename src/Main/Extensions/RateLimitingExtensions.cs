using BaseNetCore.Core.src.Main.Security.RateLimited;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace BaseNetCore.Core.src.Main.Extensions
{
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// Đăng ký Rate Limiting với options mặc định hoặc từ Configuration (optional)
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

            // Tạo và đăng ký RateLimiter
            services.AddSingleton<RateLimiter>(sp => CreateRateLimiter(options));

            return services;
        }

        public static IApplicationBuilder UseBaseRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingMiddleware>();
        }

        private static RateLimiter CreateRateLimiter(RateLimitOptions options)
        {
            return options.Type switch
            {
                RateLimitType.Fixed => new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.WindowSeconds),
                    QueueLimit = options.QueueLimit
                }),

                RateLimitType.Sliding => new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.WindowSeconds),
                    SegmentsPerWindow = 10,
                    QueueLimit = options.QueueLimit
                }),

                RateLimitType.TokenBucket => new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
                {
                    TokenLimit = options.PermitLimit,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(options.WindowSeconds),
                    TokensPerPeriod = options.PermitLimit,
                    QueueLimit = options.QueueLimit
                }),

                RateLimitType.Concurrency => new ConcurrencyLimiter(new ConcurrencyLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    QueueLimit = options.QueueLimit
                }),

                _ => throw new ArgumentException($"Unsupported rate limit type: {options.Type}")
            };
        }
    }
}