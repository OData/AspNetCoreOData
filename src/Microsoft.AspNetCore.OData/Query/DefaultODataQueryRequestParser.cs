//-----------------------------------------------------------------------------
// <copyright file="DefaultODataQueryRequestParser.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Exposes the ability to read and parse the content of a <see cref="HttpRequest" />
    /// into a query options part of an OData URL. Query options may be passed
    /// in the request body to a resource path ending in /$query.
    /// </summary>
    public class DefaultODataQueryRequestParser : IODataQueryRequestParser
    {
        private static MediaTypeHeaderValue SupportedMediaType = MediaTypeHeaderValue.Parse("text/plain");

        /// <inheritdoc/>
        public bool CanParse(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.ContentType?.StartsWith(SupportedMediaType.MediaType, StringComparison.Ordinal) == true ? true : false;
        }

        /// <inheritdoc/>
        public async Task<string> ParseAsync(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            try
            {
                Stream requestStream = request.Body;

                using (var reader = new StreamReader(
                    requestStream,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true))
                {
                    // Based on OData OASIS Standard, the request body is expected to contain the query portion of the URL
                    // and MUST use the same percent-encoding as in URLs (especially: no spaces, tabs, or line breaks allowed)
                    // and MUST follow the expected syntax rules
                    return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                throw new ODataException(SRResources.CannotParseQueryRequestPayload);
            }
        }
    }
}
