//-----------------------------------------------------------------------------
// <copyright file="ODataMetadataSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization;

public class ODataMetadataSerializerTest
{
    [Fact]
    public async Task WriteObjectAsync_ThrowsArgumentNull_MessageWriter()
    {
        // Arrange
        ODataMetadataSerializer serializer = new ODataMetadataSerializer();

        // Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => serializer.WriteObjectAsync(42, typeof(IEdmModel), messageWriter: null, writeContext: null),
            "messageWriter");
    }

    [Fact]
    public async Task ODataMetadataSerializer_Works()
    {
        // Arrange
        ODataMetadataSerializer serializer = new ODataMetadataSerializer();
        IEdmModel model = new EdmModel();

        // 1) XML
        // Act
        string payload = await this.WriteAndGetPayloadAsync(model, "application/xml", async omWriter =>
        {
            await serializer.WriteObjectAsync("42" /*useless*/, typeof(IEdmModel), omWriter, new ODataSerializerContext());
        });

        // Assert
        Assert.Contains("<edmx:Edmx Version=\"4.0\"", payload);

        // 2) JSON
        // Act
        payload = await this.WriteAndGetPayloadAsync(model, "application/json", async omWriter =>
        {
            await serializer.WriteObjectAsync("42" /*useless*/, typeof(IEdmModel), omWriter, new ODataSerializerContext());
        });

        // Assert
        Assert.Equal(@"{
  ""$Version"": ""4.0""
}", payload);
    }

    [Fact]
    public async Task ODataMetadataSerializer_Works_ForSingleton()
    {
        // Arrange
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.Singleton<Customer>("Me");
        IEdmModel model = builder.GetEdmModel();
        ODataMetadataSerializer serializer = new ODataMetadataSerializer();

        // XML
        // Act
        string payload = await this.WriteAndGetPayloadAsync(model, "application/xml", async omWriter =>
        {
            await serializer.WriteObjectAsync(model, typeof(IEdmModel), omWriter, new ODataSerializerContext());
        });

        // Assert
        Assert.Contains("<Singleton Name=\"Me\" Type=\"Microsoft.AspNetCore.OData.Tests.Formatter.Serialization.Customer\" />", payload);

        // JSON
        // Act
        payload = await this.WriteAndGetPayloadAsync(model, "application/json", async omWriter =>
        {
            await serializer.WriteObjectAsync(model, typeof(IEdmModel), omWriter, new ODataSerializerContext());
        });

        // Assert
        Assert.Contains(@"  ""Default"": {
    ""Container"": {
      ""$Kind"": ""EntityContainer"",
      ""Me"": {
        ""$Type"": ""Microsoft.AspNetCore.OData.Tests.Formatter.Serialization.Customer""
      }
    }
  }", payload);
    }

    private async Task<string> WriteAndGetPayloadAsync(IEdmModel edmModel, string contentType, Func<ODataMessageWriter, Task> test)
    {
        MemoryStream stream = new MemoryStream();
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            // the content type is necessary to write the metadata in async?
            { "Content-Type", contentType}
        };

        IODataResponseMessage message = new ODataMessageWrapper(stream, headers);

        ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings();
        writerSettings.EnableMessageStreamDisposal = false;
        writerSettings.BaseUri = new Uri("http://www.example.com/");

        using (var msgWriter = new ODataMessageWriter((IODataResponseMessageAsync)message, writerSettings, edmModel))
        {
            await test(msgWriter);
        }

        stream.Seek(0, SeekOrigin.Begin);
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }

    private class Customer
    { }
}
