// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class ActionImportSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_ActionImport()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ActionImportSegmentTemplate(actionImport: null, null), "actionImport");
        }

        [Fact]
        public void KindProperty_ReturnsActionImport()
        {
            // Assert
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmAction action = new EdmAction("NS", "action", null);
            EdmActionImport actionImport = new EdmActionImport(container, "name", action);

            ActionImportSegmentTemplate template = new ActionImportSegmentTemplate(actionImport, null);

            // Act & Assert
            Assert.Equal(ODataSegmentKind.ActionImport, template.Kind);
        }

        [Fact]
        public void Translate_ReturnsODataActionImportSegment()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmAction action = new EdmAction("NS", "action", null);
            EdmActionImport actionImport = new EdmActionImport(container, "name", action);

            ActionImportSegmentTemplate template = new ActionImportSegmentTemplate(actionImport, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            ODataPathSegment actual = template.Translate(context);

            // Assert
            Assert.NotNull(actual);
            ActionImportSegmentTemplate actionImportSegment = Assert.IsType<ActionImportSegmentTemplate>(actual);
            Assert.Same(actionImport, actionImportSegment.ActionImport);
        }
    }
}
