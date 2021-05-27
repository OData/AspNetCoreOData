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
        private static PropertySegmentTemplate _propertySegment;
        private static IEdmStructuralProperty _edmProperty;

        static PropertySegmentTemplateTests()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            _edmProperty = entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            _propertySegment = new PropertySegmentTemplate(_edmProperty);
        }

        [Fact]
        public void CtorPropertySegmentTemplate_ThrowsArgumentNull_Property()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PropertySegmentTemplate(property: null), "property");
        }

        [Fact]
        public void CtorPropertySegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PropertySegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CtorPropertySegmentTemplate_SetsProperties()
        {
            // Arrange & Act & Assert
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            PropertySegmentTemplate propertySegment = new PropertySegmentTemplate(property);
            Assert.NotNull(propertySegment.Segment);
            Assert.Same(property, propertySegment.Property);

            // Arrange & Act & Assert
            PropertySegmentTemplate propertySegment2 = new PropertySegmentTemplate(propertySegment.Segment);
            Assert.Same(propertySegment.Segment, propertySegment2.Segment);
        }

        [Fact]
        public void GetTemplatesPropertySegmentTemplate_ReturnsTemplates()
        {
            // Assert & Act & Assert
            IEnumerable<string> templates = _propertySegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/Name", template);
        }

        [Fact]
        public void TryTranslatePropertySegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => _propertySegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslatePropertySegmentTemplate_ReturnsPropertySegment()
        {
            // Arrange
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            bool ok = _propertySegment.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment segment = Assert.Single(context.Segments);
            PropertySegment odataPropertySegment = Assert.IsType<PropertySegment>(segment);
            Assert.Equal("Edm.String", odataPropertySegment.EdmType.FullTypeName());
        }
    }
}
