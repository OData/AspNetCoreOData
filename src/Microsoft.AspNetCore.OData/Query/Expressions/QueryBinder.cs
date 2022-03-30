//-----------------------------------------------------------------------------
// <copyright file="QueryBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// The base class for all expression query binders.
    /// </summary>
    public abstract partial class QueryBinder
    {
        #region Static Properties
        internal static readonly string DictionaryStringObjectIndexerName = typeof(Dictionary<string, object>).GetDefaultMembers()[0].Name;

        internal static readonly Expression NullConstant = Expression.Constant(null);
        internal static readonly Expression FalseConstant = Expression.Constant(false);
        internal static readonly Expression TrueConstant = Expression.Constant(true);

        // .NET 6 adds a new overload: TryParse<TEnum>(ReadOnlySpan<Char>, TEnum)
        // Now, with `TryParse<TEnum>(String, TEnum)`, there will have two versions with two parameters
        // So, the previous Single() will throw exception.
        internal static readonly MethodInfo EnumTryParseMethod = typeof(Enum).GetMethod("TryParse",
            new[]
            {
                typeof(string),
                Type.MakeGenericMethodParameter(0).MakeByRefType()
            });
        #endregion

        #region Bind methods
        /// <summary>
        /// Binds a <see cref="QueryNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics of the <see cref="QueryNode"/>.
        /// </summary>
        /// <param name="node">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression Bind(QueryNode node, QueryBinderContext context)
        {
            CheckArgumentNull(node, context);

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            if (node is CollectionNode collectionNode)
            {
                return BindCollectionNode(collectionNode, context);
            }
            else if (node is SingleValueNode singleValueNode)
            {
                return BindSingleValueNode(singleValueNode, context);
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(QueryBinder).Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="CollectionNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics of the <see cref="CollectionNode"/>.
        /// </summary>
        /// <param name="node">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionNode(CollectionNode node, QueryBinderContext context)
        {
            CheckArgumentNull(node, context);

            switch (node.Kind)
            {
                case QueryNodeKind.CollectionNavigationNode:
                    CollectionNavigationNode navigationNode = node as CollectionNavigationNode;
                    return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty, null, context);

                case QueryNodeKind.CollectionPropertyAccess:
                    return BindCollectionPropertyAccessNode(node as CollectionPropertyAccessNode, context);

                case QueryNodeKind.CollectionComplexNode:
                    return BindCollectionComplexNode(node as CollectionComplexNode, context);

                case QueryNodeKind.CollectionResourceCast:
                    return BindCollectionResourceCastNode(node as CollectionResourceCastNode, context);

                case QueryNodeKind.CollectionConstant:
                    return BindCollectionConstantNode(node as CollectionConstantNode, context);

                case QueryNodeKind.CollectionFunctionCall:
                case QueryNodeKind.CollectionResourceFunctionCall:
                case QueryNodeKind.CollectionOpenPropertyAccess:
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(QueryBinder).Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleValueNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics of the <see cref="SingleValueNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleValueNode(SingleValueNode node, QueryBinderContext context)
        {
            CheckArgumentNull(node, context);

            switch (node.Kind)
            {
                case QueryNodeKind.BinaryOperator:
                    return BindBinaryOperatorNode(node as BinaryOperatorNode, context);

                case QueryNodeKind.Constant:
                    return BindConstantNode(node as ConstantNode, context);

                case QueryNodeKind.Convert:
                    return BindConvertNode(node as ConvertNode, context);

                case QueryNodeKind.ResourceRangeVariableReference:
                    return BindRangeVariable((node as ResourceRangeVariableReferenceNode).RangeVariable, context);

                case QueryNodeKind.NonResourceRangeVariableReference:
                    return BindRangeVariable((node as NonResourceRangeVariableReferenceNode).RangeVariable, context);

                case QueryNodeKind.SingleValuePropertyAccess:
                    return BindPropertyAccessQueryNode(node as SingleValuePropertyAccessNode, context);

                case QueryNodeKind.SingleComplexNode:
                    return BindSingleComplexNode(node as SingleComplexNode, context);

                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    return BindDynamicPropertyAccessQueryNode(node as SingleValueOpenPropertyAccessNode, context);

                case QueryNodeKind.UnaryOperator:
                    return BindUnaryOperatorNode(node as UnaryOperatorNode, context);

                case QueryNodeKind.SingleValueFunctionCall:
                    return BindSingleValueFunctionCallNode(node as SingleValueFunctionCallNode, context);

                case QueryNodeKind.SingleNavigationNode:
                    SingleNavigationNode navigationNode = node as SingleNavigationNode;
                    return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty, GetFullPropertyPath(navigationNode), context);

                case QueryNodeKind.Any:
                    return BindAnyNode(node as AnyNode, context);

                case QueryNodeKind.All:
                    return BindAllNode(node as AllNode, context);

                case QueryNodeKind.SingleResourceCast:
                    return BindSingleResourceCastNode(node as SingleResourceCastNode, context);

                case QueryNodeKind.SingleResourceFunctionCall:
                    return BindSingleResourceFunctionCallNode(node as SingleResourceFunctionCallNode, context);

                case QueryNodeKind.In:
                    return BindInNode(node as InNode, context);

                case QueryNodeKind.Count:
                    return BindCountNode(node as CountNode, context);

                case QueryNodeKind.NamedFunctionParameter:
                case QueryNodeKind.ParameterAlias:
                case QueryNodeKind.EntitySet:
                case QueryNodeKind.KeyLookup:
                case QueryNodeKind.SearchTerm:
                // Unused or have unknown uses.
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(QueryBinder).Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="IEdmNavigationProperty"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="IEdmNavigationProperty"/>.
        /// </summary>
        /// <param name="sourceNode">The node that represents the navigation source.</param>
        /// <param name="navigationProperty">The navigation property to bind.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty, string propertyPath, QueryBinderContext context)
        {
            CheckArgumentNull(sourceNode, context);

            Expression source;

            // TODO: bug in uri parser is causing this property to be null for the root property.
            if (sourceNode == null)
            {
                // It means we should use current parameter at current context.
                source = context.CurrentParameter;
            }
            else
            {
                source = Bind(sourceNode, context);
            }

            return CreatePropertyAccessExpression(source, context, navigationProperty, propertyPath);
        }

        /// <summary>
        /// Binds a <see cref="CollectionResourceCastNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionResourceCastNode"/>.
        /// </summary>
        /// <param name="node">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionResourceCastNode(CollectionResourceCastNode node, QueryBinderContext context)
        {
            CheckArgumentNull(node, context);

            IEdmStructuredTypeReference structured = node.ItemStructuredType;
            Contract.Assert(structured != null, "NS casts can contain only structured types");

            Type clrType = context.Model.GetClrType(structured);

            Expression source = BindCastSourceNode(node.Source, context);

            if (ExpressionBinderHelper.IsIQueryable(source.Type))
            {
                return Expression.Call(null, ExpressionHelperMethods.QueryableOfType.MakeGenericMethod(clrType), source);
            }
            else
            {
                return Expression.Call(null, ExpressionHelperMethods.EnumerableOfType.MakeGenericMethod(clrType), source);
            }
        }

        /// <summary>
        /// Binds a <see cref="CollectionComplexNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionComplexNode"/>.
        /// </summary>
        /// <param name="collectionComplexNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionComplexNode(CollectionComplexNode collectionComplexNode, QueryBinderContext context)
        {
            CheckArgumentNull(collectionComplexNode, context);

            Expression source = Bind(collectionComplexNode.Source, context);
            return CreatePropertyAccessExpression(source, context, collectionComplexNode.Property);
        }

        /// <summary>
        /// Binds a <see cref="CollectionPropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionPropertyAccessNode"/>.
        /// </summary>
        /// <param name="propertyAccessNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode, QueryBinderContext context)
        {
            CheckArgumentNull(propertyAccessNode, context);

            Expression source = Bind(propertyAccessNode.Source, context);
            return CreatePropertyAccessExpression(source, context, propertyAccessNode.Property);
        }

        /// <summary>
        /// Binds a <see cref="SingleValueOpenPropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValueOpenPropertyAccessNode"/>.
        /// </summary>
        /// <param name="openNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindDynamicPropertyAccessQueryNode(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
        {
            CheckArgumentNull(openNode, context);

            if (context.ElementClrType.IsDynamicTypeWrapper())
            {
                return GetFlattenedPropertyExpression(openNode.Name, context) ?? Expression.Property(Bind(openNode.Source, context), openNode.Name);
            }

            if (context.ComputedProperties.TryGetValue(openNode.Name, out var computedProperty))
            {
                return Bind(computedProperty.Expression, context);
            }

            PropertyInfo prop = GetDynamicPropertyContainer(openNode, context);

            var propertyAccessExpression = BindPropertyAccessExpression(openNode, prop, context);
            var readDictionaryIndexerExpression = Expression.Property(propertyAccessExpression,
                DictionaryStringObjectIndexerName, Expression.Constant(openNode.Name));
            var containsKeyExpression = Expression.Call(propertyAccessExpression,
                propertyAccessExpression.Type.GetMethod("ContainsKey"), Expression.Constant(openNode.Name));
            var nullExpression = Expression.Constant(null);

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

        /// <summary>
        /// Binds a <see cref="SingleValuePropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValuePropertyAccessNode"/>.
        /// </summary>
        /// <param name="propertyAccessNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindPropertyAccessQueryNode(SingleValuePropertyAccessNode propertyAccessNode, QueryBinderContext context)
        {
            CheckArgumentNull(propertyAccessNode, context);

            Expression source = Bind(propertyAccessNode.Source, context);
            return CreatePropertyAccessExpression(source, context, propertyAccessNode.Property, GetFullPropertyPath(propertyAccessNode));
        }

        /// <summary>
        /// Binds a <see cref="SingleComplexNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleComplexNode"/>.
        /// </summary>
        /// <param name="singleComplexNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleComplexNode(SingleComplexNode singleComplexNode, QueryBinderContext context)
        {
            CheckArgumentNull(singleComplexNode, context);

            Expression source = Bind(singleComplexNode.Source, context);
            return CreatePropertyAccessExpression(source, context, singleComplexNode.Property, GetFullPropertyPath(singleComplexNode));
        }

        /// <summary>
        /// Binds a <see cref="RangeVariable"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="RangeVariable"/>.
        /// </summary>
        /// <param name="rangeVariable">The range variable to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindRangeVariable(RangeVariable rangeVariable, QueryBinderContext context)
        {
            if (rangeVariable == null)
            {
                throw Error.ArgumentNull(nameof(rangeVariable));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // Be noted: it's used in $compute for $select and $expand.
            if (context.Source != null)
            {
                return context.Source;
            }

            Expression parameter;
            // When we have a $this RangeVariable, it's refer to "current parameter" in current context.
            if (rangeVariable.Name == "$this")
            {
                parameter = context.CurrentParameter;
            }
            else if (rangeVariable.Name == "$it")
            {
                // ~/Customers?$select=Addresses($filter=$it/City eq City)
                // Both Expression.Source is the RangeVariableRefereneNode, and the name is "$it".
                // But first refers to "Addresses" in $it/City, and the second (City) refers to "Customers".
                // We can't simply identify using the "$it" so there's a workaround.
                if (context.IsNested)
                {
                    Type clrType = context.Model.GetClrType(rangeVariable.TypeReference);
                    if (clrType == context.CurrentParameter.Type)
                    {
                        parameter = context.CurrentParameter;
                    }
                    else
                    {
                        parameter = context.GetParameter("$it");
                    }
                }
                else
                {
                    parameter = context.GetParameter("$it");
                }
            }
            else
            {
                parameter = context.GetParameter(rangeVariable.Name);
            }

            return ConvertNonStandardPrimitives(parameter, context);
        }

        /// <summary>
        /// Binds a <see cref="BinaryOperatorNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="BinaryOperatorNode"/>.
        /// </summary>
        /// <param name="binaryOperatorNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode, QueryBinderContext context)
        {
            CheckArgumentNull(binaryOperatorNode, context);

            ODataQuerySettings querySettings = context.QuerySettings;

            Expression left = Bind(binaryOperatorNode.Left, context);
            Expression right = Bind(binaryOperatorNode.Right, context);

            // handle null propagation only if either of the operands can be null
            bool isNullPropagationRequired = querySettings.HandleNullPropagation == HandleNullPropagationOption.True && (ExpressionBinderHelper.IsNullable(left.Type) || ExpressionBinderHelper.IsNullable(right.Type));
            if (isNullPropagationRequired)
            {
                // |----------------------------------------------------------------|
                // |SQL 3VL truth table.                                            |
                // |----------------------------------------------------------------|
                // |p       |    q      |    p OR q     |    p AND q    |    p = q  |
                // |----------------------------------------------------------------|
                // |True    |   True    |   True        |   True        |   True    |
                // |True    |   False   |   True        |   False       |   False   |
                // |True    |   NULL    |   True        |   NULL        |   NULL    |
                // |False   |   True    |   True        |   False       |   False   |
                // |False   |   False   |   False       |   False       |   True    |
                // |False   |   NULL    |   NULL        |   False       |   NULL    |
                // |NULL    |   True    |   True        |   NULL        |   NULL    |
                // |NULL    |   False   |   NULL        |   False       |   NULL    |
                // |NULL    |   NULL    |   Null        |   NULL        |   NULL    |
                // |--------|-----------|---------------|---------------|-----------|

                // before we start with null propagation, convert the operators to nullable if already not.
                left = ExpressionBinderHelper.ToNullable(left);
                right = ExpressionBinderHelper.ToNullable(right);

                bool liftToNull = true;
                if (left == NullConstant || right == NullConstant)
                {
                    liftToNull = false;
                }

                // Expression trees do a very good job of handling the 3VL truth table if we pass liftToNull true.
                return ExpressionBinderHelper.CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: liftToNull, querySettings);
            }
            else
            {
                return ExpressionBinderHelper.CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: false, querySettings);
            }
        }

        /// <summary>
        /// Binds a <see cref="ConvertNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="ConvertNode"/>.
        /// </summary>
        /// <param name="convertNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindConvertNode(ConvertNode convertNode, QueryBinderContext context)
        {
            CheckArgumentNull(convertNode, context);

            Contract.Assert(convertNode.TypeReference != null);

            Expression source = Bind(convertNode.Source, context);

            return CreateConvertExpression(convertNode, source, context);
        }

        /// <summary>
        /// Binds a <see cref="CountNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CountNode"/>.
        /// </summary>
        /// <param name="node">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCountNode(CountNode node, QueryBinderContext context)
        {
            // $filter=navProp/$count($filter=prop gt 1) gt 2

            CheckArgumentNull(node, context);

            Expression source = Bind(node.Source, context);
            Expression countExpression = Expression.Constant(null, typeof(long?));
            Type elementType;
            if (!TypeHelper.IsCollection(source.Type, out elementType))
            {
                return countExpression;
            }

            MethodInfo countMethod;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                countMethod = ExpressionHelperMethods.QueryableCountGeneric.MakeGenericMethod(elementType);
            }
            else
            {
                countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(elementType);
            }

            // Bind the inner $filter clause within the $count segment.
            // e.g Books?$filter=Authors/$count($filter=Id gt 1) gt 1
            if (node.FilterClause != null)
            {
                QueryBinderContext nextBinderContext = new QueryBinderContext(context, context.QuerySettings, elementType);

                Expression body = Bind(node.FilterClause.Expression, nextBinderContext);

                ParameterExpression filterParameter = nextBinderContext.CurrentParameter;

                LambdaExpression filterExpr = Expression.Lambda(body, filterParameter);

                filterExpr = Expression.Lambda(ApplyNullPropagationForFilterBody(filterExpr.Body, nextBinderContext), filterExpr.Parameters);

                MethodInfo whereMethod;
                if (typeof(IQueryable).IsAssignableFrom(source.Type))
                {
                    whereMethod = ExpressionHelperMethods.QueryableWhereGeneric.MakeGenericMethod(elementType);
                }
                else
                {
                    whereMethod = ExpressionHelperMethods.EnumerableWhereGeneric.MakeGenericMethod(elementType);
                }

                // The source expression looks like: $it.Authors
                // So the generated countExpression below will look like: $it.Authors.Where($it => $it.Id > 1)
                source = Expression.Call(null, whereMethod, new[] { source, filterExpr });
            }

            // append LongCount() method.
            // The final countExpression with the nested $filter clause will look like: $it.Authors.Where($it => $it.Id > 1).LongCount()
            // The final countExpression without the nested $filter clause will look like: $it.Authors.LongCount()
            countExpression = Expression.Call(null, countMethod, new[] { source });

            if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // source == null ? null : countExpression 
                return Expression.Condition(
                       test: Expression.Equal(source, Expression.Constant(null)),
                       ifTrue: Expression.Constant(null, typeof(long?)),
                       ifFalse: ExpressionHelpers.ToNullable(countExpression));
            }
            else
            {
                return countExpression;
            }
        }

        /// <summary>
        /// Binds an <see cref="InNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="InNode"/>.
        /// </summary>
        /// <param name="inNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindInNode(InNode inNode, QueryBinderContext context)
        {
            CheckArgumentNull(inNode, context);

            Expression singleValue = Bind(inNode.Left, context);
            Expression collection = Bind(inNode.Right, context);

            Type collectionItemType = collection.Type.GetElementType();
            if (collectionItemType == null)
            {
                Type[] genericArgs = collection.Type.GetGenericArguments();
                // The model builder does not support non-generic collections like ArrayList
                // or generic collections with generic arguments > 1 like IDictionary<,>
                Contract.Assert(genericArgs.Length == 1);
                collectionItemType = genericArgs[0];
            }

            if (ExpressionBinderHelper.IsIQueryable(collection.Type))
            {
                Expression containsExpression = singleValue.Type != collectionItemType ? Expression.Call(null, ExpressionHelperMethods.QueryableCastGeneric.MakeGenericMethod(singleValue.Type), collection) : collection;
                return Expression.Call(null, ExpressionHelperMethods.QueryableContainsGeneric.MakeGenericMethod(singleValue.Type), containsExpression, singleValue);
            }
            else
            {
                Expression containsExpression = singleValue.Type != collectionItemType ? Expression.Call(null, ExpressionHelperMethods.EnumerableCastGeneric.MakeGenericMethod(singleValue.Type), collection) : collection;
                return Expression.Call(null, ExpressionHelperMethods.EnumerableContainsGeneric.MakeGenericMethod(singleValue.Type), containsExpression, singleValue);
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleResourceFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleResourceFunctionCallNode"/>.
        /// </summary>
        /// <param name="node">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleResourceFunctionCallNode(SingleResourceFunctionCallNode node, QueryBinderContext context)
        {
            CheckArgumentNull(node, context);

            switch (node.Name)
            {
                case ClrCanonicalFunctions.CastFunctionName:
                    return BindSingleResourceCastFunctionCall(node, context);
                default:
                    throw Error.NotSupported(SRResources.ODataFunctionNotSupported, node.Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleResourceFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleResourceFunctionCallNode"/>.
        /// </summary>
        /// <param name="node">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleResourceCastFunctionCall(SingleResourceFunctionCallNode node, QueryBinderContext context)
        {
            CheckArgumentNull(node, context);

            Contract.Assert(ClrCanonicalFunctions.CastFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters, context);

            Contract.Assert(arguments.Length == 2);

            IEdmModel model = context.Model;

            string targetEdmTypeName = (string)((ConstantNode)node.Parameters.Last()).Value;
            IEdmType targetEdmType = model.FindType(targetEdmTypeName);
            Type targetClrType = null;

            if (targetEdmType != null)
            {
                targetClrType = model.GetClrType(targetEdmType.ToEdmTypeReference(false));
            }

            if (arguments[0].Type == targetClrType)
            {
                // We only support to cast Entity type to the same type now.
                return arguments[0];
            }
            else if (arguments[0].Type.IsAssignableFrom(targetClrType))
            {
                // To support to cast Entity/Complex type to the sub type now.
                Expression source;
                if (node.Source != null)
                {
                    source = BindCastSourceNode(node.Source, context);
                }
                else
                {
                    // if the cast is on the root i.e $it (~/Products?$filter=NS.PopularProducts/.....),
                    // node.Source would be null. Calling BindCastSourceNode will always return '$it'.
                    // In scenarios where we are casting a navigation property to return an expression that queries against the parent property,
                    // we need to have a memberAccess expression e.g '$it.Category'. We can get this from arguments[0].
                    source = arguments[0];
                }

                return Expression.TypeAs(source, targetClrType);
            }
            else
            {
                // Cast fails and return null.
                return NullConstant;
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleResourceCastNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleResourceCastNode"/>.
        /// </summary>
        /// <param name="node">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleResourceCastNode(SingleResourceCastNode node, QueryBinderContext context)
        {
            CheckArgumentNull(node, context);

            IEdmStructuredTypeReference structured = node.StructuredTypeReference;
            Contract.Assert(structured != null, "NS casts can contain only structured types");

            Type clrType = context.Model.GetClrType(structured);

            Expression source = BindCastSourceNode(node.Source, context);
            return Expression.TypeAs(source, clrType);
        }

        /// <summary>
        /// Binds a <see cref="AllNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="AllNode"/>.
        /// </summary>
        /// <param name="allNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindAllNode(AllNode allNode, QueryBinderContext context)
        {
            CheckArgumentNull(allNode, context);

           // context.EnterLambdaScope();

            (string name, ParameterExpression allIt) = context.HandleLambdaParameters(allNode.RangeVariables);

            Expression source;
            Contract.Assert(allNode.Source != null);
            source = Bind(allNode.Source, context);

            Expression body = source;
            Contract.Assert(allNode.Body != null);

            body = Bind(allNode.Body, context);
            body = ApplyNullPropagationForFilterBody(body, context);
            body = Expression.Lambda(body, allIt);

            Expression all = All(source, body);

            context.RemoveParameter(name);
            //context.ExitLamdbaScope();

            if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && ExpressionBinderHelper.IsNullable(source.Type))
            {
                // IFF(source == null) null; else Any(body);
                all = ExpressionBinderHelper.ToNullable(all);
                return Expression.Condition(
                    test: Expression.Equal(source, NullConstant),
                    ifTrue: Expression.Constant(null, all.Type),
                    ifFalse: all);
            }
            else
            {
                return all;
            }
        }

        /// <summary>
        /// Binds a <see cref="AnyNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="AnyNode"/>.
        /// </summary>
        /// <param name="anyNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindAnyNode(AnyNode anyNode, QueryBinderContext context)
        {
            CheckArgumentNull(anyNode, context);

            //context.EnterLambdaScope();

            (string name, ParameterExpression anyIt) = context.HandleLambdaParameters(anyNode.RangeVariables);

            Expression source;
            Contract.Assert(anyNode.Source != null);
            source = Bind(anyNode.Source, context);

            Expression body = null;
            // uri parser places an Constant node with value true for empty any() body
            if (anyNode.Body != null && anyNode.Body.Kind != QueryNodeKind.Constant)
            {
                body = Bind(anyNode.Body, context);
                body = ApplyNullPropagationForFilterBody(body, context);
                body = Expression.Lambda(body, anyIt);
            }
            else if (anyNode.Body != null && anyNode.Body.Kind == QueryNodeKind.Constant
                && (bool)(anyNode.Body as ConstantNode).Value == false)
            {
                // any(false) is the same as just false
                context.RemoveParameter(name);
                return FalseConstant;
            }

            Expression any = Any(source, body);

            context.RemoveParameter(name);

            if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && ExpressionBinderHelper.IsNullable(source.Type))
            {
                // IFF(source == null) null; else Any(body);
                any = ExpressionBinderHelper.ToNullable(any);
                return Expression.Condition(
                    test: Expression.Equal(source, NullConstant),
                    ifTrue: Expression.Constant(null, any.Type),
                    ifFalse: any);
            }
            else
            {
                return any;
            }
        }

        private Expression BindCastSourceNode(QueryNode sourceNode, QueryBinderContext context)
        {
            Expression source;
            if (sourceNode == null)
            {
                // if the cast is on the root i.e $it (~/Products?$filter=NS.PopularProducts/.....),
                // source would be null. So bind null to 'current parameter' at context.
                source = context.CurrentParameter;
            }
            else
            {
                source = Bind(sourceNode, context);
            }

            return source;
        }

        /// <summary>
        /// Binds a <see cref="UnaryOperatorNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="UnaryOperatorNode"/>.
        /// </summary>
        /// <param name="unaryOperatorNode">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindUnaryOperatorNode(UnaryOperatorNode unaryOperatorNode, QueryBinderContext context)
        {
            CheckArgumentNull(unaryOperatorNode, context);

            // No need to handle null-propagation here as CLR already handles it.
            // !(null) = null
            // -(null) = null
            Expression inner = Bind(unaryOperatorNode.Operand, context);
            switch (unaryOperatorNode.OperatorKind)
            {
                case UnaryOperatorKind.Negate:
                    return Expression.Negate(inner);

                case UnaryOperatorKind.Not:
                    return Expression.Not(inner);

                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, unaryOperatorNode.Kind, typeof(QueryBinder).Name);
            }
        }

        #endregion

        #region Bind Node methods
        /// <summary>
        /// Binds a <see cref="ConstantNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="ConstantNode"/>.
        /// </summary>
        /// <param name="constantNode">The node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindConstantNode(ConstantNode constantNode, QueryBinderContext context)
        {
            CheckArgumentNull(constantNode, context);

            // no need to parameterize null's as there cannot be multiple values for null.
            if (constantNode.Value == null)
            {
                return NullConstant;
            }

            object value = constantNode.Value;
            Type constantType = RetrieveClrTypeForConstant(constantNode.TypeReference, context, ref value);

            if (context.QuerySettings.EnableConstantParameterization)
            {
                return LinqParameterContainer.Parameterize(constantType, value);
            }
            else
            {
                return Expression.Constant(value, constantType);
            }
        }

        /// <summary>
        /// Binds a <see cref="CollectionConstantNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionConstantNode"/>.
        /// </summary>
        /// <param name="node">The query node to bind.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionConstantNode(CollectionConstantNode node, QueryBinderContext context)
        {
            CheckArgumentNull(node, context);

            // It's fine if the collection is empty; the returned value will be an empty list.
            ConstantNode firstNode = node.Collection.FirstOrDefault();
            object value = null;
            if (firstNode != null)
            {
                value = firstNode.Value;
            }

            Type constantType = RetrieveClrTypeForConstant(node.ItemType, context, ref value);
            Type nullableConstantType = node.ItemType.IsNullable && constantType.IsValueType && Nullable.GetUnderlyingType(constantType) == null
                ? typeof(Nullable<>).MakeGenericType(constantType)
                : constantType;
            Type listType = typeof(List<>).MakeGenericType(nullableConstantType);
            IList castedList = Activator.CreateInstance(listType) as IList;

            // Getting a LINQ expression to dynamically cast each item in the Collection during runtime is tricky,
            // so using a foreach loop and doing an implicit cast from object to the CLR type of ItemType.
            foreach (ConstantNode item in node.Collection)
            {
                object member;
                if (item.Value == null)
                {
                    member = null;
                }
                else if (constantType.IsEnum)
                {
                    member = EnumDeserializationHelpers.ConvertEnumValue(item.Value, constantType);
                }
                else
                {
                    member = item.Value;
                }

                castedList.Add(member);
            }

            if (context.QuerySettings.EnableConstantParameterization)
            {
                return LinqParameterContainer.Parameterize(listType, castedList);
            }

            return Expression.Constant(castedList, listType);
        }
        #endregion

        #region Private helper methods
        private static void ValidateAllStringArguments(string functionName, Expression[] arguments)
        {
            if (arguments.Any(arg => arg.Type != typeof(string)))
            {
                throw new ODataException(Error.Format(SRResources.FunctionNotSupportedOnEnum, functionName));
            }
        }

        /// <summary>
        /// Recognize $it.Source where $it is FlatteningWrapper
        /// Using that do avoid wrapping it redundant into Null propagation 
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>true/false.</returns>
        private static bool IsFlatteningSource(Expression source, QueryBinderContext context)
        {
            var member = source as MemberExpression;
            return member != null
                && context.CurrentParameter.Type.IsGenericType
                && context.CurrentParameter.Type.GetGenericTypeDefinition() == typeof(FlatteningWrapper<>)
                && member.Expression == context.CurrentParameter;
        }

        private static void CollectAssigments(IDictionary<string, Expression> flattenPropertyContainer, Expression source, MemberInitExpression expression, string prefix = null)
        {
            if (expression == null)
            {
                return;
            }

            string nameToAdd = null;
            Type resultType = null;
            MemberInitExpression nextExpression = null;
            Expression nestedExpression = null;
            foreach (var expr in expression.Bindings.OfType<MemberAssignment>())
            {
                var initExpr = expr.Expression as MemberInitExpression;
                if (initExpr != null && expr.Member.Name == "Next")
                {
                    nextExpression = initExpr;
                }
                else if (expr.Member.Name == "Name")
                {
                    nameToAdd = (expr.Expression as ConstantExpression).Value as string;
                }
                else if (expr.Member.Name == "Value" || expr.Member.Name == "NestedValue")
                {
                    resultType = expr.Expression.Type;
                    if (resultType == typeof(object) && expr.Expression.NodeType == ExpressionType.Convert)
                    {
                        resultType = ((UnaryExpression)expr.Expression).Operand.Type;
                    }

                    if (typeof(GroupByWrapper).IsAssignableFrom(resultType))
                    {
                        nestedExpression = expr.Expression;
                    }
                }
            }

            if (prefix != null)
            {
                nameToAdd = prefix + "\\" + nameToAdd;
            }

            if (typeof(GroupByWrapper).IsAssignableFrom(resultType))
            {
                flattenPropertyContainer.Add(nameToAdd, Expression.Property(source, "NestedValue"));
            }
            else
            {
                flattenPropertyContainer.Add(nameToAdd, Expression.Convert(Expression.Property(source, "Value"), resultType));
            }

            if (nextExpression != null)
            {
                CollectAssigments(flattenPropertyContainer, Expression.Property(source, "Next"), nextExpression, prefix);
            }

            if (nestedExpression != null)
            {
                var nestedAccessor = ((nestedExpression as MemberInitExpression).Bindings.First() as MemberAssignment).Expression as MemberInitExpression;
                var newSource = Expression.Property(Expression.Property(source, "NestedValue"), "GroupByContainer");
                CollectAssigments(flattenPropertyContainer, newSource, nestedAccessor, nameToAdd);
            }
        }

        private static MemberInitExpression ExtractContainerExpression(MethodCallExpression expression, string containerName)
        {
            if (expression == null || expression.Arguments.Count < 2)
            {
                return null;
            }

            var memberInitExpression = ((expression.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberInitExpression;
            if (memberInitExpression != null)
            {
                var containerAssigment = memberInitExpression.Bindings.FirstOrDefault(m => m.Member.Name == containerName) as MemberAssignment;
                if (containerAssigment != null)
                {
                    return containerAssigment.Expression as MemberInitExpression;
                }
            }
            return null;
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Bind function arguments
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        protected Expression[] BindArguments(IEnumerable<QueryNode> nodes, QueryBinderContext context)
        {
            return nodes.OfType<SingleValueNode>().Select(n => Bind(n, context)).ToArray();
        }

        /// <summary>
        /// Gets property for dynamic properties dictionary.
        /// </summary>
        /// <param name="openNode"></param>
        /// <param name="context">The query binder context.</param>
        /// <returns>Returns CLR property for dynamic properties container.</returns>
        protected static PropertyInfo GetDynamicPropertyContainer(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
        {
            if (openNode == null)
            {
                throw Error.ArgumentNull(nameof(openNode));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IEdmStructuredType edmStructuredType;
            IEdmTypeReference edmTypeReference = openNode.Source.TypeReference;
            if (edmTypeReference.IsEntity())
            {
                edmStructuredType = edmTypeReference.AsEntity().EntityDefinition();
            }
            else if (edmTypeReference.IsComplex())
            {
                edmStructuredType = edmTypeReference.AsComplex().ComplexDefinition();
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, openNode.Kind, typeof(ExpressionBinderBase).Name);
            }

            return context.Model.GetDynamicPropertyDictionary(edmStructuredType);
        }

        /// <summary>
        /// Gets expression for property from previously aggregated query
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>Returns null if no aggregations were used so far</returns>
        protected Expression GetFlattenedPropertyExpression(string propertyPath, QueryBinderContext context)
        {
            if (context == null || context.ComputedProperties == null || !context.ComputedProperties.Any())
            {
                return null;
            }

            if (context.ComputedProperties.TryGetValue(propertyPath, out var expression))
            {
                return Bind(expression.Expression, context);
            }

            return null;
            // TODO: sam xu, return null?
            // throw new ODataException(Error.Format(SRResources.PropertyOrPathWasRemovedFromContext, propertyPath));
        }
        #endregion

        internal string GetFullPropertyPath(SingleValueNode node)
        {
            string path = null;
            SingleValueNode parent = null;
            switch (node.Kind)
            {
                case QueryNodeKind.SingleComplexNode:
                    var complexNode = (SingleComplexNode)node;
                    path = complexNode.Property.Name;
                    parent = complexNode.Source;
                    break;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propertyNode = ((SingleValuePropertyAccessNode)node);
                    path = propertyNode.Property.Name;
                    parent = propertyNode.Source;
                    break;
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = ((SingleNavigationNode)node);
                    path = navNode.NavigationProperty.Name;
                    parent = navNode.Source;
                    break;
            }

            if (parent != null)
            {
                var parentPath = GetFullPropertyPath(parent);
                if (parentPath != null)
                {
                    path = parentPath + "\\" + path;
                }
            }

            return path;
        }

        internal Expression CreatePropertyAccessExpression(Expression source, QueryBinderContext context, IEdmProperty property, string propertyPath = null)
        {
            string propertyName = context.Model.GetClrPropertyName(property);
            propertyPath = propertyPath ?? propertyName;
            if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && ExpressionBinderHelper.IsNullable(source.Type) &&
                source != context.CurrentParameter &&
                !IsFlatteningSource(source, context))
            {
                Expression cleanSource = ExpressionBinderHelper.RemoveInnerNullPropagation(source, context.QuerySettings);
                Expression propertyAccessExpression = null;
                propertyAccessExpression = GetFlattenedPropertyExpression(propertyPath, context) ?? Expression.Property(cleanSource, propertyName);

                // source.property => source == null ? null : [CastToNullable]RemoveInnerNullPropagation(source).property
                // Notice that we are checking if source is null already. so we can safely remove any null checks when doing source.Property

                Expression ifFalse = ExpressionBinderHelper.ToNullable(ConvertNonStandardPrimitives(propertyAccessExpression, context));
                return
                    Expression.Condition(
                        test: Expression.Equal(source, NullConstant),
                        ifTrue: Expression.Constant(null, ifFalse.Type),
                        ifFalse: ifFalse);
            }
            else
            {
                //   return GetFlattenedPropertyExpression(propertyPath, context)
                //      ?? ConvertNonStandardPrimitives(GetPropertyExpression(source, (!propertyPath.Contains("\\", StringComparison.Ordinal) ? "Instance\\" : String.Empty) + propertyName), context);

                if (context.ElementClrType == typeof(AggregationWrapper))
                {
                    return GetFlattenedPropertyExpression(propertyPath, context)
                        ?? ConvertNonStandardPrimitives(GetPropertyExpression(source, propertyName, isAggregated: true), context);
                }

                return GetFlattenedPropertyExpression(propertyPath, context)
                    ?? ConvertNonStandardPrimitives(GetPropertyExpression(source, propertyName), context);
            }
        }

        internal static Expression GetPropertyExpression(Expression source, string propertyPath, bool isAggregated = false)
        {
            string[] propertyNameParts = propertyPath.Split('\\');
            Expression propertyValue = source;
            foreach (var propertyName in propertyNameParts)
            {
                // Trying to fix problem with $apply and $orderby. https://github.com/OData/AspNetCoreOData/issues/420
                if (isAggregated)
                {
                    propertyValue = Expression.Property(propertyValue, "Values");
                    var propertyInfo = typeof(Dictionary<string, object>).GetProperty("Item");
                    var arguments = new List<Expression> { Expression.Constant(propertyName) };

                    propertyValue = Expression.MakeIndex(propertyValue, propertyInfo, arguments);
                }
                else propertyValue = Expression.Property(propertyValue, propertyName);
            }
            return propertyValue;
        }

        // If the expression is of non-standard edm primitive type (like uint), convert the expression to its standard edm type.
        // Also, note that only expressions generated for ushort, uint and ulong can be understood by linq2sql and EF.
        // The rest (char, char[], Binary) would cause issues with linq2sql and EF.
        internal static Expression ConvertNonStandardPrimitives(Expression source, QueryBinderContext context)
        {
            bool isNonstandardEdmPrimitive;
            Type conversionType = context.Model.IsNonstandardEdmPrimitive(source.Type, out isNonstandardEdmPrimitive);

            if (isNonstandardEdmPrimitive)
            {
                Type sourceType = TypeHelper.GetUnderlyingTypeOrSelf(source.Type);

                Contract.Assert(sourceType != conversionType);

                Expression convertedExpression = null;

                if (TypeHelper.IsEnum(sourceType))
                {
                    // we handle enum conversions ourselves
                    convertedExpression = source;
                }
#if NET6_0
                else if (TypeHelper.IsDateOnly(sourceType) || TypeHelper.IsTimeOnly(sourceType))
                {
                    convertedExpression = source;
                }
#endif
                else
                {
                    switch (Type.GetTypeCode(sourceType))
                    {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            convertedExpression = Expression.Convert(ExpressionBinderHelper.ExtractValueFromNullableExpression(source), conversionType);
                            break;

                        case TypeCode.Char:
                            convertedExpression = Expression.Call(ExpressionBinderHelper.ExtractValueFromNullableExpression(source), "ToString", typeArguments: null, arguments: null);
                            break;

                        case TypeCode.DateTime:
                            convertedExpression = source;
                            break;

                        case TypeCode.Object:
                            if (sourceType == typeof(char[]))
                            {
                                convertedExpression = Expression.New(typeof(string).GetConstructor(new[] { typeof(char[]) }), source);
                            }
                            else if (sourceType == typeof(XElement))
                            {
                                convertedExpression = Expression.Call(source, "ToString", typeArguments: null, arguments: null);
                            }
#if NETFX // System.Data.Linq.Binary is only supported in the AspNet version.
                            else if (sourceType == typeof(Binary))
                            {
                                convertedExpression = Expression.Call(source, "ToArray", typeArguments: null, arguments: null);
                            }
#endif
                            break;

                        default:
                            Contract.Assert(false, Error.Format("missing non-standard type support for {0}", sourceType.Name));
                            break;
                    }
                }

                if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && ExpressionBinderHelper.IsNullable(source.Type))
                {
                    // source == null ? null : source
                    return Expression.Condition(
                        ExpressionBinderHelper.CheckForNull(source),
                        ifTrue: Expression.Constant(null, ExpressionBinderHelper.ToNullable(convertedExpression.Type)),
                        ifFalse: ExpressionBinderHelper.ToNullable(convertedExpression));
                }
                else
                {
                    return convertedExpression;
                }
            }

            return source;
        }

        internal Expression CreateConvertExpression(ConvertNode convertNode, Expression source, QueryBinderContext context)
        {
            Type conversionType = context.Model.GetClrType(convertNode.TypeReference, context.AssembliesResolver);

            if (conversionType == typeof(bool?) && source.Type == typeof(bool))
            {
                // we handle null propagation ourselves. So, if converting from bool to Nullable<bool> ignore.
                return source;
            }
            else if (conversionType == typeof(Date?) &&
                (source.Type == typeof(DateTimeOffset?) || source.Type == typeof(DateTime?)))
            {
                return source;
            }
            if ((conversionType == typeof(TimeOfDay?) && source.Type == typeof(TimeOfDay)) ||
                ((conversionType == typeof(Date?) && source.Type == typeof(Date))))
            {
                return source;
            }
            else if (conversionType == typeof(TimeOfDay?) &&
                (source.Type == typeof(DateTimeOffset?) || source.Type == typeof(DateTime?) || source.Type == typeof(TimeSpan?)))
            {
                return source;
            }
            else if (ExpressionBinderHelper.IsDateAndTimeRelated(conversionType) && ExpressionBinderHelper.IsDateAndTimeRelated(source.Type))
            {
                return source;
            }
            else if (source == NullConstant)
            {
                return source;
            }
            else
            {
                if (TypeHelper.IsEnum(source.Type))
                {
                    // we handle enum conversions ourselves
                    return source;
                }
                else
                {
                    // if a cast is from Nullable<T> to Non-Nullable<T> we need to check if source is null
                    if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True
                        && ExpressionBinderHelper.IsNullable(source.Type) && !ExpressionBinderHelper.IsNullable(conversionType))
                    {
                        // source == null ? null : source.Value
                        return
                            Expression.Condition(
                            test: ExpressionBinderHelper.CheckForNull(source),
                            ifTrue: Expression.Constant(null, ExpressionBinderHelper.ToNullable(conversionType)),
                            ifFalse: Expression.Convert(ExpressionBinderHelper.ExtractValueFromNullableExpression(source), ExpressionBinderHelper.ToNullable(conversionType)));
                    }
                    else
                    {
                        return Expression.Convert(source, conversionType);
                    }
                }
            }
        }

        internal Type RetrieveClrTypeForConstant(IEdmTypeReference edmTypeReference, QueryBinderContext context, ref object value)
        {
            Type constantType = context.Model.GetClrType(edmTypeReference, context.AssembliesResolver);

            if (value != null && edmTypeReference != null && edmTypeReference.IsEnum())
            {
                ODataEnumValue odataEnumValue = (ODataEnumValue)value;
                string strValue = odataEnumValue.Value;
                Contract.Assert(strValue != null);

                constantType = Nullable.GetUnderlyingType(constantType) ?? constantType;

                IEdmEnumType enumType = edmTypeReference.AsEnum().EnumDefinition();
                ClrEnumMemberAnnotation memberMapAnnotation = context.Model.GetClrEnumMemberAnnotation(enumType);
                if (memberMapAnnotation != null)
                {
                    IEdmEnumMember enumMember = enumType.Members.FirstOrDefault(m => m.Name == strValue);
                    if (enumMember == null)
                    {
                        enumMember = enumType.Members.FirstOrDefault(m => m.Value.ToString() == strValue);
                    }

                    if (enumMember != null)
                    {
                        Enum clrMember = memberMapAnnotation.GetClrEnumMember(enumMember);
                        if (clrMember != null)
                        {
                            value = clrMember;
                        }
                        else
                        {
                            throw new ODataException(Error.Format(SRResources.CannotGetEnumClrMember, enumMember.Name));
                        }
                    }
                    else
                    {
                        value = Enum.Parse(constantType, strValue);
                    }
                }
                else
                {
                    value = Enum.Parse(constantType, strValue);
                }
            }

            if (edmTypeReference != null &&
                edmTypeReference.IsNullable &&
                (edmTypeReference.IsDate() || edmTypeReference.IsTimeOfDay()))
            {
                constantType = Nullable.GetUnderlyingType(constantType) ?? constantType;
            }

            return constantType;
        }

        internal Expression BindCastToEnumType(Type sourceType, Type targetClrType, QueryNode firstParameter, int parameterLength, QueryBinderContext context)
        {
            Type enumType = TypeHelper.GetUnderlyingTypeOrSelf(targetClrType);
            ConstantNode sourceNode = firstParameter as ConstantNode;

            if (parameterLength == 1 || sourceNode == null || sourceType != typeof(string))
            {
                // We only support to cast Enumeration type from constant string now,
                // because LINQ to Entities does not recognize the method Enum.TryParse.
                return NullConstant;
            }
            else
            {
                object[] parameters = new[] { sourceNode.Value, Enum.ToObject(enumType, 0) };
                bool isSuccessful = (bool)EnumTryParseMethod.MakeGenericMethod(enumType).Invoke(null, parameters);

                if (isSuccessful)
                {
                    if (context.QuerySettings.EnableConstantParameterization)
                    {
                        return LinqParameterContainer.Parameterize(targetClrType, parameters[1]);
                    }
                    else
                    {
                        return Expression.Constant(parameters[1], targetClrType);
                    }
                }
                else
                {
                    return NullConstant;
                }
            }
        }

        private static Expression Any(Expression source, Expression filter)
        {
            Contract.Assert(source != null);
            Type elementType;
            TypeHelper.IsCollection(source.Type, out elementType);
            Contract.Assert(elementType != null);

            if (filter == null)
            {
                if (ExpressionBinderHelper.IsIQueryable(source.Type))
                {
                    return Expression.Call(null, ExpressionHelperMethods.QueryableEmptyAnyGeneric.MakeGenericMethod(elementType), source);
                }
                else
                {
                    return Expression.Call(null, ExpressionHelperMethods.EnumerableEmptyAnyGeneric.MakeGenericMethod(elementType), source);
                }
            }
            else
            {
                if (ExpressionBinderHelper.IsIQueryable(source.Type))
                {
                    return Expression.Call(null, ExpressionHelperMethods.QueryableNonEmptyAnyGeneric.MakeGenericMethod(elementType), source, filter);
                }
                else
                {
                    return Expression.Call(null, ExpressionHelperMethods.EnumerableNonEmptyAnyGeneric.MakeGenericMethod(elementType), source, filter);
                }
            }
        }

        private static Expression All(Expression source, Expression filter)
        {
            Contract.Assert(source != null);
            Contract.Assert(filter != null);

            Type elementType;
            TypeHelper.IsCollection(source.Type, out elementType);
            Contract.Assert(elementType != null);

            if (ExpressionBinderHelper.IsIQueryable(source.Type))
            {
                return Expression.Call(null, ExpressionHelperMethods.QueryableAllGeneric.MakeGenericMethod(elementType), source, filter);
            }
            else
            {
                return Expression.Call(null, ExpressionHelperMethods.EnumerableAllGeneric.MakeGenericMethod(elementType), source, filter);
            }
        }

        private Expression BindPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode, PropertyInfo prop, QueryBinderContext context)
        {
            var source = Bind(openNode.Source, context);
            Expression propertyAccessExpression;
            if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True &&
                ExpressionBinderHelper.IsNullable(source.Type) && source != context.CurrentParameter)
            {
                propertyAccessExpression = Expression.Property(ExpressionBinderHelper.RemoveInnerNullPropagation(source, context.QuerySettings), prop.Name);
            }
            else
            {
                propertyAccessExpression = Expression.Property(source, prop.Name);
            }
            return propertyAccessExpression;
        }

        /// <summary>
        /// Apply null propagation for filter body.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        protected static Expression ApplyNullPropagationForFilterBody(Expression body, QueryBinderContext context)
        {
            Contract.Assert(body != null);
            Contract.Assert(context != null);

            if (ExpressionBinderHelper.IsNullable(body.Type))
            {
                if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // handle null as false
                    // body => body == true. passing liftToNull:false would convert null to false.
                    body = Expression.Equal(body, Expression.Constant(true, typeof(bool?)), liftToNull: false, method: null);
                }
                else
                {
                    body = Expression.Convert(body, typeof(bool));
                }
            }

            return body;
        }

        private static void CheckArgumentNull<T>(T node, QueryBinderContext context) where T : QueryNode
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }
        }
    }
}