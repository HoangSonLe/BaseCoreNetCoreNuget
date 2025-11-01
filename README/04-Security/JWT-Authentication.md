# ?? JWT Authentication

## Mục lục
- [Giới thiệu](#giới-thiệu)
- [Configuration](#configuration)
- [Token Generation](#token-generation)
- [Token Validation](#token-validation)
- [Usage Examples](#usage-examples)
- [Best Practices](#best-practices)

---

## ?? Giới thiệu

BaseNetCore.Core sẽ d?ng **JWT (JSON Web Tokens)** với **RSA signing** cho authentication.

### T?i sao RSA thay vì HMAC?

| Feature | RSA (Asymmetric) | HMAC (Symmetric) |
|---------|------------------|------------------|
| **Security** | ? Public key có th? share | ?? Secret must be kept private |
| **Microservices** | ? Services ch? c?n public key | ? All services need same secret |
| **Key Rotation** | ? Easier | ?? Complex |
| **Performance** | ?? Slower | ? Faster |

---

## ?? Configuration

### 1. Generate RSA Keys

```csharp
using BaseNetCore.Core.src.Main.Security.Algorithm;

// Console app or separate script
RsaKeyGenerator.PrintSampleConfiguration(keySizeInBits: 2048);
```

Output m?u:
```
=== Generated RSA Keys for JWT Token ===

"TokenSettings": {
  "RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\nMIIEow...\n-----END PRIVATE KEY-----",
  "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\nMIIBIj...\n-----END PUBLIC KEY-----",
  "AccessExpireTimeS": "3600",
  "RefreshExpireTimeS": "86400",
  "Issuer": "MyApp",
  "Audience": "MyAppUsers"
}
```

### 2. appsettings.json

```json
{
  "TokenSettings": {
"RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
    "AccessExpireTimeS": "900",
  "RefreshExpireTimeS": "86400",
    "Issuer": "MyApp",
    "Audience": "MyAppUsers"
  }
}
```

**?? IMPORTANT:**
- **Private Key:** NEVER commit to Git! Use Azure Key Vault / User Secrets
- **Public Key:** Safe to share (used for validation only)
- **AccessExpireTimeS:** 15-60 minutes (production: 15-30 mins)
- **RefreshExpireTimeS:** 7-30 days

### 3. Program.cs Setup

```csharp
using BaseNetCore.Core.src.Main.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Or with full BaseNetCore features
builder.Services.AddBaseNetCoreFeaturesWithAuth(builder.Configuration);

var app = builder.Build();

// Use authentication middleware
app.UseAuthentication();
app.UseAuthorization();
// Or
app.UseBaseNetCoreMiddlewareWithAuth();

app.MapControllers();
app.Run();
```

---

## ?? Token Generation

### ITokenService Interface

```csharp
public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId);
}
```

### Usage Example

```csharp
public class AuthService
{
  private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepo;

    public async Task<LoginResponse> Login(LoginRequest request)
 {
   // 1. Validate credentials
        var user = await _userRepo.FindByEmailAsync(request.Email);
      if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new TokenInvalidException("Invalid credentials");
        }

        // 2. Create claims
        var claims = new List<Claim>
 {
   new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
new Claim(ClaimTypes.Email, user.Email),
  new Claim("tenant_id", user.TenantId.ToString())
 };

        // Add roles
     foreach (var role in user.Roles)
        {
   claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 3. Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

   // 4. Save refresh token to database
     user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
await _userRepo.UpdateAsync(user);

        return new LoginResponse
     {
      AccessToken = accessToken,
     RefreshToken = refreshToken,
 ExpiresIn = 3600
};
    }
}
```

---

## ? Token Validation

### Automatic Validation

BaseNetCore t? ??ng validate tokens trong `JwtBearerEvents`:

```csharp
// Program.cs - Already configured by AddJwtAuthentication()
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
        options.TokenValidationParameters = new TokenValidationParameters
        {
       ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaPublicKey,
    ValidateIssuer = true,
  ValidIssuer = "MyApp",
ValidateAudience = true,
            ValidAudience = "MyAppUsers",
            ValidateLifetime = true,
  ClockSkew = TimeSpan.Zero
        };
    });
```

### Custom Validation (Database Check)

Implement `ITokenValidator` ?? check token trong database:

```csharp
public class DatabaseTokenValidator : ITokenValidator
{
    private readonly IUserRepository _userRepo;

    public DatabaseTokenValidator(IUserRepository userRepo)
    {
   _userRepo = userRepo;
    }

    public async Task<bool> ValidateAsync(ClaimsPrincipal principal, string rawToken, HttpContext httpContext)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
   if (!int.TryParse(userIdClaim, out var userId))
        {
      return false;
        }

    // Check user exists and is active
    var user = await _userRepo.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
{
            return false;
        }

  // Check token not blacklisted
        var isBlacklisted = await _userRepo.IsTokenBlacklistedAsync(rawToken);
        if (isBlacklisted)
        {
return false;
      }

     return true;
  }
}

// Register in Program.cs
builder.Services.AddScoped<ITokenValidator, DatabaseTokenValidator>();
```

### Manual Validation

```csharp
public class TokenHelper
{
    private readonly ITokenService _tokenService;

    public bool IsTokenValid(string token)
    {
        try
      {
       var principal = _tokenService.ValidateToken(token);
     return principal != null;
}
 catch
        {
       return false;
  }
    }

    public int? GetUserIdFromToken(string token)
    {
  var principal = _tokenService.ValidateToken(token);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
   return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
```

---

## ?? Usage Examples

### Protect Controller Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Public endpoint - no auth required
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
   // ...
    }

    // Protected endpoint - requires valid JWT
 [HttpPost]
    [Authorize]
 public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
    // User is authenticated here
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // ...
    }

    // Role-based authorization
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
      // Only Admin role can access
     // ...
    }
}
```

### Refresh Token Flow

```csharp
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
    // 1. Validate access token (ignore expiry)
    var principal = _tokenService.ValidateToken(request.AccessToken);
    if (principal == null)
    {
      return Unauthorized("Invalid access token");
    }

    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // 2. Validate refresh token from database
var isValid = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken, userId);
    if (!isValid)
    {
  return Unauthorized("Invalid refresh token");
    }

    // 3. Generate new tokens
 var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
    var newRefreshToken = _tokenService.GenerateRefreshToken();

    // 4. Update refresh token in database
    await _userRepo.UpdateRefreshTokenAsync(int.Parse(userId), newRefreshToken);

    return Ok(new
    {
   AccessToken = newAccessToken,
   RefreshToken = newRefreshToken
    });
}
```

### Logout (Token Blacklist)

```csharp
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout()
{
 var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
 var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    // Blacklist current token
 await _userRepo.BlacklistTokenAsync(token, userId);

    // Clear refresh token
    await _userRepo.ClearRefreshTokenAsync(userId);

    return Ok("Logged out successfully");
}
```

---

## ?? Best Practices

### 1. Short-lived Access Tokens

```json
{
  "TokenSettings": {
    "AccessExpireTimeS": "900",  // 15 minutes (production)
    "RefreshExpireTimeS": "604800"  // 7 days
  }
}
```

### 2. Use HttpOnly Cookies cho Refresh Tokens

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var loginResponse = await _authService.Login(request);

    // Store refresh token in HttpOnly cookie
  Response.Cookies.Append("refreshToken", loginResponse.RefreshToken, new CookieOptions
  {
     HttpOnly = true,
  Secure = true,  // HTTPS only
 SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(30)
    });

    return Ok(new
 {
        AccessToken = loginResponse.AccessToken,
        ExpiresIn = loginResponse.ExpiresIn
    });
}
```

