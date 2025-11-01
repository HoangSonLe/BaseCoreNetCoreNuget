using BaseNetCore.Core.src.Main.DAL.Models.Entities;

namespace BaseNetCore.Core.src.Main.BLL.Services
{
    /// <summary>
    /// Base interface for service classes providing User Context from JWT token.
    /// </summary>
    /// <typeparam name="TEntity">Entity type that inherits from BaseAuditableEntity</typeparam>
    public interface IBaseService<TEntity> where TEntity : BaseAuditableEntity
    {
        #region User Context

        /// <summary>
        /// Gets the current user ID from JWT token.
        /// Returns 1 (system user) if not authenticated.
        /// </summary>
        int CurrentUserId { get; }

        /// <summary>
        /// Gets the current username from JWT token.
        /// </summary>
        string? CurrentUsername { get; }

        /// <summary>
        /// Gets all roles of current user from JWT token.
        /// </summary>
        IEnumerable<string> CurrentUserRoles { get; }

        /// <summary>
        /// Checks if current user has a specific role.
        /// </summary>
        bool HasRole(string role);

        #endregion
    }
}
