//-----------------------------------------------------------------------------
// <copyright file="LinkGeneratorHelpersTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class LinkGeneratorHelpersTests
    {
        [Fact]
        public void CreateODataLink_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.CreateODataLink(), "request");
        }

        [Theory]
        [InlineData("")]
        [InlineData("odata")]
        public void CreateODataLinkReturnsODataLinksAsExpected(string prefix)
        {
            // Arrange
            string baseAddress = "http://localhost:8080/";
            HttpRequest request = RequestFactory.Create("Get", baseAddress, opt => opt.AddRouteComponents(prefix, EdmCoreModel.Instance));
            request.ODataFeature().RoutePrefix = prefix;

            // Act
            string odataLink = request.CreateODataLink();

            // Assert
            Assert.Equal(baseAddress + prefix, odataLink);
        }

        [Theory]
        [InlineData("")]
        [InlineData("odata")]
        public void CreateODataLinkWithODataSegmentsReturnsODataLinksAsExpected(string prefix)
        {
            // Arrange
            string baseAddress = "http://localhost:8080/";
            HttpRequest request = RequestFactory.Create("Get", baseAddress, opt => opt.AddRouteComponents(prefix, EdmCoreModel.Instance));
            request.ODataFeature().RoutePrefix = prefix;

            // Act
            string odataLink = request.CreateODataLink(MetadataSegment.Instance);

            // Assert
            if (prefix == "")
            {
                Assert.Equal($"{baseAddress}$metadata", odataLink);
            }
            else
            {
                Assert.Equal($"{baseAddress}{prefix}/$metadata", odataLink);
            }
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        [InlineData("anything")]
        public void CreateODataLinkReturnsODataLinksWithTemplateAsExpected(string value)
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddRouting(); // because want to use "TemplateBinderFactory"
            IServiceProvider sp = services.BuildServiceProvider();

            string baseAddress = "http://localhost:8080/";
            string prefix = "odata{version}";
            HttpRequest request = RequestFactory.Create("Get", baseAddress, opt => opt.AddRouteComponents(prefix, EdmCoreModel.Instance));
            request.ODataFeature().RoutePrefix = prefix;
            request.RouteValues = new RouteValueDictionary(new { version = value });
            request.HttpContext.RequestServices = sp; // global level SP

            // Act
            string odataLink = request.CreateODataLink();

            // Assert
            Assert.Equal(baseAddress + $"odata{value}", odataLink);
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        [InlineData("anything")]
        public void CreateODataLinkWithODataSegmentsReturnsODataLinksWithTemplateAsExpected(string value)
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddRouting(); // because want to use "TemplateBinderFactory"
            IServiceProvider sp = services.BuildServiceProvider();

            string baseAddress = "http://localhost:8080/";
            string prefix = "odata{version}";
            HttpRequest request = RequestFactory.Create("Get", baseAddress, opt => opt.AddRouteComponents(prefix, EdmCoreModel.Instance));
            request.ODataFeature().RoutePrefix = prefix;
            request.RouteValues = new RouteValueDictionary(new { version = value });
            request.HttpContext.RequestServices = sp; // global level SP

            // Act
            string odataLink = request.CreateODataLink(MetadataSegment.Instance);

            // Assert
            Assert.Equal($"{baseAddress}odata{value}/$metadata", odataLink);
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
