//-----------------------------------------------------------------------------
// <copyright file="IODataQueryOptionsBindingExtension.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;

namespace Microsoft.AspNetCore.OData.Query.Extension
{

    /// <summary>
    /// This interface allows to extend the <see cref="ODataQueryOptions.ApplyTo(object, ODataQuerySettings)"/> method
    /// and apply custom query features.
    /// </summary>
    public interface IODataQueryOptionsBindingExtension
    {

        /// <summary>
        /// Apply a custom query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="queryOptions">The <see cref="ODataQueryOptions"/> object that is executing this method.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQueryOptions queryOptions, ODataQuerySettings querySettings);

    }
}
