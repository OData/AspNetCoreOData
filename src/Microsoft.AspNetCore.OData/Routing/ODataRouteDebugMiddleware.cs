// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
    internal class ODataRouteDebugMiddleware
    {
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

            if (next == null)
            {
                throw Error.ArgumentNull(nameof(next));
            }

            if (routePattern.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                _routePattern = routePattern;
            }
            else
            {
                _routePattern = $"/{routePattern}";
            }

            _next = next;
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
                if (IsJsonAccept(context))
                {
                    await HandleJsonEndpointsAsync(context).ConfigureAwait(false);
                }
                else
                {
                    await HandleEndpointsAsync(context).ConfigureAwait(false);
                }
            }
            else
            {
                await _next(context).ConfigureAwait(false);
            }
        }

        internal async Task HandleJsonEndpointsAsync(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            List<EndpointRouteInfo> infos = new List<EndpointRouteInfo>();
            EndpointDataSource dataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
            foreach (var endpoint in dataSource.Endpoints)
            {
                ControllerActionDescriptor controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor == null)
                {
                    continue;
                }

                EndpointRouteInfo info = new EndpointRouteInfo
                {
                    DisplayName = endpoint.DisplayName,
                    HttpMethods = string.Join(",", GetHttpMethods(endpoint)),
                };

                // template name
                RouteEndpoint routeEndpoint = endpoint as RouteEndpoint;
                if (routeEndpoint != null)
                {
                    info.Template = routeEndpoint.RoutePattern.RawText;
                }
                else
                {
                    info.Template = "N/A";
                }

                IODataRoutingMetadata metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
                if (metadata == null)
                {
                    info.IsODataRoute = false;
                }
                else
                {
                    info.IsODataRoute = true;
                }

                infos.Add(info);
            }

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string output = JsonSerializer.Serialize(infos, options);
            context.Response.Headers["Content_Type"] = "application/json";
            await context.Response.WriteAsync(output).ConfigureAwait(false);
        }

        internal async Task HandleEndpointsAsync(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            StringBuilder nonSb = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            EndpointDataSource dataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
            foreach (var endpoint in dataSource.Endpoints)
            {
                ControllerActionDescriptor controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor == null)
                {
                    continue;
                }

                IODataRoutingMetadata metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
                if (metadata == null)
                {
                    AppendRoute(nonSb, endpoint);
                }
                else
                {
                    AppendRoute(sb, endpoint);
                }
            }

            string output = ODataRouteMappingHtmlTemplate.Replace("ODATACONTENT", sb.ToString(), StringComparison.Ordinal);
            output = output.Replace("NONENDPOINTCONTENT", nonSb.ToString(), StringComparison.Ordinal);

            context.Response.Headers["Content_Type"] = "text/html";
            await context.Response.WriteAsync(output).ConfigureAwait(false);
        }

        internal static bool IsJsonAccept(HttpContext context)
        {
            Contract.Assert(context != null);

            HttpRequest request = context.Request;
            IList<string> accepts = request.Headers[HeaderNames.Accept];
            if (accepts == null || accepts.Count == 0)
            {
                return false;
            }

            // Simply compare "application/json"
            string accept = accepts[0];
            return string.Equals(accept, "application/json", StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> GetHttpMethods(Endpoint endpoint)
        {
            Contract.Assert(endpoint != null);

            HttpMethodMetadata metadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
            if (metadata != null)
            {
                return metadata.HttpMethods;
            }

            return new[] { "N/A" };
        }

        private static void AppendRoute(StringBuilder sb, Endpoint endpoint)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{endpoint.DisplayName}</td>");
            sb.Append($"<td>{string.Join(",", GetHttpMethods(endpoint))}</td>");

            RouteEndpoint routeEndpoint = endpoint as RouteEndpoint;
            if (routeEndpoint != null)
            {
                if (routeEndpoint.RoutePattern.RawText.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append("<td>~").Append(routeEndpoint.RoutePattern.RawText).Append("</td>");
                }
                else
                {
                    sb.Append("<td>~/").Append(routeEndpoint.RoutePattern.RawText).Append("</td>");
                }
            }
            else
            {
                sb.Append("<td>N/A</td></tr>");
            }

            sb.Append("</tr>");
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
    td, th {
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
    <h1 id=""odataendpoint"">OData Endpoint Mapping <a href=""#nonodataendpoint""> >>> Go to non-odata endpoint mapping</a></h1>
    <table>
     <tr>
       <th> Controller & Action </th>
       <th> HttpMethods </th>
       <th> Templates </th>
    </tr>
    ODATACONTENT
    </table>
    <h1 id=""nonodataendpoint"">Non-OData Endpoint Mapping <a href=""#odataendpoint""> >>> Back to odata endpoint mapping</a></h1>
    <table>
     <tr>
       <th> Controller </th>
       <th> HttpMethods </th>
       <th> Templates </th>
    </tr>
    NONENDPOINTCONTENT
    </table>
   </body>
</html>";
    }

    internal class EndpointRouteInfo
    {
        public string DisplayName { get; set; }

        public string HttpMethods { get; set; }

        public string Template { get; set; }

        public bool IsODataRoute { get; set; }
    }
}
