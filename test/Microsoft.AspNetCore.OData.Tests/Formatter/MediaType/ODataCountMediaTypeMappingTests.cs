// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.MediaType
{
    public class ODataCountMediaTypeMappingTests
    {
        private static ODataCountMediaTypeMapping Mapping = new ODataCountMediaTypeMapping();

        [Fact]
        public void Ctor_SetMediaType()
        {
            // Arrange & Act & Assert
            Assert.Equal("text/plain", Mapping.MediaType.MediaType);
        }

        [Fact]
        public void TryMatchMediaType_ThrowsArgumentNull_WhenRequestIsNull()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => Mapping.TryMatchMediaType(null), "request");
        }

        [Fact]
        public void TryMatchMediaType_DoesnotMatchRequest_WithNonODataRequest()
        {
            // Arrange
            HttpRequest request = new DefaultHttpContext().Request;

            // Act
            double mapResult = Mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaType_MatchRequest_WithDollarCountRequest()
        {
            // Arrange
            HttpRequest request = new DefaultHttpContext().Request;
            request.ODataFeature().Path = new ODataPath(CountSegment.Instance);

            // Act
            double mapResult = Mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(1.0, mapResult);
        }
    }
}