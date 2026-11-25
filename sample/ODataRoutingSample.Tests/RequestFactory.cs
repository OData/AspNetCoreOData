using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;

namespace ODataRoutingSample
{
    public static class RequestFactory
    {
        /// <summary>
        /// Creates the <see cref="HttpRequest"/> with OData configuration.
        /// </summary>
        /// <param name="method">The http method.</param>
        /// <param name="uri">The http request uri.</param>
        /// <param name="setupDataConfigurationOption">Data configuration option action</param>
        /// <returns>The HttpRequest.</returns>
        public static HttpRequest Create(string method, string uri, Action<ODataOptions> setupDataConfigurationOption)
        {
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;

            IServiceCollection services = new ServiceCollection();
            services.Configure(setupDataConfigurationOption);
            context.RequestServices = services.BuildServiceProvider();

            request.Method = method;
            var requestUri = new Uri(uri);
            request.Scheme = requestUri.Scheme;
            request.Host = requestUri.IsDefaultPort ? new HostString(requestUri.Host) : new HostString(requestUri.Host, requestUri.Port);
            request.QueryString = new QueryString(requestUri.Query);
            request.Path = new PathString(requestUri.AbsolutePath);
            
            return request;
        }
    }
}
