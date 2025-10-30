using System.Security.Cryptography;

namespace BaseNetCore.Core.src.Main.Security.Algorithm
{
    /// <summary>
    /// Helper class for generating RSA key pairs for JWT token signing.
    /// </summary>
    public static class RsaKeyGenerator
    {
        /// <summary>
        /// Generates a new RSA key pair with the specified key size.
        /// </summary>
        /// <param name="keySizeInBits">Key size in bits (default: 2048, recommended: 2048 or 4096)</param>
        /// <returns>Tuple containing (PrivateKey, PublicKey) in PEM format</returns>
        public static (string PrivateKey, string PublicKey) GenerateKeyPair(int keySizeInBits = 2048)
        {
            using var rsa = RSA.Create(keySizeInBits);

            var privateKey = rsa.ExportRSAPrivateKeyPem();
            var publicKey = rsa.ExportRSAPublicKeyPem();

            return (privateKey, publicKey);
        }

        /// <summary>
        /// Prints a sample configuration for appsettings.json with generated RSA keys.
        /// </summary>
        public static void PrintSampleConfiguration(int keySizeInBits = 2048)
        {
            var (privateKey, publicKey) = GenerateKeyPair(keySizeInBits);

            Console.WriteLine("=== Generated RSA Keys for JWT Token ===");
            Console.WriteLine("\nAdd this to your appsettings.json:");
            Console.WriteLine("\n\"TokenSettings\": {");
            Console.WriteLine($"  \"RsaPrivateKey\": \"{privateKey.Replace("\n", "\\n")}\",");
            Console.WriteLine($"  \"RsaPublicKey\": \"{publicKey.Replace("\n", "\\n")}\",");
            Console.WriteLine("  \"ExpireTimeS\": \"86400\",");
            Console.WriteLine("  \"Issuer\": \"your-issuer\",");
            Console.WriteLine("  \"Audience\": \"your-audience\"");
            Console.WriteLine("}");
            Console.WriteLine("\n=== Private Key (Keep this SECRET!) ===");
            Console.WriteLine(privateKey);
            Console.WriteLine("\n=== Public Key (Can be shared) ===");
            Console.WriteLine(publicKey);
        }
    }
}
