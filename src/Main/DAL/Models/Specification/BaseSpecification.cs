using System.Linq.Expressions;

namespace BaseNetCore.Core.src.Main.DAL.Models.Specification
{
    /// <summary>
    /// Base specification implementation with Fluent API support
    /// Enterprise pattern used by major companies
    /// Supports both Inheritance Pattern and Fluent API Pattern
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <summary>
    /// Base specification implementation with Fluent API support
    /// Can be used directly or inherited for named specifications
    /// Enterprise pattern used by major companies
    /// Supports both Inheritance Pattern and Fluent API Pattern
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class BaseSpecification<T> : ISpecification<T>
    {
        public BaseSpecification()
        {
        }

        public BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>> Criteria { get; private set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public Expression<Func<T, object>> OrderBy { get; private set; }
        public Expression<Func<T, object>> OrderByDescending { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }
        public bool AsNoTracking { get; private set; } = true;

        #region Fluent API Methods - For method chaining

        /// <summary>
        /// Set criteria with fluent API
        /// </summary>
        /// <param name="criteria">Filter expression</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> WithCriteria(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
            return this;
        }

        /// <summary>
        /// Add additional criteria with AND logic
        /// Combines with existing criteria using AND operator
        /// </summary>
        /// <param name="criteria">Additional filter expression</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> AndCriteria(Expression<Func<T, bool>> criteria)
        {
            if (Criteria == null)
            {
                Criteria = criteria;
            }
            else
            {
                // Combine existing criteria with new criteria using AND
                var parameter = Expression.Parameter(typeof(T), "x");
                var combined = Expression.AndAlso(
                   Expression.Invoke(Criteria, parameter),
                  Expression.Invoke(criteria, parameter)
            );
                Criteria = Expression.Lambda<Func<T, bool>>(combined, parameter);
            }
            return this;
        }

        /// <summary>
        /// Add additional criteria with OR logic
        /// Combines with existing criteria using OR operator
        /// </summary>
        /// <param name="criteria">Additional filter expression</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> OrCriteria(Expression<Func<T, bool>> criteria)
        {
            if (Criteria == null)
            {
                Criteria = criteria;
            }
            else
            {
                // Combine existing criteria with new criteria using OR
                var parameter = Expression.Parameter(typeof(T), "x");
                var combined = Expression.OrElse(
              Expression.Invoke(Criteria, parameter),
                 Expression.Invoke(criteria, parameter)
               );
                Criteria = Expression.Lambda<Func<T, bool>>(combined, parameter);
            }
            return this;
        }

        /// <summary>
        /// Add include expression for eager loading with fluent API
        /// </summary>
        /// <param name="includeExpression">Include expression</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> WithInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
            return this;
        }

        /// <summary>
        /// Add include string for eager loading with fluent API (legacy support)
        /// </summary>
        /// <param name="includeString">Include string</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> WithInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
            return this;
        }

        /// <summary>
        /// Set order by ascending with fluent API
        /// </summary>
        /// <param name="orderByExpression">Order by expression</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> WithOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
            return this;
        }

        /// <summary>
        /// Set order by descending with fluent API
        /// </summary>
        /// <param name="orderByDescendingExpression">Order by descending expression</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> WithOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
            return this;
        }

        /// <summary>
        /// Apply paging with fluent API (by skip and take)
        /// </summary>
        /// <param name="skip">Number of items to skip</param>
        /// <param name="take">Number of items to take</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> WithPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
            return this;
        }

        /// <summary>
        /// Apply paging with fluent API (by page number and page size)
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> WithPagedResults(int pageNumber, int pageSize)
        {
            Skip = (pageNumber - 1) * pageSize;
            Take = pageSize;
            IsPagingEnabled = true;
            return this;
        }

        /// <summary>
        /// Enable/disable change tracking with fluent API
        /// </summary>
        /// <param name="asNoTracking">True for read-only, false for tracking</param>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> WithTracking(bool asNoTracking = true)
        {
            AsNoTracking = asNoTracking;
            return this;
        }

        /// <summary>
        /// Enable no tracking (read-only) with fluent API
        /// </summary>
        /// <returns>Current specification for chaining</returns>
        public BaseSpecification<T> AsReadOnly()
        {
            AsNoTracking = true;
            return this;
        }

        #endregion

        #region Protected Methods - For inheritance pattern

        /// <summary>
        /// Add include expression for eager loading
        /// </summary>
        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Add include string (legacy support)
        /// </summary>
        protected virtual void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        /// <summary>
        /// Set order by ascending
        /// </summary>
        protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        /// <summary>
        /// Set order by descending
        /// </summary>
        protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }

        /// <summary>
        /// Apply paging
        /// </summary>
        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        /// <summary>
        /// Enable/disable change tracking
        /// </summary>
        protected virtual void ApplyTracking(bool asNoTracking)
        {
            AsNoTracking = asNoTracking;
        }

        #endregion
    }
}
