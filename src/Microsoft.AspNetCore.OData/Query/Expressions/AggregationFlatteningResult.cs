//-----------------------------------------------------------------------------
// <copyright file="AggregationFlatteningResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Represents the result of flattening properties referenced in the aggregate clause.
    /// </summary>
    /// <remarks>
    /// Flattening properties referenced in an aggregate clause helps prevent the generation of nested queries by Entity Framework.
    /// For example, given a query like: <c>groupby((A), aggregate(B/C with max as Alias1, B/D with max as Alias2))</c>
    /// the expression is rewritten as:
    /// <code>
    /// .Select($it => new FlatteningWrapper&lt;T&gt; {
    ///     Source = $it,
    ///     Container = new {
    ///         Value = $it.B.C,
    ///         Next = new {
    ///             Value = $it.B.D
    ///         }
    ///     }
    /// })
    /// </code>
    /// A mapping is also maintained between the original properties and their flattened expressions:
    /// B/C → $it.Container.Value
    /// B/D → $it.Container.Next.Value
    /// This mapping is used during the aggregation stage to generate aggregate expressions.
    /// </remarks>
    public class AggregationFlatteningResult
    {
        /// <summary>
        /// Gets or sets the context parameter that has been redefined during the flattening process.
        /// </summary>
        public ParameterExpression RedefinedContextParameter { get; set; }

        /// <summary>
        /// Gets or sets the expression that has been rewritten as part of the flattening process.
        /// </summary>
        public Expression FlattenedExpression { get; set; }

        /// <summary>
        /// Gets or sets the mapping of single-value nodes to their corresponding flattened expressions.
        /// Example: { { $it.B.C, $it.Value }, { $it.B.D, $it.Next.Value } }
        /// </summary>
        public IDictionary<SingleValueNode, Expression> FlattenedPropertiesMapping { get; set; }
    }

}
