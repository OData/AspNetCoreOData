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
    public class SingletonSegmentTemplateTests
    {
        [Fact]
        public void CtorSingletonSegmentTemplate_ThrowsArgumentNull_Singleton()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new SingletonSegmentTemplate(singleton: null), "singleton");
        }

        [Fact]
        public void CtorSingletonSegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new SingletonSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CtorSingletonSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmSingleton singleton = new EdmSingleton(container, "singleton", entityType);
            SingletonSegmentTemplate singletonSegment = new SingletonSegmentTemplate(singleton);

            // Assert
            Assert.NotNull(singletonSegment.Segment);
            Assert.Same(singleton, singletonSegment.Singleton);
        }

        [Fact]
        public void GetTemplatesSingletonSegmentTemplate_ReturnsTemplates()
        {
            // Assert
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmSingleton singleton = new EdmSingleton(container, "singleton", entityType);
            SingletonSegmentTemplate singletonSegment = new SingletonSegmentTemplate(singleton);

            // Act & Assert
            IEnumerable<string> templates = singletonSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/singleton", template);
        }

        [Fact]
        public void TryTranslateSingletonSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmSingleton singleton = new EdmSingleton(container, "singleton", entityType);
            SingletonSegmentTemplate singletonSegment = new SingletonSegmentTemplate(singleton);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => singletonSegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateSingletonSegmentTemplate_ReturnsSingletonSegment()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmSingleton singleton = new EdmSingleton(container, "singleton", entityType);

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();
            SingletonSegmentTemplate template = new SingletonSegmentTemplate(new SingletonSegment(singleton));

            // Act
            Assert.True(template.TryTranslate(context));

            // Assert
            ODataPathSegment segment = Assert.Single(context.Segments);
            SingletonSegment singletonTemplate = Assert.IsType<SingletonSegment>(segment);
            Assert.Same(singleton, singletonTemplate.Singleton);
        }
    }
}
