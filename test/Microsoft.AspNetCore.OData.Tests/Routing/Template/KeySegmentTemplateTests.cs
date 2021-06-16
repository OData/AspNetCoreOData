// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class KeySegmentTemplateTests
    {
        private static EdmEntityType _customerType;
        private static EdmEntityContainer _container;
        private static EdmEntitySet _customers;

        static KeySegmentTemplateTests()
        {
            // EntityType
            _customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty idProperty = _customerType.AddStructuralProperty("customerId", EdmPrimitiveTypeKind.Int32);
            _customerType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            _customerType.AddKeys(idProperty);

            // EntitySet
            _container = new EdmEntityContainer("NS", "default");
            _customers = _container.AddEntitySet("Customers", _customerType);
        }

        [Fact]
        public void CtorKeySegmentTemplate_ThrowsArgumentNull_Keys()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(null, _customerType, _customers), "keys");
        }

        [Fact]
        public void CtorKeySegmentTemplate_ThrowsArgumentNull_EntityType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(new Dictionary<string, string>(), null, null), "entityType");
        }

        [Fact]
        public void CtorKeySegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CtorKeySegmentTemplate_ThrowsArgumentNull_Segment_AlternateKey()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(segment: null, keyProperties: null), "segment");
        }

        [Fact]
        public void CtorKeySegmentTemplate_ThrowsArgumentNull_KeyProperties_AlternateKey()
        {
            // Arrange
            KeySegment segment = new KeySegment(new Dictionary<string, object>(), _customerType, _customers);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new KeySegmentTemplate(segment, keyProperties: null), "keyProperties");
        }

        [Fact]
        public void CtorKeySegmentTemplate_ThrowsODataException_MismatchKey()
        {
            // Arrange
            Action test = () => new KeySegmentTemplate(new Dictionary<string, string>(), _customerType, _customers);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(test, "The input key count '0' doesn't match the number '1' of the key of entity type 'NS.Customer'.");
        }

        [Fact]
        public void CtorKeySegmentTemplate_SetsProperties()
        {
            // 1) Arrange & Act
            IDictionary<string, string> keys1 = new Dictionary<string, string>
            {
                { "customerId", "{key}"}
            };
            KeySegmentTemplate segment1 = new KeySegmentTemplate(keys1, _customerType, _customers);

            // 2) Arrange & Act
            IDictionary<string, object> keys2 = new Dictionary<string, object>
            {
                { "customerId", BuildExpression("{key}") }
            };
            KeySegment keySegment = new KeySegment(keys2, _customerType, _customers);
            KeySegmentTemplate segment2 = new KeySegmentTemplate(keySegment);

            // Assert
            foreach (var segment in new[] { segment1, segment2 })
            {
                KeyValuePair<string, string> keyMapping = Assert.Single(segment.KeyMappings);
                Assert.Equal("customerId", keyMapping.Key);
                Assert.Equal("key", keyMapping.Value);

                KeyValuePair<string, IEdmProperty> keyProperty = Assert.Single(segment.KeyProperties);
                Assert.Equal("customerId", keyProperty.Key);
                Assert.Equal("customerId", keyProperty.Value.Name);

                Assert.Same(_customers, segment.NavigationSource);
                Assert.Same(_customerType, segment.EntityType);
                Assert.Equal(1, segment.Count);
            }
        }

        [Fact]
        public void CtorKeySegmentTemplate_SetsProperties_ForCompositeKeys()
        {
            // Arrange & Act
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty firstProperty = customerType.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String);
            EdmStructuralProperty lastProperty = customerType.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String);
            customerType.AddKeys(firstProperty, lastProperty);
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "firstName", "{key1}" },
                { "lastName", "{key2}" }
            };

            KeySegment segment = new KeySegment(keys, customerType, null);
            KeySegmentTemplate template = new KeySegmentTemplate(segment);

            // Assert
            Assert.Collection(template.KeyMappings,
                e =>
                {
                    Assert.Equal("firstName", e.Key);
                    Assert.Equal("key1", e.Value);
                },
                e =>
                {
                    Assert.Equal("lastName", e.Key);
                    Assert.Equal("key2", e.Value);
                });
        }

        [Fact]
        public void CtorKeySegmentTemplate_ForAlternateKey_SetsProperties()
        {
            // Arrange & Act
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "name", BuildExpression("{nameValue}") }
            };
            KeySegment keySegment = new KeySegment(keys, _customerType, _customers);

            IEdmStructuralProperty nameProperty = _customerType.StructuralProperties().First(c => c.Name == "Name");
            IDictionary<string, IEdmProperty> properties = new Dictionary<string, IEdmProperty>
            {
                { "name", nameProperty}
            };
            KeySegmentTemplate segmentTemplate = new KeySegmentTemplate(keySegment, properties);

            // Assert
            KeyValuePair<string, string> keyMapping = Assert.Single(segmentTemplate.KeyMappings);
            Assert.Equal("name", keyMapping.Key);
            Assert.Equal("nameValue", keyMapping.Value);

            KeyValuePair<string, IEdmProperty> keyProperty = Assert.Single(segmentTemplate.KeyProperties);
            Assert.Equal("name", keyProperty.Key);
            Assert.Equal("Name", keyProperty.Value.Name);

            Assert.Same(_customers, segmentTemplate.NavigationSource);
            Assert.Same(_customerType, segmentTemplate.EntityType);
            Assert.Equal(1, segmentTemplate.Count);
        }

        [Fact]
        public void GetTemplatesKeySegmentTemplate_ReturnsTemplates()
        {
            // Arrange
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "customerId", "{key}" }
            };

            KeySegmentTemplate keySegment = new KeySegmentTemplate(keys, _customerType, _customers);

            // Act & Assert
            IEnumerable<string> templates = keySegment.GetTemplates();
            Assert.Collection(templates,
                e =>
                {
                    Assert.Equal("({key})", e);
                },
                e =>
                {
                    Assert.Equal("/{key}", e);
                });

            // Act & Assert
            templates = keySegment.GetTemplates(new ODataRouteOptions
            {
                EnableKeyAsSegment = false
            });

            string template = Assert.Single(templates);
            Assert.Equal("({key})", template);

            // Act & Assert
            templates = keySegment.GetTemplates(new ODataRouteOptions
            {
                EnableKeyInParenthesis = false
            });

            template = Assert.Single(templates);
            Assert.Equal("/{key}", template);
        }

        [Fact]
        public void GetTemplatesKeySegmentTemplate_ReturnsTemplates_ForCompositeKeys()
        {
            // Arrange
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty firstProperty = customerType.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String);
            EdmStructuralProperty lastProperty = customerType.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String);
            customerType.AddKeys(firstProperty, lastProperty);
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "firstName", "{key1}" },
                { "lastName", "{key2}" }
            };

            KeySegmentTemplate keySegment = new KeySegmentTemplate(keys, customerType, null);

            // Act & Assert
            IEnumerable<string> templates = keySegment.GetTemplates();
            Assert.Collection(templates,
                e =>
                {
                    Assert.Equal("(firstName={key1},lastName={key2})", e);
                },
                e =>
                {
                    Assert.Equal("/firstName={key1},lastName={key2}", e);
                });

            // Act & Assert
            templates = keySegment.GetTemplates(new ODataRouteOptions
            {
                EnableKeyAsSegment = false
            });

            string template = Assert.Single(templates);
            Assert.Equal("(firstName={key1},lastName={key2})", template);

            // Act & Assert
            templates = keySegment.GetTemplates(new ODataRouteOptions
            {
                EnableKeyInParenthesis = false
            });

            template = Assert.Single(templates);
            Assert.Equal("/firstName={key1},lastName={key2}", template);
        }

        [Fact]
        public void TryTranslateKeySegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "customerId", "{key}" }
            };
            KeySegmentTemplate keySegment = new KeySegmentTemplate(keys, _customerType, _customers);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => keySegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateKeySegmentTemplate_ReturnsODataKeySegment()
        {
            // Arrange
            EdmModel model = new EdmModel();
            model.AddElement(_customerType);
            model.AddElement(_container);

            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "customerId", "{key}" }
            };

            KeySegmentTemplate template = new KeySegmentTemplate(keys, _customerType, _customers);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = new RouteValueDictionary(new { key = "42" }),
                Model = model
            };

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            KeySegment keySegment = Assert.IsType<KeySegment>(actual);
            var actualKey = Assert.Single(keySegment.Keys);
            Assert.Equal("customerId", actualKey.Key);
            Assert.Equal(42, actualKey.Value);
        }

        [Fact]
        public void TryTranslateKeySegmentTemplate_ReturnsODataKeySegment_ForCompositeKey()
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
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { key1 = "'Peter'", key2 = "'Sam'" });

            KeySegmentTemplate template = new KeySegmentTemplate(keys, customerType, customers);

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary,
                Model = model
            };

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            KeySegment keySegment = Assert.IsType<KeySegment>(actual);
            Assert.Equal(2, keySegment.Keys.Count());
            Assert.Equal(new[] { "firstName", "lastName" }, keySegment.Keys.Select(c => c.Key));
            Assert.Equal(new[] { "Peter", "Sam" }, keySegment.Keys.Select(c => c.Value.ToString()));
        }

        [Fact]
        public void TryTranslateKeySegmentTemplate_ReturnsODataKeySegment_ForKeyAsSegment()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty idProperty = customerType.AddStructuralProperty("customerId", EdmPrimitiveTypeKind.String);
            customerType.AddKeys(idProperty);
            model.AddElement(customerType);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = container.AddEntitySet("Customers", customerType);
            model.AddElement(container);
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { key = "Peter" });
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "customerId", "{key}" }
            };
            KeySegmentTemplate template = new KeySegmentTemplate(keys, customerType, customers);

            RouteEndpoint endpoint = new RouteEndpoint(
                c => Task.CompletedTask,
                RoutePatternFactory.Parse("odata/customers/{key}/Name"),
                0,
                EndpointMetadataCollection.Empty,
                "test");

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary,
                Model = model,
                Endpoint = endpoint
            };

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            KeySegment keySegment = Assert.IsType<KeySegment>(actual);
            KeyValuePair<string, object> actualKeys = Assert.Single(keySegment.Keys);
            Assert.Equal("customerId", actualKeys.Key);
            Assert.Equal("Peter", actualKeys.Value);
        }

        [Fact]
        public void TryTranslateKeySegmentTemplate_ThrowsODataException_ForInvalidKey()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty idProperty = customerType.AddStructuralProperty("customerId", EdmPrimitiveTypeKind.Int32);
            customerType.AddKeys(idProperty);
            model.AddElement(customerType);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = container.AddEntitySet("Customers", customerType);
            model.AddElement(container);
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { key = "abc12" });
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "customerId", "{key}" }
            };
            KeySegmentTemplate template = new KeySegmentTemplate(keys, customerType, customers);

            RouteEndpoint endpoint = new RouteEndpoint(
                c => Task.CompletedTask,
                RoutePatternFactory.Parse("odata/customers/{key}/Name"),
                0,
                EndpointMetadataCollection.Empty,
                "test");

            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary,
                Model = model,
                Endpoint = endpoint
            };

            // Act
            Action test = () => template.TryTranslate(context);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "The key value (abc12) from request is not valid. The key value should be format of type 'Edm.Int32'.");
        }

        [Fact]
        public void TryTranslateKeySegmentTemplate_WorksWithKeyParametersAlias()
        {
            // Arrange
            IDictionary<string, string> keys = new Dictionary<string, string>
            {
                { "customerId", "{key}" }
            };
            KeySegmentTemplate template = new KeySegmentTemplate(keys, _customerType, _customers);

            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "key", "@p" }
            };
            EdmModel model = new EdmModel();
            model.AddElement(_customerType);
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?@p=42");
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                HttpContext = httpContext,
                RouteValues = routeValues,
                Model = model
            };

            // Act
            bool ok = template.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            KeySegment keySegment = Assert.IsType<KeySegment>(actual);
            KeyValuePair<string, object> actualKeys = Assert.Single(keySegment.Keys);
            Assert.Equal("customerId", actualKeys.Key);
            Assert.Equal(42, actualKeys.Value);
        }

        [Fact]
        public void CreateKeySegmentKeySegmentTemplate_ThrowsArgumentNull_EntityType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => KeySegmentTemplate.CreateKeySegment(null, null), "entityType");
        }

        [Fact]
        public void CreateKeySegmentKeySegmentTemplate_ReturnsKeySegmentTemplate()
        {
            // Arrange
            KeySegmentTemplate template = KeySegmentTemplate.CreateKeySegment(_customerType, _customers, "id");

            // Assert
            Assert.NotNull(template);
            Assert.Equal(1, template.Count);
            KeyValuePair<string, string> keyMapping = Assert.Single(template.KeyMappings);
            Assert.Equal("customerId", keyMapping.Key);
            Assert.Equal("id", keyMapping.Value);
        }

        [Fact]
        public void CreateKeySegmentKeySegmentTemplate_ReturnsKeySegmentTemplate_CompositeKey()
        {
            // Arrange
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty firstProperty = customerType.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String);
            EdmStructuralProperty lastProperty = customerType.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String);

            customerType.AddKeys(firstProperty, lastProperty);

            // Act
            KeySegmentTemplate template = KeySegmentTemplate.CreateKeySegment(customerType, null);

            // Assert
            Assert.NotNull(template);
            Assert.Equal(2, template.Count);
        }

        [Fact]
        public void BuildKeyMappingsKeySegmentTemplate_ThrowsODataException_NoProperty()
        {
            // Arrange
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "customerId", "{key}" }
            };

            IEdmStructuralProperty nameProperty = _customerType.StructuralProperties().First(c => c.Name == "Name");
            IDictionary<string, IEdmProperty> properties = new Dictionary<string, IEdmProperty>
            {
                { "name", nameProperty}
            };

            // Act
            Action test = () => KeySegmentTemplate.BuildKeyMappings(keys, _customerType, properties);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "Cannot find key 'customerId' in the 'NS.Customer' type.");
        }

        [Fact]
        public void BuildKeyMappingsKeySegmentTemplate_ThrowsODataException_InvalidTemplateLiteral()
        {
            // Arrange
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "customerId", "key" }
            };

            // Act
            Action test = () => KeySegmentTemplate.BuildKeyMappings(keys, _customerType);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "Key template value 'key' for key segment 'customerId' does not start with '{' or ends with '}'.");
        }

        [Fact]
        public void BuildKeyMappingsKeySegmentTemplate_ThrowsODataException_NullTemplateLiteral()
        {
            // Arrange
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "customerId", null }
            };

            // Act
            Action test = () => KeySegmentTemplate.BuildKeyMappings(keys, _customerType);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "Key template value '' for key segment 'customerId' does not start with '{' or ends with '}'.");
        }

        [Fact]
        public void BuildKeyMappingsKeySegmentTemplate_ThrowsODataException_EmptyTemplateLiteral()
        {
            // Arrange
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "customerId", "{}" }
            };

            // Act
            Action test = () => KeySegmentTemplate.BuildKeyMappings(keys, _customerType);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "Key template value '{}' for key segment 'customerId' is empty.");
        }

        [Fact]
        public void BuildKeyMappingsKeySegmentTemplate_ReturnsKeyMapping_NormalStringTemplate()
        {
            // Arrange
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty idProperty = customerType.AddStructuralProperty("customerId", EdmPrimitiveTypeKind.Int32);
            customerType.AddKeys(idProperty);
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "customerId", "{youkey}" }
            };

            // Act
            IDictionary<string, string> mapped = KeySegmentTemplate.BuildKeyMappings(keys, customerType);

            // Assert
            Assert.NotNull(mapped);
            KeyValuePair<string, string> actual = Assert.Single(mapped);
            Assert.Equal("customerId", actual.Key);
            Assert.Equal("youkey", actual.Value);
        }

        [Fact]
        public void BuildKeyMappingsKeySegmentTemplate_ReturnsKeyMapping_UriTemplateExpression()
        {
            // Arrange
            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            EdmStructuralProperty idProperty = customerType.AddStructuralProperty("customerId", EdmPrimitiveTypeKind.Int32);
            customerType.AddKeys(idProperty);

            UriTemplateExpression tempateExpression = BuildExpression("{yourId}", idProperty.Type);

            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "customerId", tempateExpression }
            };

            // Act
            IDictionary<string, string> mapped = KeySegmentTemplate.BuildKeyMappings(keys, customerType);

            // Assert
            Assert.NotNull(mapped);
            KeyValuePair<string, string> actual = Assert.Single(mapped);
            Assert.Equal("customerId", actual.Key);
            Assert.Equal("yourId", actual.Value);
        }

        internal static UriTemplateExpression BuildExpression(string value, object expectType = null)
        {
            // The properties of UriTemplateExpression are internal set.
            // We use the reflect to set the value.

            PropertyInfo literalText = typeof(UriTemplateExpression).GetProperty("LiteralText");
            Assert.NotNull(literalText);

            PropertyInfo expectedType = typeof(UriTemplateExpression).GetProperty("ExpectedType");
            Assert.NotNull(expectedType);

            UriTemplateExpression tempateExpression = new UriTemplateExpression();
            literalText.SetValue(tempateExpression, value);

            if (expectType != null)
            {
                expectedType.SetValue(tempateExpression, expectType);
            }

            return tempateExpression;
        }
    }
}
