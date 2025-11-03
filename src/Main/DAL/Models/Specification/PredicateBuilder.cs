using System.Linq.Expressions;

namespace BaseNetCore.Core.src.Main.DAL.Models.Specification
{
    /// <summary>
    /// PredicateBuilder for combining expressions efficiently
    /// Avoids Expression.Invoke which may not translate to SQL properly
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// Combines two expressions with AND logic
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            if (left == null) return right;
            if (right == null) return left;

            var parameter = Expression.Parameter(typeof(T), "x");
            var leftBody = new ParameterReplacer(parameter).Visit(left.Body);
            var rightBody = new ParameterReplacer(parameter).Visit(right.Body);
            var combined = Expression.AndAlso(leftBody, rightBody);

            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        /// <summary>
        /// Combines two expressions with OR logic
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(
                 this Expression<Func<T, bool>> left,
                 Expression<Func<T, bool>> right)
        {
            if (left == null) return right;
            if (right == null) return left;

            var parameter = Expression.Parameter(typeof(T), "x");
            var leftBody = new ParameterReplacer(parameter).Visit(left.Body);
            var rightBody = new ParameterReplacer(parameter).Visit(right.Body);
            var combined = Expression.OrElse(leftBody, rightBody);

            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        /// <summary>
        /// Visitor to replace parameters in expression tree
        /// </summary>
        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;

            public ParameterReplacer(ParameterExpression parameter)
            {
                _parameter = parameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parameter;
            }
        }
    }
}
