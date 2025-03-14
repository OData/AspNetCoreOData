//-----------------------------------------------------------------------------
// <copyright file="TestAggregationBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.UriParser.Aggregation;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Wrapper;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Container;

namespace Microsoft.AspNetCore.OData.TestCommon.Query.Expressions
{
    public class TestAggregationBinder : QueryBinder, IAggregationBinder
    {
        private const string GroupingPropertiesContainerProperty = "GroupByContainer";
        private const string AggregationPropertiesContainerProperty = "Container";

        public virtual Expression BindGroupBy(TransformationNode transformationNode, QueryBinderContext context)
        {
            Debug.Assert(transformationNode != null, "transformationNode != null");
            Debug.Assert(context != null, "context != null");

            if (transformationNode is GroupByTransformationNode groupByTransformationNode && groupByTransformationNode.GroupingProperties?.Any() == true)
            {
                var groupingProperties = CreateGroupByMemberAssignments(groupByTransformationNode.GroupingProperties, context);

                var groupingPropertiesContainerProperty = typeof(TestGroupByWrapper).GetProperty(GroupingPropertiesContainerProperty);
                var memberAssignments = new List<MemberAssignment>(capacity: 1)
                {
                    Expression.Bind(
                        typeof(TestGroupByWrapper).GetProperty(GroupingPropertiesContainerProperty),
                        TestAggregationPropertyContainer.CreateNextNamedPropertyContainer(groupingProperties))
                };

                return Expression.Lambda(
                    Expression.MemberInit(Expression.New(typeof(TestGroupByWrapper)), memberAssignments),
                    context.CurrentParameter);
            }

            return Expression.Lambda(Expression.New(typeof(TestGroupByWrapper)), context.CurrentParameter);
        }

        public virtual Expression BindSelect(TransformationNode transformationNode, QueryBinderContext context)
        {
            Debug.Assert(transformationNode != null, "transformationNode != null");
            Debug.Assert(context != null, "context != null");

            var groupByClrType = typeof(TestGroupByWrapper);
            var groupingType = typeof(IGrouping<,>).MakeGenericType(groupByClrType, context.TransformationElementType);
            var resultClrType = typeof(TestGroupByWrapper);

            var groupingParam = Expression.Parameter(groupingType, "$it");
            var memberAssignments = new List<MemberAssignment>();

            if (transformationNode is GroupByTransformationNode groupByTransformationNode && groupByTransformationNode.GroupingProperties?.Any() == true)
            {
                memberAssignments.Add(
                    Expression.Bind(
                        resultClrType.GetProperty(GroupingPropertiesContainerProperty),
                        Expression.Property(Expression.Property(groupingParam, "Key"), GroupingPropertiesContainerProperty)
                ));
            }

            // If there are aggregate expressions
            var aggregateExpressions = GetAggregateExpressions(transformationNode, context);
            if (aggregateExpressions?.Any() == true)
            {
                var aggregationProperties = new List<NamedPropertyExpression>();
                foreach (var aggregateExpr in aggregateExpressions)
                {
                    // For simplicity, we handle property aggregations
                    if (!(aggregateExpr.AggregateKind == AggregateExpressionKind.PropertyAggregate))
                    {
                        throw new NotSupportedException("Aggregate expression kind not supported.");
                    }

                    var propertyAggregateExpr = CreatePropertyAggregateExpression(groupingParam, aggregateExpr as AggregateExpression, context.TransformationElementType, context);
                    aggregationProperties.Add(new NamedPropertyExpression(Expression.Constant(aggregateExpr.Alias), propertyAggregateExpr));
                }

                memberAssignments.Add(
                    Expression.Bind(resultClrType.GetProperty(AggregationPropertiesContainerProperty),
                    TestAggregationPropertyContainer.CreateNextNamedPropertyContainer(aggregationProperties)));
            }

            return Expression.Lambda(
                Expression.MemberInit(Expression.New(resultClrType), memberAssignments),
                groupingParam);
        }
        
