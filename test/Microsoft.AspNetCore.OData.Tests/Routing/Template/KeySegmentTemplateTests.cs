// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class KeySegmentTemplateTests
    {
        private static IEdmEntityType _customerType;
        private static IEdmEntityContainer _container;
        private static IEdmEntitySet _customers;

        static KeySegmentTemplateTests()
        {
            // EntityType
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty idProperty = customerType.AddStructuralProperty("customerId", EdmPrimitiveTypeKind.Int32);
            customerType.AddKeys(idProperty);
            _customerType = customerType;

            // EntitySet
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            _customers = container.AddEntitySet("Customers", customerType);
            _container = container;
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Keys()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(null, _customerType, _customers), "keys");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityType()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(new Dictionary<string, string>(), null, null), "entityType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Segment()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CommonKeySegmentTemplateProperties_ReturnsAsExpected()
        {
            // Assert
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "customerId", "{key}" }
            };

            // Act
            KeySegmentTemplate template = new KeySegmentTemplate(keys, _customerType, _customers);

            // Assert
            Assert.Equal(ODataSegmentKind.Key, template.Kind);
            Assert.Equal("{key}", template.Literal);
            Assert.True(template.IsSingle);
            Assert.Same(_customerType, template.EdmType);
            Assert.Same(_customers, template.NavigationSource);
        }

        [Fact]
        public void CommonKeySegmentTemplateProperties_ReturnsAsExpected_ForCompositeKeys()
        {
            // Assert
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty firstProperty = customerType.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String);
            EdmStructuralProperty lastProperty = customerType.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String);
            customerType.AddKeys(firstProperty, lastProperty);
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "firstName", "{key1}" },
                { "lastName", "{key2}" }
            };

            // Act
            KeySegmentTemplate template = new KeySegmentTemplate(keys, customerType, null);

            // Assert
            Assert.Equal(ODataSegmentKind.Key, template.Kind);
            Assert.Equal("firstName={key1},lastName={key2}", template.Literal);
            Assert.True(template.IsSingle);
            Assert.Same(customerType, template.EdmType);
            Assert.Null(template.NavigationSource);
        }

        [Fact]
        public void Translate_ReturnsODataKeySegment()
        {
            // Arrange
            EdmModel model = new EdmModel();
            model.AddElement(_customerType);
            model.AddElement(_container);

            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "customerId", "{key}" }
            };

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { key = "42" });

            KeySegmentTemplate template = new KeySegmentTemplate(keys, _customerType, _customers);

            HttpContext httpContext = new DefaultHttpContext();
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext(httpContext, routeValueDictionary, model);

            // Act
            ODataPathSegment actual = template.Translate(context);

            // Assert
            Assert.NotNull(actual);
            KeySegment keySegment = Assert.IsType<KeySegment>(actual);
            var actualKey = Assert.Single(keySegment.Keys);
            Assert.Equal("customerId", actualKey.Key);
            Assert.Equal(42, actualKey.Value);
        }

        [Fact]
        public void Translate_ReturnsODataKeySegment_ForCompositeKey()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty firstProperty = customerType.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String);
            EdmStructuralProperty lastProperty = customerType.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String);
            customerType.AddKeys(firstProperty, lastProperty);
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "firstName", "{key1}" },
                { "lastName", "{key2}" }
            };
            model.AddElement(customerType);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = container.AddEntitySet("Customers", customerType);
            model.AddElement(container);

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { key1 = "'Peter'", key2="'Sam'" });

            KeySegmentTemplate template = new KeySegmentTemplate(keys, customerType, customers);

            HttpContext httpContext = new DefaultHttpContext();
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext(httpContext, routeValueDictionary, model);

            // Act
            ODataPathSegment actual = template.Translate(context);

            // Assert
            Assert.NotNull(actual);
            KeySegment keySegment = Assert.IsType<KeySegment>(actual);
            Assert.Equal(2, keySegment.Keys.Count());
            Assert.Equal(new[] { "firstName", "lastName" }, keySegment.Keys.Select(c => c.Key));
            Assert.Equal(new[] { "Peter", "Sam" }, keySegment.Keys.Select(c => c.Value.ToString()));
        }

        [Fact]
        public void CreateKeySegment_ReturnsKeySegementTemplate()
        {
            // Arrange
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty firstProperty = customerType.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String);
            EdmStructuralProperty lastProperty = customerType.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String);

            customerType.AddKeys(firstProperty, lastProperty);

            // Arrange
            KeySegmentTemplate template = KeySegmentTemplate.CreateKeySegment(customerType, null);

            // Assert
            Assert.NotNull(template);
            Assert.Equal(2, template.Count);
            Assert.Equal("firstName={keyfirstName},lastName={keylastName}", template.Literal);
        }

        [Fact]
        public void BuildKeyMappings_ReturnsKeyMapping_NormalStringTemplate()
        {
            // Arrange
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty idProperty = customerType.AddStructuralProperty("customerId", EdmPrimitiveTypeKind.Int32);
            customerType.AddKeys(idProperty);
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "customerId", "{youkey}" }
            };

            // Arrange
            IDictionary<string, string> mapped = KeySegmentTemplate.BuildKeyMappings(keys, customerType);

            // Assert
            Assert.NotNull(mapped);
            KeyValuePair<string, string> actual = Assert.Single(mapped);
            Assert.Equal("customerId", actual.Key);
            Assert.Equal("youkey", actual.Value);
        }

        [Fact]
        public void BuildKeyMappings_ReturnsKeyMapping_UriTemplateExpression()
        {
            // Arrange
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty idProperty = customerType.AddStructuralProperty("customerId", EdmPrimitiveTypeKind.Int32);
            customerType.AddKeys(idProperty);

            // The properties of UriTemplateExpression are internal set.
            // We use the reflect to set the value.
            PropertyInfo literalText = typeof(UriTemplateExpression).GetProperty("LiteralText");
            Assert.NotNull(literalText);

            PropertyInfo expectedType = typeof(UriTemplateExpression).GetProperty("ExpectedType");
            Assert.NotNull(expectedType);

            UriTemplateExpression tempateExpression = new UriTemplateExpression();
            literalText.SetValue(tempateExpression, "{yourId}");
            expectedType.SetValue(tempateExpression, idProperty.Type);

            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "customerId", tempateExpression }
            };

            // Arrange
            IDictionary<string, string> mapped = KeySegmentTemplate.BuildKeyMappings(keys, customerType);

            // Assert
            Assert.NotNull(mapped);
            KeyValuePair<string, string> actual = Assert.Single(mapped);
            Assert.Equal("customerId", actual.Key);
            Assert.Equal("yourId", actual.Value);
        }
    }
}
