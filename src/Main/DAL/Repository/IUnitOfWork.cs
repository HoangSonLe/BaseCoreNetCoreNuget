using Microsoft.EntityFrameworkCore.Storage;

namespace BaseNetCore.Core.src.Main.DAL.Repository
{
    /// <summary>
    /// Unit of Work pattern - quản lý transaction và repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Get repository for any entity type dynamically
        /// </summary>
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;

        // Transaction management
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
