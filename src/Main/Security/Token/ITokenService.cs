using System.Security.Claims;

namespace BaseNetCore.Core.src.Main.Security.Token
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
        string GenerateAccessToken(IEnumerable<Claim> claims);
        /// <summary>
        /// Generates a refresh token.
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken(IEnumerable<Claim> claims);

        /// <summary>
        /// Validates a JWT token and returns the claims principal.
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Checks if the provided JWT token is valid.
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>True if the token is valid, otherwise false</returns>
        bool IsTokenValid(string token);

        /// <summary>
        /// Extracts user ID from a JWT token.
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID if found, null otherwise</returns>
        string? GetUserIdFromToken(string token);


        /// <summary>
        /// Extract JTI from a validated token, or null when invalid/missing.
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID if found, null otherwise</returns>    
        string? GetJtiFromToken(string token);

        /// <summary>
        /// Extract SID (session id) from a validated token, or null when invalid/missing.
        /// </summary>
        string? GetSidFromToken(string token);


    }
}
