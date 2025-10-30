using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BaseNetCore.Core.src.Main.Security.Token
{
    /// <summary>
    /// Contract for application-provided token validation (DB checks, revocation, user status, etc.).
    /// Implemented in the application layer where persistence and domain entities live.
    /// </summary>
    public interface ITokenValidator
    {
        /// <summary>
        /// Return true when the token is considered valid by application rules (not revoked, user active...).
        /// </summary>
        Task<bool> ValidateAsync(ClaimsPrincipal principal, string rawToken, HttpContext httpContext);
    }
}
