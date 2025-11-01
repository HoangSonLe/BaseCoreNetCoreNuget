using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace BaseNetCore.Core.src.Main.Security.Algorithm
{
    /// <summary>
    /// Configuration holder for AES secret key (can be bound from appsettings).
    /// </summary>
    public sealed class AesSettings // Changed from internal to public
    {
        public string SecretKey { get; init; } = string.Empty;
    }

    /// <summary>
    /// AES-GCM helper that reads secret key from config or accepts it directly.
    /// - Uses 12-byte IV (recommended for GCM)
    /// - Uses 128-bit authentication tag
    /// - Produces/consumes Base64 encoded: IV || CipherText || Tag
    /// </summary>
    public sealed class AesAlgorithm
    {
        private const int IvSize = 12;   // 96 bits (recommended for GCM)
        private const int TagSize = 16;  // 128 bits
        private readonly byte[] _key;

        public AesAlgorithm(string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentNullException(nameof(secretKey));

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            // Accept only valid AES key lengths; otherwise derive a 256-bit key via SHA-256
            if (keyBytes.Length is 16 or 24 or 32)
            {
                _key = keyBytes;
            }
            else
            {
                using var sha256 = SHA256.Create();
                _key = sha256.ComputeHash(keyBytes); // 32 bytes
            }
        }

        /// <summary>
        /// Construct from IOptions&lt;AesSettings&gt; (bind AesSettings from appsettings).
        /// In appsettings use: "Aes": { "SecretKey": "..." } or other supported keys (see IConfiguration ctor).
        /// </summary>
        public AesAlgorithm(IOptions<AesSettings> options)
            : this(options?.Value?.SecretKey ?? throw new ArgumentNullException(nameof(options)))
        {
        }

        /// <summary>
        /// Construct from IConfiguration. Tries multiple common keys to be tolerant to different naming:
        /// "Aes:SecretKey", "aes:secret-key", "aes:secretKey", "aes.secret-key".
        /// </summary>
        public AesAlgorithm(IConfiguration config)
            : this(ReadSecretFromConfig(config))
        {
        }

        private static string ReadSecretFromConfig(IConfiguration config)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));

            // Try a few common key names (colon is the section separator in IConfiguration)
            var candidates = new[]
            {
                "Aes:SecretKey",
                "aes:secret-key",
                "aes:secretKey",
                "aes.secret-key"
            };

            foreach (var key in candidates)
            {
                var value = config[key];
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            // Also try binding to AesSettings section
            var section = config.GetSection("Aes");
            var bound = section.Get<AesSettings>();
            if (!string.IsNullOrWhiteSpace(bound?.SecretKey))
                return bound.SecretKey;

            throw new InvalidOperationException("AES secret key not found in configuration. Set one of: 'Aes:SecretKey', 'aes:secret-key', or bind an 'Aes' section with SecretKey.");
        }

        /// <summary>
        /// Encrypts plain text and returns Base64(IV || CipherText || Tag)
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (plainText is null) throw new ArgumentNullException(nameof(plainText));

            try
            {
                var iv = new byte[IvSize];
                RandomNumberGenerator.Fill(iv);

                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var cipherBytes = new byte[plainBytes.Length];
                var tag = new byte[TagSize];

                using var aesGcm = new AesGcm(_key);
                aesGcm.Encrypt(iv, plainBytes, cipherBytes, tag);

                var combined = new byte[IvSize + cipherBytes.Length + TagSize];
                Buffer.BlockCopy(iv, 0, combined, 0, IvSize);
                Buffer.BlockCopy(cipherBytes, 0, combined, IvSize, cipherBytes.Length);
                Buffer.BlockCopy(tag, 0, combined, IvSize + cipherBytes.Length, TagSize);

                return Convert.ToBase64String(combined);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Encryption failed.", ex);
            }
        }

        /// <summary>
        /// Decrypts Base64(IV || CipherText || Tag) and returns the plain text.
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (cipherText is null) throw new ArgumentNullException(nameof(cipherText));

            try
            {
                var combined = Convert.FromBase64String(cipherText);

                if (combined.Length < IvSize + TagSize)
                    throw new ArgumentException("Invalid cipher text format.", nameof(cipherText));

                var iv = new byte[IvSize];
                Buffer.BlockCopy(combined, 0, iv, 0, IvSize);

                var tag = new byte[TagSize];
                Buffer.BlockCopy(combined, combined.Length - TagSize, tag, 0, TagSize);

                var cipherBytesLength = combined.Length - IvSize - TagSize;
                var cipherBytes = new byte[cipherBytesLength];
                Buffer.BlockCopy(combined, IvSize, cipherBytes, 0, cipherBytesLength);

                var plainBytes = new byte[cipherBytesLength];

                using var aesGcm = new AesGcm(_key);
                aesGcm.Decrypt(iv, cipherBytes, tag, plainBytes);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Decryption failed.", ex);
            }
        }
    }
}
