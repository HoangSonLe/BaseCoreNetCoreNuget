# ??? Custom Error Codes

## Gi?i thi?u

BaseNetCore.Core cho ph�p ??nh ngh?a **custom error codes** cho t?ng domain/module c?a application.

---

## IErrorCode Interface

```csharp
public interface IErrorCode
{
    string Code { get; }
    string Message { get; }
}
```

---

## Define Custom Error Codes

### Approach 1: Sealed Class (Recommended)

```csharp
public sealed class ProductErrorCodes : IErrorCode
{
    public string Code { get; }
    public string Message { get; }

    private ProductErrorCodes(string code, string message)
    {
   Code = code;
    Message = message;
    }

    // Define error codes
    public static readonly ProductErrorCodes PRODUCT_NOT_FOUND =
 new("PRD001", "S?n ph?m kh�ng t?n t?i");

    public static readonly ProductErrorCodes PRODUCT_OUT_OF_STOCK =
  new("PRD002", "S?n ph?m h?t h�ng");

    public static readonly ProductErrorCodes PRODUCT_PRICE_INVALID =
        new("PRD003", "Gi� s?n ph?m kh�ng h?p l?");

    public static readonly ProductErrorCodes PRODUCT_CODE_DUPLICATE =
        new("PRD004", "M� s?n ph?m ?� t?n t?i");

    public static readonly ProductErrorCodes PRODUCT_INACTIVE =
  new("PRD005", "S?n ph?m kh�ng ho?t ??ng");
}
```

### Approach 2: Enum + Extension

```csharp
public enum EProductErrorCode
{
    PRODUCT_NOT_FOUND,
    PRODUCT_OUT_OF_STOCK,
    PRODUCT_PRICE_INVALID
}

public class ProductErrorCodes : IErrorCode
{
    public string Code { get; }
    public string Message { get; }

    private static readonly Dictionary<EProductErrorCode, (string Code, string Message)> _map = new()
    {
        { EProductErrorCode.PRODUCT_NOT_FOUND, ("PRD001", "S?n ph?m kh�ng t?n t?i") },
   { EProductErrorCode.PRODUCT_OUT_OF_STOCK, ("PRD002", "S?n ph?m h?t h�ng") },
        { EProductErrorCode.PRODUCT_PRICE_INVALID, ("PRD003", "Gi� kh�ng h?p l?") }
    };

    private ProductErrorCodes(EProductErrorCode key)
    {
        var (code, message) = _map[key];
        Code = code;
  Message = message;
    }

    public static ProductErrorCodes FromKey(EProductErrorCode key) => new(key);
}
```

---

## Custom Exceptions

```csharp
public class ProductNotFoundException : BaseApplicationException
{
    public ProductNotFoundException()
   : base(ProductErrorCodes.PRODUCT_NOT_FOUND, 
         ProductErrorCodes.PRODUCT_NOT_FOUND.Message, 
               HttpStatusCode.NotFound)
    {
    }

  public ProductNotFoundException(string message)
   : base(ProductErrorCodes.PRODUCT_NOT_FOUND, message, HttpStatusCode.NotFound)
    {
    }

    public ProductNotFoundException(int productId)
        : base(ProductErrorCodes.PRODUCT_NOT_FOUND, 
     $"S?n ph?m ID {productId} kh�ng t?n t?i", 
       HttpStatusCode.NotFound)
    {
    }
}

public class ProductOutOfStockException : BaseApplicationException
{
    public ProductOutOfStockException(string productName, int requestedQty, int availableQty)
        : base(ProductErrorCodes.PRODUCT_OUT_OF_STOCK,
 $"S?n ph?m '{productName}' ch? c�n {availableQty} (y�u c?u {requestedQty})",
       HttpStatusCode.Conflict)
    {
    }
}
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
  throw new ProductNotFoundException(id);
        }

        if (!product.IsActive)
        {
       throw new BaseApplicationException(
     ProductErrorCodes.PRODUCT_INACTIVE,
     $"S?n ph?m '{product.Name}' kh�ng c�n ho?t ??ng",
   HttpStatusCode.BadRequest);
        }

  return product;
    }

    public async Task<Order> CreateOrder(CreateOrderDto dto)
    {
  foreach (var item in dto.Items)
  {
   var product = await _productRepo.GetByIdAsync(item.ProductId);
            
     if (product.Stock < item.Quantity)
      {
     throw new ProductOutOfStockException(
          product.Name, 
    item.Quantity, 
    product.Stock);
}
        }

        // Create order...
    }
}
```

### Example 2: Validation

```csharp
public class ProductValidator
{
    public void ValidatePrice(decimal price)
    {
        if (price <= 0)
      {
       throw new BaseApplicationException(
          ProductErrorCodes.PRODUCT_PRICE_INVALID,
  "Gi� s?n ph?m ph?i l?n h?n 0",
       HttpStatusCode.BadRequest);
        }

      if (price > 1000000000)
        {
    throw new BaseApplicationException(
ProductErrorCodes.PRODUCT_PRICE_INVALID,
     "Gi� s?n ph?m kh�ng ???c v??t qu� 1 t?",
         HttpStatusCode.BadRequest);
  }
    }
}
```

