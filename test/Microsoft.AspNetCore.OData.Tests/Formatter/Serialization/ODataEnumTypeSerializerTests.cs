// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
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
        public void CreateODataEnumValue_ReturnsCorrectEnumMember()
        {
            // Arrange
            var builder = new ODataConventionModelBuilder();
            builder.EnumType<BookCategory>().Namespace = "NS";
            IEdmModel model = builder.GetEdmModel();
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().Single();

            IServiceProvider serviceProvder = new Mock<IServiceProvider>().Object;
            var provider = new DefaultODataSerializerProvider(serviceProvder);
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
