// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class ODataEntityReferenceLinkDeserializerTests
    {
        [Fact]
        public void Ctor_DoesnotThrow()
        {
            // Arrange
            var deserializer = new ODataEntityReferenceLinkDeserializer();

            // Act & Assert
            Assert.Equal(ODataPayloadKind.EntityReferenceLink, deserializer.ODataPayloadKind);
        }

        [Fact]
        public async Task ReadAsync_ThrowsArgumentNull_MessageReader()
        {
            // Arrange
            var deserializer = new ODataEntityReferenceLinkDeserializer();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => deserializer.ReadAsync(messageReader: null, type: null, readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public async Task ReadAsync_ThrowsArgumentNull_ReadContext()
        {
            // Arrange
            var deserializer = new ODataEntityReferenceLinkDeserializer();
            ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => deserializer.ReadAsync(messageReader, type: null, readContext: null),
                "readContext");
        }

        [Fact]
        public async Task ReadAsync_RoundTrips()
        {
            // Arrange
            IEdmModel model = CreateModel();
            var deserializer = new ODataEntityReferenceLinkDeserializer();
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings()
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter messageWriter = new ODataMessageWriter(requestMessage, settings);
            await messageWriter.WriteEntityReferenceLinkAsync(new ODataEntityReferenceLink { Url = new Uri("http://localhost/samplelink") });

            var request = RequestFactory.Create("Get", "http://localhost", opt => opt.AddModel("odata", model));
            request.ODataFeature().PrefixName = "odata";
            ODataMessageReaderSettings readSettings = new ODataMessageReaderSettings();
            ODataMessageReader messageReader = new ODataMessageReader(new MockODataRequestMessage(requestMessage), readSettings, model);
            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Request = request,
                Path = new ODataPath(new NavigationPropertySegment(GetNavigationProperty(model), navigationSource: null))
            };

            // Act
            Uri uri = await deserializer.ReadAsync(messageReader, typeof(Uri), context) as Uri;

            // Assert
            Assert.NotNull(uri);
            Assert.Equal("http://localhost/samplelink", uri.AbsoluteUri);
        }

        [Fact]
        public async Task ReadJsonLightAsync()
        {
            // Arrange
            var deserializer = new ODataEntityReferenceLinkDeserializer();
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings();
            writerSettings.SetContentType(ODataFormat.Json);
            IEdmModel model = CreateModel();
            ODataMessageWriter messageWriter = new ODataMessageWriter(requestMessage, writerSettings, model);
            await messageWriter.WriteEntityReferenceLinkAsync(new ODataEntityReferenceLink { Url = new Uri("http://localhost/samplelink") });
            ODataMessageReader messageReader = new ODataMessageReader(new MockODataRequestMessage(requestMessage),
                new ODataMessageReaderSettings(), model);

            IEdmNavigationProperty navigationProperty = GetNavigationProperty(model);

            var request = RequestFactory.Create("Get", "http://localhost", opt => opt.AddModel("odata", model));
            request.ODataFeature().PrefixName = "odata";
            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Request = request,
                Path = new ODataPath(new NavigationPropertySegment(navigationProperty, navigationSource: null))
            };

            // Act
            Uri uri = await deserializer.ReadAsync(messageReader, typeof(Uri), context) as Uri;

            // Assert
            Assert.NotNull(uri);
            Assert.Equal("http://localhost/samplelink", uri.AbsoluteUri);
        }

        private static IEdmModel CreateModel()
        {
            Mock<ODataModelBuilder> mock = new Mock<ODataModelBuilder>();
            mock.Setup(b => b.ValidateModel(It.IsAny<IEdmModel>())).Callback(() => { });
            mock.CallBase = true;
            ODataModelBuilder builder = mock.Object;
            EntitySetConfiguration<Entity> entities = builder.EntitySet<Entity>("entities");
            builder.EntitySet<RelatedEntity>("related");
            NavigationPropertyConfiguration entityToRelated =
                entities.EntityType.HasOptional<RelatedEntity>((e) => e.Related);
            // entities.HasNavigationPropertyLink(entityToRelated, (a, b) => new Uri("aa:b"), false);
            entities.HasOptionalBinding((e) => e.Related, "related");

            return builder.GetEdmModel();
        }

        private static IEdmNavigationProperty GetNavigationProperty(IEdmModel model)
        {
            return
                model.EntityContainer.EntitySets().First().NavigationPropertyBindings.Single().NavigationProperty;
        }

        private class Entity
        {
            public RelatedEntity Related { get; set; }
        }

        private class RelatedEntity
        {
        }
    }
}