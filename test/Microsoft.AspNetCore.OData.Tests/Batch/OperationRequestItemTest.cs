// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class OperationRequestItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            // Arrange & Act
            Mock<HttpContext> context = new Mock<HttpContext>();
            OperationRequestItem requestItem = new OperationRequestItem(context.Object);

            // Assert
            Assert.Same(context.Object, requestItem.Context);
        }

        [Fact]
        public void Constructor_NullContext_Throws()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new OperationRequestItem(null), "context");
        }

        [Fact]
        public async Task SendRequestAsync_NullHandler_Throws()
        {
            // Arrange
            Mock<HttpContext> context = new Mock<HttpContext>();
            OperationRequestItem requestItem = new OperationRequestItem(context.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => requestItem.SendRequestAsync(null), "handler");
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsOperationResponse()
        {
            // Arrange
            HttpContext context = HttpContextHelper.Create("Get", "http://example.com");
            OperationRequestItem requestItem = new OperationRequestItem(context);

            RequestDelegate handler = context =>
            {
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                return Task.FromResult(context.Response);
            };

            // Act
            ODataBatchResponseItem response = await requestItem.SendRequestAsync(handler);

            // Assert
            OperationResponseItem operationResponse = Assert.IsType<OperationResponseItem>(response);
            Assert.Equal(StatusCodes.Status304NotModified, operationResponse.Context.Response.StatusCode);
        }
    }
}
