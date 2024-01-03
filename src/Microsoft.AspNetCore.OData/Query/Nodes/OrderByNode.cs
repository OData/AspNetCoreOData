//-----------------------------------------------------------------------------
// <copyright file="OrderByNode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Represents a single order by expression in the $orderby clause.
    /// saxu: Why do we need this class and its derived type? only fetch the PropertyPath? In the next major release, we can consider to remove all of these.
    /// </summary>
    public abstract class OrderByNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByNode"/> class.
        /// </summary>
        /// <param name="direction">The direction of the sort order.</param>
        protected OrderByNode(OrderByDirection direction)
        {
            Direction = direction;
            PropertyPath = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByNode"/> class.
        /// </summary>
        /// <param name="orderByClause">The clause of the sort order.</param>
        protected OrderByNode(OrderByClause orderByClause)
        {
            if (orderByClause == null)
            {
                throw Error.ArgumentNull(nameof(orderByClause));
            }

            Direction = orderByClause.Direction;
            PropertyPath = RestorePropertyPath(orderByClause.Expression);
        }

        /// <summary>
        /// Gets the <see cref="OrderByDirection"/> for the current node.
        /// </summary>
        public OrderByDirection Direction { get; internal set; }

        internal string PropertyPath { get; set; }

        /// <summary>
        /// Creates a list of <see cref="OrderByNode"/> instances from a linked list of <see cref="OrderByClause"/> instances.
        /// </summary>
        /// <param name="orderByClause">The head of the <see cref="OrderByClause"/> linked list.</param>
        /// <returns>The list of new <see cref="OrderByPropertyNode"/> instances.</returns>
        public static IList<OrderByNode> CreateCollection(OrderByClause orderByClause)
        {
            List<OrderByNode> result = new List<OrderByNode>();
            for (OrderByClause clause = orderByClause; clause != null; clause = clause.ThenBy)
            {
                if (clause.Expression is CountNode)
                {
                    result.Add(new OrderByCountNode(clause));
                }
                else if (clause.Expression is NonResourceRangeVariableReferenceNode ||
                    clause.Expression is ResourceRangeVariableReferenceNode)
                {
                    result.Add(new OrderByItNode(clause));
                }
                else if (clause.Expression is SingleValueOpenPropertyAccessNode)
                {
                    result.Add(new OrderByOpenPropertyNode(clause));
                }
                else if(clause.Expression is SingleValuePropertyAccessNode)
                {
                    result.Add(new OrderByPropertyNode(clause));
                }
                else
                {
                    // For other, let's create a wrapper. In next major release, we don't need this wrapper.
                    result.Add(new OrderByClauseNode(clause));
                }
            }

            return result;
        }

        internal static string RestorePropertyPath(SingleValueNode expression)
        {
            if (expression == null)
            {
                return string.Empty;
            }

            string propertyName = string.Empty;
            SingleValueNode source = null;

            var accessNode = expression as SingleValuePropertyAccessNode;
            if (accessNode != null)
            {
                propertyName = accessNode.Property.Name;
                source = accessNode.Source;
            }
            else
            {
                var complexNode = expression as SingleComplexNode;
                if (complexNode != null)
                {
                    propertyName = complexNode.Property.Name;
                    source = complexNode.Source;
                }
                else
                {
                    var navNode = expression as SingleNavigationNode;
                    if (navNode != null)
                    {
                        propertyName = navNode.NavigationProperty.Name;
                        source = navNode.Source;
                    }
                }
            }

            var parentPath = RestorePropertyPath(source);
            if (string.IsNullOrEmpty(parentPath))
            {
                return propertyName;
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, "{0}/{1}", parentPath, propertyName);
            }
        }
    }
}
