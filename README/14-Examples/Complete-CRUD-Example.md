# ?? Complete CRUD Example

## Full Stack CRUD Example

Hướng dẫn t?ng bước xây d?ng hoàn chỉnh CRUD API với BaseNetCore.Core.

---

## 1. Entity

```csharp
using BaseNetCore.Core.src.Main.Common.Attributes;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using System.ComponentModel.DataAnnotations;

[SearchableEntity]
public class Product : BaseSearchableEntity
{
    public int Id { get; set; }

    [SearchableField(Order = 1)]
    public string Name { get; set; }

    [SearchableField(Order = 2)]
    public string Code { get; set; }

    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; }
    public bool IsActive { get; set; } = true;
}
```

---

## 2. DTOs

```csharp
// Request DTOs
public class CreateProductDto
{
    [Required] [MaxLength(200)]
    public string Name { get; set; }

    [Required] [RegularExpression(@"^[A-Z]{2,4}\d{2,6}$")]
    public string Code { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }

    [Range(0, double.MaxValue)]
 public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    public string Category { get; set; }
}

public class UpdateProductDto
{
    [Required] [MaxLength(200)]
    public string Name { get; set; }

  [MaxLength(1000)]
    public string Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool IsActive { get; set; }
}

// Response DTO
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
 public string Code { get; set; }
    public string Description { get; set; }
 public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 3. Service

```csharp
public interface IProductService
{
    Task<PageResponse<ProductDto>> GetProductsAsync(int page, int size, string keyword);
    Task<ProductDto> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);
    Task DeleteProductAsync(int id);
}

public class ProductService : BaseService<Product>, IProductService
{
    public ProductService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, httpContextAccessor)
    {
}

    public async Task<PageResponse<ProductDto>> GetProductsAsync(int page, int size, string keyword)
    {
        var spec = new BaseSpecification<Product>()
     .WithCriteria(p => p.IsActive);

if (!string.IsNullOrWhiteSpace(keyword))
{
      var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
            spec.AndCriteria(p => p.NonUnicodeSearchString.Contains(normalized));
 }

 spec.WithOrderBy(p => p.Name)
            .WithPagedResults(page, size);

        var result = await Repository.GetWithPagingAsync(spec);

        return new PageResponse<ProductDto>(
  result.Data.Select(MapToDto).ToList(),
   result.Success,
     result.Total,
         result.CurrentPage,
     result.PageSize
        );
 }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var product = await Repository.GetByIdAsync(id);
 if (product == null)
   throw new ResourceNotFoundException($"Product {id} not found");

        return MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // Check duplicate code
        var exists = await Repository.AnyAsync(p => p.Code == dto.Code);
   if (exists)
     throw new ConflictException($"Product code '{dto.Code}' already exists");

        var product = new Product
   {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
    Price = dto.Price,
       Stock = dto.Stock,
     Category = dto.Category,
            IsActive = true
        };

        Repository.Add(product);
    await UnitOfWork.SaveChangesAsync();

return MapToDto(product);
    }

 public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
{
        var product = await Repository.GetByIdAsync(id);
    if (product == null)
throw new ResourceNotFoundException($"Product {id} not found");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
     product.Stock = dto.Stock;
        product.IsActive = dto.IsActive;

   Repository.Update(product);
        await UnitOfWork.SaveChangesAsync();

     return MapToDto(product);
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await Repository.GetByIdAsync(id);
        if (product == null)
         throw new ResourceNotFoundException($"Product {id} not found");

        Repository.Delete(product);
        await UnitOfWork.SaveChangesAsync();
    }

    private ProductDto MapToDto(Product product)
    {
 return new ProductDto
        {
    Id = product.Id,
            Name = product.Name,
     Code = product.Code,
   Description = product.Description,
        Price = product.Price,
            Stock = product.Stock,
   Category = product.Category,
       IsActive = product.IsActive,
    CreatedAt = product.CreatedAt
};
    }
}
```

---

## 4. Controller

```csharp
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
    /// Get paginated products with optional search
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PageResponse<ProductDto>), 200)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
   [FromQuery] int size = 20,
        [FromQuery] string keyword = null)
    {
   var result = await _productService.GetProductsAsync(page, size, keyword);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(product);
    }

  /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductDto), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        _logger.LogInformation("Creating product: {ProductName}", dto.Name);
        
        var product = await _productService.CreateProductAsync(dto);
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
     _logger.LogInformation("Updating product: {ProductId}", id);
 
        var product = await _productService.UpdateProductAsync(id, dto);
        
        return Ok(product);
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> DeleteProduct(int id)
 {
        _logger.LogInformation("Deleting product: {ProductId}", id);
    
   await _productService.DeleteProductAsync(id);
     
        return NoContent();
    }
}
```

---

## 5. Registration (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// BaseNetCore features with auth
builder.Services.AddBaseNetCoreFeaturesWithAuth(builder.Configuration);

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork>(provider =>
{
    var dbContext = provider.GetRequiredService<ApplicationDbContext>();
    return new UnitOfWork(dbContext);
});

// Services
builder.Services.AddScoped<IProductService, ProductService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseBaseNetCoreMiddlewareWithAuth();
app.MapControllers();
app.Run();
```

---

## 6. Test Requests

### GET /api/products

```http
GET /api/products?page=1&size=20&keyword=iphone
```

**Response: 200**
```json
{
  "data": [
    {
    "id": 1,
      "name": "iPhone 15 Pro",
  "code": "IP15PRO",
      "description": "Latest iPhone",
      "price": 29990000,
 "stock": 50,
      "category": "Smartphone",
      "isActive": true,
      "createdAt": "2025-01-28T10:00:00Z"
    }
  ],
  "success": true,
  "total": 1,
  "currentPage": 1,
  "pageSize": 20
}
```

### POST /api/products

```http
POST /api/products
Authorization: Bearer {token}
{
  "name": "Samsung Galaxy S24",
  "code": "SGS24",
  "description": "Latest Samsung flagship",
  "price": 24990000,
  "stock": 30,
  "category": "Smartphone"
}
```

**Response: 201**
```json
{
  "id": 2,
"name": "Samsung Galaxy S24",
  "code": "SGS24",
  "description": "Latest Samsung flagship",
  "price": 24990000,
  "stock": 30,
  "category": "Smartphone",
  "isActive": true,
  "createdAt": "2025-01-28T11:00:00Z"
}
```

---

**[? Back to Documentation](../README.md)**
