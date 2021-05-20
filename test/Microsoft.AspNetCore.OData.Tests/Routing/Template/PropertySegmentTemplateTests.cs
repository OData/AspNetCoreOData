// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class PropertySegmentTemplateTests
    {
        private PropertySegmentTemplate _propertySegment;

        public PropertySegmentTemplateTests()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            _propertySegment = new PropertySegmentTemplate(property);
        }

        [Fact]
        public void CtorPropertySegmentTemplate_ThrowsArgumentNull_Property()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PropertySegmentTemplate(property: null), "property");
        }

        [Fact]
        public void CtorPropertySegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PropertySegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CtorPropertySegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            PropertySegmentTemplate propertySegment = new PropertySegmentTemplate(property);

            // Assert
            Assert.NotNull(propertySegment.Segment);
            Assert.Same(property, propertySegment.Property);
        }

        [Fact]
        public void GetTemplatesPropertySegmentTemplate_ReturnsTemplates()
        {
            // Assert
            PropertySegmentTemplate propertySegment = GetSegmentTemplate();

            // Act & Assert
            IEnumerable<string> templates = propertySegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/Name", template);
        }

        [Fact]
        public void TryTranslateSingletonSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            PropertySegmentTemplate propertySegment = GetSegmentTemplate();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => propertySegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslatePropertySegmentTemplate_ReturnsPropertySegment()
        {
            // Arrange
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();
            PropertySegmentTemplate propertySegment = GetSegmentTemplate();

            // Act
            bool ok = propertySegment.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment segment = Assert.Single(context.Segments);
            PropertySegment odataPropertySegment = Assert.IsType<PropertySegment>(segment);
            Assert.Equal("Edm.String", odataPropertySegment.EdmType.FullTypeName());
        }

        private static PropertySegmentTemplate GetSegmentTemplate()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            return new PropertySegmentTemplate(property);
        }
    }
}
