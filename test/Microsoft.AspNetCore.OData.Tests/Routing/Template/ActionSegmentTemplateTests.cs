// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class ActionSegmentTemplateTests
    {
        [Fact]
        public void CtorActionSegmentTemplate_ThrowsArgumentNull_Action()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ActionSegmentTemplate(action: null, null), "action");
        }

        [Fact]
        public void CtorActionSegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ActionSegmentTemplate(null), "segment");
        }

        [Fact]
        public void CtorActionSegmentTemplate_ThrowsODataException_NonAction()
        {
            // Arrange
            IEdmPrimitiveTypeReference IntPrimitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", IntPrimitive, false, null, false);
            OperationSegment operationSegment = new OperationSegment(function, null);

            // Act
            Action test = () => new ActionSegmentTemplate(operationSegment);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "The input segment should be 'Action' in 'ActionSegmentTemplate'.");
        }

        [Fact]
        public void CtorActionSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            EdmAction action = new EdmAction("NS", "action", null);
            ActionSegmentTemplate segment = new ActionSegmentTemplate(action, null);

            // Assert
            Assert.Same(action, segment.Action);
            Assert.NotNull(segment.Segment);
            Assert.Null(segment.NavigationSource);

            // Act & Assert
            ActionSegmentTemplate segment1 = new ActionSegmentTemplate(segment.Segment);
            Assert.Same(segment.Segment, segment1.Segment);
        }

        [Fact]
        public void GetTemplatesActionSegmentTemplate_ReturnsTemplates()
        {
            // Assert
            EdmAction action = new EdmAction("NS", "action", null);
            ActionSegmentTemplate segment = new ActionSegmentTemplate(action, null);

            // 1- Act & Assert
            IEnumerable<string> templates = segment.GetTemplates();
            Assert.Collection(templates,
                e =>
                {
                    Assert.Equal("/NS.action", e);
                },
                e =>
                {
                    Assert.Equal("/action", e);
                });

            // 2- Act & Assert
            templates = segment.GetTemplates(new ODataRouteOptions
            {
                EnableQualifiedOperationCall = false
            });
            string template = Assert.Single(templates);
            Assert.Equal("/action", template);

            // 3- Act & Assert
            templates = segment.GetTemplates(new ODataRouteOptions
            {
                EnableUnqualifiedOperationCall = false
            });
            template = Assert.Single(templates);
            Assert.Equal("/NS.action", template);
        }

        [Fact]
        public void TryTranslateActionSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            EdmAction action = new EdmAction("NS", "action", null);
            ActionSegmentTemplate template = new ActionSegmentTemplate(action, null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => template.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateActionSegmentTemplate_ReturnsODataActionImportSegment()
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
