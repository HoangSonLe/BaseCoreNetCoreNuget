# ?? Token Service

## Interface

```csharp
public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
```

---

## Implementation

```csharp
public class TokenService : ITokenService
{
    private readonly TokenSettings _settings;
    private readonly RSA _rsa;

    public TokenService(IOptions<TokenSettings> settings)
    {
        _settings = settings.Value;
_rsa = RSA.Create();
        _rsa.ImportFromPem(_settings.RsaPrivateKey);
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
   var key = new RsaSecurityKey(_rsa);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
     issuer: _settings.Issuer,
       audience: _settings.Audience,
      claims: claims,
            expires: DateTime.UtcNow.AddSeconds(int.Parse(_settings.AccessExpireTimeS)),
 signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
   var randomBytes = new byte[32];
   RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

  public ClaimsPrincipal? ValidateToken(string token)
{
        var rsa = RSA.Create();
 rsa.ImportFromPem(_settings.RsaPublicKey);

        var validationParameters = new TokenValidationParameters
    {
     ValidateIssuerSigningKey = true,
 IssuerSigningKey = new RsaSecurityKey(rsa),
            ValidateIssuer = true,
    ValidIssuer = _settings.Issuer,
         ValidateAudience = true,
       ValidAudience = _settings.Audience,
     ValidateLifetime = false  // Check manually
        };

     try
        {
   var handler = new JwtSecurityTokenHandler();
     return handler.ValidateToken(token, validationParameters, out _);
 }
   catch
        {
     return null;
   }
    }
}
```

---

## Usage

```csharp
public class AuthController : ControllerBase
{
  private readonly ITokenService _tokenService;

    [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
   var user = await ValidateUser(request);

        var claims = new[]
 {
   new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
       new Claim(ClaimTypes.Name, user.Username),
   new Claim(ClaimTypes.Role, "User")
        };

        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

     return Ok(new { accessToken, refreshToken });
  }
}
```

---

**[? Back to Documentation](../README.md)**
