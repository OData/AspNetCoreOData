// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    /// <summary>
    /// A class to create HttpRequest.
    /// </summary>
    public static class RequestFactory
    {
        public static string ReadBody(this HttpRequest request, bool multipleRead = false)
        {
            if (request.Body == null)
            {
                return "";
            }

            // Allows using several time the stream in ASP.Net Core
            if (multipleRead)
            {
                request.EnableBuffering();
            }

            string requestBody = "";
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                requestBody = reader.ReadToEnd();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            if (multipleRead)
            {
                request.Body.Position = 0;
            }

            return requestBody;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static HttpRequest Create()
        {
            HttpContext context = new DefaultHttpContext();
            return context.Request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static HttpRequest Create(IEdmModel model)
        {
            HttpContext context = new DefaultHttpContext();
            context.ODataFeature().Model = model;
            return context.Request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static HttpRequest Create(IEdmModel model, ODataPath path)
        {
            HttpContext context = new DefaultHttpContext();
            context.ODataFeature().Model = model;
            context.ODataFeature().Path = path;
            return context.Request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static HttpRequest Create(Action<IODataFeature> setupAction)
        {
            HttpContext context = new DefaultHttpContext();
            IODataFeature odataFeature = context.ODataFeature();
            setupAction?.Invoke(odataFeature);
            return context.Request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static HttpRequest Create(string method, string uri, Action<IODataFeature> setupAction)
        {
            HttpRequest request = Create(setupAction);

            request.Method = method;
            Uri requestUri = new Uri(uri);
            request.Scheme = requestUri.Scheme;
            request.Host = requestUri.IsDefaultPort ?
                new HostString(requestUri.Host) :
                new HostString(requestUri.Host, requestUri.Port);
            request.QueryString = new QueryString(requestUri.Query);
            request.Path = new PathString(requestUri.AbsolutePath);

            return request;
        }

        public static HttpRequest Create(string method, string uri, IEdmModel model)
        {
            return Create(method, uri, f => { f.Model = model; });
        }

        public static HttpRequest Create(string method, string uri, IEdmModel model, ODataPath path)
        {
            return Create(method, uri, f => { f.Model = model; f.Path = path; });
        }

        private static HttpRequest CreateRequest(IHeaderDictionary headers)
        {
            var context = new DefaultHttpContext();
            context.Features.Get<IHttpRequestFeature>().Headers = headers;
            return context.Request;
        }
    }
}
