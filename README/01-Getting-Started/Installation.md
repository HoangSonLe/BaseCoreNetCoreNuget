# ?? Installation Guide

## Mục lục
- [Yêu cấu h? th?ng](#yêu-cấu-h?-th?ng)
- [Cài đặt Package](#cài-đặt-package)
- [Cài đặt Dependencies](#cài-đặt-dependencies)
- [Ki?m tra cài đặt](#ki?m-tra-cài-đặt)
- [Troubleshooting](#troubleshooting)

---

## ??? Yêu cấu h? th?ng

### Bắt buộc
- ? **.NET 8.0 SDK** tr? lên
- ? **Visual Studio 2022** (17.8+) ho?c **VS Code** với C# extension
- ? **SQL Server** / **PostgreSQL** / **MySQL** (tùy chọn theo database)

### Ki?m tra phiên bạn .NET

```bash
dotnet --version
# Output: 8.0.xxx ho?c cao h?n
```

N?u ch?a có .NET 8, download t?i: https://dotnet.microsoft.com/download/dotnet/8.0

---

## ?? Cài đặt Package

### Option 1: Via NuGet Package Manager Console

```powershell
Install-Package BaseNetCore.Core
```

### Option 2: Via .NET CLI

```bash
dotnet add package BaseNetCore.Core
```

### Option 3: Via Visual Studio NuGet Manager

1. Right-click vào Project ? **Manage NuGet Packages**
2. Chọn tab **Browse**
3. Search: `BaseNetCore.Core`
4. Click **Install**

### Option 4: Manual Edit .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

<ItemGroup>
    <PackageReference Include="BaseNetCore.Core" Version="1.0.0" />
  </ItemGroup>
</Project>
```

Sau ?ó chạy:
```bash
dotnet restore
```

---

## ?? Cài đặt Dependencies

BaseNetCore.Core sẽ t? ??ng cài các dependencies sau:

| Package | Version | M?c ?ích |
|---------|---------|----------|
| **Microsoft.EntityFrameworkCore** | 8.0+ | Database ORM |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 8.0+ | JWT Authentication |
| **Swashbuckle.AspNetCore** | 6.5+ | Swagger/OpenAPI |
| **System.IdentityModel.Tokens.Jwt** | 7.0+ | JWT Token handling |

### Cài thêm Database Provider (tùy chọn)

**SQL Server:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

**PostgreSQL:**
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

**MySQL:**
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

**SQLite (cho dev/testing):**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

---

## ? Ki?m tra cài đặt

### 1?? Ki?m tra Package ?ã cài

```bash
dotnet list package
```

K?t qu? ph?i có:
```
Project 'YourProject' has the following package references
   [net8.0]:
   Top-level Package          Requested   Resolved
   > BaseNetCore.Core 1.0.0   1.0.0
   ...
```

### 2?? Test Import Namespace

Tạo file test `TestInstallation.cs`:

```csharp
using BaseNetCore.Core.src.Main.Extensions;
using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Common.Exceptions;

public class TestInstallation
{
    public void Test()
    {
        // N?u compile OK = cài đặt thành công
        Console.WriteLine("BaseNetCore.Core installed successfully!");
    }
}
```

Build project:
```bash
dotnet build
```

N?u build thành công = cài đặt OK! ?

### 3?? Test Runtime

Tạo minimal API test trong `Program.cs`:

```csharp
using BaseNetCore.Core.src.Main.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Test BaseNetCore extensions
builder.Services.AddBaseNetCoreFeatures(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => new
{
    Status = "OK",
    Framework = "BaseNetCore.Core",
    Version = "1.0.0"
});

app.Run();
```

Chạy:
```bash
dotnet run
```

Truy c?p: http://localhost:5000/health

K?t qu?:
```json
{
  "status": "OK",
  "framework": "BaseNetCore.Core",
  "version": "1.0.0"
}
```

? **Cài đặt thành công!**

---

## ?? Troubleshooting

### ? L?i: "Package 'BaseNetCore.Core' is not found"

**Nguyên nhân:** NuGet source ch?a ?úng ho?c package ch?a publish.

**Gi?i pháp:**
```bash
# Thêm NuGet source (n?u c?n)
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# Clear cache và restore
dotnet nuget locals all --clear
dotnet restore
```

### ? L?i: "The type or namespace name 'BaseNetCore' could not be found"

**Nguyên nhân:** Using statement ch?a ?úng ho?c package ch?a restore.

**Gi?i pháp:**
```bash
dotnet restore
dotnet clean
dotnet build
```

### ? L?i: "Ambiguous reference between 'X' and 'Y'"

**Nguyên nhân:** Conflict gi?a BaseNetCore và package khác.

**Gi?i pháp:** Explicit namespace:
```csharp
using BaseNetCore.Core.src.Main.DAL.Repository.IRepository;
// instead of
using IRepository;
```

### ? L?i: "Could not load file or assembly 'BaseNetCore.Core'"

**Nguyên nhân:** Version mismatch ho?c corrupted package.

**Gi?i pháp:**
```bash
# Xóa bin/obj folders
rm -rf bin obj

# Reinstall package
dotnet remove package BaseNetCore.Core
dotnet add package BaseNetCore.Core

# Rebuild
dotnet restore
dotnet build
```

### ? L?i: "TargetFramework 'net8.0' is not supported"

**Nguyên nhân:** Ch?a cài .NET 8 SDK.

**Gi?i pháp:**
1. Download .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
2. Install và restart IDE
3. Verify: `dotnet --version`

---

## ?? Next Steps

Sau khi cài đặt thành công:

1. ?? ??c [Quick Start Guide](Quick-Start.md)
2. ?? Xem [Configuration Guide](Configuration.md)
3. ?? Xem [Complete CRUD Example](../14-Examples/Complete-CRUD-Example.md)

---

## ?? Related Topics

- [Configuration Guide](Configuration.md)
- [Quick Start Tutorial](Quick-Start.md)
- [Project Setup Best Practices](../12-Best-Practices/Architecture-Guidelines.md)

---

**[? Back to Documentation](../README.md)**
