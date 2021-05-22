// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class DefaultODataTemplateTranslatorTests
    {
        [Fact]
        public void TranslateODataPathTemplate_ThrowsArgumentNull_Path()
        {
            // Arrange
            DefaultODataTemplateTranslator translator = new DefaultODataTemplateTranslator();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => translator.Translate(null, null), "path");
        }

        [Fact]
        public void TranslateODataPathTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            DefaultODataTemplateTranslator translator = new DefaultODataTemplateTranslator();
            ODataPathTemplate template = new ODataPathTemplate();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => translator.Translate(template, null), "context");
        }

        [Fact]
        public void TranslateODataPathTemplate_ToODataPath()
        {
            // Assert
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("CustomerId", EdmPrimitiveTypeKind.Int32));
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            var entitySet = container.AddEntitySet("Customers", customer);
            EdmModel model = new EdmModel();
            model.AddElement(customer);
            model.AddElement(container);

            ODataPathTemplate template = new ODataPathTemplate(
                new EntitySetSegmentTemplate(entitySet),
                KeySegmentTemplate.CreateKeySegment(customer, entitySet));

            DefaultODataTemplateTranslator translator = new DefaultODataTemplateTranslator();
            RouteValueDictionary routeValues = new RouteValueDictionary(new { key = "42" });
            HttpContext httpContext = new DefaultHttpContext();
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext(httpContext, routeValues, model);

            // Act
            ODataPath path = translator.Translate(template, context);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(2, path.Count);
            EntitySetSegment entitySetSegment = Assert.IsType<EntitySetSegment>(path.FirstSegment);
            Assert.Equal("Customers", entitySetSegment.EntitySet.Name);

            KeySegment keySegment = Assert.IsType<KeySegment>(path.LastSegment);
            KeyValuePair<string, object> key = Assert.Single(keySegment.Keys);
            Assert.Equal("CustomerId", key.Key);
            Assert.Equal(42, key.Value);
        }
    }
}
