// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataApplicationBuilderExtensionsTests
    {
        [Fact]
        public void UseODataBatching_ThrowsArgumentNull_AppBuilder()
        {
            // Arrange & Act & Assert
            IApplicationBuilder builder = null;
            ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataBatching(), "app");
        }

        [Fact]
        public void UseODataQueryRequest_ThrowsArgumentNull_AppBuilder()
        {
            // Arrange & Act & Assert
            IApplicationBuilder builder = null;
            ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataQueryRequest(), "app");
        }

        [Fact]
        public void UseODataRouteDebug_ThrowsArgumentNull_AppBuilder()
        {
            // Arrange & Act & Assert
            IApplicationBuilder builder = null;
            ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataRouteDebug(), "app");
        }

        [Fact]
        public void UseODataRouteDebug_UsingPattern_ThrowsArgumentNull_AppBuilder()
        {
            // Arrange & Act & Assert
            IApplicationBuilder builder = null;
            ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataRouteDebug("$odata"), "app");
        }

        [Fact]
        public void UseODataRouteDebug_UsingPattern_ThrowsArgumentNull_RoutePattern()
        {
            // Arrange & Act & Assert
            IApplicationBuilder builder = new Mock<IApplicationBuilder>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataRouteDebug(null), "routePattern");
        }
    }
}
