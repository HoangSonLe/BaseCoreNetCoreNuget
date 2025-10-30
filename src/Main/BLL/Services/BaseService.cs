using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseNetCore.Core.src.Main.DAL.Repository;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BaseNetCore.Core.src.Main.BLL.Services
{
    /// <summary>
    /// Base service providing User Context from JWT token.
    /// Derived services can access:
    /// - Current user information (CurrentUserId, CurrentUsername, CurrentUserRoles)
    /// - Repository and UnitOfWork for data access
    /// - Use AuditHelper for manual audit tracking when needed
    /// </summary>
    /// <typeparam name="TEntity">Entity type that inherits from BaseAuditableEntity</typeparam>
    public abstract class BaseService<TEntity> : IBaseService<TEntity> where TEntity : BaseAuditableEntity
    {
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly IRepository<TEntity> Repository;
        protected readonly IHttpContextAccessor HttpContextAccessor;

        protected BaseService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            Repository = unitOfWork.Repository<TEntity>();
        }

        #region User Context Properties

        /// <summary>
        /// Gets the current user's ClaimsPrincipal from HTTP context.
        /// </summary>
        protected ClaimsPrincipal? CurrentUser => HttpContextAccessor.HttpContext?.User;

        /// <summary>
        /// Gets the current user ID from JWT token (NameIdentifier or Sub claim).
        /// Returns 1 (system user) if not authenticated.
        /// </summary>
        public int CurrentUserId
        {
            get
            {
                var userIdClaim = CurrentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? CurrentUser?.FindFirst("sub")?.Value;

                if (int.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                return 1; // Default system user
            }
        }

        /// <summary>
        /// Gets the current username from JWT token (Name claim).
        /// </summary>
        public string? CurrentUsername => CurrentUser?.FindFirst(ClaimTypes.Name)?.Value
            ?? CurrentUser?.FindFirst("name")?.Value;

        /// <summary>
        /// Gets all roles of current user from JWT token.
        /// </summary>
        public IEnumerable<string> CurrentUserRoles
        {
            get
            {
                return CurrentUser?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Checks if current user has a specific role.
        /// </summary>
        public bool HasRole(string role)
        {
            return CurrentUserRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Ensures user is authenticated. Throws exception if not.
        /// </summary>
        protected void EnsureAuthenticated()
        {
            if (CurrentUserId <= 1) // System user or invalid
            {
                throw new Common.Exceptions.TokenInvalidException("User is not authenticated");
            }
        }

        #endregion
    }
}
