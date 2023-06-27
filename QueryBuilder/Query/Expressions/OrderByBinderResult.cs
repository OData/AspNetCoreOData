using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace ODataQueryBuilder.Query.Expressions
{
    /// <summary>
    /// Represents a single order by expression in the $orderby clause.
    /// </summary>
    public class OrderByBinderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByBinderResult"/> class.
        /// </summary>
        /// <param name="orderByExpression">The orderby expression.</param>
        /// <param name="direction">The orderby direction.</param>
        public OrderByBinderResult(Expression orderByExpression, OrderByDirection direction)
        {
            OrderByExpression = orderByExpression ?? throw Error.ArgumentNull(nameof(orderByExpression));
            Direction = direction;
        }

        /// <summary>
        /// Gets the orderby expression.
        /// </summary>
        public Expression OrderByExpression { get; }

        /// <summary>
        /// Gets the orderby direction.
        /// </summary>
        public OrderByDirection Direction { get; }

        /// <summary>
        /// Gets or sets the thenby result.
        /// </summary>
        public OrderByBinderResult ThenBy { get; set; }
    }
}
