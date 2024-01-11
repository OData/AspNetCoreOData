//-----------------------------------------------------------------------------
// <copyright file="OrderByItNode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query
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
            Name = "$it";
        }

        /// <summary>
        /// Instantiates a new instance of <see cref="OrderByItNode"/> class.
        /// </summary>
        /// <param name="clause">The orderby clause.</param>
        public OrderByItNode(OrderByClause clause)
            : base(clause)
        {
            if (clause == null)
            {
                throw Error.ArgumentNull(nameof(clause));
            }

            if (clause.Expression is NonResourceRangeVariableReferenceNode nonResourceVarNode)
            {
                Name = nonResourceVarNode.Name;
            }
            else if (clause.Expression is ResourceRangeVariableReferenceNode resourceVarNode)
            {
                Name = resourceVarNode.Name;
            }
            else
            {
                throw new ODataException(string.Format(SRResources.OrderByClauseInvalid, clause.Expression.Kind,
                    "NonResourceRangeVariableReferenceNode or ResourceRangeVariableReferenceNode"));
            }
        }

        /// <summary>
        /// Gets the range variable name
        /// </summary>
        public string Name { get; }
    }
}
