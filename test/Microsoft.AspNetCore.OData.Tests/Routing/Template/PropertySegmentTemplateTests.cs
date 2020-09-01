// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class PropertySegmentTemplateTests
    {
        [Fact]
        public void CtorThrowsArgumentNullSegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PropertySegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void PropertyCommonPropertiesReturnsAsExpected()
        {
            // Assert
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            PropertySegmentTemplate propertySegment = new PropertySegmentTemplate(property);

            // Act & Assert
            Assert.Equal("Name", propertySegment.Literal);
            Assert.Equal(ODataSegmentKind.Property, propertySegment.Kind);
            Assert.True(propertySegment.IsSingle);
            Assert.Equal("Edm.String", propertySegment.EdmType.FullTypeName());
        }

        [Fact]
        public void TranslateValueTemplateReturnsAsExpected()
        {
            // Arrange
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            PropertySegmentTemplate propertySegment = new PropertySegmentTemplate(property);

            // Act
            ODataPathSegment segment = propertySegment.Translate(context);

            // Assert
            PropertySegment odataPropertySegment = Assert.IsType<PropertySegment>(segment);
            Assert.Equal("Edm.String", odataPropertySegment.EdmType.FullTypeName());
        }
    }
}
