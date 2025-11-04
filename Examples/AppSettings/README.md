# ?? AppSettings Configuration Examples

Th? m?c này ch?a các file c?u hình m?u cho **BaseNetCore.Core** v?i các m?c ?? ph?c t?p khác nhau.

---

## ?? Danh sách Files

| File | Mô t? | M?c ?? |
|------|-------|--------|
| `appsettings.Basic.json` | C?u hình c? b?n v?i t?t c? tính n?ng | ? Basic |
| `appsettings.Medium.json` | C?u hình t?i ?u cho production | ?? Medium |
| `appsettings.SeriLog.json` | Ch? c?u hình Serilog logging | ? Basic |
| `appsettings.Aes.json` | Ch? c?u hình AES encryption | ? Basic |
| `appsettings.Performance.json` | Ch? c?u hình Performance optimization | ?? Medium |
| `appsettings.Authentication.json` | Ch? c?u hình JWT Authentication | ? Basic |
| `appsettings.Authorization.json` | Ch? c?u hình Authorization & Permission | ? Basic |
| `appsettings.RateLimit.json` | Ch? c?u hình Rate Limiting | ? Basic |
| `appsettings.Database.json` | Connection strings cho các lo?i database | ? Basic |

---

## ?? Cách s? d?ng

### 1. Copy file m?u vào project

```bash
# Copy file Basic vào project
cp Examples/AppSettings/appsettings.Basic.json YourProject/appsettings.json

# Ho?c copy file Medium cho production
cp Examples/AppSettings/appsettings.Medium.json YourProject/appsettings.Production.json
```

### 2. Ch?nh s?a các giá tr? c?n thi?t

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING_HERE"
  },
  "TokenSettings": {
    "RsaPrivateKey": "YOUR_PRIVATE_KEY_HERE",
    "RsaPublicKey": "YOUR_PUBLIC_KEY_HERE"
  },
  "Aes": {
    "SecretKey": "YOUR_SECRET_KEY_HERE"
  }
}
```

### 3. ??ng ký services trong Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.AddBaseSerilog();

// JWT Authentication
builder.Services.AddBaseNetCoreJwtAuthentication(builder.Configuration);

// Performance Optimization
builder.Services.AddPerformanceOptimization(builder.Configuration);

// Rate Limiting
builder.Services.AddBaseRateLimiting(builder.Configuration);

// AES Encryption
builder.Services.Configure<AesSettings>(builder.Configuration.GetSection("Aes"));
builder.Services.AddSingleton<AesAlgorithm>();

var app = builder.Build();

// Middleware pipeline
app.UseBaseNetCoreMiddlewareWithAuth();
app.MapControllers();

app.Run();
```

---

## ?? B?o m?t

### ?? KHÔNG commit các thông tin nh?y c?m vào Git:
- Database passwords
- JWT RSA keys
- AES secret keys
- API keys

### ? Best Practices:

#### Development - S? d?ng User Secrets
```bash
# Init user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;..."
dotnet user-secrets set "Aes:SecretKey" "MySecretKey..."
dotnet user-secrets set "TokenSettings:RsaPrivateKey" "-----BEGIN RSA..."
```

#### Production - S? d?ng Environment Variables
```bash
# Linux/Mac
export ConnectionStrings__DefaultConnection="Host=..."
export Aes__SecretKey="..."

# Windows
set ConnectionStrings__DefaultConnection=Host=...
set Aes__SecretKey=...
```

#### Production - S? d?ng Azure Key Vault
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## ?? Chi ti?t t?ng lo?i c?u hình

### 1. Serilog Logging

**File:** `appsettings.SeriLog.json`

**Tính n?ng:**
- Console logging v?i color theme
- File logging v?i rolling daily
- Multiple log levels
- Enrichers (MachineName, ThreadId, etc.)

**Sinks ph? bi?n:**
- Console
- File (rolling)
- Seq (centralized logging)
- Elasticsearch
- Application Insights

### 2. AES Encryption

**File:** `appsettings.Aes.json`

**Tính n?ng:**
- AES-GCM encryption
- Support key lengths: 16, 24, 32 bytes
- Auto SHA-256 hashing for other lengths

**Generate secret key:**
```csharp
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
```

### 3. Performance Optimization

