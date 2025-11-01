# ? Dynamic Permissions

## Gi?i thi?u

**Dynamic Permissions** cho phép c?u hình authorization rules t? **appsettings.json** thay vì hard-code trong code.

---

## ?u ?i?m

- ? **Configuration-based** - Không c?n rebuild khi thay ??i permissions
- ? **Centralized** - T?t c? rules ? m?t ch?
- ? **Flexible** - Regex pattern matching, wildcards
- ? **Runtime validation** - Check permissions ??ng
- ? **Role-based + Permission-based** - Hybrid approach

---

## Configuration

### appsettings.json

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
        "/api/products:GET",
        "/api/products/{REGEX}:GET",
        "/api/products:POST:@product.create",
        "/api/products/{REGEX}:PUT:@product.update",
     "/api/products/{REGEX}:DELETE:@product.delete"
      ],
      "OrderService": [
   "/api/orders:GET:@order.view",
        "/api/orders/{REGEX}:GET:@order.view",
        "/api/orders:POST:@order.create",
   "/api/orders/{REGEX}/cancel:POST:@order.cancel"
      ],
      "AdminService": [
        "/api/admin/*:@admin.access"
      ]
    }
  }
}
```

---

## Rule Syntax

```
/api/path:HTTP_METHOD:@permission.code
```

### Components

| Component | Description | Example |
|-----------|-------------|---------|
| **Path** | URL path | `/api/products` |
| **Method** | HTTP method (default: GET) | `POST`, `PUT`, `DELETE` |
| **Permission** | Required permission (optional) | `@product.create` |

### Path Patterns

```json
{
  "Permissions": {
    "Examples": [
      // Exact match
      "/api/products:GET",
      
      // Wildcard (single segment)
      "/api/products/*:GET",
   
// Regex placeholder
      "/api/products/{REGEX}:GET",
      
      // Wildcard (all segments)
 "/api/admin/*:@admin.access"
    ]
  }
}
```

---

## Setup

### 1. Register Provider

```csharp
// Program.cs
using BaseNetCore.Core.src.Main.Security.Permission;

builder.Services.AddSingleton<IDynamicPermissionProvider, DefaultDynamicPermissionProvider>();
```

### 2. Add Middleware

```csharp
var app = builder.Build();

// After UseRouting, before UseEndpoints
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Add Dynamic Permission Middleware
app.UseMiddleware<DynamicPermissionMiddleware>();

app.MapControllers();
```

### 3. Implement User Permission Service

```csharp
public interface IUserPermissionService
{
    Task<IReadOnlyList<string>> GetPermissionsAsync(string userId);
}

public class UserPermissionService : IUserPermissionService
{
    private readonly IUserRepository _userRepo;

    public UserPermissionService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
}

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(string userId)
    {
    var user = await _userRepo.GetByIdAsync(int.Parse(userId));
        if (user == null)
            return Array.Empty<string>();

        // Get permissions from roles
        var permissions = new List<string>();
        foreach (var role in user.Roles)
        {
            var rolePermissions = await _userRepo.GetRolePermissionsAsync(role);
    permissions.AddRange(rolePermissions);
        }

        return permissions;
    }
}

// Register
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
```

---

## Usage Examples

### Example 1: Basic CRUD Permissions

```json
{
  "DynamicPermissions": {
    "PermitAll": ["/api/health"],
    "Permissions": {
      "ProductService": [
        "/api/products:GET",        // Public list
   "/api/products/{REGEX}:GET",         // Public detail
  "/api/products:POST:@product.create",         // Requires permission
    "/api/products/{REGEX}:PUT:@product.update",
        "/api/products/{REGEX}:DELETE:@product.delete"
      ]
    }
  }
}
```

### Example 2: Admin-only Endpoints

```json
{
  "Permissions": {
    "AdminService": [
      "/api/admin/*:@admin.access",        // All admin endpoints
      "/api/admin/users:GET:@admin.user.view",
      "/api/admin/users:POST:@admin.user.create",
   "/api/admin/settings/*:@admin.settings.manage"
    ]
  }
}
```

### Example 3: Multi-tenant Permissions

```json
{
  "Permissions": {
    "TenantService": [
      "/api/tenant/{REGEX}/products:GET:@tenant.product.view",
  "/api/tenant/{REGEX}/products:POST:@tenant.product.create",
      "/api/tenant/{REGEX}/users:GET:@tenant.user.view"
    ]
  }
}
```

---

## Database Schema

### Permission Tables

```sql
-- Permissions
CREATE TABLE Permissions (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(100) UNIQUE NOT NULL,  -- e.g., 'product.create'
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500)
);

