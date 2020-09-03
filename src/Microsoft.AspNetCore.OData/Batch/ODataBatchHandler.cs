// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Defines the abstraction for handling OData batch requests.
    /// </summary>
    public abstract class ODataBatchHandler
    {
        // Maxing out the received message size as we depend on the hosting layer to enforce this limit.
        private readonly ODataMessageQuotas _messageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue };

        // Preference odata.continue-on-error.
        internal const string PreferenceContinueOnError = "continue-on-error";
        internal const string PreferenceContinueOnErrorFalse = "continue-on-error=false";

        /// <summary>
        /// Gets the <see cref="ODataMessageQuotas"/> used for reading/writing the batch request/response.
        /// </summary>
        public ODataMessageQuotas MessageQuotas
        {
            get { return _messageQuotas; }
        }

        /// <summary>
        /// Gets or sets the OData route associated with this batch handler.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Abstract method for processing a batch request.
        /// </summary>
        /// <param name="context">The http content.</param>
        /// ><param name="nextHandler">The next handler in the middleware chain.</param>
        /// <returns></returns>
        public abstract Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler);

        /// <summary>
        /// Creates the batch response message.
        /// </summary>
        /// <param name="responses">The responses for the batch requests.</param>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <returns>The batch response message.</returns>
        public virtual Task CreateResponseMessageAsync(IEnumerable<ODataBatchResponseItem> responses, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.CreateODataBatchResponseAsync(responses, MessageQuotas);
        }

        /// <summary>
        /// Validates the incoming request that contains the batch request messages.
        /// </summary>
        /// <param name="request">The request containing the batch request messages.</param>
        /// <returns>true if the request is valid, otherwise false.</returns>
        public virtual Task<bool> ValidateRequest(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ValidateODataBatchRequest();
        }

        /// <summary>
        /// Gets the base URI for the batched requests.
        /// </summary>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <returns>The base URI.</returns>
        public virtual Uri GetBaseUri(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.GetODataBatchBaseUri(null, null/*ODataRouteName, ODataRoute*/);
        }

        /// <summary>
        /// Gets or sets if the continue-on-error header is enable or not.
        /// </summary>
        internal bool ContinueOnError { get; private set; }

        /// <summary>
        /// Set ContinueOnError based on the request and headers.
        /// </summary>
        /// <param name="header">The request header.</param>
        /// <param name="enableContinueOnErrorHeader">Flag indicating if continue on error header is enabled.</param>
        internal void SetContinueOnError(IHeaderDictionary header, bool enableContinueOnErrorHeader)
        {
            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(header);
            if ((preferHeader != null &&
                preferHeader.Contains(PreferenceContinueOnError, StringComparison.OrdinalIgnoreCase) &&
                !preferHeader.Contains(PreferenceContinueOnErrorFalse, StringComparison.OrdinalIgnoreCase))
                || (!enableContinueOnErrorHeader))
            {
                ContinueOnError = true;
            }
            else
            {
                ContinueOnError = false;
            }
        }
    }
}
