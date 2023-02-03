//-----------------------------------------------------------------------------
// <copyright file="IODataQueryOptionsProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// The context for OData query options provider.
    /// </summary>
    public class QueryOptionsProviderContext
    {
        /// <summary>
        /// The HttpContext.
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// The Action context.
        /// </summary>
        public ActionContext ActionContext { get; set; }
    }

    /// <summary>
    /// Exposes the ability to create <see cref="ODataQueryOptions" /> for a certain request on a certain action.
    /// </summary>
    public interface IODataQueryOptionsProvider
    {
        /// <summary>
        /// Gets the <see cref="ODataQueryOptions"/>
        /// </summary>
        /// <param name="context">The query option provider context.</param>
        /// <returns>The OData query options created.</returns>
        ODataQueryOptions GetQueryOptions(QueryOptionsProviderContext context);
    }
}
