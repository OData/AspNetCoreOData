//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinksSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataEntityReferenceLinksSerializerTest
    {
        [Fact]
        public async Task WriteObjectAsync_ThrowsArgumentNull_MessageWriter()
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(graph: null, type: typeof(ODataEntityReferenceLinks), messageWriter: null,
                    writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public async Task WriteObjectAsync_ThrowsArgumentNull_WriteContext()
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(graph: null, type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public async Task WriteObjectAsync_Throws_ObjectCannotBeWritten_IfGraphIsNotUri()
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteObjectAsync(graph: "not uri", type: typeof(ODataEntityReferenceLinks),
                    messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: new ODataSerializerContext()),
                "ODataEntityReferenceLinksSerializer cannot write an object of type 'System.String'.");
        }

        public static TheoryDataSet<object> SerializationTestData
        {
            get
            {
                Uri uri1 = new Uri("http://uri1");
                Uri uri2 = new Uri("http://uri2");
                return new TheoryDataSet<object>
                {
                    new Uri[] { uri1, uri2 },

                    new ODataEntityReferenceLinks
                    {
                        Links = new ODataEntityReferenceLink[]
                        {
                            new ODataEntityReferenceLink{ Url = uri1 },
                            new ODataEntityReferenceLink{ Url = uri2 }
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(SerializationTestData))]
        public async Task ODataEntityReferenceLinkSerializer_Serializes_UrisAndEntityReferenceLinks(object uris)
        {
            // Arrange
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
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
            await serializer.WriteObjectAsync(uris, typeof(ODataEntityReferenceLinks), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            string result = await new StreamReader(stream).ReadToEndAsync();

            // Assert
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata#Collection($ref)\"," +
                "\"value\":[{\"@odata.id\":\"http://uri1/\"},{\"@odata.id\":\"http://uri2/\"}]}",
                result);
        }

        public static TheoryDataSet<object> SerializationTestData2
        {
            get
            {
                Uri uri1 = new Uri("http://uri1");
                return new TheoryDataSet<object>
                {
                    new Uri[] {uri1}
                };
            }
        }

        [Theory]
        [MemberData(nameof(SerializationTestData2))]
        public async Task ODataEntityReferenceLinkSerializer_Serializes_UrisAndEntityReferenceLinks_WithCount(object uris)
        {
            // Arrange
            //var config = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(/*config, "OData"*/);
            ODataEntityReferenceLinksSerializer serializer = new ODataEntityReferenceLinksSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext();
            writeContext.Request = request;
            writeContext.Request.ODataFeature().TotalCount = 1;

            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };

            settings.SetContentType(ODataFormat.Json);
            ODataMessageWriter writer = new ODataMessageWriter(message, settings);

            // Act
            await serializer.WriteObjectAsync(uris, typeof(ODataEntityReferenceLinks), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            string result = await new StreamReader(stream).ReadToEndAsync();
            Assert.Equal(
                string.Format("{0},{1},{2}",
                    "{\"@odata.context\":\"http://any/$metadata#Collection($ref)\"",
                    "\"@odata.count\":1",
                    "\"value\":[{\"@odata.id\":\"http://uri1/\"}]}"), result);
        }
    }
}
