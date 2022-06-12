//-----------------------------------------------------------------------------
// <copyright file="MultipleODataQueryOptionsBindingExtension.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Query.Extension
{

    /// <summary>
    /// This class allows to use multiple <see cref="IODataQueryOptionsBindingExtension"/> extenion interfaces.
    /// </summary>
    public class MultipleODataQueryOptionsBindingExtension : IODataQueryOptionsBindingExtension
    {

        private readonly IList<IODataQueryOptionsBindingExtension> _extensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleODataQueryOptionsBindingExtension"/> class
        /// with a list of extension interfaces.
        /// </summary>
        /// <param name="extensions">A list of query extensions to apply.</param>
        public MultipleODataQueryOptionsBindingExtension(IList<IODataQueryOptionsBindingExtension> extensions)
        {
            _extensions = extensions;
        }

        /// <summary>
        /// Apply multiple custom queries to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="queryOptions">The <see cref="ODataQueryOptions"/> object that is executing this method.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQueryOptions queryOptions, ODataQuerySettings querySettings)
        {
            if (query == null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            if (queryOptions == null)
            {
                throw Error.ArgumentNull(nameof(queryOptions));
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull(nameof(querySettings));
            }

            IQueryable result = query;
            foreach (var extension in _extensions)
            {
                result = extension.ApplyTo(query, queryOptions, querySettings);
            }
            return result;
        }
    }
}
