//-----------------------------------------------------------------------------
// <copyright file="QueryBinder.Transformations.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// The base class for all expression binders.
    /// </summary>
    public abstract partial class QueryBinder
    {
        /// <summary>
        /// Wrap a value type with Expression.Convert.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to be wrapped.</param>
        /// <returns>The wrapped  <see cref="Expression"/></returns>
        protected static Expression WrapConvert(Expression expression)
        {
            if (expression == null)
            {
                throw Error.ArgumentNull(nameof(expression));
            }

            // Expression that we are generating looks like Value = $it.PropertyName where Value is defined as object and PropertyName can be value 
            // Proper .NET expression must look like as Value = (object) $it.PropertyName for proper boxing or AccessViolationException will be thrown
            // Cast to object isn't translatable by EF6 as a result skipping (object) in that case
            // Update: We have removed support for EF6
            return (!expression.Type.IsValueType)
                ? expression
                : Expression.Convert(expression, typeof(object));
        }

        /// <summary>
        /// Creates an <see cref="Expression"/> from the <see cref="QueryNode"/>.
        /// </summary>
        /// <param name="node">The <see cref="QueryNode"/> to be bound.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <param name="baseElement">The <see cref="Expression"/> for the base element.</param>
        /// <returns>The created <see cref="Expression"/>.</returns>
        public virtual Expression BindAccessor(QueryNode node, QueryBinderContext context, Expression baseElement = null)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            switch (node.Kind)
            {
                case QueryNodeKind.ResourceRangeVariableReference:
                    return context.LambdaParameter.Type.IsGenericType && context.LambdaParameter.Type.GetGenericTypeDefinition() == typeof(FlatteningWrapper<>)
                        ? (Expression)Expression.Property(context.LambdaParameter, "Source")
                        : context.LambdaParameter;
                case QueryNodeKind.SingleValuePropertyAccess:
                    SingleValuePropertyAccessNode propAccessNode = node as SingleValuePropertyAccessNode;
                    return CreatePropertyAccessExpression(BindAccessor(propAccessNode.Source, context, baseElement), context, propAccessNode.Property, GetFullPropertyPath(propAccessNode));
                case QueryNodeKind.AggregatedCollectionPropertyNode:
                    AggregatedCollectionPropertyNode aggPropAccessNode = node as AggregatedCollectionPropertyNode;
                    return CreatePropertyAccessExpression(BindAccessor(aggPropAccessNode.Source, context, baseElement), context, aggPropAccessNode.Property);
                case QueryNodeKind.SingleComplexNode:
                    SingleComplexNode singleComplexNode = node as SingleComplexNode;
                    return CreatePropertyAccessExpression(BindAccessor(singleComplexNode.Source, context, baseElement), context, singleComplexNode.Property, GetFullPropertyPath(singleComplexNode));
                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    SingleValueOpenPropertyAccessNode openNode = node as SingleValueOpenPropertyAccessNode;
                    return GetFlattenedPropertyExpression(openNode.Name, context) ?? CreateOpenPropertyAccessExpression(openNode, context);
                case QueryNodeKind.None:
                case QueryNodeKind.SingleNavigationNode:
                    SingleNavigationNode navNode = node as SingleNavigationNode;
                    return CreatePropertyAccessExpression(BindAccessor(navNode.Source, context), context, navNode.NavigationProperty, GetFullPropertyPath(navNode));
                case QueryNodeKind.BinaryOperator:
                    BinaryOperatorNode binaryNode = node as BinaryOperatorNode;
                    Expression leftExpression = BindAccessor(binaryNode.Left, context, baseElement);
                    Expression rightExpression = BindAccessor(binaryNode.Right, context, baseElement);
                    return ExpressionBinderHelper.CreateBinaryExpression(binaryNode.OperatorKind, leftExpression, rightExpression,
                        liftToNull: true, context.QuerySettings);
                case QueryNodeKind.Convert:
                    ConvertNode convertNode = node as ConvertNode;
                    return CreateConvertExpression(convertNode, BindAccessor(convertNode.Source, context, baseElement), context);
                case QueryNodeKind.CollectionNavigationNode:
                    return baseElement ?? context.LambdaParameter;
                case QueryNodeKind.SingleValueFunctionCall:
                    return BindSingleValueFunctionCallNode(node as SingleValueFunctionCallNode, context);
                case QueryNodeKind.Constant:
                    return BindConstantNode(node as ConstantNode, context);
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind,
                        typeof(AggregationBinder).Name);
            }
        }

        internal static IEnumerable<GroupByPropertyNode> GetGroupingProperties(TransformationNode transformation)
        {
            if (transformation.Kind == TransformationNodeKind.GroupBy)
            {
                GroupByTransformationNode groupByClause = transformation as GroupByTransformationNode;

                return groupByClause.GroupingProperties;
            }

            return null;
        }

        /// <summary>
        /// Get a collection of aggregate expressions from a <see cref="TransformationNode"/>.
        /// </summary>
        /// <param name="context">The query binder context.</param>
        /// <param name="transformation">The <see cref="TransformationNode"/>.</param>
        /// <returns>A collection of aggregate expressions.</returns>
        public virtual IEnumerable<AggregateExpressionBase> GetAggregateExpressions(QueryBinderContext context, TransformationNode transformation)
        {
            Contract.Assert(context != null);
            Contract.Assert(transformation != null);

            IEnumerable<AggregateExpressionBase> aggregateExpressions = null;

            switch (transformation.Kind)
            {
                case TransformationNodeKind.Aggregate:
                    AggregateTransformationNode aggregateClause = transformation as AggregateTransformationNode;
                    aggregateExpressions = FixCustomMethodReturnTypes(aggregateClause.AggregateExpressions, context);
                    break;
                case TransformationNodeKind.GroupBy:
                    GroupByTransformationNode groupByClause = transformation as GroupByTransformationNode;
                    if (groupByClause.ChildTransformations != null)
                    {
                        if (groupByClause.ChildTransformations.Kind == TransformationNodeKind.Aggregate)
                        {
                            AggregateTransformationNode aggregationNode = (AggregateTransformationNode)groupByClause.ChildTransformations;
                            aggregateExpressions = FixCustomMethodReturnTypes(aggregationNode.AggregateExpressions, context);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture,
                        SRResources.NotSupportedTransformationKind, transformation.Kind));
            }

            return aggregateExpressions;
        }

        internal IEnumerable<AggregateExpressionBase> FixCustomMethodReturnTypes(IEnumerable<AggregateExpressionBase> aggregateExpressions, QueryBinderContext context)
        {
            return aggregateExpressions.Select(x =>
            {
                AggregateExpression ae = x as AggregateExpression;
                return ae != null ? FixCustomMethodReturnType(ae, context) : x;
            });
        }

        internal AggregateExpression FixCustomMethodReturnType(AggregateExpression expression, QueryBinderContext context)
        {
            if (expression.Method != AggregationMethod.Custom)
            {
                return expression;
            }

            MethodInfo customMethod = GetCustomMethod(expression, context);

            // var typeReference = customMethod.ReturnType.GetEdmPrimitiveTypeReference();
            IEdmPrimitiveTypeReference typeReference = context.Model.GetEdmPrimitiveTypeReference(customMethod.ReturnType);

            return new AggregateExpression(expression.Expression, expression.MethodDefinition, expression.Alias, typeReference);
        }

        /// <summary>
        /// Get a custom aggregation method from the aggregation expression.
        /// </summary>
        /// <param name="expression">The <see cref="AggregateExpression"/>.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The custom method.</returns>
        public virtual MethodInfo GetCustomMethod(AggregateExpression expression, QueryBinderContext context)
        {
            LambdaExpression propertyLambda = Expression.Lambda(BindAccessor(expression.Expression, context), context.LambdaParameter);
            Type inputType = propertyLambda.Body.Type;

            string methodToken = expression.MethodDefinition.MethodLabel;
            CustomAggregateMethodAnnotation customFunctionAnnotations = context.Model.GetAnnotationValue<CustomAggregateMethodAnnotation>(context.Model);

            MethodInfo customMethod;
            if (!customFunctionAnnotations.GetMethodInfo(methodToken, inputType, out customMethod))
            {
                throw new ODataException(
                    Error.Format(
                        SRResources.AggregationNotSupportedForType,
                        expression.Method,
                        expression.Expression,
                        inputType));
            }

            return customMethod;
        }

        /// <summary>
        /// Creates a LINQ <see cref="Expression"/> that represents the semantics of the <see cref="SingleValueOpenPropertyAccessNode"/>.
        /// </summary>
        /// <param name="openNode">They query node to create an expression from.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression CreateOpenPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
        {
            Expression sourceAccessor = BindAccessor(openNode.Source, context);

            // First check that property exists in source
            // It's the case when we are apply transformation based on earlier transformation
            if (sourceAccessor.Type.GetProperty(openNode.Name) != null)
            {
                return Expression.Property(sourceAccessor, openNode.Name);
            }

            // Property doesn't exists go for dynamic properties dictionary
            PropertyInfo prop = GetDynamicPropertyContainer(openNode, context);
            MemberExpression propertyAccessExpression = Expression.Property(sourceAccessor, prop.Name);
            IndexExpression readDictionaryIndexerExpression = Expression.Property(propertyAccessExpression,
                            DictionaryStringObjectIndexerName, Expression.Constant(openNode.Name));
            MethodCallExpression containsKeyExpression = Expression.Call(propertyAccessExpression,
                propertyAccessExpression.Type.GetMethod("ContainsKey"), Expression.Constant(openNode.Name));
            ConstantExpression nullExpression = Expression.Constant(null);

            if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                BinaryExpression dynamicDictIsNotNull = Expression.NotEqual(propertyAccessExpression, Expression.Constant(null));
                BinaryExpression dynamicDictIsNotNullAndContainsKey = Expression.AndAlso(dynamicDictIsNotNull, containsKeyExpression);
                return Expression.Condition(
                    dynamicDictIsNotNullAndContainsKey,
                    readDictionaryIndexerExpression,
                    nullExpression);
            }
            else
            {
                return Expression.Condition(
                    containsKeyExpression,
                    readDictionaryIndexerExpression,
                    nullExpression);
            }
        }
    }
}
