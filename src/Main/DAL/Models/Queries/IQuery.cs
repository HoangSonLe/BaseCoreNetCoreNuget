using BaseNetCore.Core.src.Main.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BaseNetCore.Core.src.Main.DAL.Models.Queries
{
    public interface IQuery<TEntity> where TEntity : class
    {
        /// <summary>
        /// Filter expression (WHERE clause)
        /// </summary>
        Expression<Func<TEntity, bool>> Filter { get; set; }

        /// <summary>
        /// Ordering function
        /// </summary>
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderBy { get; set; }

        /// <summary>
        /// Pagination parameters
        /// </summary>
        PageRequest Paging { get; set; }

        /// <summary>
        /// String-based includes (legacy support)
        /// </summary>
        string IncludeProperties { get; set; }

        /// <summary>
        /// Strongly-typed includes
        /// </summary>
        List<Expression<Func<TEntity, object>>> Includes { get; set; }

        /// <summary>
        /// Enable/disable change tracking
        /// </summary>
        bool AsNoTracking { get; set; }

        /// <summary>
        /// Projection/Select expression (for DTOs)
        /// </summary>
        Expression<Func<TEntity, object>> Selector { get; set; }

        /// <summary>
        /// Skip records (for manual paging)
        /// </summary>
        int? Skip { get; set; }

        /// <summary>
        /// Take records (for manual paging)
        /// </summary>
        int? Take { get; set; }
    }

}
