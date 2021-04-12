// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
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
        public void Ctor_ThrowsArgumentNull_Parameters()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(null, null, navigationSource: null), "parameters");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Function()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(new Dictionary<string, string>(), null, navigationSource: null), "function");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_OperationSegment()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(operationSegment: null), "operationSegment");
        }

        //[Fact]
        //public void Ctor_ThrowsArgumentNull_RequiredParameters()
        //{
        //    // Assert
        //    IEdmFunction function = new Mock<IEdmFunction>().Object;

        //    // Act & Assert
        //    ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(function, null, null), "requiredParameters");
        //}

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
        public void CommonFunctionTemplateProperties_ReturnsAsExpected()
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
        public void CommonFunctionTemplateProperties_ReturnsAsExpected_WithParameters()
        {
            // Assert
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive, true, null, false);
            function.AddParameter("bindingParameter", primitive);
            function.AddParameter("name", primitive);
            function.AddParameter("title", primitive);
            function.AddOptionalParameter("option1", primitive);
            function.AddOptionalParameter("option2", primitive);
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "name", "{nameTemp}" },
                { "title", "{titleTemp}" },
                { "option2", "{option2Temp}" },
            };

            FunctionSegmentTemplate functionSegment = new FunctionSegmentTemplate(parameters, function, null);

            // Act & Assert
            Assert.Equal("NS.MyFunction(name={nameTemp},title={titleTemp},option2={option2Temp})", functionSegment.Literal);
            Assert.Equal(ODataSegmentKind.Function, functionSegment.Kind);
            Assert.True(functionSegment.IsSingle);
            Assert.Same(primitive.Definition, functionSegment.EdmType);
            Assert.Null(functionSegment.NavigationSource);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryTranslate_ReturnsODataFunctionSegment(bool hasRouteData)
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
            bool ok = template.TryTranslate(context);

            // Assert
            if (hasRouteData)
            {
                Assert.True(ok);
                ODataPathSegment actual = Assert.Single(context.Segments);
                OperationSegment functionSegment = Assert.IsType<OperationSegment>(actual);
                Assert.Same(function, functionSegment.Operations.First());
                Assert.Equal(2, functionSegment.Parameters.Count());
                Assert.Equal(new[] { "age", "price" }, functionSegment.Parameters.Select(p => p.Name));
            }
            else
            {
                Assert.False(ok);
                Assert.Empty(context.Segments);
            }
        }
    }
}
