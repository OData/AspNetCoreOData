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
    }
}