-- Roles
CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(500)
);

-- RolePermissions (Many-to-Many)
CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id)
);

-- UserRoles (Many-to-Many)
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
```

### Seed Data

```csharp
// Permissions
INSERT INTO Permissions (Code, Name, Description) VALUES
('product.create', 'Create Product', 'Can create new products'),
('product.update', 'Update Product', 'Can update existing products'),
('product.delete', 'Delete Product', 'Can delete products'),
('product.view', 'View Product', 'Can view product details'),
('order.create', 'Create Order', 'Can create orders'),
('order.cancel', 'Cancel Order', 'Can cancel orders'),
('admin.access', 'Admin Access', 'Full admin access');

// Roles
INSERT INTO Roles (Name, Description) VALUES
('Admin', 'Full system access'),
('Manager', 'Product and order management'),
('User', 'Basic user access');

// RolePermissions
-- Admin has all permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 1, Id FROM Permissions;

-- Manager has product and order permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 2, Id FROM Permissions WHERE Code LIKE 'product.%' OR Code LIKE 'order.%';

-- User has view permissions only
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 3, Id FROM Permissions WHERE Code IN ('product.view', 'order.create');
```

---

## Controller Usage

### With [AllowAnonymous]

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Public endpoint - bypasses permission check
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        // Anyone can access
    }

    // Protected by dynamic permissions
  [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
      // Requires 'product.create' permission (from appsettings.json)
    }
}
```

---

## Custom Permission Provider

```csharp
public class DatabasePermissionProvider : IDynamicPermissionProvider
{
    private readonly IPermissionRepository _permissionRepo;

    public DatabasePermissionProvider(IPermissionRepository permissionRepo)
    {
_permissionRepo = permissionRepo;
    }

public async Task<IReadOnlyList<string>> GetPermitAllAsync()
    {
        return await _permissionRepo.GetPublicEndpointsAsync();
    }

    public async Task<IReadOnlyList<DynamicPermissionRule>> GetRulesAsync()
    {
 var rules = await _permissionRepo.GetAllRulesAsync();
        
        return rules.Select(r => new DynamicPermissionRule
        {
         RawPathPattern = r.PathPattern,
 PathRegex = new Regex(r.PathPattern, RegexOptions.IgnoreCase),
   HttpMethod = r.HttpMethod,
  RequiredPermissions = r.RequiredPermissions.ToArray()
    }).ToList();
}
}

// Register
builder.Services.AddSingleton<IDynamicPermissionProvider, DatabasePermissionProvider>();
```

---

## Testing

```csharp
[Test]
public async Task CreateProduct_WithoutPermission_Returns403()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = GenerateTokenWithoutPermission("product.create");
    client.DefaultRequestHeaders.Authorization = new("Bearer", token);

    // Act
    var response = await client.PostAsJsonAsync("/api/products", new Product());

    // Assert
    Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
}

[Test]
public async Task CreateProduct_WithPermission_Returns201()
{
    // Arrange
  var client = _factory.CreateClient();
    var token = GenerateTokenWithPermission("product.create");
    client.DefaultRequestHeaders.Authorization = new("Bearer", token);

    // Act
    var response = await client.PostAsJsonAsync("/api/products", new Product());

    // Assert
    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
}
```

---

## Troubleshooting

### ? "Authorization failed" for public endpoints

**Gi?i pháp:** Add to `PermitAll`:

```json
{
  "DynamicPermissions": {
    "PermitAll": [
  "/api/products",
      "/api/products/*"
    ]
  }
}
```

### ? Regex not matching

**Gi?i pháp:** Check pattern escaping:

```json
{
  "Permissions": {
    "ProductService": [
      "/api/products/{REGEX}:GET"  // ? Matches /api/products/123
    ]
  }
}
```

---

**[? Back to Documentation](../README.md)**
