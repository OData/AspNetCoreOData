//-----------------------------------------------------------------------------
// <copyright file="EdmModelExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class EdmModelExtensionsTests
    {
        private static IEdmModel _model = GetEdmModel();

        [Fact]
        public void ResolveAlternateKeyProperties_ThrowsArugmentNull()
        {
            // Arrange & Act & Assert
            IEdmModel model = null;
            ExceptionAssert.ThrowsArgumentNull(() => model.ResolveAlternateKeyProperties(null), "model");

            // Arrange & Act & Assert
            model = EdmCoreModel.Instance;
            ExceptionAssert.ThrowsArgumentNull(() => model.ResolveAlternateKeyProperties(null), "keySegment");
        }

        [Fact]
        public void ResolveProperty_ThrowsArugmentNull()
        {
            // Arrange & Act & Assert
            IEdmStructuredType structuredType = null;
            ExceptionAssert.ThrowsArgumentNull(() => structuredType.ResolveProperty(null), "structuredType");
        }

        [Fact]
        public void ResolvePropertyTest_WorksForCaseSensitiveAndInsensitive()
        {
            // Arrange
            EdmComplexType structuredType = new EdmComplexType("NS", "Complex");
            structuredType.AddStructuralProperty("Title", EdmPrimitiveTypeKind.String);
            structuredType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            structuredType.AddStructuralProperty("nAme", EdmPrimitiveTypeKind.Int32);
            structuredType.AddStructuralProperty("naMe", EdmPrimitiveTypeKind.Double);

            // 1) Act & Assert: Cannot find the property
            IEdmProperty property = structuredType.ResolveProperty("Unknown");
            Assert.Null(property);

            // 2) Act & Assert : Can find one "Title" property
            foreach (var name in new[] { "Title", "title", "tiTle", "TITLE" })
            {
                VerifyResolvedProperty(structuredType, name, "Title", "Edm.String");
            }

            // 3) Act & Assert: Can find the correct overload version
            VerifyResolvedProperty(structuredType, "Name", "Name", "Edm.String");
            VerifyResolvedProperty(structuredType, "nAme", "nAme", "Edm.Int32");
            VerifyResolvedProperty(structuredType, "naMe", "naMe", "Edm.Double");
        }

        private static void VerifyResolvedProperty(IEdmStructuredType structuredType, string propertyName, string expectedName, string expectedTypeName)
        {
            IEdmProperty property = structuredType.ResolveProperty(propertyName);
            Assert.NotNull(property);

            Assert.Equal(expectedName, property.Name);
            Assert.Equal(expectedTypeName, property.Type.FullName());
        }

        [Fact]
        public void ResolvePropertyTest_ThrowsForAmbiguousPropertyName()
        {
            // Arrange
            EdmComplexType structuredType = new EdmComplexType("NS", "Complex");
            structuredType.AddStructuralProperty("Title", EdmPrimitiveTypeKind.String);
            structuredType.AddStructuralProperty("tiTle", EdmPrimitiveTypeKind.Int32);
            structuredType.AddStructuralProperty("tiTlE", EdmPrimitiveTypeKind.Double);

            // Act & Assert - Positive case
            IEdmProperty edmProperty = structuredType.ResolveProperty("tiTlE");
            Assert.NotNull(edmProperty);
            Assert.Equal("Edm.Double", edmProperty.Type.FullName());

            // Act & Assert - Negative case
            Action test = () => structuredType.ResolveProperty("title");
            ExceptionAssert.Throws<ODataException>(test, "Ambiguous property name 'title' found. Please use correct property name case.");
        }

        [Fact]
        public void FindProperty_ThrowsArugmentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            IEdmModel model = null;
            ExceptionAssert.ThrowsArgumentNull(() => model.FindProperty(null, null), "model");

            model = new Mock<IEdmModel>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => model.FindProperty(null, null), "structuredType");

            IEdmStructuredType structuredType = new Mock<IEdmStructuredType>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => model.FindProperty(structuredType, null), "path");
        }

        [Theory]
        [InlineData("Code", "Code")]
        [InlineData("Location", "Location")]
        [InlineData("Location/Street", "Street")]
        [InlineData("Location/City", "City")]
        [InlineData("Location/NS.VipAddress/ZipCode", "ZipCode")]
        public void FindPropertyTest_WorksForDirectPropertyOnType(string pathStr, string name)
        {
            // Arrange
            EdmPropertyPathExpression path = new EdmPropertyPathExpression(pathStr);
            IEdmEntityType company = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Company");
            Assert.NotNull(company);

            // Act
            IEdmProperty property = _model.FindProperty(company, path);

            // Assert
            Assert.NotNull(property);
            Assert.Equal(name, property.Name);
        }

        [Fact]
        public void FindPropertyTest_ThrowsForPropertyNotFoundOnPathExpression()
        {
            // Arrange
            EdmPropertyPathExpression path = new EdmPropertyPathExpression("OtherPath");
            IEdmEntityType company = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Company");
            Assert.NotNull(company);

            // Act & Assert
            Action test = () => _model.FindProperty(company, path);
            ExceptionAssert.Throws<ODataException>(test, "Can not resolve the property using property path 'OtherPath' from type 'NS.Company'.");
        }

        [Fact]
        public void FindPropertyTest_ThrowsForResourceTypeNotInModel()
        {
            // Arrange
            EdmPropertyPathExpression path = new EdmPropertyPathExpression("Location/NS.AnotherType");
            IEdmEntityType company = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Company");
            Assert.NotNull(company);

            // Act & Assert
            Action test = () => _model.FindProperty(company, path);
            ExceptionAssert.Throws<ODataException>(test, "Cannot find the resource type 'NS.AnotherType' in the model.");
        }

        [Fact]
        public void ResolveNavigationSource_ThrowsArugmentNull()
        {
            // Arrange & Act & Assert
            IEdmModel model = null;
            ExceptionAssert.ThrowsArgumentNull(() => model.ResolveNavigationSource(null), "model");
        }

        [Fact]
        public void ResolveNavigationSource_ThrowsODataException_AmbiguousIdentifier()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            EdmEntityContainer containter = new EdmEntityContainer("NS", "Default");
            model.AddElement(entityType);
            model.AddElement(containter);
            containter.AddEntitySet("entities", entityType);
            containter.AddEntitySet("enTIties", entityType);

            // Act & Assert
            Assert.NotNull(model.ResolveNavigationSource("enTIties"));
            Assert.NotNull(model.ResolveNavigationSource("enTIties", true));

            // Act & Assert
            Assert.Null(model.ResolveNavigationSource("Entities"));

            Action test = () => model.ResolveNavigationSource("Entities", true);
            ExceptionAssert.Throws<ODataException>(test,
                "Ambiguous navigation source (entity set or singleton) name 'Entities' found. Please use correct navigation source name case.");
        }

        [Fact]
        public void IsEnumOrCollectionEnum_Works_EdmType()
        {
            // Arrange & Act & Assert
            IEdmTypeReference typeReference = EdmCoreModel.Instance.GetString(false);
            Assert.False(typeReference.IsEnumOrCollectionEnum());

            // Arrange & Act & Assert
            EdmEnumType enumType = new EdmEnumType("NS", "Enum");
            typeReference = new EdmEnumTypeReference(enumType, true);
            Assert.True(typeReference.IsEnumOrCollectionEnum());

            // Arrange & Act & Assert
            typeReference = new EdmCollectionTypeReference(new EdmCollectionType(new EdmEnumTypeReference(enumType, true)));
            Assert.True(typeReference.IsEnumOrCollectionEnum());
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // complex type address
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            EdmComplexType vipAddress = new EdmComplexType("NS", "VipAddress");
            vipAddress.AddStructuralProperty("ZipCode", EdmPrimitiveTypeKind.String);
            model.AddElement(vipAddress);

            EdmEntityType company = new EdmEntityType("NS", "Company");
            company.AddKeys(company.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            company.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Int32);
            company.AddStructuralProperty("Location", new EdmComplexTypeReference(address, isNullable: true));
            model.AddElement(company);
            return model;
        }
    }
}
