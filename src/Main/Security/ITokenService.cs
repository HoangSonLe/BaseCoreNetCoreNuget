using System.Security.Claims;

namespace BaseNetCore.Core.src.Main.Security
{
    /// <summary>
    /// Interface for JWT token generation and validation service.
    /// </summary>
  public interface ITokenService
    {
   /// <summary>
      /// Generates a JWT token with the specified claims.
        /// </summary>
        /// <param name="claims">Collection of claims to include in the token</param>
        /// <returns>JWT token string</returns>
  string GenerateToken(IEnumerable<Claim> claims);

        /// <summary>
        /// Generates a JWT token for a user with specified user ID and optional roles.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="username">Username</param>
        /// <param name="roles">Optional collection of user roles</param>
/// <param name="additionalClaims">Optional additional claims</param>
        /// <returns>JWT token string</returns>
        string GenerateToken(string userId, string username, IEnumerable<string>? roles = null, Dictionary<string, string>? additionalClaims = null);

   /// <summary>
        /// Validates a JWT token and returns the claims principal.
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
    ClaimsPrincipal? ValidateToken(string token);

  /// <summary>
        /// Extracts user ID from a JWT token.
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID if found, null otherwise</returns>
        string? GetUserIdFromToken(string token);

        /// <summary>
     /// Generates a refresh token.
        /// </summary>
    /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();
    }
}
