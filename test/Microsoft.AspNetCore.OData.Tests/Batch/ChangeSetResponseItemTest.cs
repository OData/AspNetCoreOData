// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ChangeSetResponseItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            // Arrange & Act
            HttpContext[] contexts = Array.Empty<HttpContext>();
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(contexts);

            // Assert
            Assert.Same(contexts, responseItem.Contexts);
        }

        [Fact]
        public void Constructor_NullContexts_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ChangeSetResponseItem(null), "contexts");
        }

        [Fact]
        public async Task WriteResponseAsync_NullWriter_Throws()
        {
            // Arrange & Act
            HttpContext[] contexts = Array.Empty<HttpContext>();
            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(contexts);

            await ExceptionAssert.ThrowsArgumentNullAsync(() => responseItem.WriteResponseAsync(null), "writer");
        }

        [Fact]
        public void WriteResponse_SynchronouslyWritesChangeSet_Throws()
        {
            // Arrange
            HttpContext context1 = HttpContextHelper.Create(StatusCodes.Status202Accepted);
            HttpContext context2 = HttpContextHelper.Create(StatusCodes.Status204NoContent);

            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(new[] { context1, context2 });
            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);

            // Act
            ODataBatchWriter batchWriter = writer.CreateODataBatchWriter();
            batchWriter.WriteStartBatch();

            // Assert
            Action test = () => responseItem.WriteResponseAsync(batchWriter).Wait();
            ODataException exception = ExceptionAssert.Throws<ODataException>(test);
            Assert.Equal("An asynchronous operation was called on a synchronous batch writer. Calls on a batch writer instance must be either all synchronous or all asynchronous.",
                exception.Message);
        }

        [Fact]
        public async Task WriteResponseAsync_WritesChangeSet()
        {
            // Arrange
            HttpContext context1 = HttpContextHelper.Create(StatusCodes.Status202Accepted);
            HttpContext context2 = HttpContextHelper.Create(StatusCodes.Status204NoContent);

            ChangeSetResponseItem responseItem = new ChangeSetResponseItem(new[] { context1, context2 });
            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);

            // Act
            ODataBatchWriter batchWriter = await writer.CreateODataBatchWriterAsync();
            await batchWriter.WriteStartBatchAsync();
            await responseItem.WriteResponseAsync(batchWriter);
            await batchWriter.WriteEndBatchAsync();

            memoryStream.Position = 0;
            string responseString = new StreamReader(memoryStream).ReadToEnd();

            // Assert
            Assert.Contains("changesetresponse", responseString);
            Assert.Contains("Accepted", responseString);
            Assert.Contains("No Content", responseString);
        }

        [Fact]
        public void IsResponseSuccessful_TestResponse()
        {
            // Arrange
            HttpContext[] successResponses = new HttpContext[]
            {
                HttpContextHelper.Create(StatusCodes.Status202Accepted),
                HttpContextHelper.Create(StatusCodes.Status201Created),
                HttpContextHelper.Create(StatusCodes.Status200OK)
            };

            HttpContext[] errorResponses = new HttpContext[]
            {
                HttpContextHelper.Create(StatusCodes.Status201Created),
                HttpContextHelper.Create(StatusCodes.Status502BadGateway),
                HttpContextHelper.Create(StatusCodes.Status300MultipleChoices)
            };

            ChangeSetResponseItem successResponseItem = new ChangeSetResponseItem(successResponses);
            ChangeSetResponseItem errorResponseItem = new ChangeSetResponseItem(errorResponses);

            // Act & Assert
            Assert.True(successResponseItem.IsResponseSuccessful());
            Assert.False(errorResponseItem.IsResponseSuccessful());
        }
    }
}
