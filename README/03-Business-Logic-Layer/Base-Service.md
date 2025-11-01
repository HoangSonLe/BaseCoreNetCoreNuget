# ??? Base Service

## Giới thiệu

**BaseService<TEntity>** cung c?p **User Context** và common functionality cho t?t c? services.

---

## Features

- ? **User Context** - T? ??ng extract t? JWT token
- ? **Repository Access** - Direct access to repository
- ? **UnitOfWork** - Transaction management
- ? **Authorization Helpers** - Check roles/permissions

---

## Implementation

```csharp
public abstract class BaseService<TEntity> : IBaseService<TEntity> 
    where TEntity : BaseAuditableEntity
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IRepository<TEntity> Repository;
    protected readonly IHttpContextAccessor HttpContextAccessor;

 protected BaseService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
  {
  UnitOfWork = unitOfWork;
     HttpContextAccessor = httpContextAccessor;
        Repository = unitOfWork.Repository<TEntity>();
    }

    // User Context Properties
    protected ClaimsPrincipal? CurrentUser => HttpContextAccessor.HttpContext?.User;

    public int CurrentUserId
 {
        get
        {
 var userIdClaim = CurrentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value
       ?? CurrentUser?.FindFirst("sub")?.Value;
       return int.TryParse(userIdClaim, out var userId) ? userId : 1;
        }
    }

    public string? CurrentUsername => CurrentUser?.FindFirst(ClaimTypes.Name)?.Value;

    public IEnumerable<string> CurrentUserRoles
  {
        get => CurrentUser?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

  public bool HasRole(string role)
    {
        return CurrentUserRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    protected void EnsureAuthenticated()
    {
  if (CurrentUserId <= 1)
        {
            throw new TokenInvalidException("User is not authenticated");
        }
    }
}
```

---

## Usage Examples

### Example 1: Basic Service với User Context

```csharp
public class ProductService : BaseService<Product>
{
    public ProductService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, httpContextAccessor)
    {
 }

    public async Task<Product> CreateProduct(Product product)
    {
      // Auto-set audit fields (CreatedBy/UpdatedBy handled by EF Core interceptor)
        // But you can access user info:
        Console.WriteLine($"Product created by: {CurrentUsername} (ID: {CurrentUserId})");

        Repository.Add(product);
        await UnitOfWork.SaveChangesAsync();
        
        return product;
    }
}
```

### Example 2: Authorization Check

```csharp
public class AdminService : BaseService<Setting>
{
    public AdminService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, httpContextAccessor)
 {
}

    public async Task UpdateSystemSetting(Setting setting)
    {
   // Check authentication
        EnsureAuthenticated();

        // Check role
        if (!HasRole("Admin"))
        {
  throw new ForbiddenException("Only admins can update system settings");
        }

     Repository.Update(setting);
        await UnitOfWork.SaveChangesAsync();
    }
}
```

### Example 3: User-specific Data

```csharp
public class OrderService : BaseService<Order>
{
    public OrderService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    : base(unitOfWork, httpContextAccessor)
    {
    }

    public async Task<List<Order>> GetMyOrders()
    {
        EnsureAuthenticated();

        var spec = new BaseSpecification<Order>()
 .WithCriteria(o => o.CustomerId == CurrentUserId)
            .WithOrderByDescending(o => o.OrderDate);

        return await Repository.GetAsync(spec);
    }
}
```

---

## User Context Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentUser` | `ClaimsPrincipal?` | Full user claims |
| `CurrentUserId` | `int` | User ID from token (default: 1) |
| `CurrentUsername` | `string?` | Username from token |
| `CurrentUserRoles` | `IEnumerable<string>` | All user roles |
| `HasRole(role)` | `bool` | Check specific role |
| `EnsureAuthenticated()` | `void` | Throw if not logged in |

---

## Registration

```csharp
// Program.cs
builder.Services.AddHttpContextAccessor();  // Required!
builder.Services.AddScoped<IProductService, ProductService>();
```

---

**[? Back to Documentation](../README.md)**
