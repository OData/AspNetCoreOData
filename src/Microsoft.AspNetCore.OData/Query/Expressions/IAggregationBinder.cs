//-----------------------------------------------------------------------------
// <copyright file="IAggregationBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Exposes the ability to translate an OData $apply parse tree represented by a <see cref="TransformationNode"/> to
    /// an <see cref="Expression"/>.
    /// </summary>
    public interface IAggregationBinder
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

        /// <summary>
        /// Translates an OData $apply parse tree represented by a <see cref="TransformationNode"/> to
        /// a LINQ <see cref="Expression"/> that performs a GroupBy operation.
        /// </summary>
        /// <param name="transformationNode">The OData $apply parse tree represented by <see cref="TransformationNode"/>.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/> containing the current query context..</param>
        /// <returns>A <see cref="LambdaExpression"/> representing the GroupBy operation in LINQ.</returns>
        /// <remarks>
        /// Generates an expression similar to:
        /// <code>
        /// $it => new DynamicTypeWrapper() {
        ///     GroupByContainer = new AggregationPropertyContainer() {
        ///         Name = "Prop1",
        ///         Value = $it.Prop1,
        ///         Next = new AggregationPropertyContainer() {
        ///             Name = "Prop2",
        ///             Value = $it.Prop2,
        ///             Next = new LastInChain() {
        ///                 Name = "Prop3",
        ///                 Value = $it.Prop3
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        Expression BindGroupBy(TransformationNode transformationNode, QueryBinderContext context);

        /// <summary>
        /// Translates an OData $apply parse tree represented by a <see cref="TransformationNode"/> to
        /// a LINQ <see cref="Expression"/> that performs a Select operation.
        /// </summary>
        /// <param name="transformationNode">The OData $apply parse tree represented by <see cref="TransformationNode"/>.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/> containing the current query context.</param>
        /// <returns>A <see cref="LambdaExpression"/> representing the Select operation in LINQ.</returns>
        /// <remarks>
        /// Generates an expression similar to:
        /// <code>
        /// $it => new DynamicTypeWrapper() {
        ///     GroupByContainer = $it.Key.GroupByContainer // If groupby clause present
        ///     Container = new AggregationPropertyContainer() {
        ///         Name = "Alias1",
        ///         Value = $it.AsQueryable().Sum(i => i.AggregatableProperty),
        ///         Next = new LastInChain() {
        ///             Name = "Alias2",
        ///             Value = $it.AsQueryable().Sum(i => i.AggregatableProperty)
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        Expression BindSelect(TransformationNode transformationNode, QueryBinderContext context);
    }
}
