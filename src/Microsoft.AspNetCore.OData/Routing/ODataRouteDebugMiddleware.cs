//-----------------------------------------------------------------------------
// <copyright file="ODataRouteDebugMiddleware.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// An OData route debug middleware
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class ODataRouteDebugMiddleware
    {
        private static IReadOnlyList<string> EmptyHeaders = Array.Empty<string>();
        private readonly RequestDelegate _next;
        private readonly string _routePattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteDebugMiddleware"/> class.
        /// </summary>
        /// <param name="routePattern">The route pattern.</param>
        /// <param name="next">The next middleware.</param>
        public ODataRouteDebugMiddleware(string routePattern, RequestDelegate next)
        {
            if (routePattern == null)
            {
                throw Error.ArgumentNull(nameof(routePattern));
            }

            // ensure _routePattern starts with /
            _routePattern = routePattern.StartsWith('/') ? routePattern : $"/{routePattern}";
            _next = next ?? throw Error.ArgumentNull(nameof(next)); ;
        }

        /// <summary>
        /// Invoke the OData Route debug middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            HttpRequest request = context.Request;

            if (string.Equals(request.Path.Value, _routePattern, StringComparison.OrdinalIgnoreCase))
            {
                var routeInfoList = GetRouteInfo(context);
                if (AcceptsJson(request.Headers))
                {
                    await WriteRoutesAsJson(context, routeInfoList).ConfigureAwait(false);
                }
                else
                {
                    await WriteRoutesAsHtml(context, routeInfoList).ConfigureAwait(false);
                }
            }
            else
            {
                await _next(context).ConfigureAwait(false);
            }
        }

        internal static IList<EndpointRouteInfo> GetRouteInfo(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            var routInfoList = new List<EndpointRouteInfo>();
            var dataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
            foreach (var endpoint in dataSource.Endpoints)
            {
                ControllerActionDescriptor controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor == null)
                {
                    continue;
                }

                var routeEndpoint = endpoint as RouteEndpoint;
                var metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();

                var info = new EndpointRouteInfo
                {
                    DisplayName = endpoint.DisplayName,
                    HttpMethods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? EmptyHeaders,
                    Pattern = routeEndpoint?.RoutePattern?.RawText ?? "N/A",
                    IsODataRoute = metadata != null,
                };

                routInfoList.Add(info);
            }

            return routInfoList;
        }

        internal static async Task WriteRoutesAsJson(HttpContext context, IList<EndpointRouteInfo> routeInfoList)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string output = JsonSerializer.Serialize(routeInfoList, options);
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(output).ConfigureAwait(false);
        }

        internal static async Task WriteRoutesAsHtml(HttpContext context, IList<EndpointRouteInfo> routeInfoList)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            var stdRouteTable = new StringBuilder();
            var odataRouteTable = new StringBuilder();
            foreach (var routeInfo in routeInfoList)
            {
                if (routeInfo.IsODataRoute)
                {
                    AppendRoute(odataRouteTable, routeInfo);
                }
                else
                {
                    AppendRoute(stdRouteTable, routeInfo);
                }
            }

            string output = ODataRouteMappingHtmlTemplate;
            output = output.Replace("ODATA_ROUTE_CONTENT", odataRouteTable.ToString(), StringComparison.Ordinal);
            output = output.Replace("STD_ROUTE_CONTENT", stdRouteTable.ToString(), StringComparison.Ordinal);

            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(output).ConfigureAwait(false);
        }

        internal static bool AcceptsJson(IHeaderDictionary headers)
        {
            var acceptHeaders = MediaTypeHeaderValue.ParseList(headers[HeaderNames.Accept]);

            var result = acceptHeaders.Any(h =>
                h.IsSubsetOf(new MediaTypeHeaderValue(MediaTypeNames.Application.Json)));
            return result;
        }

        private static void AppendRoute(StringBuilder builder, EndpointRouteInfo routeInfo)
        {
            builder.Append("<tr>");
            builder.Append($"<td>{routeInfo.DisplayName}</td>");
            builder.Append($"<td>{string.Join(",", routeInfo.HttpMethods)}</td>");

            if (routeInfo.Pattern == null)
            {
                builder.Append($"<td>N/A</td>");
            }
            else if (routeInfo.HttpMethods.Contains("GET"))
            {
                builder.Append($"<td><a href=\"{routeInfo.Pattern}\">{routeInfo.Pattern}</a></td>");
            }
            else
            {
                builder.Append($"<td>{routeInfo.Pattern}</td>");
            }
            builder.AppendLine("</tr>");
        }


        private static string ODataRouteMappingHtmlTemplate = @"<html>
<head>
    <title>OData Endpoint Routing Debugger</title>
    <style>
        table {
            font-family: arial, sans-serif;
            border-collapse: collapse;
            width: 100%;
        }
        td,
        th {
            border: 1px solid #dddddd;
            text-align: left;
            padding: 8px;
        }
        tr:nth-child(even) {
            background-color: #dddddd;
        }
    </style>
</head>
<body>
    <h1 id=""odata"">OData Endpoint Mappings</h1>
    <p>
        <a href=""#standard"">Go to non-OData endpoint mappings</a>
    </p>
    <table>
        <tr>
            <th> Controller & Action </th>
            <th> HttpMethods </th>
            <th> Template </th>
        </tr>
        ODATA_ROUTE_CONTENT
    </table>
    <h1 id=""standard"">Non-OData Endpoint Mappings</h1>
    <p>
        <a href=""#odata"">Go to OData endpoint mappings</a>
    </p>
    <table>
        <tr>
            <th> Controller </th>
            <th> HttpMethods </th>
            <th> Template </th>
        </tr>
        STD_ROUTE_CONTENT
    </table>
</body>
</html>";

        internal class EndpointRouteInfo
        {
            public string DisplayName { get; set; }

            public IReadOnlyList<string> HttpMethods { get; set; }

            public string Pattern { get; set; }

            public bool IsODataRoute { get; set; }
        }
    }
}
