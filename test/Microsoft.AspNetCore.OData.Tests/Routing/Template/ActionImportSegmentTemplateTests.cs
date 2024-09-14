//-----------------------------------------------------------------------------
// <copyright file="ActionImportSegmentTemplateTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template;

public class ActionImportSegmentTemplateTests
{
    [Fact]
    public void CtorActionImportSegmentTemplate_ThrowsArgumentNull_ActionImport()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new ActionImportSegmentTemplate(actionImport: null, null), "actionImport");
    }

    [Fact]
    public void CtorActionImportSegmentTemplate_ThrowsArgumentNull_Segment()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new ActionImportSegmentTemplate(segment: null), "segment");
    }

    [Fact]
    public void CtorActionImportSegmentTemplate_ThrowsException_NonActionImport()
    {
        // Arrange
        IEdmPrimitiveTypeReference IntPrimitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
        EdmFunction function = new EdmFunction("NS", "MyFunction", IntPrimitive, false, null, false);

        Mock<IEdmFunctionImport> import = new Mock<IEdmFunctionImport>();
        import.Setup(i => i.Name).Returns("any");
        import.Setup(i => i.ContainerElementKind).Returns(EdmContainerElementKind.FunctionImport);
        import.Setup(i => i.Operation).Returns(function);
        OperationImportSegment operationImportSegment = new OperationImportSegment(import.Object, null);

        // Act
        Action test = () => new ActionImportSegmentTemplate(operationImportSegment);

        // Assert
        ExceptionAssert.Throws<ODataException>(test, "The input segment should be 'ActionImport' in 'ActionImportSegmentTemplate'.");
    }

    [Fact]
    public void CtorActionSegmentTemplate_SetsProperties()
    {
        // Arrange & Act
        ActionImportSegmentTemplate segment = GetSegmentTemplate(out IEdmActionImport actionImport);

        // Assert
        Assert.Same(actionImport, segment.ActionImport);
        Assert.NotNull(segment.Segment);
        Assert.Single(segment.Segment.OperationImports);

        // Act & Assert
        ActionImportSegmentTemplate segment1 = new ActionImportSegmentTemplate(segment.Segment);
        Assert.Same(segment.Segment, segment1.Segment);
    }

    [Fact]
    public void GetTemplatesActionImportSegmentTemplate_ReturnsTemplates()
    {
        // Arrange
        ActionImportSegmentTemplate segment = GetSegmentTemplate(out _);

        // Act & Assert
        IEnumerable<string> templates = segment.GetTemplates();
        string template = Assert.Single(templates);
        Assert.Equal("/actionImport", template);
    }

    [Fact]
    public void TryTranslateActionImportSegmentTemplate_ThrowsArgumentNull_Context()
    {
        // Arrange
        ActionImportSegmentTemplate segment = GetSegmentTemplate(out _);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => segment.TryTranslate(null), "context");
    }

    [Fact]
    public void TryTranslateActionImportSegmentTemplate_ReturnsODataActionImportSegment()
    {
        // Arrange
        ActionImportSegmentTemplate template = GetSegmentTemplate(out IEdmActionImport actionImport);
        ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

        // Act
        bool ok = template.TryTranslate(context);

        // Assert
        Assert.True(ok);
        ODataPathSegment actual = Assert.Single(context.Segments);
        OperationImportSegment actionImportSegment = Assert.IsType<OperationImportSegment>(actual);
        Assert.Same(actionImport, actionImportSegment.OperationImports.First());
    }

    private static ActionImportSegmentTemplate GetSegmentTemplate(out IEdmActionImport actionImport)
    {
        EdmEntityContainer container = new EdmEntityContainer("NS", "default");
        EdmAction action = new EdmAction("NS", "action", null);
        actionImport = new EdmActionImport(container, "actionImport", action);
        return new ActionImportSegmentTemplate(actionImport, null);
    }
}
