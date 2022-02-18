//-----------------------------------------------------------------------------
// <copyright file="ODataQueryRequestMiddleware.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Defines the middleware for handling OData $query requests.
    /// This middleware essentially transforms the incoming request (Post) to a "Get" request.
    /// Be noted: should put this middle ware before "UseRouting()".
    /// </summary>
    public class ODataQueryRequestMiddleware
    {
        private IEnumerable<IODataQueryRequestParser> _parsers;
        private readonly RequestDelegate _next;

        /// <summary>
        /// Instantiates a new instance of <see cref="ODataQueryRequestMiddleware"/>.
        /// </summary>
        /// <param name="queryRequestParsers">The query request parsers.</param>
        /// <param name="next">The next middleware.</param>
        public ODataQueryRequestMiddleware(IEnumerable<IODataQueryRequestParser> queryRequestParsers, RequestDelegate next)
        {
            _parsers = queryRequestParsers;
            _next = next;
        }

        /// <summary>
        /// Invoke the OData $query middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // shall we verify it?
            //if (context.GetEndpoint() != null)
            //{
            //    throw new ODataException("Should put this middleware before UseRouting");
            //}

            HttpRequest request = context.Request;
            if (request.IsODataQueryRequest())
            {
                foreach (IODataQueryRequestParser parser in _parsers)
                {
                    if (parser.CanParse(request))
                    {
                        await TransformQueryRequestAsync(parser, request).ConfigureAwait(false);
                        break;
                    }
                }
            }

            await _next(context).ConfigureAwait(false);
        }

        /// <summary>
        /// Transforms a POST request targeted at a resource path ending in $query into a GET request.
        /// The query options are parsed from the request body and appended to the request URL.
        /// </summary>
        /// <param name="parser">The query request parser.</param>
        /// <param name="request">The Http request.</param>
        internal static async Task TransformQueryRequestAsync(IODataQueryRequestParser parser, HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            // Parse query options in request body
            string queryOptions = await parser.ParseAsync(request).ConfigureAwait(false);

            string requestPath = request.Path.Value.TrimEnd('/');
            string queryString = request.QueryString.Value;

            // Strip off the /$query part
            requestPath = requestPath.Substring(0, requestPath.LastIndexOf("/$query", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(queryOptions))
            {
                if (string.IsNullOrWhiteSpace(queryString))
                {
                    queryString = '?' + queryOptions;
                }
                else
                {
                    queryString += '&' + queryOptions;
                }
            }

            request.Path = new PathString(requestPath);
            request.QueryString = new QueryString(queryString);
            request.Method = HttpMethods.Get;
        }
    }
}
