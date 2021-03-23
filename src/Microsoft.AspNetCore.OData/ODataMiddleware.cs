// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Defines a middleware for dealing with general per-request
    /// init and cleanup functionality related to OData queries.
    /// </summary>
    public class ODataMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Instantiates a new instance of <see cref="ODataMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        public ODataMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke the OData middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            var prefix = context.Request.ODataFeature().PrefixName;
            if (prefix != null)
            {
                context.Request.CreateSubServiceProvider(context.Request.ODataFeature().PrefixName);
                await _next(context).ConfigureAwait(false);
                context.Request.DeleteSubRequestProvider(true);
            }
            else
            {
                await _next(context).ConfigureAwait(false);
            }
        }
    }
}
