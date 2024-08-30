//-----------------------------------------------------------------------------
// <copyright file="OrderByCountNode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query;

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
        : base(orderByClause)
    {
        OrderByClause = orderByClause;
        if (!(orderByClause.Expression is CountNode))
        {
            throw new ODataException(string.Format(SRResources.OrderByClauseInvalid, orderByClause.Expression.Kind, QueryNodeKind.Count));
        }
    }

    /// <summary>
    /// Gets the <see cref="OrderByClause"/> of this node.
    /// </summary>
    public OrderByClause OrderByClause { get; }
}
