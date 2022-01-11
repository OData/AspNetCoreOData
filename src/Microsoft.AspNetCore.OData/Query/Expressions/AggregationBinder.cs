//-----------------------------------------------------------------------------
// <copyright file="AggregationBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// The default implementation to bind an OData $apply represented by <see cref="ApplyClause"/> to a <see cref="Expression"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
    public class AggregationBinder : QueryBinder, IAggregationBinder
    {
        private const string GroupByContainerProperty = "GroupByContainer";

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

            LambdaExpression groupLambda = null;
            IEnumerable<GroupByPropertyNode> groupingProperties = GetGroupingProperties(transformationNode);
            if (groupingProperties != null && groupingProperties.Any())
            {
                // Generates expression
                // .GroupBy($it => new DynamicTypeWrapper()
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
                List<NamedPropertyExpression> properties = CreateGroupByMemberAssignments(groupingProperties, context);

                var wrapperProperty = typeof(GroupByWrapper).GetProperty(GroupByContainerProperty);
                List<MemberAssignment> wta = new List<MemberAssignment>();
                wta.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));
                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(typeof(GroupByWrapper)), wta), context.LambdaParameter);
            }
            else
            {
                // We do not have properties to aggregate
                // .GroupBy($it => new NoGroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(typeof(NoGroupByWrapper)), context.LambdaParameter);
            }

            return groupLambda;
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

            // Should return following expression
            // .Select($it => New DynamicType2()
            //                  {
            //                      GroupByContainer = $it.Key.GroupByContainer // If groupby section present
            //                      Container => new AggregationPropertyContainer() {
            //                          Name = "Alias1",
            //                          Value = $it.AsQuaryable().Sum(i => i.AggregatableProperty),
            //                          Next = new LastInChain() {
            //                              Name = "Alias2",
            //                              Value = $it.AsQuaryable().Sum(i => i.AggregatableProperty)
            //                          }
            //                      }
            //                  })
            var groupingType = typeof(IGrouping<,>).MakeGenericType(typeof(GroupByWrapper), context.TransformationElementType);
            ParameterExpression accum = Expression.Parameter(groupingType, "$it");
            Type resultClrType = transformationNode.Kind == TransformationNodeKind.Aggregate ? typeof(NoGroupByAggregationWrapper) : typeof(AggregationWrapper);
            IEnumerable<AggregateExpressionBase> aggregateExpressions = GetAggregateExpressions(context, transformationNode);
            IEnumerable<GroupByPropertyNode> groupingProperties = GetGroupingProperties(transformationNode);

            List <MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            // Setting GroupByContainer property when previous step was grouping
            if (groupingProperties != null && groupingProperties.Any())
            {
                var wrapperProperty = resultClrType.GetProperty(GroupByContainerProperty);

                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, Expression.Property(Expression.Property(accum, "Key"), GroupByContainerProperty)));
            }

            // Setting Container property when we have aggregation clauses
            if (aggregateExpressions != null)
            {
                var properties = new List<NamedPropertyExpression>();
                foreach (var aggExpression in aggregateExpressions)
                {
                    properties.Add(new NamedPropertyExpression(Expression.Constant(aggExpression.Alias), CreateAggregationExpression(accum, aggExpression, context.TransformationElementType, context)));
                }

                var wrapperProperty = resultClrType.GetProperty("Container");
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));
            }

            var initilizedMember =
                Expression.MemberInit(Expression.New(resultClrType), wrapperTypeMemberAssignments);
            var selectLambda = Expression.Lambda(initilizedMember, accum);

            return selectLambda;
        }

        private static Expression WrapDynamicCastIfNeeded(Expression propertyAccessor)
        {
            if (propertyAccessor.Type == typeof(object))
            {
                return Expression.Call(null, ExpressionHelperMethods.ConvertToDecimal, propertyAccessor);
            }

            return propertyAccessor;
        }

        private Expression CreateAggregationExpression(ParameterExpression accum, AggregateExpressionBase expression, Type baseType, QueryBinderContext context)
        {
            switch (expression.AggregateKind)
            {
                case AggregateExpressionKind.PropertyAggregate:
                    return CreatePropertyAggregateExpression(accum, expression as AggregateExpression, baseType, context);
                case AggregateExpressionKind.EntitySetAggregate:
                    return CreateEntitySetAggregateExpression(accum, expression as EntitySetAggregateExpression, baseType, context);
                default:
                    throw new ODataException(Error.Format(SRResources.AggregateKindNotSupported, expression.AggregateKind));
            }
        }

        private Expression CreateEntitySetAggregateExpression(
            ParameterExpression accum, EntitySetAggregateExpression expression, Type baseType, QueryBinderContext context)
        {
            // Should return following expression
            //  $it => $it.AsQueryable()
            //      .SelectMany($it => $it.SomeEntitySet)
            //      .GroupBy($gr => new Object())
            //      .Select($p => new DynamicTypeWrapper()
            //      {
            //          AliasOne = $p.AsQueryable().AggMethodOne($it => $it.SomePropertyOfSomeEntitySet),
            //          AliasTwo = $p.AsQueryable().AggMethodTwo($it => $it.AnotherPropertyOfSomeEntitySet),
            //          ...
            //          AliasN =  ... , // A nested expression of this same format.
            //          ...
            //      })

            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();
            var asQueryableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(baseType);
            Expression asQueryableExpression = Expression.Call(null, asQueryableMethod, accum);

            // Create lambda to access the entity set from expression
            var source = BindAccessor(expression.Expression.Source, context);
            string propertyName = context.Model.GetClrPropertyName(expression.Expression.NavigationProperty);

            var property = Expression.Property(source, propertyName);

            var baseElementType = source.Type;
            var selectedElementType = property.Type.GenericTypeArguments.Single();

            // Create method to get property collections to aggregate
            MethodInfo selectManyMethod
                = ExpressionHelperMethods.EnumerableSelectManyGeneric.MakeGenericMethod(baseElementType, selectedElementType);

            // Create the lambda that access the property in the selectMany clause.
            var selectManyParam = Expression.Parameter(baseElementType, "$it");
            var propertyExpression = Expression.Property(selectManyParam, expression.Expression.NavigationProperty.Name);

            // Collection selector body is IQueryable, we need to adjust the type to IEnumerable, to match the SelectMany signature
            // therefore the delegate type is specified explicitly
            var collectionSelectorLambdaType = typeof(Func<,>).MakeGenericType(
                source.Type,
                typeof(IEnumerable<>).MakeGenericType(selectedElementType));
            var selectManyLambda = Expression.Lambda(collectionSelectorLambdaType, propertyExpression, selectManyParam);

            // Get expression to get collection of entities
            var entitySet = Expression.Call(null, selectManyMethod, asQueryableExpression, selectManyLambda);

            // Getting method and lambda expression of groupBy
            var groupKeyType = typeof(object);
            MethodInfo groupByMethod =
                ExpressionHelperMethods.EnumerableGroupByGeneric.MakeGenericMethod(selectedElementType, groupKeyType);
            var groupByLambda = Expression.Lambda(
                Expression.New(groupKeyType),
                Expression.Parameter(selectedElementType, "$gr"));

            // Group entities in a single group to apply select
            var groupedEntitySet = Expression.Call(null, groupByMethod, entitySet, groupByLambda);

            var groupingType = typeof(IGrouping<,>).MakeGenericType(groupKeyType, selectedElementType);
            ParameterExpression innerAccum = Expression.Parameter(groupingType, "$p");

            // Nested properties
            // Create dynamicTypeWrapper to encapsulate the aggregate result
            var properties = new List<NamedPropertyExpression>();
            foreach (var aggExpression in expression.Children)
            {
                properties.Add(new NamedPropertyExpression(Expression.Constant(aggExpression.Alias), CreateAggregationExpression(innerAccum, aggExpression, selectedElementType, context)));
            }

            var nestedResultType = typeof(EntitySetAggregationWrapper);
            var wrapperProperty = nestedResultType.GetProperty("Container");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

            var initializedMember =
                Expression.MemberInit(Expression.New(nestedResultType), wrapperTypeMemberAssignments);
            var selectLambda = Expression.Lambda(initializedMember, innerAccum);

            // Get select method
            MethodInfo selectMethod =
                ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(
                    groupingType,
                    selectLambda.Body.Type);

            return Expression.Call(null, selectMethod, groupedEntitySet, selectLambda);
        }

        private Expression CreatePropertyAggregateExpression(ParameterExpression accum, AggregateExpression expression, Type baseType, QueryBinderContext context)
        {
            // accumulate type is IGrouping<,baseType> that implements IEnumerable<baseType> 
            // we need cast it to IEnumerable<baseType> during expression building (IEnumerable)$it
            // however for EF6 we need to use $it.AsQueryable() due to limitations in types of casts that will properly translated
            // UPDATE: We removed support for EF6
            Expression asQuerableExpression = null;

            var queryableType = typeof(IEnumerable<>).MakeGenericType(baseType);
            asQuerableExpression = Expression.Convert(accum, queryableType);

            // $count is a virtual property, so there's not a propertyLambda to create.
            if (expression.Method == AggregationMethod.VirtualPropertyCount)
            {
                MethodInfo countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(baseType);
                return WrapConvert(Expression.Call(null, countMethod, asQuerableExpression));
            }

            Expression body;

            var lambdaParameter = baseType == context.TransformationElementType ? context.LambdaParameter : Expression.Parameter(baseType, "$it");
            if (!context.PreFlattenedMap.TryGetValue(expression.Expression, out body))
            {
                body = BindAccessor(expression.Expression, context, lambdaParameter);
            }
            LambdaExpression propertyLambda = Expression.Lambda(body, lambdaParameter);

            Expression aggregationExpression;

            switch (expression.Method)
            {
                case AggregationMethod.Min:
                    {
                        var minMethod = ExpressionHelperMethods.EnumerableMin.MakeGenericMethod(baseType, propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, minMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationMethod.Max:
                    {
                        var maxMethod = ExpressionHelperMethods.EnumerableMax.MakeGenericMethod(baseType, propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, maxMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationMethod.Sum:
                    {
                        MethodInfo sumGenericMethod;
                        // For Dynamic properties cast to decimal
                        Expression propertyExpression = WrapDynamicCastIfNeeded(body);
                        propertyLambda = Expression.Lambda(propertyExpression, lambdaParameter);

                        if (!ExpressionHelperMethods.EnumerableSumGenerics.TryGetValue(propertyExpression.Type, out sumGenericMethod))
                        {
                            throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                                expression.Method, expression.Expression, propertyExpression.Type));
                        }

                        var sumMethod = sumGenericMethod.MakeGenericMethod(baseType);
                        aggregationExpression = Expression.Call(null, sumMethod, asQuerableExpression, propertyLambda);

                        // For Dynamic properties cast back to object
                        if (propertyLambda.Type == typeof(object))
                        {
                            aggregationExpression = Expression.Convert(aggregationExpression, typeof(object));
                        }
                    }
                    break;
                case AggregationMethod.Average:
                    {
                        MethodInfo averageGenericMethod;
                        // For Dynamic properties cast to decimal
                        Expression propertyExpression = WrapDynamicCastIfNeeded(body);
                        propertyLambda = Expression.Lambda(propertyExpression, lambdaParameter);

                        if (!ExpressionHelperMethods.EnumerableAverageGenerics.TryGetValue(propertyExpression.Type, out averageGenericMethod))
                        {
                            throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                                expression.Method, expression.Expression, propertyExpression.Type));
                        }

                        var averageMethod = averageGenericMethod.MakeGenericMethod(baseType);
                        aggregationExpression = Expression.Call(null, averageMethod, asQuerableExpression, propertyLambda);

                        // For Dynamic properties cast back to object
                        if (propertyLambda.Type == typeof(object))
                        {
                            aggregationExpression = Expression.Convert(aggregationExpression, typeof(object));
                        }
                    }
                    break;
                case AggregationMethod.CountDistinct:
                    {
                        var selectMethod = ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(context.TransformationElementType,
                                propertyLambda.Body.Type);
                        Expression queryableSelectExpression = Expression.Call(null, selectMethod, asQuerableExpression,
                            propertyLambda);

                        // I run distinct over the set of items
                        var distinctMethod = ExpressionHelperMethods.EnumerableDistinct.MakeGenericMethod(propertyLambda.Body.Type);
                        Expression distinctExpression = Expression.Call(null, distinctMethod, queryableSelectExpression);

                        // I count the distinct items as the aggregation expression
                        var countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, countMethod, distinctExpression);
                    }
                    break;
                case AggregationMethod.Custom:
                    {
                        MethodInfo customMethod = GetCustomMethod(expression, context);
                        var selectMethod = ExpressionHelperMethods.EnumerableSelectGeneric
                            .MakeGenericMethod(context.TransformationElementType, propertyLambda.Body.Type);
                        var selectExpression = Expression.Call(null, selectMethod, asQuerableExpression, propertyLambda);
                        aggregationExpression = Expression.Call(null, customMethod, selectExpression);
                    }
                    break;
                default:
                    throw new ODataException(Error.Format(SRResources.AggregationMethodNotSupported, expression.Method));
            }

            return WrapConvert(aggregationExpression);
        }

        private List<NamedPropertyExpression> CreateGroupByMemberAssignments(IEnumerable<GroupByPropertyNode> nodes, QueryBinderContext context)
        {
            var properties = new List<NamedPropertyExpression>();
            foreach (var grpProp in nodes)
            {
                var propertyName = grpProp.Name;
                if (grpProp.Expression != null)
                {
                    properties.Add(new NamedPropertyExpression(Expression.Constant(propertyName), WrapConvert(BindAccessor(grpProp.Expression, context))));
                }
                else
                {
                    var wrapperProperty = typeof(GroupByWrapper).GetProperty(GroupByContainerProperty);
                    List<MemberAssignment> wta = new List<MemberAssignment>();
                    wta.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(CreateGroupByMemberAssignments(grpProp.ChildTransformations, context))));
                    properties.Add(new NamedPropertyExpression(Expression.Constant(propertyName), Expression.MemberInit(Expression.New(typeof(GroupByWrapper)), wta)));
                }
            }

            return properties;
        }
    }
}
