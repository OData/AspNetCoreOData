//-----------------------------------------------------------------------------
// <copyright file="QueryOptionsBindingExtensionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Extension;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class QueryOptionsBindingExtensionTests
    {

        [Fact]
        public void ODataQueryOptions_ApplyCustom_RequestContainerIsNull()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers");
            var context = new ODataQueryContext(CreateModel(), typeof(Customer));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
            context.RequestContainer = null;
            ODataQuerySettings querySettings = context.GetODataQuerySettings();
            var customers = CreateTestQueryable();

            // Act
            var result = queryOptions.ApplyCustom(customers, querySettings);

            // Arrange
            Assert.Equal(customers, result);
        }

        [Fact]
        public void ODataQueryOptions_ApplyCustom_NoExtensionHandlerProvided()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers");
            var context = new ODataQueryContext(CreateModel(), typeof(Customer));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
            ODataQuerySettings querySettings = context.GetODataQuerySettings();
            var customers = CreateTestQueryable();

            // Act
            var result = queryOptions.ApplyCustom(customers, querySettings);

            // Arrange
            Assert.Equal(customers, result);
        }

        [Fact]
        public void ODataQueryOptions_ApplyCustom_WithExtensionHandlerProvided()
        {
            // Arrange
            IServiceProvider serviceProvider = new MockServiceProvider((builder) =>
            {
                builder.AddService(ServiceLifetime.Singleton
                    ,typeof(IODataQueryOptionsBindingExtension)
                    ,typeof(MockODataQueryOptionsBindingExtension));
            });

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers");
            var context = new ODataQueryContext(CreateModel(), typeof(Customer));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
            context.RequestContainer = serviceProvider;
            ODataQuerySettings querySettings = context.GetODataQuerySettings();
            var customers = CreateTestQueryable();

            var expected = ApplyTestExpressions(customers,querySettings);

            // Act
            var result = queryOptions.ApplyCustom(customers, querySettings);

            // Arrange
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MultipleODataQueryOptionsBindingExtension_ApplyTo()
        {
            // Arrange
            MultipleODataQueryOptionsBindingExtension extensions = new MultipleODataQueryOptionsBindingExtension(
                new List<IODataQueryOptionsBindingExtension>{
                    new MockODataQueryOptionsBindingExtension(),
                    new MockODataQueryOptionsBindingExtension(),
                });

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers");
            var context = new ODataQueryContext(CreateModel(), typeof(Customer));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
            ODataQuerySettings querySettings = context.GetODataQuerySettings();

            var customers = CreateTestQueryable();

            var expected = ApplyTestExpressions(ApplyTestExpressions(customers, querySettings), querySettings);

            // Act
            var result = extensions.ApplyTo(customers, queryOptions, querySettings);

            // Arrange
            Assert.Equal(expected, result);
        }

        private static IEdmModel CreateModel()
        {
            return new ODataModelBuilder()
                .Add_Customer_EntityType_With_Address()
                .Add_Customers_EntitySet().GetEdmModel();
        }

        private static IQueryable<Customer> CreateTestQueryable()
        {
            return (new List<Customer>{
                new Customer { Id = 1, Address = new Address { City = "A" } },
                new Customer { Id = 2, Address = new Address { City = "B" } },
                new Customer { Id = 3, Address = new Address { City = "C" } }
            }).AsQueryable();
        }

        //Used skip expression for tests, because it has a simple usage
        private static IQueryable ApplyTestExpressions(IQueryable queryable, ODataQuerySettings querySettings)
        {
            return ExpressionHelpers.Skip(queryable, 10, queryable.ElementType, querySettings.EnableConstantParameterization);
        }

        private class MockODataQueryOptionsBindingExtension : IODataQueryOptionsBindingExtension
        {
            public IQueryable ApplyTo(IQueryable query, ODataQueryOptions queryOptions, ODataQuerySettings querySettings)
            {
                return ApplyTestExpressions(query, querySettings);
            }
        }

    }
}
