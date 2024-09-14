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
