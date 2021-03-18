// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;

namespace ODataDynamicModel.Controllers
{
    public class ODataEndpointController : ControllerBase
    {
        private EndpointDataSource _dataSource;

        public ODataEndpointController(EndpointDataSource dataSource)
        {
            _dataSource = dataSource;
        }

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
                    AppendNonODataRoute(nonSb, endpoint);
                    continue;
                }

                // controller and action details
                StringBuilder action = new StringBuilder();
                if (controllerActionDescriptor.MethodInfo.ReturnType != null)
                {
                    action.Append(controllerActionDescriptor.MethodInfo.ReturnType.Name + " ");
                }
                else
                {
                    action.Append("void ");
                }
                action.Append(controllerActionDescriptor.MethodInfo.Name + "(");
                action.Append(string.Join(",", controllerActionDescriptor.MethodInfo.GetParameters().Select(p => p.ParameterType.Name)));
                action.Append(")");
                string actionName = controllerActionDescriptor.MethodInfo.Name;

                sb.Append("<tr>");
                sb.Append($"<td>{GetActionDesciption(controllerActionDescriptor)}</td>");

                // http methods
                sb.Append($"<td>{string.Join(",", GetHttpMethods(endpoint))}</td>");

                // template name
                RouteEndpoint routeEndpoint = endpoint as RouteEndpoint;
                if (routeEndpoint != null)
                {
                    sb.Append("<td>~/").Append(routeEndpoint.RoutePattern.RawText).Append("</td></tr>");
                }
                else
                {
                    sb.Append("<td>---NON RouteEndpoint---</td></tr>");
                }
            }

            string output = ODataRouteMappingHtmlTemplate.Replace("{CONTENT}", sb.ToString());
            output = output.Replace("{NONENDPOINTCONTENT}", nonSb.ToString());

            return base.Content(output, "text/html");
        }

        private static string GetActionDesciption(ControllerActionDescriptor actionDescriptor)
        {
            // controller and action details
            StringBuilder action = new StringBuilder();
            if (actionDescriptor.MethodInfo.ReturnType != null)
            {
                action.Append(actionDescriptor.MethodInfo.ReturnType.Name + " ");
            }
            else
            {
                action.Append("void ");
            }

            action.Append(actionDescriptor.ControllerTypeInfo.FullName);
            action.Append(".");
            action.Append(actionDescriptor.MethodInfo.Name + "(");
            action.Append(string.Join(",", actionDescriptor.MethodInfo.GetParameters().Select(p => p.ParameterType.Name)));
            action.Append(")");
            return action.ToString();
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
        /// Process the non-odata route
        /// </summary>
        /// <param name="sb">The string builder</param>
        /// <param name="endPoint">The endpoint.</param>
        private static void AppendNonODataRoute(StringBuilder sb, Endpoint endpoint)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{endpoint.DisplayName}</td>");

            sb.Append($"<td>{string.Join(",", GetHttpMethods(endpoint))}</td>");

            RouteEndpoint routeEndpoint = endpoint as RouteEndpoint;
            if (routeEndpoint != null)
            {
                if (routeEndpoint.RoutePattern.RawText.StartsWith("/"))
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
    {CONTENT}
    </table>
    <h1 id=""nonodataendpoint"">Non-OData Endpoint Mapping <a href=""#odataendpoint""> >>> Back to odata endpoint mapping</a></h1>
    <table>
     <tr>
       <th> Controller </th>
       <th> HttpMethods </th>
       <th> Templates </th>
    </tr>
    {NONENDPOINTCONTENT}
    </table>
   </body>
</html>";
    }
}

