# ?? AES Encryption

## Gi?i thi?u

**AesAlgorithm** s? d?ng **AES-GCM** (Galois/Counter Mode) cho encryption at rest.

---

## Features

- ? **AES-256-GCM** - Industry standard encryption
- ? **Authenticated Encryption** - Prevents tampering
- ? **Auto Key Derivation** - SHA-256 t? secret key
- ? **Configuration Support** - Load t? appsettings.json

---

## Configuration

### appsettings.json

```json
{
  "Aes": {
    "SecretKey": "YourSecretKey-MustBe32CharsMin!"
  }
}
```

### Generate Strong Key

```csharp
using System.Security.Cryptography;

var keyBytes = new byte[32]; // 256-bit
RandomNumberGenerator.Fill(keyBytes);
var secretKey = Convert.ToBase64String(keyBytes);
Console.WriteLine(secretKey);  // Use in appsettings.json
```

---

## Usage

### Setup

```csharp
// Program.cs
builder.Services.AddAesAlgorithmConfiguration(builder.Configuration);
```

### Encrypt/Decrypt

```csharp
public class UserService
{
    private readonly AesAlgorithm _aes;

    public UserService(AesAlgorithm aes)
    {
        _aes = aes;
    }

    public async Task<User> CreateUser(RegisterDto dto)
    {
    var user = new User
        {
     Username = dto.Username,
          Email = dto.Email,
 // Encrypt sensitive data
   SocialSecurityNumber = _aes.Encrypt(dto.SSN),
  CreditCardNumber = _aes.Encrypt(dto.CreditCard)
        };

 await _userRepo.AddAsync(user);
        return user;
    }

    public async Task<UserDetailsDto> GetUserDetails(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);

  return new UserDetailsDto
        {
       Username = user.Username,
   Email = user.Email,
    // Decrypt sensitive data
        SSN = _aes.Decrypt(user.SocialSecurityNumber),
        CreditCard = _aes.Decrypt(user.CreditCardNumber)
  };
    }
}
```

---

## Encrypt vs Hash

| Use Case | Use Encryption | Use Hashing |
|----------|----------------|-------------|
| **Passwords** | ? | ? BCrypt/Argon2 |
| **Credit Cards** | ? AES | ? |
| **SSN** | ? AES | ? |
| **API Keys** | ? AES | ? |
| **Tokens** | Depends | Depends |

---

## Best Practices

### 1. Never Log Encrypted Data

```csharp
// ? BAD
_logger.LogInformation($"Encrypted SSN: {encryptedSSN}");

// ? GOOD
_logger.LogInformation("SSN encrypted successfully");
```

### 2. Rotate Keys Periodically

```csharp
public async Task RotateEncryptionKey(string oldKey, string newKey)
{
    var oldAes = new AesAlgorithm(oldKey);
    var newAes = new AesAlgorithm(newKey);

    var users = await _userRepo.GetAllAsync();
    
    foreach (var user in users)
    {
        // Decrypt with old key
     var ssn = oldAes.Decrypt(user.SocialSecurityNumber);
        
        // Re-encrypt with new key
        user.SocialSecurityNumber = newAes.Encrypt(ssn);
    }

    await _unitOfWork.SaveChangesAsync();
}
```

### 3. Use Azure Key Vault in Production

```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVaultUrl"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
new DefaultAzureCredential());
}
```

---

**[? Back to Documentation](../README.md)**
