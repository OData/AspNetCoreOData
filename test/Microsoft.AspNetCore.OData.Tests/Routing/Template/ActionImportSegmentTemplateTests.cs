// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
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
        public void CommonProperties_ReturnsAsExpected()
        {
            // Assert
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmAction action = new EdmAction("NS", "action", null);
            EdmActionImport actionImport = new EdmActionImport(container, "actionImport", action);

            ActionImportSegmentTemplate template = new ActionImportSegmentTemplate(actionImport, null);

            // Act & Assert
            Assert.Equal(ODataSegmentKind.ActionImport, template.Kind);
            Assert.Equal("actionImport", template.Literal);
            Assert.False(template.IsSingle);
            Assert.Null(template.EdmType);
            Assert.Null(template.NavigationSource);
        }

        [Fact]
        public void TryTranslate_ReturnsODataActionImportSegment()
        {
            // Arrange
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            EdmAction action = new EdmAction("NS", "action", null);
            EdmActionImport actionImport = new EdmActionImport(container, "name", action);

            ActionImportSegmentTemplate template = new ActionImportSegmentTemplate(actionImport, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            OperationImportSegment actionImportSegment = Assert.IsType<OperationImportSegment>(actual);
            Assert.Same(actionImport, actionImportSegment.OperationImports.First());
        }
    }
}
