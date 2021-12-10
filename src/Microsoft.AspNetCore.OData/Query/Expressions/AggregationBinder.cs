//-----------------------------------------------------------------------------
// <copyright file="AggregationBinder.cs" company=".NET Foundation">
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
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// The default implementation to bind an OData $apply represented by <see cref="ApplyClause"/> to an <see cref="Expression"/>.
    /// </summary>
    public class AggregationBinder : QueryBinder, IAggregationBinder
    {
        /// <inheritdoc/>
        public virtual Expression BindGroupBy(TransformationNode transformationNode, QueryBinderContext context)
        {
            if (transformationNode == null)
            {
                throw Error.ArgumentNull(nameof(transformationNode));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            LambdaExpression groupByLambda = null;
            IEnumerable<GroupByPropertyNode> groupingProperties = GetGroupingProperties(transformationNode);

            if (groupingProperties?.Any() == true)
            {
                // Generates the expression:
                // $it => new DynamicTypeWrapper() {
                //     GroupByContainer => new AggregationPropertyContainer() {
                //         Name = "Prop1",
                //         Value = $it.Prop1,
                //         Next = new AggregationPropertyContainer() {
                //             Name = "Prop2",
                //             Value = $it.Prop2, // int
                //             Next = new LastInChain() {
                //                 Name = "Prop3",
                //                 Value = $it.Prop3
                //             }
                //         }
                //     }
                // }

                List<NamedPropertyExpression> properties = CreateGroupByMemberAssignments(groupingProperties, context);

                PropertyInfo wrapperProperty = typeof(GroupByWrapper).GetProperty(QueryConstants.GroupByWrapperGroupByContainerProperty);
                List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>(capacity: 1)
                {
                    Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties))
                };

                groupByLambda = Expression.Lambda(
                    Expression.MemberInit(Expression.New(typeof(GroupByWrapper)), wrapperTypeMemberAssignments),
                    context.CurrentParameter);
            }
            else
            {
                // No GroupBy properties
                // .GroupBy($it => new NoGroupByWrapper())
                groupByLambda = Expression.Lambda(Expression.New(typeof(NoGroupByWrapper)), context.CurrentParameter);
            }

            return groupByLambda;
        }

        /// <inheritdoc/>
        public virtual Expression BindSelect(TransformationNode transformationNode, QueryBinderContext context)
        {
            if (transformationNode == null)
            {
                throw Error.ArgumentNull(nameof(transformationNode));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // Generates the expression:
            // $it => new DynamicTypeWrapper() {
            //     GroupByContainer = $it.Key.GroupByContainer // If groupby section present
            //     Container => new AggregationPropertyContainer() {
            //         Name = "Alias1",
            //         Value = $it.AsQueryable().Sum(i => i.AggregatableProperty),
            //         Next = new LastInChain() {
            //             Name = "Alias2",
            //             Value = $it.AsQueryable().Sum(i => i.AggregatableProperty)
            //         }
            //     }
            // }

            Type groupByClrType = transformationNode.Kind == TransformationNodeKind.GroupBy ? typeof(GroupByWrapper) : typeof(NoGroupByWrapper);
            Type groupingType = typeof(IGrouping<,>).MakeGenericType(groupByClrType, context.TransformationElementType);
            Type resultClrType = transformationNode.Kind == TransformationNodeKind.Aggregate ? typeof(NoGroupByAggregationWrapper) : typeof(AggregationWrapper);
            ParameterExpression groupingParameter = Expression.Parameter(groupingType, "$it");

            IEnumerable<AggregateExpressionBase> aggregateExpressions = GetAggregateExpressions(transformationNode, context);
            IEnumerable<GroupByPropertyNode> groupingProperties = GetGroupingProperties(transformationNode);

            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            // Setting GroupByContainer property when we have GroupBy properties
            if (groupingProperties?.Any() == true)
            {
                PropertyInfo wrapperProperty = resultClrType.GetProperty(QueryConstants.GroupByWrapperGroupByContainerProperty);

                wrapperTypeMemberAssignments.Add(
                    Expression.Bind(wrapperProperty,
                    Expression.Property(Expression.Property(groupingParameter, "Key"), QueryConstants.GroupByWrapperGroupByContainerProperty)));
            }

            // Setting Container property when we have aggregation clauses
            if (aggregateExpressions != null)
            {
                List<NamedPropertyExpression> properties = new List<NamedPropertyExpression>();
                foreach (AggregateExpressionBase aggregateExpression in aggregateExpressions)
                {
                    properties.Add(new NamedPropertyExpression(
                        Expression.Constant(aggregateExpression.Alias),
                        CreateAggregateExpression(groupingParameter, aggregateExpression, context.TransformationElementType, context)));
                }

                PropertyInfo wrapperProperty = resultClrType.GetProperty(QueryConstants.GroupByWrapperContainerProperty);
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));
            }

            MemberInitExpression body = Expression.MemberInit(
                Expression.New(resultClrType),
                wrapperTypeMemberAssignments);
            
            return Expression.Lambda(body, groupingParameter);
        }

        /// <inheritdoc/>
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

            IEnumerable<GroupByPropertyNode> groupingProperties = GetGroupingProperties(transformationNode);
            // Aggregate expressions to flatten - excludes VirtualPropertyCount ($count)
            List<AggregateExpression> aggregateExpressions = GetAggregateExpressions(transformationNode, context)?.OfType<AggregateExpression>()
                .Where(e => e.Method != AggregationMethod.VirtualPropertyCount).ToList();

            if ((aggregateExpressions?.Count ?? 0) == 0 || groupingProperties?.Any() != true)
            {
                return null;
            }

            Type wrapperType = typeof(FlatteningWrapper<>).MakeGenericType(context.TransformationElementType);
            PropertyInfo sourceProperty = wrapperType.GetProperty(QueryConstants.FlatteningWrapperSourceProperty);
            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>
            {
                Expression.Bind(sourceProperty, context.CurrentParameter)
            };

            // Generated Select will be stack-like; meaning that first property in the list will be deepest one
            // For example if we add $it.B.C, $it.B.D, Select will look like
            // $it => new FlatteningWrapper() {
            //     Source = $it,
            //     Container = new {
            //         Value = $it.B.C
            //         Next = new {
            //             Value = $it.B.D
            //         }
            //     }
            // }

            // We are generating references (in containerExpression) from the beginning of the Select ($it.Value, then $it.Next.Value etc.)
            // We have proper match we need insert properties in reverse order

            int aliasIdx = aggregateExpressions.Count - 1;
            NamedPropertyExpression[] properties = new NamedPropertyExpression[aggregateExpressions.Count];

            AggregationFlatteningResult flatteningResult = new AggregationFlatteningResult
            {
                RedefinedContextParameter = Expression.Parameter(wrapperType, "$it"),
                FlattenedPropertiesMapping = new Dictionary<SingleValueNode, Expression>(aggregateExpressions.Count)
            };

            MemberExpression containerExpression = Expression.Property(flatteningResult.RedefinedContextParameter, QueryConstants.GroupByWrapperGroupByContainerProperty);

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
                    Expression.Property(
                        // Convert necessary because the Value property is declared on inherited NamedProperty<T> class
                        Expression.Convert(containerExpression, typeof(AggregationPropertyContainer)),
                        QueryConstants.AggregationPropertyContainerValueProperty),
                    type);
                containerExpression = Expression.Property(containerExpression, QueryConstants.AggregationPropertyContainerNextProperty);
                flatteningResult.FlattenedPropertiesMapping.Add(aggregateExpression.Expression, flattenedAccessExpression);
                aliasIdx--;
            }

            PropertyInfo wrapperProperty = wrapperType.GetProperty(QueryConstants.GroupByWrapperGroupByContainerProperty);

            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

            flatteningResult.FlattenedExpression = Expression.Lambda(Expression.MemberInit(Expression.New(wrapperType), wrapperTypeMemberAssignments), context.CurrentParameter);

            return flatteningResult;
        }

        private static Expression WrapDynamicCastIfNeeded(Expression propertyAccessExpression)
        {
            if (propertyAccessExpression.Type == typeof(object))
            {
                return Expression.Call(null, ExpressionHelperMethods.ConvertToDecimal, propertyAccessExpression);
            }

            return propertyAccessExpression;
        }

        /// <summary>
        /// Creates an expression for an aggregate.
        /// </summary>
        /// <param name="groupingParameter">The parameter representing the group.</param>
        /// <param name="aggregateExpression">The aggregate expression.</param>
        /// <param name="baseType">The element type at the base of the transformation.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>An expression representing the aggregate.</returns>
        /// <exception cref="ODataException"></exception>
        private Expression CreateAggregateExpression(ParameterExpression groupingParameter, AggregateExpressionBase aggregateExpression, Type baseType, QueryBinderContext context)
        {
            switch (aggregateExpression.AggregateKind)
            {
                case AggregateExpressionKind.PropertyAggregate:
                    return CreatePropertyAggregateExpression(groupingParameter, aggregateExpression as AggregateExpression, baseType, context);
                case AggregateExpressionKind.EntitySetAggregate:
                    return CreateEntitySetAggregateExpression(groupingParameter, aggregateExpression as EntitySetAggregateExpression, baseType, context);
                default:
                    throw new ODataException(Error.Format(SRResources.AggregateKindNotSupported, aggregateExpression.AggregateKind));
            }
        }

        /// <summary>
        /// Creates an expression for an entity set aggregate.
        /// </summary>
        /// <param name="groupingParameter">The parameter representing the group.</param>
        /// <param name="entitySetAggregateExpression">The entity set aggregate expression.</param>
        /// <param name="baseType">The element type at the base of the transformation.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>An expression for the entity set aggregate.</returns>
        /// <remarks>
        /// Generates an expression similar to:
        /// <code>
        ///  $it => $it.AsQueryable()
        ///      .SelectMany($it => $it.SomeEntitySet)
        ///      .GroupBy($gr => new Object())
        ///      .Select($p => new DynamicTypeWrapper() {
        ///          Container = new AggregationPropertyContainer() {
        ///              Name = "Alias1",
        ///              Value = $it.AsQueryable().AggregateMethod1($it => $it.SomePropertyOfSomeEntitySet),
        ///              Next = new LastInChain() {
        ///                  Name = "Alias2",
        ///                  Value = $p.AsQueryable().AggregateMethod2($it => $it.AnotherPropertyOfSomeEntitySet)
        ///              }
        ///          }
        ///      })
        /// </code>
        /// </remarks>
        private Expression CreateEntitySetAggregateExpression(
            ParameterExpression groupingParameter,
            EntitySetAggregateExpression entitySetAggregateExpression,
            Type baseType,
            QueryBinderContext context)
        {
            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();
            MethodInfo asQueryableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(baseType);
            Expression asQueryableExpression = Expression.Call(null, asQueryableMethod, groupingParameter);

            // Create lambda to access the entity set from expression
            Expression source = BindAccessExpression(entitySetAggregateExpression.Expression.Source, context);
            string propertyName = context.Model.GetClrPropertyName(entitySetAggregateExpression.Expression.NavigationProperty);

            MemberExpression property = Expression.Property(source, propertyName);

            Type baseElementType = source.Type;
            Type selectedElementType = property.Type.GenericTypeArguments.Single();

            // Create method to get property collections to aggregate
            MethodInfo selectManyMethod = ExpressionHelperMethods.EnumerableSelectManyGeneric.MakeGenericMethod(baseElementType, selectedElementType);

            // Create the lambda that access the property in the SelectMany clause.
            ParameterExpression selectManyParam = Expression.Parameter(baseElementType, "$it");
            MemberExpression propertyExpression = Expression.Property(selectManyParam, entitySetAggregateExpression.Expression.NavigationProperty.Name);

            // Collection selector body is IQueryable, we need to adjust the type to IEnumerable, to match the SelectMany signature
            // therefore the delegate type is specified explicitly
            Type collectionSelectorLambdaType = typeof(Func<,>).MakeGenericType(
                source.Type,
                typeof(IEnumerable<>).MakeGenericType(selectedElementType));
            LambdaExpression selectManyLambda = Expression.Lambda(collectionSelectorLambdaType, propertyExpression, selectManyParam);

            // Get expression to get collection of entities
            MethodCallExpression entitySet = Expression.Call(null, selectManyMethod, asQueryableExpression, selectManyLambda);

            // Getting method and lambda expression of groupBy
            Type groupKeyType = typeof(object);
            MethodInfo groupByMethod =
                ExpressionHelperMethods.EnumerableGroupByGeneric.MakeGenericMethod(selectedElementType, groupKeyType);
            LambdaExpression groupByLambda = Expression.Lambda(
                Expression.New(groupKeyType),
                Expression.Parameter(selectedElementType, "$gr"));

            // Group entities in a single group to apply select
            MethodCallExpression groupedEntitySet = Expression.Call(null, groupByMethod, entitySet, groupByLambda);

            Type groupingType = typeof(IGrouping<,>).MakeGenericType(groupKeyType, selectedElementType);
            ParameterExpression innerGroupingParameter = Expression.Parameter(groupingType, "$p");

            // Nested properties
            // Create dynamicTypeWrapper to encapsulate the aggregate result
            List<NamedPropertyExpression> properties = new List<NamedPropertyExpression>();
            foreach (AggregateExpressionBase aggregateExpression in entitySetAggregateExpression.Children)
            {
                properties.Add(new NamedPropertyExpression(
                    Expression.Constant(aggregateExpression.Alias),
                    CreateAggregateExpression(innerGroupingParameter, aggregateExpression, selectedElementType, context)));
            }

            Type nestedResultType = typeof(EntitySetAggregationWrapper);
            PropertyInfo wrapperProperty = nestedResultType.GetProperty(QueryConstants.GroupByWrapperContainerProperty);
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

            MemberInitExpression initializedMember = Expression.MemberInit(Expression.New(nestedResultType), wrapperTypeMemberAssignments);
            LambdaExpression selectLambda = Expression.Lambda(initializedMember, innerGroupingParameter);

            // Get select method
            MethodInfo selectMethod = ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(
                groupingType,
                selectLambda.Body.Type);

            return Expression.Call(null, selectMethod, groupedEntitySet, selectLambda);
        }

        /// <summary>
        /// Creates an expression for a property aggregate.
        /// </summary>
        /// <param name="groupingParameter">The parameter representing the group.</param>
        /// <param name="aggregateExpression">The aggregate expression.</param>
        /// <param name="baseType">The element type at the base of the transformation.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>An expression for a property aggregate.</returns>
        /// <remarks>
        /// Generates an expression similar to:
        /// <code>
        ///  $it => $it.AsQueryable().Select($it => $it.SomeProperty).AggregateMethod()
        /// </code>
        /// Example:
        /// <code>
        /// $it => $it.AsQueryable().Sum($it => $it.SomeProperty)
        /// </code>
        /// If the aggregation method is <see cref="AggregationMethod.Custom"/>, the method uses a custom aggregation function provided by the caller.
        /// </remarks>
        private Expression CreatePropertyAggregateExpression(
            ParameterExpression groupingParameter,
            AggregateExpression aggregateExpression,
            Type baseType,
            QueryBinderContext context)
        {
            // groupingParameter type is IGrouping<,baseType> that implements IEnumerable<baseType> 
            // we need cast it to IEnumerable<baseType> during expression building (IEnumerable)$it
            Type queryableType = typeof(IEnumerable<>).MakeGenericType(baseType);
            Expression queryableExpression = Expression.Convert(groupingParameter, queryableType);

            // $count is a virtual property, so there's no propertyLambda to create.
            if (aggregateExpression.Method == AggregationMethod.VirtualPropertyCount)
            {
                MethodInfo countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(baseType);
                return WrapConvert(Expression.Call(null, countMethod, queryableExpression));
            }

            ParameterExpression lambdaParameter = baseType == context.TransformationElementType ? context.CurrentParameter : Expression.Parameter(baseType, "$it");
            if (!(context.FlattenedExpressionMapping?.TryGetValue(aggregateExpression.Expression, out Expression body) == true))
            {
                body = BindAccessExpression(aggregateExpression.Expression, context, lambdaParameter);
            }

            LambdaExpression propertyLambda = Expression.Lambda(body, lambdaParameter);

            Expression propertyAggregateExpression;

            switch (aggregateExpression.Method)
            {
                case AggregationMethod.Min:
                    {
                        MethodInfo minMethod = ExpressionHelperMethods.EnumerableMin.MakeGenericMethod(baseType, propertyLambda.Body.Type);
                        propertyAggregateExpression = Expression.Call(null, minMethod, queryableExpression, propertyLambda);
                    }

                    break;

                case AggregationMethod.Max:
                    {
                        MethodInfo maxMethod = ExpressionHelperMethods.EnumerableMax.MakeGenericMethod(baseType, propertyLambda.Body.Type);
                        propertyAggregateExpression = Expression.Call(null, maxMethod, queryableExpression, propertyLambda);
                    }

                    break;

                case AggregationMethod.Sum:
                    {
                        // For dynamic properties, cast to decimal
                        Expression propertyExpression = WrapDynamicCastIfNeeded(body);
                        propertyLambda = Expression.Lambda(propertyExpression, lambdaParameter);

                        if (!ExpressionHelperMethods.EnumerableSumGenerics.TryGetValue(propertyExpression.Type, out MethodInfo sumGenericMethod))
                        {
                            throw new ODataException(Error.Format(
                                SRResources.AggregationNotSupportedForType,
                                aggregateExpression.Method,
                                aggregateExpression.Expression,
                                propertyExpression.Type));
                        }

                        MethodInfo sumMethod = sumGenericMethod.MakeGenericMethod(baseType);
                        propertyAggregateExpression = Expression.Call(null, sumMethod, queryableExpression, propertyLambda);

                        // For dynamic properties, cast back to object
                        if (body.Type == typeof(object))
                        {
                            propertyAggregateExpression = Expression.Convert(propertyAggregateExpression, typeof(object));
                        }
                    }

                    break;

                case AggregationMethod.Average:
                    {
                        // For dynamic properties, cast dynamic to decimal
                        Expression propertyExpression = WrapDynamicCastIfNeeded(body);
                        propertyLambda = Expression.Lambda(propertyExpression, lambdaParameter);

                        if (!ExpressionHelperMethods.EnumerableAverageGenerics.TryGetValue(propertyExpression.Type, out MethodInfo averageGenericMethod))
                        {
                            throw new ODataException(Error.Format(
                                SRResources.AggregationNotSupportedForType,
                                aggregateExpression.Method,
                                aggregateExpression.Expression,
                                propertyExpression.Type));
                        }

                        MethodInfo averageMethod = averageGenericMethod.MakeGenericMethod(baseType);
                        propertyAggregateExpression = Expression.Call(null, averageMethod, queryableExpression, propertyLambda);

                        // For dynamic properties, cast back to object
                        if (body.Type == typeof(object))
                        {
                            propertyAggregateExpression = Expression.Convert(propertyAggregateExpression, typeof(object));
                        }
                    }

                    break;

                case AggregationMethod.CountDistinct:
                    {
                        MethodInfo selectMethod = ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(
                            context.TransformationElementType,
                            propertyLambda.Body.Type);

                        Expression queryableSelectExpression = Expression.Call(null, selectMethod, queryableExpression, propertyLambda);

                        // Expression to get distinct items
                        MethodInfo distinctMethod = ExpressionHelperMethods.EnumerableDistinct.MakeGenericMethod(propertyLambda.Body.Type);
                        Expression distinctExpression = Expression.Call(null, distinctMethod, queryableSelectExpression);

                        // Expression to get count of distinct items
                        MethodInfo countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(propertyLambda.Body.Type);
                        propertyAggregateExpression = Expression.Call(null, countMethod, distinctExpression);
                    }

                    break;

                case AggregationMethod.Custom:
                    {
                        MethodInfo customMethod = GetCustomMethod(aggregateExpression, context);
                        MethodInfo selectMethod = ExpressionHelperMethods.EnumerableSelectGeneric
                            .MakeGenericMethod(context.TransformationElementType, propertyLambda.Body.Type);
                        MethodCallExpression queryableSelectExpression = Expression.Call(null, selectMethod, queryableExpression, propertyLambda);
                        propertyAggregateExpression = Expression.Call(null, customMethod, queryableSelectExpression);
                    }

                    break;

                default:
                    throw new ODataException(Error.Format(SRResources.AggregationMethodNotSupported, aggregateExpression.Method));
            }

            return WrapConvert(propertyAggregateExpression);
        }

        /// <summary>
        /// Creates a list of <see cref="NamedPropertyExpression"/> from a collection of <see cref="GroupByPropertyNode"/>.
        /// </summary>
        /// <param name="groupByNodes">GroupBy nodes.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>A list of <see cref="NamedPropertyExpression"/> representing properties in the GroupBy clause.</returns>
        private List<NamedPropertyExpression> CreateGroupByMemberAssignments(IEnumerable<GroupByPropertyNode> groupByNodes, QueryBinderContext context)
        {
            List<NamedPropertyExpression> properties = new List<NamedPropertyExpression>();

            foreach (GroupByPropertyNode groupByNode in groupByNodes)
            {
                string propertyName = groupByNode.Name;

                if (groupByNode.Expression != null)
                {
                    properties.Add(new NamedPropertyExpression(
                        Expression.Constant(propertyName),
                        WrapConvert(BindAccessExpression(groupByNode.Expression, context))));
                }
                else
                {
                    PropertyInfo wrapperProperty = typeof(GroupByWrapper).GetProperty(QueryConstants.GroupByWrapperGroupByContainerProperty);

                    List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>(capacity: 1)
                    {
                        Expression.Bind(
                            wrapperProperty,
                            AggregationPropertyContainer.CreateNextNamedPropertyContainer(
                                CreateGroupByMemberAssignments(groupByNode.ChildTransformations, context)))
                    };

                    properties.Add(new NamedPropertyExpression(
                        Expression.Constant(propertyName),
                        Expression.MemberInit(Expression.New(typeof(GroupByWrapper)), wrapperTypeMemberAssignments)));
                }
            }

            return properties;
        }

        /// <summary>
        /// Gets a collection of <see cref="GroupByPropertyNode"/> from a <see cref="TransformationNode"/>.
        /// </summary>
        /// <param name="transformationNode">The transformation node.</param>
        /// <returns>A collection of <see cref="GroupByPropertyNode"/>.</returns>
        private static IEnumerable<GroupByPropertyNode> GetGroupingProperties(TransformationNode transformationNode)
        {
            if (transformationNode.Kind == TransformationNodeKind.GroupBy)
            {
                GroupByTransformationNode groupByClause = transformationNode as GroupByTransformationNode;

                return groupByClause.GroupingProperties;
            }

            return null;
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
                    aggregateExpressions = FixCustomMethodReturnTypes(aggregateClause.AggregateExpressions, context);

                    break;

                case TransformationNodeKind.GroupBy:
                    GroupByTransformationNode groupByClause = transformationNode as GroupByTransformationNode;
                    if (groupByClause.ChildTransformations != null)
                    {
                        if (groupByClause.ChildTransformations.Kind == TransformationNodeKind.Aggregate)
                        {
                            AggregateTransformationNode aggregationNode = groupByClause.ChildTransformations as AggregateTransformationNode;
                            aggregateExpressions = FixCustomMethodReturnTypes(aggregationNode.AggregateExpressions, context);
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                SRResources.NotSupportedChildTransformationKind,
                                groupByClause.ChildTransformations.Kind,
                                transformationNode.Kind));
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
        /// Fixes return types for custom aggregation methods.
        /// </summary>
        /// <param name="aggregateExpressions">The aggregation expressions.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<AggregateExpressionBase> FixCustomMethodReturnTypes(IEnumerable<AggregateExpressionBase> aggregateExpressions, QueryBinderContext context)
        {
            return aggregateExpressions.Select(exp =>
            {
                AggregateExpression aggregationExpression = exp as AggregateExpression;

                return aggregationExpression?.Method == AggregationMethod.Custom ? FixCustomMethodReturnType(aggregationExpression, context) : exp;
            });
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
