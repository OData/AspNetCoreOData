//-----------------------------------------------------------------------------
// <copyright file="OrderByClauseHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// Helper methods for <see cref="OrderByClause"/>.
    /// </summary>
    internal static class OrderByClauseHelpers
    {
        public static readonly string OrderByGlobalNameKey = "__orderby_EDC7A1AC-97F7-463F-BFF9-DE1FD7FCE27E";
        public static readonly string OrderByPropertyNamePrefix = "__orderby_";

        /// <summary>
        /// Convert the OrderByClause to list.
        /// </summary>
        /// <param name="clause">The input orderby clause, the 'ThenBy' in each node does matter.</param>
        /// <returns>The output orderby clauses, the 'ThenBy' in each node does NOT matter.</returns>
        public static List<OrderByClause> ToList(this OrderByClause clause)
        {
            List<OrderByClause> clauses = new List<OrderByClause>();
            while (clause != null)
            {
                // Be noted, in order to save the memory, we don't need to create a new OrderByClause and set the 'ThenBy' to null.
                //
                // clauses.Add(new OrderByClause(null, clause.Expression, clause.Direction, clause.RangeVariable));
                //
                clauses.Add(clause);
                clause = clause.ThenBy;
            }

            return clauses;
        }

        /// <summary>
        /// Test the OrderByClause to see whether it's an orderby like: $orderby=name
        /// For others, for example, $orderby=location/city is not top-level single property orderby.
        /// When we support key alias (key on sub-property or complex property), we should consider to update this.
        /// </summary>
        /// <param name="clause">The OrderbyClause.</param>
        /// <param name="property">The output property.</param>
        /// <param name="propertyName">The output property name, it works for dynamic property.</param>
        /// <returns>true/false.</returns>
        public static bool IsTopLevelSingleProperty(this OrderByClause clause, out IEdmProperty property, out string propertyName)
        {
            property = null;
            propertyName = null;
            if (clause == null)
            {
                return false;
            }

            // we only care about scenarios like: $orderby=name
            // we do nothing for others, for example: $orderby=location/city  or $orderby=tolower(name)
            if (clause.Expression is SingleValuePropertyAccessNode node &&
                (node.Source is ResourceRangeVariableReferenceNode || node.Source is NonResourceRangeVariableReferenceNode))
            {
                property = node.Property;
                propertyName = node.Property.Name;
                return true;
            }

            if (clause.Expression is SingleValueOpenPropertyAccessNode openNode &&
                (openNode.Source is ResourceRangeVariableReferenceNode || openNode.Source is NonResourceRangeVariableReferenceNode))
            {
                propertyName = openNode.Name;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove the ' desc' from the orderby clause.
        /// </summary>
        /// <param name="orderby">The orderby clause</param>
        /// <returns>changed orderby clause.</returns>
        public static string RemoveDesc(this string orderby)
        {
            if (orderby == null)
            {
                return orderby;
            }

            int index = orderby.LastIndexOf(" desc");
            if (index != -1)
            {
                return orderby.Substring(0, index);
            }

            return orderby;
        }
    }
}
