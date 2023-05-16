using Microsoft.OData.UriParser;

namespace QueryBuilder.Query
{
    /// <summary>
    /// Represents the order by expression '$it' in the $orderby clause.
    /// </summary>
    public class OrderByItNode : OrderByNode
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="OrderByItNode"/> class.
        /// </summary>
        /// <param name="direction">The <see cref="OrderByDirection"/> for this node.</param>
        public OrderByItNode(OrderByDirection direction)
            : base(direction)
        {
        }
    }
}
