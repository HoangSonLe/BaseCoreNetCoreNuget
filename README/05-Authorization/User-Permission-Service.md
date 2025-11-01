# ?? User Permission Service

## Interface

```csharp
public interface IUserPermissionService
{
    Task<IReadOnlyList<string>> GetPermissionsAsync(string userId);
}
```

---

## Implementation

```csharp
public class UserPermissionService : IUserPermissionService
{
    private readonly ApplicationDbContext _context;

  public UserPermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(string userId)
    {
    if (!int.TryParse(userId, out var id))
   return Array.Empty<string>();

 var permissions = await _context.Users
  .Where(u => u.Id == id)
          .SelectMany(u => u.Roles)
      .SelectMany(r => r.Permissions)
      .Select(p => p.Code)
   .Distinct()
 .ToListAsync();

    return permissions;
    }
}
```

---

## With Caching

```csharp
public class CachedUserPermissionService : IUserPermissionService
{
    private readonly IMemoryCache _cache;
    private readonly IUserPermissionService _inner;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public CachedUserPermissionService(
  IMemoryCache cache,
    IUserPermissionService inner)
    {
      _cache = cache;
        _inner = inner;
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(string userId)
    {
 var cacheKey = $"user_permissions:{userId}";

  if (_cache.TryGetValue(cacheKey, out IReadOnlyList<string> cached))
 {
     return cached;
      }

    var permissions = await _inner.GetPermissionsAsync(userId);
        
        _cache.Set(cacheKey, permissions, _cacheExpiration);
        
        return permissions;
    }

    public void InvalidateCache(string userId)
    {
  _cache.Remove($"user_permissions:{userId}");
    }
}
```

---

## Registration

```csharp
// Program.cs
builder.Services.AddMemoryCache();

// Without caching
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();

// With caching (Decorator pattern)
builder.Services.AddScoped<UserPermissionService>(); // Inner service
builder.Services.AddScoped<IUserPermissionService, CachedUserPermissionService>();
```

---

## Usage in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserPermissionService _permissionService;

    [HttpGet("me/permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
  var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var permissions = await _permissionService.GetPermissionsAsync(userId);
        
        return Ok(permissions);
    }
}
```

---

**[? Back to Documentation](../README.md)**
