# JWT với RSA Algorithm - Hướng dẫn sẽ d?ng

D? án ?ã ???c c?p nh?t ?? sẽ d?ng thu?t toán **RSA256** thay vì HMAC-SHA256 cho JWT tokens.

## Thay ??i chính

### 1. TokenSettings
- **Lo?i b?**: `SecretKey` (symmetric key)
- **Thêm m?i**: 
  - `RsaPrivateKey`: Private key dùng ?? ký token (PEM format)
  - `RsaPublicKey`: Public key dùng ?? xác th?c token (PEM format)

### 2. TokenService
- S? d?ng `RSA.Create()` và `RsaSecurityKey` thay vì `SymmetricSecurityKey`
- Thu?t toán: `SecurityAlgorithms.RsaSha256` thay vì `SecurityAlgorithms.HmacSha256`

### 3. TokenServiceExtensions
- C?p nh?t `AddJwtAuthentication` ?? sẽ d?ng RSA public key

## Tạo RSA Key Pair

### Cách 1: S? d?ng RsaKeyGenerator (Khuy?n ngh?)

```csharp
using BaseNetCore.Core.src.Main.Security;

// In ra cấu hình m?u với keys m?i
RsaKeyGenerator.PrintSampleConfiguration(2048);

// Ho?c ch? tạo key pair
var (privateKey, publicKey) = RsaKeyGenerator.GenerateKeyPair(2048);
Console.WriteLine("Private Key:");
Console.WriteLine(privateKey);
Console.WriteLine("\nPublic Key:");
Console.WriteLine(publicKey);
```

### Cách 2: S? d?ng OpenSSL

```bash
# Tạo private key (2048 bit)
openssl genrsa -out private_key.pem 2048

# Tạo public key t? private key
openssl rsa -in private_key.pem -pubout -out public_key.pem

# Xem n?i dung keys
cat private_key.pem
cat public_key.pem
```

### Cách 3: S? d?ng PowerShell (Windows)

```powershell
# Tạo RSA key pair
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

## Cấu hình appsettings.json

```json
{
  "TokenSettings": {
    "RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\nMIIEowIBAAKCAQEA...\n-----END PRIVATE KEY-----",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\nMIIBCgKCAQEA...\n-----END PUBLIC KEY-----",
    "ExpireTimeS": "3600",
    "Issuer": "your-app-name",
    "Audience": "your-app-audience"
  }
}
```

**Lưu ý quan tr?ng:**
- Thay th? `\n` cho m?i dòng m?i trong PEM format
- Private key ph?i ???c b?o một tuy?t ??i
- Ch? public key có th? ???c chia sẽ công khai

## S? d?ng trong Startup/Program.cs

```csharp
using BaseNetCore.Core.src.Main.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Thêm JWT authentication với RSA
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

## Ví d? sẽ d?ng TokenService

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
        // Xác th?c user...
        var userId = "123";
        var username = "john.doe";
    var roles = new[] { "Admin", "User" };

    // Tạo JWT token với RSA
        var token = _tokenService.GenerateToken(userId, username, roles);
   
        // Tạo refresh token
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

## ?u ?i?m c?a RSA so với HMAC

1. **B?o một cao h?n**: 
   - Private key ch? ???c sẽ d?ng ?? ký token
   - Public key có th? ???c chia sẽ ?? xác th?c token
   - Phù h?p cho ki?n trúc microservices

2. **Phân tán t?t h?n**:
   - Các service khác ch? c?n public key ?? xác th?c
   - Không c?n chia sẽ secret key gi?a các service

3. **Tuân th? tiêu chu?n**:
   - RSA là tiêu chu?n công nghi?p
   - H? tr? t?t cho OAuth 2.0 và OpenID Connect

## Lưu ý b?o một

1. **Private Key**:
   - Không commit vào source control
   - S? d?ng Azure Key Vault, AWS Secrets Manager, ho?c t??ng t?
   - Rotate ??nh k?

2. **Key Size**:
   - T?i thiệu: 2048 bit
   - Khuy?n ngh?: 2048-4096 bit
   - Càng l?n càng b?o một nh?ng ch?m h?n

3. **Environment Variables** (Production):
```bash
export TokenSettings__RsaPrivateKey="$(cat private_key.pem)"
export TokenSettings__RsaPublicKey="$(cat public_key.pem)"
```

## Troubleshooting

### L?i: "PEM format is not valid"
- Ki?m tra format c?a key ph?i b?t ??u với `-----BEGIN ...-----`
- ??m b?o `\n` ???c sẽ d?ng cho newlines trong JSON

### L?i: "Token validation failed"
- Ki?m tra public key ?úng với private key ?ã dùng ?? ký
- Ki?m tra Issuer và Audience n?u ?ã cấu hình
- Ki?m tra token ch?a h?t h?n

### Performance Issue
- RSA ch?m h?n HMAC, nh?ng ch? ?áng k? khi x? lý hàng nghìn token/giây
- N?u c?n performance cao, cân nh?c cache token validation results
