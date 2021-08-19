//-----------------------------------------------------------------------------
// <copyright file="ODataOpenApiMiddleware.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Primitives;

namespace ODataRoutingSample.OpenApi
{
    public class ODataOpenApiMiddleware
    {
        private readonly RequestDelegate _next;
        private Dictionary<TemplateMatcher, string> _templateMappings = new Dictionary<TemplateMatcher, string>();
        private string _requestName = "$openapi";

        /// <summary>
        /// Instantiates a new instance of <see cref="ODataOpenApiMiddleware"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider, we don't inject the ODataOptions.</param>
        /// <param name="next">The next middleware.</param>
        /// <param name="requestName">The request name.</param>
        public ODataOpenApiMiddleware(IServiceProvider serviceProvider, RequestDelegate next, string requestName)
        {
            _next = next;
            _requestName = requestName;

            // We inject the service provider to let the middle ware pass without ODataOptions injected.
            IOptions<ODataOptions> odataOptionsOptions = serviceProvider?.GetService<IOptions<ODataOptions>>();
            if (odataOptionsOptions != null && odataOptionsOptions.Value != null)
            {
                Initialize(odataOptionsOptions.Value);
            }
        }

        /// <summary>
        /// Invoke the OData $openapi middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string prefixName;
            if (TryGetPrefixName(context, out prefixName))
            {
                await ProcessOpenApiAsync(context, prefixName).ConfigureAwait(false);
            }
            else
            {
                await _next(context).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// process a $openapi request.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="prefixName">The related prefix.</param>
        /// <returns></returns>
        public virtual async Task ProcessOpenApiAsync(HttpContext context, string prefixName)
        {
            var doc = OpenApiDocumentExtensions.CreateDocument(context, prefixName);

            (string contentType, OpenApiSpecVersion openApiSpecVersion) = GetContentTypeAndVersion(context);
            context.Response.Headers["Content-Type"] = contentType;

            string output;

            if (openApiSpecVersion == OpenApiSpecVersion.OpenApi3_0)
            {
                if (contentType == "application/json")
                {
                    output = doc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                }
                else
                {
                    output = doc.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0);
                }
            }
            else
            {
                if (contentType == "application/json")
                {
                    output = doc.SerializeAsJson(OpenApiSpecVersion.OpenApi2_0);
                }
                else
                {
                    output = doc.SerializeAsYaml(OpenApiSpecVersion.OpenApi2_0);
                }
            }

            await context.Response.WriteAsync(output);
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

        /// <summary>
        /// Initialize the middleware
        /// </summary>
        /// <param name="options">The OData options.</param>
        private void Initialize(ODataOptions options)
        {
            Contract.Assert(options != null);

            foreach (var model in options.RouteComponents)
            {
                string openapiPath = string.IsNullOrEmpty(model.Key) ? $"/{_requestName}" : $"/{model.Key}/{_requestName}";
                AddRoute(model.Key, openapiPath);
            }
        }

        /// <summary>
        /// Add a route name and template for openapi.
        /// </summary>
        /// <param name="prefixName">The route prefix name.</param>
        /// <param name="routeTemplate">The route template.</param>
        private void AddRoute(string prefixName, string routeTemplate)
        {
            string newRouteTemplate = routeTemplate.StartsWith("/", StringComparison.Ordinal) ? routeTemplate.Substring(1) : routeTemplate;
            RouteTemplate parsedTemplate = TemplateParser.Parse(newRouteTemplate);
            TemplateMatcher matcher = new TemplateMatcher(parsedTemplate, new RouteValueDictionary());
            _templateMappings[matcher] = prefixName;
        }

        /// <summary>
        /// Try and get the openapi handler for a given path.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="prefixName">The route/prefix name if found or null.</param>
        /// <returns>true if a route name is found, otherwise false.</returns>
        private bool TryGetPrefixName(HttpContext context, out string prefixName)
        {
            Contract.Assert(context != null);

            prefixName = null;
            string path = context.Request.Path;
            foreach (var item in _templateMappings)
            {
                RouteValueDictionary routeData = new RouteValueDictionary();
                if (item.Key.TryMatch(path, routeData))
                {
                    prefixName = item.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
