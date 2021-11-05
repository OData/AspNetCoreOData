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
        /// Creates a <see cref="LambdaExpression"/> from an OData $filter parse tree represented by <see cref="FilterClause"/>.
        /// </summary>
        /// <param name="context">An instance of the <see cref="FilterBinderContext"/>.</param>
        /// <returns>A <see cref="LambdaExpression"/> which can be later applied to an <see cref="IQueryable"/>.</returns>
        LambdaExpression Bind(FilterBinderContext context);
    }
}
