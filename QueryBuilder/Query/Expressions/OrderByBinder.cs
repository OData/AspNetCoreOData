using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;
using QueryBuilder.Query.Expressions;
using QueryBuilder;

namespace QueryBuilder.Query.Expressions
{
    /// <summary>
    /// The default implementation to bind an OData $orderby represented by <see cref="OrderByClause"/>
    /// an <see cref="Expression"/> wrappered in <see cref="OrderByBinderResult"/>.
    /// </summary>
    public class OrderByBinder : QueryBinder, IOrderByBinder
    {
        /// <summary>
        /// Translates an OData $orderby represented by <see cref="OrderByClause"/> to <see cref="Expression"/>.
        /// $orderby=Age
        ///    |--  x => x.Age
        /// </summary>
        /// <param name="orderByClause">The orderby clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The OrderBy binder result, <see cref="OrderByBinderResult"/>.</returns>
        public virtual OrderByBinderResult BindOrderBy(OrderByClause orderByClause, QueryBinderContext context)
        {
            if (orderByClause == null)
            {
                throw Error.ArgumentNull(nameof(orderByClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            OrderByBinderResult head = null;
            OrderByBinderResult last = null;
            for (OrderByClause clause = orderByClause; clause != null; clause = clause.ThenBy)
            {
                Expression body = Bind(clause.Expression, context);

                ParameterExpression parameter = context.CurrentParameter;

                LambdaExpression orderByLambda = Expression.Lambda(body, parameter);

                OrderByBinderResult result = new OrderByBinderResult(orderByLambda, clause.Direction);

                if (head == null)
                {
                    head = result;
                    last = result;
                }
                else
                {
                    Contract.Assert(last != null);
                    last.ThenBy = result;
                    last = result;
                }
            }

            return head;
        }
    }
}
