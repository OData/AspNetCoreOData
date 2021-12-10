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
        /// Translates an OData $apply parse tree represented by a <see cref="TransformationNode"/> to
        /// an <see cref="Expression"/>.
        /// </summary>
        /// <param name="transformationNode">The OData $apply parse tree represented by <see cref="TransformationNode"/>.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <remarks>
        /// Generates an expression structured like:
        /// $it => new DynamicTypeWrapper()
        /// {
        ///     GroupByContainer => new AggregationPropertyContainer() {
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
        /// })
        /// </remarks>
        /// <returns>The generated LINQ expression representing the OData $apply parse tree.</returns>
        Expression BindGroupBy(TransformationNode transformationNode, QueryBinderContext context);

        /// <summary>
        /// Translates an OData $apply parse tree represented by a <see cref="TransformationNode"/> to
        /// an <see cref="Expression"/>.
        /// </summary>
        /// <param name="transformationNode">The OData $apply parse tree represented by <see cref="TransformationNode"/>.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <remarks>
        /// Generates an expression structured like:
        /// $it => New DynamicType2()
        /// {
        ///     GroupByContainer = $it.Key.GroupByContainer /// If groupby clause present
        ///     Container => new AggregationPropertyContainer() {
        ///         Name = "Alias1",
        ///         Value = $it.AsQueryable().Sum(i => i.AggregatableProperty),
        ///         Next = new LastInChain() {
        ///             Name = "Alias2",
        ///             Value = $it.AsQueryable().Sum(i => i.AggregatableProperty)
        ///         }
        ///     }
        /// }
        /// </remarks>
        /// <returns>The generated LINQ expression representing the OData $apply parse tree.</returns>
        Expression BindSelect(TransformationNode transformationNode, QueryBinderContext context);

        /// <summary>
        /// Flattens properties referenced in aggregate clause to avoid generation of nested queries by Entity Framework.
        /// For query like groupby((A),aggregate(B/C with max as Alias1,B/D with max as Alias2)), generates an expression like:
        /// .Select($it => new FlatteningWrapper() {
        ///     Source = $it,
        ///     Container = new {
        ///         Value = $it.B.C
        ///         Next = new {
        ///             Value = $it.B.D
        ///         }
        ///     }
        /// })
        /// Also populate expressions to access B/C and B/D in aggregate stage to look like:
        /// B/C : $it.Container.Value
        /// B/D : $it.Container.Next.Value
        /// </summary>
        /// <param name="transformationNode">The <see cref="TransformationNode"/>.</param>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="context">The query binder context.</param>
        /// <param name="contextParameter">The parameter at the root of the current query binder context. The parameter can be reinitialized in the course of flattening.</param>
        /// <param name="flattenedPropertiesMap">Mapping of flattened single value nodes and their values. For example { {$it.B.C, $it.Value}, {$it.B.D, $it.Next.Value} }</param>
        /// <returns>Query with Select expression with flattened properties.</returns>
        IQueryable FlattenReferencedProperties(
            TransformationNode transformationNode,
            IQueryable query,
            QueryBinderContext context,
            out ParameterExpression contextParameter,
            out IDictionary<SingleValueNode, Expression> flattenedPropertiesMap);
    }
}
