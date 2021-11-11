//-----------------------------------------------------------------------------
// <copyright file="FilterOrderByBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    public interface IFilterBinder
    {
        // "ProductName eq 'Doritos'"
        // => "$it => ($it.ProductName == \"Doritos\")"
        Expression BindFilter(FilterClause filter, QueryBinderContext context);
    }

    /// <summary>
    /// The default implementation to bind an OData $filter represented by <see cref="FilterClause"/>
    /// or $orderby represented by <see cref="OrderByClause"/> to an <see cref="Expression"/>.
    /// </summary>
    public class FilterBinder2 : QueryBinder, IFilterBinder
    {
        /// <summary>
        /// Binds an OData $filter represented by <see cref="FilterClause"/> to an <see cref="Expression"/>.
        ///    $filter="ProductName eq 'Doritos'"
        ///       "$it => ($it.ProductName == \"Doritos\")"
        /// </summary>
        /// <param name="filter">The filter clause <see cref="FilterClause"/>.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindFilter(FilterClause filter, QueryBinderContext context)
        {
            if (filter == null)
            {
                throw Error.ArgumentNull(nameof(filter));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // Save the filter binder into the context internally
            // It will be used in sub-filter, for example:  $filter=collectionProp/$count($filter=Name eq 'abc') gt 2
            context.FilterBinder = this;

            Type filterType = context.ElementClrType;

            LambdaExpression filterExpr = BindExpression(filter.Expression, filter.RangeVariable, context);
            filterExpr = Expression.Lambda(ApplyNullPropagationForFilterBody(filterExpr.Body, context), filterExpr.Parameters);

            Type expectedFilterType = typeof(Func<,>).MakeGenericType(filterType, typeof(bool));
            if (filterExpr.Type != expectedFilterType)
            {
                throw Error.Argument("filterType", SRResources.CannotCastFilter, filterExpr.Type.FullName, expectedFilterType.FullName);
            }

            return filterExpr;
        }

        /// <summary>
        /// Binds an OData $orderby represented by <see cref="OrderByClause"/> to an <see cref="Expression"/>.
        /// </summary>
        /// <param name="orderBy">The filter clause <see cref="OrderByClause"/>.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        //public virtual Expression BindOrderBy(OrderByClause orderBy, QueryBinderContext context)
        //{
        //    if (orderBy == null)
        //    {
        //        throw Error.ArgumentNull(nameof(orderBy));
        //    }

        //    if (context == null)
        //    {
        //        throw Error.ArgumentNull(nameof(context));
        //    }

        //    Type elementType = context.ElementClrType;
        //    LambdaExpression orderByLambda = BindExpression(orderBy.Expression, orderBy.RangeVariable, elementType, context);
        //    return orderByLambda;
        //}

        //private LambdaExpression BindExpression(SingleValueNode expression, RangeVariable rangeVariable, Type elementType, QueryBinderContext context)
        //{
        //    ParameterExpression filterParameter = Expression.Parameter(elementType, rangeVariable.Name);

        //    context.AddlambdaParameters(rangeVariable.Name, filterParameter);

        //    // EnsureFlattenedPropertyContainer(filterParameter);

        //    Expression body = Bind(expression, context);
        //    return Expression.Lambda(body, filterParameter);
        //}

       

        /// <summary>
        /// Get $it parameter
        /// </summary>
        /// <returns></returns>
        //protected override ParameterExpression Parameter
        //{
        //    get
        //    {
        //        return this._lambdaParameters[ODataItParameterName];
        //    }
        //}
    }
}
