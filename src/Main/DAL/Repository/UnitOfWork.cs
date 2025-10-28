using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BaseNetCore.Core.src.Main.DAL.Repository
{
    /// <summary>
    /// Unit of Work implementation - quản lý transaction và repositories
    /// IMPROVED: Pure generic pattern for Core Library/NuGet Package
    /// - No domain-specific repositories (User, Role, etc.)
    /// - Fully reusable across projects
    /// - Supports dynamic repository creation with caching
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private IDbContextTransaction _transaction;

        // Dictionary để cache các repository instances
        private readonly Dictionary<Type, object> _repositories;

        /// <summary>
        /// Constructor nhận DbContext - flexible cho mọi database provider
        /// </summary>
        public UnitOfWork(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Get repository for any entity type dynamically
        /// Sử dụng lazy initialization và caching
        /// </summary>
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            // Kiểm tra và trả về repository từ cache nếu đã tồn tại
            if (!_repositories.TryGetValue(type, out var repository))
            {
                // Tạo mới và cache repository
                repository = new Repository<TEntity>(_context);
                _repositories[type] = repository;
            }

            return (IRepository<TEntity>)repository;
        }

        // Transaction Management
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return _transaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);

                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Dispose pattern
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _repositories.Clear();
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
