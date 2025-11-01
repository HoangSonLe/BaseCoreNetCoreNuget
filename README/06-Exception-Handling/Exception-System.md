# ?? Exception System

## Giới thiệu

BaseNetCore.Core cung c?p **standardized exception handling system** với:

- ? **Custom error codes** - Extensible error code system
- ? **HTTP status mapping** - Auto-map exceptions to status codes
- ? **Consistent responses** - ApiErrorResponse format
- ? **Global handling** - Catch all unhandled exceptions

---

## Exception Hierarchy

```
Exception
??? BaseApplicationException (BaseNetCore)
    ??? SystemErrorException (500)
    ??? ServerErrorException (500)
    ??? ServiceUnavailableException (503)
    ??? BadRequestException (400)
    ??? RequestInvalidException (400)
    ??? ResourceNotFoundException (404)
    ??? ConflictException (409)
    ??? ForbiddenException (403)
    ??? SystemAuthorizationException (403)
    ??? TokenInvalidException (401)
  ??? TooManyRequestsException (429)
```

---

## BaseApplicationException

```csharp
public class BaseApplicationException : Exception
{
    public IErrorCode ErrorCode { get; }
 public new string Message { get; }
    public HttpStatusCode HttpStatus { get; }

    public BaseApplicationException(
        IErrorCode errorCode, 
 string? message, 
        HttpStatusCode status = HttpStatusCode.BadRequest)
     : base(message ?? errorCode?.Message)
    {
   ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
     Message = message ?? errorCode.Message;
        HttpStatus = status;
}
}
```

---

## Built-in Exceptions

### 400 Bad Request

```csharp
// BadRequestException
throw new BadRequestException("Invalid input data");

// RequestInvalidException
throw new RequestInvalidException("Email format is invalid");
```

### 401 Unauthorized

```csharp
// TokenInvalidException
throw new TokenInvalidException("Token has expired");
```

### 403 Forbidden

```csharp
// ForbiddenException
throw new ForbiddenException("Access denied");

// SystemAuthorizationException
throw new SystemAuthorizationException("Insufficient permissions");
```

### 404 Not Found

```csharp
// ResourceNotFoundException
throw new ResourceNotFoundException($"Product {id} not found");
```

### 409 Conflict

```csharp
// ConflictException
throw new ConflictException("Email already exists");
```

### 429 Too Many Requests

```csharp
// TooManyRequestsException
throw new TooManyRequestsException("Rate limit exceeded");
```

### 500 Internal Server Error

```csharp
// SystemErrorException
throw new SystemErrorException("Database connection failed");

// ServerErrorException
throw new ServerErrorException("Unexpected error occurred");
```

### 503 Service Unavailable

```csharp
// ServiceUnavailableException
throw new ServiceUnavailableException("Service temporarily unavailable");
```

---

## Usage Examples

### Example 1: Service Layer

```csharp
public class ProductService
{
    public async Task<Product> GetProductById(int id)
{
    var product = await _productRepo.GetByIdAsync(id);
   
        if (product == null)
        {
          throw new ResourceNotFoundException($"Product {id} not found");
        }
        
   return product;
    }

  public async Task<Product> CreateProduct(Product product)
    {
   // Check duplicate
  var exists = await _productRepo.AnyAsync(p => p.Code == product.Code);
    if (exists)
        {
       throw new ConflictException($"Product code '{product.Code}' already exists");
  }

  _productRepo.Add(product);
  await _unitOfWork.SaveChangesAsync();
        
 return product;
    }
}
```

### Example 2: Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
  [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        // Exception will be caught by GlobalExceptionMiddleware
   var product = await _productService.GetProductById(id);
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto dto)
    {
        if (dto.Price < 0)
        {
            throw new BadRequestException("Price cannot be negative");
     }

    var product = await _productService.CreateProduct(dto);
  return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}
