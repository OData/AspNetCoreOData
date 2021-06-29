// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequest"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataBatchHttpRequestExtensions
    {
        private const string BatchMediaTypeMime = "multipart/mixed";
        private const string BatchMediaTypeJson = "application/json";
        private const string Boundary = "boundary";

        /// <summary>
        /// Determine if the request is a batch request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsODataBatchRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ContentType != null &&
                (request.ContentType.StartsWith(BatchMediaTypeMime, StringComparison.OrdinalIgnoreCase) ||
                request.ContentType.StartsWith(BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retrieves the Batch ID associated with the request.
        /// </summary>
        /// <param name="request">The Http request.</param>
        /// <returns>The Batch ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static Guid? GetODataBatchId(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ODataBatchFeature().BatchId;
        }

        /// <summary>
        /// Associates a given Batch ID with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="batchId">The Batch ID.</param>
        public static void SetODataBatchId(this HttpRequest request, Guid batchId)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            request.ODataBatchFeature().BatchId = batchId;
        }

        /// <summary>
        /// Retrieves the ChangeSet ID associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The ChangeSet ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static Guid? GetODataChangeSetId(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ODataBatchFeature().ChangeSetId;
        }

        /// <summary>
        /// Associates a given ChangeSet ID with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="changeSetId">The ChangeSet ID.</param>
        public static void SetODataChangeSetId(this HttpRequest request, Guid changeSetId)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            request.ODataBatchFeature().ChangeSetId = changeSetId;
        }

        /// <summary>
        /// Retrieves the Content-ID associated with the sub-request of a batch.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Content-ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static string GetODataContentId(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ODataBatchFeature().ContentId;
        }

        /// <summary>
        /// Associates a given Content-ID with the sub-request of a batch.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="contentId">The Content-ID.</param>
        public static void SetODataContentId(this HttpRequest request, string contentId)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            request.ODataBatchFeature().ContentId = contentId;
        }

        /// <summary>
        /// Retrieves the Content-ID to Location mapping associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Content-ID to Location mapping associated with this request, or <c>null</c> if there isn't one.</returns>
        public static IDictionary<string, string> GetODataContentIdMapping(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ODataBatchFeature().ContentIdMapping;
        }

        /// <summary>
        /// Associates a given Content-ID to Location mapping with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="contentIdMapping">The Content-ID to Location mapping.</param>
        public static void SetODataContentIdMapping(this HttpRequest request, IDictionary<string, string> contentIdMapping)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IDictionary<string, string> old = request.ODataBatchFeature().ContentIdMapping;
            old.Clear();

            if (contentIdMapping != null)
            {
                foreach (var idMap in contentIdMapping)
                {
                    old.Add(idMap);
                }
            }
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        internal static Task CreateODataBatchResponseAsync(this HttpRequest request, IEnumerable<ODataBatchResponseItem> responses, ODataMessageQuotas messageQuotas)
        {
            Contract.Assert(request != null);

            ODataVersion odataVersion = GetODataResponseVersion(request);

            IServiceProvider requestContainer = request.GetSubServiceProvider();
            ODataMessageWriterSettings writerSettings = requestContainer.GetRequiredService<ODataMessageWriterSettings>();
            writerSettings.Version = odataVersion;
            writerSettings.MessageQuotas = messageQuotas;

            HttpResponse response = request.HttpContext.Response;

            IEnumerable<MediaTypeHeaderValue> acceptHeaders = MediaTypeHeaderValue.ParseList(request.Headers.GetCommaSeparatedValues("Accept"));
            string responseContentType = null;
            foreach (MediaTypeHeaderValue acceptHeader in acceptHeaders.OrderByDescending(h => h.Quality == null ? 1 : h.Quality))
            {
                if (acceptHeader.MediaType.Equals(BatchMediaTypeMime, StringComparison.OrdinalIgnoreCase))
                {
                    responseContentType = string.Format(CultureInfo.InvariantCulture, "multipart/mixed;boundary=batchresponse_{0}", Guid.NewGuid());
                    break;
                }
                else if (acceptHeader.MediaType.Equals(BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase))
                {
                    responseContentType = BatchMediaTypeJson;
                    break;
                }
            }
            if (responseContentType == null)
            {
                // In absence of accept, if request was JSON then default response to be JSON.
                // Note that, if responseContentType is not set, then it will default to multipart/mixed
                // when constructing the BatchContent, so we don't need to handle that case here
                if (!string.IsNullOrEmpty(request.ContentType)
                && request.ContentType.IndexOf(BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    responseContentType = BatchMediaTypeJson;
                }
            }

            response.StatusCode = StatusCodes.Status200OK;
            ODataBatchContent batchContent = new ODataBatchContent(responses, requestContainer, responseContentType);
            foreach (var header in batchContent.Headers)
            {
                // Copy headers from batch content, overwriting any existing headers.
                response.Headers[header.Key] = header.Value;
            }

            return batchContent.SerializeToStreamAsync(response.Body);
        }

        internal static async Task<bool> ValidateODataBatchRequest(this HttpRequest request)
        {
            Contract.Assert(request != null);

            HttpResponse response = request.HttpContext.Response;

            if (request.Body == null)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await response.WriteAsync(SRResources.BatchRequestMissingBody).ConfigureAwait(false);
                return false;
            }

            RequestHeaders headers = request.GetTypedHeaders();
            MediaTypeHeaderValue contentType = headers.ContentType;
            if (contentType == null)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await response.WriteAsync(SRResources.BatchRequestMissingContentType).ConfigureAwait(false);
                return false;
            }

            string mediaType = contentType.MediaType.ToString();
            bool isMimeBatch = string.Equals(mediaType, BatchMediaTypeMime, StringComparison.OrdinalIgnoreCase);
            bool isJsonBatch = string.Equals(mediaType, BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase);

            if (!isMimeBatch && !isJsonBatch)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await response.WriteAsync(Error.Format(SRResources.BatchRequestInvalidMediaType,
                    BatchMediaTypeMime, BatchMediaTypeJson)).ConfigureAwait(false);
                return false;
            }

            if (isMimeBatch)
            {
                NameValueHeaderValue boundary = contentType.Parameters.FirstOrDefault(p => String.Equals(p.Name.ToString(), Boundary, StringComparison.OrdinalIgnoreCase));
                if (boundary == null || string.IsNullOrEmpty(boundary.Value.ToString()))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await response.WriteAsync(SRResources.BatchRequestMissingBoundary).ConfigureAwait(false);
                    return false;
                }
            }

            return true;
        }

        internal static Uri GetODataBatchBaseUri(this HttpRequest request, string oDataPrefixName)
        {
            Contract.Assert(request != null);

            if (oDataPrefixName == null)
            {
                // Return request's base address.
                return new Uri(request.GetDisplayUrl());
            }

            // Maybe we can just do:
            // string requestUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
            // use requestUri to remove the "$batch"?

            request.ODataFeature().PrefixName = oDataPrefixName;

            RouteValueDictionary batchRouteData = request.ODataFeature().BatchRouteData;
            if (batchRouteData != null && batchRouteData.Any())
            {
                foreach (var data in batchRouteData)
                {
                    request.RouteValues.Add(data.Key, data.Value);
                }
            }

            return new Uri(request.CreateODataLink());
        }

        internal static ODataVersion GetODataResponseVersion(HttpRequest request)
        {
            // OData protocol requires that you send the minimum version that the client needs to know to
            // understand the response. There is no easy way we can figure out the minimum version that the client
            // needs to understand our response. We send response headers much ahead generating the response. So if
            // the requestMessage has a OData-MaxVersion, tell the client that our response is of the same
            // version; else use the DataServiceVersionHeader. Our response might require a higher version of the
            // client and it might fail. If the client doesn't send these headers respond with the default version
            // (V4).
            return request.ODataMaxServiceVersion() ??
                request.ODataServiceVersion() ??
                ODataVersionConstraint.DefaultODataVersion;
        }
    }
}
