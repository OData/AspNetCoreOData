// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// Defines the middleware for handling OData $batch requests.
    /// This middleware essentially acts like branching middleware and redirects OData $batch
    /// requests to the appropriate ODataBatchHandler.
    /// </summary>
    public class ODataBatchMiddleware
    {
        private readonly RequestDelegate _next;
        private ODataBatchPathMapping _batchMapping;

        /// <summary>
        /// Instantiates a new instance of <see cref="ODataBatchMiddleware"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider, we don't inject the ODataOptions.</param>
        /// <param name="next">The next middleware.</param>
        public ODataBatchMiddleware(IServiceProvider serviceProvider, RequestDelegate next)
        {
            _next = next;

            // We inject the service provider to let the middle ware pass without ODataOptions injected.
            IOptions<ODataOptions> odataOptionsOptions = serviceProvider?.GetService<IOptions<ODataOptions>>();
            if (odataOptionsOptions != null && odataOptionsOptions.Value != null)
            {
                Initialize(odataOptionsOptions.Value);
            }
        }

        /// <summary>
        /// Gets the batch path mapping, for unit test only
        /// </summary>
        internal ODataBatchPathMapping BatchMapping => _batchMapping;

        /// <summary>
        /// Invoke the OData $Batch middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }
            string prefixName;
            ODataBatchHandler batchHandler;

            // The batch middleware should not handle the options requests for cors to properly function.
            bool isPreFlight = HttpMethods.IsOptions(context.Request.Method);

            if (!isPreFlight
                && _batchMapping != null
                && _batchMapping.TryGetPrefixName(context, out prefixName, out batchHandler))
            {
                Contract.Assert(batchHandler != null);
                await batchHandler.ProcessBatchAsync(context, _next).ConfigureAwait(false);
            }
            else
            {
                await _next(context).ConfigureAwait(false);
            }
        }

        private void Initialize(ODataOptions options)
        {
            Contract.Assert(options != null);

            foreach (var model in options.Models)
            {
                IServiceProvider subServiceProvider = model.Value.Item2;
                if (subServiceProvider == null)
                {
                    continue;
                }

                // If a batch handler is present, register the route with the batch path mapper. This will be used
                // by the batching middleware to handle the batch request. Batching still requires the injection
                // of the batching middleware via UseODataBatching().
                ODataBatchHandler batchHandler = subServiceProvider.GetService<ODataBatchHandler>();
                if (batchHandler != null)
                {
                    batchHandler.PrefixName = model.Key;
                    string batchPath = string.IsNullOrEmpty(model.Key)  ? "/$batch" : $"/{model.Key}/$batch";

                    if (_batchMapping == null)
                    {
                        _batchMapping = new ODataBatchPathMapping();
                    }

                    _batchMapping.AddRoute(model.Key, batchPath, batchHandler);
                }
            }
        }
    }
}
