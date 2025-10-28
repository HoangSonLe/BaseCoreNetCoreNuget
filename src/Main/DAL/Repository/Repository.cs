using BaseNetCore.Core.src.Main.Common.Attributes;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseNetCore.Core.src.Main.DAL.Models.Specification;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BaseNetCore.Core.src.Main.DAL.Repository
{
    /// <summary>
    /// IMPROVED: Base Repository implementation với CQRS support
    /// Changes:
    /// - Added strongly-typed Include support
    /// - Improved ApplySpecifications logic
    /// - Added overload methods for common scenarios
    /// - Better separation of concerns
    /// - Added automatic search string generation for entities marked with [SearchableEntity]
    /// </summary>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        #region Read Operations - IMPROVED

        // ============= SIMPLE QUERY METHODS - Đơn giản, dễ sử dụng =============

        /// <summary>
        /// Tìm entity theo ID (Primary Key)
        /// Default: Tracking enabled (dùng cho update/delete)
        /// </summary>
        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Tìm entity đầu tiên theo điều kiện
        /// Default: NoTracking (dùng cho read-only)
        /// Set tracking = true nếu cần update sau đó
        /// </summary>
        public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            IQueryable<TEntity> query = tracking ? _dbSet : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Lấy tất cả entities với điều kiện tùy chọn
        /// Default: NoTracking (dùng cho read-only)
        /// </summary>
        public virtual async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter = null, bool tracking = false)
        {
            IQueryable<TEntity> query = tracking ? _dbSet : _dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Count entities
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null
             ? await _dbSet.CountAsync()
           : await _dbSet.CountAsync(filter);
        }

        /// <summary>
        /// Check if any entity exists
        /// </summary>
        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null
         ? await _dbSet.AnyAsync()
             : await _dbSet.AnyAsync(filter);
        }

        /// <summary>
        /// NEW: Get IQueryable for advanced scenarios
        /// </summary>
        public virtual IQueryable<TEntity> Query(bool asNoTracking = true)
        {
            return asNoTracking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();
        }

        #endregion

        #region NEW: Specification Pattern Support

        /// <summary>
        /// Get entities using Specification pattern
        /// </summary>
        public virtual async Task<List<TEntity>> GetAsync(ISpecification<TEntity> specification)
        {
            var query = SpecificationEvaluator<TEntity>.GetQuery(_dbSet, specification);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Get paginated entities using Specification pattern
        /// </summary>
        public virtual async Task<PageResponse<TEntity>> GetWithPagingAsync(ISpecification<TEntity> specification)
        {
            if (!specification.IsPagingEnabled)
                throw new InvalidOperationException("Specification must have paging enabled. Use ApplyPaging() in your specification.");

            // Get total count before paging
            var countQuery = _dbSet.AsQueryable();
            if (specification.AsNoTracking)
                countQuery = countQuery.AsNoTracking();
            if (specification.Criteria != null)
                countQuery = countQuery.Where(specification.Criteria);

            var totalRecords = await countQuery.CountAsync();

            // Get paged data
            var query = SpecificationEvaluator<TEntity>.GetQuery(_dbSet, specification);
            var data = await query.ToListAsync();

            var pageSize = specification.Take;
            var pageNumber = (specification.Skip / specification.Take) + 1;

            return new PageResponse<TEntity>(data, true, totalRecords, pageNumber, pageSize);
        }

        /// <summary>
        /// Get first entity using Specification pattern
        /// </summary>
        public virtual async Task<TEntity> FirstOrDefaultAsync(ISpecification<TEntity> specification)
        {
            var query = SpecificationEvaluator<TEntity>.GetQuery(_dbSet, specification);
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Count entities using Specification pattern
        /// </summary>
        public virtual async Task<int> CountAsync(ISpecification<TEntity> specification)
        {
            var countQuery = _dbSet.AsQueryable();

            if (specification.AsNoTracking)
                countQuery = countQuery.AsNoTracking();

            if (specification.Criteria != null)
                countQuery = countQuery.Where(specification.Criteria);

            return await countQuery.CountAsync();
        }

        #endregion

        #region Write Operations - IMPROVED

        /// <summary>
        /// Add single entity
        /// Automatically generates search string if entity is marked with [SearchableEntity] attribute
        /// </summary>
        public virtual void Add(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Auto-generate search string if entity has [SearchableEntity] attribute and implements ISearchableEntity
            //GenerateSearchStringIfNeeded(entity);

            _dbSet.Add(entity);
        }

        /// <summary>
        /// Add multiple entities
        /// Automatically generates search strings for entities marked with [SearchableEntity] attribute
        /// </summary>
        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            // IMPROVED: Check if enumerable is empty before adding
            var entitiesList = entities.ToList();
            if (entitiesList.Count == 0)
                return; // No error, just skip

            // Auto-generate search strings for all searchable entities
            //foreach (var entity in entitiesList)
            //{
            //    GenerateSearchStringIfNeeded(entity);
            //}

            _dbSet.AddRange(entitiesList);
        }

        /// <summary>
        /// Update single entity
        /// Automatically generates search string if entity is marked with [SearchableEntity] attribute
        /// </summary>
        public virtual void Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Auto-generate search string if entity has [SearchableEntity] attribute and implements ISearchableEntity
            //GenerateSearchStringIfNeeded(entity);

            // IMPROVED: Check if entity is tracked
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }

            entry.State = EntityState.Modified;
        }

        /// <summary>
        /// Update multiple entities
        /// Automatically generates search strings for entities marked with [SearchableEntity] attribute
        /// </summary>
        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entitiesList = entities.ToList();
            if (entitiesList.Count == 0)
                return;

            //// Auto-generate search strings for all searchable entities
            //foreach (var entity in entitiesList)
            //{
            //    GenerateSearchStringIfNeeded(entity);
            //}

            _dbSet.UpdateRange(entitiesList);
        }

        /// <summary>
        /// Delete single entity
        /// </summary>
        public virtual void Delete(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // IMPROVED: Handle detached entities
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Delete multiple entities
        /// </summary>
        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entitiesList = entities.ToList();
            if (entitiesList.Count == 0)
                return;

            _dbSet.RemoveRange(entitiesList);
        }

        /// <summary>
        /// IMPROVED: Delete by ID - Find and mark for deletion
        /// Note: Still requires SaveChanges from UnitOfWork
        /// </summary>
        public virtual async Task DeleteAsync(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
            // If entity not found, do nothing (idempotent operation)
        }

        /// <summary>
        /// NEW: Attach entity to context
        /// </summary>
        public virtual void Attach(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
        }

        /// <summary>
        /// NEW: Detach entity from context
        /// </summary>
        public virtual void Detach(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Detached;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Generates search string if entity is marked with [SearchableEntity] attribute and implements ISearchableEntity.
        /// Only generates if attribute exists and is enabled.
        /// </summary>
        private void GenerateSearchStringIfNeeded(TEntity entity)
        {
            // Check if entity type has [SearchableEntity] attribute
            var entityType = entity.GetType();
            var searchableAttr = entityType.GetCustomAttributes(typeof(SearchableEntityAttribute), true)
            .FirstOrDefault() as SearchableEntityAttribute;

            // If no attribute or disabled, skip
            if (searchableAttr == null || !searchableAttr.Enabled)
                return;

            // Check if entity implements ISearchableEntity
            if (entity is ISearchableEntity searchableEntity)
            {
                searchableEntity.GenerateSearchString();
            }
        }

        #endregion
    }
}
