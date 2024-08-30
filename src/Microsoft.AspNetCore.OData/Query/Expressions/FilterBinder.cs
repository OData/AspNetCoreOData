//-----------------------------------------------------------------------------
// <copyright file="FilterBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions;

/// <summary>
/// The default implementation to bind an OData $filter represented by <see cref="FilterClause"/> to a <see cref="Expression"/>.
/// </summary>
public class FilterBinder : QueryBinder, IFilterBinder
{
    /// <summary>
    /// Translates an OData $filter represented by <see cref="FilterClause"/> to <see cref="Expression"/>.
    /// $filter=Name eq 'Sam'
    ///    |--  $it => $it.Name == "Sam"
    /// </summary>
    /// <param name="filterClause">The filter clause.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The filter binder result.</returns>
    public virtual Expression BindFilter(FilterClause filterClause, QueryBinderContext context)
    {
        if (filterClause == null)
        {
            throw Error.ArgumentNull(nameof(filterClause));
        }

        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        Expression body = Bind(filterClause.Expression, context);

        ParameterExpression filterParameter = context.CurrentParameter;

        LambdaExpression filterExpr = Expression.Lambda(body, filterParameter);

        filterExpr = Expression.Lambda(ApplyNullPropagationForFilterBody(filterExpr.Body, context), filterExpr.Parameters);

        Type elementType = context.ElementClrType;

        Type expectedFilterType = typeof(Func<,>).MakeGenericType(elementType, typeof(bool));
        if (filterExpr.Type != expectedFilterType)
        {
            throw Error.Argument("filterType", SRResources.CannotCastFilter, filterExpr.Type.FullName, expectedFilterType.FullName);
        }

        return filterExpr;
    }
}
