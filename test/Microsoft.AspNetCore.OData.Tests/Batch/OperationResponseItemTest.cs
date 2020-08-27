// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class OperationResponseItemTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            // Arrange & Act
            Mock<HttpContext> context = new Mock<HttpContext>();
            OperationResponseItem responseItem = new OperationResponseItem(context.Object);

            // Assert
            Assert.Same(context.Object, responseItem.Context);
        }

        [Fact]
        public void Constructor_NullContext_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new OperationResponseItem(null), "context");
        }

        [Fact]
        public async Task WriteResponseAsync_NullWriter_Throws()
        {
            // Arrange & Act
            Mock<HttpContext> context = new Mock<HttpContext>();
            OperationResponseItem responseItem = new OperationResponseItem(context.Object);

            // Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => responseItem.WriteResponseAsync(null, false), "writer");
        }

        [Fact]
        public async Task WriteResponseAsync_SynchronouslyWritesOperation()
        {
            // Arrange
            HttpContext context = HttpContextHelper.Create(StatusCodes.Status202Accepted);

            OperationResponseItem responseItem = new OperationResponseItem(context);
            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);

            // Act
            ODataBatchWriter batchWriter = writer.CreateODataBatchWriter();
            batchWriter.WriteStartBatch();
            await responseItem.WriteResponseAsync(batchWriter, false);
            batchWriter.WriteEndBatch();
            memoryStream.Position = 0;
            string responseString = new StreamReader(memoryStream).ReadToEnd();

            // Assert
            Assert.Contains("Accepted", responseString);
        }

        [Fact]
        public async Task WriteResponseAsync_AsynchronouslyWritesOperation()
        {
            // Arrange
            HttpContext context = HttpContextHelper.Create(StatusCodes.Status202Accepted);

            OperationResponseItem responseItem = new OperationResponseItem(context);
            MemoryStream memoryStream = new MemoryStream();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(memoryStream);
            ODataMessageWriter writer = new ODataMessageWriter(responseMessage);

            // Act
            ODataBatchWriter batchWriter = await writer.CreateODataBatchWriterAsync();
            await batchWriter.WriteStartBatchAsync();
            await responseItem.WriteResponseAsync(batchWriter, true);

            await batchWriter.WriteEndBatchAsync();
            memoryStream.Position = 0;
            string responseString = new StreamReader(memoryStream).ReadToEnd();

            // Assert
            Assert.Contains("Accepted", responseString);
        }

        [Fact]
        public void IsResponseSucess_TestResponse()
        {
            // Arrange
            OperationResponseItem successResponseItem = new OperationResponseItem(HttpContextHelper.Create(StatusCodes.Status200OK));
            OperationResponseItem errorResponseItem = new OperationResponseItem(HttpContextHelper.Create(StatusCodes.Status300MultipleChoices));

            // Act & Assert
            Assert.True(successResponseItem.IsResponseSuccessful());
            Assert.False(errorResponseItem.IsResponseSuccessful());
        }
    }
}
