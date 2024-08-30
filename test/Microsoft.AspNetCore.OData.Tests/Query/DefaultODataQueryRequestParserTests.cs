//-----------------------------------------------------------------------------
// <copyright file="DefaultODataQueryRequestParserTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class DefaultODataQueryRequestParserTests
{
    private const string QueryOptionsString = "$filter=Id le 5";

    [Fact]
    public async Task ParseAsync_WithQueryOptionsInRequestBody()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));

        // Act
        var result = await new DefaultODataQueryRequestParser().ParseAsync(context.Request);

        // Assert
        Assert.Equal(QueryOptionsString, result);
    }

    [Fact]
    public async Task ParseAsync_Throws_WithDisposedStream()
    {
        // Arrange
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(QueryOptionsString));
        HttpContext context = new DefaultHttpContext();
        context.Request.Body = memoryStream;
        memoryStream.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ODataException>(async () => await new DefaultODataQueryRequestParser().ParseAsync(context.Request));
    }

    [Fact]
    public async Task ParseAsync_WithEmptyStream()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream();

        // Act
        var result = await new DefaultODataQueryRequestParser().ParseAsync(context.Request);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void CanParse_MatchesRequest_WithMatchingContentType()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.ContentType = "text/plain";

        // Act & Assert
        Assert.True(new DefaultODataQueryRequestParser().CanParse(request));
    }

    [Fact]
    public void CanParse_DoesNotMatchRequest_WithNonMatchingContentType()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.ContentType = "application/json";

        // Act & Assert
        Assert.False(new DefaultODataQueryRequestParser().CanParse(request));
    }

    [Fact]
    public void CanParse_MatchesRequest_WithNonExactContentType()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.ContentType = "text/plain;charset=utf-8";

        // Act & Assert
        Assert.True(new DefaultODataQueryRequestParser().CanParse(request));
    }
}
