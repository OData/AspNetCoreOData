//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinkSerializerTest.cs" company=".NET Foundation">
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
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization;

public class ODataEntityReferenceLinkSerializerTest
{
    [Fact]
    public async Task WriteObject_ThrowsArgumentNull_MessageWriter()
    {
        // Arrange
        ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();

        // Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => serializer.WriteObjectAsync(graph: null, type: typeof(ODataEntityReferenceLink), messageWriter: null,
                writeContext: new ODataSerializerContext()),
            "messageWriter");
    }

    [Fact]
    public async Task WriteObjectAsync_ThrowsArgumentNull_WriteContext()
    {
        // Arrange
        ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();

        // Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => serializer.WriteObjectAsync(graph: null, type: typeof(ODataEntityReferenceLink),
                messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
            "writeContext");
    }

    [Fact]
    public async Task WriteObjectAsync_Throws_ObjectCannotBeWritten_IfGraphIsNotUri()
    {
        // Arrange
        ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();

        // Act & Assert
        await ExceptionAssert.ThrowsAsync<SerializationException>(
            () => serializer.WriteObjectAsync(graph: "not uri", type: typeof(ODataEntityReferenceLink),
                messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: new ODataSerializerContext()),
            "ODataEntityReferenceLinkSerializer cannot write an object of type 'System.String'.");
    }

    public static TheoryDataSet<object> SerializationTestData
    {
        get
        {
            Uri uri = new Uri("http://sampleuri/");
            return new TheoryDataSet<object>
            {
                uri,
                new ODataEntityReferenceLink { Url = uri }
            };
        }
    }

    [Theory]
    [MemberData(nameof(SerializationTestData))]
    public async Task ODataEntityReferenceLinkSerializer_Serializes_Uri(object link)
    {
        // Arrange
        ODataEntityReferenceLinkSerializer serializer = new ODataEntityReferenceLinkSerializer();
        ODataSerializerContext writeContext = new ODataSerializerContext();
        MemoryStream stream = new MemoryStream();
        IODataResponseMessage message = new ODataMessageWrapper(stream);

        ODataMessageWriterSettings settings = new ODataMessageWriterSettings
        {
            ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
        };

        settings.SetContentType(ODataFormat.Json);
        ODataMessageWriter writer = new ODataMessageWriter(message, settings);

        // Act
        await serializer.WriteObjectAsync(link, typeof(ODataEntityReferenceLink), writer, writeContext);
        stream.Seek(0, SeekOrigin.Begin);
        string result = new StreamReader(stream).ReadToEnd();

        // Assert
        Assert.Equal("{\"@odata.context\":\"http://any/$metadata#$ref\",\"@odata.id\":\"http://sampleuri/\"}", result);
    }
}
