// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Batch;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides extension methods for <see cref="IApplicationBuilder"/> to add OData routes.
    /// </summary>
    public static class ODataApplicationBuilderExtensions
    {
        /// <summary>
        /// Use OData middleware.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <param name="batching">Value which determines whether OData batching is enabled.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseOData(this IApplicationBuilder app, bool batching = false)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseMiddleware<ODataMiddleware>();

            if (batching)
            {
                app.UseMiddleware<ODataBatchMiddleware>();
            }

            return app;
        }
    }
}
