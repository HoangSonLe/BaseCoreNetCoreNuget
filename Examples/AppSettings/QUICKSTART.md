# ?? Quick Start - AppSettings Examples

H??ng d?n nhanh ?? b?t ??u v?i các file c?u hình m?u c?a BaseNetCore.Core

---

## ?? B?n c?n gì?

### Ch?n theo nhu c?u:

#### ?? Tôi mu?n setup nhanh ?? b?t ??u
? S? d?ng **`appsettings.Basic.json`**

#### ?? Tôi chu?n b? deploy production
? S? d?ng **`appsettings.Medium.json`**

#### ?? Tôi ch? c?n c?u hình m?t feature c? th?
? Ch?n file t??ng ?ng:
- Logging ? `appsettings.SeriLog.json`
- Encryption ? `appsettings.Aes.json`
- Authentication ? `appsettings.Authentication.json`
- Authorization ? `appsettings.Authorization.json`
- Rate Limiting ? `appsettings.RateLimit.json`
- Performance ? `appsettings.Performance.json`
- Database ? `appsettings.Database.json`

---

## ? Setup trong 3 b??c

### B??c 1: Copy file vào project
```bash
cp Examples/AppSettings/appsettings.Basic.json YourProject/appsettings.json
```

### B??c 2: Generate RSA keys cho JWT
```csharp
using BaseNetCore.Core.src.Main.Security.Algorithm;

var keyPair = RsaKeyGenerator.GenerateKeyPair(2048);
Console.WriteLine("Private Key:\n" + keyPair.PrivateKey);
Console.WriteLine("\nPublic Key:\n" + keyPair.PublicKey);
```

Paste keys vào `appsettings.json`:
```json
{
  "TokenSettings": {
    "RsaPrivateKey": "-----BEGIN RSA PRIVATE KEY-----\n...",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n..."
  }
}
```

### B??c 3: Setup Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.AddBaseSerilog();
builder.Services.AddBaseNetCoreJwtAuthentication(builder.Configuration);
builder.Services.AddPerformanceOptimization(builder.Configuration);
builder.Services.AddBaseRateLimiting(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

// Add middleware
app.UseBaseNetCoreMiddlewareWithAuth();
app.MapControllers();

app.Run();
```

**? Done!** Your app is ready to run.

---

## ?? B?o m?t Secrets

### ?? QUAN TR?NG: Không commit secrets vào Git!

Add vào `.gitignore`:
```gitignore
appsettings.*.json
!appsettings.Development.json
```

### Development - S? d?ng User Secrets
```bash
# Setup user secrets
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;..."
dotnet user-secrets set "Aes:SecretKey" "YourSecretKey..."
```

### Production - S? d?ng Environment Variables
```bash
# Azure App Service
az webapp config appsettings set --name MyApp --resource-group MyRG \
  --settings ConnectionStrings__DefaultConnection="Host=..."

# Docker
docker run -e ConnectionStrings__DefaultConnection="Host=..." myapp
```

---

## ?? Tài li?u ??y ??

- **[README.md](README.md)** - H??ng d?n chi ti?t t?ng file
- **[COMPARISON.md](COMPARISON.md)** - So sánh các m?c ?? c?u hình
- **[Main Documentation](../../README.md)** - Tài li?u chính

---

## ?? Troubleshooting

### ? Error: "RSA key not found"
? Ch?a c?u hình `TokenSettings:RsaPrivateKey` và `RsaPublicKey`  
? Follow B??c 2 ?? generate keys

### ? Error: "AES secret key not found"
? Ch?a c?u hình `Aes:SecretKey`  
? Set key (32 characters):
```json
{ "Aes": { "SecretKey": "MySecretKey123456789012345678901" } }
```

### ? Error: "Connection string not found"
? Ch?a c?u hình `ConnectionStrings:DefaultConnection`  
? Xem `appsettings.Database.json` ?? tham kh?o

### ? Error: "Serilog not logging"
? Ch?a call `builder.AddBaseSerilog()`  
? Add vào `Program.cs`

---

## ?? Examples

### Minimal API with all features
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Services
builder.AddBaseSerilog();
builder.Services.AddBaseNetCoreJwtAuthentication(builder.Configuration);
builder.Services.AddPerformanceOptimization(builder.Configuration);
builder.Services.AddBaseRateLimiting(builder.Configuration);

// AES Encryption
builder.Services.Configure<AesSettings>(builder.Configuration.GetSection("Aes"));
builder.Services.AddSingleton<AesAlgorithm>();

var app = builder.Build();

// Middleware
app.UseBaseNetCoreMiddlewareWithAuth();

// Endpoints
app.MapGet("/", () => "Hello World!");

app.MapGet("/encrypt/{text}", (string text, AesAlgorithm aes) => 
{
    var encrypted = aes.Encrypt(text);
    return Results.Ok(new { encrypted });
});

app.MapGet("/decrypt/{text}", (string text, AesAlgorithm aes) => 
{
    var decrypted = aes.Decrypt(text);
    return Results.Ok(new { decrypted });
});

app.Run();
```

### Controller API with authorization
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly AesAlgorithm _aes;

    public UsersController(ILogger<UsersController> logger, AesAlgorithm aes)
    {
        _logger = logger;
        _aes = aes;
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        _logger.LogInformation("Getting all users");
        return Ok(new[] { "User1", "User2" });
    }

    [HttpPost("encrypt")]
    public IActionResult EncryptData([FromBody] string data)
    {
        var encrypted = _aes.Encrypt(data);
        return Ok(new { encrypted });
    }
}
```

---

## ?? Pro Tips

### 1. Merge multiple config files
```csharp
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.SeriLog.json", optional: true)
    .AddJsonFile("appsettings.Performance.json", optional: true);
```

### 2. Validate config on startup
```csharp
builder.Services.AddOptions<TokenSettings>()
    .Bind(builder.Configuration.GetSection("TokenSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### 3. Override config with code
```csharp
builder.Services.Configure<RateLimitOptions>(options =>
{
    options.PermitLimit = 500; // Override from code
});
```

### 4. Access config in code
```csharp
public class MyService
{
    private readonly IOptions<TokenSettings> _tokenSettings;

    public MyService(IOptions<TokenSettings> tokenSettings)
    {
        _tokenSettings = tokenSettings;
        var expireTime = _tokenSettings.Value.AccessExpireTimeS;
    }
}
```

---

## ?? Need Help?

- ?? [Full Documentation](../../README.md)
- ?? [GitHub Issues](https://github.com/HoangSonLe/BaseCoreNetCoreNuget/issues)
- ?? Contact: hoangson.it@gmail.com

---

**Happy Coding! ??**
