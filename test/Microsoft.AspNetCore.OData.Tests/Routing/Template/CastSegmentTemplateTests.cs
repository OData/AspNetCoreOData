// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class CastSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_CastType()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new CastSegmentTemplate(null, null, null), "castType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_ExpectedType()
        {
            // Assert
            Mock<IEdmType> edmType = new Mock<IEdmType>();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new CastSegmentTemplate(edmType.Object, null, null), "expectedType");
        }

        [Fact]
        public void CommonCastProperties_ReturnsAsExpected()
        {
            // Assert
            EdmEntityType baseType = new EdmEntityType("NS", "base");
            EdmEntityType subType = new EdmEntityType("NS", "sub", baseType);
            CastSegmentTemplate template = new CastSegmentTemplate(subType, baseType, null);

            // Act & Assert
            Assert.Equal(ODataSegmentKind.Cast, template.Kind);
            Assert.Equal("NS.sub", template.Literal);
            Assert.True(template.IsSingle);
            Assert.Same(subType, template.EdmType);
            Assert.Null(template.NavigationSource);
        }

        [Fact]
        public void Translate_ReturnsODataActionImportSegment()
        {
            // Arrange
            EdmEntityType baseType = new EdmEntityType("NS", "base");
            EdmEntityType subType = new EdmEntityType("NS", "sub", baseType);
            CastSegmentTemplate template = new CastSegmentTemplate(subType, baseType, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            ODataPathSegment actual = template.Translate(context);

            // Assert
            Assert.NotNull(actual);
            TypeSegment typeSegment = Assert.IsType<TypeSegment>(actual);
            Assert.Same(subType, typeSegment.EdmType);
        }
    }
}
