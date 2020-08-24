// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequest"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the <see cref="ODataMessageReader"/> for the <see cref="HttpRequest"/> stream.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <returns>A task object that produces an <see cref="ODataMessageReader"/> when completed.</returns>
        public static ODataMessageReader GetODataMessageReader(this HttpRequest request, IServiceProvider requestContainer)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(request.Body, request.Headers, requestContainer);
            ODataMessageReaderSettings settings = requestContainer.GetRequiredService<ODataMessageReaderSettings>();
            ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, settings);
            return oDataMessageReader;
        }

        /// <summary>
        /// Copy an absolute Uri to a <see cref="HttpRequest"/> stream.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="uri">The absolute uri to copy.</param>
        public static void CopyAbsoluteUrl(this HttpRequest request, Uri uri)
        {
            request.Scheme = uri.Scheme;
            request.Host = uri.IsDefaultPort ?
                new HostString(uri.Host) :
                new HostString(uri.Host, uri.Port);
            request.QueryString = new QueryString(uri.Query);
            var path = new PathString(uri.AbsolutePath);
            if (path.StartsWithSegments(request.PathBase, out PathString remainingPath))
            {
                path = remainingPath;
            }
            request.Path = path;
        }

        /// <summary>
        /// Copies the properties from another <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="subRequest">The sub-request.</param>
        /// <param name="batchRequest">The batch request that contains the properties to copy.</param>
        /// <remarks>
        /// Currently, this method is unused but is retained to keep a similar API surface area
        /// between the AspNet and AspNetCore versions of OData WebApi.
        /// </remarks>
        public static void CopyBatchRequestProperties(this HttpRequest subRequest, HttpRequest batchRequest)
        {
            if (subRequest == null)
            {
                throw new ArgumentNullException(nameof(subRequest));
            }

            if (batchRequest == null)
            {
                throw new ArgumentNullException(nameof(batchRequest));
            }
        }
    }
}