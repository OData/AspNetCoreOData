// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        public void CtorThrowsArgumentNullSegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new SingletonSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void SingletonCommonPropertiesReturnsAsExpected()
        {
            // Assert
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmSingleton singleton = new EdmSingleton(container, "singleton", entityType);
            SingletonSegmentTemplate singletonSegment = new SingletonSegmentTemplate(singleton);

            // Act & Assert
            Assert.Equal("singleton", singletonSegment.Literal);
            Assert.Equal(ODataSegmentKind.Singleton, singletonSegment.Kind);
            Assert.True(singletonSegment.IsSingle);
            Assert.Same(entityType, singletonSegment.EdmType);
            Assert.Same(singleton, singletonSegment.NavigationSource);
        }

        [Fact]
        public void TranslateValueTemplateReturnsAsExpected()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmSingleton singleton = new EdmSingleton(container, "singleton", entityType);

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();
            SingletonSegmentTemplate template = new SingletonSegmentTemplate(new SingletonSegment(singleton));

            // Act
            ODataPathSegment segment = template.Translate(context);

            // Assert
            Assert.NotNull(segment);
            SingletonSegment singletonTemplate = Assert.IsType<SingletonSegment>(segment);
            Assert.Same(singleton, singletonTemplate.Singleton);
        }
    }
}
