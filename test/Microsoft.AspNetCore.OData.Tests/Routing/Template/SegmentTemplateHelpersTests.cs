// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class SegmentTemplateHelpersTests
    {
        private static IEdmPrimitiveTypeReference _IntType = EdmCoreModel.Instance.GetInt32(false);
        private static IEdmPrimitiveTypeReference _StrType = EdmCoreModel.Instance.GetString(false);

        [Fact]
        public void MatchForFunction_ThrowsODataException_ForInvalidUriValue()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmFunction function = new EdmFunction("NS", "MyFunction", _IntType, true, null, false);
            function.AddParameter("data", _IntType);
            model.AddElement(function);

            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "dataValue", "ef12abc" }
            };

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValues,
                Model = model
            };

            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "data", "dataValue" }
            };

            // Act
            Action test = () => SegmentTemplateHelpers.Match(context, function, parameterMappings);
            ExceptionAssert.Throws<ODataException>(test,
                "The parameter value (ef12abc) from request is not valid. The parameter value should be format of type 'Edm.Int32'.");
        }

        [Fact]
        public void MatchForFunction_ReturnsBuiltParameters()
        {
            // Arrange
            EdmComplexType complex = new EdmComplexType("NS", "Address");
            complex.AddStructuralProperty("street", _StrType);

            EdmModel model = new EdmModel();
            EdmFunction function = new EdmFunction("NS", "MyFunction", _IntType, true, null, false);
            function.AddParameter("name", _StrType);
            function.AddParameter("title", _IntType);
            function.AddOptionalParameter("address", new EdmComplexTypeReference(complex, false));
            model.AddElement(function);

            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "nameValue", "'abc'" },
                { "titleValue", "10001" },
                { "addressValue", "{\"street\":\"efg\" }" }
            };

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValues,
                Model = model
            };

            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "name", "nameValue" },
                { "title", "titleValue" },
                { "address", "addressValue" },
            };

            // Act
            IList<OperationSegmentParameter> parameters = SegmentTemplateHelpers.Match(context, function, parameterMappings);

            // Assert
            Assert.Collection(parameters,
                e =>
                {
                    Assert.Equal("name", e.Name);
                    Assert.Equal("abc", e.Value);
                },
                e =>
                {
                    Assert.Equal("title", e.Name);
                    Assert.Equal(10001, e.Value);
                },
                e =>
                {
                    Assert.Equal("address", e.Name);
                    Assert.Equal("{\"street\":\"efg\" }", e.Value);
                });
        }

        [Theory]
        [InlineData("'Green'")]
        [InlineData("NS.Color'Green'")]
        public void MatchForFunction_ReturnsBuiltEnumParameters(string enumValue)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEnumType enumType = new EdmEnumType("NS", "Color");
            enumType.AddMember("Red", new EdmEnumMemberValue(1));
            enumType.AddMember("Green", new EdmEnumMemberValue(2));
            model.AddElement(enumType);

            var intType = EdmCoreModel.Instance.GetInt32(false);
            EdmFunction function = new EdmFunction("NS", "MyFunction", intType);
            function.AddParameter("favoriateColor", new EdmEnumTypeReference(enumType, false));
            model.AddElement(function);

            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "favoriateColorValue", $"{enumValue}" }
            };

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValues,
                Model = model
            };

            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "favoriateColor", "favoriateColorValue" }
            };

            // Act
            IList<OperationSegmentParameter> parameters = SegmentTemplateHelpers.Match(context, function, parameterMappings);

            // Assert
            OperationSegmentParameter operationParameter = Assert.Single(parameters);
            Assert.Equal("favoriateColor", operationParameter.Name);
            ODataEnumValue oDataEnumValue = Assert.IsType<ODataEnumValue>(operationParameter.Value);
            Assert.Equal("2", oDataEnumValue.Value);
        }

        [Fact]
        public void MatchForFunction_ReturnsBuiltParameters_ParameterAlias()
        {
            // Arrange
            var strType = EdmCoreModel.Instance.GetString(false);
            EdmModel model = new EdmModel();
            EdmFunction function = new EdmFunction("NS", "MyFunction", strType, true, null, false);
            function.AddParameter("name", strType);
            model.AddElement(function);

            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "nameValue", "@p" }
            };

            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?@p='abc'");
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                HttpContext = httpContext,
                RouteValues = routeValues,
                Model = model
            };

            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "name", "nameValue" }
            };

            // Act
            IList<OperationSegmentParameter> parameters = SegmentTemplateHelpers.Match(context, function, parameterMappings);

            // Assert
            OperationSegmentParameter functionParameter = Assert.Single(parameters);
            Assert.Equal("name", functionParameter.Name);
            Assert.Equal("abc", functionParameter.Value);
        }

        [Fact]
        public void IsMatchParameters_ReturnsCorrect_ForDifferentParameters()
        {
            // Arrange
            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "p1", "a" },
                { "p2", "b,p3=c" },
            };

            // 1) Act & Assert
            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "p1", "p1" },
                { "p2", "p2" },
            };
            Assert.False(SegmentTemplateHelpers.IsMatchParameters(routeValues, parameterMappings));

            // 2) Act & Assert
            parameterMappings = new Dictionary<string, string>
            {
                { "p1", "p1" },
                { "p2", "p2" },
                { "p3", "p3" },
            };

            Assert.True(SegmentTemplateHelpers.IsMatchParameters(routeValues, parameterMappings));

            // 3) Act & Assert
            parameterMappings = new Dictionary<string, string>
            {
                { "Name", "name" }
            };
            Assert.False(SegmentTemplateHelpers.IsMatchParameters(routeValues, parameterMappings));

            // 4) Act & Assert
            parameterMappings = new Dictionary<string, string>();
            Assert.True(SegmentTemplateHelpers.IsMatchParameters(routeValues, parameterMappings));
        }

        [Fact]
        public void IsMatchParameters_ReturnsCorrect_ForWrongParameterFormatter()
        {
            // Arrange
            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "p1", "b,p2===c" },
            };

            // Act
            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "p1", "p1" },
                { "p2", "p2" },
            };

            // Assert
            Assert.False(SegmentTemplateHelpers.IsMatchParameters(routeValues, parameterMappings));
        }
    }
}
