// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class CastSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_ActualType()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new CastSegmentTemplate(null, null, null), "actualType");
        }

        [Fact]
        public void KindProperty_ReturnsCast()
        {
            // Assert
            EdmEntityType baseType = new EdmEntityType("NS", "base");
            EdmEntityType subType = new EdmEntityType("NS", "sub", baseType);
            CastSegmentTemplate template = new CastSegmentTemplate(subType, baseType, null);

            // Act & Assert
            Assert.Equal(ODataSegmentKind.Cast, template.Kind);
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
