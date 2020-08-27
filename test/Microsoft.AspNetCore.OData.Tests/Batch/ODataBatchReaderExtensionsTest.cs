// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ODataBatchReaderExtensionsTest
    {
        [Fact]
        public async Task ReadChangeSetRequest_NullReader_Throws()
        {
            // Arrange
            Mock<HttpContext> mockContext = new Mock<HttpContext>();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadChangeSetRequestAsync(null, mockContext.Object, Guid.NewGuid(), CancellationToken.None),
                "reader");
        }

        [Fact]
        public async Task ReadChangeSetRequest_InvalidState_Throws()
        {
            // Arrange
            StringContent httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            ODataMessageReader reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            Mock<HttpContext> mockContext = new Mock<HttpContext>();

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadChangeSetRequestAsync(reader.CreateODataBatchReader(), mockContext.Object, Guid.NewGuid(), CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'ChangesetStart'.");
        }

        [Fact]
        public async Task ReadOperationRequest_NullReader_Throws()
        {
            // Arrange
            Mock<HttpContext> mockContext = new Mock<HttpContext>();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadOperationRequestAsync(null, mockContext.Object, Guid.NewGuid(), false, CancellationToken.None),
                "reader");
        }

        [Fact]
        public async Task ReadOperationRequest_InvalidState_Throws()
        {
            // Arrange
            StringContent httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");

            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            ODataMessageReader reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            Mock<HttpContext> mockContext = new Mock<HttpContext>();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadOperationRequestAsync(reader.CreateODataBatchReader(), mockContext.Object, Guid.NewGuid(), false, CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'Operation'.");
        }

        [Fact]
        public async Task ReadChangeSetOperationRequest_NullReader_Throws()
        {
            // Arrange
            Mock<HttpContext> mockContext = new Mock<HttpContext>();

            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchReaderExtensions.ReadChangeSetOperationRequestAsync(null, mockContext.Object, Guid.NewGuid(), Guid.NewGuid(), false, CancellationToken.None),
                "reader");
        }

        [Fact]
        public async Task ReadChangeSetOperationRequest_InvalidState_Throws()
        {
            // Arrange
            StringContent httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            ODataMessageReader reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            Mock<HttpContext> mockContext = new Mock<HttpContext>();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => ODataBatchReaderExtensions.ReadChangeSetOperationRequestAsync(reader.CreateODataBatchReader(), mockContext.Object,
                    Guid.NewGuid(), Guid.NewGuid(), false, CancellationToken.None),
                "The current batch reader state 'Initial' is invalid. The expected state is 'Operation'.");
        }
    }
}
