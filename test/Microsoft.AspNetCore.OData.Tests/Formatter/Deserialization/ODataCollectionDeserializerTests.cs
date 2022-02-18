//-----------------------------------------------------------------------------
// <copyright file="ODataCollectionDeserializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class ODataCollectionDeserializerTests
    {
        private static readonly IEdmModel Model = GetEdmModel();

        // private static readonly ODataSerializerProvider SerializerProvider = ODataSerializerProviderFactory.Create();
        private static readonly IODataSerializerProvider SerializerProvider = ODataFormatterHelpers.GetSerializerProvider(); // TODO:

        // private static readonly ODataDeserializerProvider DeserializerProvider = ODataDeserializerProviderFactory.Create();
        private static readonly IODataDeserializerProvider DeserializerProvider = ODataFormatterHelpers.GetDeserializerProvider(); // TODO:

        private static readonly IEdmEnumTypeReference ColorType =
            new EdmEnumTypeReference(Model.SchemaElements.OfType<IEdmEnumType>().First(c => c.Name == "Color"),
                isNullable: true);

        private static readonly IEdmCollectionTypeReference ColorCollectionType = new EdmCollectionTypeReference(new EdmCollectionType((ColorType)));

        private static readonly IEdmCollectionTypeReference IntCollectionType =
            new EdmCollectionTypeReference(new EdmCollectionType(Model.GetEdmTypeReference(typeof(int))));

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataCollectionDeserializer(deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public async Task Read_ThrowsArgumentNull_MessageReader()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => deserializer.ReadAsync(messageReader: null, type: typeof(int[]), readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void ReadAsync_ThrowsArgumentMustBeOfType_Type()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(() => deserializer.ReadAsync(messageReader: ODataTestUtil.GetMockODataMessageReader(),
                type: typeof(int), readContext: new ODataDeserializerContext { Model = Model }).Wait(),
                "type", "The argument must be of type 'Collection'.");
        }

        [Fact]
        public void ReadInline_ThrowsArgument_ArgumentMustBeOfType()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(
                () => deserializer.ReadInline(42, IntCollectionType, new ODataDeserializerContext()),
                "The argument must be of type 'ODataCollectionValue'. (Parameter 'item')");
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_EdmType()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => deserializer.ReadInline(42, null, new ODataDeserializerContext()),
                "edmType");
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_ReadContext()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadInline(42, IntCollectionType, null), "readContext");
        }

        [Fact]
        public void ReadInline_ThrowsSerializationException_IfNonCollectionType()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(() => deserializer.ReadInline(42, intType, new ODataDeserializerContext()),
                "'[Edm.Int32 Nullable=False]' cannot be deserialized using the OData input formatter.");
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            Assert.Null(deserializer.ReadInline(item: null, edmType: IntCollectionType, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Calls_ReadCollectionValue()
        {
            // Arrange
            Mock<ODataCollectionDeserializer> deserializer = new Mock<ODataCollectionDeserializer>(DeserializerProvider);
            ODataCollectionValue collectionValue = new ODataCollectionValue();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.CallBase = true;
            deserializer.Setup(s => s.ReadCollectionValue(collectionValue, IntCollectionType.ElementType(), readContext)).Verifiable();

            // Act
            deserializer.Object.ReadInline(collectionValue, IntCollectionType, readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ReadInline_ReturnsNull_ReadCollectionValueReturnsNull()
        {
            // Arrange
            Mock<ODataCollectionDeserializer> deserializer = new Mock<ODataCollectionDeserializer>(DeserializerProvider);
            ODataCollectionValue collectionValue = new ODataCollectionValue();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.CallBase = true;
            deserializer.Setup(s => s.ReadCollectionValue(collectionValue, IntCollectionType.ElementType(), readContext)).Returns((IEnumerable)null);

            // Act
            object actual = deserializer.Object.ReadInline(collectionValue, IntCollectionType, readContext);

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void ReadCollectionValue_ThrowsArgumentNull_CollectionValue()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadCollectionValue(collectionValue: null,
                elementType: IntCollectionType.ElementType(), readContext: new ODataDeserializerContext()).GetEnumerator().MoveNext(),
                "collectionValue");
        }

        [Fact]
        public void ReadCollectionValue_ThrowsArgumentNull_ElementType()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => deserializer.ReadCollectionValue(new ODataCollectionValue(), elementType: null,
                    readContext: new ODataDeserializerContext()).GetEnumerator().MoveNext(),
                "elementType");
        }

        [Fact]
        public void ReadCollectionValue_Throws_IfElementTypeCannotBeDeserialized()
        {
            // Arrange
            Mock<IODataDeserializerProvider> deserializerProvider = new Mock<IODataDeserializerProvider>();
            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(ColorType, false)).Returns<ODataResourceDeserializer>(null);
            var deserializer = new ODataCollectionDeserializer(deserializerProvider.Object);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => deserializer.ReadCollectionValue(new ODataCollectionValue() { Items = new[] { 1, 2, 3 }.Cast<object>() },
                    ColorCollectionType.ElementType(), new ODataDeserializerContext())
                    .GetEnumerator()
                    .MoveNext(),
                "'NS.Color' cannot be deserialized using the OData input formatter.");
        }

        [Fact]
        public async Task Read_Roundtrip_PrimitiveCollection()
        {
            // Arrange
            int[] numbers = Enumerable.Range(0, 100).ToArray();

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(SerializerProvider);
            ODataCollectionDeserializer deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, Model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), Model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = Model };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = Model };

            // Act
            await serializer.WriteObjectAsync(numbers, numbers.GetType(), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readnumbers = await deserializer.ReadAsync(messageReader, typeof(int[]), readContext) as IEnumerable;

            // Assert
            Assert.Equal(numbers, readnumbers.Cast<int>());
        }

        [Fact]
        public async Task Read_Roundtrip_EnumCollection()
        {
            // Arrange
            Color[] colors = { Color.Blue, Color.Green };

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(SerializerProvider);
            ODataCollectionDeserializer deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, Model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), Model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = Model };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = Model };

            // Act
            await serializer.WriteObjectAsync(colors, colors.GetType(), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readAddresses = await deserializer.ReadAsync(messageReader, typeof(Color[]), readContext) as IEnumerable;

            // Assert
            Assert.Equal(colors, readAddresses.Cast<Color>());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EnumType<Color>().Namespace = "NS";
            return builder.GetEdmModel();
        }

        public enum Color
        {
            Red,
            Blue,
            Green
        }
    }
}
