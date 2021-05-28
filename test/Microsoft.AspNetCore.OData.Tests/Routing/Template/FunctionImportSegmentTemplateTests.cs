// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing;
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
        public void CtorFunctionImportSegmentTemplate_ThrowsArgumentNull_FunctionImport()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(functionImport: null, null), "functionImport");
        }

        [Fact]
        public void CtorFunctionImportSegmentTemplate_ThrowsArgumentNull_Parameters()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(parameters: null, null, null), "parameters");
        }

        [Fact]
        public void CtorFunctionImportSegmentTemplate_ThrowsArgumentNull_FunctionImport_InParametersCtor()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(new Dictionary<string, string>(), null, null), "functionImport");
        }

        [Fact]
        public void CtorFunctionImportSegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionImportSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CtorFunctionImportSegmentTemplate_ThrowsException_NonFunctionImport()
        {
            // Arrange
            EdmAction action = new EdmAction("NS", "MyAction", null);
            Mock<IEdmActionImport> import = new Mock<IEdmActionImport>();
            import.Setup(i => i.Name).Returns("any");
            import.Setup(i => i.ContainerElementKind).Returns(EdmContainerElementKind.ActionImport);
            import.Setup(i => i.Operation).Returns(action);
            OperationImportSegment operationImportSegment = new OperationImportSegment(import.Object, null);

            // Act
            Action test = () => new FunctionImportSegmentTemplate(operationImportSegment);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "The input segment should be 'FunctionImport' in 'FunctionImportSegmentTemplate'.");
        }

        [Fact]
        public void CtorFunctionImportSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            FunctionImportSegmentTemplate functionImportSegment = new FunctionImportSegmentTemplate(_functionImport, null);

            // Assert
            Assert.Same(_functionImport, functionImportSegment.FunctionImport);
            Assert.Null(functionImportSegment.NavigationSource);
            Assert.NotNull(functionImportSegment.ParameterMappings);
            Assert.Collection(functionImportSegment.ParameterMappings,
                e =>
                {
                    Assert.Equal("name", e.Key);
                    Assert.Equal("name", e.Value);
                },
                e =>
                {
                    Assert.Equal("title", e.Key);
                    Assert.Equal("title", e.Value);
                });

            // Arrange & Act
            OperationImportSegment importSegment = new OperationImportSegment(_functionImport, null);
            functionImportSegment = new FunctionImportSegmentTemplate(importSegment);
            Assert.Empty(functionImportSegment.ParameterMappings);
        }

        [Fact]
        public void GetTemplatesFunctionImportSegmentTemplate_ReturnsTemplates()
        {
            // Arrange
            FunctionImportSegmentTemplate functionImportSegment = new FunctionImportSegmentTemplate(_functionImport, null);

            // Act & Assert
            IEnumerable<string> templates = functionImportSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/MyFunctionImport(name={name},title={title})", template);
            Assert.Null(functionImportSegment.NavigationSource);
        }

        [Fact]
        public void GetTemplatesFunctionImportSegmentTemplate_ReturnsTemplates_ForEmptyParameter()
        {
            // Arrange
            EdmFunction function = new EdmFunction("NS", "MyFunctionImport", IntPrimitive, false, null, false);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "MyFunctionImport", function);
            FunctionImportSegmentTemplate functionImportSegment = new FunctionImportSegmentTemplate(functionImport, null);

            // Act & Assert
            IEnumerable<string> templates = functionImportSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/MyFunctionImport()", template);

            // Act & Assert
            templates = functionImportSegment.GetTemplates(new ODataRouteOptions
            {
                EnableNonParenthsisForEmptyParameterFunction = true
            });
            template = Assert.Single(templates);
            Assert.Equal("/MyFunctionImport", template);
        }

        [Fact]
        public void TryTranslateActionImportSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            FunctionImportSegmentTemplate functionImportSegment = new FunctionImportSegmentTemplate(_functionImport, null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => functionImportSegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateFunctionImportSegmentTemplate_ReturnsODataFunctionImportSegment()
        {
            // Arrange
            EdmFunction function = new EdmFunction("NS", "MyFunctionImport", IntPrimitive, false, null, false);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "MyFunctionImport", function);

            FunctionImportSegmentTemplate template = new FunctionImportSegmentTemplate(functionImport, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = new RouteValueDictionary()
            };

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
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValues,
                Model = model
            };

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
        public void TryTranslateFunctionImportSegmentTemplate_ReturnsFalse_WithOptionalParameterMisMatch()
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
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValues,
                Model = model
            };

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.False(ok);
            Assert.Empty(context.Segments);
        }

        [Fact]
        public void TryTranslateFunctionImportSegmentTemplate_ReturnsFalse_WithMisMatchParameter()
        {
            // Arrange
            EdmModel model = new EdmModel();
            model.AddElement(_function);
            model.AddElement(_container);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "name", "{nameTemp}" },
                { "title", "{titleTemp}" },
            };
            FunctionImportSegmentTemplate template = new FunctionImportSegmentTemplate(parameters, _functionImport, null);

            RouteValueDictionary routeValues = new RouteValueDictionary(new { name = "'pt'" });
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValues,
                Model = model
            };

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.False(ok);
            Assert.Empty(context.Segments);
        }
    }
}
