// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Extension method to return the <see cref="IODataFeature"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <returns>The <see cref="IODataFeature"/>.</returns>
        public static IODataFeature ODataFeature(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull(nameof(httpContext));
            }

            IODataFeature odataFeature = httpContext.Features.Get<IODataFeature>();
            if (odataFeature == null)
            {
                odataFeature = new ODataFeature();
                httpContext.Features.Set(odataFeature);
            }

            return odataFeature;
        }

        /// <summary>
        /// Extension method to return the <see cref="IODataBatchFeature"/> from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <returns>The <see cref="IODataBatchFeature"/>.</returns>
        public static IODataBatchFeature ODataBatchFeature(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull(nameof(httpContext));
            }

            IODataBatchFeature odataBatchFeature = httpContext.Features.Get<IODataBatchFeature>();
            if (odataBatchFeature == null)
            {
                odataBatchFeature = new ODataBatchFeature();
                httpContext.Features.Set(odataBatchFeature);
            }

            return odataBatchFeature;
        }

        /// <summary>
        /// Returns the <see cref="ODataOptions"/> instance from the DI container.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> instance to extend.</param>
        /// <returns>The <see cref="ODataOptions"/> instance from the DI container.</returns>
        public static ODataOptions ODataOptions(this HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            return context.RequestServices.GetRequiredService<IOptions<ODataOptions>>().Value;
        }


    }
}
