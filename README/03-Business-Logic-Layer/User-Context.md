# ?? User Context

## Gi?i thi?u

**User Context** t? ??ng extract user information t? JWT token trong HTTP request.

---

## Available Claims

```csharp
// ClaimTypes.NameIdentifier or "sub"
var userId = CurrentUserId;  // int

// ClaimTypes.Name or "name"
var username = CurrentUsername;  // string?

// ClaimTypes.Role
var roles = CurrentUserRoles;  // IEnumerable<string>

// ClaimTypes.Email
var email = CurrentUser?.FindFirst(ClaimTypes.Email)?.Value;

// Custom claims
var customClaim = CurrentUser?.FindFirst("custom_claim")?.Value;
```

---

## JWT Token Structure

```json
{
  "sub": "123",
  "name": "john.doe",
  "email": "john@example.com",
  "role": ["User", "Admin"],
  "custom_claim": "custom_value",
  "exp": 1234567890,
  "iss": "MyApp",
  "aud": "MyAppUsers"
}
```

Mapping:
- `sub` ? `CurrentUserId`
- `name` ? `CurrentUsername`
- `role` ? `CurrentUserRoles`

---

## Usage in Controllers

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
 {
  // Access via HttpContext
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var profile = await _userService.GetUserProfile(userId);
  return Ok(profile);
    }
}
```

---

## Usage in Services

```csharp
public class UserService : BaseService<User>
{
    public async Task<User> GetMyProfile()
    {
        EnsureAuthenticated();
   
 // CurrentUserId auto-extracted from JWT
        return await Repository.GetByIdAsync(CurrentUserId);
    }

    public async Task UpdateMyProfile(UpdateProfileDto dto)
    {
        EnsureAuthenticated();

        var user = await Repository.GetByIdAsync(CurrentUserId);
   user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;

        Repository.Update(user);
        await UnitOfWork.SaveChangesAsync();
    }
}
```

---

## Custom Claims

### Add Custom Claims when Generating Token

```csharp
public class TokenService : ITokenService
{
    public string GenerateAccessToken(User user)
    {
    var claims = new List<Claim>
        {
 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
     new Claim(ClaimTypes.Email, user.Email),
         
       // Custom claims
            new Claim("tenant_id", user.TenantId.ToString()),
   new Claim("department", user.Department),
     new Claim("is_premium", user.IsPremiumUser.ToString())
 };

  foreach (var role in user.Roles)
        {
claims.Add(new Claim(ClaimTypes.Role, role));
        }

     // Generate token...
    }
}
```

### Access Custom Claims

```csharp
public class BaseService<TEntity>
{
    protected int TenantId
    {
        get
        {
        var tenantId = CurrentUser?.FindFirst("tenant_id")?.Value;
   return int.TryParse(tenantId, out var id) ? id : 0;
        }
  }

    protected string Department => CurrentUser?.FindFirst("department")?.Value ?? "Unknown";

    protected bool IsPremiumUser
    {
        get
        {
   var isPremium = CurrentUser?.FindFirst("is_premium")?.Value;
            return bool.TryParse(isPremium, out var result) && result;
 }
    }
}
```

---

## Multi-tenancy Support

```csharp
public class TenantAwareService<TEntity> : BaseService<TEntity>
    where TEntity : BaseAuditableEntity, ITenantEntity
{
    protected int TenantId
    {
  get
        {
         var tenantId = CurrentUser?.FindFirst("tenant_id")?.Value;
return int.TryParse(tenantId, out var id) ? id : throw new ForbiddenException("No tenant");
        }
    }

    public async Task<List<TEntity>> GetMyTenantData()
    {
 var spec = new BaseSpecification<TEntity>()
  .WithCriteria(e => e.TenantId == TenantId);

 return await Repository.GetAsync(spec);
    }
}
```

---

**[? Back to Documentation](../README.md)**
