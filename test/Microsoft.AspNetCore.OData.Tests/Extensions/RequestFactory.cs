// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    /// <summary>
    /// A class to create HttpRequest for tests.
    /// </summary>
    public static class RequestFactory
    {
        /// <summary>
        /// Reads the request body as string.
        /// </summary>
        /// <param name="request">The Http request.</param>
        /// <param name="multipleRead">true/false for multiple read.</param>
        /// <returns>The request body or empty string.</returns>
        public static string ReadBody(this HttpRequest request, bool multipleRead = false)
        {
            if (request == null || request.Body == null)
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
        /// Creates the <see cref="HttpRequest"/> with OData configuration.
        /// </summary>
        /// <param name="setupAction">The OData options configuration.</param>
        /// <returns>The Http Request.</returns>
        public static HttpRequest Create(Action<ODataOptions> setupAction)
        {
            return Create("Get", "http://localhost", setupAction);
        }

        /// <summary>
        /// Creates the <see cref="HttpRequest"/> with OData configuration.
        /// </summary>
        /// <param name="method">The http method.</param>
        /// <param name="uri">The http request uri.</param>
        /// <param name="setupAction">The OData configuration.</param>
        /// <returns>The HttpRequest.</returns>
        public static HttpRequest Create(string method, string uri, Action<ODataOptions> setupAction)
        {
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;

            IServiceCollection services = new ServiceCollection();
            services.Configure(setupAction);
            context.RequestServices = services.BuildServiceProvider();

            request.Method = method;
            Uri requestUri = new Uri(uri);
            request.Scheme = requestUri.Scheme;
            request.Host = requestUri.IsDefaultPort ? new HostString(requestUri.Host) : new HostString(requestUri.Host, requestUri.Port);
            request.QueryString = new QueryString(requestUri.Query);
            request.Path = new PathString(requestUri.AbsolutePath);

            //request.Host = HostString.FromUriComponent(BaseAddress);
            //if (BaseAddress.IsDefaultPort)
            //{
            //    request.Host = new HostString(request.Host.Host);
            //}
            //var pathBase = PathString.FromUriComponent(BaseAddress);
            //if (pathBase.HasValue && pathBase.Value.EndsWith("/"))
            //{
            //    pathBase = new PathString(pathBase.Value[..^1]); // All but the last character.
            //}
            //request.PathBase = pathBase;

            return request;
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

        public static HttpRequest Create(string method, string uri, IEdmModel model)
        {
            HttpRequest request = Create(method, uri, opt => opt.AddModel("odata", model));
            IODataFeature feature = request.ODataFeature();
            feature.PrefixName = "odata";
            feature.Model = model;
            return request;
        }

        /// <summary>
        /// Confgiures the http request with OData values.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="path">The OData path.</param>
        /// <returns></returns>
        public static HttpRequest Configure(this HttpRequest request, string prefix, IEdmModel model, ODataPath path)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IODataFeature feature = request.ODataFeature();
            feature.PrefixName = prefix;
            feature.Model = model;
            feature.Path = path;
            return request;
        }

        private static HttpRequest CreateRequest(IHeaderDictionary headers)
        {
            var context = new DefaultHttpContext();
            context.Features.Get<IHttpRequestFeature>().Headers = headers;
            return context.Request;
        }

        public static TKey GetKeyFromLinkUri<TKey>(this HttpRequest request, Uri link)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (link == null)
            {
                throw new ArgumentNullException(nameof(link));
            }

            var serviceRoot = request.CreateODataLink();
            IEdmModel model = request.GetModel();

            ODataUriParser uriParser = new ODataUriParser(model, new Uri(serviceRoot), new Uri(link.LocalPath, UriKind.Relative),
                request.GetSubServiceProvider());

            var odataPath = uriParser.ParsePath();

            var keySegment = odataPath.Where(p => p is KeySegment).FirstOrDefault() as KeySegment;

            if (keySegment == null || !keySegment.Keys.Any())
                throw new InvalidOperationException("This link does not contain a key.");

            // Return the key value of the first segment
            return (TKey)keySegment.Keys.First().Value;
        }
    }
}