**File:** `appsettings.Performance.json`

**Tính n?ng:**
- Response Compression (Brotli/Gzip)
- Response Caching
- Output Cache (.NET 7+)
- Kestrel Limits

**Khi nào c?n:**
- High traffic applications
- Large response bodies
- Bandwidth optimization

### 4. JWT Authentication

**File:** `appsettings.Authentication.json`

**Tính n?ng:**
- RSA signing (secure)
- Configurable expiration times
- Issuer/Audience claims

**Generate RSA keys:**
```csharp
using BaseNetCore.Core.src.Main.Security.Algorithm;

var keyPair = RsaKeyGenerator.GenerateKeyPair(2048);
Console.WriteLine("Private Key:\n" + keyPair.PrivateKey);
Console.WriteLine("\nPublic Key:\n" + keyPair.PublicKey);
```

### 5. Authorization & Permission

**File:** `appsettings.Authorization.json`

**Tính n?ng:**
- Dynamic permission checking
- Permission caching
- Extensible permission providers

**Implementation:**
```csharp
public class MyPermissionService : IUserPermissionService
{
    public async Task<bool> HasPermissionAsync(
        ClaimsPrincipal user, 
        string resource, 
        string action)
    {
        // Your permission logic here
        return true;
    }
}
```

### 6. Rate Limiting

**File:** `appsettings.RateLimit.json`

**Tính n?ng:**
- Multiple rate limit types (Fixed, Sliding, TokenBucket, Concurrency)
- Whitelisted paths
- Custom error messages
- Rate limit headers

**Common scenarios:**
- Public API: 50-100 requests/minute
- Internal API: 200-500 requests/minute
- Admin API: 1000+ requests/minute

### 7. Database Connection

**File:** `appsettings.Database.json`

**Supported databases:**
- PostgreSQL (recommended for BaseNetCore.Core)
- SQL Server
- MySQL
- MongoDB
- Redis

**Connection pooling tips:**
- MinPoolSize: 5-10
- MaxPoolSize: 50-100 (depends on traffic)
- ConnectionIdleLifetime: 300 seconds

---

## ?? Khuy?n ngh? theo môi tr??ng

### Development
```json
// appsettings.Development.json
{
  "Serilog": {
    "MinimumLevel": { "Default": "Debug" }
  },
  "RateLimiting": {
    "Enabled": false  // T?t ?? test d? dàng
  },
  "PerformanceOptimization": {
    "EnableResponseCompression": false  // T?t ?? debug
  }
}
```

### Staging
```json
// appsettings.Staging.json
{
  "Serilog": {
    "MinimumLevel": { "Default": "Information" }
  },
  "RateLimiting": {
    "Enabled": true,
    "PermitLimit": 200
  }
}
```

### Production
```json
// appsettings.Production.json
{
  "Serilog": {
    "MinimumLevel": { "Default": "Warning" }
  },
  "RateLimiting": {
    "Enabled": true,
    "PermitLimit": 100,
    "Type": "Sliding"
  },
  "PerformanceOptimization": {
    "EnableResponseCompression": true,
    "BrotliCompressionLevel": "Optimal"
  }
}
```

---

## ?? Tài li?u liên quan

- [Main Documentation](../../README.md)
- [Configuration Guide](../../README/01-Getting-Started/Configuration.md)
- [Quick Start](../../README/01-Getting-Started/Quick-Start.md)
- [Middleware Pipeline](../../README/09-Middleware/Middleware-Pipeline.md)
- [Serilog Logging](../../README/10-Extensions/Serilog-Logging.md)

---

## ?? Tips & Tricks

### Merge multiple configurations
```csharp
// Program.cs
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env}.json", optional: true)
    .AddJsonFile("appsettings.SeriLog.json", optional: true)
    .AddJsonFile("appsettings.Performance.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();
```

### Validate configuration on startup
```csharp
// Program.cs
builder.Services.AddOptions<TokenSettings>()
    .Bind(builder.Configuration.GetSection("TokenSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### Override configuration with code
```csharp
builder.Services.Configure<PerformanceOptions>(options =>
{
    options.EnableResponseCompression = true;
    options.BrotliCompressionLevel = "Optimal";
});
```

---

**[?? Back to Main Documentation](../../README.md)**
