// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;

namespace ODataSampleCommon
{
    /// <summary>
    /// A debug controller to show the OData endpoint.
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
        public ContentResult GetAllRoutes()
        {
            StringBuilder nonSb = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            foreach (var endpoint in _dataSource.Endpoints)
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

            string output = ODataRouteMappingHtmlTemplate.Replace("ODATACONTENT", sb.ToString(), StringComparison.OrdinalIgnoreCase);
            output = output.Replace("NONENDPOINTCONTENT", nonSb.ToString(), StringComparison.OrdinalIgnoreCase);

            return base.Content(output, "text/html");
        }

        private static IEnumerable<string> GetHttpMethods(Endpoint endpoint)
        {
            HttpMethodMetadata metadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
            if (metadata != null)
            {
                return metadata.HttpMethods;
            }

            return new[] { "No HttpMethodMetadata" };
        }

        /// <summary>
        /// Process the endpoint
        /// </summary>
        /// <param name="sb">The string builder</param>
        /// <param name="endpoint">The endpoint.</param>
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
                sb.Append("<td>---NON RouteEndpoint---</td></tr>");
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
}
