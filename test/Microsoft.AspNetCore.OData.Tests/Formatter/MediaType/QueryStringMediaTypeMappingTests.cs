// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.MediaType
{
    public class QueryStringMediaTypeMappingTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_WhenParameterNameNull()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new QueryStringMediaTypeMapping(null,"application/json"), "queryStringParameterName");
        }

        [Fact]
        public void Ctor_SetMediaType()
        {
            // Arrange & Act
            QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping("$format", "json", "application/json;odata.streaming=true");

            // Assert
            Assert.Equal("$format", mapping.QueryStringParameterName);
            Assert.Equal("json", mapping.QueryStringParameterValue);

            Assert.Equal("application/json", mapping.MediaType.MediaType);
            var parameter = Assert.Single(mapping.MediaType.Parameters);
            Assert.Equal("odata.streaming", parameter.Name);
            Assert.Equal("true", parameter.Value);
        }

        [Fact]
        public void TryMatchMediaType_ThrowsArgumentNull_WhenRequestIsNull()
        {
            // Arrange & Act
            QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping("$format", "application/json");

            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => mapping.TryMatchMediaType(null), "request");
        }

        [Fact]
        public void TryMatchMediaType_DoesnotMatchRequest_WithNonQueryString()
        {
            // Arrange
            QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping("$format", "application/json");
            HttpRequest request = new DefaultHttpContext().Request;

            // Act
            double mapResult = mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(0, mapResult);
        }

        [Theory]
        [InlineData("?$format=application/json;odata.streaming=true", 1.0)]
        [InlineData("?$format=json", 1.0)]
        [InlineData("?$format=application/json", 0.0)]
        [InlineData("?$format=application/xml", 0.0)]
        [InlineData("?$format=xml", 0.0)]
        public void TryMatchMediaType_MatchRequest_WithStreamPropertyRequest(string queryString, double expect)
        {
            // Arrange
            QueryStringMediaTypeMapping mapping = new QueryStringMediaTypeMapping("$format", "json", "application/json;odata.streaming=true");
            HttpRequest request = new DefaultHttpContext().Request;
            request.QueryString = new QueryString(queryString);

            // Act
            double mapResult = mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(expect, mapResult);
        }
    }
}
