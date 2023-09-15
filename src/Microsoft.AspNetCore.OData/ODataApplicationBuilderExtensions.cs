//-----------------------------------------------------------------------------
// <copyright file="ODataApplicationBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides extension methods for <see cref="IApplicationBuilder"/> to add OData routes.
    /// </summary>
    public static class ODataApplicationBuilderExtensions
    {
        private const string DefaultODataRouteDebugMiddlewareRoutePattern = "$odata";

        /// <summary>
        /// Use OData batching middleware.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseODataBatching(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw Error.ArgumentNull(nameof(app));
            }

            return app.UseMiddleware<ODataBatchMiddleware>();
        }

        /// <summary>
        /// Use OData query request middleware. An OData query request is a Http Post request ending with /$query.
        /// The Request body contains the query options.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseODataQueryRequest(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw Error.ArgumentNull(nameof(app));
            }

            return app.UseMiddleware<ODataQueryRequestMiddleware>();
        }

        /// <summary>
        /// Use OData route debug middleware. You can send request "~/$odata" after enabling this middleware.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseODataRouteDebug(this IApplicationBuilder app)
        {
            return app.UseODataRouteDebug(DefaultODataRouteDebugMiddlewareRoutePattern);
        }

        /// <summary>
        /// Use OData route debug middleware using the given route pattern.
        /// For example, if the given route pattern is "myrouteinfo", then you can send request "~/myrouteinfo" after enabling this middleware.
        /// Please use basic (literal) route pattern.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <param name="routePattern">The given route pattern.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseODataRouteDebug(this IApplicationBuilder app, string routePattern)
        {
            if (app == null)
            {
                throw Error.ArgumentNull(nameof(app));
            }

            if (routePattern == null)
            {
                throw Error.ArgumentNull(nameof(routePattern));
            }

            return app.UseMiddleware<ODataRouteDebugMiddleware>(routePattern);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="app">TODO</param>
        /// <returns>TODO</returns>
        public static IApplicationBuilder UseMinimalOData(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw Error.ArgumentNull(nameof(app));
            }

            return app.UseMiddleware<ODataMinimalApiMiddleware>();
        }
    }

    internal class ODataMinimalApiMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataMinimalApiMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        public ODataMinimalApiMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next)); ;
        }

        /// <summary>
        /// Invoke the OData minimal middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var dataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
            var odataOptions = context.RequestServices.GetRequiredService<IOptions<ODataOptions>>().Value;
            var odataPathTemplateParser = context.RequestServices.GetRequiredService<IODataPathTemplateParser>();

            var routePrefixes = odataOptions.RouteComponents.Keys;

            foreach (RouteEndpoint endpoint in dataSource.Endpoints)
            {
                // TODO: Does endpoint.RoutePattern contain the route prefix when MapGroup (.NET 7) is used?
                var routeTemplate = endpoint.RoutePattern.RawText;
                var routePrefix = ODataRoutingHelpers.FindRoutePrefix(routeTemplate, routePrefixes, out string sanitizedRouteTemplate);

                var model = odataOptions.RouteComponents[routePrefix].EdmModel;
                var serviceProvider = odataOptions.RouteComponents[routePrefix].ServiceProvider;

                var odataPathTemplate = odataPathTemplateParser.Parse(model, sanitizedRouteTemplate, serviceProvider);

                // endpoint.Metadata.Add(new ODataRoutingMetadata(routePrefix, model, odataPathTemplate));
                endpoint.Metadata.Append(new ODataRoutingMetadata(routePrefix, model, odataPathTemplate));
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}
