// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ODataBatchResponseItemTest
    {
        [Fact]
        public async Task WriteMessageAsync_NullWriter_Throws()
        {
            // Arrange
            HttpContext context = new Mock<HttpContext>().Object;
            
            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => ODataBatchResponseItem.WriteMessageAsync(null, context), "writer");
        }

        [Fact]
        public async Task WriteMessageAsync_NullContext_Throws()
        {
            // Arrange
            HeaderDictionary headers = new HeaderDictionary
            {
                { "Content-Type", $"multipart/mixed;charset=utf-8;boundary={Guid.NewGuid()}" },
            };
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(new MemoryStream(), headers);
            ODataMessageWriter messageWriter = new ODataMessageWriter(odataResponse);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchResponseItem.WriteMessageAsync(messageWriter.CreateODataBatchWriter(), null), "context");
        }

        [Fact]
        public void WriteMessage_SynchronouslyWritesResponseMessage_Throws()
        {
            HeaderDictionary headers = new HeaderDictionary
            {
                { "Content-Type", $"multipart/mixed;charset=utf-8;boundary={Guid.NewGuid()}" },
            };

            MemoryStream ms = new MemoryStream();
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(ms, headers);

            HeaderDictionary responseHeaders = new HeaderDictionary
            {
                { "customHeader", "bar" }
            };
            HttpResponse response = CreateResponse("example content", responseHeaders, "text/example");

            // Act
            ODataBatchWriter batchWriter = new ODataMessageWriter(odataResponse).CreateODataBatchWriter();
            batchWriter.WriteStartBatch();

            // Assert
            Action test = () => ODataBatchResponseItem.WriteMessageAsync(batchWriter, response.HttpContext).Wait();
            ODataException exception = ExceptionAssert.Throws<ODataException>(test);
            Assert.Equal("An asynchronous operation was called on a synchronous batch writer. Calls on a batch writer instance must be either all synchronous or all asynchronous.",
                exception.Message);
        }

        [Fact]
        public async Task WriteMessageAsync_AsynchronouslyWritesResponseMessage()
        {
            // Arrange
            HeaderDictionary headers = new HeaderDictionary
            {
                { "Content-Type", $"multipart/mixed;charset=utf-8;boundary={Guid.NewGuid()}" },
            };
            MemoryStream ms = new MemoryStream();
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(ms, headers);

            HeaderDictionary responseHeaders = new HeaderDictionary
            {
                { "customHeader", "bar" }
            };
            HttpResponse response = CreateResponse("example content", responseHeaders, "text/example");

            // Act
            ODataBatchWriter batchWriter = await new ODataMessageWriter(odataResponse).CreateODataBatchWriterAsync();
            await batchWriter.WriteStartBatchAsync();
            await ODataBatchResponseItem.WriteMessageAsync(batchWriter, response.HttpContext);
            await batchWriter.WriteEndBatchAsync();

            ms.Position = 0;
            string result = new StreamReader(ms).ReadToEnd();

            // Assert
            Assert.Contains("example content", result);
            Assert.Contains("text/example", result);
            Assert.Contains("customHeader", result);
            Assert.Contains("bar", result);
        }

        [Fact]
        public void WriteMessageAsync_SynchronousResponseContainsContentId_IfHasContentIdInRequestChangeSet()
        {
            // Arrange
            HeaderDictionary headers = new HeaderDictionary
            {
                { "Content-Type", $"multipart/mixed;charset=utf-8;boundary={Guid.NewGuid()}" },
            };

            MemoryStream ms = new MemoryStream();
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(ms, headers);

            string contentId = Guid.NewGuid().ToString();
            HttpResponse httpResponse = CreateResponse("any", new HeaderDictionary(), "text/example;charset=utf-8");
            httpResponse.HttpContext.Request.SetODataContentId(contentId);

            // Act
            ODataBatchWriter batchWriter = new ODataMessageWriter(odataResponse).CreateODataBatchWriter();
            batchWriter.WriteStartBatch();
            batchWriter.WriteStartChangeset();

            // Assert
            Action test = () => ODataBatchResponseItem.WriteMessageAsync(batchWriter, httpResponse.HttpContext).Wait();
            ODataException exception = ExceptionAssert.Throws<ODataException>(test);
            Assert.Equal("An asynchronous operation was called on a synchronous batch writer. Calls on a batch writer instance must be either all synchronous or all asynchronous.",
                exception.Message);
        }

        [Fact]
        public async Task WriteMessageAsync_ResponseContainsContentId_IfHasContentIdInRequestChangeSet()
        {
            // Arrange
            HeaderDictionary headers = new HeaderDictionary
            {
                { "Content-Type", $"multipart/mixed;charset=utf-8;boundary={Guid.NewGuid()}" },
            };

            MemoryStream ms = new MemoryStream();
            IODataResponseMessage odataResponse = ODataMessageWrapperHelper.Create(ms, headers);

            string contentId = Guid.NewGuid().ToString();
            HttpResponse httpResponse = CreateResponse("any", new HeaderDictionary(), "text/example;charset=utf-8");
            httpResponse.HttpContext.Request.SetODataContentId(contentId);

            // Act
            ODataBatchWriter batchWriter = await new ODataMessageWriter(odataResponse).CreateODataBatchWriterAsync();
            await batchWriter.WriteStartBatchAsync();
            await batchWriter.WriteStartChangesetAsync();
            await ODataBatchResponseItem.WriteMessageAsync(batchWriter, httpResponse.HttpContext);
            await batchWriter.WriteEndChangesetAsync();
            await batchWriter.WriteEndBatchAsync();

            ms.Position = 0;
            string result = new StreamReader(ms).ReadToEnd();

            // Assert
            Assert.Contains("any", result);
            Assert.Contains("text/example", result);
            Assert.Contains("Content-ID", result);
            Assert.Contains(contentId, result);
        }

        private static HttpResponse CreateResponse(string body, IHeaderDictionary headers, string contextType)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Features.Get<IHttpResponseFeature>().Headers = headers;
            httpContext.Response.ContentType = contextType;
            byte[] contentBytes = Encoding.UTF8.GetBytes(body);
            httpContext.Response.Body = new MemoryStream(contentBytes);
            return httpContext.Response;
        }
    }
}
