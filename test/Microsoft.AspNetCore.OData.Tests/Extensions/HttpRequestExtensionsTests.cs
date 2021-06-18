// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
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
        public void GetSubServiceProvider_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.GetSubServiceProvider(), "request");
        }

        [Fact]
        public void CreateSubServiceProvider_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.CreateSubServiceProvider(""), "request");
        }

        [Fact]
        public void DeleteSubRequestProvider_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            HttpRequest request = null;
            ExceptionAssert.ThrowsArgumentNull(() => request.DeleteSubRequestProvider(false), "request");
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