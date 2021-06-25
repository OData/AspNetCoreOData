// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ODataBatchPathMappingTest
    {
        [Fact]
        public void ODataBatchPathMappingWorksForNormalTemplate()
        {
            // Arrange
            ODataBatchHandler handler = new Mock<ODataBatchHandler>().Object;
            var mapping = new ODataBatchPathMapping();
            var request = RequestFactory.Create(HttpMethods.Get, "http://localhost/$batch");
            mapping.AddRoute("odata", "$batch", handler);

            // Act & Assert
            Assert.True(mapping.TryGetPrefixName(request.HttpContext, out string outputName, out ODataBatchHandler outHandler));
            Assert.Equal("odata", outputName);
            Assert.Same(outHandler, handler);
        }

        [Theory]
        [InlineData("world")]
        [InlineData("kit")]
        public void ODataBatchPathMappingWorksForSimpleTemplate(string name)
        {
            // Arrange
            string routeName = "odata";
            string routeTemplate = "hello/{name}/$batch";
            string uri = "http://localhost/hello/" + name + "/$batch";
            var request = RequestFactory.Create(HttpMethods.Get, uri);
            var mapping = new ODataBatchPathMapping();
            ODataBatchHandler handler = new Mock<ODataBatchHandler>().Object;

            // Act
            mapping.AddRoute(routeName, routeTemplate, handler);

            bool result = mapping.TryGetPrefixName(request.HttpContext, out string outputName, out ODataBatchHandler outHandler);

            // Assert
            Assert.True(result);
            Assert.Equal(outputName, routeName);
            var routeData = request.ODataFeature().BatchRouteData;
            Assert.NotNull(routeData);
            var actual = Assert.Single(routeData);
            Assert.Equal("name", actual.Key);
            Assert.Equal(name, actual.Value);
        }

        [Theory]
        [InlineData("1", "4")]
        [InlineData("2", "3")]
        [InlineData("latest", "unknown")]
        public void ODataBatchPathMappingWorksForComplexTemplate(string version, string spec)
        {
            // Arrange
            string routeName = "odata";
            string routeTemplate = "/v{api-version:apiVersion}/odata{spec}/$batch";
            string uri = "http://localhost/v" + version + "/odata" + spec + "/$batch";
            var request = RequestFactory.Create(HttpMethods.Get, uri);
            var mapping = new ODataBatchPathMapping();
            ODataBatchHandler handler = new Mock<ODataBatchHandler>().Object;

            // Act
            mapping.AddRoute(routeName, routeTemplate, handler);

            bool result = mapping.TryGetPrefixName(request.HttpContext, out string outputName, out ODataBatchHandler outHandler);

            // Assert
            Assert.True(result);
            Assert.Equal(outputName, routeName);
            var routeData = request.ODataFeature().BatchRouteData;
            Assert.NotNull(routeData);
            Assert.Equal(new[] { "api-version", "spec" }, routeData.Keys);
            Assert.Equal(version, routeData["api-version"]);
            Assert.Equal(spec, routeData["spec"]);
        }
    }
}
