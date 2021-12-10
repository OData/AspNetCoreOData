//-----------------------------------------------------------------------------
// <copyright file="TransformationBinderBase2.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    internal class TransformationBinderBase2 : /*ExpressionBinderBase*/ QueryBinder
    {
        /*internal TransformationBinderBase2(ODataQuerySettings settings, IAssemblyResolver assembliesResolver, Type elementType,
            IEdmModel model)*//* : base(model, assembliesResolver, settings)*//*
        {
            Contract.Assert(elementType != null);
            LambdaParameter = Expression.Parameter(elementType, "$it");
        }

        protected Type ElementType { get { return this.LambdaParameter.Type; } }

        protected ParameterExpression LambdaParameter { get; set; }

        protected bool ClassicEF { get; private set; }

        /// <summary>
        /// Gets CLR type returned from the query.
        /// </summary>
        public Type ResultClrType
        {
            get; protected set;
        }*/

        /// <summary>
        /// Checks IQueryable provider for need of EF6 optimization
        /// </summary>
        /// <param name="query"></param>
        /// <returns>True if EF6 optimization are needed.</returns>
        internal virtual bool IsClassicEF(IQueryable query)
        {
            var providerNS = query.Provider.GetType().Namespace;
            return (providerNS == HandleNullPropagationOptionHelper.ObjectContextQueryProviderNamespaceEF6
                || providerNS == HandleNullPropagationOptionHelper.EntityFrameworkQueryProviderNamespace);
        }

        protected void PreprocessQuery(IQueryable query, QueryBinderContext context)
        {
            Contract.Assert(query != null);

            context.ClassicEF = IsClassicEF(query);
            //context.BaseQuery = query;
            context = EnsureFlattenedPropertyContainer(context);
        }

        protected Expression WrapConvert(Expression expression, QueryBinderContext context)
        {
            // Expression that we are generating looks like Value = $it.PropertyName where Value is defined as object and PropertyName can be value 
            // Proper .NET expression must look like as Value = (object) $it.PropertyName for proper boxing or AccessViolationException will be thrown
            // Cast to object isn't translatable by EF6 as a result skipping (object) in that case
            return (context.ClassicEF || !expression.Type.IsValueType)
                ? expression
                : Expression.Convert(expression, typeof(object));
        }

        public virtual Expression Bind(QueryNode node, QueryBinderContext context)
        {
            SingleValueNode singleValueNode = node as SingleValueNode;
            if (node != null)
            {
                return BindAccessor(singleValueNode, context);
            }

            throw Error.Argument(nameof(node), SRResources.OnlySingleValueNodeSupported);
        }

        /*protected override ParameterExpression Parameter
        {
            get
            {
                return this.LambdaParameter;
            }
        }*/

        protected Expression BindAccessor(QueryNode node, QueryBinderContext context, Expression baseElement = null)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.ResourceRangeVariableReference:
                    return context.LambdaParameter.Type.IsGenericType && context.LambdaParameter.Type.GetGenericTypeDefinition() == typeof(FlatteningWrapper<>)
                        ? (Expression)Expression.Property(context.LambdaParameter, "Source")
                        : context.LambdaParameter;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propAccessNode = node as SingleValuePropertyAccessNode;
                    return CreatePropertyAccessExpression(BindAccessor(propAccessNode.Source, context, baseElement), context, propAccessNode.Property, GetFullPropertyPath(propAccessNode));
                case QueryNodeKind.AggregatedCollectionPropertyNode:
                    var aggPropAccessNode = node as AggregatedCollectionPropertyNode;
                    return CreatePropertyAccessExpression(BindAccessor(aggPropAccessNode.Source, context, baseElement), context, aggPropAccessNode.Property);
                case QueryNodeKind.SingleComplexNode:
                    var singleComplexNode = node as SingleComplexNode;
                    return CreatePropertyAccessExpression(BindAccessor(singleComplexNode.Source, context, baseElement), context, singleComplexNode.Property, GetFullPropertyPath(singleComplexNode));
                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    var openNode = node as SingleValueOpenPropertyAccessNode;
                    return GetFlattenedPropertyExpression(openNode.Name) ?? CreateOpenPropertyAccessExpression(openNode, context);
                case QueryNodeKind.None:
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = (SingleNavigationNode)node;
                    return CreatePropertyAccessExpression(BindAccessor(navNode.Source, context), context, navNode.NavigationProperty, GetFullPropertyPath(navNode));
                case QueryNodeKind.BinaryOperator:
                    var binaryNode = (BinaryOperatorNode)node;
                    var leftExpression = BindAccessor(binaryNode.Left, context, baseElement);
                    var rightExpression = BindAccessor(binaryNode.Right, context, baseElement);
                    return ExpressionBinderHelper.CreateBinaryExpression(binaryNode.OperatorKind, leftExpression, rightExpression,
                        liftToNull: true, context.QuerySettings);
                case QueryNodeKind.Convert:
                    var convertNode = (ConvertNode)node;
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

        private Expression CreateOpenPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
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
                var dynamicDictIsNotNull = Expression.NotEqual(propertyAccessExpression, Expression.Constant(null));
                var dynamicDictIsNotNullAndContainsKey = Expression.AndAlso(dynamicDictIsNotNull, containsKeyExpression);
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
