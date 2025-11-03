using System.IO.Compression;

namespace BaseNetCore.Core.src.Main.Extensions.Performance
{
    /// <summary>
    /// Performance optimization configuration options
    /// All properties have default values, making configuration section optional
    /// </summary>
    public class PerformanceOptions
    {
        public const string SectionName = "PerformanceOptimization";

        // ===== Response Compression =====
        /// <summary>Enable Response Compression (Brotli/Gzip)</summary>
        public bool EnableResponseCompression { get; set; } = true;

        /// <summary>Enable compression for HTTPS requests</summary>
        public bool CompressionEnableForHttps { get; set; } = true;

        /// <summary>Brotli compression level: Optimal, Fastest, NoCompression, SmallestSize</summary>
        public string BrotliCompressionLevel { get; set; } = "Fastest";

        /// <summary>Gzip compression level: Optimal, Fastest, NoCompression, SmallestSize</summary>
        public string GzipCompressionLevel { get; set; } = "Fastest";

        // ===== Response Caching =====
        /// <summary>Enable Response Caching middleware</summary>
        public bool EnableResponseCaching { get; set; } = true;

        /// <summary>Maximum body size to cache (bytes). Default: 1MB</summary>
        public long ResponseCacheMaxBodySize { get; set; } = 1024 * 1024;

        /// <summary>Use case-sensitive paths for caching</summary>
        public bool ResponseCacheCaseSensitive { get; set; } = false;

        // ===== Output Cache (.NET 7+) =====
        /// <summary>Enable Output Cache middleware (.NET 7+)</summary>
        public bool EnableOutputCache { get; set; } = true;

        /// <summary>Default cache expiration in seconds</summary>
        public int OutputCacheExpirationSeconds { get; set; } = 10;

        // Helper method to parse compression level
        public CompressionLevel GetBrotliCompressionLevel()
        {
            return ParseCompressionLevel(BrotliCompressionLevel);
        }

        public CompressionLevel GetGzipCompressionLevel()
        {
            return ParseCompressionLevel(GzipCompressionLevel);
        }

        private static CompressionLevel ParseCompressionLevel(string level)
        {
            return level?.ToLowerInvariant() switch
            {
                "fastest" => CompressionLevel.Fastest,
                "optimal" => CompressionLevel.Optimal,
                "nocompression" => CompressionLevel.NoCompression,
                "smallestsize" => CompressionLevel.SmallestSize,
                _ => CompressionLevel.Fastest
            };
        }
    }
}