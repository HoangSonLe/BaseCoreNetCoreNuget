using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Linq;

namespace BaseNetCore.Core.src.Main.Security.Token
{
    /// <summary>
    /// Service for generating and validating JWT tokens using RSA algorithm.
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

        /// <summary>
        /// Create token and ensure jti and sid claims exist (generate them when missing).
        /// </summary>
        private string GenerateToken(IEnumerable<Claim> claims, int? expireTimeS)
        {
            // normalize claims
            var claimList = claims?.ToList() ?? new List<Claim>();

            // ensure JTI
            if (!claimList.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
            {
                claimList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            }

            // ensure SID (session id)
            const string SidClaim = "sid";
            if (!claimList.Any(c => c.Type == SidClaim))
            {
                claimList.Add(new Claim(SidClaim, Guid.NewGuid().ToString()));
            }

            var rsa = RSA.Create();
            rsa.ImportFromPem(_tokenSettings.RsaPrivateKey);

            var key = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var expireTime = expireTimeS != null
                   ? DateTime.UtcNow.AddSeconds(expireTimeS.Value)
                 : DateTime.UtcNow.AddHours(24); // Default 24 hours

            var token = new JwtSecurityToken(
                                issuer: _tokenSettings.Issuer,
                                audience: _tokenSettings.Audience,
                                claims: claimList,
                                expires: expireTime,
                                signingCredentials: credentials
                           );

            return _tokenHandler.WriteToken(token);
        }

        /// <inheritdoc/>
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            return GenerateToken(claims, int.TryParse(_tokenSettings.AccessExpireTimeS, out var seconds) ? seconds : (int?)null);
        }

        public string GenerateRefreshToken(IEnumerable<Claim> claims)
        {
            return GenerateToken(claims, int.TryParse(_tokenSettings.RefreshExpireTimeS, out var seconds) ? seconds : (int?)null);
        }

        /// <inheritdoc/>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(_tokenSettings.RsaPublicKey);

                var key = new RsaSecurityKey(rsa);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = !string.IsNullOrEmpty(_tokenSettings.Issuer),
                    ValidIssuer = _tokenSettings.Issuer,
                    ValidateAudience = !string.IsNullOrEmpty(_tokenSettings.Audience),
                    ValidAudience = _tokenSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken &&
                      jwtToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
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

        public bool IsTokenValid(string token)
        {
            return ValidateToken(token) != null;
        }

        public string? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        /// <summary>
        /// Extract JTI from a validated token, or null when invalid/missing.
        /// </summary>
        public string? GetJtiFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        }

        /// <summary>
        /// Extract SID (session id) from a validated token, or null when invalid/missing.
        /// </summary>
        public string? GetSidFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst("sid")?.Value;
        }
    }
}
