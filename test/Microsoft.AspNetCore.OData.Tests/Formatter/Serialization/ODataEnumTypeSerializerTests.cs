//-----------------------------------------------------------------------------
// <copyright file="ODataEnumTypeSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataEnumTypeSerializerTests
    {
        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation()
        {
            // Arrange
            ODataEnumValue enumValue = new ODataEnumValue("value");
            IEdmEnumTypeReference enumType = new EdmEnumTypeReference(
                new EdmEnumType("TestModel", "EnumType"), isNullable: false);

            // Act
            ODataEnumSerializer.AddTypeNameAnnotationAsNeeded(enumValue, enumType, ODataMetadataLevel.Minimal);

            // Assert
            ODataTypeAnnotation annotation = enumValue.TypeAnnotation;
            Assert.Null(annotation);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddAnnotation_InFullMetadataMode()
        {
            // Arrange
            ODataEnumValue enumValue = new ODataEnumValue("value");
            IEdmEnumTypeReference enumType = new EdmEnumTypeReference(
                new EdmEnumType("TestModel", "EnumType"), isNullable: false);

            // Act
            ODataEnumSerializer.AddTypeNameAnnotationAsNeeded(enumValue, enumType, ODataMetadataLevel.Full);

            // Assert
            ODataTypeAnnotation annotation = enumValue.TypeAnnotation;
            Assert.NotNull(annotation);
            Assert.Equal("TestModel.EnumType", annotation.TypeName);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsNullAnnotation_InNoMetadataMode()
        {
            // Arrange
            ODataEnumValue enumValue = new ODataEnumValue("value");
            IEdmEnumTypeReference enumType = new EdmEnumTypeReference(
                new EdmEnumType("TestModel", "EnumType"), isNullable: false);

            // Act
            ODataEnumSerializer.AddTypeNameAnnotationAsNeeded(enumValue, enumType, ODataMetadataLevel.None);

            // Assert
            ODataTypeAnnotation annotation = enumValue.TypeAnnotation;
            Assert.NotNull(annotation);
            Assert.Null(annotation.TypeName);
        }

        [Fact]
        public async Task WriteObjectAsync_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange
            IODataSerializerProvider provider = new Mock<IODataSerializerProvider>().Object;
            ODataEnumSerializer serializer = new ODataEnumSerializer(provider);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => serializer.WriteObjectAsync(graph: null, type: null, messageWriter: null, writeContext: null),
                "messageWriter");

            // Arrange & Act & Assert
            ODataMessageWriter messageWriter = ODataTestUtil.GetMockODataMessageWriter();
            await ExceptionAssert.ThrowsArgumentNullAsync(() => serializer.WriteObjectAsync(graph: null, type: null, messageWriter, null),
                "writeContext");

            // Arrange & Act & Assert
            ODataSerializerContext context = new ODataSerializerContext();
            context.RootElementName = null;
            await ExceptionAssert.ThrowsAsync<ArgumentException>(() => serializer.WriteObjectAsync(graph: null, type: null, messageWriter, context),
                "The 'RootElementName' property is required on 'ODataSerializerContext'. (Parameter 'writeContext')");
        }

        [Fact]
        public void CreateODataEnumValue_ReturnsCorrectEnumMember()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EnumType<BookCategory>().Namespace = "NS";
            IEdmModel model = builder.GetEdmModel();
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().Single();

            IServiceProvider serviceProvder = new Mock<IServiceProvider>().Object;
            var provider = new ODataSerializerProvider(serviceProvder);
            ODataEnumSerializer serializer = new ODataEnumSerializer(provider);
            ODataSerializerContext writeContext = new ODataSerializerContext
            {
                Model = model
            };

            // Act
            ODataEnumValue value = serializer.CreateODataEnumValue(BookCategory.Newspaper,
                new EdmEnumTypeReference(enumType, false), writeContext);

            // Assert
            Assert.NotNull(value);
            Assert.Equal("news", value.Value);
        }

        [Fact]
        public void CreateODataValue_ThrowsInvalidOperation_NonEnumType()
        {
            // Arrange
            IODataSerializerProvider provider = new Mock<IODataSerializerProvider>().Object;
            ODataEnumSerializer serializer = new ODataEnumSerializer(provider);
            IEdmTypeReference expectedType = EdmCoreModel.Instance.GetString(false);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => serializer.CreateODataValue(graph: null, expectedType: expectedType, writeContext: null),
                "ODataEnumSerializer cannot write an object of type 'Edm.String'.");
        }

        [Fact]
        public void CreateODataValue_RetrunsNull_IfGraphNull()
        {
            // Arrange
            ODataSerializerContext writeContext = new ODataSerializerContext();
            IODataSerializerProvider provider = new Mock<IODataSerializerProvider>().Object;
            ODataEnumSerializer serializer = new ODataEnumSerializer(provider);
            IEdmEnumType enumType = new EdmEnumType("NS", "Enum");
            IEdmTypeReference expectedType = new EdmEnumTypeReference(enumType, false);

            // Act
            ODataValue actual = serializer.CreateODataValue(graph: null, expectedType: expectedType, writeContext);

            // Assert
            Assert.IsType<ODataNullValue>(actual);
        }

        [Fact]
        public void CreateODataValue_Retruns_CorrectODataValue()
        {
            // Arrange
            ODataSerializerContext writeContext = new ODataSerializerContext();
            IODataSerializerProvider provider = new Mock<IODataSerializerProvider>().Object;
            Mock<ODataEnumSerializer> serializer = new Mock<ODataEnumSerializer>(provider);
            ODataEnumValue enumValue = new ODataEnumValue("Cartoon");
            serializer.Setup(s => s.CreateODataEnumValue(null, It.IsAny<IEdmEnumTypeReference>(), writeContext)).Returns(enumValue);

            IEdmEnumType enumType = new EdmEnumType("NS", "Enum");
            IEdmTypeReference expectedType = new EdmEnumTypeReference(enumType, false);

            // Act
            ODataValue actual = serializer.Object.CreateODataValue(graph: null, expectedType: expectedType, writeContext);

            // Assert
            Assert.Same(enumValue, actual);
        }
    }

    [DataContract(Name = "category")]
    public enum BookCategory
    {
        [EnumMember(Value = "cartoon")]
        Cartoon,

        [EnumMember(Value = "news")]
        Newspaper
    }
}
