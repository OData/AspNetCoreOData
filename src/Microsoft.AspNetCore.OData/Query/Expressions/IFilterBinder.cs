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
        /// <param name="source">The original <see cref="IQueryable"/>.</param>
        /// <param name="context">An instance of the <see cref="FilterBinderContext"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
        IQueryable Bind(IQueryable source, FilterBinderContext context);

        /// <summary>
        /// Creates an <see cref="LambdaExpression"/> from an OData $filter parse tree represented by <see cref="FilterClause"/>.
        /// </summary>
        /// <param name="source">The original <see cref="IQueryable"/>.</param>
        /// <param name="context">An instance of the <see cref="FilterBinderContext"/>.</param>
        /// <returns>An <see cref="LambdaExpression"/> which can be later applied to an <see cref="IQueryable"/>.</returns>
        LambdaExpression BindFilterClause(IQueryable source, FilterBinderContext context);
    }
}
