# ?? Configuration Guide

## Mục lục
- [Overview](#overview)
- [appsettings.json Configuration](#appsettingsjson-configuration)
- [Environment-specific Configuration](#environment-specific-configuration)
- [Security Configuration](#security-configuration)
- [Database Configuration](#database-configuration)
- [JWT Configuration](#jwt-configuration)
- [Encryption Configuration](#encryption-configuration)
- [Dynamic Permissions Configuration](#dynamic-permissions-configuration)
- [Logging Configuration](#logging-configuration)
- [Best Practices](#best-practices)

---

## ?? Overview

BaseNetCore.Core sẽ d?ng ASP.NET Core configuration system với c�c section ch�nh:

| Section | Mục đích | Bắt buộc |
|---------|----------|----------|
| **ConnectionStrings** | Database connections | ? |
| **TokenSettings** | JWT authentication | ? (nếu dùng auth) |
| **Aes** | Data encryption | ? (nếu dùng encryption) |
| **DynamicPermissions** | Authorization rules | ?? Optional |
| **Logging** | Logging configuration | ?? Optional |

---

## ?? appsettings.json Configuration

### Minimal Configuration (No Auth)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Full Configuration (With All Features)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;",
    "ReadOnlyConnection": "Server=localhost;Database=MyDb_ReadOnly;User Id=reader;Password=ReadPassword;"
  },
  "TokenSettings": {
    "RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\nMIIEowIBAAKCAQEA...\n-----END PRIVATE KEY-----",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG...\n-----END PUBLIC KEY-----",
  "AccessExpireTimeS": "3600",
    "RefreshExpireTimeS": "86400",
    "Issuer": "MyApp",
    "Audience": "MyAppUsers"
  },
  "Aes": {
    "SecretKey": "MySecretKey-32-Characters-Long!!"
  },
  "DynamicPermissions": {
    "PermitAll": [
      "/api/health",
      "/api/auth/login",
      "/api/auth/register"
    ],
    "Permissions": {
      "ProductService": [
        "/api/products:GET",
     "/api/products/*:GET",
        "/api/products:POST:@product.create",
        "/api/products/*:PUT:@product.update",
        "/api/products/*:DELETE:@product.delete"
      ],
 "UserService": [
        "/api/users:GET:@user.view",
        "/api/users/*:GET:@user.view",
        "/api/users:POST:@user.create",
        "/api/users/*:PUT:@user.update",
   "/api/users/*:DELETE:@user.delete"
      ]
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
  "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
 "https://yourdomain.com"
    ]
  }
}
```

---

## ?? Environment-specific Configuration

### Structure

```
?? YourProject/
??? appsettings.json  # Base config
??? appsettings.Development.json        # Dev overrides
??? appsettings.Staging.json        # Staging overrides
??? appsettings.Production.json         # Production overrides
??? appsettings.Local.json         # Local dev (git-ignored)
```

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyAppDb_Dev;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "TokenSettings": {
    "AccessExpireTimeS": "86400",
    "RefreshExpireTimeS": "604800"
  }
}
```

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "***INJECTED_FROM_AZURE_KEY_VAULT***"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "TokenSettings": {
    "AccessExpireTimeS": "1800",
    "RefreshExpireTimeS": "86400"
  }
}
```

---

## ?? Security Configuration

### ?? NEVER commit secrets to Git!

### Option 1: User Secrets (Development)

```bash
# Initialize user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "TokenSettings:RsaPrivateKey" "-----BEGIN PRIVATE KEY-----..."
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
dotnet user-secrets set "Aes:SecretKey" "YourSecretKey"
```

Access trong code:
```csharp
var privateKey = builder.Configuration["TokenSettings:RsaPrivateKey"];
```

### Option 2: Environment Variables (Production)

```bash
# Linux/Mac
export TokenSettings__RsaPrivateKey="-----BEGIN PRIVATE KEY-----..."
export ConnectionStrings__DefaultConnection="Server=..."
export Aes__SecretKey="YourSecretKey"

# Windows
set TokenSettings__RsaPrivateKey=-----BEGIN PRIVATE KEY-----...
set ConnectionStrings__DefaultConnection=Server=...
```

**Note:** D�ng `__` (double underscore) thay cho `:` trong environment variables.

### Option 3: Azure Key Vault (Production Recommended)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVaultEndpoint"]);
    builder.Configuration.AddAzureKeyVault(
   keyVaultEndpoint,
        new DefaultAzureCredential());
}
```

---

## ??? Database Configuration

### SQL Server

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MyDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### PostgreSQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=YourPassword;"
  }
}
```

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### MySQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=YourPassword;"
  }
}
```

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
```

### SQLite (Development/Testing)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=myapp.db"
  }
}
```

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## ?? JWT Configuration

### Generate RSA Keys

```csharp
using BaseNetCore.Core.src.Main.Security.Algorithm;

// In a separate console app or script
RsaKeyGenerator.PrintSampleConfiguration();
```

Output:
```
=== Generated RSA Keys for JWT Token ===

Add this to your appsettings.json:

"TokenSettings": {
  "RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
  "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
  "AccessExpireTimeS": "3600",
  "RefreshExpireTimeS": "86400",
  "Issuer": "your-issuer",
  "Audience": "your-audience"
}
```

### TokenSettings Explained

| Property | Description | Example | Recommended |
|----------|-------------|---------|-------------|
| **RsaPrivateKey** | Private key for signing tokens | `-----BEGIN ...` | Keep SECRET |
| **RsaPublicKey** | Public key for validating tokens | `-----BEGIN...` | Can share |
| **AccessExpireTimeS** | Access token lifetime (seconds) | `3600` | 15-60 mins |
| **RefreshExpireTimeS** | Refresh token lifetime (seconds) | `86400` | 7-30 days |
| **Issuer** | Token issuer (iss claim) | `"MyApp"` | Your app name |
| **Audience** | Token audience (aud claim) | `"MyAppUsers"` | Your users |

### Security Recommendations

- ? **Access Token:** 15-60 minutes (short-lived)
- ? **Refresh Token:** 7-30 days (stored securely)
- ? **Use RSA-2048 or RSA-4096** for production
- ?? **Rotate keys periodically** (quarterly recommended)

---

## ?? Encryption Configuration

### AES Configuration

```json
{
  "Aes": {
    "SecretKey": "MySecretKey-Must-Be-32-Chars!!"
  }
}
```

### Generate Strong Secret Key

```csharp
using System.Security.Cryptography;

var keyBytes = new byte[32]; // 256-bit
RandomNumberGenerator.Fill(keyBytes);
var secretKey = Convert.ToBase64String(keyBytes);
Console.WriteLine(secretKey); // Use this in appsettings.json
```

### Usage in Code

```csharp
using BaseNetCore.Core.src.Main.Security.Algorithm;

public class MyService
{
    private readonly AesAlgorithm _aes;

    public MyService(AesAlgorithm aes)
    {
        _aes = aes;
    }

  public string EncryptSensitiveData(string plainText)
  {
        return _aes.Encrypt(plainText);
    }

  public string DecryptSensitiveData(string cipherText)
  {
 return _aes.Decrypt(cipherText);
    }
}
```

---

## ??? Dynamic Permissions Configuration

### Structure

```json
{
  "DynamicPermissions": {
    "PermitAll": [
      "/api/health",
      "/api/auth/login"
    ],
    "Permissions": {
      "ServiceName": [
        "path:method:@permission.code"
  ]
    }
  }
}
```

### Syntax

```
/api/path:HTTP_METHOD:@permission.code
```

- **path**: URL path (supports `*` wildcard and `{REGEX}` placeholder)
- **HTTP_METHOD**: GET, POST, PUT, DELETE, PATCH (default: GET)
- **@permission.code**: Required permission (optional)

### Examples

```json
{
"DynamicPermissions": {
    "PermitAll": [
      "/api/health",
      "/api/auth/login",
    "/api/auth/register",
      "/swagger"
    ],
    "Permissions": {
"ProductService": [
  "/api/products:GET",  // Anyone can list products
        "/api/products/{REGEX}:GET",    // Anyone can view product detail
        "/api/products:POST:@product.create",  // Requires 'product.create' permission
        "/api/products/{REGEX}:PUT:@product.update",   // Requires 'product.update' permission
        "/api/products/{REGEX}:DELETE:@product.delete" // Requires 'product.delete' permission
      ],
      "AdminService": [
        "/api/admin/*:@admin.access"    // All admin paths require 'admin.access'
   ]
    }
  }
}
```

### Enable Dynamic Permissions Middleware

```csharp
// Program.cs
using BaseNetCore.Core.src.Main.Security.Permission;

builder.Services.AddSingleton<IDynamicPermissionProvider, DefaultDynamicPermissionProvider>();

var app = builder.Build();

// Add middleware
app.UseMiddleware<DynamicPermissionMiddleware>();
```

---

## ?? Logging Configuration

### Serilog (Recommended)

BaseNetCore.Core hỗ trợ Serilog với zero configuration hoặc full customization.

#### Option 1: Zero Configuration

```csharp
// Program.cs
builder.AddBaseNetCoreSerilog(); // Works without appsettings.json
```

#### Option 2: Full Configuration

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "BaseNetCore.Core": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 10485760,
          "rollOnFileSizeLimit": true,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "MyApp"
    }
  }
}
```

#### Logging Settings (Optional - for default configuration)

Nếu không có section "Serilog", BaseNetCore sẽ dùng các settings này:

```json
{
  "Logging": {
    "FilePath": "logs/log-.txt",
    "RetainedFileCountLimit": 30,
    "FileSizeLimitBytes": 10485760,
    "RollingInterval": "Day",
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

**Chi tiết xem tại:** [Serilog Logging Guide](../10-Extensions/Serilog-Logging.md)

### ASP.NET Core Default Logging

Nếu không dùng Serilog:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

---

## ? Best Practices

### ? DO

- ? **Use User Secrets** for local development
- ? **Use Azure Key Vault** for production
- ? **Separate configs** per environment
- ? **Validate config** at startup
- ? **Use strong encryption keys** (32+ characters)
- ? **Rotate JWT keys** periodically
- ? **Log config errors** (but not secrets!)

### ? DON'T

- ? **Commit secrets** to Git
- ? **Use weak passwords** in connection strings
- ? **Hard-code** sensitive values
- ? **Share private keys** publicly
- ? **Log** sensitive configuration values
- ? **Use same keys** across environments

### ?? Security Checklist

- [ ] Private keys stored in Azure Key Vault / AWS Secrets Manager
- [ ] Connection strings use environment variables
- [ ] `.gitignore` includes `appsettings.Local.json`
- [ ] Production uses HTTPS only
- [ ] JWT tokens expire within 1 hour
- [ ] Refresh tokens stored securely (HttpOnly cookies)
- [ ] CORS configured for known origins only
- [ ] Encryption keys are 256-bit or stronger

---

## ?? Configuration Validation

### Startup Validation

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Validate required configuration
var tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>();
if (string.IsNullOrEmpty(tokenSettings?.RsaPrivateKey))
{
    throw new InvalidOperationException("TokenSettings:RsaPrivateKey is required in configuration!");
}

var connString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required!");
}

// Continue with app setup...
```

### Health Check Endpoint

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

app.MapHealthChecks("/health");
```

---

## ?? Related Topics

- [Quick Start Guide](Quick-Start.md)
- [JWT Authentication](../04-Security/JWT-Authentication.md)
- [Dynamic Permissions](../05-Authorization/Dynamic-Permissions.md)
- [Security Best Practices](../12-Best-Practices/Security-Best-Practices.md)

---

**[? Back to Documentation](../README.md)**
