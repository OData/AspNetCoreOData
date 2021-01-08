// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ODataRoutingSample.Extensions
{
    public static class ODataRouteHandler
    {
        /// <summary>
        /// Handle ~/$odata request
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>The task.</returns>
        public static async Task HandleOData(HttpContext context)
        {
            EndpointDataSource dataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();

            StringBuilder sb = new StringBuilder();
            foreach (var endpoint in dataSource.Endpoints)
            {
                IODataRoutingMetadata metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
                if (metadata == null)
                {
                    continue;
                }

                ControllerActionDescriptor controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor == null)
                {
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
                sb.Append($"<td>{controllerActionDescriptor.ControllerTypeInfo.FullName}</td>");
                sb.Append($"<td>{action}</td>");

                // http methods
                string httpMethods = string.Join(",", metadata.HttpMethods);
                sb.Append($"<td>{httpMethods.ToUpper()}</td>");

                // OData routing templates
                RouteEndpoint routeEndpoint = endpoint as RouteEndpoint;
                if (routeEndpoint != null)
                {
                    sb.Append("<td>~/").Append(routeEndpoint.RoutePattern.RawText).Append("</td></tr>");
                }
                else
                {
                    int index = 1;
                    sb.Append("<td>");
                    foreach (var template in metadata.Template.GetTemplates())
                    {
                        sb.Append($"{index++})");
                        if (string.IsNullOrEmpty(metadata.Prefix))
                        {
                            sb.Append(" ~/");
                        }
                        else
                        {
                            sb.Append($" ~/{metadata.Prefix}/");
                        }

                        sb.Append(template).Append("<br/>");
                    }
                    sb.Append("</td></tr>");
                }
            }

            string output = ODataRouteMappingHtmlTemplate.Replace("{CONTNET}", sb.ToString());
            context.Response.Headers["Content_Type"] = "text/html";

            await context.Response.WriteAsync(output);

            //string content = sb.ToString();
            //byte[] requestBytes = Encoding.UTF8.GetBytes(content);
            //context.Response.Headers.ContentLength = requestBytes.Length;
            //context.Response.Body = new MemoryStream(requestBytes);
            //await Task.CompletedTask;
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
    <h1>OData Endpoint Mapping</h1>
    <table>
     <tr>
       <th> Controller </th>
       <th> Action </th>
       <th> HttpMethods </th>
       <th> Templates </th>
    </tr>
    {CONTNET}
    </table>
   </body>
</html>";
    }
}
