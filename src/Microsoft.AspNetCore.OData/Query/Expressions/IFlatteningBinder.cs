//-----------------------------------------------------------------------------
// <copyright file="IFlatteningBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Provides an abstraction for flattening property access expressions within an OData $apply clause
    /// to support efficient translation of aggregation pipelines in LINQ providers like Entity Framework.
    /// </summary>
    /// <remarks>
    /// Entity Framework versions earlier than EF Core 6.0 may generate nested queries when accessing navigation properties
    /// in aggregation clauses. Flattening these properties can help generate flatter, more efficient SQL queries.
    /// This interface allows conditional support for flattening based on the capabilities of the underlying LINQ provider.
    /// </remarks>
    public interface IFlatteningBinder
    {
        /// <summary>
        /// Flattens properties referenced in aggregate clause to avoid generation of nested queries by Entity Framework.
        /// </summary>
        /// <param name="transformationNode">The OData $apply parse tree represented by <see cref="TransformationNode"/>.</param>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/> containing the current query context.</param>
        /// <returns>
        /// An <see cref="AggregationFlatteningResult"/> containing the modified query source and
        /// additional metadata resulting from the flattening operation.
        /// </returns>
        /// <remarks>
        /// This method generates a Select expression that flattens the properties referenced in the aggregate clause.
        /// Flattening properties helps prevent the generation of nested queries by Entity Framework,
        /// resulting in more efficient SQL generation.
        /// For query like:
        /// <code>
        /// groupby((A),aggregate(B/C with max as Alias1,B/D with max as Alias2))
        /// </code>
        /// generate an expression similar to:
        /// <code>
        /// $it => new FlatteningWrapper() {
        ///     Source = $it,
        ///     Container = new {
        ///         Value = $it.B.C
        ///         Next = new {
        ///             Value = $it.B.D
        ///         }
        ///     }
        /// }
        /// </code>
        /// Also populate expressions to access B/C and B/D in aggregate stage to look like:
        /// B/C : $it.Container.Value
        /// B/D : $it.Container.Next.Value
        /// </remarks>
        AggregationFlatteningResult FlattenReferencedProperties(
            TransformationNode transformationNode,
            IQueryable query,
            QueryBinderContext context);
    }
}
