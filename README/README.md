# ?? BaseNetCore.Core - Enterprise .NET 8 Framework

<div align="center">

[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/BaseNetCore.Core.svg)](https://www.nuget.org/packages/BaseNetCore.Core/)

**Framework .NET 8 hoàn chỉnh cho ?ng d?ng Enterprise với Repository Pattern, JWT Authentication, Dynamic Permissions, và Vietnamese Search Support**

[?? Documentation](#-documentation) • [?? Quick Start](#-quick-start) • [? Features](#-features) • [?? Installation](#-installation)

</div>

---

## ?? Mục lục

- [Giới thiệu](#-giới-thiệu)
- [Tính n?ng n?i b?t](#-tính-n?ng-n?i-b?t)
- [Cài đặt](#-cài-đặt)
- [Quick Start](#-quick-start)
- [Cấu trúc Documentation](#-cấu-trúc-documentation)
- [Architecture Overview](#-architecture-overview)
- [So sánh với các Framework khác](#-so-sánh-với-các-framework-khác)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)
- [License](#-license)

---

## ?? Giới thiệu

**BaseNetCore.Core** là một framework .NET 8 ???c thi?t k? ??:

- ? **Gi?m boilerplate code** - T? ??ng hóa các tác v? l?p ?i l?p l?i
- ? **T?ng t?c development** - Focus vào business logic thay vì infrastructure
- ? **??m b?o consistency** - Chu?n hóa error handling, API responses
- ? **Enterprise-ready** - Production-ready với security, performance, scalability
- ? **Vietnam-friendly** - H? tr? tìm ki?m ti?ng Vi?t không d?u

### ?? Dành cho ai?

- ????? Developers xây d?ng Web APIs với .NET 8
- ?? Teams c?n chu?n hóa architecture cho nhi?u projects
- ?? Startups c?n MVP nhanh với best practices
- ?? H?c sinh/sinh viên mu?n h?c enterprise patterns

---

## ? Tính n?ng n?i b?t

### ??? Data Access Layer
- ? **Generic Repository Pattern** - CQRS support, async/await
- ? **Unit of Work Pattern** - Transaction management
- ? **Specification Pattern** - Encapsulate complex queries
- ? **Query Object Pattern** - Fluent API cho queries
- ? **Auto-tracking management** - Performance optimization

### ?? Security & Authentication
- ? **JWT Authentication** - RSA-based signing/validation
- ? **Token Service** - Access & Refresh tokens
- ? **AES-GCM Encryption** - Data encryption at rest
- ? **Dynamic Permissions** - Runtime authorization
- ? **User Context** - Automatic user info extraction t? JWT

### ?? Exception Handling
- ? **Standardized Error Responses** - `ApiErrorResponse` format
- ? **Custom Error Codes** - Extensible error code system
- ? **Global Exception Middleware** - Catch all exceptions
- ? **Built-in Exception Types** - 11 pre-defined exceptions (400, 401, 403, 404, 409, 429, 500, 503)

### ?? Vietnamese Search Support
- ? **Searchable Fields** - Attribute-based search configuration
- ? **Diacritics Removal** - Automatic Vietnamese accent removal
- ? **Full-text Search** - Search across multiple fields
- ? **Opt-in Design** - Only entities marked with `[SearchableEntity]`

### ?? API Models & Validation
- ? **Pagination** - `PageRequest` & `PageResponse<T>`
- ? **Auto Model Validation** - DataAnnotations support
- ? **Standardized Responses** - Consistent API response format
- ? **Sorting & Filtering** - Built-in support

### ??? Developer Experience
- ? **Fluent Configuration** - Extension methods cho easy setup
- ? **Dependency Injection** - Auto-registration support
- ? **Base Services** - Common functionality cho services
- ? **Comprehensive Examples** - Real-world scenarios

---

## ?? Cài đặt

### Via NuGet Package Manager
```bash
Install-Package BaseNetCore.Core
```

### Via .NET CLI
```bash
dotnet add package BaseNetCore.Core
```

### Via Package Reference
```xml
<PackageReference Include="BaseNetCore.Core" Version="1.0.0" />
```

---

## ?? Quick Start

### 1?? Cấu hình `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyDb;User Id=sa;Password=***;"
  },
  "TokenSettings": {
    "RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
    "AccessExpireTimeS": "3600",
    "RefreshExpireTimeS": "86400",
    "Issuer": "your-issuer",
    "Audience": "your-audience"
  },
  "Aes": {
    "SecretKey": "your-32-character-secret-key-here!!"
  }
}
```

### 2?? Cấu hình `Program.cs`

```csharp
using BaseNetCore.Core.src.Main.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add BaseNetCore features with JWT authentication
builder.Services.AddBaseNetCoreFeaturesWithAuth(builder.Configuration);

// Register UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

// Add BaseNetCore middleware with authentication
app.UseBaseNetCoreMiddlewareWithAuth();

app.MapControllers();

app.Run();
```

### 3?? Tạo Entity với Searchable Support

```csharp
using BaseNetCore.Core.src.Main.Common.Attributes;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;

[SearchableEntity]
public class Product : BaseSearchableEntity
{
  public int Id { get; set; }
    
    [SearchableField(Order = 1)]
    public string Name { get; set; }
    
    [SearchableField(Order = 2)]
    public string Code { get; set; }
    
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
```

### 4?? Tạo Service với Repository & UnitOfWork

```csharp
using BaseNetCore.Core.src.Main.BLL.Services;
using BaseNetCore.Core.src.Main.DAL.Repository;

public class ProductService : BaseService<Product>
{
  public ProductService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, httpContextAccessor)
    {
 }

    public async Task<int> CreateProduct(Product product)
    {
    // CurrentUserId, CurrentUsername are available from BaseService
   Repository.Add(product);
        return await UnitOfWork.SaveChangesAsync();
    }

    public async Task<List<Product>> SearchProducts(string keyword)
    {
        var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
        return await Repository.GetAllAsync(
    filter: p => p.NonUnicodeSearchString.Contains(normalized)
        );
    }
}
```

### 5?? Tạo Controller với Exception Handling

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
  private readonly ProductService _productService;

public ProductsController(ProductService productService)
    {
     _productService = productService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
     var product = await _productService.Repository.GetByIdAsync(id);
        
        if (product == null)
       throw new ResourceNotFoundException($"Product {id} not found");
            
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        var result = await _productService.CreateProduct(product);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}
```

? **Xong!** Bạn ?ã có:
- ? JWT Authentication
- ? Global Exception Handling
- ? Auto Model Validation
- ? Repository Pattern
- ? Vietnamese Search Support
- ? Standardized API Responses

---

## ?? Cấu trúc Documentation

Tài li?u ???c t? ch?c theo modules ?? d? tìm ki?m:

### ?? Getting Started
- [?? Installation Guide](01-Getting-Started/Installation.md)
- [?? Quick Start Tutorial](01-Getting-Started/Quick-Start.md)
- [?? Configuration Guide](01-Getting-Started/Configuration.md)

### ??? Data Access Layer
- [?? Repository Pattern](02-Data-Access-Layer/Repository-Pattern.md)
- [?? Unit of Work](02-Data-Access-Layer/Unit-Of-Work.md)
- [?? Specification Pattern](02-Data-Access-Layer/Specification-Pattern.md)
- [?? Query Object Pattern](02-Data-Access-Layer/Query-Object-Pattern.md)

### ?? Business Logic Layer
- [??? Base Service](03-Business-Logic-Layer/Base-Service.md)
- [?? User Context](03-Business-Logic-Layer/User-Context.md)

### ?? Security
- [?? JWT Authentication](04-Security/JWT-Authentication.md)
- [?? Token Service](04-Security/Token-Service.md)
- [?? AES Encryption](04-Security/AES-Encryption.md)
- [??? RSA Key Generation](04-Security/RSA-Key-Generation.md)

### ??? Authorization
- [? Dynamic Permissions](05-Authorization/Dynamic-Permissions.md)
- [?? Permission Middleware](05-Authorization/Permission-Middleware.md)
- [?? User Permission Service](05-Authorization/User-Permission-Service.md)

### ?? Exception Handling
- [?? Exception System](06-Exception-Handling/Exception-System.md)
- [??? Custom Error Codes](06-Exception-Handling/Custom-Error-Codes.md)
- [?? Global Exception Middleware](06-Exception-Handling/Global-Exception-Middleware.md)

### ?? Searchable Entities
- [?? Searchable Fields](07-Searchable-Entities/Searchable-Fields.md)
- [???? Vietnamese Search](07-Searchable-Entities/Vietnamese-Search.md)
- [?? Search Best Practices](07-Searchable-Entities/Search-Best-Practices.md)

### ?? API Models
- [?? Pagination](08-API-Models/Pagination.md)
- [?? API Responses](08-API-Models/API-Responses.md)
- [? Model Validation](08-API-Models/Model-Validation.md)

### ?? Middleware
- [?? Middleware Pipeline](09-Middleware/Middleware-Pipeline.md)
- [??? Custom Middleware](09-Middleware/Custom-Middleware.md)

### ?? Extensions
- [? BaseNetCore Extensions](10-Extensions/BaseNetCore-Extensions.md)
- [?? Authentication Extensions](10-Extensions/Authentication-Extensions.md)
- [?? Validation Extensions](10-Extensions/Validation-Extensions.md)

### ??? Utilities
- [?? DI Utilities](11-Utilities/DI-Utilities.md)
- [?? String Utilities](11-Utilities/String-Utilities.md)
- [?? Enum Utilities](11-Utilities/Enum-Utilities.md)

### ?? Best Practices
- [??? Architecture Guidelines](12-Best-Practices/Architecture-Guidelines.md)
- [? Performance Optimization](12-Best-Practices/Performance-Optimization.md)
- [?? Security Best Practices](12-Best-Practices/Security-Best-Practices.md)
- [?? Testing Strategies](12-Best-Practices/Testing-Strategies.md)

### ?? Migration Guides
- [?? From EF Core to BaseNetCore](13-Migration-Guides/From-EF-Core-To-BaseNetCore.md)
- [?? From Other Frameworks](13-Migration-Guides/From-Other-Frameworks.md)

### ?? Examples
- [?? Complete CRUD Example](14-Examples/Complete-CRUD-Example.md)
- [?? Authentication Example](14-Examples/Authentication-Example.md)
- [?? Search Example](14-Examples/Search-Example.md)
- [?? Advanced Scenarios](14-Examples/Advanced-Scenarios.md)

### ?? API Reference
- [?? Interfaces Reference](15-API-Reference/Interfaces-Reference.md)
- [??? Classes Reference](15-API-Reference/Classes-Reference.md)
- [??? Attributes Reference](15-API-Reference/Attributes-Reference.md)

---

## ??? Architecture Overview

```mermaid
graph TB
    subgraph "Presentation Layer"
   A[Controllers] --> B[DTOs/Models]
    end
    
    subgraph "Business Logic Layer"
        C[Services] --> D[BaseService]
        C --> E[Business Logic]
    end
    
    subgraph "Data Access Layer"
        F[Repository] --> G[UnitOfWork]
        F --> H[Specification]
      G --> I[DbContext]
    end
  
    subgraph "Cross-Cutting Concerns"
      J[Exception Middleware]
        K[Auth Middleware]
        L[Permission Middleware]
    end
    
    A --> C
    C --> F
    J -.-> A
    K -.-> A
    L -.-> A
```

### ?? Layer Responsibilities

| Layer | Responsibility | Key Components |
|-------|----------------|----------------|
| **Presentation** | Handle HTTP requests/responses | Controllers, DTOs, Validation |
| **Business Logic** | Implement business rules | Services, Domain Logic, User Context |
| **Data Access** | Database operations | Repository, UnitOfWork, Specifications |
| **Cross-Cutting** | Infrastructure concerns | Middleware, Exceptions, Logging |

---

## ?? So sánh với các Framework khác

| Feature | BaseNetCore.Core | Clean Architecture | ABP Framework | Ardalis.Specification |
|---------|------------------|-------------------|---------------|----------------------|
| **Repository Pattern** | ? Generic + CQRS | ? Custom | ? Generic | ? Specification-based |
| **JWT Auth** | ? RSA-based | ? Manual setup | ? Identity Server | ? Manual setup |
| **Exception Handling** | ? Global + Codes | ?? Manual | ? Built-in | ? Manual |
| **Vietnamese Search** | ? Built-in | ? | ? | ? |
| **Dynamic Permissions** | ? Config-based | ? | ? Complex | ? |
| **Learning Curve** | ?? Low | ?? High | ?? Very High | ?? Medium |
| **Setup Time** | ?? 5 minutes | ?? 2+ hours | ?? 4+ hours | ?? 30 minutes |
| **Flexibility** | ?? High | ?? High | ?? Medium | ?? High |
| **Documentation** | ?? Vietnamese | ?? English | ?? English | ?? Limited |

### ?? Khi nào dùng BaseNetCore.Core?

? **Dùng khi:**
- C?n MVP/prototype nhanh với best practices
- Team nh?, c?n consistency cross projects
- ?ng d?ng v?a và nh? ??n enterprise
- C?n Vietnamese search support
- Mu?n focus vào business logic

? **Không dùng khi:**
- Microservices ph?c t?p với DDD (dùng ABP)
- C?n customization sâu ? data layer
- Team ?ã có architecture riêng ???c thi?t l?p

---

## ??? Roadmap

### ? Version 1.0 (Current)
- ? Repository + UnitOfWork Pattern
- ? JWT Authentication
- ? Exception Handling
- ? Vietnamese Search
- ? Dynamic Permissions
- ? Model Validation

### ?? Version 1.1 (Q2 2025)
- ?? Distributed Caching (Redis)
- ?? Background Jobs (Hangfire integration)
- ?? Audit Logging (Enhanced)
- ?? Multi-tenancy Support

### ?? Version 2.0 (Q4 2025)
- ?? GraphQL Support
- ?? gRPC Integration
- ?? Real-time with SignalR
- ?? Advanced CQRS + MediatR

---

## ?? Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md).

### ?? Báo l?i
- [GitHub Issues](https://github.com/HoangSonLe/BaseCoreNetCoreNuget/issues)

### ?? ?? xu?t tính n?ng
- [GitHub Discussions](https://github.com/HoangSonLe/BaseCoreNetCoreNuget/discussions)

### ?? Liên h?
- Email: support@basenetcore.com
- Website: [basenetcore.com](https://basenetcore.com)

---

## ?? License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

---

## ?? Acknowledgments

- Inspired by Clean Architecture, ABP Framework, and Ardalis.Specification
- Built with ?? for the .NET community in Vietnam

---

<div align="center">

**[? Back to Top](#-basenetcorecore---enterprise-net-8-framework)**

Made with ?? by [Hoang Son Le](https://github.com/HoangSonLe)

</div>
