// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    /// <summary>
    /// A class to create HttpContext.
    /// </summary>
    public class HttpContextHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static HttpContext Create()
        {
            return new DefaultHttpContext();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static HttpContext Create(string requestMethod, string uri)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = httpContext.Request;

            request.Method = requestMethod;
            Uri requestUri = new Uri(uri);
            request.Scheme = requestUri.Scheme;
            request.Host = requestUri.IsDefaultPort ?
                new HostString(requestUri.Host) :
                new HostString(requestUri.Host, requestUri.Port);
            request.QueryString = new QueryString(requestUri.Query);
            request.Path = new PathString(requestUri.AbsolutePath);
            return httpContext;
        }
    }
}
