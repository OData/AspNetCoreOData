//-----------------------------------------------------------------------------
// <copyright file="IFilterBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Exposes the ability to translate an OData $filter represented by <see cref="FilterClause"/> to the <see cref="Expression"/>.
    /// </summary>
    public interface IFilterBinder
    {
        /// <summary>
        /// Translates an OData $filter represented by <see cref="FilterClause"/> to <see cref="Expression"/>.
        /// $filter=Name eq 'Sam'
        ///    |--  $it => $it.Name == "Sam"
        /// </summary>
        /// <param name="filterClause">The filter clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The filter binder result.</returns>
        Expression BindFilter(FilterClause filterClause, QueryBinderContext context);
    }
}
