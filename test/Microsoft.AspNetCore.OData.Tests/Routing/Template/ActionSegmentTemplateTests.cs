//-----------------------------------------------------------------------------
// <copyright file="ActionSegmentTemplateTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
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
        public void CtorActionSegmentTemplate_ThrowsArgument_NonboundAction()
        {
            // Arrange
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmAction action = new EdmAction("NS", "MyAction", primitive, false, null);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => new ActionSegmentTemplate(action, null),
                "The input operation 'MyAction' is not a bound 'action'.");
        }

        [Fact]
        public void CtorActionSegmentTemplate_ThrowsODataException_NonAction()
        {
            // Arrange
            IEdmPrimitiveTypeReference intPrimitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", intPrimitive, false, null, false);
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
            EdmAction action = new EdmAction("NS", "action", null, true, null);
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
            EdmAction action = new EdmAction("NS", "action", null, true, null);
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
            EdmAction action = new EdmAction("NS", "action", null, true, null);
            ActionSegmentTemplate template = new ActionSegmentTemplate(action, null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => template.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateActionSegmentTemplate_ReturnsODataActionImportSegment()
        {
            // Arrange
            EdmAction action = new EdmAction("NS", "action", null, true, null);
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

        [Fact]
        public void TryTranslateActionSegmentTemplate_ReturnsODataActionSegment_WithReturnedEntitySet()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>().Object;
            var endpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test");
            var routeValues = new RouteValueDictionary();

            var model = new EdmModel();
            var entityType = new EdmEntityType("NS", "Entity");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            model.AddElement(entityType);
            EdmAction action = new EdmAction("NS", "Action", new EdmEntityTypeReference(entityType, true), true, null);
            model.AddElement(action);
            var entityContainer = new EdmEntityContainer("NS", "Default");
            var entitySet = entityContainer.AddEntitySet("EntitySet", entityType);
            model.AddElement(entityContainer);
            model.SetAnnotationValue(action, new ReturnedEntitySetAnnotation("EntitySet"));

            var template = new ActionSegmentTemplate(action, null);
            var translateContext = new ODataTemplateTranslateContext(httpContext, endpoint, routeValues, model);

            // Act
            bool ok = template.TryTranslate(translateContext);

            // Assert
            Assert.True(ok);
            var actual = Assert.Single(translateContext.Segments);
            var actionSegment = Assert.IsType<OperationSegment>(actual);
            Assert.Equal(actionSegment.EdmType, entityType);
            Assert.Equal(actionSegment.EntitySet, entitySet);
        }
    }
}