        public virtual AggregationFlatteningResult FlattenReferencedProperties(
            TransformationNode transformationNode,
            IQueryable query,
            QueryBinderContext context)
        {
            if (transformationNode == null)
            {
                throw Error.ArgumentNull(nameof(transformationNode));
            }

            if (query == null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IEnumerable<GroupByPropertyNode> groupingProperties = null;
            if (transformationNode.Kind == TransformationNodeKind.GroupBy)
            {
                groupingProperties = (transformationNode as GroupByTransformationNode)?.GroupingProperties;
            }

            // Aggregate expressions to flatten - excludes VirtualPropertyCount
            List<AggregateExpression> aggregateExpressions = GetAggregateExpressions(transformationNode, context)?.OfType<AggregateExpression>()
                .Where(e => e.Method != AggregationMethod.VirtualPropertyCount).ToList();

            if ((aggregateExpressions?.Count ?? 0) == 0 || !(groupingProperties?.Any() == true))
            {
                return null;
            }

            Type wrapperType = typeof(TestFlatteningWrapper<>).MakeGenericType(context.TransformationElementType);
            PropertyInfo sourceProperty = wrapperType.GetProperty(QueryConstants.FlatteningWrapperSourceProperty);
            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>
            {
                Expression.Bind(sourceProperty, context.CurrentParameter)
            };

            // Generated Select will be stack-like; meaning that first property in the list will be deepest one
            // For example if we add $it.B.C, $it.B.D, Select will look like
            // new {
            //      Value = $it.B.C
            //      Next = new {
            //          Value = $it.B.D
            //      }
            // }

            // We are generating references (in currentContainerExpr) from the beginning of the Select ($it.Value, then $it.Next.Value etc.)
            // We have proper match we need insert properties in reverse order
            // After this,
            //     properties = { $it.B.D, $it.B.C }
            //     PreFlattenedMap = { {$it.B.C, $it.Value}, {$it.B.D, $it.Next.Value} }

            int aliasIdx = aggregateExpressions.Count - 1;
            NamedPropertyExpression[] properties = new NamedPropertyExpression[aggregateExpressions.Count];

            AggregationFlatteningResult flatteningResult = new AggregationFlatteningResult
            {
                RedefinedContextParameter = Expression.Parameter(wrapperType, "$it"),
                FlattenedPropertiesMapping = new Dictionary<SingleValueNode, Expression>(aggregateExpressions.Count)
            };

            MemberExpression containerExpr = Expression.Property(flatteningResult.RedefinedContextParameter, GroupingPropertiesContainerProperty);

            for (int i = 0; i < aggregateExpressions.Count; i++)
            {
                AggregateExpression aggregateExpression = aggregateExpressions[i];

                string alias = string.Concat("Property", aliasIdx.ToString(CultureInfo.CurrentCulture)); // We just need unique alias, we aren't going to use it

                // Add Value = $it.B.C
                Expression propertyAccessExpression = BindAccessExpression(aggregateExpression.Expression, context);
                Type type = propertyAccessExpression.Type;
                propertyAccessExpression = WrapConvert(propertyAccessExpression);
                properties[aliasIdx] = new NamedPropertyExpression(Expression.Constant(alias), propertyAccessExpression);

                // Save $it.Container.Next.Value for future use
                UnaryExpression flattenedAccessExpression = Expression.Convert(
                    Expression.Property(containerExpr, "Value"),
                    type);
                containerExpr = Expression.Property(containerExpr, "Next");
                flatteningResult.FlattenedPropertiesMapping.Add(aggregateExpression.Expression, flattenedAccessExpression);
                aliasIdx--;
            }

            PropertyInfo wrapperProperty = typeof(TestGroupByWrapper).GetProperty(GroupingPropertiesContainerProperty);

            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, TestAggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

            flatteningResult.FlattenedExpression = Expression.Lambda(Expression.MemberInit(Expression.New(wrapperType), wrapperTypeMemberAssignments), context.CurrentParameter);

            return flatteningResult;
        }

        private IList<NamedPropertyExpression> CreateGroupByMemberAssignments(IEnumerable<GroupByPropertyNode> groupByPropertyNodes, QueryBinderContext context)
        {
            var namedProperties = new List<NamedPropertyExpression>();

            foreach (var groupByPropertyNode in groupByPropertyNodes)
            {
                if (groupByPropertyNode.Expression != null)
                {
                    namedProperties.Add(new NamedPropertyExpression(
                        Expression.Constant(groupByPropertyNode.Name),
                        WrapConvert(BindAccessExpression(groupByPropertyNode.Expression, context))));
                }
                else
                {
                    var memberAssignments = new List<MemberAssignment>(capacity: 1)
                    {
                        Expression.Bind(
                            typeof(TestGroupByWrapper).GetProperty(GroupingPropertiesContainerProperty),
                            TestAggregationPropertyContainer.CreateNextNamedPropertyContainer(
                                CreateGroupByMemberAssignments(groupByPropertyNode.ChildTransformations, context)))
                    };

                    namedProperties.Add(new NamedPropertyExpression(
                        Expression.Constant(groupByPropertyNode.Name),
                        Expression.MemberInit(Expression.New(typeof(TestGroupByWrapper)), memberAssignments)));
                }
            }

            return namedProperties;
        }

        private Expression CreatePropertyAggregateExpression(ParameterExpression groupingParam, AggregateExpression aggregateExpr, Type baseType, QueryBinderContext context)
        {
            var queryableType = typeof(IEnumerable<>).MakeGenericType(baseType);
            var queryableExpr = Expression.Convert(groupingParam, queryableType);

            if (aggregateExpr.Method == AggregationMethod.VirtualPropertyCount)
            {
                MethodInfo countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(baseType);
                return WrapConvert(Expression.Call(null, countMethod, queryableExpr));
            }

            var lambdaParam = baseType == context.TransformationElementType ? context.CurrentParameter : Expression.Parameter(baseType, "$it");
            if (!(context.FlattenedExpressionMapping?.TryGetValue(aggregateExpr.Expression, out Expression body) == true))
            {
                body = BindAccessExpression(aggregateExpr.Expression, context, lambdaParam);
            }

            var propertyLambda = Expression.Lambda(body, lambdaParam);

            Expression propertyAggregateExpr;

            switch (aggregateExpr.Method)
            {
                case AggregationMethod.Min:
                    var minMethod = ExpressionHelperMethods.EnumerableMin.MakeGenericMethod(baseType, propertyLambda.Body.Type);
                    propertyAggregateExpr = Expression.Call(null, minMethod, queryableExpr, propertyLambda);

                    break;
                case AggregationMethod.Max:
                    var maxMethod = ExpressionHelperMethods.EnumerableMax.MakeGenericMethod(baseType, propertyLambda.Body.Type);
                    propertyAggregateExpr = Expression.Call(null, maxMethod, queryableExpr, propertyLambda);

                    break;
                case AggregationMethod.Sum:
                    {
                        // For dynamic properties, cast dynamic to decimal
                        var propertyDynamicCastExpr = WrapDynamicCastIfNeeded(body);
                        propertyLambda = Expression.Lambda(propertyDynamicCastExpr, lambdaParam);

                        if (!ExpressionHelperMethods.EnumerableSumGenerics.TryGetValue(propertyDynamicCastExpr.Type, out MethodInfo sumGenericMethod))
                        {
                            throw new NotSupportedException(
                                $"Aggregation '{aggregateExpr.Method}' not supported for property '{aggregateExpr.Expression}' of type '{propertyDynamicCastExpr.Type}'.");
                        }

                        var sumMethod = sumGenericMethod.MakeGenericMethod(baseType);
                        propertyAggregateExpr = Expression.Call(null, sumMethod, queryableExpr, propertyLambda);

                        // For dynamic properties, cast dynamic to back to object
                        if (propertyLambda.Type == typeof(object))
                        {
                            propertyAggregateExpr = Expression.Convert(propertyAggregateExpr, typeof(object));
                        }
                    }

                    break;
                case AggregationMethod.Average:
                    {
                        // For dynamic properties, cast dynamic to decimal
                        var propertyDynamicCastExpr = WrapDynamicCastIfNeeded(body);
                        propertyLambda = Expression.Lambda(propertyDynamicCastExpr, lambdaParam);

                        if (!ExpressionHelperMethods.EnumerableAverageGenerics.TryGetValue(propertyDynamicCastExpr.Type, out MethodInfo averageGenericMethod))
                        {
                            throw new NotSupportedException(
                                $"Aggregation '{aggregateExpr.Method}' not supported for property '{aggregateExpr.Expression}' of type '{propertyDynamicCastExpr.Type}'.");
                        }

                        var averageMethod = averageGenericMethod.MakeGenericMethod(baseType);
                        propertyAggregateExpr = Expression.Call(null, averageMethod, queryableExpr, propertyLambda);

                        // For dynamic properties, cast dynamic to back to object
                        if (propertyLambda.Type == typeof(object))
                        {
                            propertyAggregateExpr = Expression.Convert(propertyAggregateExpr, typeof(object));
                        }
                    }

                    break;
                case AggregationMethod.CountDistinct:
                    {
                        MethodInfo selectMethod = ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(
                            context.TransformationElementType,
                            propertyLambda.Body.Type);

                        Expression queryableSelectExpr = Expression.Call(null, selectMethod, queryableExpr, propertyLambda);

                        // Run distinct over the set of items
                        MethodInfo distinctMethod = ExpressionHelperMethods.EnumerableDistinct.MakeGenericMethod(propertyLambda.Body.Type);
                        Expression distinctExpr = Expression.Call(null, distinctMethod, queryableSelectExpr);

                        // Count the distinct items as the aggregation expression
                        MethodInfo countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(propertyLambda.Body.Type);
                        propertyAggregateExpr = Expression.Call(null, countMethod, distinctExpr);
                    }

                    break;
                case AggregationMethod.Custom:
                    {
                        var customMethod = GetCustomMethod(aggregateExpr, context);
                        var selectMethod = ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(context.TransformationElementType, propertyLambda.Body.Type);
                        var queryableSelectExpr = Expression.Call(null, selectMethod, queryableExpr, propertyLambda);
                        propertyAggregateExpr = Expression.Call(null, customMethod, queryableSelectExpr);
                    }

                    break;
                default:
                    throw new NotSupportedException($"{aggregateExpr.Method} method not supported");
            }

            return WrapConvert(propertyAggregateExpr);
        }

        private static Expression WrapDynamicCastIfNeeded(Expression propertyAccessExpr)
        {
            if (propertyAccessExpr.Type == typeof(object))
            {
                return Expression.Call(null, ExpressionHelperMethods.ConvertToDecimal, propertyAccessExpr);
            }

            return propertyAccessExpr;
        }

        /// <summary>
        /// Gets a collection of <see cref="AggregateExpressionBase"/> from a <see cref="TransformationNode"/>.
        /// </summary>
        /// <param name="context">The query binder context.</param>
        /// <param name="transformationNode">The <see cref="TransformationNode"/>.</param>
        /// <returns>A collection of aggregate expressions.</returns>
        private IEnumerable<AggregateExpressionBase> GetAggregateExpressions(TransformationNode transformationNode, QueryBinderContext context)
        {
            Contract.Assert(transformationNode != null);
            Contract.Assert(context != null);

            IEnumerable<AggregateExpressionBase> aggregateExpressions = null;

            switch (transformationNode.Kind)
            {
                case TransformationNodeKind.Aggregate:
                    AggregateTransformationNode aggregateClause = transformationNode as AggregateTransformationNode;
                    return aggregateClause.AggregateExpressions.Select(exp =>
                    {
                        AggregateExpression aggregationExpr = exp as AggregateExpression;

                        return aggregationExpr?.Method == AggregationMethod.Custom ? FixCustomMethodReturnType(aggregationExpr, context) : exp;
                    });

                case TransformationNodeKind.GroupBy:
                    GroupByTransformationNode groupByClause = transformationNode as GroupByTransformationNode;
                    if (groupByClause.ChildTransformations != null)
                    {
                        if (groupByClause.ChildTransformations.Kind == TransformationNodeKind.Aggregate)
                        {
                            AggregateTransformationNode aggregationNode = groupByClause.ChildTransformations as AggregateTransformationNode;
                            return aggregationNode.AggregateExpressions.Select(exp =>
                            {
                                AggregateExpression aggregationExpr = exp as AggregateExpression;

                                return aggregationExpr?.Method == AggregationMethod.Custom ? FixCustomMethodReturnType(aggregationExpr, context) : exp;
                            });
                        }
                        else
                        {
                            throw new NotSupportedException(
                                $"Transformation kind '{groupByClause.ChildTransformations.Kind}' is not supported as a child transformation of kind '{transformationNode.Kind}'");
                        }
                    }

                    break;

                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        SRResources.NotSupportedTransformationKind,
                        transformationNode.Kind));
            }

