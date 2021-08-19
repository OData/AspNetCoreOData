//-----------------------------------------------------------------------------
// <copyright file="ODataOpenApiController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData;
using Microsoft.OpenApi.OData.Edm;
using ODataRoutingSample.OpenApi;

namespace ODataRoutingSample.Controllers
{
    public class ODataOpenApiController : ControllerBase
    {
        private EndpointDataSource _dataSource;

        public ODataOpenApiController(EndpointDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        [HttpGet("$openapi")]
        public ContentResult GetOpenApi()
        {
            OpenApiDocument document = CreateDocument(string.Empty);
            return CreateContent(document);
        }

        [HttpGet("v1/$openapi")]
        public ContentResult GetV1OpenApi()
        {
            OpenApiDocument document = CreateDocument("v1");
            return CreateContent(document);
        }

        [HttpGet("v2{data}/$openapi")]
        public ContentResult GetV2OpenApi(string data)
        {
            OpenApiDocument document = CreateDocument("v2{data}");
            return CreateContent(document);
        }

        private ContentResult CreateContent(OpenApiDocument document)
        {
            HttpContext httpContext = Request.HttpContext;
            (string contentType, OpenApiSpecVersion openApiSpecVersion) = GetContentTypeAndVersion(httpContext);
            httpContext.Response.Headers["Content-Type"] = contentType;

            string output;

            if (openApiSpecVersion == OpenApiSpecVersion.OpenApi3_0)
            {
                if (contentType == "application/json")
                {
                    output = document.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                }
                else
                {
                    output = document.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0);
                }
            }
            else
            {
                if (contentType == "application/json")
                {
                    output = document.SerializeAsJson(OpenApiSpecVersion.OpenApi2_0);
                }
                else
                {
                    output = document.SerializeAsYaml(OpenApiSpecVersion.OpenApi2_0);
                }
            }

            return base.Content(output, contentType);
        }

        private OpenApiDocument CreateDocument(string prefixName)
        {
            IDictionary<string, ODataPath> tempateToPathDict = new Dictionary<string, ODataPath>();
            ODataOpenApiPathProvider provider = new ODataOpenApiPathProvider();
            IEdmModel model = null;
            foreach (var endpoint in _dataSource.Endpoints)
            {
                IODataRoutingMetadata metadata = endpoint.Metadata.GetMetadata<IODataRoutingMetadata>();
                if (metadata == null)
                {
                    continue;
                }

                if (metadata.Prefix != prefixName)
                {
                    continue;
                }
                model = metadata.Model;

                RouteEndpoint routeEndpoint = endpoint as RouteEndpoint;
                if (routeEndpoint == null)
                {
                    continue;
                }

                // get rid of the prefix
                int length = prefixName.Length;
                string routePathTemplate = routeEndpoint.RoutePattern.RawText.Substring(length);
                routePathTemplate = routePathTemplate.StartsWith("/") ? routePathTemplate : "/" + routePathTemplate;

                if (tempateToPathDict.TryGetValue(routePathTemplate, out ODataPath pathValue))
                {
                    var methods = GetHttpMethods(endpoint);
                    foreach (var method in methods)
                    {
                        pathValue.HttpMethods.Add(method);
                    }
                    continue;
                }

                var path = metadata.Template.Translate();
                if (path == null)
                {
                    continue;
                }

                path.PathTemplate = routePathTemplate;
                provider.Add(path);

                var methods1 = GetHttpMethods(endpoint);
                foreach (var method in methods1)
                {
                    path.HttpMethods.Add(method);
                }

                tempateToPathDict[routePathTemplate] = path;
            }

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                PathProvider = provider,
                ServiceRoot = BuildAbsolute()
            };

            return model.ConvertToOpenApi(settings);
        }

        internal static (string, OpenApiSpecVersion) GetContentTypeAndVersion(HttpContext context)
        {
            Contract.Assert(context != null);

            OpenApiSpecVersion specVersion = OpenApiSpecVersion.OpenApi3_0; // by default
            // $format=application/json;version=2.0
            // $format=application/yaml;version=2.0
            // accept=application/json;version3.0
            HttpRequest request = context.Request;

            string dollarFormatValue = null;
            IQueryCollection queryCollection = request.Query;
            if (queryCollection.ContainsKey("$format"))
            {
                StringValues dollarFormat = queryCollection["$format"];
                dollarFormatValue = dollarFormat.FirstOrDefault();
            }

            if (dollarFormatValue != null)
            {
                MediaTypeHeaderValue parsedValue;
                bool success = MediaTypeHeaderValue.TryParse(dollarFormatValue, out parsedValue);
                if (success)
                {
                    NameValueHeaderValue nameValueHeaderValue = parsedValue.Parameters.FirstOrDefault(p => p.Name == "version");
                    if (nameValueHeaderValue != null)
                    {
                        string version = nameValueHeaderValue.Value.Value;
                        if (version == "2.0")
                        {
                            specVersion = OpenApiSpecVersion.OpenApi2_0;
                        }
                    }

                    if (parsedValue.MediaType == "application/yaml")
                    {
                        return ("application/yaml", specVersion);
                    }

                    return ("application/json", specVersion);
                }
            }

            // default
            return ("application/json", OpenApiSpecVersion.OpenApi3_0);
        }

        private static IEnumerable<string> GetHttpMethods(Endpoint endpoint)
        {
            HttpMethodMetadata methodMetadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
            if (methodMetadata != null)
            {
                return methodMetadata.HttpMethods;
            }

            throw new Exception();
        }

        private Uri BuildAbsolute()
        {
            HttpRequest request = Request;
            string wholeRequest = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
            int index = wholeRequest.IndexOf("/$openapi");
            string path = wholeRequest.Substring(0, index);
            return new Uri(path);
        }
    }
}
