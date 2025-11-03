using System.ComponentModel.DataAnnotations;

namespace BaseNetCore.Core.src.Main.Extensions.Token
{
    /// <summary>
    /// Configuration settings for JWT token generation and validation.
    /// </summary>
    public class TokenSettings
    {
        /// <summary>
        /// RSA Private Key in PEM format for signing JWT tokens.
        /// </summary>
        [Required]
        public string RsaPrivateKey { get; set; } = string.Empty;

        /// <summary>
        /// RSA Public Key in PEM format for validating JWT tokens.
        /// </summary>
        [Required]
        public string RsaPublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time in seconds. Default is 86400 (24 hours).
        /// </summary>
        [Required]
        public string AccessExpireTimeS { get; set; } = "86400";

        [Required]
        public string RefreshExpireTimeS { get; set; } = "86400";

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
