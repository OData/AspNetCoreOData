// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static HttpContext Create(string requestMethod, string uri, string requestBody, string contentType = "application/json")
        {
            HttpContext httpContext = Create(requestMethod, uri);
            byte[] body = Encoding.UTF8.GetBytes(requestBody);
            httpContext.Request.Body = new MemoryStream(body);
            httpContext.Request.ContentType = contentType;
            httpContext.Request.ContentLength = body.Length;
            return httpContext;
        }

        public static HttpContext Create(int statusCode)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.StatusCode = statusCode;
            return httpContext;
        }
    }
}
