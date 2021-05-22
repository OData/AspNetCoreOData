// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class PathTemplateSegmentTemplateTests
    {
        private static IEdmEntityType _entityType;

        static PathTemplateSegmentTemplateTests()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "Customer", null, false, true);
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("Title", EdmPrimitiveTypeKind.String);

            entityType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "RelatedCustomers",
                Target = entityType,
                TargetMultiplicity = EdmMultiplicity.Many
            });

            _entityType = entityType;
        }

        [Fact]
        public void CtorPathTemplateSegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PathTemplateSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CtorPathTemplateSegmentTemplate_ThrowsODataException_WrongTemplate()
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment("property");

            // Act & Assert
            ODataException exception = ExceptionAssert.Throws<ODataException>(() => new PathTemplateSegmentTemplate(segment));
            Assert.Equal("The attribute routing template contains invalid segment 'property'. The template string does not start with '{' or ends with '}'.",
                exception.Message);
        }

        [Fact]
        public void CtorPathTemplateSegmentTemplate_ThrowsODataException_EmptyTemplate()
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment("{}");

            // Act & Assert
            ODataException exception = ExceptionAssert.Throws<ODataException>(() => new PathTemplateSegmentTemplate(segment));
            Assert.Equal("The route template in path template '{}' is empty.", exception.Message);
        }

        [Fact]
        public void CtorPathTemplateSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            PathTemplateSegment segment = new PathTemplateSegment("{any}");
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);

            // Assert
            Assert.Equal("any", pathSegment.ParameterName);
            Assert.Same(segment, pathSegment.Segment);
        }

        [Fact]
        public void GetTemplatesPathTemplateSegmentTemplate_ReturnsTemplates()
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment("{any}");
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);

            // Act & Assert
            IEnumerable<string> templates = pathSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/{any}", template);
        }

        [Fact]
        public void TryTranslatePathTemplateSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment("{any}");
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => pathSegment.TryTranslate(null), "context");
        }

        [Theory]
        [InlineData("{any}")] // false because unknown parameter name
        [InlineData("{property}")] // false because no previous segment in the context
        [InlineData("{dynamicproperty}")] // false because no previous segment in the context
        public void TryTranslatePathTemplateSegmentTemplate_ReturnsFalse_NotSupportedTemplate(string template)
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment(template);
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act & Assert
            Assert.False(pathSegment.TryTranslate(context));
        }

        [Theory]
        [InlineData("{property}")]
        [InlineData("{dynamicproperty}")]
        public void TryTranslatePathTemplateSegmentTemplate_ReturnsFalse_PreviousSegmentNotStructuredType(string template)
        {
            // Arrange
            MySegment previousSegment = new MySegment(EdmCoreModel.Instance.GetString(false).Definition);

            PathTemplateSegment segment = new PathTemplateSegment(template);
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();
            context.Segments.Add(previousSegment);

            // Act & Assert
            Assert.False(pathSegment.TryTranslate(context));
        }

        [Theory]
        [InlineData("{property}")]
        [InlineData("{dynamicproperty}")]
        public void TryTranslatePathTemplateSegmentTemplate_ReturnsFalse_NoCorrectRouteData(string template)
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment(template);
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = new RouteValueDictionary()
            };
            context.Segments.Add(new MySegment(_entityType));

            // Act & Assert
            Assert.False(pathSegment.TryTranslate(context));
        }

        [Fact]
        public void TryTranslatePathTemplateSegmentTemplate_ReturnsFalse_UnknownProperty()
        {
            // Arrange
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { property = "Unknown" });
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary
            };
            context.Segments.Add(new MySegment(_entityType));

            PathTemplateSegment segment = new PathTemplateSegment("{property}");
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);

            // Act & Assert
            Assert.False(pathSegment.TryTranslate(context));
        }

        [Fact]
        public void TryTranslatePathTemplateSegmentTemplate_ReturnsPropertySegment()
        {
            // Arrange
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { property = "Title" });
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary
            };
            context.Segments.Add(new MySegment(_entityType));

            PathTemplateSegment segment = new PathTemplateSegment("{property}");
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);

            // Act
            Assert.True(pathSegment.TryTranslate(context));

            // Assert
            Assert.Equal(2, context.Segments.Count); // 1 - MySegment, 2 - Property Segment
            Assert.Collection(context.Segments,
                e =>
                {
                    Assert.IsType<MySegment>(e);
                },
                e =>
                {
                    PropertySegment propertySegment = Assert.IsType<PropertySegment>(e);
                    Assert.Equal("Title", propertySegment.Property.Name);
                });
        }

        [Fact]
        public void TryTranslatePathTemplateSegmentTemplate_ReturnsNavigationPropertySegment()
        {
            // Arrange
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { property = "RelatedCustomers" });
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary
            };
            context.Segments.Add(new MySegment(_entityType));

            PathTemplateSegment segment = new PathTemplateSegment("{property}");
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);

            // Act
            Assert.True(pathSegment.TryTranslate(context));

            // Assert
            Assert.Equal(2, context.Segments.Count); // 1 - MySegment, 2 - Property Segment
            Assert.Collection(context.Segments,
                e =>
                {
                    Assert.IsType<MySegment>(e);
                },
                e =>
                {
                    NavigationPropertySegment propertySegment = Assert.IsType<NavigationPropertySegment>(e);
                    Assert.Equal("RelatedCustomers", propertySegment.NavigationProperty.Name);
                });
        }

        [Fact]
        public void TryTranslatePathTemplateSegmentTemplate_ReturnsFalse_DynamicPathSegmentWithKnowProperty()
        {
            // Arrange
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { dynamicproperty = "Name" });
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary
            };
            context.Segments.Add(new MySegment(_entityType));

            PathTemplateSegment segment = new PathTemplateSegment("{dynamicproperty}");
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);

            // Act & Assert
            Assert.False(pathSegment.TryTranslate(context));
        }

        [Fact]
        public void TryTranslatePathTemplateSegmentTemplate_ReturnsDynamicPathSegment()
        {
            // Arrange
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { dynamicproperty = "Dynamic" });
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary
            };
            context.Segments.Add(new MySegment(_entityType));

            PathTemplateSegment segment = new PathTemplateSegment("{dynamicproperty}");
            PathTemplateSegmentTemplate pathSegment = new PathTemplateSegmentTemplate(segment);

            // Act
            Assert.True(pathSegment.TryTranslate(context));

            // Assert
            Assert.Equal(2, context.Segments.Count); // 1 - MySegment, 2 - Property Segment
            Assert.Collection(context.Segments,
                e =>
                {
                    Assert.IsType<MySegment>(e);
                },
                e =>
                {
                    DynamicPathSegment dynamicSegment = Assert.IsType<DynamicPathSegment>(e);
                    Assert.Equal("Dynamic", dynamicSegment.Identifier);
                });
        }

        private class MySegment : ODataPathSegment
        {
            public MySegment(IEdmType type)
            {
                EdmType = type;
            }

            public override IEdmType EdmType { get; }

            public override void HandleWith(PathSegmentHandler handler)
            {
                throw new System.NotImplementedException();
            }

            public override T TranslateWith<T>(PathSegmentTranslator<T> translator)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
