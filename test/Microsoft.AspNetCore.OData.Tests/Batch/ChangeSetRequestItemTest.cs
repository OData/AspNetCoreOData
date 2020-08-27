// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ChangeSetRequestItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            // Arrange & Act
            HttpContext[] contexts = Array.Empty<HttpContext>();
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(contexts);

            // Assert
            Assert.Same(contexts, requestItem.Contexts);
        }

        [Fact]
        public void Constructor_NullRequests_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ChangeSetRequestItem(null), "contexts");
        }

        [Fact]
        public async Task SendRequestAsync_NullHandler_Throws()
        {
            // Arrange
            HttpContext[] contexts = Array.Empty<HttpContext>();
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(contexts);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => requestItem.SendRequestAsync(null), "handler");
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsChangeSetResponse()
        {
            // Arrange
            HttpContext[] contexts = new HttpContext[]
            {
                HttpContextHelper.Create("Get", "http://example.com"),
                HttpContextHelper.Create("Post", "http://example.com")
            };
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(contexts);

            RequestDelegate handler = context => Task.FromResult(context.Response);

            // Act
            ODataBatchResponseItem response = await requestItem.SendRequestAsync(handler);

            // Assert
            ChangeSetResponseItem changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            Assert.Equal(2, changesetResponse.Contexts.Count());
        }

        [Fact]
        public async Task SendRequestAsync_ReturnsSingleErrorResponse()
        {
            // Arrange
            HttpContext[] contexts = new HttpContext[]
                {
                    HttpContextHelper.Create("Get", "http://example.com"),
                    HttpContextHelper.Create("Post", "http://example.com"),
                    HttpContextHelper.Create("Put", "http://example.com")
                };
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(contexts);

            RequestDelegate handler = context =>
            {
                if (context.Request.Method == "Post")
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }

                return Task.FromResult(context.Response);
            };

            // Act
            ODataBatchResponseItem response = await requestItem.SendRequestAsync(handler);

            // Assert
            ChangeSetResponseItem changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            HttpContext responseContext = Assert.Single(changesetResponse.Contexts);
            Assert.Equal(StatusCodes.Status400BadRequest, responseContext.Response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_DisposesResponseInCaseOfException()
        {
            // Arrange
            HttpContext[] contexts = new HttpContext[]
                {
                    HttpContextHelper.Create("Get", "http://example.com"),
                    HttpContextHelper.Create("Post", "http://example.com"),
                    HttpContextHelper.Create("Put", "http://example.com")
                };
            ChangeSetRequestItem requestItem = new ChangeSetRequestItem(contexts);

            List<HttpResponse> responses = new List<HttpResponse>();
            RequestDelegate handler = context =>
            {
                if (context.Request.Method == "Put")
                {
                    throw new InvalidOperationException();
                }

                responses.Add(context.Response);
                return Task.FromResult(context.Response);
            };

            // Act
            ODataBatchResponseItem response = await requestItem.SendRequestAsync(handler);

            // Assert
            ChangeSetResponseItem changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            HttpContext responseContext = Assert.Single(changesetResponse.Contexts);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseContext.Response.StatusCode);

            Assert.Equal(2, responses.Count);
        }
    }
}
