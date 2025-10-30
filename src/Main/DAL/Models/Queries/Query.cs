using BaseNetCore.Core.src.Main.Common.Models;
using System.Linq.Expressions;

namespace BaseNetCore.Core.src.Main.DAL.Models.Queries
{
    /// <summary>
    /// Base Query Object implementation
    /// Enterprise pattern for encapsulating query logic
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public class Query<TEntity> : IQuery<TEntity> where TEntity : class
    {
        public Expression<Func<TEntity, bool>> Filter { get; set; }
        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderBy { get; set; }
        public PageRequest Paging { get; set; }
        public string IncludeProperties { get; set; }
        public List<Expression<Func<TEntity, object>>> Includes { get; set; }
        public bool AsNoTracking { get; set; }
        public Expression<Func<TEntity, object>> Selector { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }

        public Query()
        {
            Includes = new List<Expression<Func<TEntity, object>>>();
            AsNoTracking = true; // Default to read-only
        }

        /// <summary>
        /// Fluent API: Set filter
        /// </summary>
        public Query<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
        {
            Filter = filter;
            return this;
        }

        /// <summary>
        /// Fluent API: Set ordering
        /// </summary>
        public Query<TEntity> WithOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            OrderBy = orderBy;
            return this;
        }

        /// <summary>
        /// Fluent API: Set paging
        /// </summary>
        public Query<TEntity> WithPaging(int pageNumber, int pageSize)
        {
            Paging = new PageRequest(pageNumber, pageSize);
            return this;
        }

        /// <summary>
        /// Fluent API: Set paging
        /// </summary>
        public Query<TEntity> WithPaging(PageRequest paging)
        {
            Paging = paging;
            return this;
        }

        /// <summary>
        /// Fluent API: Add include
        /// </summary>
        public Query<TEntity> WithInclude(Expression<Func<TEntity, object>> include)
        {
            Includes.Add(include);
            return this;
        }

        /// <summary>
        /// Fluent API: Add string-based include
        /// </summary>
        public Query<TEntity> WithInclude(string includeProperty)
        {
            if (string.IsNullOrEmpty(IncludeProperties))
                IncludeProperties = includeProperty;
            else
                IncludeProperties += "," + includeProperty;
            return this;
        }

        /// <summary>
        /// Fluent API: Set tracking
        /// </summary>
        public Query<TEntity> WithTracking(bool asNoTracking = true)
        {
            AsNoTracking = asNoTracking;
            return this;
        }

        /// <summary>
        /// Fluent API: Set projection
        /// </summary>
        public Query<TEntity> WithSelector(Expression<Func<TEntity, object>> selector)
        {
            Selector = selector;
            return this;
        }

        /// <summary>
        /// Fluent API: Set skip/take
        /// </summary>
        public Query<TEntity> WithSkipTake(int skip, int take)
        {
            Skip = skip;
            Take = take;
            return this;
        }
    }
}
