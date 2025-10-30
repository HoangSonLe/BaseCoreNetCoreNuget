# JWT v?i RSA Algorithm - H??ng d?n s? d?ng

D? �n ?� ???c c?p nh?t ?? s? d?ng thu?t to�n **RSA256** thay v� HMAC-SHA256 cho JWT tokens.

## Thay ??i ch�nh

### 1. TokenSettings
- **Lo?i b?**: `SecretKey` (symmetric key)
- **Th�m m?i**: 
  - `RsaPrivateKey`: Private key d�ng ?? k� token (PEM format)
  - `RsaPublicKey`: Public key d�ng ?? x�c th?c token (PEM format)

### 2. TokenService
- S? d?ng `RSA.Create()` v� `RsaSecurityKey` thay v� `SymmetricSecurityKey`
- Thu?t to�n: `SecurityAlgorithms.RsaSha256` thay v� `SecurityAlgorithms.HmacSha256`

### 3. TokenServiceExtensions
- C?p nh?t `AddJwtAuthentication` ?? s? d?ng RSA public key

## T?o RSA Key Pair

### C�ch 1: S? d?ng RsaKeyGenerator (Khuy?n ngh?)

```csharp
using BaseNetCore.Core.src.Main.Security;

// In ra c?u h�nh m?u v?i keys m?i
RsaKeyGenerator.PrintSampleConfiguration(2048);

// Ho?c ch? t?o key pair
var (privateKey, publicKey) = RsaKeyGenerator.GenerateKeyPair(2048);
Console.WriteLine("Private Key:");
Console.WriteLine(privateKey);
Console.WriteLine("\nPublic Key:");
Console.WriteLine(publicKey);
```

### C�ch 2: S? d?ng OpenSSL

```bash
# T?o private key (2048 bit)
openssl genrsa -out private_key.pem 2048

# T?o public key t? private key
openssl rsa -in private_key.pem -pubout -out public_key.pem

# Xem n?i dung keys
cat private_key.pem
cat public_key.pem
```

### C�ch 3: S? d?ng PowerShell (Windows)

```powershell
# T?o RSA key pair
$rsa = [System.Security.Cryptography.RSA]::Create(2048)

# Export private key
$privateKey = $rsa.ExportRSAPrivateKeyPem()
$privateKey | Out-File -FilePath "private_key.pem" -Encoding UTF8

# Export public key
$publicKey = $rsa.ExportRSAPublicKeyPem()
$publicKey | Out-File -FilePath "public_key.pem" -Encoding UTF8

Write-Host "Private Key:"
Write-Host $privateKey
Write-Host "`nPublic Key:"
Write-Host $publicKey
```

## C?u h�nh appsettings.json

```json
{
  "TokenSettings": {
    "RsaPrivateKey": "-----BEGIN RSA PRIVATE KEY-----\nMIIEowIBAAKCAQEA...\n-----END RSA PRIVATE KEY-----",
    "RsaPublicKey": "-----BEGIN RSA PUBLIC KEY-----\nMIIBCgKCAQEA...\n-----END RSA PUBLIC KEY-----",
    "ExpireTimeS": "3600",
    "Issuer": "your-app-name",
    "Audience": "your-app-audience"
  }
}
```

**L?u � quan tr?ng:**
- Thay th? `\n` cho m?i d�ng m?i trong PEM format
- Private key ph?i ???c b?o m?t tuy?t ??i
- Ch? public key c� th? ???c chia s? c�ng khai

## S? d?ng trong Startup/Program.cs

```csharp
using BaseNetCore.Core.src.Main.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Th�m JWT authentication v?i RSA
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

## V� d? s? d?ng TokenService

```csharp
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
     _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // X�c th?c user...
        var userId = "123";
        var username = "john.doe";
    var roles = new[] { "Admin", "User" };

    // T?o JWT token v?i RSA
        var token = _tokenService.GenerateToken(userId, username, roles);
   
        // T?o refresh token
    var refreshToken = _tokenService.GenerateRefreshToken();

        return Ok(new
 {
    AccessToken = token,
            RefreshToken = refreshToken,
   ExpiresIn = 3600
      });
    }

    [HttpPost("validate")]
    public IActionResult ValidateToken([FromBody] string token)
    {
   var principal = _tokenService.ValidateToken(token);
  
      if (principal == null)
        {
            return Unauthorized("Invalid token");
        }

    var userId = _tokenService.GetUserIdFromToken(token);
        return Ok(new { UserId = userId, Valid = true });
    }
}
```

## ?u ?i?m c?a RSA so v?i HMAC

1. **B?o m?t cao h?n**: 
   - Private key ch? ???c s? d?ng ?? k� token
   - Public key c� th? ???c chia s? ?? x�c th?c token
   - Ph� h?p cho ki?n tr�c microservices

2. **Ph�n t�n t?t h?n**:
   - C�c service kh�c ch? c?n public key ?? x�c th?c
   - Kh�ng c?n chia s? secret key gi?a c�c service

3. **Tu�n th? ti�u chu?n**:
   - RSA l� ti�u chu?n c�ng nghi?p
   - H? tr? t?t cho OAuth 2.0 v� OpenID Connect

## L?u � b?o m?t

1. **Private Key**:
   - Kh�ng commit v�o source control
   - S? d?ng Azure Key Vault, AWS Secrets Manager, ho?c t??ng t?
   - Rotate ??nh k?

2. **Key Size**:
   - T?i thi?u: 2048 bit
   - Khuy?n ngh?: 2048-4096 bit
   - C�ng l?n c�ng b?o m?t nh?ng ch?m h?n

3. **Environment Variables** (Production):
```bash
export TokenSettings__RsaPrivateKey="$(cat private_key.pem)"
export TokenSettings__RsaPublicKey="$(cat public_key.pem)"
```

## Troubleshooting

### L?i: "PEM format is not valid"
- Ki?m tra format c?a key ph?i b?t ??u v?i `-----BEGIN RSA ...-----`
- ??m b?o `\n` ???c s? d?ng cho newlines trong JSON

### L?i: "Token validation failed"
- Ki?m tra public key ?�ng v?i private key ?� d�ng ?? k�
- Ki?m tra Issuer v� Audience n?u ?� c?u h�nh
- Ki?m tra token ch?a h?t h?n

### Performance Issue
- RSA ch?m h?n HMAC, nh?ng ch? ?�ng k? khi x? l� h�ng ngh�n token/gi�y
- N?u c?n performance cao, c�n nh?c cache token validation results
