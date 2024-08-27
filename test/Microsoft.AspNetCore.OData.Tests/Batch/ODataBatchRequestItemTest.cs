//-----------------------------------------------------------------------------
// <copyright file="ODataBatchRequestItemTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
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
                return Task.CompletedTask;
            };

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, new Dictionary<string, string>());

            // Assert
            Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        }

        [Fact]
        public async Task SendMessageAsync_Resolves_Uri_From_ContentId()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            HttpResponseMessage response = new HttpResponseMessage();
            RequestDelegate handler = (c) => Task.CompletedTask;
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>();
            contentIdLocationMappings.Add("1", "http://localhost:12345/odata/Customers(42)");
            Uri unresolvedUri = new Uri("http://localhost:12345/odata/$1/Orders");
            context.Request.CopyAbsoluteUrl(unresolvedUri);

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.Equal("/odata/Customers(42)/Orders", context.Request.Path.ToString());
        }
    }
}
