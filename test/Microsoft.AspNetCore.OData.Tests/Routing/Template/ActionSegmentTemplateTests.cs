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
        public void KindProperty_ReturnsAction()
        {
            // Assert
            EdmAction action = new EdmAction("NS", "action", null);
            ActionSegmentTemplate template = new ActionSegmentTemplate(action, null);

            // Act & Assert
            Assert.Equal(ODataSegmentKind.Action, template.Kind);
        }

        [Fact]
        public void Translate_ReturnsODataActionImportSegment()
        {
            // Arrange
            EdmAction action = new EdmAction("NS", "action", null);
            ActionSegmentTemplate template = new ActionSegmentTemplate(action, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            ODataPathSegment actual = template.Translate(context);

            // Assert
            Assert.NotNull(actual);
            OperationSegment actionSegment = Assert.IsType<OperationSegment>(actual);
            Assert.Same(action, actionSegment.Operations.First());
        }
    }
}
