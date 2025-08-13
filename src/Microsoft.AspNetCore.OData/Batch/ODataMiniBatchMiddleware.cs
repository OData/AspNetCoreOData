//-----------------------------------------------------------------------------
// <copyright file="ODataMiniBatchMiddleware.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNetCore.OData.Batch;

/// <summary>
/// Defines the middleware for handling OData $batch requests in the minimal API scenarios.
/// This middleware essentially acts like branching middleware and redirects OData $batch
/// requests to the appropriate ODataBatchHandler.
/// </summary>
public class ODataMiniBatchMiddleware
{
    internal const string MinimalApiMetadataKey = "MS_MinimalApiMetadataKey_386F76A4-5E7C-4B4D-8A20-261A23C3DD9A";

    private readonly RequestDelegate _next;
    private readonly TemplateMatcher _routeMatcher;
    private readonly ODataBatchHandler _handler;
    private readonly ODataMiniMetadata _metadata;

    /// <summary>
    /// Instantiates a new instance of <see cref="ODataBatchMiddleware"/>.
    /// </summary>
    /// <param name="routePattern">The route pattern for the OData $batch endpoint.</param>
    /// <param name="handler">The handler for processing batch requests.</param>
    /// <param name="metadata">the metadata for the OData $batch endpoint.</param>
    /// <param name="next">The next middleware.</param>
    public ODataMiniBatchMiddleware(string routePattern, ODataBatchHandler handler, ODataMiniMetadata metadata, RequestDelegate next)
    {
        if (routePattern == null)
        {
            throw Error.ArgumentNull(nameof(routePattern));
        }

        // ensure _routePattern starts with /
        string newRouteTemplate = routePattern.StartsWith("/", StringComparison.Ordinal) ? routePattern.Substring(1) : routePattern;
        RouteTemplate parsedTemplate = TemplateParser.Parse(newRouteTemplate);
        _routeMatcher = new TemplateMatcher(parsedTemplate, new RouteValueDictionary());
        _handler = handler ?? throw Error.ArgumentNull(nameof(handler));
        _metadata = metadata ?? throw Error.ArgumentNull(nameof(metadata));

        _next = next;
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
            throw Error.ArgumentNull(nameof(context));
        }

        // The batch middleware should not handle the options requests for cors to properly function.
        bool isPostRequest = HttpMethods.IsPost(context.Request.Method);
        if (!isPostRequest)
        {
            await _next(context);
        }
        else
        {
            string path = context.Request.Path;
            RouteValueDictionary routeData = new RouteValueDictionary();
            if (_routeMatcher.TryMatch(path, routeData))
            {
                if (routeData.Count > 0)
                {
                    Merge(context.ODataFeature().BatchRouteData, routeData);
                }

                // Add the ODataMiniMetadata to the context items for later retrieval.
                context.Items.Add(MinimalApiMetadataKey, _metadata);

                await _handler.ProcessBatchAsync(context, _next);
            }
            else
            {
                await _next(context);
            }
        }
    }

    private static void Merge(RouteValueDictionary batchRouteData, RouteValueDictionary routeData)
    {
        foreach (var item in routeData)
        {
            batchRouteData.Add(item.Key, item.Value);
        }
    }
}
