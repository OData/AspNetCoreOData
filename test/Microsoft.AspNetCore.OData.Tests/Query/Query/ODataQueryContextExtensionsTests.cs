//-----------------------------------------------------------------------------
// <copyright file="ODataQueryOptionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Query;

public class ODataQueryContextExtensionsTests
{
    [Fact]
    public void GetODataQuerySettings_WithContext_SetsTimeZone()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddLogging().AddControllers().AddOData();
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        Mock<HttpRequest> mockHttpRequest = new Mock<HttpRequest>();
        Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.SetupGet(x => x.RequestServices).Returns(serviceProvider);
        mockHttpRequest.Setup(x => x.HttpContext)
            .Returns(mockHttpContext.Object);

        ODataQueryContext context = new ODataQueryContext
        {
            Request = mockHttpRequest.Object,
        };

        // Act
        ODataQuerySettings querySettings = context.GetODataQuerySettings();

        // Assert
        Assert.NotNull(querySettings.TimeZone);
    }
}
