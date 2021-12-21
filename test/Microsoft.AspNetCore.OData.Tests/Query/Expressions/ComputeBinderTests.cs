//-----------------------------------------------------------------------------
// <copyright file="ComputeBinderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    public class ComputeBinderTests
    {
        private static IEdmModel _model = GetEdmModel();

        [Fact]
        public void FilterBinder_BindsComputedPropertyInDollarFilter_FromDollarCompute()
        {
            // Arrange
            QueryClause clause = CreateQueryClause("$filter=Total lt 30&$compute=Price mul Qty as Total", _model, typeof(ComputeCustomer));

            ODataQuerySettings querySettings = new ODataQuerySettings();
            IFilterBinder binder = new FilterBinder();
            QueryBinderContext context = new QueryBinderContext(_model, querySettings, typeof(ComputeCustomer));
            if (clause.Compute != null)
            {
                context.AddComputedProperties(clause.Compute.ComputedItems);
            }

            // Act
            Expression filterExp = binder.BindFilter(clause.Filter, context);

            // Assert
            string resultExpression = ExpressionStringBuilder.ToString(filterExp);
            Assert.Equal("$it => (Convert(($it.Price * Convert($it.Qty))) < Convert(30))", resultExpression);

            Assert.True(FilterBinderTests.InvokeFilter(new ComputeCustomer { Price = 10, Qty = 2 }, filterExp));
            Assert.False(FilterBinderTests.InvokeFilter(new ComputeCustomer { Price = 10, Qty = 4 }, filterExp));
        }

        [Fact]
        public void OrderByBinder_BindsComputedPropertyInDollarOrderBy_FromDollarCompute()
        {
            // Arrange
            QueryClause clause = CreateQueryClause("$orderby=Total&$compute=Price mul Qty as Total", _model, typeof(ComputeCustomer));

            ODataQuerySettings querySettings = new ODataQuerySettings();
            IOrderByBinder binder = new OrderByBinder();
            QueryBinderContext context = new QueryBinderContext(_model, querySettings, typeof(ComputeCustomer));
            if (clause.Compute != null)
            {
                context.AddComputedProperties(clause.Compute.ComputedItems);
            }

            // Act
            OrderByBinderResult orderByResult = binder.BindOrderBy(clause.OrderBy, context);

            // Assert
            Assert.Null(orderByResult.ThenBy);
            Assert.Equal(OrderByDirection.Ascending, orderByResult.Direction);
            string resultExpression = ExpressionStringBuilder.ToString(orderByResult.OrderByExpression);
            Assert.Equal("$it => ($it.Price * Convert($it.Qty))", resultExpression);

            IEnumerable<ComputeCustomer> customers = new[]
            {
                new ComputeCustomer { Id = 1, Qty = 3, Price = 5.99 },
                new ComputeCustomer { Id = 2, Qty = 5, Price = 2.99 },
                new ComputeCustomer { Id = 3, Qty = 2, Price = 18.01 },
                new ComputeCustomer { Id = 4, Qty = 4, Price = 9.99 },
            };

            var orderedResult = OrderByBinderTests.InvokeOrderBy(customers, orderByResult.OrderByExpression, orderByResult.Direction, false);

            Assert.True(new [] { 2, 1, 3, 4 }.SequenceEqual(orderedResult.Select(a => a.Id))); // ordered id
        }

        [Fact]
        public void SelectExpandBinder_BindsComputedPropertyInSelect_FromDollarCompute()
        {
            // Arrange
            QueryClause clause = CreateQueryClause("$select=Name,Total&$compute=Price mul Qty as Total", _model, typeof(ComputeCustomer));

            ODataQuerySettings querySettings = new ODataQuerySettings();
            SelectExpandBinder binder = new SelectExpandBinder();
            QueryBinderContext context = new QueryBinderContext(_model, querySettings, typeof(ComputeCustomer));
            if (clause.Compute != null)
            {
                context.AddComputedProperties(clause.Compute.ComputedItems);
            }

            // Act
            Expression selectExp = binder.BindSelectExpand(clause.SelectExpand, context);

            // Assert
            Assert.NotNull(selectExp);
            string resultExpression = ExpressionStringBuilder.ToString(selectExp);
            Assert.Equal("$it => new SelectSome`1() {" +
                "Model = Microsoft.OData.Edm.EdmModel, " +
                "Container = new NamedPropertyWithNext1`1() {" +
                  "Name = Name, " +
                  "Value = $it.Name, " +
                  "Next0 = new NamedProperty`1() {" +
                    "Name = Total, " +
                    "Value = ($it.Price * Convert($it.Qty)), " +
                  "}, " +
                  "Next1 = new AutoSelectedNamedProperty`1() {" +
                    "Name = Id, " +
                    "Value = Convert($it.Id), " +
                  "}, }, }", resultExpression);

            ComputeCustomer customer = new ComputeCustomer { Name = "Peter", Price = 1.99, Qty = 3 };
            IDictionary<string, object> result = SelectExpandBinderTest.InvokeSelectExpand(customer, selectExp);

            Assert.Equal(2, result.Count);
            Assert.Collection(result,
                e =>
                {
                    Assert.Equal("Name", e.Key);
                    Assert.Equal("Peter", e.Value);
                },
                e =>
                {
                    Assert.Equal("Total", e.Key);
                    Assert.Equal(5.97, e.Value);
                });
        }

        [Fact]
        public void SelectExpandBinder_BindsComputedPropertyInNestedSelect_FromDollarCompute()
        {
            // Arrange
            QueryClause clause = CreateQueryClause("$select=Location($select=StateCode;$compute=Zipcode div 1000 as StateCode)", _model, typeof(ComputeCustomer));

            ODataQuerySettings querySettings = new ODataQuerySettings();
            SelectExpandBinder binder = new SelectExpandBinder();
            QueryBinderContext context = new QueryBinderContext(_model, querySettings, typeof(ComputeCustomer));
            if (clause.Compute != null)
            {
                context.AddComputedProperties(clause.Compute.ComputedItems);
            }

            // Act
            Expression selectExp = binder.BindSelectExpand(clause.SelectExpand, context);

            // Assert
            Assert.NotNull(selectExp);
            string resultExpression = ExpressionStringBuilder.ToString(selectExp);
            Assert.Equal("$it => new SelectSome`1() {" +
                "Model = Microsoft.OData.Edm.EdmModel, " +
                "Container = new NamedPropertyWithNext0`1() {" +
                  "Name = Location, " +
                  "Value = new SelectSome`1() {" +
                    "Model = Microsoft.OData.Edm.EdmModel, " +
                    "Container = new NamedProperty`1() {" +
                      "Name = StateCode, " +
                      "Value = ($it.Location.Zipcode / 1000), " +
                    "}, " +
                  "}, " +
                  "Next0 = new AutoSelectedNamedProperty`1() {Name = Id, Value = Convert($it.Id), }, }, }", resultExpression);

            ComputeCustomer customer = new ComputeCustomer
            {
                Location = new ComputeAddress { Zipcode = 98029 }
            };
            IDictionary<string, object> result = SelectExpandBinderTest.InvokeSelectExpand(customer, selectExp);

            var location = Assert.Single(result);
            Assert.Equal("Location", location.Key);
            SelectExpandWrapper itemWrapper = location.Value as SelectExpandWrapper;
            IDictionary<string, object> locationResult = itemWrapper.ToDictionary();

            var stateCode = Assert.Single(locationResult);
            Assert.Equal("StateCode", stateCode.Key);
            Assert.Equal(98, stateCode.Value);
        }

        [Fact]
        public void SelectExpandBinder_BindsComputedPropertyInExpand_FromDollarCompute()
        {
            // Arrange
            QueryClause clause = CreateQueryClause("$expand=Orders($select=Title,Tax;$compute=Amount mul TaxRate as Tax)", _model, typeof(ComputeCustomer));

            ODataQuerySettings querySettings = new ODataQuerySettings();
            SelectExpandBinder binder = new SelectExpandBinder();
            QueryBinderContext context = new QueryBinderContext(_model, querySettings, typeof(ComputeCustomer));
            if (clause.Compute != null)
            {
                context.AddComputedProperties(clause.Compute.ComputedItems);
            }

            // Act
            Expression selectExp = binder.BindSelectExpand(clause.SelectExpand, context);

            // Assert
            Assert.NotNull(selectExp);
            string resultExpression = ExpressionStringBuilder.ToString(selectExp);
            Assert.Equal("$it => new SelectAllAndExpand`1() {Model = Microsoft.OData.Edm.EdmModel, " +
                "Instance = $it, UseInstanceForProperties = True, " +
                "Container = new NamedPropertyWithNext0`1() " +
                "{" +
                  "Name = Orders, " +
                  "Value = $it.Orders.Select($it => new SelectSome`1() " +
                  "{" +
                    "Model = Microsoft.OData.Edm.EdmModel, " +
                    "Container = new NamedPropertyWithNext1`1() " +
                    "{" +
                      "Name = Title, " +
                      "Value = $it.Title, " +
                      "Next0 = new NamedProperty`1() {Name = Tax, Value = (Convert($it.Amount) * $it.TaxRate), }, " +
                      "Next1 = new AutoSelectedNamedProperty`1() " +
                      "{" +
                        "Name = Id, Value = Convert($it.Id), " +
                      "}, " +
                    "}, " +
                  "}), " +
                  "Next0 = new NamedProperty`1() {Name = Dynamics, Value = $it.Dynamics, }, }, }", resultExpression);

            ComputeCustomer customer = new ComputeCustomer
            {
                Orders = new List<ComputeOrder>
                {
                    new ComputeOrder { Title = "Kerry", Amount = 4, TaxRate = 0.35 },
                    new ComputeOrder { Title = "WU", Amount = 6, TaxRate = 0.5 },
                    new ComputeOrder { Title = "XU", Amount = 5, TaxRate = 0.12 },
                }
            };
            IDictionary<string, object> result = SelectExpandBinderTest.InvokeSelectExpand(customer, selectExp);

            Assert.Equal(8, result.Count); // Because it's select-all

            int idx = 0;
            var ordersValue = result["Orders"] as IEnumerable;
            foreach (var order in ordersValue)
            {
                SelectExpandWrapper itemWrapper = order as SelectExpandWrapper;

                var orderDic = itemWrapper.ToDictionary();
                Assert.Equal(customer.Orders[idx].Title, orderDic["Title"]);
                Assert.Equal(customer.Orders[idx].Amount * customer.Orders[idx].TaxRate, orderDic["Tax"]);
                idx++;
            }
        }

        private static QueryClause CreateQueryClause(string query, IEdmModel model, Type type)
        {
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == type.Name);
            Assert.NotNull(entityType); // Guard

            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // Guard

            string[] queryItems = query.Split('&');

            Dictionary<string, string> queries = new Dictionary<string, string>();
            foreach (string item in queryItems)
            {
                if (item.StartsWith("$select=", StringComparison.Ordinal))
                {
                    queries["$select"] = item.Substring(8);
                }
                else if (item.StartsWith("$expand=", StringComparison.Ordinal))
                {
                    queries["$expand"] = item.Substring(8);
                }
                else if (item.StartsWith("$filter=", StringComparison.Ordinal))
                {
                    queries["$filter"] = item.Substring(8);
                }
                else if (item.StartsWith("$orderby=", StringComparison.Ordinal))
                {
                    queries["$orderby"] = item.Substring(9);
                }
                else if (item.StartsWith("$compute=", StringComparison.Ordinal))
                {
                    queries["$compute"] = item.Substring(9);
                }
            }

            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, entityType, entitySet, queries);
            return new QueryClause
            {
                Filter = parser.ParseFilter(),
                OrderBy = parser.ParseOrderBy(),
                SelectExpand = parser.ParseSelectAndExpand(),
                Compute = parser.ParseCompute()
            };
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "NS";
            builder.EntitySet<ComputeCustomer>("Customers");
            builder.EntitySet<ComputeOrder>("Orders");
            return builder.GetEdmModel();
        }

        private class QueryClause
        {
            public FilterClause Filter { get; set; }
            public OrderByClause OrderBy { get; set; }
            public SelectExpandClause SelectExpand { get; set; }
            public ComputeClause Compute { get; set; }
        }
    }

    public class ComputeCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public double Price { get; set; }

        public int Qty { get; set; }

        public ComputeAddress Location { get; set; }

        public IList<ComputeOrder> Orders { get; set; }

        public IDictionary<string, object> Dynamics { get; set; }
    }

    public class ComputeAddress
    {
        public string Street { get; set; }

        public int Zipcode { get; set; }

        public IDictionary<string, object> Dynamics { get; set; }
    }

    public class ComputeOrder
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public int Amount { get; set; }
        public double Price { get; set; }
        public double TaxRate { get; set; }

        public IDictionary<string, object> Dynamics { get; set; }
    }
}
