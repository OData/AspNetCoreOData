//-----------------------------------------------------------------------------
// <copyright file="IOrderByBinder.cs" company=".NET Foundation">
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
    /// Exposes the ability to translate an OData $orderby parse tree represented by <see cref="OrderByClause"/> to
    /// an <see cref="Expression"/>.
    /// </summary>
    public interface IOrderByBinder
    {
        /// <summary>
        /// Creates a <see cref="LambdaExpression"/> from an OData $orderby parse tree represented by <see cref="OrderByClause"/>.
        /// </summary>
        /// <param name="context">An instance of the <see cref="OrderByBinderContext"/>.</param>
        /// <returns>The <see cref="LambdaExpression"/> after the orderby query has been translated.</returns>
        LambdaExpression Bind(OrderByBinderContext context);

        /// <summary>
        /// Applies an OData orderby parse tree represented by <see cref="OrderByClause"/> to an <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="source">The original <see cref="IQueryable"/>.</param>
        /// <param name="context">An instance of the <see cref="OrderByBinderContext"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
        IQueryable Bind(IQueryable source, OrderByBinderContext context);
    }
}
