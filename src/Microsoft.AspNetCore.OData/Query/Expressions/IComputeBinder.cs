//-----------------------------------------------------------------------------
// <copyright file="IComputeBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq.Expressions;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Exposes the ability to translate an OData $apply parse tree represented by a <see cref="ComputeTransformationNode"/> to an <see cref="Expression"/>.
    /// </summary>
    public interface IComputeBinder
    {
        /// <summary>
        /// Translates an OData $apply parse tree represented by a <see cref="ComputeTransformationNode"/> to
        /// an <see cref="Expression"/>.
        /// </summary>
        /// <param name="computeTransformationNode">The OData $apply parse tree represented by <see cref="ComputeTransformationNode"/>.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <remarks>
        /// Generates an expression structured like:
        /// $it => new ComputeWrapper&lt;T&gt; {
        ///     Instance = $it,
        ///     Model = parametrized(IEdmModel),
        ///     Container => new AggregationPropertyContainer() {
        ///         Name = "Z", 
        ///         Value = $it.X + $it.Y, 
        ///         Next = new LastInChain() {
        ///             Name = "C",
        ///             Value = $it.A * $it.B
        ///         }
        ///     }
        /// }
        /// </remarks>
        /// <returns>The generated LINQ expression representing the OData $apply parse tree.</returns>
        Expression BindCompute(ComputeTransformationNode computeTransformationNode, QueryBinderContext context);
    }
}
