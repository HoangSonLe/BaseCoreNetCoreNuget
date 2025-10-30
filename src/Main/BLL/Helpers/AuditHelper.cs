using BaseNetCore.Core.src.Main.Common.Enums;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BaseNetCore.Core.src.Main.BLL.Helpers
{
    /// <summary>
    /// Static helper class for setting audit fields on entities.
    /// Automatically gets current user ID from HttpContext.
    /// </summary>
    public static class AuditHelper
    {
        /// <summary>
        /// Sets audit fields for entity creation.
        /// - CreatedDate = DateTime.UtcNow
        /// - CreatedBy = userId
        /// - UpdatedDate = DateTime.UtcNow
        /// - UpdatedBy = userId
        /// - State = Active
        /// </summary>
        public static void SetCreateAudit<TEntity>(TEntity entity, int userId) where TEntity : BaseAuditableEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var now = DateTime.UtcNow;
            entity.CreatedDate = now;
            entity.CreatedBy = userId;
            entity.UpdatedDate = now;
            entity.UpdatedBy = userId;
            entity.State = EState.Active;
        }

        /// <summary>
        /// Sets audit fields for entity update.
        /// - UpdatedDate = DateTime.UtcNow
        /// - UpdatedBy = userId
        /// </summary>
        public static void SetUpdateAudit<TEntity>(TEntity entity, int userId) where TEntity : BaseAuditableEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = userId;
        }

        /// <summary>
        /// Sets audit fields for entity soft delete.
        /// - UpdatedDate = DateTime.UtcNow
        /// - UpdatedBy = userId
        /// - State = Delete
        /// </summary>
        public static void SetDeleteAudit<TEntity>(TEntity entity, int userId) where TEntity : BaseAuditableEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            entity.State = EState.Delete;
        }

        /// <summary>
        /// Sets audit fields for multiple entities on creation.
        /// </summary>
        public static void SetCreateAuditRange<TEntity>(IEnumerable<TEntity> entities, int userId) where TEntity : BaseAuditableEntity
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                SetCreateAudit(entity, userId);
            }
        }

        /// <summary>
        /// Sets audit fields for multiple entities on update.
        /// </summary>
        public static void SetUpdateAuditRange<TEntity>(IEnumerable<TEntity> entities, int userId) where TEntity : BaseAuditableEntity
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                SetUpdateAudit(entity, userId);
            }
        }

        /// <summary>
        /// Sets audit fields for multiple entities on soft delete.
        /// </summary>
        public static void SetDeleteAuditRange<TEntity>(IEnumerable<TEntity> entities, int userId) where TEntity : BaseAuditableEntity
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                SetDeleteAudit(entity, userId);
            }
        }

        /// <summary>
        /// Gets current user ID from HttpContext.
        /// Returns 1 (system user) if not authenticated.
        /// </summary>
        public static int GetCurrentUserId(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor?.HttpContext?.User == null)
                return 1; // Default system user

            var user = httpContextAccessor.HttpContext.User;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return 1; // Default system user
        }
    }
}
