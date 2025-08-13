//-----------------------------------------------------------------------------
// <copyright file="ODataApplicationBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using System;

namespace Microsoft.AspNetCore.OData;

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
    /// Use OData batching minimal API middleware.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
    /// <param name="routePattern">The route pattern,</param>
    /// <param name="model">The edm model.</param>
    /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
    public static IApplicationBuilder UseODataMiniBatching(this IApplicationBuilder app, string routePattern, IEdmModel model)
     => app.UseODataMiniBatching(routePattern, model, new DefaultODataBatchHandler());

    /// <summary>
    /// Use OData batching minimal API middleware.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
    /// <param name="routePattern">The route pattern,</param>
    /// <param name="model">The edm model.</param>
    /// <param name="handler">The batch handler.</param>
    /// <param name="configAction">The services configuration.</param>
    /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
    public static IApplicationBuilder UseODataMiniBatching(this IApplicationBuilder app, string routePattern, IEdmModel model,
        ODataBatchHandler handler, Action<IServiceCollection> configAction = null)
    {
        if (app == null)
        {
            throw Error.ArgumentNull(nameof(app));
        }

        ODataMiniMetadata metadata = new ODataMiniMetadata();

        // retrieve the global minimal API OData configuration
        ODataMiniOptions options = app.ApplicationServices.GetService<IOptions<ODataMiniOptions>>()?.Value;
        if (options is not null)
        {
            metadata.Options.UpdateFrom(options);
        }

        metadata.Model = model;
        if (configAction != null)
        {
            metadata.Services = configAction;
        }

        app.UseMiddleware<ODataMiniBatchMiddleware>(routePattern, handler, metadata);

        // This is required to enable the OData batch request.
        // Otherwise, the sub requests will not be routed correctly.
        return app.UseRouting();
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
}
