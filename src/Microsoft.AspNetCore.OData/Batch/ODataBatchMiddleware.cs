// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly IPerRouteContainer _perRouteContainer;
        private readonly ODataBatchPathMapping _batchMapping;

        /// <summary>
        /// Instantiates a new instance of <see cref="ODataBatchMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        /// <param name="perRouteContainer">The next middleware.</param>
        /// <param name="batchMapping">The next middleware.</param>
        public ODataBatchMiddleware(RequestDelegate next,
            IPerRouteContainer perRouteContainer,
            ODataBatchPathMapping batchMapping)
        {
            _next = next;
            _perRouteContainer = perRouteContainer;
            _batchMapping = batchMapping;
            Initialize();
        }

        /// <summary>
        /// Invoke the OData $Batch middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Attempt to match the path to a bach route.
            ODataBatchPathMapping batchMapping = context.RequestServices.GetRequiredService<ODataBatchPathMapping>();

            string routeName;
            if (batchMapping.TryGetRouteName(context, out routeName))
            {
                // Get the per-route continer and retrieve the batch handler.
                //IPerRouteContainer perRouteContainer = context.RequestServices.GetRequiredService<IPerRouteContainer>();
                //if (perRouteContainer == null)
                //{
                //    throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
                //}

                //IServiceProvider rootContainer = perRouteContainer.GetODataRootContainer(routeName);
                // IServiceProvider serviceProvider = context.RequestServices;

                IServiceProvider serviceProvider = _perRouteContainer.GetServiceProvider(routeName);
                ODataBatchHandler batchHandler = serviceProvider.GetRequiredService<ODataBatchHandler>();

                await batchHandler.ProcessBatchAsync(context, _next).ConfigureAwait(false);
            }
            else
            {
                await _next(context);
            }
        }

        private void Initialize()
        {
            foreach (var service in _perRouteContainer.Services)
            {
                // If a batch handler is present, register the route with the batch path mapper. This will be used
                // by the batching middleware to handle the batch request. Batching still requires the injection
                // of the batching middleware via UseODataBatching().
                ODataBatchHandler batchHandler = service.Value.GetService<ODataBatchHandler>();

                if (batchHandler != null)
                {
                    batchHandler.RouteName = service.Key;
                    string batchPath = String.IsNullOrEmpty(service.Key)  ? "/$batch" : $"/{service.Key}/$batch";
                    _batchMapping.AddRoute(service.Key, batchPath);
                }
            }
        }
    }
}
