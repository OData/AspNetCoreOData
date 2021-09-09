//-----------------------------------------------------------------------------
// <copyright file="IFilterBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Exposes the ability to translate an OData $filter parse tree represented by <see cref="FilterClause"/> to
    /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>.
    /// </summary>
    public interface IFilterBinder
    {
        /// <summary>
        /// Applies an OData $filter parse tree represented by <see cref="FilterClause"/> to an <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="context">An instance of the <see cref="FilterBinderContext"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
        IQueryable Bind(FilterBinderContext context);

        /// <summary>
        /// Creates an <see cref="Expression"/> from an OData $filter parse tree represented by <see cref="FilterClause"/>.
        /// </summary>
        /// <param name="context">An instance of the <see cref="FilterBinderContext"/>.</param>
        /// <returns>An <see cref="Expression"/> which can be later applied to an <see cref="IQueryable"/>.</returns>
        Expression BindFilterClause(FilterBinderContext context);

        /// <summary>
        /// Creates an <see cref="Expression"/> from an OData $orderby parse tree represented by <see cref="OrderByClause"/>.
        /// </summary>
        /// <param name="context">An instance of the <see cref="FilterBinderContext"/>.</param>
        /// <returns>An <see cref="Expression"/> which can be later applied to an <see cref="IQueryable"/>.</returns>
        Expression BindOrderByClause(FilterBinderContext context);

        /// <summary>
        /// Creates an <see cref="Expression"/> from two expressions.
        /// </summary>
        /// <param name="binaryOperator">The <see cref="BinaryOperatorKind"/> for the returned <see cref="Expression"/>.</param>
        /// <param name="left">The <see cref="Expression"/> on the left side.</param>
        /// <param name="right">The <see cref="Expression"/> on the right side.</param>
        /// <param name="liftToNull">If the operator's return type is lifted to a nullable type.</param>
        /// <returns>An <see cref="Expression"/>.</returns>
        Expression CreateBinaryExpression(BinaryOperatorKind binaryOperator, Expression left, Expression right, bool liftToNull);
    }
}
