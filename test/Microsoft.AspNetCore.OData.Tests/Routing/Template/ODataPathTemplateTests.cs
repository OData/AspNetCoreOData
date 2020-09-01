// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class ODataPathTemplateTests
    {
        [Fact]
        public void TemplatePropertyWithNoSegments()
        {
            // Arrange
            ODataPathTemplate path = new ODataPathTemplate();

            // Act & Assert
            Assert.Equal("", path.Template);
        }

        [Fact]
        public void TemplatePropertyWithOneSegment()
        {
            // Arrange
            ODataPathTemplate path = new ODataPathTemplate(MetadataSegmentTemplate.Instance);

            // Act & Assert
            Assert.Equal("$metadata", path.Template);
        }

        [Fact]
        public void TemplatePropertyWithTwoSegments()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmEntitySet entitySet = new EdmEntitySet(container, "set", entityType);
            EdmAction action = new EdmAction("NS", "action", null, true, null);
            ODataPathTemplate path = new ODataPathTemplate(new EntitySetSegmentTemplate(entitySet),
                new ActionSegmentTemplate(action, null));

            // Act & Assert
            Assert.Equal("set/NS.action", path.Template);
        }
    }
}
