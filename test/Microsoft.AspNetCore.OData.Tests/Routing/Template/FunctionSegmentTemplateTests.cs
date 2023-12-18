//-----------------------------------------------------------------------------
// <copyright file="FunctionSegmentTemplateTests.cs" company=".NET Foundation">
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
    public class FunctionSegmentTemplateTests
    {
        private static IEdmPrimitiveTypeReference IntType = EdmCoreModel.Instance.GetInt32(false);
        private static IEdmPrimitiveTypeReference StrType = EdmCoreModel.Instance.GetString(false);
        private EdmFunction _edmFunction;

        public FunctionSegmentTemplateTests()
        {
            _edmFunction = new EdmFunction("NS", "MyFunction", IntType, true, null, false);
            _edmFunction.AddParameter("bindingParameter", IntType);
            _edmFunction.AddParameter("name", StrType);
            _edmFunction.AddParameter("title", StrType);
        }

        [Fact]
        public void CtorFunctionSegmentTemplate_ThrowsArgumentNull_Function()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(function: null, null), "function");
        }

        [Fact]
        public void CtorFunctionSegmentTemplate_ThrowsArgumentNull_Parameters()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(null, null, navigationSource: null), "parameters");
        }

        [Fact]
        public void CtorFunctionSegmentTemplate_ThrowsArgumentNull_Function_InParametersCtor()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(new Dictionary<string, string>(), null, navigationSource: null), "function");
        }

        [Fact]
        public void CtorFunctionSegmentTemplate_ThrowsArgumentNull_OperationSegment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FunctionSegmentTemplate(operationSegment: null), "operationSegment");
        }

        [Fact]
        public void CtorFunctionSegmentTemplate_ThrowsArgument_NonboundFunction()
        {
            // Arrange
            IEdmPrimitiveTypeReference primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive);
            function.AddParameter("title", primitive);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => new FunctionSegmentTemplate(function, null),
                "The input operation 'MyFunction' is not a bound 'function'.");
        }

        [Fact]
        public void CtorFunctionSegmentTemplate_ThrowsODataException_NonFunction()
        {
            // Arrange
            IEdmPrimitiveTypeReference intPrimitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmAction action = new EdmAction("NS", "MyAction", intPrimitive);
            OperationSegment operationSegment = new OperationSegment(action, null);

            // Act
            Action test = () => new FunctionSegmentTemplate(operationSegment);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "The input segment should be 'Function' in 'FunctionSegmentTemplate'.");
        }

        [Fact]
        public void CtorFunctionSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            FunctionSegmentTemplate segment = new FunctionSegmentTemplate(_edmFunction, null);

            // Assert
            Assert.Same(_edmFunction, segment.Function);
            Assert.Null(segment.NavigationSource);
            Assert.Collection(segment.ParameterMappings,
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

            // Arrange
            OperationSegment operationSegment = new OperationSegment(_edmFunction, null);

            // Act
            segment = new FunctionSegmentTemplate(operationSegment);

            // Assert
            Assert.Same(_edmFunction, segment.Function);
            Assert.Null(segment.NavigationSource);
        }

        [Fact]
        public void GetTemplatesFunctionSegmentTemplate_ReturnsTemplates()
        {
            // Assert
            _edmFunction.AddOptionalParameter("option1", IntType);
            _edmFunction.AddOptionalParameter("option2", IntType);
            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "name", "{nameTemp}" },
                { "title", "{titleTemp}" },
                { "option2", "{option2Temp}" },
            };

            FunctionSegmentTemplate functionSegment = new FunctionSegmentTemplate(parameters, _edmFunction, null);

            // Act & Assert
            IEnumerable<string> templates = functionSegment.GetTemplates();
            Assert.Collection(templates,
                e =>
                {
                    Assert.Equal("/NS.MyFunction(name={nameTemp},title={titleTemp},option2={option2Temp})", e);
                },
                e =>
                {
                    Assert.Equal("/MyFunction(name={nameTemp},title={titleTemp},option2={option2Temp})", e);
                });

            // Act & Assert
            templates = functionSegment.GetTemplates(new ODataRouteOptions
            {
                EnableUnqualifiedOperationCall = false
            });

            string template = Assert.Single(templates);
            Assert.Equal("/NS.MyFunction(name={nameTemp},title={titleTemp},option2={option2Temp})", template);

            // Act & Assert
            templates = functionSegment.GetTemplates(new ODataRouteOptions
            {
                EnableQualifiedOperationCall = false
            });

            template = Assert.Single(templates);
            Assert.Equal("/MyFunction(name={nameTemp},title={titleTemp},option2={option2Temp})", template);
        }

        [Fact]
        public void GetTemplatesFunctionImportSegmentTemplate_ReturnsTemplates_ForEmptyParameter()
        {
            // Arrange
            EdmFunction function = new EdmFunction("NS", "MyFunction", IntType, true, null, false);
            function.AddParameter("bindingParameter", IntType);
            FunctionSegmentTemplate functionSegment = new FunctionSegmentTemplate(function, null);

            // Act & Assert
            IEnumerable<string> templates = functionSegment.GetTemplates();
            Assert.Collection(templates,
                e =>
                {
                    Assert.Equal("/NS.MyFunction()", e);
                },
                e =>
                {
                    Assert.Equal("/MyFunction()", e);
                });

            // Act & Assert
            templates = functionSegment.GetTemplates(new ODataRouteOptions
            {
                EnableNonParenthesisForEmptyParameterFunction = true,
                EnableQualifiedOperationCall = false
            });
            string template = Assert.Single(templates);
            Assert.Equal("/MyFunction", template);
        }

        [Fact]
        public void TryTranslateFunctionSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            FunctionSegmentTemplate template = new FunctionSegmentTemplate(_edmFunction, null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => template.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateFunctionImportSegmentTemplate_ReturnsTemplates_ForEmptyParameter()
        {
            // Arrange
            EdmModel edmModel = new EdmModel();
            EdmFunction function = new EdmFunction("NS", "MyFunction", IntType, true, null, false);
            function.AddParameter("bindingParameter", IntType);
            edmModel.AddElement(function);

            FunctionSegmentTemplate template = new FunctionSegmentTemplate(function, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = new RouteValueDictionary(),
                Model = edmModel
            };

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            OperationSegment functionSegment = Assert.IsType<OperationSegment>(actual);
            Assert.Same(function, functionSegment.Operations.First());
            Assert.Empty(functionSegment.Parameters);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryTranslateFunctionSegmentTemplate_ReturnsODataFunctionSegment(bool hasRouteData)
        {
            // Arrange
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive, true, null, false);
            function.AddParameter("bindingParameter", primitive);
            function.AddParameter("age", primitive);
            function.AddParameter("price", primitive);

            FunctionSegmentTemplate template = new FunctionSegmentTemplate(function, null);
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

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValue,
                Model = edmModel
            };

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

        [Fact]
        public void TryTranslateFunctionSegmentTemplate_ReturnsFalse_WithOptionalParameterMisMatch()
        {
            // Arrange
            EdmModel model = new EdmModel();
            _edmFunction.AddOptionalParameter("min", IntType);
            _edmFunction.AddOptionalParameter("max", IntType);
            model.AddElement(_edmFunction);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "name", "{name}" },
                { "title", "{title}" },
                { "min", "{min}" },
            };
            FunctionSegmentTemplate template = new FunctionSegmentTemplate(parameters, _edmFunction, null);

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
        public void TryTranslateFunctionSegmentTemplate_ReturnsODataFunctionSegment_WithOptionalParameters()
        {
            // Arrange
            EdmModel model = new EdmModel();
            _edmFunction.AddOptionalParameter("min", IntType);
            _edmFunction.AddOptionalParameter("max", IntType);
            model.AddElement(_edmFunction);

            IDictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "name", "{nameTemp}" },
                { "title", "{titleTemp}" },
                { "min", "{minTemp}" },
            };
            FunctionSegmentTemplate template = new FunctionSegmentTemplate(parameters, _edmFunction, null);

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
            OperationSegment functionSegment = Assert.IsType<OperationSegment>(actual);
            Assert.Same(_edmFunction, functionSegment.Operations.First());
            Assert.Equal(3, functionSegment.Parameters.Count());
            Assert.Collection(functionSegment.Parameters,
                e =>
                {
                    Assert.Equal("name", e.Name);
                    Assert.Equal("pt", e.Value);
                },
                e =>
                {
                    Assert.Equal("title", e.Name);
                    Assert.Equal("abc", e.Value);
                },
                e =>
                {
                    Assert.Equal("min", e.Name);
                    Assert.Equal(42, e.Value);
                });
        }

        [Fact]
        public void TryTranslateFunctionSegmentTemplate_ReturnsODataFunctionSegment_UsingEscapedString()
        {
            // Arrange
            var primitive = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", primitive, true, null, false);
            function.AddParameter("bindingParameter", primitive);
            function.AddParameter("name", primitive);

            FunctionSegmentTemplate template = new FunctionSegmentTemplate(function, null);
            EdmModel edmModel = new EdmModel();
            edmModel.AddElement(function);

            RouteValueDictionary routeValue = new RouteValueDictionary(new { name = "'Ji%2FChange%23%20T'" });

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValue,
                Model = edmModel
            };

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            OperationSegment functionSegment = Assert.IsType<OperationSegment>(actual);
            Assert.Same(function, functionSegment.Operations.First());
            var parameter = Assert.Single(functionSegment.Parameters);
            Assert.Equal("name", parameter.Name);
            Assert.Equal("Ji/Change%23%20T", parameter.Value.ToString());
        }

        [Fact]
        public void TryTranslateFunctionSegmentTemplate_ReturnsODataFunctionSegment_WithReturnedEntitySet()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>().Object;
            var endpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test");
            var routeValues = new RouteValueDictionary();

            var model = new EdmModel();
            var entityType = new EdmEntityType("NS", "Entity");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            model.AddElement(entityType);
            EdmFunction function = new EdmFunction("NS", "Function", new EdmEntityTypeReference(entityType, true), true, null, true);
            model.AddElement(function);
            var entityContainer = new EdmEntityContainer("NS", "Default");
            var entitySet = entityContainer.AddEntitySet("EntitySet", entityType);
            model.AddElement(entityContainer);
            model.SetAnnotationValue(function, new ReturnedEntitySetAnnotation("EntitySet"));

            var template = new FunctionSegmentTemplate(function, null);
            var translateContext = new ODataTemplateTranslateContext(httpContext, endpoint, routeValues, model);

            // Act
            bool ok = template.TryTranslate(translateContext);

            // Assert
            Assert.True(ok);
            var actual = Assert.Single(translateContext.Segments);
            var functionSegment = Assert.IsType<OperationSegment>(actual);
            Assert.Equal(functionSegment.EdmType, entityType);
            Assert.Equal(functionSegment.EntitySet, entitySet);
        }
    }
}
