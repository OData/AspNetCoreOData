//-----------------------------------------------------------------------------
// <copyright file="ODataErrorSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization;

public class ODataErrorSerializerTest
{
    [Fact]
    public void WriteObjectAsync_SupportsHttpError()
    {
        // Arrange
        ODataErrorSerializer serializer = new ODataErrorSerializer();
        SerializableError error = new SerializableError();

        Mock<IODataResponseMessageAsync> mockResponseMessage = new Mock<IODataResponseMessageAsync>();
        mockResponseMessage.Setup(response => response.GetStreamAsync()).ReturnsAsync(new MemoryStream());

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => serializer.WriteObjectAsync(error, typeof(ODataError), new ODataMessageWriter(mockResponseMessage.Object), new ODataSerializerContext())
            .Wait());
    }

    [Fact]
    public async Task WriteObjectAsync_ThrowsArgumentNull_Graph()
    {
        // Arrange
        ODataErrorSerializer serializer = new ODataErrorSerializer();

        // Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => serializer.WriteObjectAsync(graph: null, type: typeof(ODataError), messageWriter: null, writeContext: null),
            "graph");
    }

    [Fact]
    public async Task WriteObjectAsync_ThrowsArgumentNull_MessageWriter()
    {
        // Arrange
        ODataErrorSerializer serializer = new ODataErrorSerializer();

        // Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => serializer.WriteObjectAsync(graph: 42, type: typeof(ODataError), messageWriter: null, writeContext: null),
            "messageWriter");
    }

    [Fact]
    public async Task WriteObject_ThrowsAsync_ErrorTypeMustBeODataErrorOrHttpError()
    {
        // Arrange
        ODataErrorSerializer serializer = new ODataErrorSerializer();

        // Act & Assert
        await ExceptionAssert.ThrowsAsync<SerializationException>(
            () => serializer.WriteObjectAsync(42, typeof(ODataError), ODataTestUtil.GetMockODataMessageWriter(), new ODataSerializerContext()),
            "The type 'System.Int32' is not supported by the ODataErrorSerializer. The type must be ODataError or HttpError.");
    }

    [Fact]
    public async Task ODataErrorSerializer_Works()
    {
        // Arrange
        ODataErrorSerializer serializer = new ODataErrorSerializer();
        MemoryStream stream = new MemoryStream();
        IODataResponseMessageAsync message = new ODataMessageWrapper(stream);
        ODataError error = new ODataError { Message = "Error!!!" };
        ODataMessageWriterSettings settings = new ODataMessageWriterSettings();
        settings.SetContentType(ODataFormat.Json);
        ODataMessageWriter writer = new ODataMessageWriter(message, settings);

        // Act
        await serializer.WriteObjectAsync(error, typeof(ODataError), writer, new ODataSerializerContext());
        stream.Seek(0, SeekOrigin.Begin);
        string result = new StreamReader(stream).ReadToEnd();

        // Assert
        Assert.Equal("{\"error\":{\"code\":\"\",\"message\":\"Error!!!\"}}", result);
    }
}
