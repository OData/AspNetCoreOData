//-----------------------------------------------------------------------------
// <copyright file="PropertyCatchAllSegmentTemplateTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class PropertyCatchAllSegmentTemplateTests
    {
        private static IEdmEntityType _entityType;

        static PropertyCatchAllSegmentTemplateTests()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "Customer", null, false, true);
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("Title", EdmPrimitiveTypeKind.String);
            _entityType = entityType;
        }

        [Fact]
        public void CtorPropertyCatchAllSegmentTemplate_ThrowsArgumentNull_DeclaredType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PropertyCatchAllSegmentTemplate(null), "declaredType");
        }

        [Fact]
        public void CtorPropertyCatchAllSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            PropertyCatchAllSegmentTemplate pathSegment = new PropertyCatchAllSegmentTemplate(_entityType);

            // Assert
            Assert.Same(_entityType, pathSegment.StructuredType);
        }

        [Fact]
        public void GetTemplatesPropertyCatchAllSegmentTemplate_ReturnsTemplates()
        {
            // Arrange
            PropertyCatchAllSegmentTemplate pathSegment = new PropertyCatchAllSegmentTemplate(_entityType);

            // Act & Assert
            IEnumerable<string> templates = pathSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/{property}", template);
        }

        [Fact]
        public void TryTranslatePropertyCatchAllSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            PropertyCatchAllSegmentTemplate pathSegment = new PropertyCatchAllSegmentTemplate(_entityType);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => pathSegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslatePropertyCatchAllSegmentTemplate_ReturnsFalse_NoCorrectRouteData()
        {
            // Arrange
            PropertyCatchAllSegmentTemplate pathSegment = new PropertyCatchAllSegmentTemplate(_entityType);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = new RouteValueDictionary()
            };

            // Act & Assert
            Assert.False(pathSegment.TryTranslate(context));
        }

        [Theory]
        [InlineData("Title")]
        [InlineData("title")]
        [InlineData("tiTLE")]
        public void TryTranslatePropertyCatchAllSegmentTemplate_ReturnsPropertySegment(string property)
        {
            // Arrange
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { property = $"{property}" });
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary
            };

            PropertyCatchAllSegmentTemplate pathSegment = new PropertyCatchAllSegmentTemplate(_entityType);

            // Act
            bool ok = pathSegment.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment segment = Assert.Single(context.Segments);
            PropertySegment propertySegment = Assert.IsType<PropertySegment>(segment);
            Assert.Equal("Title", propertySegment.Property.Name);
        }
    }
}
