//-----------------------------------------------------------------------------
// <copyright file="BinderExtensions.cs" company=".NET Foundation">
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
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Extension methods for Query binders
    /// </summary>
    public static class BinderExtensions
    {
        private const string DollarThis = "$this";

        /// <summary>
        /// Translates an OData $filter represented by <see cref="FilterClause"/> to <see cref="Expression"/> and apply to <see cref="IEnumerable" />.
        /// </summary>
        /// <param name="binder">The given filter binder.</param>
        /// <param name="query">The given IEnumerable.</param>
        /// <param name="filterClause">The filter clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The applied result.</returns>
        public static IEnumerable ApplyBind(this IFilterBinder binder, IEnumerable query, FilterClause filterClause, QueryBinderContext context)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (query == null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            if (filterClause == null)
            {
                throw Error.ArgumentNull(nameof(filterClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            Expression filterExp = binder.BindFilter(filterClause, context);

            MethodInfo whereMethod = ExpressionHelperMethods.EnumerableWhereGeneric.MakeGenericMethod(context.ElementClrType);
            return whereMethod.Invoke(null, new object[] { query, filterExp }) as IEnumerable;
        }

        /// <summary>
        /// Translates an OData $filter represented by <see cref="FilterClause"/> to <see cref="Expression"/> and apply to <see cref="IQueryable" />.
        /// </summary>
        /// <param name="binder">The given filter binder.</param>
        /// <param name="query">The given queryable.</param>
        /// <param name="filterClause">The filter clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The applied result.</returns>
        public static IQueryable ApplyBind(this IFilterBinder binder, IQueryable query, FilterClause filterClause, QueryBinderContext context)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (query == null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            if (filterClause == null)
            {
                throw Error.ArgumentNull(nameof(filterClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            context.EnsureFlattenedProperties(context.CurrentParameter, query);

            Expression filterExp = binder.BindFilter(filterClause, context);
            return ExpressionHelpers.Where(query, filterExp, context.ElementClrType);
        }

        /// <summary>
        /// Translates an OData $filter represented by <see cref="FilterClause"/> to <see cref="Expression"/> and apply to <see cref="Expression" />.
        /// </summary>
        /// <param name="binder">The given filter binder.</param>
        /// <param name="source">The given source.</param>
        /// <param name="filterClause">The filter clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The applied result.</returns>
        public static Expression ApplyBind(this IFilterBinder binder, Expression source, FilterClause filterClause, QueryBinderContext context)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (source == null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (filterClause == null)
            {
                throw Error.ArgumentNull(nameof(filterClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            Expression filterExp = binder.BindFilter(filterClause, context);

            Type elementType = context.ElementClrType;

            MethodInfo filterMethod;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                filterMethod = ExpressionHelperMethods.QueryableWhereGeneric.MakeGenericMethod(elementType);
            }
            else
            {
                filterMethod = ExpressionHelperMethods.EnumerableWhereGeneric.MakeGenericMethod(elementType);
            }

            return Expression.Call(filterMethod, source, filterExp);
        }

        /// <summary>
        /// Translates an OData $orderby represented by <see cref="OrderByClause"/> to <see cref="Expression"/> and apply to <see cref="IQueryable" />.
        /// </summary>
        /// <param name="binder">The given filter binder.</param>
        /// <param name="query">The given source.</param>
        /// <param name="orderByClause">The filter clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <param name="alreadyOrdered">The boolean value indicating whether it's ordered or not.</param>
        /// <returns>The applied result.</returns>
        public static IQueryable ApplyBind(this IOrderByBinder binder, IQueryable query, OrderByClause orderByClause, QueryBinderContext context, bool alreadyOrdered)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (query == null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            if (orderByClause == null)
            {
                throw Error.ArgumentNull(nameof(orderByClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            OrderByBinderResult orderByResult = binder.BindOrderBy(orderByClause, context);
            IQueryable querySoFar = query;

            Type elementType = context.ElementClrType;
            OrderByBinderResult result = orderByResult;
            do
            {
                LambdaExpression orderByExpression = result.OrderByExpression as LambdaExpression;
                Contract.Assert(orderByExpression != null);

                OrderByDirection direction = result.Direction;

                querySoFar = ExpressionHelpers.OrderBy(querySoFar, orderByExpression, direction, elementType, alreadyOrdered);

                alreadyOrdered = true;

                result = result.ThenBy;
            }
            while (result != null);

            return querySoFar;
        }

        /// <summary>
        /// Translates an OData $orderby represented by <see cref="OrderByClause"/> to <see cref="Expression"/> and apply to <see cref="Expression" />.
        /// </summary>
        /// <param name="binder">The given filter binder.</param>
        /// <param name="source">The given source.</param>
        /// <param name="orderByClause">The filter clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <param name="alreadyOrdered">The boolean value indicating whether it's ordered or not.</param>
        /// <returns>The applied result.</returns>
        public static Expression ApplyBind(this IOrderByBinder binder, Expression source, OrderByClause orderByClause, QueryBinderContext context, bool alreadyOrdered)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (source == null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (orderByClause == null)
            {
                throw Error.ArgumentNull(nameof(orderByClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            OrderByBinderResult orderByResult = binder.BindOrderBy(orderByClause, context);

            Type elementType = context.ElementClrType;
            OrderByBinderResult result = orderByResult;
            do
            {
                LambdaExpression orderByExpression = result.OrderByExpression as LambdaExpression;
                Contract.Assert(orderByExpression != null);

                OrderByDirection direction = result.Direction;

                source = ExpressionHelpers.OrderBy(source, orderByExpression, elementType, direction, alreadyOrdered);

                alreadyOrdered = true;

                result = result.ThenBy;
            }
            while (result != null);

            return source;
        }

        /// <summary>
        /// Translate an OData $select or $expand parse tree represented by <see cref="SelectExpandClause"/> to
        /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>. ALso <see cref="IQueryable"/>
        /// </summary>
        /// <param name="binder">The built in <see cref="ISelectExpandBinder"/></param>
        /// <param name="source">The original <see cref="IQueryable"/>.</param>
        /// <param name="selectExpandClause">The OData $select or $expand parse tree.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <returns></returns>
        public static IQueryable ApplyBind(this ISelectExpandBinder binder, IQueryable source, SelectExpandClause selectExpandClause, QueryBinderContext context)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (source == null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (selectExpandClause == null)
            {
                throw Error.ArgumentNull(nameof(selectExpandClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            Type elementType = context.ElementClrType;

            LambdaExpression projectionLambda = binder.BindSelectExpand(selectExpandClause, context) as LambdaExpression;

            MethodInfo selectMethod = ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(elementType, projectionLambda.Body.Type);
            return selectMethod.Invoke(null, new object[] { source, projectionLambda }) as IQueryable;
        }

        /// <summary>
        /// Translate an OData $select or $expand parse tree represented by <see cref="SelectExpandClause"/> to
        /// an <see cref="Expression"/> and applies it to an <see cref="object"/>.
        /// </summary>
        /// <param name="binder">The built in <see cref="ISelectExpandBinder"/></param>
        /// <param name="source">The original <see cref="object"/>.</param>
        /// <param name="selectExpandClause">The OData $select or $expand parse tree.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <returns></returns>
        public static object ApplyBind(this ISelectExpandBinder binder, object source, SelectExpandClause selectExpandClause, QueryBinderContext context)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (source == null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (selectExpandClause == null)
            {
                throw Error.ArgumentNull(nameof(selectExpandClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            LambdaExpression projectionLambda = binder.BindSelectExpand(selectExpandClause, context) as LambdaExpression;

            return projectionLambda.Compile().DynamicInvoke(source);
        }

        /// <summary>
        /// Translate an OData $search parse tree represented by <see cref="SearchClause"/> to
        /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="binder">The built in <see cref="ISelectExpandBinder"/></param>
        /// <param name="source">The original <see cref="IQueryable"/>.</param>
        /// <param name="searchClause">The OData $search parse tree.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
        /// <returns>The applied result.</returns>
        public static IQueryable ApplyBind(this ISearchBinder binder, IQueryable source, SearchClause searchClause, QueryBinderContext context)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (source == null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (searchClause == null)
            {
                throw Error.ArgumentNull(nameof(searchClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            Expression searchExp = binder.BindSearch(searchClause, context);
            return ExpressionHelpers.Where(source, searchExp, context.ElementClrType);
        }

        /// <summary>
        /// Translate an OData $apply parse tree represented by <see cref="TransformationNode"/> to
        /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="binder">An instance of <see cref="IAggregationBinder"/>.</param>
        /// <param name="source">The original <see cref="IQueryable"/>.</param>
        /// <param name="transformationNode">The OData $apply parse tree.</param>
        /// <param name="context">An instance of the <see cref="QueryBinderContext"/> containing the current query context.</param>
        /// <param name="resultClrType">The type of wrapper used to create an expression from the $apply parse tree.</param>
        /// <returns>The applied result.</returns>
        public static IQueryable ApplyBind(this IAggregationBinder binder, IQueryable source, TransformationNode transformationNode, QueryBinderContext context, out Type resultClrType)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull(nameof(binder));
            }

            if (source == null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            if (transformationNode == null)
            {
                throw Error.ArgumentNull(nameof(transformationNode));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // Ensure that the flattened properties are populated for the current query context.
            context.EnsureFlattenedProperties(context.CurrentParameter, source);

            // ApplyBind may be called multiple times if there are multiple groupby transformations
            // e.g., $apply=groupby((a,b),aggregate(c))/groupby((d),aggregate(e))
            // In this case, the first groupby will be applied to the original source,
            // and the second groupby will be applied to the result of the first groupby
            // There would be no reason to flatten the properties again if they were already flattened
            if (context.FlattenedProperties == null || context.FlattenedProperties.Count == 0)
            {
                AggregationFlatteningResult flatteningResult = binder.FlattenReferencedProperties(
                    transformationNode,
                    source,
                    context);

                if (flatteningResult?.FlattenedExpression != null)
                {
                    Type originalTransformationElementType = context.TransformationElementType;

                    QueryBinderValidator.ValidateFlatteningResult(flatteningResult);
                    context.FlattenedExpressionMapping = flatteningResult.FlattenedPropertiesMapping;
                    context.SetParameter(DollarThis, flatteningResult.RedefinedContextParameter);

                    LambdaExpression flattenedLambda = flatteningResult.FlattenedExpression as LambdaExpression;
                    Contract.Assert(flattenedLambda != null, $"{nameof(flattenedLambda)} != null");
                    Type flattenedType = flattenedLambda.Body.Type;
                    QueryBinderValidator.ValidateFlattenedExpressionType(flattenedType);

                    source = ExpressionHelpers.Select(source, flattenedLambda, originalTransformationElementType);
                }
            }

            // We are aiming for: query.GroupBy($it => new DynamicType1 {...}).Select($it => new DynamicType2 {...})
            // We are doing Grouping even if only aggregate was specified to have a IQueryable after aggregation

            LambdaExpression groupByLambda = binder.BindGroupBy(transformationNode, context) as LambdaExpression;
            Contract.Assert(groupByLambda != null, $"{nameof(groupByLambda)} != null");
            Type groupByType = groupByLambda.Body.Type;
            QueryBinderValidator.ValidateGroupByExpressionType(groupByType);

            // Invoke GroupBy method
            IQueryable grouping = ExpressionHelpers.GroupBy(source, groupByLambda, source.ElementType, groupByType);

            LambdaExpression selectLambda = binder.BindSelect(transformationNode, context) as LambdaExpression;
            Contract.Assert(selectLambda != null, $"{nameof(selectLambda)} != null");
            resultClrType = selectLambda.Body.Type;
            QueryBinderValidator.ValidateSelectExpressionType(resultClrType);

            // Invoke Select method
            Type groupingType = typeof(IGrouping<,>).MakeGenericType(groupByType, context.TransformationElementType);

            IQueryable result = ExpressionHelpers.Select(grouping, selectLambda, groupingType);

            return result;
        }
    }
}
