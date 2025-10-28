using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.DAL.Models.Specification;
using System.Linq.Expressions;

namespace BaseNetCore.Core.src.Main.DAL.Repository
{
    /// <summary>
    /// Full repository interface - kết hợp Read và Write
    /// </summary>
    public interface IRepository<TEntity> : IReadRepository<TEntity>, IWriteRepository<TEntity>
      where TEntity : class
    {
    }

    /// <summary>
    /// Read-only repository interface - Query operations only
    /// </summary>
    public interface IReadRepository<TEntity> where TEntity : class
    {
        // ============= SIMPLE QUERY METHODS - Đơn giản, dễ sử dụng =============

        /// <summary>
        /// Tìm entity theo ID (Primary Key)
        /// ⚡ Always TRACKED - Dùng cho Update/Delete scenarios
        /// 📖 Dùng FindAsync nếu cần read-only
        /// </summary>
        Task<TEntity> GetByIdAsync(object id);

        /// <summary>
        /// Tìm entity đầu tiên theo điều kiện
        /// Default: NoTracking (dùng cho read-only)
        /// Set tracking = true nếu cần update sau đó
        /// </summary>
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false);

        /// <summary>
        /// Lấy tất cả entities với điều kiện tùy chọn
        /// Default: NoTracking (dùng cho read-only)
        /// </summary>
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter = null, bool tracking = false);

        /// <summary>
        /// Đếm số lượng entities theo điều kiện
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null);

        /// <summary>
        /// Kiểm tra tồn tại theo điều kiện
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null);

        // ============= SPECIFICATION PATTERN - Cho query phức tạp =============

        /// <summary>
        /// NEW: Get entities using Specification pattern
        /// </summary>
        Task<List<TEntity>> GetAsync(ISpecification<TEntity> specification);

        /// <summary>
        /// NEW: Get paginated entities using Specification pattern
        /// </summary>
        Task<PageResponse<TEntity>> GetWithPagingAsync(ISpecification<TEntity> specification);

        /// <summary>
        /// NEW: Get first entity using Specification pattern
        /// </summary>
        Task<TEntity> FirstOrDefaultAsync(ISpecification<TEntity> specification);

        /// <summary>
        /// NEW: Count entities using Specification pattern
        /// </summary>
        Task<int> CountAsync(ISpecification<TEntity> specification);
    }
    /// <summary>
    /// Write-only repository interface - Command operations only
    /// KHÔNG tự động SaveChanges - để UnitOfWork quản lý
    /// </summary>
    public interface IWriteRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
        Task DeleteAsync(object id);
    }

}
