namespace BaseNetCore.Core.src.Main.Extensions.Performance
{
    /// <summary>
    /// Kestrel server limits configuration
    /// All properties have default values
    /// </summary>
    public class KestrelLimitsOptions
    {
        public const string SectionName = "KestrelLimits";

        /// <summary>Maximum number of concurrent connections</summary>
        public long? MaxConcurrentConnections { get; set; } = null; // null = unlimited

        /// <summary>Maximum number of concurrent upgraded connections (WebSockets)</summary>
        public long? MaxConcurrentUpgradedConnections { get; set; } = null;

        /// <summary>Maximum request body size in bytes. Default: 50MB</summary>
        public long? MaxRequestBodySize { get; set; } = 52428800;

        /// <summary>Keep-alive timeout in minutes. Default: 2 minutes</summary>
        public int KeepAliveTimeoutMinutes { get; set; } = 2;

        /// <summary>Request headers timeout in seconds. Default: 30 seconds</summary>
        public int RequestHeadersTimeoutSeconds { get; set; } = 30;
    }
}