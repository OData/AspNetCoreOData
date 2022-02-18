//-----------------------------------------------------------------------------
// <copyright file="ODataEdmTypeSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataEdmTypeSerializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            // Arrange
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported).Object;

            // Act & Assert
            Assert.Equal(ODataPayloadKind.Unsupported, serializer.ODataPayloadKind);
        }

        [Fact]
        public void Ctor_SetsProperty_SerializerProvider()
        {
            // Arrange & Act
            IODataSerializerProvider serializerProvider = new Mock<IODataSerializerProvider>().Object;
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported, serializerProvider).Object;

            // Assert
            Assert.Same(serializerProvider, serializer.SerializerProvider);
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Throws_NotSupported()
        {
            // Arrange
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported) { CallBase = true };

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<NotSupportedException>(
                () => serializer.Object.WriteObjectInlineAsync(graph: null, expectedType: null, writer: null, writeContext: null),
                "ODataEdmTypeSerializerProxy does not support WriteObjectInline.");
        }

        [Fact]
        public void CreateODataValue_Throws_NotSupported()
        {
            // Arrange
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported) { CallBase = true };

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(
                () => serializer.Object.CreateODataValue(graph: null, expectedType: edmType, writeContext: null),
                "ODataEdmTypeSerializerProxy does not support CreateODataValue.");
        }

        [Fact]
        public void CreateProperty_Returns_ODataProperty()
        {
            // Arrange
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Unsupported);
            serializer
                .Setup(s => s.CreateODataValue(42, edmType, null))
                .Returns(new ODataPrimitiveValue(42));

            // Act
            ODataProperty property = serializer.Object.CreateProperty(graph: 42, expectedType: edmType,
                elementName: "SomePropertyName", writeContext: null);

            // Assert
            Assert.NotNull(property);
            Assert.Equal("SomePropertyName", property.Name);
            Assert.Equal(42, property.Value);
        }
    }
}
