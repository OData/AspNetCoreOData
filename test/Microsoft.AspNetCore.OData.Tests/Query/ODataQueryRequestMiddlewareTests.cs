// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class ODataQueryRequestMiddlewareTests
    {
        [Fact]
        public void InvokeQueryRequestMiddleware_ThrowsArgumentNull_Context()
        {
            // Arrange
            ODataQueryRequestMiddleware middleware = new ODataQueryRequestMiddleware(null, null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => middleware.Invoke(context: null).Wait(), "context");
        }

        [Fact]
        public async void InvokeQueryRequestMiddleware_Transforms_ODataQueryRequest()
        {
            // Arrange
            RequestDelegate next = c => Task.CompletedTask;
            IODataQueryRequestParser[] parsers = new IODataQueryRequestParser[]
            {
                new DefaultODataQueryRequestParser()
            };

            ODataQueryRequestMiddleware middleware = new ODataQueryRequestMiddleware(parsers, next);

            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            context.Request.Path = new PathString("/$query");
            request.ContentType = "text/plain";
            request.Method = "Post";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("$filter=Id le 5"));

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.Equal("Get", request.Method, ignoreCase: true);
            Assert.Equal("?$filter=Id le 5", request.QueryString.Value);
        }
    }
}

