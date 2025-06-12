//-----------------------------------------------------------------------------
// <copyright file="HttpContextExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions;

public class HttpContextExtensionsTests
{
    [Fact]
    public void ODataFeature_ThrowsArgumentNull_HttpContext()
    {
        // Arrange & Act & Assert
        HttpContext httpContext = null;
        ExceptionAssert.ThrowsArgumentNull(() => httpContext.ODataFeature(), "httpContext");
    }

    [Fact]
    public void ODataBatchFeature_ThrowsArgumentNull_HttpContext()
    {
        // Arrange & Act & Assert
        HttpContext httpContext = null;
        ExceptionAssert.ThrowsArgumentNull(() => httpContext.ODataBatchFeature(), "httpContext");
    }

    [Fact]
    public void ODataOptions_ThrowsArgumentNull_HttpContext()
    {
        // Arrange & Act & Assert
        HttpContext httpContext = null;
        ExceptionAssert.ThrowsArgumentNull(() => httpContext.ODataOptions(), "httpContext");
    }

    [Fact]
    public void ODataFeature_ReturnsODataFeature()
    {
        // Arrange
        HttpContext httpContext = new DefaultHttpContext();
        IODataFeature odataFeature = new ODataFeature();
        httpContext.Features.Set(odataFeature);

        // Act
        IODataFeature result = httpContext.ODataFeature();

        // Assert
        Assert.Same(odataFeature, result);
    }

    [Fact]
    public void IsMinimalEndpoint_ThrowsArgumentNull_HttpContext()
    {
        // Arrange & Act & Assert
        HttpContext httpContext = null;
        ExceptionAssert.ThrowsArgumentNull(() => httpContext.IsMinimalEndpoint(), "httpContext");
    }

    [Fact]
    public void IsMinimalEndpoint_ReturnsFalse_WhenEndpointIsNull()
    {
        // Arrange
        HttpContext httpContext = new DefaultHttpContext();
        httpContext.SetEndpoint(null);

        // Act
        bool result = httpContext.IsMinimalEndpoint();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsMinimalEndpoint_ReturnsFalse_WhenEndpointIsNotNull_WithoutMetadata()
    {
        // Arrange
        HttpContext httpContext = new DefaultHttpContext();
        Endpoint endpoint = new Endpoint((context) => Task.CompletedTask, EndpointMetadataCollection.Empty, "TestEndpoint");
        httpContext.SetEndpoint(endpoint);

        // Act
        bool result = httpContext.IsMinimalEndpoint();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsMinimalEndpoint_ReturnsTrue_WhenEndpointHasMetadata()
    {
        // Arrange
        HttpContext httpContext = new DefaultHttpContext();
        Endpoint endpoint = new Endpoint(
            (context) => Task.CompletedTask,
            new EndpointMetadataCollection([new ODataMiniMetadata()]),
            "TestEndpoint");

        httpContext.SetEndpoint(endpoint);

        // Act
        bool result = httpContext.IsMinimalEndpoint();

        // Assert
        Assert.True(result);
    }

}
