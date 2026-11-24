//-----------------------------------------------------------------------------
// <copyright file="ODataMiniBatchMiddlewareTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch;

public class ODataMiniBatchMiddlewareTest
{
    [Fact]
    public void Ctor_ThrowsForNullRoutePattern()
    {
        Assert.Throws<ArgumentNullException>(() => new ODataMiniBatchMiddleware(null, Mock.Of<ODataBatchHandler>(), new ODataMiniMetadata(), _ => Task.CompletedTask));
    }

    [Theory]
    [InlineData("odata/$batch", "/odata/$batch", "odata")]
    [InlineData("/odata/$batch", "/odata/$batch", "odata")]
    [InlineData("odata/$batch/", "/odata/$batch/", "odata")]
    [InlineData("api/v1/$batch", "/api/v1/$batch", "api/v1")]
    [InlineData("{tenant}/$batch", "/contoso/$batch", "contoso")]
    [InlineData("$batch", "/$batch", "")]
    public async Task Invoke_SetsRoutePrefixFromMatchedRequestPath(string routePattern, string requestPath, string expectedRoutePrefix)
    {
        // Arrange
        string capturedRoutePrefix = null;
        Mock<ODataBatchHandler> batchHandler = new Mock<ODataBatchHandler>();
        batchHandler.Setup(handler => handler.ProcessBatchAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
            .Returns<HttpContext, RequestDelegate>((batchContext, _) =>
            {
                capturedRoutePrefix = batchContext.ODataFeature().RoutePrefix;
                return Task.CompletedTask;
            });
        ODataMiniBatchMiddleware middleware = new ODataMiniBatchMiddleware(routePattern, batchHandler.Object, new ODataMiniMetadata(), _ => Task.CompletedTask);
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = requestPath;
        context.ODataFeature().RoutePrefix = "stale";

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.Equal(expectedRoutePrefix, capturedRoutePrefix);
    }

    [Fact]
    public async Task Invoke_DoesNotChangeRoutePrefixForUnmatchedPost()
    {
        // Arrange
        bool nextCalled = false;
        Mock<ODataBatchHandler> batchHandler = new Mock<ODataBatchHandler>();
        batchHandler.Setup(handler => handler.ProcessBatchAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
            .Throws(new InvalidOperationException("The batch handler should not be called."));
        ODataMiniBatchMiddleware middleware = new ODataMiniBatchMiddleware("odata/$batch", batchHandler.Object, new ODataMiniMetadata(), _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/other/$batch";
        context.ODataFeature().RoutePrefix = "existing";

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("existing", context.ODataFeature().RoutePrefix);
    }

    [Fact]
    public async Task Invoke_DoesNotChangeRoutePrefixForNonPostRequest()
    {
        // Arrange
        bool nextCalled = false;
        Mock<ODataBatchHandler> batchHandler = new Mock<ODataBatchHandler>();
        batchHandler.Setup(handler => handler.ProcessBatchAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
            .Throws(new InvalidOperationException("The batch handler should not be called."));
        ODataMiniBatchMiddleware middleware = new ODataMiniBatchMiddleware("odata/$batch", batchHandler.Object, new ODataMiniMetadata(), _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/odata/$batch";
        context.ODataFeature().RoutePrefix = "existing";

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("existing", context.ODataFeature().RoutePrefix);
    }

    [Fact]
    public async Task Invoke_DerivesRoutePrefixForEachRequest()
    {
        // Arrange
        var capturedRoutePrefixes = new List<string>();
        Mock<ODataBatchHandler> batchHandler = new Mock<ODataBatchHandler>();
        batchHandler.Setup(handler => handler.ProcessBatchAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
            .Returns<HttpContext, RequestDelegate>((batchContext, _) =>
            {
                capturedRoutePrefixes.Add(batchContext.ODataFeature().RoutePrefix);
                return Task.CompletedTask;
            });
        ODataMiniBatchMiddleware middleware = new ODataMiniBatchMiddleware("{tenant}/$batch", batchHandler.Object, new ODataMiniMetadata(), _ => Task.CompletedTask);
        DefaultHttpContext contosoContext = new DefaultHttpContext();
        contosoContext.Request.Method = HttpMethods.Post;
        contosoContext.Request.Path = "/contoso/$batch";
        DefaultHttpContext fabrikamContext = new DefaultHttpContext();
        fabrikamContext.Request.Method = HttpMethods.Post;
        fabrikamContext.Request.Path = "/fabrikam/$batch";

        // Act
        await middleware.Invoke(contosoContext);
        await middleware.Invoke(fabrikamContext);

        // Assert
        Assert.Equal(new[] { "contoso", "fabrikam" }, capturedRoutePrefixes);
    }

}
