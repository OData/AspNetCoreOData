// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Extensions to generator the Link.
    /// </summary>
    public static class LinkGeneratorHelpers
    {
        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="request">The Http request.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this HttpRequest request, params ODataPathSegment[] segments)
        {
            return request.CreateODataLink(segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the given OData route name, path handler, and segments.
        /// </summary>
        /// <param name="request">The Http request.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this HttpRequest request, IList<ODataPathSegment> segments)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            IODataFeature oDataFeature = request.ODataFeature();
            string odataPath = segments.GetPathString();

            // retrieve the cached base address
            string baseAddress = oDataFeature.BaseAddress;
            if (baseAddress != null)
            {
                return CombinePath(baseAddress, odataPath);
            }

            // if no, calculate the base address
            string uriString = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase);
            string prefix = oDataFeature.PrefixName;
            if (string.IsNullOrEmpty(prefix))
            {
                baseAddress = uriString;
            }
            else
            {
                // Construct the prefix template if it's a template
                RoutePattern routePattern = RoutePatternFactory.Parse(prefix);
                if (!routePattern.Parameters.Any())
                {
                    baseAddress = CombinePath(uriString, prefix);
                }
                else
                {
                    if (TryProcessPrefixTemplate(request, routePattern, out var path))
                    {
                        baseAddress = CombinePath(uriString, path);
                    }
                    else
                    {
                        throw new ODataException(Error.Format(SRResources.CannotProcessPrefixTemplate, prefix));
                    }
                }
            }

            // cache the base address
            oDataFeature.BaseAddress = baseAddress;
            return CombinePath(baseAddress, odataPath);
        }

        private static bool TryProcessPrefixTemplate(HttpRequest request, RoutePattern routePattern, out string path)
        {
            // TODO: Do you have a better way to process the prefix template?
            Contract.Assert(request != null);
            Contract.Assert(routePattern != null);

            HttpContext httpContext = request.HttpContext;
            TemplateBinderFactory factory = request.GetRequiredService<TemplateBinderFactory>();
            TemplateBinder templateBinder = factory.Create(routePattern);

            RouteValueDictionary ambientValues = GetAmbientValues(httpContext);

            var templateValuesResult = templateBinder.GetValues(ambientValues, request.RouteValues);
            if (templateValuesResult == null)
            {
                // We're missing one of the required values for this route.
                path = default;
                return false;
            }

            if (!templateBinder.TryProcessConstraints(httpContext, templateValuesResult.CombinedValues, out var _, out var _))
            {
                path = default;
                return false;
            }

            path = templateBinder.BindValues(templateValuesResult.AcceptedValues);
            int index = path.IndexOf("?", StringComparison.Ordinal); // remove the query string

            if (index >= 0)
            {
                path = path.Substring(0, index);
            }

            return true;
        }

        private static RouteValueDictionary GetAmbientValues(HttpContext httpContext)
        {
            return httpContext?.Features.Get<IRouteValuesFeature>()?.RouteValues;
        }

        private static string CombinePath(string baseAddress, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return baseAddress;
            }

            if (path.StartsWith("/", StringComparison.Ordinal))
            {
                path = path.Substring(1); // remove the first "/"
            }

            if (baseAddress.EndsWith("/", StringComparison.Ordinal))
            {
                return baseAddress + path;
            }
            else
            {
                return $"{baseAddress}/{path}";
            }
        }
    }
}
