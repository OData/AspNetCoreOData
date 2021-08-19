//-----------------------------------------------------------------------------
// <copyright file="ODataServiceDocumentSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataServiceDocumentSerializerTests
    {
        private Type _workspaceType = typeof(ODataServiceDocument);

        [Fact]
        public async Task WriteObjectAsync_ThrowsArgumentNull_MessageWriter()
        {
            // Arrange
            ODataServiceDocumentSerializer serializer = new ODataServiceDocumentSerializer();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(42, _workspaceType, messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public async Task WriteObjectAsync_ThrowsArgumentNull_Graph()
        {
            // Arrange
            ODataServiceDocumentSerializer serializer = new ODataServiceDocumentSerializer();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(null, type: _workspaceType, messageWriter: null, writeContext: null),
                "graph");
        }

        [Fact]
        public async Task WriteObjectAsync_Throws_CannotWriteType()
        {
            // Arrange
            ODataServiceDocumentSerializer serializer = new ODataServiceDocumentSerializer();

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteObjectAsync(42, _workspaceType, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "ODataServiceDocumentSerializer cannot write an object of type 'ODataServiceDocument'.");
        }

        [Fact]
        public async Task ODataServiceDocumentSerializer_Works()
        {
            // Arrange
            ODataServiceDocumentSerializer serializer = new ODataServiceDocumentSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter writer = new ODataMessageWriter(message, settings);

            // Act
            await serializer.WriteObjectAsync(new ODataServiceDocument(), _workspaceType, writer, new ODataSerializerContext());
            await stream.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata\",\"value\":[]}", result);
        }
    }
}
