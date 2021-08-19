//-----------------------------------------------------------------------------
// <copyright file="IODataQueryRequestParser.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Exposes the ability to read and parse the content of a <see cref="HttpRequest" />
    /// into a query options part of an OData URL. Query options may be passed
    /// in the request body to a resource path ending in /$query.
    /// </summary>
    public interface IODataQueryRequestParser
    {
        /// <summary>
        /// Determines whether this <see cref="IODataQueryRequestParser"/> can parse the http request.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>true if this <see cref="IODataQueryRequestParser"/> can parse the http request; false otherwise.</returns>
        bool CanParse(HttpRequest request);

        /// <summary>
        /// Reads and parses the content of a <see cref="HttpRequest"/>
        /// into a query options part of an OData URL.
        /// </summary>
        /// <param name="request">A http request containing the query options.</param>
        /// <returns>A string representing the query options part of an OData URL.</returns>
        Task<string> ParseAsync(HttpRequest request);
    }
}