---

## Module-specific Error Codes

### Order Module

```csharp
public sealed class OrderErrorCodes : IErrorCode
{
    public string Code { get; }
    public string Message { get; }

 private OrderErrorCodes(string code, string message)
    {
   Code = code;
      Message = message;
  }

    public static readonly OrderErrorCodes ORDER_NOT_FOUND =
 new("ORD001", "??n h�ng kh�ng t?n t?i");

    public static readonly OrderErrorCodes ORDER_ALREADY_CANCELLED =
    new("ORD002", "??n h�ng ?� b? h?y");

    public static readonly OrderErrorCodes ORDER_CANNOT_CANCEL =
  new("ORD003", "Kh�ng th? h?y ??n h�ng ?ang giao");

    public static readonly OrderErrorCodes ORDER_PAYMENT_FAILED =
   new("ORD004", "Thanh to�n th?t b?i");
}
```

### User Module

```csharp
public sealed class UserErrorCodes : IErrorCode
{
    public string Code { get; }
    public string Message { get; }

    private UserErrorCodes(string code, string message)
    {
    Code = code;
        Message = message;
    }

    public static readonly UserErrorCodes USER_NOT_FOUND =
        new("USR001", "Ng??i d�ng kh�ng t?n t?i");

    public static readonly UserErrorCodes USER_EMAIL_EXISTS =
        new("USR002", "Email ?� ???c s? d?ng");

    public static readonly UserErrorCodes USER_INACTIVE =
 new("USR003", "T�i kho?n ?� b? v� hi?u h�a");

    public static readonly UserErrorCodes USER_PASSWORD_INVALID =
        new("USR004", "M?t kh?u kh�ng ?�ng");

    public static readonly UserErrorCodes USER_LOCKED =
        new("USR005", "T�i kho?n ?� b? kh�a");
}
```

---

## Error Code Naming Convention

### Format

```
[MODULE][NUMBER]
```

| Module | Prefix | Examples |
|--------|--------|----------|
| **System** | SYS | SYS001-SYS099 |
| **Product** | PRD | PRD001-PRD099 |
| **Order** | ORD | ORD001-ORD099 |
| **User** | USR | USR001-USR099 |
| **Payment** | PAY | PAY001-PAY099 |
| **Inventory** | INV | INV001-INV099 |

---

## Response Examples

### Product Not Found (404)

```json
{
  "guid": "abc-123",
  "code": "PRD001",
  "message": "S?n ph?m ID 999 kh�ng t?n t?i",
  "path": "/api/products/999",
  "method": "GET",
  "timestamp": "2025-01-28T10:00:00Z"
}
```

### Product Out of Stock (409)

```json
{
  "guid": "abc-123",
  "code": "PRD002",
  "message": "S?n ph?m 'iPhone 15 Pro' ch? c�n 5 (y�u c?u 10)",
  "path": "/api/orders",
  "method": "POST",
  "timestamp": "2025-01-28T10:00:00Z"
}
```

### User Email Exists (409)

```json
{
  "guid": "abc-123",
  "code": "USR002",
  "message": "Email 'user@example.com' ?� ???c s? d?ng",
  "path": "/api/users",
  "method": "POST",
  "timestamp": "2025-01-28T10:00:00Z"
}
```

---

## Best Practices

### ? DO

```csharp
// ? Define all error codes in one place
public sealed class ProductErrorCodes : IErrorCode
{
    // All product-related errors here
}

// ? Use descriptive codes and messages
public static readonly ProductErrorCodes PRODUCT_OUT_OF_STOCK =
  new("PRD002", "S?n ph?m h?t h�ng");

// ? Include context in exception message
throw new ProductNotFoundException($"Product ID {id} not found");
```

### ? DON'T

```csharp
// ? Magic strings
throw new BaseApplicationException("ERROR", "Something wrong", HttpStatusCode.BadRequest);

// ? Generic error codes
throw new BaseApplicationException("E001", "Error", HttpStatusCode.BadRequest);

// ? Duplicate codes across modules
// ProductErrorCodes: PRD001
// OrderErrorCodes: PRD001  // ? Conflict!
```

---

## Localization

```csharp
public sealed class ProductErrorCodes : IErrorCode
{
    public string Code { get; }
    public string Message { get; }
    private readonly string _messageKey;

    private ProductErrorCodes(string code, string messageKey, string defaultMessage)
  {
      Code = code;
  _messageKey = messageKey;
        Message = defaultMessage;
    }

    public string GetLocalizedMessage(IStringLocalizer localizer)
    {
   return localizer[_messageKey] ?? Message;
    }

    public static readonly ProductErrorCodes PRODUCT_NOT_FOUND =
   new("PRD001", "ProductNotFound", "S?n ph?m kh�ng t?n t?i");
}
```

---

**[? Back to Documentation](../README.md)**
