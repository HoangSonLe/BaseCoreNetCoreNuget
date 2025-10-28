using System.Linq.Expressions;

namespace BaseNetCore.Core.src.Main.DAL.Models.Specification
{
    /// <summary>
    /// Base specification interface for encapsulating business rules
    /// Used by: Microsoft, Amazon, Netflix, Google
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Filter criteria
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// Includes for eager loading
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// Include strings for string-based includes (legacy support)
        /// </summary>
        List<string> IncludeStrings { get; }

        /// <summary>
        /// Order by expression
        /// </summary>
        Expression<Func<T, object>> OrderBy { get; }

        /// <summary>
        /// Order by descending expression
        /// </summary>
        Expression<Func<T, object>> OrderByDescending { get; }

        /// <summary>
        /// Paging - Skip count
        /// </summary>
        int Take { get; }

        /// <summary>
        /// Paging - Take count
        /// </summary>
        int Skip { get; }

        /// <summary>
        /// Enable paging
        /// </summary>
        bool IsPagingEnabled { get; }

        /// <summary>
        /// Use AsNoTracking for read-only queries
        /// </summary>
        bool AsNoTracking { get; }
    }

}
