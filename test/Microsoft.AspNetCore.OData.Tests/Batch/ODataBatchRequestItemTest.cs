// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ODataBatchRequestItemTest
    {
        [Fact]
        public async Task SendRequestAsync_Throws_WhenInvokerIsNull()
        {
            // Arrange
            Mock<HttpContext> httpContext = new Mock<HttpContext>();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchRequestItem.SendRequestAsync(null, httpContext.Object, null),
                "handler");
        }

        [Fact]
        public async Task SendRequestAsync_Throws_WhenRequestIsNull()
        {
            // Arrange
            Mock<RequestDelegate> handler = new Mock<RequestDelegate>();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchRequestItem.SendRequestAsync(handler.Object, null, null),
                "context");
        }

        [Fact]
        public async Task SendRequestAsync_CallsHandler()
        {
            // Arrange
            HttpContext context = HttpContextHelper.Create("Get", "http://example.com");

            RequestDelegate handler = context =>
            {
                context.Response.StatusCode = StatusCodes.Status201Created;
                return Task.FromResult(context.Response);
            };

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, new Dictionary<string, string>());

            // Assert
            Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        }
    }
}
