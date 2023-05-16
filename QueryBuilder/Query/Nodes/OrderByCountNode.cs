using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace QueryBuilder.Query
{
    /// <summary>
    /// Represents an order by <see cref="IEdmProperty"/> expression.
    /// </summary>
    public class OrderByCountNode : OrderByNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByCountNode"/> class.
        /// </summary>
        /// <param name="orderByClause">The orderby clause representing property access.</param>
        public OrderByCountNode(OrderByClause orderByClause)
        {
            OrderByClause = orderByClause ?? throw Error.ArgumentNull(nameof(orderByClause));
            Direction = orderByClause.Direction;
        }

        /// <summary>
        /// Gets the <see cref="OrderByClause"/> of this node.
        /// </summary>
        public OrderByClause OrderByClause { get; }
    }
}
