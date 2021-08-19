//-----------------------------------------------------------------------------
// <copyright file="HttpRequestExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Creates a message reader that the user manages.")]
        public static ODataMessageReader GetODataMessageReader(this HttpRequest request, IServiceProvider requestContainer)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // how to dispose it?
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
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

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
    }
}
