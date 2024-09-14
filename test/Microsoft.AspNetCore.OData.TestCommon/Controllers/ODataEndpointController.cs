//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.OData.TestCommon;

/// <summary>
/// If want to see the templates, add this controller into controller feature:
/// services.ConfigureControllers(typeof(ODataEndpointController));
/// </summary>
public class ODataEndpointController : ControllerBase
{
    /*
    [Fact]
    public async Task TestRoutes()
    {
        // Arrange
        string requestUri = "$odata";
        HttpClient client = CreateClient();

        // Act
        var response = await client.GetAsync(requestUri);

        // Assert
        response.EnsureSuccessStatusCode();
        string payload = await response.Content.ReadAsStringAsync();
    }
    */
    private EndpointDataSource _dataSource;

    public ODataEndpointController(EndpointDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    [HttpGet("$odata")]
    public IEnumerable<EndpointRouteInfo> GetAllRoutes()
    {
        if (_dataSource == null)
        {
            return Enumerable.Empty<EndpointRouteInfo>();
        }

        IList<EndpointRouteInfo> routeInfos = new List<EndpointRouteInfo>();
        foreach (var endpoint in _dataSource.Endpoints)
        {
            RouteEndpoint routeEndpoint = endpoint as RouteEndpoint;
            if (routeEndpoint == null)
            {
                continue;
            }

            ControllerActionDescriptor controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor == null)
            {
                continue;
            }

            EndpointRouteInfo routeInfo = new EndpointRouteInfo();

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

            routeInfo.ControllerFullName = controllerActionDescriptor.ControllerTypeInfo.FullName;
            routeInfo.ActionFullName = action.ToString();
            routeInfo.Template = routeEndpoint.RoutePattern.RawText;

            var httpMethods = GetHttpMethods(endpoint);
            routeInfo.HttpMethods = string.Join(",", httpMethods);

            IODataRoutingMetadata metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
            if (metadata == null)
            {
                routeInfo.IsODataRoute = false;
            }
            else
            {
                routeInfo.IsODataRoute = true;
            }

            routeInfos.Add(routeInfo);
        }

        return routeInfos;
    }

    private static IEnumerable<string> GetHttpMethods(Endpoint endpoint)
    {
        HttpMethodMetadata metadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        if (metadata != null)
        {
            return metadata.HttpMethods;
        }

        return new[] { "No HttpMethod" };
    }
}