```

### Example 3: Validation

```csharp
public class UserService
{
    public async Task UpdatePassword(int userId, string oldPassword, string newPassword)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
        {
     throw new ResourceNotFoundException($"User {userId} not found");
        }

     if (!VerifyPassword(oldPassword, user.PasswordHash))
        {
            throw new BadRequestException("Current password is incorrect");
        }

  if (newPassword.Length < 8)
        {
throw new RequestInvalidException("Password must be at least 8 characters");
        }

        user.PasswordHash = HashPassword(newPassword);
        await _userRepo.UpdateAsync(user);
    }
}
```

---

## Response Format

### Success Response

```json
{
  "id": 1,
  "name": "iPhone 15 Pro",
  "price": 29990000
}
```

### Error Response

```json
{
  "guid": "abc-123",
  "code": "SYS005",
  "message": "Product 999 not found",
  "path": "/api/products/999",
  "method": "GET",
  "timestamp": "2025-01-28T10:00:00Z"
}
```

### Validation Error Response

```json
{
  "guid": "abc-123",
  "code": "SYS004",
  "message": "D? li?u yêu cấu không h?p l?.",
  "path": "/api/products",
  "method": "POST",
  "timestamp": "2025-01-28T10:00:00Z",
  "errors": {
    "Name": ["Tên sẽn ph?m là b?t bu?c"],
    "Price": ["Giá ph?i l?n h?n 0"]
  }
}
```

---

## Exception vs HTTP Status

| Exception | HTTP Status | Error Code |
|-----------|-------------|------------|
| `BadRequestException` | 400 | SYS010 |
| `RequestInvalidException` | 400 | SYS004 |
| `TokenInvalidException` | 401 | SYS007 |
| `ForbiddenException` | 403 | SYS003 |
| `SystemAuthorizationException` | 403 | SYS008 |
| `ResourceNotFoundException` | 404 | SYS005 |
| `ConflictException` | 409 | SYS002 |
| `TooManyRequestsException` | 429 | SYS011 |
| `SystemErrorException` | 500 | SYS001 |
| `ServerErrorException` | 500 | SYS006 |
| `ServiceUnavailableException` | 503 | SYS009 |

---

## Best Practices

### ? DO

```csharp
// ? Specific exceptions
throw new ResourceNotFoundException($"Product {id} not found");

// ? Descriptive messages
throw new ConflictException($"Email '{email}' is already registered");

// ? Use in service layer
public async Task<Product> GetProduct(int id)
{
    var product = await _repo.GetByIdAsync(id);
    if (product == null)
        throw new ResourceNotFoundException($"Product {id} not found");
    return product;
}
```

### ? DON'T

```csharp
// ? Generic exceptions
throw new Exception("Something went wrong");

// ? Catch and swallow
try { ... }
catch { /* silent fail */ }

// ? Return null instead of throwing
public async Task<Product> GetProduct(int id)
{
    var product = await _repo.GetByIdAsync(id);
    return product; // ? Returns null if not found
}
```

---

## Testing

```csharp
[Test]
public void GetProduct_NotFound_ThrowsResourceNotFoundException()
{
// Arrange
    var mockRepo = new Mock<IRepository<Product>>();
    mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product)null);
    
    var service = new ProductService(mockRepo.Object);

    // Act & Assert
    Assert.ThrowsAsync<ResourceNotFoundException>(() => 
        service.GetProductById(999));
}

[Test]
public async Task CreateProduct_DuplicateCode_ThrowsConflictException()
{
    // Arrange
    var mockRepo = new Mock<IRepository<Product>>();
    mockRepo.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>()))
     .ReturnsAsync(true);
    
    var service = new ProductService(mockRepo.Object);
    var product = new Product { Code = "IP15" };

    // Act & Assert
    Assert.ThrowsAsync<ConflictException>(() => 
 service.CreateProduct(product));
}
```

---

**[? Back to Documentation](../README.md)**
