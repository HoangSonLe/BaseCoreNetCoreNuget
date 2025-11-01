# ?? Quick Start Guide

## Mục lục
- [Giới thiệu](#giới-thiệu)
- [Prerequisites](#prerequisites)
- [Bước 1: Tạo Project](#bước-1-tạo-project)
- [Bước 2: Cài đặt Packages](#bước-2-cài-đặt-packages)
- [Bước 3: Configuration](#bước-3-configuration)
- [Bước 4: Setup Program.cs](#bước-4-setup-programcs)
- [Bước 5: Tạo DbContext](#bước-5-tạo-dbcontext)
- [Bước 6: Tạo Entity](#bước-6-tạo-entity)
- [Bước 7: Tạo Service](#bước-7-tạo-service)
- [Bước 8: Tạo Controller](#bước-8-tạo-controller)
- [Bước 9: Run & Test](#bước-9-run--test)
- [Next Steps](#next-steps)

---

## ?? Giới thiệu

Hướng dẫn này sẽ giúp bạn tạo một Web API hoàn chỉnh với **BaseNetCore.Core** trong **15 phút**. Bạn sẽ có:

- ? JWT Authentication
- ? Repository Pattern + Unit of Work
- ? Global Exception Handling
- ? Auto Model Validation
- ? Vietnamese Search Support
- ? Swagger UI

---

## ? Prerequisites

- ? .NET 8.0 SDK
- ? Visual Studio 2022 / VS Code / Rider
- ? SQL Server / PostgreSQL / MySQL (ho?c SQLite cho demo)

---

## ?? Bước 1: Tạo Project

### Via .NET CLI

```bash
# Tạo solution
dotnet new sln -n MyApp

# Tạo Web API project
dotnet new webapi -n MyApp.API

# Add project vào solution
dotnet sln add MyApp.API/MyApp.API.csproj

# Navigate vào project
cd MyApp.API
```

### Via Visual Studio

1. **File ? New ? Project**
2. Chọn **ASP.NET Core Web API**
3. Name: `MyApp.API`
4. Framework: **.NET 8.0**
5. Configure:
   - ? Use controllers
   - ? Enable OpenAPI support
   - ? Use top-level statements (optional)
6. **Create**

---

## ?? Bước 2: Cài đặt Packages

```bash
# BaseNetCore.Core
dotnet add package BaseNetCore.Core

# Entity Framework Core (chọn 1 provider)
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
# ho?c
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# EF Core Tools (cho migrations)
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

## ?? Bước 3: Configuration

### 3.1 Generate RSA Keys cho JWT

Tạo file `GenerateKeys.cs`:

```csharp
using BaseNetCore.Core.src.Main.Security.Algorithm;

public class Program
{
    public static void Main()
    {
 RsaKeyGenerator.PrintSampleConfiguration();
    }
}
```

Chạy và copy keys:
```bash
dotnet run
```

### 3.2 Cấu hình `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "TokenSettings": {
 "RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\n[PASTE YOUR PRIVATE KEY HERE]\n-----END PRIVATE KEY-----",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n[PASTE YOUR PUBLIC KEY HERE]\n-----END PUBLIC KEY-----",
    "AccessExpireTimeS": "3600",
    "RefreshExpireTimeS": "86400",
    "Issuer": "MyApp",
    "Audience": "MyAppUsers"
  },
  "Aes": {
    "SecretKey": "MySecretKey-32-Characters-Long!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**?? Lưu ý:** Trong production, lưu secrets vào Azure Key Vault / AWS Secrets Manager, **không commit vào Git**!

---

## ??? Bước 4: Setup Program.cs

```csharp
using BaseNetCore.Core.src.Main.Extensions;
using BaseNetCore.Core.src.Main.DAL.Repository;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add BaseNetCore features with JWT authentication
//    Bao g?m: JWT Auth, Model Validation, AES Encryption, Controllers
builder.Services.AddBaseNetCoreFeaturesWithAuth(builder.Configuration);

// 3. Register Unit of Work (generic)
builder.Services.AddScoped<IUnitOfWork>(provider =>
{
    var dbContext = provider.GetRequiredService<ApplicationDbContext>();
    return new UnitOfWork(dbContext);
});

// 4. Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MyApp API", Version = "v1" });
 
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
  In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
  Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new()
    {
        {
     new()
    {
       Reference = new()
                {
  Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
           Id = "Bearer"
            }
      },
   Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

// 5. Add BaseNetCore middleware (Exception + Auth + Authorization)
app.UseBaseNetCoreMiddlewareWithAuth();

app.MapControllers();

app.Run();
```

---

## ??? Bước 5: Tạo DbContext

Tạo folder `Data` và file `ApplicationDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using MyApp.API.Models;

namespace MyApp.API.Data
{
    public class ApplicationDbContext : DbContext
{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options)
        {
   }

   public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
base.OnModelCreating(modelBuilder);

      // Configure entities
modelBuilder.Entity<Product>(entity =>
    {
              entity.HasKey(e => e.Id);
  entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
       entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

          modelBuilder.Entity<User>(entity =>
       {
        entity.HasKey(e => e.Id);
  entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
    entity.HasIndex(e => e.Email).IsUnique();
            });

       // Seed data
  modelBuilder.Entity<Product>().HasData(
           new Product { Id = 1, Name = "iPhone 15 Pro", Code = "IP15P", Price = 29990000, Stock = 10, IsActive = true },
              new Product { Id = 2, Name = "Samsung Galaxy S24", Code = "SGS24", Price = 24990000, Stock = 15, IsActive = true }
   );
 }
    }
}
```

---

## ?? Bước 6: Tạo Entity

Tạo folder `Models` và các entity files:

### `Product.cs` - Entity với Vietnamese Search

```csharp
using BaseNetCore.Core.src.Main.Common.Attributes;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyApp.API.Models
{
    /// <summary>
    /// Product entity với Vietnamese search support
    /// </summary>
    [SearchableEntity]
 public class Product : BaseSearchableEntity
    {
     public int Id { get; set; }

   [Required(ErrorMessage = "Tên sẽn ph?m là b?t bu?c")]
        [MaxLength(200, ErrorMessage = "Tên sẽn ph?m không ???c quá 200 ký t?")]
        [SearchableField(Order = 1)]
        public string Name { get; set; }

        [Required]
      [SearchableField(Order = 2)]
        public string Code { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá ph?i l?n h?n 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "S? l??ng không ???c âm")]
   public int Stock { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
```

### `User.cs` - Entity không c?n search

```csharp
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyApp.API.Models
{
    /// <summary>
    /// User entity - Không c?n search nên không có [SearchableEntity]
    /// </summary>
    public class User : BaseAuditableEntity
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
 public string PasswordHash { get; set; }

        public string FullName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
```

---

## ?? Bước 7: Tạo Service

Tạo folder `Services` và file `ProductService.cs`:

```csharp
using BaseNetCore.Core.src.Main.BLL.Services;
using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.DAL.Models.Specification;
using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Utils;
using Microsoft.AspNetCore.Http;
using MyApp.API.Models;

namespace MyApp.API.Services
{
    public interface IProductService
    {
    Task<PageResponse<Product>> GetProducts(int page, int size, string keyword = null);
     Task<Product> GetProductById(int id);
        Task<Product> CreateProduct(Product product);
        Task<Product> UpdateProduct(int id, Product product);
        Task DeleteProduct(int id);
    }

    public class ProductService : BaseService<Product>, IProductService
    {
  public ProductService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
   : base(unitOfWork, httpContextAccessor)
   {
        }

        public async Task<PageResponse<Product>> GetProducts(int page, int size, string keyword = null)
 {
            var spec = new BaseSpecification<Product>();

            // Search by keyword (Vietnamese support)
    if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
   spec.WithCriteria(p => p.NonUnicodeSearchString.Contains(normalized));
          }

       // Only active products
            spec.AndCriteria(p => p.IsActive);

     // Order by name
    spec.WithOrderBy(p => p.Name);

            // Apply paging
            spec.WithPagedResults(page, size);

            return await Repository.GetWithPagingAsync(spec);
        }

        public async Task<Product> GetProductById(int id)
        {
         var product = await Repository.GetByIdAsync(id);
            if (product == null)
            {
          throw new ResourceNotFoundException($"S?n ph?m ID {id} không t?n t?i");
        }
  return product;
     }

   public async Task<Product> CreateProduct(Product product)
  {
        // Validation: Check duplicate code
  var exists = await Repository.AnyAsync(p => p.Code == product.Code);
    if (exists)
          {
            throw new ConflictException($"Mã sẽn ph?m '{product.Code}' ?ã t?n t?i");
    }

            // Add product
     Repository.Add(product);
       await UnitOfWork.SaveChangesAsync();

            return product;
    }

        public async Task<Product> UpdateProduct(int id, Product product)
        {
     var existing = await GetProductById(id);

    // Update fields
        existing.Name = product.Name;
            existing.Code = product.Code;
   existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.IsActive = product.IsActive;

            Repository.Update(existing);
     await UnitOfWork.SaveChangesAsync();

       return existing;
        }

        public async Task DeleteProduct(int id)
        {
      var product = await GetProductById(id);
Repository.Delete(product);
 await UnitOfWork.SaveChangesAsync();
        }
    }
}
```

**??ng ký Service trong `Program.cs`:**

```csharp
// Add after AddBaseNetCoreFeaturesWithAuth
builder.Services.AddScoped<IProductService, ProductService>();
```

---

## ?? Bước 8: Tạo Controller

Tạo folder `Controllers` và file `ProductsController.cs`:

```csharp
using BaseNetCore.Core.src.Main.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.API.Models;
using MyApp.API.Services;

namespace MyApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
  {
     _productService = productService;
            _logger = logger;
        }

  /// <summary>
        /// L?y danh sách sẽn ph?m với phân trang và tìm ki?m
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PageResponse<Product>>> GetProducts(
 [FromQuery] int page = 1,
  [FromQuery] int size = 20,
        [FromQuery] string keyword = null)
        {
   var result = await _productService.GetProducts(page, size, keyword);
        return Ok(result);
        }

        /// <summary>
        /// L?y chi ti?t sẽn ph?m theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductById(id);
            return Ok(product);
        }

        /// <summary>
        /// Tạo sẽn ph?m m?i
        /// </summary>
   [HttpPost]
[Authorize] // C?n JWT token
      public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
     {
var created = await _productService.CreateProduct(product);
      return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
      }

  /// <summary>
  /// C?p nh?t sẽn ph?m
        /// </summary>
        [HttpPut("{id}")]
   [Authorize]
        public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product product)
        {
            var updated = await _productService.UpdateProduct(id, product);
       return Ok(updated);
        }

        /// <summary>
        /// Xóa sẽn ph?m
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProduct(id);
     return NoContent();
     }
    }
}
```

---

## ?? Bước 9: Run & Test

### 9.1 Create Database Migration

```bash
# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### 9.2 Run Application

```bash
dotnet run
```

ho?c **F5** trong Visual Studio.

### 9.3 Test với Swagger

M? browser: **https://localhost:5001/swagger**

#### Test GET Products (không c?n auth)

1. Expand **GET /api/Products**
2. Click **Try it out**
3. Click **Execute**

K?t qu?:
```json
{
  "data": [
    {
      "id": 1,
      "name": "iPhone 15 Pro",
      "code": "IP15P",
      "price": 29990000,
      "stock": 10,
      "isActive": true
    }
  ],
  "success": true,
  "total": 2,
  "currentPage": 1,
  "pageSize": 20
}
```

#### Test Search (Vietnamese)

1. **GET /api/Products?keyword=iphone** ?
2. **GET /api/Products?keyword=dien thoai** ? (không d?u v?n tìm ???c)
3. **GET /api/Products?keyword=ip15** ?

#### Test POST Product (c?n JWT)

1. Expand **POST /api/Products**
2. Click **Authorize** button (top-right)
3. Nh?p JWT token: `Bearer <your-token>`
4. Click **Try it out**
5. Body:
```json
{
  "name": "?i?n tho?i Xiaomi 14",
  "code": "XM14",
  "price": 15990000,
  "stock": 20,
  "isActive": true
}
```
6. Click **Execute**

#### Test Error Handling

**GET /api/Products/999** ? Tr? v?:
```json
{
  "guid": "abc-123",
  "code": "SYS005",
  "message": "S?n ph?m ID 999 không t?n t?i",
  "path": "/api/Products/999",
  "method": "GET",
  "timestamp": "2025-01-28T10:00:00Z"
}
```

---

## ?? Xong r?i!

Bạn ?ã có một Web API hoàn chỉnh với:

- ? **Repository Pattern** - Clean data access
- ? **Unit of Work** - Transaction management
- ? **JWT Authentication** - Secure API
- ? **Global Exception Handling** - Standardized errors
- ? **Model Validation** - Automatic validation
- ? **Vietnamese Search** - Tìm ki?m không d?u
- ? **Swagger UI** - API documentation
- ? **Pagination** - Scalable listing

---

## ?? Next Steps

### ?? H?c thêm v? các tính n?ng:

1. **[Repository Pattern](../02-Data-Access-Layer/Repository-Pattern.md)** - Hi?u rõ v? Repository
2. **[JWT Authentication](../04-Security/JWT-Authentication.md)** - Setup authentication flow
3. **[Vietnamese Search](../07-Searchable-Entities/Vietnamese-Search.md)** - Advanced search scenarios
4. **[Dynamic Permissions](../05-Authorization/Dynamic-Permissions.md)** - Fine-grained authorization

### ??? M? r?ng ?ng d?ng:

- ? Thêm **User Authentication** endpoint
- ? Implement **Refresh Token** mechanism
- ? Add **Audit Logging** cho changes
- ? Setup **Dynamic Permissions** t? config
- ? Integrate **Redis Caching**

### ?? ??c Best Practices:

- [Architecture Guidelines](../12-Best-Practices/Architecture-Guidelines.md)
- [Performance Optimization](../12-Best-Practices/Performance-Optimization.md)
- [Security Best Practices](../12-Best-Practices/Security-Best-Practices.md)

---

## ?? Tips

### Performance
- ? Dùng `AsNoTracking()` cho read-only queries
- ? Dùng Specification Pattern cho complex queries
- ? Enable response compression

### Security
- ?? **Không commit secrets vào Git**
- ? Dùng HTTPS trong production
- ? Validate input ? t?t c? endpoints

### Code Organization
- ? Tách DTOs ra kh?i Entities
- ? Dùng AutoMapper cho mapping
- ? Implement CQRS cho large applications

---

**[? Back to Documentation](../README.md)**
