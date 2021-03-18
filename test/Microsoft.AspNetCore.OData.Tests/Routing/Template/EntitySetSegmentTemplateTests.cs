// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class EntitySetSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EntitySet()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EntitySetSegmentTemplate(entitySet: null), "entitySet");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EntitySetSegment()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EntitySetSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CommonEntitySetProperties_ReturnsAsExpected()
        {
            // Assert
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmEntitySet entityset = new EdmEntitySet(container, "entities", entityType);
            EntitySetSegmentTemplate template = new EntitySetSegmentTemplate(new EntitySetSegment(entityset));

            // Act & Assert
            Assert.Equal(ODataSegmentKind.EntitySet, template.Kind);

            Assert.Equal("entities", template.Literal);
            Assert.False(template.IsSingle);
            Assert.Equal(EdmTypeKind.Collection, template.EdmType.TypeKind);
            Assert.Same(entityset, template.NavigationSource);
        }

        [Fact]
        public void TryTranslateEntitySetSegmentTemplate_ReturnsODataEntitySetSegment()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmEntitySet entityset = new EdmEntitySet(container, "entities", entityType);
            EntitySetSegmentTemplate template = new EntitySetSegmentTemplate(new EntitySetSegment(entityset));

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            EntitySetSegment setSegment = Assert.IsType<EntitySetSegment>(actual);
            Assert.Same(entityset, setSegment.EntitySet);
        }
    }
}
