//-----------------------------------------------------------------------------
// <copyright file="HttpRequestODataQueryExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class HttpRequestODataQueryExtensionsTests
{
    [Fact]
    public void GetETag_ThrowsArgumentNull_Request()
    {
        // Arrange & Act & Assert
        HttpRequest request = null;
        ExceptionAssert.ThrowsArgumentNull(() => request.GetETag(null), "request");
    }

    [Fact]
    public void GetETag_Returns_Null()
    {
        // Arrange & Act & Assert
        HttpRequest request = new Mock<HttpRequest>().Object;

        // Act
        ETag etag = request.GetETag(null);

        // Assert
        Assert.Null(etag);
    }

    [Fact]
    public void GetETag_Returns_ETagAny()
    {
        // Arrange & Act & Assert
        HttpRequest request = new Mock<HttpRequest>().Object;

        // Act
        ETag etag = request.GetETag(EntityTagHeaderValue.Any);

        // Assert
        Assert.NotNull(etag);
        Assert.True(etag.IsAny);
    }

    [Fact]
    public void GetETagOfEntity_Returns_ETagAny()
    {
        // Arrange & Act & Assert
        HttpRequest request = new Mock<HttpRequest>().Object;

        // Act
        ETag etag = request.GetETag<HttpRequestODataQueryExtensionsTests>(EntityTagHeaderValue.Any);

        // Assert
        Assert.NotNull(etag);
        Assert.IsType<ETag<HttpRequestODataQueryExtensionsTests>>(etag);
        Assert.True(etag.IsAny);
    }
}
