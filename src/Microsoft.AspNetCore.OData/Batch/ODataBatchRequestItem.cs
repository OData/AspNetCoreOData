//-----------------------------------------------------------------------------
// <copyright file="ODataBatchRequestItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Represents an OData batch request.
    /// </summary>
    public abstract class ODataBatchRequestItem
    {
        /// <summary>
        /// Routes a single OData batch request.
        /// </summary>
        /// <param name="handler">The handler for processing a message.</param>
        /// <param name="context">The http context.</param>
        /// <param name="contentIdToLocationMapping">The Content-ID to Location mapping.</param>
        /// <returns></returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static async Task SendRequestAsync(RequestDelegate handler, HttpContext context, IDictionary<string, string> contentIdToLocationMapping)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Features.Set<IEndpointFeature>(new ODataEndpointFeature());

            if (contentIdToLocationMapping != null)
            {
                string encodedUrl = context.Request.GetEncodedUrl();
                string resolvedRequestUrl = ContentIdHelpers.ResolveContentId(encodedUrl, contentIdToLocationMapping);
                Uri resolvedUri;
                if (!string.IsNullOrEmpty(resolvedRequestUrl)
                    && Uri.TryCreate(resolvedRequestUrl, UriKind.Absolute, out resolvedUri)
                    && IsSameAuthority(resolvedUri, context.Request))
                {
                    // Content-ID resolution can replace the original URL prefix with a mapped
                    // Location. URI normalization can then change the path, so validate the final
                    // resolved path against the OData service root before dispatching the sub-request.
                    string routePrefix = context.ODataFeature()?.RoutePrefix;
                    PathString absolutePath = resolvedUri.AbsolutePath;
                    if (!absolutePath.IsPathWithinServiceRoot(routePrefix, context.Request.PathBase))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    context.Request.CopyAbsoluteUrl(resolvedUri);
                }

                context.Request.SetODataContentIdMapping(contentIdToLocationMapping);
            }

            try
            {
                await handler(context).ConfigureAwait(false);

                string contentId = context.Request.GetODataContentId();

                if (contentIdToLocationMapping != null && contentId != null)
                {
                    AddLocationHeaderToMapping(context.Response, contentIdToLocationMapping, contentId);
                }
            }
            catch (Exception)
            {
                // Unlike AspNet, the exception handling is (by default) upstream of this middleware
                // so we need to trap exceptions on our own. This code is similar to the
                // ExceptionHandlerMiddleware class in AspNetCore.
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        /// <summary>
        /// Routes the request.
        /// </summary>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>A <see cref="ODataBatchResponseItem"/>.</returns>
        public abstract Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler);

        /// <summary>
        /// Default dictionary that ODataBatchRequestItems should use when mapping ID references to URLs
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Collection needs to be provided externally.")]
        public IDictionary<string, string> ContentIdToLocationMapping { get; set; }

        private static void AddLocationHeaderToMapping(HttpResponse response, IDictionary<string, string> contentIdToLocationMapping, string contentId)
        {
            Contract.Assert(response != null);
            Contract.Assert(response.Headers != null);
            Contract.Assert(contentIdToLocationMapping != null);
            Contract.Assert(contentId != null);

            var headers = response.GetTypedHeaders();
            if (headers.Location != null)
            {
                contentIdToLocationMapping.Add(contentId, headers.Location.AbsoluteUri);
            }
        }

        // Returns true when the resolved Content-ID URI shares the same scheme, host, and port as
        // the current sub-request context.  Content-ID references are resolved by string-
        // concatenating a Location header value with the remainder of the relative reference; the
        // Location header may point to a different origin (redirect, multi-tenant alias, etc.) so
        // the authority must be re-validated after resolution before the URL is applied.
        private static bool IsSameAuthority(Uri resolvedUri, HttpRequest request)
        {
            Uri requestBaseUri = new Uri($"{request.Scheme}://{request.Host}/");
            return Uri.Compare(resolvedUri, requestBaseUri, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }

        // Endpoint routing stores the selected endpoint in IEndpointFeature. Batch sub-requests use
        // this feature to prevent a root-mounted or broadly prefixed OData service from dispatching
        // to an unrelated application endpoint that happens to share the same path prefix.
        private sealed class ODataEndpointFeature : IEndpointFeature
        {
            // EndpointMiddleware executes this endpoint instead of a non-OData endpoint selected by
            // routing, ensuring the unrelated application handler is never invoked.
            private static readonly Endpoint RejectedEndpoint = new Endpoint(RejectedEndpointHandler, EndpointMetadataCollection.Empty, "OData batch endpoint validation failure");
            private Endpoint _endpoint;

            public Endpoint Endpoint
            {
                get => _endpoint;
                set
                {
                    // A null value means routing has not selected an endpoint. OData controller
                    // endpoints carry IODataRoutingMetadata. Any other selected endpoint is outside the OData service.
                    bool isODataEndpoint = value?.Metadata.GetMetadata<IODataRoutingMetadata>() != null;
                    _endpoint = value == null || isODataEndpoint ? value : RejectedEndpoint;
                }
            }

            // Batch responses need only the failure status. An empty body works for both JSON and
            // multipart batch responses and avoids introducing a response payload contract.
            private static RequestDelegate RejectedEndpointHandler => context =>
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Task.CompletedTask;
            };
        }
    }
}
