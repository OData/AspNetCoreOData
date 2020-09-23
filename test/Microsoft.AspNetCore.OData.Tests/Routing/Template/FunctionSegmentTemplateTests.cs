// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class FunctionSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Function()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(function: null, null), "function");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_RequiredParameters()
        {
            // Assert
            IEdmFunction function = new Mock<IEdmFunction>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(function, null, null), "requiredParameters");
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Translate_ReturnsODataFunctionSegment(bool hasRouteData)
        {
            // Arrange
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive, true, null, false);
            function.AddParameter("bindingParameter", primitive);
            function.AddParameter("age", primitive);
            function.AddParameter("price", primitive);

            FunctionSegmentTemplate template = new FunctionSegmentTemplate(function, null);
            HttpContext httpContext = new Mock<HttpContext>().Object;
            EdmModel edmModel = new EdmModel();
            edmModel.AddElement(function);

            RouteValueDictionary routeValue;
            if (hasRouteData)
            {
                routeValue = new RouteValueDictionary(new { age = "34", price = "9" });
            }
            else
            {
                routeValue = new RouteValueDictionary(); // Empty
            }

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext(httpContext, routeValue, edmModel);

            // Act
            ODataPathSegment actual = template.Translate(context);

            // Assert
            if (hasRouteData)
            {
                Assert.NotNull(actual);
                OperationSegment functionSegment = Assert.IsType<OperationSegment>(actual);
                Assert.Same(function, functionSegment.Operations.First());
                Assert.Equal(2, functionSegment.Parameters.Count());
                Assert.Equal(new[] { "age", "price" }, functionSegment.Parameters.Select(p => p.Name));
            }
            else
            {
                Assert.Null(actual);
            }
        }
    }
}
