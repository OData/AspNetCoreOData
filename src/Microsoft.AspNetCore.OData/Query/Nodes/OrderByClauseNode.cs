//-----------------------------------------------------------------------------
// <copyright file="OrderByClauseNode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Represents the order by expression in the $orderby clause.
    /// Use this to represent other $orderby except 'Property,OpenProperty,$count, $it' orderBy expression.
    /// </summary>
    /// <remarks>
    /// Again, in the next major release, we don't need this class.
    /// Track on it at: https://github.com/OData/AspNetCoreOData/issues/1153
    /// </remarks>
    public class OrderByClauseNode : OrderByNode
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="OrderByClauseNode"/> class.
        /// </summary>
        /// <param name="orderByClause">The order by clause.</param>
        public OrderByClauseNode(OrderByClause orderByClause)
            : base(orderByClause)
        {
            OrderByClause = orderByClause;
        }

        /// <summary>
        /// Gets the <see cref="OrderByClause"/> of this node.
        /// </summary>
        public OrderByClause OrderByClause { get; }
    }
}
