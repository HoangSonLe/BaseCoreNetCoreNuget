namespace BaseNetCore.Core.src.Main.Security.RateLimited
{
    public class RateLimitOptions
    {
        public const string SectionName = "RateLimiting";

        /// <summary>
        /// Số lượng request tối đa trong time window
        /// </summary>
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Thời gian window (giây)
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Thời gian retry sau khi bị rate limit (giây)
        /// </summary>
        public int RetryAfterSeconds { get; set; } = 60;

        /// <summary>
        /// Tên policy để tracking
        /// </summary>
        public string? PolicyName { get; set; }

        /// <summary>
        /// Message khi rate limit exceeded
        /// </summary>
        public string? RateLimitExceededMessage { get; set; }

        /// <summary>
        /// Có thêm rate limit headers vào response không
        /// </summary>
        public bool IncludeRateLimitHeaders { get; set; } = true;

        /// <summary>
        /// Số lượng request được queue khi đạt limit
        /// </summary>
        public int QueueLimit { get; set; } = 0;

        /// <summary>
        /// Loại rate limiter (Fixed, Sliding, TokenBucket, Concurrency)
        /// </summary>
        public RateLimitType Type { get; set; } = RateLimitType.Fixed;

        /// <summary>
        /// Danh sách paths được bỏ qua rate limiting
        /// </summary>
        public List<string> WhitelistedPaths { get; set; } = new List<string>
        {
            "/health",
            "/metrics",
            "/api/status"
        };

        /// <summary>
        /// Có enable rate limiting không
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    public enum RateLimitType
    {
        Fixed,
        Sliding,
        TokenBucket,
        Concurrency
    }
}
