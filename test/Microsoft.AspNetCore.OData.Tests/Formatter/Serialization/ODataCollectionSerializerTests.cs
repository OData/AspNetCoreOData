// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataCollectionSerializerTests
    {
        private IEdmPrimitiveTypeReference _edmIntType;
        private IEdmCollectionTypeReference _collectionType;

        public ODataCollectionSerializerTests()
        {
            _edmIntType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);
            _collectionType = new EdmCollectionTypeReference(new EdmCollectionType(_edmIntType));
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataCollectionSerializer(serializerProvider: null), "serializerProvider");
        }

        [Fact]
        public async Task WriteObjectAsync_Throws_ArgumentNull_MessageWriter()
        {
            // Arrange
            ODataSerializerProvider provider = new Mock<ODataSerializerProvider>().Object;
            ODataCollectionSerializer serializer = new ODataCollectionSerializer(provider);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(graph: null, type: typeof(int[]), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public async Task WriteObjectAsync_Throws_ArgumentNull_WriteContext()
        {
            // Arrange
            ODataMessageWriter messageWriter = ODataTestUtil.GetMockODataMessageWriter();
            ODataSerializerProvider provider = new Mock<ODataSerializerProvider>().Object;
            ODataCollectionSerializer serializer = new ODataCollectionSerializer(provider);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(graph: null, type: typeof(int[]), messageWriter, writeContext: null),
                "writeContext");
        }

        [Fact]
        public async Task WriteObjectAsync_WritesValueReturnedFrom_CreateODataCollectionValue()
        {
            // Arrange
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings()
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };

            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter messageWriter = new ODataMessageWriter(message, settings);
            ODataSerializerProvider provider = new Mock<ODataSerializerProvider>().Object;
            Mock<ODataCollectionSerializer> serializer = new Mock<ODataCollectionSerializer>(provider);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "CollectionName", Model = EdmCoreModel.Instance };
            IEnumerable enumerable = new int[0];
            ODataCollectionValue collectionValue = new ODataCollectionValue { TypeName = "NS.Name", Items = new object[] { 0, 1, 2 } };

            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateODataCollectionValue(enumerable, It.Is<IEdmTypeReference>(e => e.Definition == this._edmIntType.Definition), writeContext))
                .Returns(collectionValue).Verifiable();

            // Act
            await serializer.Object.WriteObjectAsync(enumerable, typeof(int[]), messageWriter, writeContext);

            // Assert
            serializer.Verify();
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata#Collection(Edm.Int32)\",\"value\":[0,1,2]}", result);
        }

        [Fact]
        public void CreateODataCollectionValue_ThrowsArgumentNull_WriteContext()
        {
            // Arrange
            ODataSerializerProvider provider = new Mock<ODataSerializerProvider>().Object;
            ODataCollectionSerializer serializer = new ODataCollectionSerializer(provider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.CreateODataCollectionValue(Enumerable.Empty<object>(), this._edmIntType, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void CreateODataValue_ThrowsArgument_IfGraphIsNotEnumerable()
        {
            // Arrange
            object nonEnumerable = new object();
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var serializer = new ODataCollectionSerializer(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns<IEdmTypeReference>(null);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => serializer.CreateODataValue(graph: nonEnumerable, expectedType: null, writeContext: new ODataSerializerContext()),
                "graph",
                "The argument must be of type 'IEnumerable'.");
        }

        [Fact]
        public void CreateODataCollectionValue_ThrowsSerializationException_TypeCannotBeSerialized()
        {
            // Arrange
            IEnumerable enumerable = new[] { 0 };
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var serializer = new ODataCollectionSerializer(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns<IEdmTypeReference>(null);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.CreateODataCollectionValue(enumerable, this._edmIntType, new ODataSerializerContext { Model = EdmCoreModel.Instance }),
                "'Edm.Int32' cannot be serialized using the OData output formatter.");
        }

        [Fact]
        public void CreateODataValue_Calls_CreateODataCollectionValue()
        {
            // Arrange
            ODataCollectionValue oDataCollectionValue = new ODataCollectionValue();
            var collection = new object[0];
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataCollectionSerializer> serializer = new Mock<ODataCollectionSerializer>(serializerProvider.Object);
            ODataSerializerContext writeContext = new ODataSerializerContext();
            serializer.CallBase = true;
            serializer
                .Setup(s => s.CreateODataCollectionValue(collection, _edmIntType, writeContext))
                .Returns(oDataCollectionValue)
                .Verifiable();

            // Act
            ODataValue value = serializer.Object.CreateODataValue(collection, _collectionType, writeContext);

            // Assert
            serializer.Verify();
            Assert.Same(oDataCollectionValue, value);
        }

        [Fact]
        public void CreateODataCollectionValue_Serializes_AllElementsInTheCollection()
        {
            // Arrange
            ODataPrimitiveSerializer primitiveSerializer = new ODataPrimitiveSerializer();
            ODataSerializerContext writeContext = new ODataSerializerContext { Model = EdmCoreModel.Instance };
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(primitiveSerializer);

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(serializerProvider.Object);

            // Act
            var oDataValue = serializer.CreateODataCollectionValue(new int[] { 1, 2, 3 }, _edmIntType, writeContext);

            // Assert
            var values = Assert.IsType<ODataCollectionValue>(oDataValue);

            List<int> elements = new List<int>();
            foreach (var item in values.Items)
            {
                elements.Add(Assert.IsType<int>(item));
            }

            Assert.Equal(elements, new int[] { 1, 2, 3 });
        }

        [Fact]
        public void CreateODataCollectionValue_CanSerialize_IEdmObjects()
        {
            // Arrange
            Mock<IEdmEnumObject> edmEnumObject = new Mock<IEdmEnumObject>();
            IEdmEnumObject[] collection = { edmEnumObject.Object };
            ODataSerializerContext serializerContext = new ODataSerializerContext();
            IEdmEnumTypeReference elementType = new EdmEnumTypeReference(new EdmEnumType("NS", "EnumType"), isNullable: true);
            edmEnumObject.Setup(s => s.GetEdmType()).Returns(elementType);

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataEnumSerializer> elementSerializer = new Mock<ODataEnumSerializer>(MockBehavior.Strict, serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(elementType)).Returns(elementSerializer.Object);
            elementSerializer.Setup(s => s.CreateODataEnumValue(collection[0], elementType, serializerContext)).Returns(new ODataEnumValue("1", "NS.EnumType")).Verifiable();

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(serializerProvider.Object);

            // Act
            serializer.CreateODataCollectionValue(collection, elementType, serializerContext);

            // Assert
            elementSerializer.Verify();
        }

        [Fact]
        public void CreateODataCollectionValue_Returns_EmptyODataCollectionValue_ForNull()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataCollectionSerializer serializer = new ODataCollectionSerializer(serializerProvider.Object);

            // Act
            var oDataValue = serializer.CreateODataCollectionValue(null, _edmIntType, new ODataSerializerContext());

            // Assert
            Assert.NotNull(oDataValue);
            ODataCollectionValue collection = Assert.IsType<ODataCollectionValue>(oDataValue);
            Assert.Empty(collection.Items);
        }

        [Fact]
        public void CreateODataCollectionValue_SetsTypeName()
        {
            // Arrange
            IEnumerable enumerable = new int[] { 1, 2, 3 };
            ODataSerializerContext context = new ODataSerializerContext { Model = EdmCoreModel.Instance };
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(new ODataPrimitiveSerializer());
            ODataCollectionSerializer serializer = new ODataCollectionSerializer(serializerProvider.Object);

            // Act
            ODataValue oDataValue = serializer.CreateODataCollectionValue(enumerable, _edmIntType, context);

            // Assert
            ODataCollectionValue collection = Assert.IsType<ODataCollectionValue>(oDataValue);
            Assert.Equal("Collection(Edm.Int32)", collection.TypeName);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation_InDefaultMetadataMode()
        {
            // Arrange
            ODataCollectionValue value = new ODataCollectionValue();

            // Act
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.Minimal);

            // Assert
            Assert.Null(value.TypeAnnotation);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataCollectionValue value = new ODataCollectionValue
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.Full);

            // Assert
            ODataTypeAnnotation annotation = value.TypeAnnotation;
            Assert.NotNull(annotation); // Guard
            Assert.Equal(expectedTypeName, annotation.TypeName);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotationWithNull_InJsonLightNoMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataCollectionValue value = new ODataCollectionValue
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataCollectionSerializer.AddTypeNameAnnotationAsNeeded(value, ODataMetadataLevel.None);

            // Assert
            ODataTypeAnnotation annotation = value.TypeAnnotation;
            Assert.NotNull(annotation); // Guard
            Assert.Null(annotation.TypeName);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Full, true)]
        [InlineData(ODataMetadataLevel.Minimal, false)]
        [InlineData(ODataMetadataLevel.None, true)]
        public void ShouldAddTypeNameAnnotation(ODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataCollectionSerializer.ShouldAddTypeNameAnnotation(metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Full, false)]
        [InlineData(ODataMetadataLevel.None, true)]
        public void ShouldSuppressTypeNameSerialization(ODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataCollectionSerializer.ShouldSuppressTypeNameSerialization(metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
