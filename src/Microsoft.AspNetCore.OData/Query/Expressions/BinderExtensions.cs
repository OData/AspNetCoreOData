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
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="query"></param>
        /// <param name="filterClause"></param>
        /// <param name="context"></param>
        /// <returns></returns>
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

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            Expression filterExp = binder.BindFilter(filterClause, context);

            MethodInfo whereMethod = ExpressionHelperMethods.EnumerableWhereGeneric.MakeGenericMethod(context.ElementClrType);
            return whereMethod.Invoke(null, new object[] { query, filterExp }) as IEnumerable;
        }

        /// <summary>
        /// Translates an OData $orderby represented by <see cref="OrderByClause"/> to <see cref="Expression"/>.
        /// $orderby=Age
        ///    |--  x => x.Age
        /// </summary>
        /// <param name="filterClause">The orderby clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The orderBy binder result.</returns>
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

            Expression filterExp = binder.BindFilter(filterClause, context);
            return ExpressionHelpers.Where(query, filterExp, context.ElementClrType);
        }

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

        public static IQueryable ApplyBind(this IOrderByBinder binder, IQueryable query, OrderByClause orderByClause, QueryBinderContext context, bool alreadyOrdered = false)
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

        public static Expression ApplyBind(this IOrderByBinder binder, Expression source, OrderByClause orderByClause, QueryBinderContext context)
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
            bool alreadyOrdered = false;
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
    }
}
