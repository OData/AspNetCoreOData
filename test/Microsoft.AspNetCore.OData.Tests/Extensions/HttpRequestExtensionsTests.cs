// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Moq;
using System;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class HttpRequestExtensionsTests
    {
        [Fact]
        public void ODataFeature_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.ODataFeature(), "request");
        }

        [Fact]
        public void ODataBatchFeature_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.ODataBatchFeature(), "request");
        }

        [Fact]
        public void GetModel_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetModel(), "request");
        }

        [Fact]
        public void GetTimeZoneInfo_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetTimeZoneInfo(), "request");
        }

        [Fact]
        public void IsNoDollarQueryEnable_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.IsNoDollarQueryEnable(), "request");
        }

        [Fact]
        public void IsCountRequest_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.IsCountRequest(), "request");
        }

        [Fact]
        public void GetReaderSettings_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetReaderSettings(), "request");
        }

        [Fact]
        public void GetWriterSettings_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetWriterSettings(), "request");
        }

        [Fact]
        public void GetDeserializerProvider_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetDeserializerProvider(), "request");
        }

        [Fact]
        public void GetNextPageLink_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetNextPageLink(4, 4, null), "request");
        }

        [Fact]
        public void GetNextPageLink_Returns_Uri()
        {
            // Arrange & Act & Assert
            HttpRequest request = RequestFactory.Create("get", "http://localhost");

            // Act
            Uri uri = request.GetNextPageLink(4, 4, null);

            // Assert
            Assert.Equal(new Uri("http://localhost/?$skip=4"), uri);
        }

        [Fact]
        public void CreateETag_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.CreateETag(null, null), "request");
        }

        [Fact]
        public void GetETagHandler_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetETagHandler(), "request");
        }

        [Fact]
        public void IsODataQueryRequest_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.IsODataQueryRequest(), "request");
        }

        [Fact]
        public void GetRouteServices_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetRouteServices(), "request");
        }

        [Fact]
        public void CreateRouteServices_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.CreateRouteServices(""), "request");
        }

        [Fact]
        public void CreateRouteServices_ThrowsInvalidOperation_RouteServices()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create();
            request.ODataFeature().Services = new Mock<IServiceProvider>().Object;

            // Act
            Action test = () => request.CreateRouteServices("odata");

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(test, "A request container already exists on the request.");
        }

        [Fact]
        public void ClearRouteServices_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.ClearRouteServices(), "request");
        }

        [Fact]
        public void GetODataVersion_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetODataVersion(), "request");
        }

        [Fact]
        public void GetQueryOptions_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetQueryOptions(), "request");
        }

        [Fact]
        public void ODataServiceVersion_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.ODataServiceVersion(), "request");
        }

        [Fact]
        public void ODataMaxServiceVersion_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.ODataMaxServiceVersion(), "request");
        }

        [Fact]
        public void ODataMinServiceVersion_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.ODataMinServiceVersion(), "request");
        }
    }
}