### 3. Rotate Keys Periodically

```bash
# Generate new keys quarterly
dotnet run --project KeyRotation.Console

# Update appsettings.json with new keys
# Deploy with zero-downtime strategy
```

### 4. Validate Tokens in Database

```csharp
// Check user status, token blacklist, etc.
builder.Services.AddScoped<ITokenValidator, DatabaseTokenValidator>();
```

### 5. Use HTTPS Only in Production

```csharp
// Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

---

## ?? Troubleshooting

### ? "Token signature is invalid"

**Nguyên nhân:** Public key không kh?p với private key dùng ?? sign.

**Gi?i pháp:**
```bash
# Re-generate keys và update c? private + public keys
dotnet run --project GenerateKeys
```

### ? "Token has expired"

**Nguyên nhân:** Access token ?ã h?t h?n.

**Gi?i pháp:** Implement refresh token flow.

### ? "Audience validation failed"

**Nguyên nhân:** Token `aud` claim không kh?p với config.

**Gi?i pháp:**
```json
{
  "TokenSettings": {
    "Audience": "MyAppUsers"  // Must match token generation
  }
}
```

---

## ?? Related Topics

- [Token Service](Token-Service.md)
- [RSA Key Generation](RSA-Key-Generation.md)
- [Security Best Practices](../12-Best-Practices/Security-Best-Practices.md)

---

**[? Back to Documentation](../README.md)**
