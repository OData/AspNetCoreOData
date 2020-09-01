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
    public class FunctionImportSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_FunctionImport()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(functionImport: null, null), "functionImport");
        }

        [Fact]
        public void FunctionCommonProperties_ReturnsAsExpected()
        {
            // Assert
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive, false, null, false);
            function.AddParameter("name", primitive);
            function.AddParameter("title", primitive);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "MyFunctionImport", function);

            FunctionImportSegmentTemplate functionImportSegment = new FunctionImportSegmentTemplate(functionImport, null);

            // Act & Assert
            Assert.Equal("MyFunctionImport(name={name},title={title})", functionImportSegment.Literal);
            Assert.Equal(ODataSegmentKind.FunctionImport, functionImportSegment.Kind);
            Assert.True(functionImportSegment.IsSingle);
            Assert.Same(primitive.Definition, functionImportSegment.EdmType);
            Assert.Null(functionImportSegment.NavigationSource);
        }

        [Fact]
        public void Translate_ReturnsODataFunctionImportSegment()
        {
            // Arrange
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive, false, null, false);
            function.AddParameter("name", primitive);
            function.AddParameter("title", primitive);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "MyFunctionImport", function);
            FunctionImportSegmentTemplate template = new FunctionImportSegmentTemplate(functionImport, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            ODataPathSegment actual = template.Translate(context);

            // Assert
            Assert.NotNull(actual);
            OperationImportSegment functionImportSegment = Assert.IsType<OperationImportSegment>(actual);
            Assert.Same(function, functionImportSegment.OperationImports.First().Operation);
        }
    }
}
