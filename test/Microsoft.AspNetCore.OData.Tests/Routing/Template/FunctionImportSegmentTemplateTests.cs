// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class FunctionImportSegmentTemplateTests
    {
        private static IEdmPrimitiveTypeReference IntPrimitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
        private static IEdmPrimitiveTypeReference StringPrimitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, false);

        private EdmFunctionImport _functionImport;
        private EdmFunction _function;
        private EdmEntityContainer _container;

        public FunctionImportSegmentTemplateTests()
        {
            _function = new EdmFunction("NS", "MyFunctionImport", IntPrimitive, false, null, false);
            _function.AddParameter("name", StringPrimitive);
            _function.AddParameter("title", StringPrimitive);
            _container = new EdmEntityContainer("NS", "Default");
            _functionImport = new EdmFunctionImport(_container, "MyFunctionImport", _function);
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_FunctionImport()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(functionImport: null, null), "functionImport");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Parameters()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(parameters: null, null, null), "parameters");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_FunctionImport_InParametersCtor()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(new Dictionary<string, string>(), null, null), "functionImport");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Segment()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CommonFunctionImportTemplateProperties_ReturnsAsExpected()
        {
            // Assert
            FunctionImportSegmentTemplate functionImportSegment = new FunctionImportSegmentTemplate(_functionImport, null);

            // Act & Assert
            Assert.Equal("MyFunctionImport(name={name},title={title})", functionImportSegment.Literal);
            Assert.Equal(ODataSegmentKind.FunctionImport, functionImportSegment.Kind);
            Assert.True(functionImportSegment.IsSingle);
            Assert.Same(IntPrimitive.Definition, functionImportSegment.EdmType);
            Assert.Null(functionImportSegment.NavigationSource);
        }

        [Fact]
        public void TryTranslate_ReturnsODataFunctionImportSegment()
        {
            // Arrange
            EdmFunction function = new EdmFunction("NS", "MyFunctionImport", IntPrimitive, false, null, false);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "MyFunctionImport", function);

            FunctionImportSegmentTemplate template = new FunctionImportSegmentTemplate(functionImport, null);

            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            Mock<IEdmModel> edmModel = new Mock<IEdmModel>();
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext(httpContext.Object,
                new RouteValueDictionary(), edmModel.Object);

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            OperationImportSegment functionImportSegment = Assert.IsType<OperationImportSegment>(actual);
            Assert.Same(function, functionImportSegment.OperationImports.First().Operation);
        }

        [Fact]
        public void TryTranslateFunctionImportSegmentTemplate_ReturnsODataFunctionImportSegment_WithOptionalParameter()
        {
            // Arrange
            _function.AddOptionalParameter("min", IntPrimitive);
            _function.AddOptionalParameter("max", IntPrimitive);
            EdmModel model = new EdmModel();
            model.AddElement(_function);
            model.AddElement(_container);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "name", "{nameTemp}" },
                { "title", "{titleTemp}" },
                { "min", "{minTemp}" },
            };
            FunctionImportSegmentTemplate template = new FunctionImportSegmentTemplate(parameters, _functionImport, null);

            RouteValueDictionary routeValues = new RouteValueDictionary(new { nameTemp = "'pt'", titleTemp = "'abc'", minTemp = "42" });

            HttpContext httpContext = new DefaultHttpContext();
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext(httpContext, routeValues, model);

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            OperationImportSegment functionImportSegment = Assert.IsType<OperationImportSegment>(actual);
            Assert.Same(_function, functionImportSegment.OperationImports.First().Operation);
            Assert.Equal(3, functionImportSegment.Parameters.Count());
            Assert.Equal(new[] { "name", "title", "min" }, functionImportSegment.Parameters.Select(p => p.Name));

            Assert.Equal("pt", context.UpdatedValues["nameTemp"]);
            Assert.Equal("abc", context.UpdatedValues["titleTemp"]);
            Assert.Equal(42, context.UpdatedValues["minTemp"]);
        }

        [Fact]
        public void TryTranslateFunctionImportSegmentTemplate_ReturnsNull_WithOptionalParameterMisMatch()
        {
            // Arrange
            _function.AddOptionalParameter("min", IntPrimitive);
            _function.AddOptionalParameter("max", IntPrimitive);
            EdmModel model = new EdmModel();
            model.AddElement(_function);
            model.AddElement(_container);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "name", "{nameTemp}" },
                { "title", "{titleTemp}" },
                { "min", "{minTemp}" },
            };
            FunctionImportSegmentTemplate template = new FunctionImportSegmentTemplate(parameters, _functionImport, null);

            RouteValueDictionary routeValues = new RouteValueDictionary(new { name = "'pt'", title = "'abc'", min = "42,max=5" });
            HttpContext httpContext = new DefaultHttpContext();
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext(httpContext, routeValues, model);

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.False(ok);
            Assert.Empty(context.Segments);
        }
    }
}
