// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="request">The URL helper.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this HttpRequest request, params ODataPathSegment[] segments)
        {
            return request.CreateODataLink(segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the given OData route name, path handler, and segments.
        /// </summary>
        /// <param name="request">The name of the OData route.</param>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this HttpRequest request, IList<ODataPathSegment> segments)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string aUriString = null;
            LinkGenerator linkGenerator = request.HttpContext.RequestServices?.GetService<LinkGenerator>();
            if (linkGenerator == null)
            {
                aUriString = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase);
            }
            else
            {
                Endpoint endPoint = request.HttpContext.GetEndpoint();
                EndpointNameMetadata endpointName = endPoint.Metadata.GetMetadata<EndpointNameMetadata>();

                if (endpointName != null)
                {
                    aUriString = linkGenerator.GetUriByName(request.HttpContext, endpointName.EndpointName,
                        request.RouteValues, request.Scheme, request.Host, request.PathBase);
                }

                if (aUriString == null)
                {
                    RouteNameMetadata routeName = endPoint.Metadata.GetMetadata<RouteNameMetadata>();
                    if (routeName != null)
                    {
                        aUriString = linkGenerator.GetUriByRouteValues(request.HttpContext, routeName.RouteName,
                            request.RouteValues, request.Scheme, request.Host, request.PathBase);
                    }
                }
            }

            aUriString = aUriString[aUriString.Length - 1] == '/' ? aUriString.Substring(0, aUriString.Length - 1) : aUriString;

            string odataPath = segments.GetPathString();

            if (string.IsNullOrEmpty(odataPath))
            {
                return aUriString;
            }

            return $"{aUriString}/{odataPath}";
        }
    }
}
