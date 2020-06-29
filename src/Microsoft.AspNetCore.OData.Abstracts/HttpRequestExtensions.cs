// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequest"/>.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the <see cref="IODataFeature"/> from the services container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataFeature"/> from the services container.</returns>
        public static IODataFeature ODataFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.HttpContext.ODataFeature();
        }

        /// <summary>
        /// Gets the <see cref="IEdmModel"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IEdmModel"/> from the request container.</returns>
        public static IEdmModel GetModel(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ODataFeature().Model;
        }
    }
}
