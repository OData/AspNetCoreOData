//-----------------------------------------------------------------------------
// <copyright file="BinderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Extension methods for Query binders
    /// </summary>
    public static class BinderExtensions
    {
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

            if (string.IsNullOrEmpty(context.QueryProvider))
            {
                context.QueryProvider = source.Provider.GetType().Namespace;
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
    }
}
