using System.ComponentModel.DataAnnotations;

namespace BaseNetCore.Core.src.Main.Security
{
    /// <summary>
    /// Configuration settings for JWT token generation and validation.
    /// </summary>
    public class TokenSettings
    {
        /// <summary>
        /// Secret key for signing JWT tokens. Must be at least 32 characters.
        /// </summary>
        [Required]
        [MinLength(32, ErrorMessage = "SecretKey must be at least 32 characters long")]
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time in seconds. Default is 86400 (24 hours).
        /// </summary>
        [Required]
        public string ExpireTimeS { get; set; } = "86400";

        /// <summary>
        /// Optional: Issuer claim (iss) for the token.
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        /// Optional: Audience claim (aud) for the token.
        /// </summary>
        public string? Audience { get; set; }
    }
}