            return aggregateExpressions;
        }

        /// <summary>
        /// Fixes return type for custom aggregation method.
        /// </summary>
        /// <param name="aggregationExpression">The aggregation expression</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The </returns>
        private AggregateExpression FixCustomMethodReturnType(AggregateExpression aggregationExpression, QueryBinderContext context)
        {
            Debug.Assert(aggregationExpression != null, $"{nameof(aggregationExpression)} != null");
            Debug.Assert(aggregationExpression.Method == AggregationMethod.Custom, $"{nameof(aggregationExpression)}.Method == {nameof(AggregationMethod.Custom)}");

            MethodInfo customMethod = GetCustomMethod(aggregationExpression, context);

            IEdmPrimitiveTypeReference typeReference = context.Model.GetEdmPrimitiveTypeReference(customMethod.ReturnType);

            return new AggregateExpression(aggregationExpression.Expression, aggregationExpression.MethodDefinition, aggregationExpression.Alias, typeReference);
        }

        /// <summary>
        /// Gets a custom aggregation method for the aggregation expression.
        /// </summary>
        /// <param name="aggregationExpression">The aggregation expression.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The custom method.</returns>
        private MethodInfo GetCustomMethod(AggregateExpression aggregationExpression, QueryBinderContext context)
        {
            LambdaExpression propertyLambda = Expression.Lambda(BindAccessExpression(aggregationExpression.Expression, context), context.CurrentParameter);
            Type inputType = propertyLambda.Body.Type;

            string methodToken = aggregationExpression.MethodDefinition.MethodLabel;
            CustomAggregateMethodAnnotation customMethodAnnotations = context.Model.GetAnnotationValue<CustomAggregateMethodAnnotation>(context.Model);

            MethodInfo customMethod;
            if (!customMethodAnnotations.GetMethodInfo(methodToken, inputType, out customMethod))
            {
                throw new ODataException(Error.Format(
                    SRResources.AggregationNotSupportedForType,
                    aggregationExpression.Method,
                    aggregationExpression.Expression,
                    inputType));
            }

            return customMethod;
        }
    }
}
