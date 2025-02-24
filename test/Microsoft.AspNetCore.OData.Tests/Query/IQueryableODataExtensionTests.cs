//-----------------------------------------------------------------------------
// <copyright file="IQueryableODataExtensionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Query.Expressions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class IQueryableODataExtensionTests
    {
        private readonly ODataQuerySettings _settings;
        private readonly IEdmModel _model;

        public IQueryableODataExtensionTests()
        {
            _model = SelectExpandBinderTest.GetEdmModel();
            _settings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False };
        }

        [Fact]
        public void OCast_Works_ForSingleObject()
        {
            // Arrange & Act & Assert
            object source = null;
            Assert.Null(source.OCast<object>());

            // Arrange & Act & Assert
            QueryCustomer customer = new QueryCustomer
            {
                Name = "A"
            };

            Assert.Null(customer.OCast<string>());

            var expectCustomer = customer.OCast<QueryCustomer>();
            Assert.Same(customer, expectCustomer);
        }

        [Fact]
        public void OCast_Works_ForSingleSelectAndExpand()
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(_model, typeof(QueryCustomer)) { RequestContainer = new MockServiceProvider() };
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: "Name", expand: null, context: context);
            QueryCustomer customer = new QueryCustomer
            {
                Name = "A",
                Age = 42
            };

            SelectExpandBinder binder = new SelectExpandBinder();
            QueryBinderContext queryBinderContext = new QueryBinderContext(_model, _settings, typeof(QueryCustomer))
            {
                NavigationSource = context.NavigationSource
            };

            object result = binder.ApplyBind(customer, selectExpand.SelectExpandClause, queryBinderContext);

            // Act
            QueryCustomer expectedCustomer = result.OCast<QueryCustomer>();

            // Assert
            Assert.Equal("A", expectedCustomer.Name);
            Assert.Equal(0, expectedCustomer.Age);
        }

        [Theory]
        [InlineData("Name", false)]
        [InlineData("Name,Age", true)]
        public void OCast_Works_ForQuerableSelectAndExpand(string select, bool withAge)
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(_model, typeof(QueryCustomer)) { RequestContainer = new MockServiceProvider() };
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select, expand: null, context: context);
            IQueryable customers = new[] {
                new QueryCustomer
                {
                    Name = "A",
                    Age = 42
                },
                new QueryCustomer
                {
                    Name = "B",
                    Age = 38
                }
            }.AsQueryable();

            SelectExpandBinder binder = new SelectExpandBinder();
            QueryBinderContext queryBinderContext = new QueryBinderContext(_model, _settings, typeof(QueryCustomer))
            {
                NavigationSource = context.NavigationSource
            };

            IQueryable queryable = binder.ApplyBind(customers, selectExpand.SelectExpandClause, queryBinderContext);

            // Act
            IEnumerable<QueryCustomer> expectedCustomers = queryable.OCast<QueryCustomer>();

            // Assert
            Assert.Collection(expectedCustomers,
                e =>
                {
                    Assert.Equal("A", e.Name);

                    if (withAge)
                        Assert.Equal(42, e.Age);
                    else
                        Assert.Equal(0, e.Age);
                },
                e =>
                {
                    Assert.Equal("B", e.Name);

                    if (withAge)
                        Assert.Equal(38, e.Age);
                    else
                        Assert.Equal(0, e.Age);
                });
        }

        [Fact]
        public void OCast_Works_ForInheritancePropertiesQuerableSelectAndExpand()
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(_model, typeof(QueryVipCustomer)) { RequestContainer = new MockServiceProvider() };
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: "Taxes($top=2)", expand: null, context: context);
            IQueryable customers = new[] {
                new QueryVipCustomer
                {
                    Name = "A",
                    Age = 42,
                    Taxes = new List<int> { 1, 2, 3, 4, 5}
                },
                new QueryVipCustomer
                {
                    Name = "B",
                    Age = 38,
                    Taxes = new List<int> { 6, 7, 8, 9, 10}
                }
            }.AsQueryable();

            SelectExpandBinder binder = new SelectExpandBinder();
            QueryBinderContext queryBinderContext = new QueryBinderContext(_model, _settings, typeof(QueryCustomer))
            {
                NavigationSource = context.NavigationSource
            };

            IQueryable queryable = binder.ApplyBind(customers, selectExpand.SelectExpandClause, queryBinderContext);

            // Act
            IEnumerable<QueryCustomer> expectedCustomers = queryable.OCast<QueryCustomer>();

            // Assert
            Assert.Collection(expectedCustomers,
                e =>
                {
                    QueryVipCustomer vipCustomer = Assert.IsType<QueryVipCustomer>(e);
                    Assert.Null(vipCustomer.Name);
                    Assert.NotNull(vipCustomer.Taxes);
                    Assert.Equal(new int[] { 1, 2 }, vipCustomer.Taxes);
                },
                e =>
                {
                    QueryVipCustomer vipCustomer = Assert.IsType<QueryVipCustomer>(e);
                    Assert.Null(vipCustomer.Name);
                    Assert.NotNull(vipCustomer.Taxes);
                    Assert.Equal(new int[] { 6, 7 }, vipCustomer.Taxes);
                });
        }

        [Fact]
        public void OCast_Works_ForCollectionSelectAndExpand()
        {
            QueryCustomer customer = new QueryCustomer
            {
                Name = "A",
                Orders = new List<QueryOrder>()
            };
            QueryOrder order = new QueryOrder { Customer = customer };
            customer.Orders.Add(order);

            QueryCustomer vipCustomer = new QueryVipCustomer
            {
                Name = "B",
                Orders = new List<QueryOrder>()
            };
            order = new QueryOrder { Customer = vipCustomer };
            vipCustomer.Orders.Add(order);

            var queryableOf = new[] { customer, vipCustomer }.AsQueryable();

            // Arrange
            ODataQueryContext context = new ODataQueryContext(_model, typeof(QueryCustomer)) { RequestContainer = new MockServiceProvider() };
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption("Orders", "Orders,Orders($expand=Customer)", context);

            // Act
            SelectExpandBinder binder = new SelectExpandBinder();
            QueryBinderContext queryBinderContext = new QueryBinderContext(_model, _settings, typeof(QueryCustomer))
            {
                NavigationSource = context.NavigationSource
            };

            IQueryable queryable = binder.ApplyBind(queryableOf, selectExpand.SelectExpandClause, queryBinderContext);
            IEnumerable<QueryCustomer> expectedCustomers = queryable.OCast<QueryCustomer>();

            // Assert
            Assert.Collection(expectedCustomers,
                e =>
                {
                    QueryCustomer c = Assert.IsType<QueryCustomer>(e);
                    Assert.NotNull(c.Orders);
                    QueryOrder order = Assert.Single(c.Orders);
                    Assert.NotNull(order.Customer);
                    Assert.Equal("A", order.Customer.Name);
                },
                e =>
                {
                    QueryVipCustomer c = Assert.IsType<QueryVipCustomer>(e);
                    Assert.NotNull(c.Orders);
                    QueryOrder order = Assert.Single(c.Orders);
                    Assert.NotNull(order.Customer);
                    Assert.Equal("B", order.Customer.Name);
                });
        }
    }
}
