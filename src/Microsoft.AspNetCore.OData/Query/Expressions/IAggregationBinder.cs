//-----------------------------------------------------------------------------
// <copyright file="IAggregationBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq.Expressions;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Exposes the ability to translate an OData $apply parse tree represented by <see cref="TransformationNode"/> to
    /// an <see cref="Expression"/>.
    /// </summary>
    public interface IAggregationBinder
    {
        // Generates expression
        //          $it => new DynamicTypeWrapper()
        //                                      {
        //                                           GroupByContainer => new AggregationPropertyContainer() {
        //                                               Name = "Prop1",
        //                                               Value = $it.Prop1,
        //                                               Next = new AggregationPropertyContainer() {
        //                                                   Name = "Prop2",
        //                                                   Value = $it.Prop2, // int
        //                                                   Next = new LastInChain() {
        //                                                       Name = "Prop3",
        //                                                       Value = $it.Prop3
        //                                                   }
        //                                               }
        //                                           }
        //                                      })

        /// <summary>
        /// Translate an OData $apply parse tree represented by <see cref="TransformationNode"/> to
        /// an <see cref="Expression"/>.
        /// </summary>
        /// <param name="transformationNode">The OData $apply parse tree represented by <see cref="TransformationNode"/>.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <returns></returns>
        Expression BindGroupBy(TransformationNode transformationNode, QueryBinderContext context);

        // Generates expression
        // $it => New DynamicType2()
        //                  {
        //                      GroupByContainer = $it.Key.GroupByContainer // If groupby section present
        //                      Container => new AggregationPropertyContainer() {
        //                          Name = "Alias1",
        //                          Value = $it.AsQueryable().Sum(i => i.AggregatableProperty),
        //                          Next = new LastInChain() {
        //                              Name = "Alias2",
        //                              Value = $it.AsQueryable().Sum(i => i.AggregatableProperty)
        //                          }
        //                      }
        //                  }

        /// <summary>
        /// Translate an OData $apply parse tree represented by <see cref="TransformationNode"/> to
        /// an <see cref="Expression"/>.
        /// </summary>
        /// <param name="transformationNode">The OData $apply parse tree represented by <see cref="TransformationNode"/>.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <returns></returns>
        Expression BindSelect(TransformationNode transformationNode, QueryBinderContext context);
    }
}
