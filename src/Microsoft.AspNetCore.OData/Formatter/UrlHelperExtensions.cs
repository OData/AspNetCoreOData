// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData.UriParser;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// 
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string CreateODataLink(this HttpRequest request,
            string baseAddress, params ODataPathSegment[] segments)
        {
            IList<ODataPathSegment> segmentList = segments as IList<ODataPathSegment>;
            string path = segmentList.GetPathString();
            return baseAddress + "/" + path;
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this IUrlHelper urlHelper, params ODataPathSegment[] segments)
        {
            return urlHelper.CreateODataLink(segments as IList<ODataPathSegment>);
        }

        /// <summary>
        /// Generates an OData link using the request's OData route name and path handler and given segments.
        /// </summary>
        /// <param name="segments">The OData path segments.</param>
        /// <returns>The generated OData link.</returns>
        public static string CreateODataLink(this IUrlHelper urlHelper, IList<ODataPathSegment> segments)
        {
            //string routeName = this.innerHelper.ActionContext.HttpContext.Request.ODataFeature().RouteName;
            //if (String.IsNullOrEmpty(routeName))
            //{
            //    throw Error.InvalidOperation(SRResources.RequestMustHaveODataRouteName);
            //}

            //IODataPathHandler pathHandler = this.innerHelper.ActionContext.HttpContext.Request.GetPathHandler();
            //return CreateODataLink(routeName, pathHandler, segments);

            return "";
        }
    }
}
