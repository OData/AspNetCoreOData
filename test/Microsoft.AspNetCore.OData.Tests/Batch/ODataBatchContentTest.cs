// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Common;
using Xunit;
using Microsoft.AspNetCore.OData.TestCommon;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ODataBatchContentTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            // Arrange & Act
            const string boundaryHeader = "boundary";
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[0]);
            string contentTypeHeader = batchContent.Headers["Content-Type"].FirstOrDefault();
            string mediaType = contentTypeHeader.Substring(0, contentTypeHeader.IndexOf(';'));
            int boundaryParamStart = contentTypeHeader.IndexOf(boundaryHeader);
            string boundary = contentTypeHeader.Substring(boundaryParamStart + boundaryHeader.Length);
            var odataVersion = batchContent.Headers.FirstOrDefault(h => String.Equals(h.Key, ODataVersionConstraint.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.NotEmpty(boundary);
            Assert.NotEmpty(odataVersion.Value);
            Assert.Equal("multipart/mixed", mediaType);
        }

        [Fact]
        public void Constructor_Throws_WhenResponsesAreNull()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => CreateBatchContent(null), "responses");
        }

        [Fact]
        public void ODataVersionInWriterSetting_IsPropagatedToTheHeader()
        {
            // Arrange & Act
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[0]);
            var odataVersion = batchContent.Headers
                .FirstOrDefault(h => string.Equals(h.Key, ODataVersionConstraint.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.Equal("4.0", odataVersion.Value.FirstOrDefault());
        }

        [Fact]
        public async Task SerializeToStreamAsync_WritesODataBatchResponseItems()
        {
            // Arrange
            HttpContext okContext = new DefaultHttpContext();
            okContext.Response.StatusCode = StatusCodes.Status200OK;

            HttpContext acceptedContext = new DefaultHttpContext();
            acceptedContext.Response.StatusCode = StatusCodes.Status202Accepted;

            HttpContext badRequestContext = new DefaultHttpContext();
            badRequestContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            // Act
            ODataBatchContent batchContent = CreateBatchContent(new ODataBatchResponseItem[]
            {
                new OperationResponseItem(okContext),
                new ChangeSetResponseItem(new HttpContext[]
                {
                    acceptedContext,
                    badRequestContext
                })
            });

            MemoryStream stream = new MemoryStream();
            await batchContent.SerializeToStreamAsync(stream);
            stream.Position = 0;
            string responseString = await new StreamReader(stream).ReadToEndAsync();

            // Assert
            Assert.Contains("changesetresponse", responseString);
            Assert.Contains("OK", responseString);
            Assert.Contains("Accepted", responseString);
            Assert.Contains("Bad Request", responseString);
        }

        private static ODataBatchContent CreateBatchContent(IEnumerable<ODataBatchResponseItem> responses)
        {
            return new ODataBatchContent(responses, new MockServiceProvider());
        }
    }
}
