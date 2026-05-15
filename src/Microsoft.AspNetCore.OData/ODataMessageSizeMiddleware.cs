//-----------------------------------------------------------------------------
// <copyright file="ODataMessageSizeMiddleware.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// Middleware that enforces MaxReceivedMessageSize for incoming POST/PUT/PATCH requests
/// by configuring Kestrel's <see cref="IHttpMaxRequestBodySizeFeature"/>.
/// For hosts without that feature, throws <see cref="ODataException"/> when Content-Length exceeds the limit.
/// This works for both Content-Length and chunked transfer requests.
/// Registered automatically by AddOData via IStartupFilter.
/// </summary>
internal class ODataMessageSizeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly long _maxReceivedMessageSize;

    public ODataMessageSizeMiddleware(RequestDelegate next, IOptions<ODataMiniOptions> options)
    {
        _next = next;
        _maxReceivedMessageSize = options.Value.MaxReceivedMessageSize;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsPost(context.Request.Method) ||
            HttpMethods.IsPut(context.Request.Method) ||
            HttpMethods.IsPatch(context.Request.Method))
        {
            // Skip batch requests — they have their own enforcement via ODataBatchHandler.MessageQuotas.
            if (context.Request.Path.Value?.EndsWith("/$batch", StringComparison.OrdinalIgnoreCase) != true)
            {
                // Prefer Kestrel's transport-level enforcement (handles both Content-Length and chunked).
                IHttpMaxRequestBodySizeFeature maxBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (maxBodySizeFeature != null && !maxBodySizeFeature.IsReadOnly)
                {
                    maxBodySizeFeature.MaxRequestBodySize = _maxReceivedMessageSize;
                }
                else if (context.Request.ContentLength > _maxReceivedMessageSize)
                {
                    // Throw with the same message pattern as OData.NET's MessageStreamWrapper
                    // for consistent error behavior across batch and non-batch requests.
                    throw new ODataException(
                        Error.Format(SRResources.MaxReceivedMessageSizeExceeded,
                            context.Request.ContentLength.Value,
                            _maxReceivedMessageSize));
                }
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Startup filter that registers the <see cref="ODataMessageSizeMiddleware"/> early in the pipeline.
/// </summary>
internal class ODataMessageSizeStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<ODataMessageSizeMiddleware>();
            next(app);
        };
    }
}
