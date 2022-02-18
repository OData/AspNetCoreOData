//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;

namespace ODataSampleCommon
{
    /// <summary>
    /// A controller for debugging that shows the OData endpoints.
    /// </summary>
    public class ODataEndpointController : ControllerBase
    {
        private EndpointDataSource _dataSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEndpointController" /> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        public ODataEndpointController(EndpointDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        /// <summary>
        /// Get all routes.
        /// </summary>
        /// <returns>The content result.</returns>
        [HttpGet("$odata")]
        public IActionResult GetAllRoutes()
        {
            CreateRouteTables(_dataSource.Endpoints, out var stdRouteTable, out var odataRouteTable);

            string output = ODataRouteMappingHtmlTemplate;
            output = output.Replace("ODATA_ROUTE_CONTENT", odataRouteTable, StringComparison.OrdinalIgnoreCase);
            output = output.Replace("STD_ROUTE_CONTENT", stdRouteTable, StringComparison.OrdinalIgnoreCase);

            return base.Content(output, "text/html");
        }

        private void CreateRouteTables(IReadOnlyList<Endpoint> endpoints, out string stdRouteTable, out string odataRouteTable)
        {
            var stdRoutes = new StringBuilder();
            var odataRoutes = new StringBuilder();
            foreach (var endpoint in _dataSource.Endpoints.OfType<RouteEndpoint>())
            {
                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor == null)
                {
                    continue;
                }

                var metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
                if (metadata == null)
                {
                    AppendRoute(stdRoutes, endpoint);
                }
                else
                {
                    AppendRoute(odataRoutes, endpoint);
                }
            }
            stdRouteTable = stdRoutes.ToString();
            odataRouteTable = odataRoutes.ToString();
        }

        private static string GetHttpMethods(Endpoint endpoint)
        {
            var methodMetadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
            if (methodMetadata == null)
            {
                return "";
            }
            return string.Join(", ", methodMetadata.HttpMethods);
        }

        /// <summary>
        /// Process the endpoint
        /// </summary>
        /// <param name="sb">The string builder to append HTML to.</param>
        /// <param name="endpoint">The endpoint to render.</param>
        private static void AppendRoute(StringBuilder sb, RouteEndpoint endpoint)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{endpoint.DisplayName}</td>");

            sb.Append($"<td>{string.Join(",", GetHttpMethods(endpoint))}</td>");

            sb.Append("<td>");
            var link = "" + endpoint.RoutePattern.RawText.TrimStart('/');
            sb.Append($"<a href=\"/{link}\">~/{link}</a>");
            sb.Append("</td>");

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
        <a href=""#standard"">Got to non-OData endpoint mappings</a>
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
    }
}
