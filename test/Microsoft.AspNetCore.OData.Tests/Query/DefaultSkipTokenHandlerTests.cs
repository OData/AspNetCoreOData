// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class DefaultSkipTokenHandlerTests
    {
        [Theory]
        [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skip=10")]
        [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skip=10")]
        public void GetNextPageLink_ReturnsCorrectNextLink(string baseUri, string expectedUri)
        {
            // Arrange
            var context = GetContext(false);
            var nextLinkGenerator = context.QueryContext.GetSkipTokenHandler();

            // Act
            var uri = nextLinkGenerator.GenerateNextPageLink(new Uri(baseUri), 10, null, context);
            var actualUri = uri.ToString();

            // Assert
            Assert.Equal(expectedUri, actualUri);
        }

        private ODataSerializerContext GetContext(bool enableSkipToken = false)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmEntitySet entitySet = model.Customers;
            IEdmEntityType entityType = entitySet.EntityType();
            IEdmProperty edmProperty = entityType.FindProperty("Name");
            IEdmType edmType = entitySet.Type;
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, edmType, path);
            queryContext.DefaultQuerySettings.EnableSkipToken = enableSkipToken;

            var request = RequestFactory.Create(opt => opt.AddModel(model.Model));
            ResourceContext resource = new ResourceContext();
            ODataSerializerContext context = new ODataSerializerContext(resource, edmProperty, queryContext, null);
            return context;
        }
    }
}