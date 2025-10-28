using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BaseNetCore.Core.src.Main.Security
{
    /// <summary>
    /// Service for generating and validating JWT tokens.
    /// </summary>
    public class TokenService : ITokenService
{
  private readonly TokenSettings _tokenSettings;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public TokenService(IOptions<TokenSettings> tokenSettings)
     {
 _tokenSettings = tokenSettings.Value;
  _tokenHandler = new JwtSecurityTokenHandler();
        }

    /// <inheritdoc/>
  public string GenerateToken(IEnumerable<Claim> claims)
        {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var expireTime = int.TryParse(_tokenSettings.ExpireTimeS, out var seconds)
           ? DateTime.UtcNow.AddSeconds(seconds)
     : DateTime.UtcNow.AddHours(24); // Default 24 hours

          var token = new JwtSecurityToken(
   claims: claims,
         expires: expireTime,
          signingCredentials: credentials
          );

          return _tokenHandler.WriteToken(token);
        }

        /// <inheritdoc/>
    public string GenerateToken(string userId, string username, IEnumerable<string>? roles = null, Dictionary<string, string>? additionalClaims = null)
        {
         var claims = new List<Claim>
   {
           new Claim(ClaimTypes.NameIdentifier, userId),
new Claim(ClaimTypes.Name, username),
        new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

  // Add roles
        if (roles != null)
{
           claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
 }

   // Add additional claims
  if (additionalClaims != null)
          {
 claims.AddRange(additionalClaims.Select(kvp => new Claim(kvp.Key, kvp.Value)));
     }

      return GenerateToken(claims);
        }

/// <inheritdoc/>
 public ClaimsPrincipal? ValidateToken(string token)
        {
 try
   {
   var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));

                var validationParameters = new TokenValidationParameters
      {
             ValidateIssuerSigningKey = true,
     IssuerSigningKey = key,
             ValidateIssuer = false,
  ValidateAudience = false,
     ValidateLifetime = true,
      ClockSkew = TimeSpan.Zero
        };

                var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

       if (validatedToken is JwtSecurityToken jwtToken &&
       jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
   {
              return principal;
    }

             return null;
            }
       catch
        {
                return null;
            }
        }

        /// <inheritdoc/>
        public string? GetUserIdFromToken(string token)
   {
 var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
          ?? principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        /// <inheritdoc/>
  public string GenerateRefreshToken()
  {
     var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
      return Convert.ToBase64String(randomNumber);
        }
 }
}
