# ?? Authentication Example

## Complete JWT Authentication Flow

---

## 1. User Entity

```csharp
public class User : BaseAuditableEntity
{
    public int Id { get; set; }
  public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
 public string RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<UserRole> UserRoles { get; set; }
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public User User { get; set; }
    public Role Role { get; set; }
}
```

---

## 2. DTOs

```csharp
public class LoginRequest
{
    [Required] [EmailAddress]
    public string Email { get; set; }

    [Required] [MinLength(6)]
    public string Password { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public UserDto User { get; set; }
}

public class RefreshTokenRequest
{
[Required]
public string RefreshToken { get; set; }
}

public class RegisterRequest
{
    [Required] [MaxLength(50)]
    public string Username { get; set; }

    [Required] [EmailAddress]
    public string Email { get; set; }

    [Required] [MinLength(8)]
    public string Password { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
}
```

---

## 3. Auth Service

```csharp
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
  Task<UserDto> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
   var userRepo = _unitOfWork.Repository<User>();
     
  var user = await userRepo.FindAsync(u => u.Email == request.Email);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
{
            throw new TokenInvalidException("Invalid email or password");
   }

    if (!user.IsActive)
        {
    throw new ForbiddenException("Account is disabled");
        }

        // Get roles
  var roles = await GetUserRolesAsync(user.Id);

        // Generate tokens
        var claims = new List<Claim>
   {
       new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Email, user.Email)
 };

        foreach (var role in roles)
        {
     claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var accessToken = _tokenService.GenerateAccessToken(claims);
 var refreshToken = _tokenService.GenerateRefreshToken();

   // Save refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
     _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponse
        {
     AccessToken = accessToken,
    RefreshToken = refreshToken,
            ExpiresIn = 3600,
   User = new UserDto
        {
                Id = user.Id,
 Username = user.Username,
          Email = user.Email,
       Roles = roles
            }
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
 var userRepo = _unitOfWork.Repository<User>();
        
 var user = await userRepo.FindAsync(u => 
  u.RefreshToken == refreshToken && 
    u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
          throw new TokenInvalidException("Invalid or expired refresh token");
     }

    // Get roles
        var roles = await GetUserRolesAsync(user.Id);

    // Generate new tokens
        var claims = new List<Claim>
        {
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
     new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        foreach (var role in roles)
     {
   claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var accessToken = _tokenService.GenerateAccessToken(claims);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

      user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
        ExpiresIn = 3600,
            User = new UserDto
 {
         Id = user.Id,
       Username = user.Username,
        Email = user.Email,
       Roles = roles
            }
     };
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
   var userRepo = _unitOfWork.Repository<User>();
        
      // Check duplicate email
        var exists = await userRepo.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
    throw new ConflictException("Email already registered");
        }

      var user = new User
        {
  Username = request.Username,
         Email = request.Email,
          PasswordHash = HashPassword(request.Password),
            IsActive = true
        };

        userRepo.Add(user);
        await _unitOfWork.SaveChangesAsync();

return new UserDto
        {
          Id = user.Id,
Username = user.Username,
Email = user.Email,
   Roles = new List<string> { "User" }
        };
    }

    public async Task LogoutAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userId, out var id))
        {
     var userRepo = _unitOfWork.Repository<User>();
    var user = await userRepo.GetByIdAsync(id);
            
            if (user != null)
  {
         user.RefreshToken = null;
user.RefreshTokenExpiry = null;
   userRepo.Update(user);
     await _unitOfWork.SaveChangesAsync();
       }
        }
    }

 private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
}

    private async Task<List<string>> GetUserRolesAsync(int userId)
    {
        // Implementation depends on your data model
      return new List<string> { "User" };
    }
}
```

---

## 4. Auth Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
      var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetProfile), user);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
    await _authService.LogoutAsync();
return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Get user profile...
        return Ok();
    }
}
```

---

## 5. Test Requests

### POST /api/auth/login

```http
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response: 200**
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "Uy8zZjM2YjY5LTBkNDgtNGQ4Ny1hOWM0LTZjNzE3NjY2YzM5Mw==",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "username": "john.doe",
    "email": "user@example.com",
    "roles": ["User"]
  }
}
```

### POST /api/auth/refresh

```http
POST /api/auth/refresh
{
  "refreshToken": "Uy8zZjM2YjY5LTBkNDgtNGQ4Ny1hOWM0LTZjNzE3NjY2YzM5Mw=="
}
```

---

**[? Back to Documentation](../README.md)**
