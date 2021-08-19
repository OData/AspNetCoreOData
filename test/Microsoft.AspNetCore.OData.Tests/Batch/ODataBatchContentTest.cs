//-----------------------------------------------------------------------------
// <copyright file="ODataBatchContentTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
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
            ODataBatchContent batchContent = CreateBatchContent(Array.Empty<ODataBatchResponseItem>());
            string contentTypeHeader = batchContent.Headers["Content-Type"].FirstOrDefault();
            string mediaType = contentTypeHeader.Substring(0, contentTypeHeader.IndexOf(';'));
            int boundaryParamStart = contentTypeHeader.IndexOf(boundaryHeader);
            string boundary = contentTypeHeader.Substring(boundaryParamStart + boundaryHeader.Length);
            var odataVersion = batchContent.Headers.FirstOrDefault(h => string.Equals(h.Key, ODataVersionConstraint.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

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
        public void NoODataVersionSettingInWriterSetting_SetDefaultVersionInTheHeader()
        {
            // Arrange & Act
            ODataBatchContent batchContent = CreateBatchContent(Array.Empty<ODataBatchResponseItem>());

            var odataVersion = batchContent.Headers
                .FirstOrDefault(h => string.Equals(h.Key, ODataVersionConstraint.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.Equal("4.0", odataVersion.Value.FirstOrDefault());
        }

        [Theory]
        [InlineData(ODataVersion.V4, "4.0")]
        [InlineData(ODataVersion.V401, "4.01")]
        public void ODataVersionInWriterSetting_IsPropagatedToTheHeader(ODataVersion version, string expect)
        {
            // Arrange & Act
            ODataBatchContent batchContent = CreateBatchContent(Array.Empty<ODataBatchResponseItem>(),
                s => s.AddSingleton(new ODataMessageWriterSettings { Version = version }));

            var odataVersion = batchContent.Headers
                .FirstOrDefault(h => string.Equals(h.Key, ODataVersionConstraint.ODataServiceVersionHeader, StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.Equal(expect, odataVersion.Value.FirstOrDefault());
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
            ODataBatchContent batchContent = new ODataBatchContent(new ODataBatchResponseItem[]
                {
                    new OperationResponseItem(okContext),
                    new ChangeSetResponseItem(new HttpContext[]
                    {
                        acceptedContext,
                        badRequestContext
                    })
                },
                new MockServiceProvider());

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
            return CreateBatchContent(responses, s => s.AddSingleton<ODataMessageWriterSettings>());
        }

        private static ODataBatchContent CreateBatchContent(IEnumerable<ODataBatchResponseItem> responses, Action<IServiceCollection> setupConfig)
        {
            IServiceCollection services = new ServiceCollection();
            setupConfig?.Invoke(services);
            IServiceProvider sp = services.BuildServiceProvider();
            return new ODataBatchContent(responses, sp);
        }
    }
}
