//-----------------------------------------------------------------------------
// <copyright file="AggregationBinderHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Helper class for <see cref="AggregationBinder"/>.
    /// </summary>
    internal class AggregationBinderHelper : QueryBinder
    {
        private const string GroupByContainerProperty = "GroupByContainer";

        /// <summary>
        /// Pre flattens properties referenced in aggregate clause to avoid generation of nested queries by EF.
        /// For query like groupby((A), aggregate(B/C with max as Alias1, B/D with max as Alias2)) we need to generate 
        /// .Select(
        ///     $it => new FlattenninWrapper () {
        ///         Source = $it, // Will used in groupby stage
        ///         Container = new {
        ///             Value = $it.B.C
        ///             Next = new {
        ///                 Value = $it.B.D
        ///             }
        ///         }
        ///     }
        /// )
        /// Also we need to populate expressions to access B/C and B/D in aggregate stage. It will look like:
        /// B/C : $it.Container.Value
        /// B/D : $it.Container.Next.Value
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="context">The query binder context</param>
        /// <param name="flattenedPropertyContainer">Flattened list of properties from base query.</param>
        /// <param name="transformationNode">The <see cref="TransformationNode"/>.</param>
        /// <returns>Query with Select that flattens properties</returns>
        internal IQueryable FlattenReferencedProperties(
            IQueryable query,
            QueryBinderContext context,
            IDictionary<string, Expression> flattenedPropertyContainer,
            TransformationNode transformationNode)
        {
            IEnumerable<AggregateExpressionBase> aggregateExpressions = GetAggregateExpressions(context, transformationNode);
            IEnumerable<GroupByPropertyNode> groupingProperties = GetGroupingProperties(transformationNode);

            if (aggregateExpressions != null
                && aggregateExpressions.OfType<AggregateExpression>().Any(e => e.Method != AggregationMethod.VirtualPropertyCount)
                && groupingProperties != null
                && groupingProperties.Any()
                && (flattenedPropertyContainer == null || !flattenedPropertyContainer.Any()))
            {
                Type wrapperType = typeof(FlatteningWrapper<>).MakeGenericType(context.TransformationElementType);
                PropertyInfo sourceProperty = wrapperType.GetProperty("Source");
                List<MemberAssignment> wta = new List<MemberAssignment>();
                wta.Add(Expression.Bind(sourceProperty, context.LambdaParameter));

                List<AggregateExpression> aggregatedPropertiesToFlatten = aggregateExpressions.OfType<AggregateExpression>().Where(e => e.Method != AggregationMethod.VirtualPropertyCount).ToList();
                // Generated Select will be stack like, meaning that first property in the list will be deepest one
                // For example if we add $it.B.C, $it.B.D, select will look like
                // new {
                //      Value = $it.B.C
                //      Next = new {
                //          Value = $it.B.D
                //      }
                // }
                // We are generating references (in currentContainerExpression) from  the beginning of the  Select ($it.Value, then $it.Next.Value etc.)
                // We have proper match we need insert properties in reverse order
                // After this 
                // properties = { $it.B.D, $it.B.C}
                // _preFlattendMAp = { {$it.B.C, $it.Value}, {$it.B.D, $it.Next.Value} }
                NamedPropertyExpression[] properties = new NamedPropertyExpression[aggregatedPropertiesToFlatten.Count];
                int aliasIdx = aggregatedPropertiesToFlatten.Count - 1;
                ParameterExpression aggParam = Expression.Parameter(wrapperType, "$it");
                MemberExpression currentContainerExpression = Expression.Property(aggParam, GroupByContainerProperty);

                foreach (AggregateExpression aggExpression in aggregatedPropertiesToFlatten)
                {
                    string alias = "Property" + aliasIdx.ToString(CultureInfo.CurrentCulture); // We just need unique alias, we aren't going to use it

                    // Add Value = $it.B.C
                    Expression propAccessExpression = BindAccessor(aggExpression.Expression, context);
                    Type type = propAccessExpression.Type;
                    propAccessExpression = WrapConvert(propAccessExpression);
                    properties[aliasIdx] = new NamedPropertyExpression(Expression.Constant(alias), propAccessExpression);

                    // Save $it.Container.Next.Value for future use
                    UnaryExpression flatAccessExpression = Expression.Convert(
                        Expression.Property(currentContainerExpression, "Value"),
                        type);
                    currentContainerExpression = Expression.Property(currentContainerExpression, "Next");
                    context.PreFlattenedMap.Add(aggExpression.Expression, flatAccessExpression);
                    aliasIdx--;
                }

                PropertyInfo wrapperProperty = typeof(AggregationWrapper).GetProperty(GroupByContainerProperty);

                wta.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

                LambdaExpression flatLambda = Expression.Lambda(Expression.MemberInit(Expression.New(wrapperType), wta), context.LambdaParameter);

                query = ExpressionHelpers.Select(query, flatLambda, context.TransformationElementType);

                // We applied flattening let .GroupBy know about it.
                context.LambdaParameter = aggParam; // see how we can update the context
            }

            return query;
        }
    }
}
