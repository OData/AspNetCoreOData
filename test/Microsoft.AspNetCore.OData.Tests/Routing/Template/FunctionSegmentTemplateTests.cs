// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class FunctionSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Function()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(function: null, null), "action");
        }

        [Fact]
        public void Ctor_ThrowsArgument_NonboundFunction()
        {
            // Assert
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive);
            function.AddParameter("title", primitive);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => new FunctionSegmentTemplate(function, null),
                "The input function 'MyFunction' is not a bound function.");
        }

        [Fact]
        public void FunctionCommonProperties_ReturnsAsExpected()
        {
            // Assert
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive, true, null, false);
            function.AddParameter("bindingParameter", primitive);
            function.AddParameter("name", primitive);
            function.AddParameter("title", primitive);

            FunctionSegmentTemplate functionSegment = new FunctionSegmentTemplate(function, null);

            // Act & Assert
            Assert.Equal("NS.MyFunction(name={name},title={title})", functionSegment.Literal);
            Assert.Equal(ODataSegmentKind.Function, functionSegment.Kind);
            Assert.True(functionSegment.IsSingle);
            Assert.Same(primitive.Definition, functionSegment.EdmType);
            Assert.Null(functionSegment.NavigationSource);
        }

        [Fact]
        public void Translate_ReturnsODataFunctionSegment()
        {
            // Arrange
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive, true, null, false);
            function.AddParameter("bindingParameter", primitive);
            function.AddParameter("name", primitive);
            function.AddParameter("title", primitive);
            FunctionSegmentTemplate template = new FunctionSegmentTemplate(function, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            ODataPathSegment actual = template.Translate(context);

            // Assert
            Assert.NotNull(actual);
            OperationSegment functionSegment = Assert.IsType<OperationSegment>(actual);
            Assert.Same(function, functionSegment.Operations.First());
        }
    }
}
