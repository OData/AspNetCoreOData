// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// Extensions.
    /// </summary>
    internal static class UrlHelperExtensions
    {
        /// <summary>
        /// Creates the OData link
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="segments">The segments.</param>
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
