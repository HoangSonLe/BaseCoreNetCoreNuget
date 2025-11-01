# ??? RSA Key Generation

## Quick Start

```csharp
using BaseNetCore.Core.src.Main.Security.Algorithm;

// Console app
class Program
{
    static void Main()
    {
 RsaKeyGenerator.PrintSampleConfiguration(keySizeInBits: 2048);
    }
}
```

Output:
```
=== Generated RSA Keys for JWT Token ===

"TokenSettings": {
  "RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
  "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
  "AccessExpireTimeS": "3600",
  "RefreshExpireTimeS": "86400",
  "Issuer": "your-issuer",
  "Audience": "your-audience"
}
```

---

## Key Sizes

| Key Size | Security | Performance | Recommendation |
|----------|----------|-------------|----------------|
| **1024-bit** | ?? Weak | ? Fast | ? Not recommended |
| **2048-bit** | ? Good | ? Good | ? Default |
| **4096-bit** | ? Very Strong | ?? Slower | ? High security |

---

## Generate Programmatically

```csharp
using System.Security.Cryptography;

public static class KeyGenerator
{
    public static (string PrivateKey, string PublicKey) Generate(int keySize = 2048)
    {
        using var rsa = RSA.Create(keySize);
   
  var privateKey = rsa.ExportRSAPrivateKeyPem();
   var publicKey = rsa.ExportRSAPublicKeyPem();

     return (privateKey, publicKey);
    }
}

// Usage
var (privateKey, publicKey) = KeyGenerator.Generate(2048);
Console.WriteLine("Private Key:");
Console.WriteLine(privateKey);
Console.WriteLine("\nPublic Key:");
Console.WriteLine(publicKey);
```

---

## Store Keys Securely

### ? DON'T

```csharp
// ? Hard-coded in code
const string PRIVATE_KEY = "-----BEGIN PRIVATE KEY-----...";

// ? Committed to Git
// appsettings.json with keys checked in
```

### ? DO

```csharp
// ? User Secrets (Development)
dotnet user-secrets set "TokenSettings:RsaPrivateKey" "-----BEGIN..."

// ? Environment Variables (Production)
export TokenSettings__RsaPrivateKey="-----BEGIN..."

// ? Azure Key Vault (Production)
builder.Configuration.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());
```

---

## Key Rotation

```csharp
public class KeyRotationService
{
    public async Task RotateKeysAsync()
    {
  // 1. Generate new keys
        var (newPrivate, newPublic) = KeyGenerator.Generate(2048);

    // 2. Update configuration
        await UpdateKeyVaultAsync("RsaPrivateKey-New", newPrivate);
   await UpdateKeyVaultAsync("RsaPublicKey-New", newPublic);

        // 3. Deploy new version with both old + new keys
  // 4. After deployment, remove old keys
   }
}
```

---

**[? Back to Documentation](../README.md)**
