// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    /// <summary>
    /// A class to create HttpRequest.
    /// </summary>
    public class RequestFactory
    {
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

        public static HttpRequest Create(string method, string uri, IEdmModel model, ODataPath path)
        {
            return Create(method, uri, f => { f.Model = model; f.Path = path; });
        }
    }
}
