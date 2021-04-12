// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class ActionSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Action()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ActionSegmentTemplate(action: null, null), "action");
        }

        [Fact]
        public void CommonActionProperties_ReturnsAsExpected()
        {
            // Assert
            EdmAction action = new EdmAction("NS", "action", null);
            ActionSegmentTemplate template = new ActionSegmentTemplate(action, null);

            // Act & Assert
            Assert.Equal(ODataSegmentKind.Action, template.Kind);
            Assert.Equal("NS.action", template.Literal);
            Assert.False(template.IsSingle);
            Assert.Null(template.EdmType);
            Assert.Null(template.NavigationSource);
        }

        [Fact]
        public void TryTranslate_ReturnsODataActionImportSegment()
        {
            // Arrange
            EdmAction action = new EdmAction("NS", "action", null);
            ActionSegmentTemplate template = new ActionSegmentTemplate(action, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            OperationSegment actionSegment = Assert.IsType<OperationSegment>(actual);
            Assert.Same(action, actionSegment.Operations.First());
        }
    }
}
