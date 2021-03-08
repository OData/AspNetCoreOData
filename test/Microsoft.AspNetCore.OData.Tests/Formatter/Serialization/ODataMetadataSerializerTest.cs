// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
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
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings()
            {
                EnableMessageStreamDisposal = false
            };
            IEdmModel model = new EdmModel();

            // Act
            using (ODataMessageWriter msgWriter = new ODataMessageWriter(message, settings, model))
            {
                await serializer.WriteObjectAsync("42", typeof(IEdmModel),msgWriter, new ODataSerializerContext());
            }

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal("Edmx", element.Name.LocalName);
        }

        [Fact]
        public async Task ODataMetadataSerializer_Works_ForSingleton()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Singleton<Customer>("Me");
            IEdmModel model = builder.GetEdmModel();

            ODataMetadataSerializer serializer = new ODataMetadataSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings()
            {
                EnableMessageStreamDisposal = false
            };

            // Act
            using (ODataMessageWriter msgWriter = new ODataMessageWriter(message, settings, model))
            {
                await serializer.WriteObjectAsync(model, typeof(IEdmModel), msgWriter, new ODataSerializerContext());
            }

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();
            Assert.Contains("<Singleton Name=\"Me\" Type=\"Microsoft.AspNetCore.OData.Tests.Formatter.Serialization.Customer\" />", result);
        }

        private class Customer
        { }
    }
}
