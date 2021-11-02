//-----------------------------------------------------------------------------
// <copyright file="FilterOrderByBinderBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Translates an OData $filter parse tree represented by <see cref="FilterClause"/> to
    /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>.
    /// </summary>
    public class FilterOrderByBinderBase : ExpressionBinderBase
    {
        internal const string ODataItParameterName = "$it";
        internal const string ODataThisParameterName = "$this";

        internal Stack<Dictionary<string, ParameterExpression>> _parametersStack = new Stack<Dictionary<string, ParameterExpression>>();
        internal Dictionary<string, ParameterExpression> _lambdaParameters;
        internal Type _filterType;
        internal FilterOrderByBinderBase(IServiceProvider requestContainer)
            : base(requestContainer)
        {
        }
        internal FilterOrderByBinderBase(IEdmModel model, IAssemblyResolver assembliesResolver, ODataQuerySettings settings)
            : base(model, assembliesResolver, settings)
        {
        }

        internal LambdaExpression BindFilterClause(FilterClause filterClause, Type filterType)
        {
            LambdaExpression filter = BindExpression(filterClause.Expression, filterClause.RangeVariable, filterType);
            filter = Expression.Lambda(ApplyNullPropagationForFilterBody(filter.Body), filter.Parameters);

            Type expectedFilterType = typeof(Func<,>).MakeGenericType(filterType, typeof(bool));
            if (filter.Type != expectedFilterType)
            {
                throw Error.Argument("filterType", SRResources.CannotCastFilter, filter.Type.FullName, expectedFilterType.FullName);
            }

            return filter;
        }

        /// <summary>
        /// Binds a <see cref="QueryNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics
        /// of the <see cref="QueryNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public override Expression Bind(QueryNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            CollectionNode collectionNode = node as CollectionNode;
            SingleValueNode singleValueNode = node as SingleValueNode;

            if (collectionNode != null)
            {
                return BindCollectionNode(collectionNode);
            }
            else if (singleValueNode != null)
            {
                return BindSingleValueNode(singleValueNode);
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="CountNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CountNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCountNode(CountNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            Expression source = Bind(node.Source);
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

            MethodInfo whereMethod;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                whereMethod = ExpressionHelperMethods.QueryableWhereGeneric.MakeGenericMethod(elementType);
            }
            else
            {
                whereMethod = ExpressionHelperMethods.EnumerableWhereGeneric.MakeGenericMethod(elementType);
            }

            // Bind the inner $filter clause within the $count segment.
            // e.g Books?$filter=Authors/$count($filter=Id gt 1) gt 1
            Expression filterExpression = null;
            if (node.FilterClause != null)
            {
                filterExpression = BindFilterClause(node.FilterClause, elementType);

                // The source expression looks like: $it.Authors
                // So the generated countExpression below will look like: $it.Authors.Where($it => $it.Id > 1)
                source = Expression.Call(null, whereMethod, new[] { source, filterExpression });
            }

            // append LongCount() method.
            // The final countExpression with the nested $filter clause will look like: $it.Authors.Where($it => $it.Id > 1).LongCount()
            // The final countExpression without the nested $filter clause will look like: $it.Authors.LongCount()
            countExpression = Expression.Call(null, countMethod, new[] { source });

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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
        /// Binds a <see cref="SingleValueOpenPropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValueOpenPropertyAccessNode"/>.
        /// </summary>
        /// <param name="openNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindDynamicPropertyAccessQueryNode(SingleValueOpenPropertyAccessNode openNode)
        {
            if (openNode == null)
            {
                throw Error.ArgumentNull(nameof(openNode));
            }

            if (_filterType.IsDynamicTypeWrapper())
            {
                return GetFlattenedPropertyExpression(openNode.Name) ?? Expression.Property(Bind(openNode.Source), openNode.Name);
            }
            PropertyInfo prop = GetDynamicPropertyContainer(openNode);

            var propertyAccessExpression = BindPropertyAccessExpression(openNode, prop);
            var readDictionaryIndexerExpression = Expression.Property(propertyAccessExpression,
                DictionaryStringObjectIndexerName, Expression.Constant(openNode.Name));
            var containsKeyExpression = Expression.Call(propertyAccessExpression,
                propertyAccessExpression.Type.GetMethod("ContainsKey"), Expression.Constant(openNode.Name));
            var nullExpression = Expression.Constant(null);

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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

        private Expression BindPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode, PropertyInfo prop)
        {
            var source = Bind(openNode.Source);
            Expression propertyAccessExpression;
            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True &&
                ExpressionBinderHelper.IsNullable(source.Type) && source != _lambdaParameters[ODataItParameterName])
            {
                propertyAccessExpression = Expression.Property(ExpressionBinderHelper.RemoveInnerNullPropagation(source, QuerySettings), prop.Name);
            }
            else
            {
                propertyAccessExpression = Expression.Property(source, prop.Name);
            }
            return propertyAccessExpression;
        }

        /// <summary>
        /// Binds a <see cref="SingleResourceFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleResourceFunctionCallNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleResourceFunctionCallNode(SingleResourceFunctionCallNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            switch (node.Name)
            {
                case ClrCanonicalFunctions.CastFunctionName:
                    return BindSingleResourceCastFunctionCall(node);
                default:
                    throw Error.NotSupported(SRResources.ODataFunctionNotSupported, node.Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleResourceFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleResourceFunctionCallNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleResourceCastFunctionCall(SingleResourceFunctionCallNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            Contract.Assert(ClrCanonicalFunctions.CastFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 2);

            string targetEdmTypeName = (string)((ConstantNode)node.Parameters.Last()).Value;
            IEdmType targetEdmType = Model.FindType(targetEdmTypeName);
            Type targetClrType = null;

            if (targetEdmType != null)
            {
                targetClrType = Model.GetClrType(targetEdmType.ToEdmTypeReference(false));
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
                    source = BindCastSourceNode(node.Source);
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
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleResourceCastNode(SingleResourceCastNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            IEdmStructuredTypeReference structured = node.StructuredTypeReference;
            Contract.Assert(structured != null, "NS casts can contain only structured types");

            Type clrType = Model.GetClrType(structured);

            Expression source = BindCastSourceNode(node.Source);
            return Expression.TypeAs(source, clrType);
        }

        /// <summary>
        /// Binds a <see cref="CollectionResourceCastNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionResourceCastNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionResourceCastNode(CollectionResourceCastNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            IEdmStructuredTypeReference structured = node.ItemStructuredType;
            Contract.Assert(structured != null, "NS casts can contain only structured types");

            Type clrType = Model.GetClrType(structured);

            Expression source = BindCastSourceNode(node.Source);
            return OfType(source, clrType);
        }

        private Expression BindCastSourceNode(QueryNode sourceNode)
        {
            Expression source;
            if (sourceNode == null)
            {
                // if the cast is on the root i.e $it (~/Products?$filter=NS.PopularProducts/.....),
                // source would be null. So bind null to '$it'.
                source = _lambdaParameters[ODataItParameterName];
            }
            else
            {
                source = Bind(sourceNode);
            }

            return source;
        }

        private static Expression OfType(Expression source, Type elementType)
        {
            Contract.Assert(source != null);
            Contract.Assert(elementType != null);

            if (ExpressionBinderHelper.IsIQueryable(source.Type))
            {
                return Expression.Call(null, ExpressionHelperMethods.QueryableOfType.MakeGenericMethod(elementType), source);
            }
            else
            {
                return Expression.Call(null, ExpressionHelperMethods.EnumerableOfType.MakeGenericMethod(elementType), source);
            }
        }

        /// <summary>
        /// Binds a <see cref="IEdmNavigationProperty"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="IEdmNavigationProperty"/>.
        /// </summary>
        /// <param name="sourceNode">The node that represents the navigation source.</param>
        /// <param name="navigationProperty">The navigation property to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty)
        {
            return BindNavigationPropertyNode(sourceNode, navigationProperty, null);
        }

        /// <summary>
        /// Binds a <see cref="IEdmNavigationProperty"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="IEdmNavigationProperty"/>.
        /// </summary>
        /// <param name="sourceNode">The node that represents the navigation source.</param>
        /// <param name="navigationProperty">The navigation property to bind.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty, string propertyPath)
        {
            Expression source;

            // TODO: bug in uri parser is causing this property to be null for the root property.
            if (sourceNode == null)
            {
                source = _lambdaParameters[ODataItParameterName];
            }
            else
            {
                source = Bind(sourceNode);
            }

            return CreatePropertyAccessExpression(source, navigationProperty, propertyPath);
        }

        /// <summary>
        /// Binds a <see cref="BinaryOperatorNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="BinaryOperatorNode"/>.
        /// </summary>
        /// <param name="binaryOperatorNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode)
        {
            if (binaryOperatorNode == null)
            {
                throw Error.ArgumentNull(nameof(binaryOperatorNode));
            }

            Expression left = Bind(binaryOperatorNode.Left);
            Expression right = Bind(binaryOperatorNode.Right);

            // handle null propagation only if either of the operands can be null
            bool isNullPropagationRequired = QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && (ExpressionBinderHelper.IsNullable(left.Type) || ExpressionBinderHelper.IsNullable(right.Type));
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
                return ExpressionBinderHelper.CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: liftToNull, QuerySettings);
            }
            else
            {
                return ExpressionBinderHelper.CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: false, QuerySettings);
            }
        }

        /// <summary>
        /// Binds an <see cref="InNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="InNode"/>.
        /// </summary>
        /// <param name="inNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindInNode(InNode inNode)
        {
            if (inNode == null)
            {
                throw Error.ArgumentNull(nameof(inNode));
            }

            Expression singleValue = Bind(inNode.Left);
            Expression collection = Bind(inNode.Right);

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
        /// Binds a <see cref="ConvertNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="ConvertNode"/>.
        /// </summary>
        /// <param name="convertNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindConvertNode(ConvertNode convertNode)
        {
            Contract.Assert(convertNode != null);
            Contract.Assert(convertNode.TypeReference != null);

            Expression source = Bind(convertNode.Source);

            return CreateConvertExpression(convertNode, source);
        }

        internal LambdaExpression BindExpression(SingleValueNode expression, RangeVariable rangeVariable, Type elementType)
        {
            ParameterExpression filterParameter = Expression.Parameter(elementType, rangeVariable.Name);
            _lambdaParameters = new Dictionary<string, ParameterExpression>();
            _lambdaParameters.Add(rangeVariable.Name, filterParameter);

            EnsureFlattenedPropertyContainer(filterParameter);

            Expression body = Bind(expression);
            return Expression.Lambda(body, filterParameter);
        }

        /// <summary>
        /// Binds a <see cref="RangeVariable"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="RangeVariable"/>.
        /// </summary>
        /// <param name="rangeVariable">The range variable to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindRangeVariable(RangeVariable rangeVariable)
        {
            if (rangeVariable == null)
            {
                throw Error.ArgumentNull(nameof(rangeVariable));
            }

            ParameterExpression parameter;
            // When we have a $this RangeVariable, we still create a $it parameter.
            // i.e $it => $it instead of $this => $this
            if (rangeVariable.Name == ODataThisParameterName)
            {
                parameter = _lambdaParameters[ODataItParameterName];
            }
            else
            {
                parameter = _lambdaParameters[rangeVariable.Name];
            }

            return ConvertNonStandardPrimitives(parameter);
        }

        /// <summary>
        /// Binds a <see cref="CollectionPropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionPropertyAccessNode"/>.
        /// </summary>
        /// <param name="propertyAccessNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode)
        {
            if (propertyAccessNode == null)
            {
                throw Error.ArgumentNull(nameof(propertyAccessNode));
            }

            Expression source = Bind(propertyAccessNode.Source);
            return CreatePropertyAccessExpression(source, propertyAccessNode.Property);
        }

        /// <summary>
        /// Binds a <see cref="CollectionComplexNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionComplexNode"/>.
        /// </summary>
        /// <param name="collectionComplexNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionComplexNode(CollectionComplexNode collectionComplexNode)
        {
            if (collectionComplexNode == null)
            {
                throw Error.ArgumentNull(nameof(collectionComplexNode));
            }

            Expression source = Bind(collectionComplexNode.Source);
            return CreatePropertyAccessExpression(source, collectionComplexNode.Property);
        }

        /// <summary>
        /// Binds a <see cref="SingleValuePropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValuePropertyAccessNode"/>.
        /// </summary>
        /// <param name="propertyAccessNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindPropertyAccessQueryNode(SingleValuePropertyAccessNode propertyAccessNode)
        {
            if (propertyAccessNode == null)
            {
                throw Error.ArgumentNull(nameof(propertyAccessNode));
            }

            Expression source = Bind(propertyAccessNode.Source);
            return CreatePropertyAccessExpression(source, propertyAccessNode.Property, GetFullPropertyPath(propertyAccessNode));
        }

        /// <summary>
        /// Binds a <see cref="SingleComplexNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleComplexNode"/>.
        /// </summary>
        /// <param name="singleComplexNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleComplexNode(SingleComplexNode singleComplexNode)
        {
            if (singleComplexNode == null)
            {
                throw Error.ArgumentNull(nameof(singleComplexNode));
            }

            Expression source = Bind(singleComplexNode.Source);
            return CreatePropertyAccessExpression(source, singleComplexNode.Property, GetFullPropertyPath(singleComplexNode));
        }

        /// <summary>
        /// Binds a <see cref="UnaryOperatorNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="UnaryOperatorNode"/>.
        /// </summary>
        /// <param name="unaryOperatorNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindUnaryOperatorNode(UnaryOperatorNode unaryOperatorNode)
        {
            if (unaryOperatorNode == null)
            {
                throw Error.ArgumentNull(nameof(unaryOperatorNode));
            }

            // No need to handle null-propagation here as CLR already handles it.
            // !(null) = null
            // -(null) = null
            Expression inner = Bind(unaryOperatorNode.Operand);
            switch (unaryOperatorNode.OperatorKind)
            {
                case UnaryOperatorKind.Negate:
                    return Expression.Negate(inner);

                case UnaryOperatorKind.Not:
                    return Expression.Not(inner);

                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, unaryOperatorNode.Kind, typeof(FilterBinder).Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="AllNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="AllNode"/>.
        /// </summary>
        /// <param name="allNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindAllNode(AllNode allNode)
        {
            if (allNode == null)
            {
                throw Error.ArgumentNull(nameof(allNode));
            }

            ParameterExpression allIt = HandleLambdaParameters(allNode.RangeVariables);

            Expression source;
            Contract.Assert(allNode.Source != null);
            source = Bind(allNode.Source);

            Expression body = source;
            Contract.Assert(allNode.Body != null);

            body = Bind(allNode.Body);
            body = ApplyNullPropagationForFilterBody(body);
            body = Expression.Lambda(body, allIt);

            Expression all = All(source, body);

            ExitLamdbaScope();

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && ExpressionBinderHelper.IsNullable(source.Type))
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
        /// <param name="anyNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindAnyNode(AnyNode anyNode)
        {
            if (anyNode == null)
            {
                throw Error.ArgumentNull(nameof(anyNode));
            }

            ParameterExpression anyIt = HandleLambdaParameters(anyNode.RangeVariables);

            Expression source;
            Contract.Assert(anyNode.Source != null);
            source = Bind(anyNode.Source);

            Expression body = null;
            // uri parser places an Constant node with value true for empty any() body
            if (anyNode.Body != null && anyNode.Body.Kind != QueryNodeKind.Constant)
            {
                body = Bind(anyNode.Body);
                body = ApplyNullPropagationForFilterBody(body);
                body = Expression.Lambda(body, anyIt);
            }
            else if (anyNode.Body != null && anyNode.Body.Kind == QueryNodeKind.Constant
                && (bool)(anyNode.Body as ConstantNode).Value == false)
            {
                // any(false) is the same as just false
                ExitLamdbaScope();
                return FalseConstant;
            }

            Expression any = Any(source, body);

            ExitLamdbaScope();

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && ExpressionBinderHelper.IsNullable(source.Type))
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

        /// <summary>
        /// Get $it parameter
        /// </summary>
        /// <returns></returns>
        protected override ParameterExpression Parameter
        {
            get
            {
                return this._lambdaParameters[ODataItParameterName];
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleValueNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics
        /// of the <see cref="SingleValueNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        private Expression BindSingleValueNode(SingleValueNode node)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.BinaryOperator:
                    return BindBinaryOperatorNode(node as BinaryOperatorNode);

                case QueryNodeKind.Constant:
                    return BindConstantNode(node as ConstantNode);

                case QueryNodeKind.Convert:
                    return BindConvertNode(node as ConvertNode);

                case QueryNodeKind.ResourceRangeVariableReference:
                    return BindRangeVariable((node as ResourceRangeVariableReferenceNode).RangeVariable);

                case QueryNodeKind.NonResourceRangeVariableReference:
                    return BindRangeVariable((node as NonResourceRangeVariableReferenceNode).RangeVariable);

                case QueryNodeKind.SingleValuePropertyAccess:
                    return BindPropertyAccessQueryNode(node as SingleValuePropertyAccessNode);

                case QueryNodeKind.SingleComplexNode:
                    return BindSingleComplexNode(node as SingleComplexNode);

                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    return BindDynamicPropertyAccessQueryNode(node as SingleValueOpenPropertyAccessNode);

                case QueryNodeKind.UnaryOperator:
                    return BindUnaryOperatorNode(node as UnaryOperatorNode);

                case QueryNodeKind.SingleValueFunctionCall:
                    return BindSingleValueFunctionCallNode(node as SingleValueFunctionCallNode);

                case QueryNodeKind.SingleNavigationNode:
                    SingleNavigationNode navigationNode = node as SingleNavigationNode;
                    return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty, GetFullPropertyPath(navigationNode));

                case QueryNodeKind.Any:
                    return BindAnyNode(node as AnyNode);

                case QueryNodeKind.All:
                    return BindAllNode(node as AllNode);

                case QueryNodeKind.SingleResourceCast:
                    return BindSingleResourceCastNode(node as SingleResourceCastNode);

                case QueryNodeKind.SingleResourceFunctionCall:
                    return BindSingleResourceFunctionCallNode(node as SingleResourceFunctionCallNode);

                case QueryNodeKind.In:
                    return BindInNode(node as InNode);

                case QueryNodeKind.Count:
                    return BindCountNode(node as CountNode);

                case QueryNodeKind.NamedFunctionParameter:
                case QueryNodeKind.ParameterAlias:
                case QueryNodeKind.EntitySet:
                case QueryNodeKind.KeyLookup:
                case QueryNodeKind.SearchTerm:
                // Unused or have unknown uses.
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="CollectionNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics
        /// of the <see cref="CollectionNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        private Expression BindCollectionNode(CollectionNode node)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.CollectionNavigationNode:
                    CollectionNavigationNode navigationNode = node as CollectionNavigationNode;
                    return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty);

                case QueryNodeKind.CollectionPropertyAccess:
                    return BindCollectionPropertyAccessNode(node as CollectionPropertyAccessNode);

                case QueryNodeKind.CollectionComplexNode:
                    return BindCollectionComplexNode(node as CollectionComplexNode);

                case QueryNodeKind.CollectionResourceCast:
                    return BindCollectionResourceCastNode(node as CollectionResourceCastNode);

                case QueryNodeKind.CollectionConstant:
                    return BindCollectionConstantNode(node as CollectionConstantNode);

                case QueryNodeKind.CollectionFunctionCall:
                case QueryNodeKind.CollectionResourceFunctionCall:
                case QueryNodeKind.CollectionOpenPropertyAccess:
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
            }
        }

        private ParameterExpression HandleLambdaParameters(IEnumerable<RangeVariable> rangeVariables)
        {
            ParameterExpression lambdaIt = null;

            EnterLambdaScope();

            Dictionary<string, ParameterExpression> newParameters = new Dictionary<string, ParameterExpression>();
            foreach (RangeVariable rangeVariable in rangeVariables)
            {
                ParameterExpression parameter;

                // Create a Parameter Expression for rangeVariables which are not $it Lambda parameters or $this.
                if (!_lambdaParameters.TryGetValue(rangeVariable.Name, out parameter) && rangeVariable.Name != ODataThisParameterName)
                {
                    // Work-around issue 481323 where UriParser yields a collection parameter type
                    // for primitive collections rather than the inner element type of the collection.
                    // Remove this block of code when 481323 is resolved.
                    IEdmTypeReference edmTypeReference = rangeVariable.TypeReference;
                    IEdmCollectionTypeReference collectionTypeReference = edmTypeReference as IEdmCollectionTypeReference;
                    if (collectionTypeReference != null)
                    {
                        IEdmCollectionType collectionType = collectionTypeReference.Definition as IEdmCollectionType;
                        if (collectionType != null)
                        {
                            edmTypeReference = collectionType.ElementType;
                        }
                    }

                    parameter = Expression.Parameter(Model.GetClrType(edmTypeReference, InternalAssembliesResolver), rangeVariable.Name);
                    Contract.Assert(lambdaIt == null, "There can be only one parameter in an Any/All lambda");
                    lambdaIt = parameter;
                }
                newParameters.Add(rangeVariable.Name, parameter);
            }

            _lambdaParameters = newParameters;
            return lambdaIt;
        }

        private void EnterLambdaScope()
        {
            Contract.Assert(_lambdaParameters != null);
            _parametersStack.Push(_lambdaParameters);
        }

        private void ExitLamdbaScope()
        {
            if (_parametersStack.Count != 0)
            {
                _lambdaParameters = _parametersStack.Pop();
            }
            else
            {
                _lambdaParameters = null;
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

        internal Expression ApplyNullPropagationForFilterBody(Expression body)
        {
            if (ExpressionBinderHelper.IsNullable(body.Type))
            {
                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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
    }
}
