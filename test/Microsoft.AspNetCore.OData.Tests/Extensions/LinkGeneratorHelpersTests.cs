// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class LinkGeneratorHelpersTests
    {
        private static IServiceProvider _serviceProvider = BuildServiceProvider();

        [Fact]
        public void CreateODataLinkReturnsODataLinksAsExpectedForNonODataPath()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create("Get", "http://localhost:8080/odata", model: null);
            request.HttpContext.RequestServices = _serviceProvider;
            request.RouteValues = new RouteValueDictionary(new { controller = "Customers" });

            var endpoint = EndpointFactory.CreateRouteEndpoint("Customers", metadata: new object[] { new EndpointNameMetadata("MyName"), });
            request.HttpContext.SetEndpoint(endpoint);

            string odataLink = request.CreateODataLink();

            Assert.Equal("http://localhost:8080/odata", odataLink);
        }

        private static IServiceProvider BuildServiceProvider(/*EndpointDataSource[] dataSources*/)
        {
            IServiceCollection services = new ServiceCollection();

          //  services.TryAddSingleton<LinkGenerator, DefaultLinkGenerator>();

            services.AddRouting();
            services.AddOptions();
            services.AddLogging();

            //services.Configure<RouteOptions>(o =>
            //{
            //    if (dataSources != null)
            //    {
            //        foreach (var dataSource in dataSources)
            //        {
            //           // o.EndpointDataSources.Add(dataSource);
            //        }
            //    }
            //});

            return services.BuildServiceProvider();
        }
    }

    internal static class EndpointFactory
    {
        public static RouteEndpoint CreateRouteEndpoint(
            string template,
            object defaults = null,
            object policies = null,
            object requiredValues = null,
            int order = 0,
            string displayName = null,
            params object[] metadata)
        {
            var routePattern = RoutePatternFactory.Parse(template, defaults, policies, requiredValues);

            return CreateRouteEndpoint(routePattern, order, displayName, metadata);
        }

        public static RouteEndpoint CreateRouteEndpoint(
            RoutePattern routePattern = null,
            int order = 0,
            string displayName = null,
            IList<object> metadata = null)
        {
            return new RouteEndpoint(
                TestConstants.EmptyRequestDelegate,
                routePattern,
                order,
                new EndpointMetadataCollection(metadata ?? Array.Empty<object>()),
                displayName);
        }
    }

    public static class TestConstants
    {
        internal static readonly RequestDelegate EmptyRequestDelegate = (context) => Task.CompletedTask;
    }
}
