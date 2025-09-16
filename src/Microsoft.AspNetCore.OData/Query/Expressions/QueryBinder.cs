//-----------------------------------------------------------------------------
// <copyright file="QueryBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Microsoft.AspNetCore.OData.Query.Expressions;

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

            case QueryNodeKind.CollectionOpenPropertyAccess:
                return BindCollectionOpenPropertyAccessNode(node as CollectionOpenPropertyAccessNode, context);

            case QueryNodeKind.CollectionComplexNode:
                return BindCollectionComplexNode(node as CollectionComplexNode, context);

            case QueryNodeKind.CollectionResourceCast:
                return BindCollectionResourceCastNode(node as CollectionResourceCastNode, context);

            case QueryNodeKind.CollectionConstant:
                return BindCollectionConstantNode(node as CollectionConstantNode, context);

            case QueryNodeKind.CollectionFunctionCall:
            case QueryNodeKind.CollectionResourceFunctionCall:
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
    /// Binds a <see cref="CollectionPropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
    /// represents the semantics of the <see cref="CollectionPropertyAccessNode"/>.
    /// </summary>
    /// <param name="openCollectionNode">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    public virtual Expression BindCollectionOpenPropertyAccessNode(CollectionOpenPropertyAccessNode openCollectionNode, QueryBinderContext context)
    {
        CheckArgumentNull(openCollectionNode, context);

        if (context.ElementClrType.IsDynamicTypeWrapper())
        {
            return GetFlattenedPropertyExpression(openCollectionNode.Name, context) ?? Expression.Property(
                Bind(openCollectionNode.Source, context), openCollectionNode.Name);
        }

        if (context.ComputedProperties.TryGetValue(openCollectionNode.Name, out ComputeExpression computedProperty))
        {
            return Bind(computedProperty.Expression, context);
        }

        if (openCollectionNode.Source is SingleValueOpenPropertyAccessNode
            || openCollectionNode.Source is NonResourceRangeVariableReferenceNode)
        {
            return BindNestedCollectionOpenPropertyAccessExpression(
                openCollectionNode,
                context);
        }
        else
        {
            return BindCollectionOpenPropertyAccessExpression(openCollectionNode, context);
        }
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
            return GetFlattenedPropertyExpression(GetFullPropertyPath(openNode), context) ?? Expression.Property(Bind(openNode.Source, context), openNode.Name);
        }

        if (context.ComputedProperties.TryGetValue(openNode.Name, out ComputeExpression computedProperty))
        {
            return Bind(computedProperty.Expression, context);
        }

        if (openNode.Source is SingleValueOpenPropertyAccessNode
            || openNode.Source is NonResourceRangeVariableReferenceNode)
        {
            return BindNestedDynamicPropertyAccessExpression(openNode, context);
        }
        else
        {
            return BindDynamicPropertyAccessExpression(openNode, context);
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
        if (context.Source != null && rangeVariable.Name == "$it")
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

        IEdmTypeReference targetEdmType = null;
        QueryNode queryNode = node.Parameters.Last();
        if (queryNode is ConstantNode constantNode)
        {
            targetEdmType = model.FindType((string)constantNode.Value).ToEdmTypeReference(false);
        }
        else if (queryNode is SingleResourceCastNode singleResourceCastNode)
        {
            targetEdmType = singleResourceCastNode.TypeReference;
        }
        else
        {
            throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, queryNode.Kind, nameof(BindSingleResourceCastFunctionCall));
        }

        Type targetClrType = null;
        if (targetEdmType != null)
        {
            targetClrType = model.GetClrType(targetEdmType);
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

    /// <summary>
    /// Creates an <see cref="Expression"/> from the <see cref="QueryNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="QueryNode"/> to be bound.</param>
    /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
    /// <param name="baseElement">The <see cref="Expression"/> for the base element.</param>
    /// <returns>The created <see cref="Expression"/>.</returns>
    public virtual Expression BindAccessExpression(QueryNode node, QueryBinderContext context, Expression baseElement = null)
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
                return context.CurrentParameter.Type.IsFlatteningWrapper()
                    ? (Expression)Expression.Property(context.CurrentParameter, QueryConstants.FlatteningWrapperSourceProperty)
                    : context.CurrentParameter;

            case QueryNodeKind.SingleValuePropertyAccess:
                SingleValuePropertyAccessNode singleValueNode = node as SingleValuePropertyAccessNode;
                return CreatePropertyAccessExpression(
                    BindAccessExpression(singleValueNode.Source, context, baseElement),
                    context,
                    singleValueNode.Property,
                    GetFullPropertyPath(singleValueNode));

            case QueryNodeKind.AggregatedCollectionPropertyNode:
                AggregatedCollectionPropertyNode aggregatedCollectionNode = node as AggregatedCollectionPropertyNode;
                return CreatePropertyAccessExpression(
                    BindAccessExpression(aggregatedCollectionNode.Source, context, baseElement),
                    context,
                    aggregatedCollectionNode.Property);

            case QueryNodeKind.SingleComplexNode:
                SingleComplexNode singleComplexNode = node as SingleComplexNode;
                return CreatePropertyAccessExpression(
                    BindAccessExpression(singleComplexNode.Source, context, baseElement),
                    context,
                    singleComplexNode.Property,
                    GetFullPropertyPath(singleComplexNode));

            case QueryNodeKind.SingleValueOpenPropertyAccess:
                SingleValueOpenPropertyAccessNode openNode = node as SingleValueOpenPropertyAccessNode;
                return GetFlattenedPropertyExpression(openNode.Name, context) ?? CreateOpenPropertyAccessExpression(openNode, context);

            case QueryNodeKind.None:
            case QueryNodeKind.SingleNavigationNode:
                SingleNavigationNode navigationNode = node as SingleNavigationNode;
                return CreatePropertyAccessExpression(
                    BindAccessExpression(navigationNode.Source, context),
                    context,
                    navigationNode.NavigationProperty,
                    GetFullPropertyPath(navigationNode));

            case QueryNodeKind.BinaryOperator:
                BinaryOperatorNode binaryNode = node as BinaryOperatorNode;
                Expression leftExpression = BindAccessExpression(binaryNode.Left, context, baseElement);
                Expression rightExpression = BindAccessExpression(binaryNode.Right, context, baseElement);
                return ExpressionBinderHelper.CreateBinaryExpression(
                    binaryNode.OperatorKind,
                    leftExpression,
                    rightExpression,
                    liftToNull: true,
                    context.QuerySettings);

            case QueryNodeKind.Convert:
                ConvertNode convertNode = node as ConvertNode;
                return CreateConvertExpression(convertNode, BindAccessExpression(convertNode.Source, context, baseElement), context);

            case QueryNodeKind.CollectionNavigationNode:
                return baseElement ?? context.CurrentParameter;

            case QueryNodeKind.SingleValueFunctionCall:
                return BindSingleValueFunctionCallNode(node as SingleValueFunctionCallNode, context);

            case QueryNodeKind.Constant:
                return BindConstantNode(node as ConstantNode, context);

            case QueryNodeKind.SingleResourceCast:
                SingleResourceCastNode singleResourceCastNode = node as SingleResourceCastNode;
                return BindSingleResourceCastNode(singleResourceCastNode, context);

            default:
                throw Error.NotSupported(
                    SRResources.QueryNodeBindingNotSupported,
                    node.Kind,
                    typeof(AggregationBinder).Name);
        }
    }

    /// <summary>
    /// Creates a LINQ <see cref="Expression"/> that represents the semantics of the <see cref="SingleValueOpenPropertyAccessNode"/>.
    /// </summary>
    /// <param name="openNode">They query node to create an expression from.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    public virtual Expression CreateOpenPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
    {
        Expression source = BindAccessExpression(openNode.Source, context);

        // First check that property exists in source
        // It's the case when we are apply transformation based on earlier transformation
        if (source.Type.GetProperty(openNode.Name) != null)
        {
            return Expression.Property(source, openNode.Name);
        }

        // Property doesn't exists go for dynamic properties dictionary
        PropertyInfo prop = GetDynamicPropertyContainer(openNode, context);
        MemberExpression propertyAccessExpression = Expression.Property(source, prop.Name);

        return CreateDynamicPropertyAccessExpression(propertyAccessExpression, openNode.Name, context);
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
        MemberExpression member = source as MemberExpression;
        return member != null
            && context.CurrentParameter.Type.IsFlatteningWrapper()
            && member.Expression == context.CurrentParameter;
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
    /// <param name="openNode">The single-valued open property access node.</param>
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

        return GetDynamicPropertyContainer(openNode.Source.TypeReference, openNode.Kind, context.Model);
    }

    /// <summary>
    /// Gets property for dynamic properties dictionary.
    /// </summary>
    /// <param name="openCollectionNode">The collection-valued open property access node.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>Returns CLR property for dynamic properties container.</returns>
    protected static PropertyInfo GetDynamicPropertyContainer(CollectionOpenPropertyAccessNode openCollectionNode, QueryBinderContext context)
    {
        if (openCollectionNode == null)
        {
            throw Error.ArgumentNull(nameof(openCollectionNode));
        }

        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        return GetDynamicPropertyContainer(openCollectionNode.Source.TypeReference, openCollectionNode.Kind, context.Model);
    }

    /// <summary>
    /// Gets expression for property from previously aggregated query
    /// </summary>
    /// <param name="propertyPath">The property path.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>Returns null if no aggregations were used so far</returns>
    protected Expression GetFlattenedPropertyExpression(string propertyPath, QueryBinderContext context)
    {
        if (context?.FlattenedProperties is null || context.FlattenedProperties.Count == 0)
        {
            return null;
        }

        if (context.FlattenedProperties.TryGetValue(propertyPath, out Expression expression))
        {
            return expression;
        }

        return null;
        // TODO: sam xu, return null?
        // throw new ODataException(Error.Format(SRResources.PropertyOrPathWasRemovedFromContext, propertyPath));
    }

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
    #endregion

    internal static string GetFullPropertyPath(SingleValueNode node)
    {
        string path = null;
        SingleValueNode parent = null;
        switch (node.Kind)
        {
            case QueryNodeKind.SingleComplexNode:
                SingleComplexNode complexNode = (SingleComplexNode)node;
                path = complexNode.Property.Name;
                parent = complexNode.Source;
                break;
            case QueryNodeKind.SingleValuePropertyAccess:
                SingleValuePropertyAccessNode propertyNode = (SingleValuePropertyAccessNode)node;
                path = propertyNode.Property.Name;
                parent = propertyNode.Source;
                break;
            case QueryNodeKind.SingleNavigationNode:
                SingleNavigationNode navNode = (SingleNavigationNode)node;
                path = navNode.NavigationProperty.Name;
                parent = navNode.Source;
                break;
            case QueryNodeKind.SingleValueOpenPropertyAccess:
                SingleValueOpenPropertyAccessNode openPropertyNode = (SingleValueOpenPropertyAccessNode)node;
                path = openPropertyNode.Name;
                parent = openPropertyNode.Source;
                break;
        }

        if (parent != null)
        {
            string parentPath = GetFullPropertyPath(parent);
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
            Expression propertyAccessExpression = propertyAccessExpression = GetFlattenedPropertyExpression(propertyPath, context);
            if (propertyAccessExpression == null)
            {
                Expression propertyExpression = source.Type.IsComputeWrapper(out _)
                    // e.g., if source.Type is ComputeWrapper<Sale>, for Amount property, the valid source will be $it.Instance
                    ? GetPropertyExpression(source, QueryConstants.ComputeWrapperInstanceProperty)
                    : source;

                propertyExpression = GetPropertyExpression(propertyExpression, propertyName);
                propertyAccessExpression = ConvertNonStandardPrimitives(propertyExpression, context);
            }

            return propertyAccessExpression;
        }
    }

    private static Expression GetPropertyExpression(Expression source, string propertyPath)
    {
        string[] propertyNameParts = propertyPath.Split('\\');
        Expression propertyValue = source;
        foreach (string propertyName in propertyNameParts)
        {
            propertyValue = Expression.Property(propertyValue, propertyName);

            // Consider a scenario like:
            // Sales?$apply=compute(Amount mul Product/TaxRate as Tax)/compute(Amount add Tax as Total,Amount div 10 as Discount)/compute(Total sub Discount as SalePrice)
            // To get to the Amount property, we'll need the expression $it->Instance->Instance->Amount
            // $it is of type ComputeWrapper<T1> (where T1 is of type ComputeWrapper<T2>)
            // $it->Instance is of type ComputeWrapper<T2> (where T2 is of type Sale)
            // $it->Instance->Instance is of type Sale
            // $it=>Instance->Instance->Amount is the correct member expression for Amount property
            if (propertyValue.Type.IsComputeWrapper(out _))
            {
                propertyValue = GetPropertyExpression(propertyValue, QueryConstants.ComputeWrapperInstanceProperty);
            }
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
            else if (TypeHelper.IsDateOnly(sourceType) || TypeHelper.IsTimeOnly(sourceType))
            {
                convertedExpression = source;
            }
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
                        Contract.Assert(false, Error.Format(SRResources.MissingNonStandardTypeSupportFor, sourceType.Name));
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

        if (edmTypeReference != null && edmTypeReference.IsUntyped())
        {
            constantType = typeof(object);
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

    internal static IDictionary<string, Expression> GetFlattenedProperties(ParameterExpression source, QueryBinderContext context, IQueryable query)
    {
        if (!query.ElementType.IsGroupByWrapper())
        {
            return null;
        }

        MethodCallExpression expression = query.Expression as MethodCallExpression;
        if (expression == null)
        {
            return null;
        }

        // After $apply we could have other clauses, like $filter, $orderby etc.
        // Skip all filter expressions
        expression = SkipFilters(expression);

        if (expression == null)
        {
            return null;
        }

        Dictionary<string, Expression> flattenedPropertiesMap = new Dictionary<string, Expression>();
        CollectContainerAssignments(source, expression, flattenedPropertiesMap);
        if (query?.ElementType?.IsComputeWrapper(out _) == true)
        {
            MemberExpression instanceProperty = Expression.Property(source, QueryConstants.ComputeWrapperInstanceProperty);
            if (typeof(DynamicTypeWrapper).IsAssignableFrom(instanceProperty.Type))
            {
                MethodCallExpression computeExpression = expression.Arguments.FirstOrDefault() as MethodCallExpression;
                computeExpression = SkipFilters(computeExpression);
                if (computeExpression != null)
                {
                    CollectContainerAssignments(instanceProperty, computeExpression, flattenedPropertiesMap);
                }
            }
        }

        return flattenedPropertiesMap;
    }

    private static MethodCallExpression SkipFilters(MethodCallExpression expression)
    {
        while (expression.Method.Name == "Where")
        {
            expression = expression.Arguments.FirstOrDefault() as MethodCallExpression;
        }

        return expression;
    }

    private static void CollectContainerAssignments(Expression source, MethodCallExpression expression, Dictionary<string, Expression> result)
    {
        CollectAssignments(
            result,
            Expression.Property(source, QueryConstants.GroupByWrapperGroupByContainerProperty),
            ExtractContainerExpression(expression.Arguments.FirstOrDefault() as MethodCallExpression, QueryConstants.GroupByWrapperGroupByContainerProperty));
        CollectAssignments(
            result,
            Expression.Property(source, QueryConstants.GroupByWrapperContainerProperty),
            ExtractContainerExpression(expression, QueryConstants.GroupByWrapperContainerProperty));
    }

    private static void CollectAssignments(IDictionary<string, Expression> flattenPropertyContainer, Expression source, MemberInitExpression expression, string prefix = null)
    {
        if (expression == null)
        {
            return;
        }

        string nameToAdd = null;
        Type resultType = null;
        MemberInitExpression nextExpression = null;
        Expression nestedExpression = null;
        foreach (MemberAssignment expr in expression.Bindings.OfType<MemberAssignment>())
        {
            MemberInitExpression initExpr = expr.Expression as MemberInitExpression;
            if (initExpr != null && expr.Member.Name == QueryConstants.AggregationPropertyContainerNextProperty)
            {
                nextExpression = initExpr;
            }
            else if (expr.Member.Name == QueryConstants.AggregationPropertyContainerNameProperty)
            {
                nameToAdd = (expr.Expression as ConstantExpression).Value as string;
            }
            else if (expr.Member.Name == QueryConstants.AggregationPropertyContainerValueProperty || expr.Member.Name == QueryConstants.AggregationPropertyContainerNestedValueProperty)
            {
                resultType = expr.Expression.Type;
                if (resultType == typeof(object) && expr.Expression.NodeType == ExpressionType.Convert)
                {
                    resultType = ((UnaryExpression)expr.Expression).Operand.Type;
                }

                if (resultType.IsGroupByWrapper())
                {
                    nestedExpression = expr.Expression;
                }
            }
        }

        if (prefix != null)
        {
            nameToAdd = prefix + "\\" + nameToAdd;
        }

        if (resultType.IsGroupByWrapper())
        {
            flattenPropertyContainer.Add(nameToAdd, Expression.Property(source, QueryConstants.AggregationPropertyContainerNestedValueProperty));
        }
        else
        {
            source = ConvertToAggregationPropertyContainerIfNeeded(source);

            flattenPropertyContainer.Add(nameToAdd, Expression.Convert(Expression.Property(source, QueryConstants.AggregationPropertyContainerValueProperty), resultType));
        }

        if (nextExpression != null)
        {
            CollectAssignments(flattenPropertyContainer, Expression.Property(source, QueryConstants.AggregationPropertyContainerNextProperty), nextExpression, prefix);
        }

        if (nestedExpression != null)
        {
            MemberInitExpression nestedAccessor = ((nestedExpression as MemberInitExpression).Bindings.First() as MemberAssignment).Expression as MemberInitExpression;
            MemberExpression newSource = Expression.Property(Expression.Property(source, QueryConstants.AggregationPropertyContainerNestedValueProperty), QueryConstants.GroupByWrapperGroupByContainerProperty);
            CollectAssignments(flattenPropertyContainer, newSource, nestedAccessor, nameToAdd);
        }
    }

    private static MemberInitExpression ExtractContainerExpression(MethodCallExpression expression, string containerName)
    {
        if (expression == null || expression.Arguments.Count < 2)
        {
            return null;
        }

        MemberInitExpression memberInitExpression = ((expression.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberInitExpression;
        if (memberInitExpression != null)
        {
            MemberAssignment containerAssignment = memberInitExpression.Bindings.FirstOrDefault(m => m.Member.Name == containerName) as MemberAssignment;
            if (containerAssignment != null)
            {
                return containerAssignment.Expression as MemberInitExpression;
            }
        }

        return null;
    }

    /// <summary>
    /// Wraps <paramref name="source"/> expression with a Convert expression to enforce type <see cref="AggregationPropertyContainer"/> if needed.
    /// </summary>
    /// <param name="source">The source expression.</param>
    /// <returns>The wrapped expression, or the original <paramref name="source"/> expression, as the case may be.</returns>
    /// <remarks>
    /// Wrapping with a Convert expression is only needed when dealing with <see cref="AggregationPropertyContainer"/> (default implementation).
    /// It's needed because <see cref="AggregationPropertyContainer"/> inherits from <see cref="NamedProperty{T}"/> - meaning that
    /// <see cref="AggregationPropertyContainer"/> implements <see cref="IAggregationPropertyContainer{TWrapper, TContainer}"/> "Name" and "Value" properties
    /// indirectly via <see cref="NamedProperty{T}"/>.
    /// Without Convert({expression}, typeof(AggregationPropertyContainer)), the "Value" property cannot be resolved and translation fails.
    /// </remarks>
    private static Expression ConvertToAggregationPropertyContainerIfNeeded(Expression source)
    {
        // NOTE: We should reconsider the inheritance of AggregationPropertyContainer from NamedProperty<T> for the following reasons:
        // 1) Unnecessary complexity for aggregation scenarios
        // - The NamedProperty<T> type, which inherits from PropertyContainer, was designed primarily for SelectExpand scenarios.
        // - It introduces a significant amount of logic that is not required for aggregation.
        // - Aggregation functionality only depends on the Name and Value properties, as well as the ToDictionaryCore method.
        // - The ToDictionaryCore method requires a boolean includeAutoSelected parameter, which is irrelevant in aggregation scenarios and is simply ignored.
        // 2) Challenges in exposing wrapper and container types as public API
        // - The inheritance structure complicates making essential wrapper types (e.g., GroupByWrapper, FlatteningWrapper<T>) and container types (e.g., AggregationPropertyContainer) public.
        // - Exposing these types as part of the public API is necessary to enable subclassing AggregationBinder, instead of forcing developers to implement IAggregationBinder from scratch.
        // - However, due to the dependency on NamedProperty<T>, making these types public would require exposing approximately 128 private types from PropertyContainer, which is impractical.

        Debug.Assert(source != null, $"{nameof(source)} != null");

        Expression targetSource = source;

        do
        {
            if (targetSource.Type.InheritsFromGenericBase(typeof(NamedProperty<>)))
            {
                return Expression.Convert(source, typeof(AggregationPropertyContainer));
            }

        } while ((targetSource is MemberExpression memberExpression) && ((targetSource = memberExpression.Expression) != null));

        return source;
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

    /// <summary>
    /// Binds property to the source node.
    /// </summary>
    /// <param name="sourceNode">The source node.</param>
    /// <param name="prop">The property.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private Expression BindPropertyAccessExpression(SingleValueNode sourceNode, PropertyInfo prop, QueryBinderContext context)
    {
        Expression source = Bind(sourceNode, context);

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
    /// Gets property for dynamic properties dictionary.
    /// </summary>
    /// <param name="edmTypeReference">The Edm type reference.</param>
    /// <param name="queryNodeKind">Query node kind.</param>
    /// <param name="model">The Edm model.</param>
    /// <returns>Returns CLR property for dynamic properties container.</returns>
    private static PropertyInfo GetDynamicPropertyContainer(IEdmTypeReference edmTypeReference, QueryNodeKind queryNodeKind, IEdmModel model)
    {
        Debug.Assert(edmTypeReference != null, $"{nameof(edmTypeReference) != null}");

        IEdmStructuredType edmStructuredType;
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
            throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, queryNodeKind, typeof(QueryBinder).Name);
        }

        if (!edmStructuredType.IsOpen)
        {
            throw Error.NotSupported(SRResources.TypeMustBeOpenType, edmStructuredType.FullTypeName());
        }

        return model.GetDynamicPropertyDictionary(edmStructuredType);
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

    /// <summary>
    /// Binds dynamic property to the source node.
    /// </summary>
    /// <param name="openNode">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private Expression BindDynamicPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
    {
        PropertyInfo prop = GetDynamicPropertyContainer(openNode, context);

        Expression containerPropertyAccessExpression = BindPropertyAccessExpression(openNode.Source, prop, context);

        return CreateDynamicPropertyAccessExpression(containerPropertyAccessExpression, openNode.Name, context);
    }

    /// <summary>
    /// Creates an expression for retrieving a dynamic property from the container property.
    /// </summary>
    /// <param name="containerPropertyAccessExpr">The container property access expression.</param>
    /// <param name="propertyName">The dynamic property name.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private static Expression CreateDynamicPropertyAccessExpression(
        Expression containerPropertyAccessExpr,
        string propertyName,
        QueryBinderContext context)
    {
        // Get dynamic property value
        IndexExpression dictionaryIndexExpr = Expression.Property(
            containerPropertyAccessExpr, DictionaryStringObjectIndexerName,
            Expression.Constant(propertyName));

        // ContainsKey method
        MethodCallExpression containsKeyExpression = Expression.Call(
            containerPropertyAccessExpr,
            containerPropertyAccessExpr.Type.GetMethod("ContainsKey"),
            Expression.Constant(propertyName));

        // Handle null propagation
        if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
        {
            BinaryExpression dictionaryIsNotNullExpr = Expression.NotEqual(containerPropertyAccessExpr, NullConstant);
            BinaryExpression dictionaryIsNotNullAndContainsKeyExpr = Expression.AndAlso(dictionaryIsNotNullExpr, containsKeyExpression);
            return Expression.Condition(
                dictionaryIsNotNullAndContainsKeyExpr,
                dictionaryIndexExpr,
                NullConstant);
        }
        else
        {
            return Expression.Condition(
                containsKeyExpression,
                dictionaryIndexExpr,
                NullConstant);
        }
    }

    /// <summary>
    /// Binds nested dynamic property to the source node.
    /// </summary>
    /// <param name="openNode">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private Expression BindNestedDynamicPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
    {
        // NOTE: Every segment after a dynamic property segment will be represented
        // as a dynamic segment though it may not strictly resolve a dynamic property.
        // The URI parser is unable to determine that so we handle that here.

        // For the expression $filter=DynamicProperty/Property eq '{value}',
        // the Property segment will be a SingleValueOpenPropertyAccessNode but
        // can resolve into either a declared or dynamic property
        // Similarly, for %filter=DynamicProperty/Level1Property/Level2Property,
        // both Level1Property and Level2Property will be SingleValueOpenPropertyAccessNode but
        // can resolve into either declared or dynamic properties

        Expression sourceExpr = null;

        if (openNode.Source is SingleValueOpenPropertyAccessNode openNodeParent)
        {
            sourceExpr = BindNestedDynamicPropertyAccessExpression(openNodeParent, context);
        }
        else if (openNode.Source is NonResourceRangeVariableReferenceNode nonResourceRangeVariableReferenceNode)
        {
            return CreateNestedDynamicPropertyAccessExpression(
                BindRangeVariable(nonResourceRangeVariableReferenceNode.RangeVariable, context),
                openNode,
                context);
        }
        else
        {
            return BindDynamicPropertyAccessExpression(openNode, context);
        }

        return CreateNestedDynamicPropertyAccessExpression(sourceExpr, openNode, context);
    }

    /// <summary>
    /// Creates an expression for retrieving a nested dynamic property.
    /// </summary>
    /// <param name="sourceExpr">The source expression.</param>
    /// <param name="openNode">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private static Expression CreateNestedDynamicPropertyAccessExpression(
        Expression sourceExpr,
        SingleValueOpenPropertyAccessNode openNode,
        QueryBinderContext context)
    {
        // Scenario 1: Declared property => DynamicProperty/DeclaredProperty
        // Yields the equivalent of:
        // dynamicPropertyValue.GetType().GetProperty(propertyName).GetValue(dynamicPropertyValue)

        Expression getInstanceTypeExpr = Expression.Call(sourceExpr, "GetType", Type.EmptyTypes);

        // Get declared property
        Expression getPropertyExpr = Expression.Call(
            getInstanceTypeExpr,
            typeof(Type).GetMethod("GetProperty", new[] { typeof(string) }),
            Expression.Constant(openNode.Name));

        // Get declared property value
        Expression getDeclaredPropertyValueExpr = Expression.Call(
            getPropertyExpr,
            "GetValue",
            Type.EmptyTypes,
            sourceExpr);

        // SCENARIO 2: Dynamic property =>  DynamicProperty/DynamicProperty
        // Yields the equivalent of:
        // dynamicPropertyValue.GetType().GetProperty({DynamicContainerPropertyName}).GetValue(dynamicPropertyValue).Item[propertyName]

        // Get dynamic property from container property
        Expression getDynamicPropertyValueExpr = CreateNestedDynamicPropertyAccessExpression(
            sourceExpr,
            openNode.Name,
            openNode.Kind,
            context);

        // We only consider scenario 2 if scenario 1 doesn't apply
        Expression getValueExpr = Expression.Condition(
            Expression.NotEqual(getPropertyExpr, NullConstant),
            getDeclaredPropertyValueExpr,
            getDynamicPropertyValueExpr);

        // Handle null propagation
        if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
        {
            return Expression.Condition(
                Expression.Equal(sourceExpr, NullConstant),
                NullConstant,
                getValueExpr);
        }

        return getValueExpr;
    }

    /// <summary>
    /// Binds dynamic collection property to the source node.
    /// </summary>
    /// <param name="openCollectionNode">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private Expression BindCollectionOpenPropertyAccessExpression(CollectionOpenPropertyAccessNode openCollectionNode, QueryBinderContext context)
    {
        PropertyInfo containerProperty = GetDynamicPropertyContainer(openCollectionNode, context);

        Expression containerPropertyAccessExpr = BindPropertyAccessExpression(openCollectionNode.Source, containerProperty, context);

        IndexExpression dynamicPropertyValueExpr = Expression.Property(containerPropertyAccessExpr,
            DictionaryStringObjectIndexerName, Expression.Constant(openCollectionNode.Name));

        // Method call expression for ((IEnumerable)value).Cast<object>()
        MethodCallExpression dynamicPropertyValueToEnumerableCastExpr = Expression.Call(
            ExpressionHelperMethods.EnumerableCastGeneric.MakeGenericMethod(typeof(object)),
            Expression.Convert(dynamicPropertyValueExpr, typeof(IEnumerable)));

        // Throw exception if value is not IEnumerable
        Expression dynamicPropertyIsNotCollectionExceptionExpr = Expression.Throw(
            CreatePropertyIsNotCollectionExceptionExpression(
                Expression.Constant(context.Model.GetClrType(openCollectionNode.Source.TypeReference), typeof(Type)),
                Expression.Call(dynamicPropertyValueExpr, "GetType", Type.EmptyTypes),
                openCollectionNode.Name),
            typeof(IEnumerable<object>));

        // Conditional expression to throw exception if value is not IEnumerable, else perform Cast<object>()
        Expression dynamicPropertyConditionalCastExpr = Expression.Condition(
            Expression.TypeIs(dynamicPropertyValueExpr, typeof(IEnumerable)),
            dynamicPropertyValueToEnumerableCastExpr,
            dynamicPropertyIsNotCollectionExceptionExpr);

        Expression containsKeyExpr = Expression.Call(containerPropertyAccessExpr,
            containerPropertyAccessExpr.Type.GetMethod("ContainsKey"), Expression.Constant(openCollectionNode.Name));

        // Return null if not IEnumerable
        Expression defaultNullExpr = Expression.Convert(NullConstant, typeof(IEnumerable<object>));

        if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
        {
            Expression dynamicDictIsNotNullExpr = Expression.NotEqual(containerPropertyAccessExpr, NullConstant);
            Expression dynamicDictIsNotNullAndContainsKeyExpr = Expression.AndAlso(dynamicDictIsNotNullExpr, containsKeyExpr);
            return Expression.Condition(
                dynamicDictIsNotNullAndContainsKeyExpr,
                dynamicPropertyConditionalCastExpr,
                defaultNullExpr);
        }
        else
        {
            return Expression.Condition(
                containsKeyExpr,
                dynamicPropertyConditionalCastExpr,
                defaultNullExpr);
        }
    }

    /// <summary>
    /// Binds nested dynamic collection property to the source node.
    /// </summary>
    /// <param name="openCollectionNode">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private Expression BindNestedCollectionOpenPropertyAccessExpression(CollectionOpenPropertyAccessNode openCollectionNode, QueryBinderContext context)
    {
        // NOTE: Every segment after a dynamic property segment will be represented
        // as a dynamic segment though it may resolve a declared property.
        // The URI parser is unable to determine that so we handle that here.

        // For the expression $filter=DynamicSingleValuedProperty/CollectionValuedProperty/any(...),
        // the CollectionValuedProperty segment will be a CollectionOpenPropertyAccessNode but
        // can resolve into either a declared or dynamic property. The parser is unable to determine this

        Contract.Assert(openCollectionNode.Source is SingleValueOpenPropertyAccessNode
            || openCollectionNode.Source is NonResourceRangeVariableReferenceNode);

        Expression sourceExpr = null;

        if (openCollectionNode.Source is SingleValueOpenPropertyAccessNode openNodeParent)
        {
            sourceExpr = BindNestedDynamicPropertyAccessExpression(openNodeParent, context);
        }
        else if (openCollectionNode.Source is NonResourceRangeVariableReferenceNode nonResourceRangeVariableReferenceNode)
        {
            sourceExpr = BindRangeVariable(nonResourceRangeVariableReferenceNode.RangeVariable, context);
        }
        else
        {
            Contract.Assert(sourceExpr == null);
        }

        return CreateNestedCollectionOpenPropertyAccessExpression(sourceExpr, openCollectionNode, context);
    }

    /// <summary>
    /// Creates an expression for retrieving a nested dynamic collection property.
    /// </summary>
    /// <param name="sourceExpr">The source expression.</param>
    /// <param name="openCollectionNode">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private static Expression CreateNestedCollectionOpenPropertyAccessExpression(
        Expression sourceExpr,
        CollectionOpenPropertyAccessNode openCollectionNode,
        QueryBinderContext context)
    {
        // Scenario 1: Nested declared collection property => DynamicProperty/DeclaredCollectionValuedProperty
        // Yields the equivalent of:
        // (IEnumerable<object>)dynamicPropertyValue.GetType().GetProperty(propertyName).GetValue(dynamicPropertyValue)

        Expression getInstanceTypeExpr = Expression.Call(sourceExpr, "GetType", Type.EmptyTypes);

        // Get declared property
        Expression getPropertyExpr = Expression.Call(
            getInstanceTypeExpr,
            typeof(Type).GetMethod("GetProperty", new[] { typeof(string) }),
            Expression.Constant(openCollectionNode.Name));

        // Get declared property value
        Expression getDeclaredPropertyValueExpr = Expression.Call(
            getPropertyExpr,
            "GetValue",
            Type.EmptyTypes,
            sourceExpr);

        // Method call expression for ((IEnumerable)value).Cast<object>()
        MethodCallExpression declaredPropertyValueToEnumerableCastExpr = Expression.Call(
            ExpressionHelperMethods.EnumerableCastGeneric.MakeGenericMethod(typeof(object)),
            Expression.Convert(getDeclaredPropertyValueExpr, typeof(IEnumerable)));

        // Throw exception if value is not IEnumerable
        Expression declaredPropertyIsNotCollectionExceptionExpr = Expression.Throw(
            CreatePropertyIsNotCollectionExceptionExpression(
                getInstanceTypeExpr,
                Expression.Call(getDeclaredPropertyValueExpr, "GetType", Type.EmptyTypes),
                openCollectionNode.Name),
            typeof(IEnumerable<object>));

        // Conditional expression to throw exception if value is not IEnumerable, else perform Cast<object>()
        Expression declaredPropertyConditionalCastExpr = Expression.Condition(
            Expression.TypeIs(getDeclaredPropertyValueExpr, typeof(IEnumerable)),
            declaredPropertyValueToEnumerableCastExpr,
            declaredPropertyIsNotCollectionExceptionExpr);

        // SCENARIO 2: Nested dynamic collection property =>  DynamicProperty/DynamicCollectionProperty
        // Yields the equivalent of:
        // (IEnumerable<object>)dynamicPropertyValue.GetType().GetProperty({DynamicContainerPropertyName}).GetValue(dynamicPropertyValue).Item[propertyName]

        // Get dynamic property from container property
        Expression getDynamicPropertyValueExpr = CreateNestedDynamicPropertyAccessExpression(
            sourceExpr,
            openCollectionNode.Name,
            openCollectionNode.Kind,
            context);

        // Method call expression for ((IEnumerable)value).Cast<object>()
        MethodCallExpression dynamicPropertyValueToEnumerableCastExpr = Expression.Call(
            ExpressionHelperMethods.EnumerableCastGeneric.MakeGenericMethod(typeof(object)),
            Expression.Convert(getDynamicPropertyValueExpr, typeof(IEnumerable)));

        // Throw exception if value is not IEnumerable
        Expression dynamicPropertyIsNotCollectionExceptionExpr = Expression.Throw(
            CreatePropertyIsNotCollectionExceptionExpression(
                getInstanceTypeExpr,
                Expression.Call(getDynamicPropertyValueExpr, "GetType", Type.EmptyTypes),
                openCollectionNode.Name),
            typeof(IEnumerable<object>));

        // Conditional expression to throw exception if value is not IEnumerable, else perform Cast<object>()
        Expression dynamicPropertyConditionCastExpr = Expression.Condition(
            Expression.NotEqual(getDynamicPropertyValueExpr, NullConstant),
            Expression.Condition(
                Expression.TypeIs(getDynamicPropertyValueExpr, typeof(IEnumerable)),
                dynamicPropertyValueToEnumerableCastExpr,
                dynamicPropertyIsNotCollectionExceptionExpr),
           Expression.Convert(NullConstant, typeof(IEnumerable<object>)));

        // We only consider scenario 2 if scenario 1 doesn't apply
        Expression getPropertyValueExpr = Expression.Condition(
            Expression.NotEqual(getPropertyExpr, NullConstant),
            declaredPropertyConditionalCastExpr,
            dynamicPropertyConditionCastExpr);

        // Handle null propagation
        if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
        {
            return Expression.Condition(
                Expression.Equal(sourceExpr, NullConstant),
                Expression.Constant(null, typeof(IEnumerable<object>)),
                getPropertyValueExpr);
        }

        return getPropertyValueExpr;
    }

    /// <summary>
    /// Creates an expression for retrieving a dynamic property from the container property.
    /// </summary>
    /// <param name="sourceExpr">The source expression.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="queryNodeKind">The query node kind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private static Expression CreateNestedDynamicPropertyAccessExpression(
        Expression sourceExpr,
        string propertyName,
        QueryNodeKind queryNodeKind,
        QueryBinderContext context)
    {
        Expression getInstanceTypeExpr = Expression.Call(sourceExpr, "GetType", Type.EmptyTypes);

        // GetEdmTypeReference method
        MethodInfo getEdmTypeReferenceMethod = typeof(EdmClrTypeMapExtensions).GetMethod(
            nameof(EdmClrTypeMapExtensions.GetEdmTypeReference),
            BindingFlags.Public | BindingFlags.Static,
            new[] { typeof(IEdmModel), typeof(Type) });

        // Get Edm type reference of the property value type
        Expression getEdmTypeReferenceExpr = Expression.Call(
            null,
            getEdmTypeReferenceMethod,
            new Expression[]
            {
                Expression.Constant(context.Model, typeof(IEdmModel)),
                getInstanceTypeExpr
            });

        // Throw exception if GetEdmTypeReference returns null - Resource type not in model
        Expression edmTypeReferenceConditionalExpr = Expression.Condition(
            Expression.Equal(getEdmTypeReferenceExpr, NullConstant),
            Expression.Throw(
                CreateResourceTypeNotInModelExceptionExpression(getInstanceTypeExpr),
                typeof(IEdmTypeReference)),
            getEdmTypeReferenceExpr);

        // GetDynamicPropertyContainer method
        MethodInfo getDynamicPropertyContainerMethod = typeof(QueryBinder).GetMethod(
            nameof(GetDynamicPropertyContainer),
            BindingFlags.NonPublic | BindingFlags.Static,
            new[] { typeof(IEdmTypeReference), typeof(QueryNodeKind), typeof(IEdmModel) });

        // Get container property
        Expression getDynamicPropertyContainerExpr = Expression.Call(
            null,
            getDynamicPropertyContainerMethod,
            edmTypeReferenceConditionalExpr,
            Expression.Constant(queryNodeKind, typeof(QueryNodeKind)),
            Expression.Constant(context.Model, typeof(IEdmModel)));

        // Get container property value - value is a dictionary
        Expression dynamicPropertiesContainerExpr = Expression.Convert(
            Expression.Call(getDynamicPropertyContainerExpr, "GetValue", Type.EmptyTypes, sourceExpr),
            typeof(IDictionary<string, object>));

        // Get dynamic property from container property
        return CreateDynamicPropertyAccessExpression(
            dynamicPropertiesContainerExpr,
            propertyName,
            context);
    }

    /// <summary>
    /// Creates an <see cref="ODataException"/> for the scenario where resource type is not in the Edm model.
    /// </summary>
    /// <param name="instanceTypeExpr">The resource instance type expression.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private static Expression CreateResourceTypeNotInModelExceptionExpression(Expression instanceTypeExpr)
    {
        return Expression.New(
            typeof(ODataException).GetConstructor(new[] { typeof(string) }),
            Expression.Call(
                typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object) }),
                Expression.Constant(SRResources.ResourceTypeNotInModel),
                Expression.Property(instanceTypeExpr, nameof(Type.FullName))));
    }

    /// <summary>
    /// Creates an <see cref="ODataException"/> for the scenario where
    /// any/any operators are applied to a property that is not a collection.
    /// </summary>
    /// <param name="instanceTypeExpr">The resource instance type expression.</param>
    /// <param name="propertyTypeExpr">The property type expression.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    private static Expression CreatePropertyIsNotCollectionExceptionExpression(
        Expression instanceTypeExpr,
        Expression propertyTypeExpr,
        string propertyName)
    {
        return Expression.New(
            typeof(ODataException).GetConstructor(new[] { typeof(string) }),
            Expression.Call(
                typeof(string).GetMethod("Format", new[] { typeof(string), typeof(object), typeof(object), typeof(object) }),
                Expression.Constant(SRResources.PropertyIsNotCollection, typeof(string)),
                Expression.Property(propertyTypeExpr, nameof(Type.FullName)),
                Expression.Constant(propertyName),
                Expression.Property(instanceTypeExpr, nameof(Type.FullName))));
    }

    [DebuggerStepThrough]
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
