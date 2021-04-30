// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class EdmModelExtensionsTests
    {
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
    }
}
