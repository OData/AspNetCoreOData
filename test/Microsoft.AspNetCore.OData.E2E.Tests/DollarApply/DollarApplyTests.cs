//-----------------------------------------------------------------------------
// <copyright file="DollarApplyTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

// Conditional compilation due to known bug affecting EF Core and lower
#if NET6_0_OR_GREATER
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Wrapper;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.TestCommon.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply
{
    public class DollarApplyTests : WebApiTestBase<DollarApplyTests>
    {
        public DollarApplyTests(WebApiTestFixture<DollarApplyTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            var model = DollarApplyEdmModel.GetEdmModel();

            string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=DollarApplySqlDbContext";
            services.AddDbContext<DollarApplyDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddDbContext<DollarApplySqlDbContext>(options => options.UseSqlServer(connectionString));
            services.ConfigureControllers(typeof(InMemorySalesController), typeof(SqlSalesController), typeof(EmployeesController));

            services.AddControllers().AddOData(options =>
            {
                options.EnableQueryFeatures();
                options.EnableAttributeRouting = true;

                // Due to how route matching works, `defaultsql` and `customsql` must be registered before `default` and `custom`
                options.AddRouteComponents("defaultsql", model);
                options.AddRouteComponents("customsql", model, (nestedServices) =>
                {
                    nestedServices.AddSingleton<IAggregationBinder, TestAggregationBinder>();
                });

                options.AddRouteComponents("default", model);
                options.AddRouteComponents("custom", model, (nestedServices) =>
                {
                    nestedServices.AddSingleton<IAggregationBinder, TestAggregationBinder>();
                });
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new TestGroupByWrapperConverter());
            });
        }

        protected static void UpdateConfigure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter))")]
        public async Task TestGroupByPrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Equal("2022-1", Assert.IsType<JObject>(result[0]).Value<string>("Quarter"));
            Assert.Equal("2022-2", Assert.IsType<JObject>(result[1]).Value<string>("Quarter"));
            Assert.Equal("2022-3", Assert.IsType<JObject>(result[2]).Value<string>("Quarter"));
            Assert.Equal("2022-4", Assert.IsType<JObject>(result[3]).Value<string>("Quarter"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Year))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Year))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Year))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Year))")]
        public async Task TestGroupByMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal("2022", resultAt0.Value<string>("Year"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal("2022", resultAt1.Value<string>("Year"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal("2022", resultAt2.Value<string>("Year"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal("2022", resultAt3.Value<string>("Year"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name))")]
        public async Task TestGroupByNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Matches("\\{\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name,Customer/Id))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name,Customer/Id))")]
        public async Task TestGroupByMultipleNestedPropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name))")]
        public async Task TestGroupByMultiNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id))")]
        public async Task TestGroupByHybridNestedPropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Product/Name))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Product/Name))")]
        public async Task TestGroupByNestedAndNonNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\",\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-4\",\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Amount with sum as SumAmount)")]
        [InlineData("custom/Sales?$apply=aggregate(Amount with sum as SumAmount)")]
        [InlineData("defaultsql/Sales?$apply=aggregate(Amount with sum as SumAmount)")]
        [InlineData("customsql/Sales?$apply=aggregate(Amount with sum as SumAmount)")]
        public async Task TestAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(24m, Assert.Single(result).Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Product/TaxRate with min as MinTaxRate)")]
        [InlineData("custom/Sales?$apply=aggregate(Product/TaxRate with min as MinTaxRate)")]
        [InlineData("defaultsql/Sales?$apply=aggregate(Product/TaxRate with min as MinTaxRate)")]
        [InlineData("customsql/Sales?$apply=aggregate(Product/TaxRate with min as MinTaxRate)")]
        public async Task TestAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(0.06m, Assert.Single(result).Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate)")]
        [InlineData("custom/Sales?$apply=aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate)")]
        [InlineData("defaultsql/Sales?$apply=aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate)")]
        [InlineData("customsql/Sales?$apply=aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate)")]
        public async Task TestAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.Single(result);
            Assert.Equal(3m, resultAt0.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MaxTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate($count as SalesCount)")]
        [InlineData("custom/Sales?$apply=aggregate($count as SalesCount)")]
        [InlineData("defaultsql/Sales?$apply=aggregate($count as SalesCount)")]
        [InlineData("customsql/Sales?$apply=aggregate($count as SalesCount)")]
        public async Task TestAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(8, Assert.Single(result).Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Product/Name with countdistinct as DistinctProducts)")]
        [InlineData("custom/Sales?$apply=aggregate(Product/Name with countdistinct as DistinctProducts)")]
        [InlineData("defaultsql/Sales?$apply=aggregate(Product/Name with countdistinct as DistinctProducts)")]
        [InlineData("customsql/Sales?$apply=aggregate(Product/Name with countdistinct as DistinctProducts)")]
        public async Task TestAggregateNestedPropertyWithCountDistinctAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, Assert.Single(result).Value<int>("DistinctProducts"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate(Amount with sum as SumAmount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter),aggregate(Amount with sum as SumAmount))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByPrimitivePropertyAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(9m, resultAt0.Value<decimal>("SumAmount"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(4m, resultAt1.Value<decimal>("SumAmount"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(5m, resultAt2.Value<decimal>("SumAmount"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(6m, resultAt3.Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByPrimitivePropertyAndAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(0.06m, resultAt0.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(0.14m, resultAt3.Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByPrimitivePropertyAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(4.5m, resultAt0.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(2m, resultAt1.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(2.5m, resultAt2.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt2.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(3m, resultAt3.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt3.Value<decimal>("MaxTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with sum as SumAmount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with sum as SumAmount))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByMultiplePropertiesAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt0.Value<int>("Year"));
            Assert.Equal(9m, resultAt0.Value<decimal>("SumAmount"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt1.Value<int>("Year"));
            Assert.Equal(4m, resultAt1.Value<decimal>("SumAmount"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt2.Value<int>("Year"));
            Assert.Equal(5m, resultAt2.Value<decimal>("SumAmount"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt3.Value<int>("Year"));
            Assert.Equal(6m, resultAt3.Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Year),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Year),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Year),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Year),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByMultiplePropertiesAndAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt0.Value<int>("Year"));
            Assert.Equal(0.06m, resultAt0.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt1.Value<int>("Year"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt3.Value<int>("Year"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt3.Value<int>("Year"));
            Assert.Equal(0.14m, resultAt3.Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByMultiplePropertiesAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt0.Value<int>("Year"));
            Assert.Equal(4.5m, resultAt0.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt2.Value<int>("Year"));
            Assert.Equal(2m, resultAt1.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt2.Value<int>("Year"));
            Assert.Equal(2.5m, resultAt2.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt2.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt3.Value<int>("Year"));
            Assert.Equal(3m, resultAt3.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt3.Value<decimal>("MaxTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByNestedPropertyAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Matches("\\{\"SumAmount\":8(?:\\.0+)?,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":4(?:\\.0+)?,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":12(?:\\.0+)?,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByNestedPropertyAndAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Matches("\\{\"MinTaxRate\":0\\.14,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByNestedPropertyAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.14,\"AverageAmount\":2(?:\\.0+)?,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":2(?:\\.0+)?,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":6(?:\\.0+)?,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            Assert.Matches("\\{\"SumAmount\":1(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":2(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":4(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":8(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":4(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":2(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":3(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            Assert.Matches("\\{\"MinTaxRate\":0\\.14,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.14,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.14,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.14,\"AverageAmount\":1(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":2(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":4(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":8(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.14,\"AverageAmount\":4(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":2(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.14,\"AverageAmount\":1\\.5(?:0+)?,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);            
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByMultiNestedPropertyAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"SumAmount\":8(?:\\.0+)?,\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":16(?:\\.0+)?,\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByMultiNestedPropertyAndAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"MinTaxRate\":0\\.14,\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByMultiNestedPropertyAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.14,\"AverageAmount\":2(?:\\.0+)?,\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":4(?:\\.0+)?,\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"SumAmount\":1(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":6(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":8(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":4(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":2(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":3(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"MinTaxRate\":0\\.14,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.14,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.06,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MinTaxRate\":0\\.14,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.14,\"AverageAmount\":1(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":3(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":8(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.14,\"AverageAmount\":4(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.06,\"AverageAmount\":2(?:\\.0+)?,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"MaxTaxRate\":0\\.14,\"AverageAmount\":1\\.5(?:0+)?,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"SumAmount\":1(?:\\.0+)?,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\",\"SumAmount\":4(?:\\.0+)?,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"SumAmount\":4(?:\\.0+)?,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"SumAmount\":8(?:\\.0+)?,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-4\",\"SumAmount\":6(?:\\.0+)?,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"SumAmount\":1(?:\\.0+)?,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"MinTaxRate\":0\\.14,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\",\"MinTaxRate\":0\\.06,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"MinTaxRate\":0\\.06,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"MinTaxRate\":0\\.06,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-4\",\"MinTaxRate\":0\\.14,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"MinTaxRate\":0\\.14,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"MaxTaxRate\":0\\.14,\"AverageAmount\":1(?:\\.0+)?,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\",\"MaxTaxRate\":0\\.06,\"AverageAmount\":2(?:\\.0+)?,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"MaxTaxRate\":0\\.06,\"AverageAmount\":4(?:\\.0+)?,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"MaxTaxRate\":0\\.06,\"AverageAmount\":8(?:\\.0+)?,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-4\",\"MaxTaxRate\":0\\.14,\"AverageAmount\":3(?:\\.0+)?,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"MaxTaxRate\":0\\.14,\"AverageAmount\":1(?:\\.0+)?,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate($count as SalesCount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter),aggregate($count as SalesCount))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter),aggregate($count as SalesCount))")]
        public async Task TestGroupByPrimitivePropertyAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(2, resultAt0.Value<int>("SalesCount"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(2, resultAt1.Value<int>("SalesCount"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(2, resultAt2.Value<int>("SalesCount"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(2, resultAt3.Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Year),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Year),aggregate($count as SalesCount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Year),aggregate($count as SalesCount))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Year),aggregate($count as SalesCount))")]
        public async Task TestGroupByMultiplePropertiesAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt0.Value<int>("Year"));
            Assert.Equal(2, resultAt0.Value<int>("SalesCount"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt1.Value<int>("Year"));
            Assert.Equal(2, resultAt1.Value<int>("SalesCount"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt2.Value<int>("Year"));
            Assert.Equal(2, resultAt2.Value<int>("SalesCount"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(2022, resultAt3.Value<int>("Year"));
            Assert.Equal(2, resultAt3.Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name),aggregate($count as SalesCount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name),aggregate($count as SalesCount))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name),aggregate($count as SalesCount))")]
        public async Task TestGroupByNestedPropertyAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Matches("\\{\"SalesCount\":4,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":2,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":2,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate($count as SalesCount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate($count as SalesCount))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate($count as SalesCount))")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":2,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate($count as SalesCount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name),aggregate($count as SalesCount))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name),aggregate($count as SalesCount))")]
        public async Task TestGroupByMultiNestedPropertyAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"SalesCount\":4,\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}\\}\\}", content);
            Assert.Matches("\\{\"SalesCount\":4,\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate($count as SalesCount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate($count as SalesCount))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate($count as SalesCount))")]
        public async Task TestGroupByMultipleHybridNestedPropertiesAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}}\\}", content);
            Assert.Matches("\\{\"SalesCount\":2,\"Customer\":\\{\"Id\":\"C1\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}}\\}", content);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}}\\}", content);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C2\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}}\\}", content);
            Assert.Matches("\\{\"SalesCount\":1,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}}\\}", content);
            Assert.Matches("\\{\"SalesCount\":2,\"Customer\":\\{\"Id\":\"C3\"\\},\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name),aggregate($count as SalesCount))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Product/Name),aggregate($count as SalesCount))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Product/Name),aggregate($count as SalesCount))")]
        public async Task TestGroupByNestedAndNonNestedPropertiesAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"SalesCount\":1,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\",\"SalesCount\":2,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"SalesCount\":1,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"SalesCount\":1,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-4\",\"SalesCount\":2,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"SalesCount\":1,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);            
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        public async Task TestGroupByPrimitivePropertyAndAggregateNestedPropertyWithCountDistinctAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(2, resultAt0.Value<int>("DistinctProducts"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(1, resultAt1.Value<int>("DistinctProducts"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(2, resultAt2.Value<int>("DistinctProducts"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(1, resultAt3.Value<int>("DistinctProducts"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Year),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Year),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter,Year),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter,Year),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        public async Task TestGroupByMultiplePropertiesAndAggregateNestedPropertyWithCountDistinctAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal(2, resultAt0.Value<int>("DistinctProducts"));
            Assert.Equal(2022, resultAt0.Value<int>("Year"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal(1, resultAt1.Value<int>("DistinctProducts"));
            Assert.Equal(2022, resultAt1.Value<int>("Year"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal(2, resultAt2.Value<int>("DistinctProducts"));
            Assert.Equal(2022, resultAt2.Value<int>("Year"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
            Assert.Equal(1, resultAt3.Value<int>("DistinctProducts"));
            Assert.Equal(2022, resultAt3.Value<int>("Year"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Amount with custom.stdev as StdDev)")]
        [InlineData("custom/Sales?$apply=aggregate(Amount with custom.stdev as StdDev)")]
        [InlineData("defaultsql/Sales?$apply=aggregate(Amount with custom.stdev as StdDev)")]
        [InlineData("customsql/Sales?$apply=aggregate(Amount with custom.stdev as StdDev)")]
        public async Task TestAggregatePrimitivePropertyWithCustomAggregateFunctionAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2.32992949004287, Assert.Single(result).Value<double>("StdDev"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Product/TaxRate with custom.stdev as StdDev)")]
        [InlineData("custom/Sales?$apply=aggregate(Product/TaxRate with custom.stdev as StdDev)")]
        [InlineData("defaultsql/Sales?$apply=aggregate(Product/TaxRate with custom.stdev as StdDev)")]
        [InlineData("customsql/Sales?$apply=aggregate(Product/TaxRate with custom.stdev as StdDev)")]
        public async Task TestAggregateNestedPropertyWithCustomAggregateFunctionAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(0.042761798705987904, Assert.Single(result).Value<double>("StdDev"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with custom.stdev as StdDev))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with custom.stdev as StdDev))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with custom.stdev as StdDev))")]
        [InlineData("customsql/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with custom.stdev as StdDev))")]
        public async Task TestGroupByMultiNestedPropertyAndAggregatePrimitivePropertyWithCustomAggregateFunctionAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"StdDev\":1\\.4142135623730951,\"Product\":\\{\"Category\":\\{\"Name\":\"Non-Food\"\\}}\\}", content);
            Assert.Matches("\\{\"StdDev\":2\\.8284271247461903,\"Product\":\\{\"Category\":\\{\"Name\":\"Food\"\\}}\\}", content);
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Customer/Id),aggregate(Product/TaxRate with custom.stdev as StdDev))")]
        [InlineData("custom/Sales?$apply=groupby((Customer/Id),aggregate(Product/TaxRate with custom.stdev as StdDev))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Customer/Id),aggregate(Product/TaxRate with custom.stdev as StdDev))")]
        [InlineData("customsql/Sales?$apply=groupby((Customer/Id),aggregate(Product/TaxRate with custom.stdev as StdDev))")]
        public async Task TestGroupByNestedPropertyAndAggregateNestedPropertyWithCustomAggregateFunctionAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.046188021535170064, resultAt0.Value<double>("StdDev"));
            Assert.Equal("C2", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.0565685424949238, resultAt1.Value<double>("StdDev"));
            Assert.Equal("C3", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.046188021535170064, resultAt2.Value<double>("StdDev"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Gender))")]
        [InlineData("custom/Employees?$apply=groupby((Gender))")]
        public async Task TestGroupByDynamicPrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Female", Assert.IsType<JObject>(result[0]).Value<string>("Gender"));
            Assert.Equal("Male", Assert.IsType<JObject>(result[1]).Value<string>("Gender"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=aggregate(Commission with average as AverageCommission)")]
        [InlineData("custom/Employees?$apply=aggregate(Commission with average as AverageCommission)")]
        public async Task TestAggregateSingleDynamicPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(280, Assert.Single(result).Value<decimal>("AverageCommission"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Gender),aggregate(Commission with sum as SumCommission))")]
        [InlineData("custom/Employees?$apply=groupby((Gender),aggregate(Commission with sum as SumCommission))")]
        public async Task TestGroupByDynamicPrimitivePropertyAndAggregateDynamicPrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            Assert.Equal("Female", resultAt0.Value<string>("Gender"));
            Assert.Equal(930m, resultAt0.Value<decimal>("SumCommission"));
            Assert.Equal("Male", resultAt1.Value<string>("Gender"));
            Assert.Equal(190m, resultAt1.Value<decimal>("SumCommission"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Gender),aggregate(BaseSalary with max as MaxSalary))")]
        [InlineData("custom/Employees?$apply=groupby((Gender),aggregate(BaseSalary with max as MaxSalary))")]
        public async Task TestGroupByDynamicPrimitivePropertyAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            Assert.Equal("Female", resultAt0.Value<string>("Gender"));
            Assert.Equal(1300m, resultAt0.Value<decimal>("MaxSalary"));
            Assert.Equal("Male", resultAt1.Value<string>("Gender"));
            Assert.Equal(1500m, resultAt1.Value<decimal>("MaxSalary"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate(Amount with min as MinAmount))/groupby((Quarter))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate(Amount with min as MinAmount))/groupby((Quarter))")]
        [InlineData("defaultsql/Sales?$apply=groupby((Quarter),aggregate(Amount with min as MinAmount))/groupby((Quarter))")]
        [InlineData("customsql/Sales?$apply=groupby((Quarter),aggregate(Amount with min as MinAmount))/groupby((Quarter))")]
        public async Task TestGroupByPrimitivePropertyAndAggregatePrimitivePropertyThenGroupByPrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Equal("2022-1", Assert.IsType<JObject>(result[0]).Value<string>("Quarter"));
            Assert.Equal("2022-2", Assert.IsType<JObject>(result[1]).Value<string>("Quarter"));
            Assert.Equal("2022-3", Assert.IsType<JObject>(result[2]).Value<string>("Quarter"));
            Assert.Equal("2022-4", Assert.IsType<JObject>(result[3]).Value<string>("Quarter"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength with sum as CombinedProductNameLength)")]
        [InlineData("custom/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength with sum as CombinedProductNameLength)")]
        [InlineData("defaultsql/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength with sum as CombinedProductNameLength)")]
        [InlineData("customsql/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength with sum as CombinedProductNameLength)")]
        public async Task TestComputeNestedStringPropertyLengthThenAggregateComputedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(42, Assert.Single(result).Value<int>("CombinedProductNameLength"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength add Id with sum as CombinedProductNameLength)")]
        [InlineData("custom/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength add Id with sum as CombinedProductNameLength)")]
        [InlineData("defaultsql/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength add Id with sum as CombinedProductNameLength)")]
        [InlineData("customsql/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength add Id with sum as CombinedProductNameLength)")]
        public async Task TestComputeNestedStringPropertyLengthThenAggregateSumOfComputedAndPrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(78, Assert.IsType<JObject>(Assert.Single(result)).Value<int>("CombinedProductNameLength"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/groupby((Product/Name),aggregate(Id with sum as Total, ProductNameLength with max as MaxProductNameLength))")]
        [InlineData("custom/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/groupby((Product/Name),aggregate(Id with sum as Total, ProductNameLength with max as MaxProductNameLength))")]
        [InlineData("defaultsql/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/groupby((Product/Name),aggregate(Id with sum as Total, ProductNameLength with max as MaxProductNameLength))")]
        [InlineData("customsql/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/groupby((Product/Name),aggregate(Id with sum as Total, ProductNameLength with max as MaxProductNameLength))")]
        public async Task TestComputeNestedStringPropertyLengthThenGroupByNestedPropertyAndAggregatePrimitiveAndComputedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Matches("\\{\"MaxProductNameLength\":5,\"Total\":21,\"Product\":\\{\"Name\":\"Paper\"\\}\\}", content);
            Assert.Matches("\\{\"MaxProductNameLength\":5,\"Total\":8,\"Product\":\\{\"Name\":\"Sugar\"\\}\\}", content);
            Assert.Matches("\\{\"MaxProductNameLength\":6,\"Total\":7,\"Product\":\\{\"Name\":\"Coffee\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Address/City,Address/State))/groupby((Address/State),aggregate(Address/City with max as MaxCity))")]
        [InlineData("custom/Employees?$apply=groupby((Address/City,Address/State))/groupby((Address/State),aggregate(Address/City with max as MaxCity))")]
        public async Task TestGroupByMultipleNestedPropertiesThenGroupByNestedPropertyAndAggregateNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            Assert.Equal("Tacoma", resultAt0.Value<string>("MaxCity"));
            Assert.Equal("WA", Assert.IsType<JObject>(resultAt0.GetValue("Address")).Value<string>("State"));
            Assert.Equal("London", resultAt1.Value<string>("MaxCity"));
            Assert.Equal("UK", Assert.IsType<JObject>(resultAt1.GetValue("Address")).Value<string>("State"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Company/VP/Address/City,Company/VP/Address/State))/groupby((Company/VP/Address/State),aggregate(Company/VP/Address/City with max as MaxCity))")]
        [InlineData("custom/Employees?$apply=groupby((Company/VP/Address/City,Company/VP/Address/State))/groupby((Company/VP/Address/State),aggregate(Company/VP/Address/City with max as MaxCity))")]
        public async Task TestGroupByMultipleMultiNestedPropertiesThenGroupByMultiNestedPropertyAndAggregateMultiNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.IsType<JObject>(Assert.Single(result));
            Assert.Equal("Tacoma", resultAt0.Value<string>("MaxCity"));
            var company = Assert.IsType<JObject>(resultAt0.GetValue("Company"));
            var vp = Assert.IsType<JObject>(company.GetValue("VP"));
            var address = Assert.IsType<JObject>(vp.GetValue("Address"));
            Assert.Equal("WA", address.Value<string>("State"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Company/VP/Name,Company/VP/BaseSalary))/groupby((Company/VP/BaseSalary),aggregate(Company/VP/Name with max as MaxName))")]
        [InlineData("custom/Employees?$apply=groupby((Company/VP/Name,Company/VP/BaseSalary))/groupby((Company/VP/BaseSalary),aggregate(Company/VP/Name with max as MaxName))")]
        public async Task TestGroupByMultipleMultiNestedPropertiesThenGroupByMultiNestedPropertyAndAggregateMultiNestedPropertyWithMaxAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.IsType<JObject>(Assert.Single(result));
            Assert.Equal("Andrew Fuller", resultAt0.Value<string>("MaxName"));
            var company = Assert.IsType<JObject>(resultAt0.GetValue("Company"));
            var vp = Assert.IsType<JObject>(company.GetValue("VP"));
            Assert.Equal(1500m, vp.Value<decimal>("BaseSalary"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Company/VP/Name,Company/VP/BaseSalary))/groupby((Company/VP/Name),aggregate(Company/VP/BaseSalary with average as AverageBaseSalary))")]
        [InlineData("custom/Employees?$apply=groupby((Company/VP/Name,Company/VP/BaseSalary))/groupby((Company/VP/Name),aggregate(Company/VP/BaseSalary with average as AverageBaseSalary))")]
        public async Task TestGroupByMultipleMultiNestedPropertiesThenGroupByMultiNestedPropertyAndAverageMultiNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.IsType<JObject>(Assert.Single(result));
            Assert.Equal(1500m, resultAt0.Value<decimal>("AverageBaseSalary"));
            var company = Assert.IsType<JObject>(resultAt0.GetValue("Company"));
            var vp = Assert.IsType<JObject>(company.GetValue("VP"));
            Assert.Equal("Andrew Fuller", vp.Value<string>("Name"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Address/State),aggregate(BaseSalary with min as MinBaseSalary))/groupby((Address/State))")]
        [InlineData("custom/Employees?$apply=groupby((Address/State),aggregate(BaseSalary with min as MinBaseSalary))/groupby((Address/State))")]
        public async Task TestGroupByNestedPropertyAndAggregatePrimitivePropertyThenGroupByNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var addressAt0 = Assert.IsType<JObject>(result[0]).GetValue("Address");
            var addressAt1 = Assert.IsType<JObject>(result[1]).GetValue("Address");
            Assert.Equal("WA", Assert.IsType<JObject>(addressAt0).Value<string>("State"));
            Assert.Equal("UK", Assert.IsType<JObject>(addressAt1).Value<string>("State"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=compute(length(Name) as NameLen)/aggregate(NameLen with sum as TotalLen)")]
        [InlineData("custom/Employees?$apply=compute(length(Name) as NameLen)/aggregate(NameLen with sum as TotalLen)")]
        public async Task TestComputeStringPropertyLengthThenAggregateComputedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(55, Assert.IsType<JObject>(Assert.Single(result)).Value<int>("TotalLen"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=compute(length(Name) as NameLen)/aggregate(NameLen add Id with sum as TotalLen)")]
        [InlineData("custom/Employees?$apply=compute(length(Name) as NameLen)/aggregate(NameLen add Id with sum as TotalLen)")]
        public async Task TestComputeStringPropertyLengthThenAggregateSumOfComputedPropertyAndPrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(70, Assert.IsType<JObject>(Assert.Single(result)).Value<int>("TotalLen"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=compute(length(Address/State) as StateLen)/groupby((Address/State),aggregate(Id with sum as Total,StateLen with max as MaxStateLen))")]
        [InlineData("custom/Employees?$apply=compute(length(Address/State) as StateLen)/groupby((Address/State),aggregate(Id with sum as Total,StateLen with max as MaxStateLen))")]
        public async Task TestComputeNestedStringPropertyLengthThenGroupByNestedPropertyAndAggregateComputedAndPrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            Assert.Equal(2, resultAt0.Value<int>("MaxStateLen"));
            Assert.Equal(6, resultAt0.Value<int>("Total"));
            Assert.Equal("WA", Assert.IsType<JObject>(resultAt0.GetValue("Address")).Value<string>("State"));
            Assert.Equal(2, resultAt1.Value<int>("MaxStateLen"));
            Assert.Equal(9, resultAt1.Value<int>("Total"));
            Assert.Equal("UK", Assert.IsType<JObject>(resultAt1.GetValue("Address")).Value<string>("State"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=compute(length(Name) as NameLen)/groupby((NameLen),aggregate(Id with sum as Total))")]
        [InlineData("custom/Employees?$apply=compute(length(Name) as NameLen)/groupby((NameLen),aggregate(Id with sum as Total))")]
        public async Task TestComputeStringPropertyLengthThenGroupByComputedPropertyAndAggregatePrimitivePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            Assert.Equal(13, resultAt0.Value<int>("NameLen"));
            Assert.Equal(3, resultAt0.Value<int>("Total"));
            Assert.Equal(15, resultAt1.Value<int>("NameLen"));
            Assert.Equal(3, resultAt1.Value<int>("Total"));
            Assert.Equal(14, resultAt2.Value<int>("NameLen"));
            Assert.Equal(9, resultAt2.Value<int>("Total"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=compute(length(Address/City) as CityLength)/groupby((Address/State),aggregate(Address/City with max as MaxCity,Address/City with min as MinCity,CityLength with max as MaxCityLen))")]
        [InlineData("custom/Employees?$apply=compute(length(Address/City) as CityLength)/groupby((Address/State),aggregate(Address/City with max as MaxCity,Address/City with min as MinCity,CityLength with max as MaxCityLen))")]
        public async Task TestComputeNestedStringPropertyLengthThenGroupByNestedPropertyAndAggregateComputedAndNestedPropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            Assert.Equal(8, resultAt0.Value<int>("MaxCityLen"));
            Assert.Equal("Kirkland", resultAt0.Value<string>("MinCity"));
            Assert.Equal("Tacoma", resultAt0.Value<string>("MaxCity"));
            Assert.Equal("WA", Assert.IsType<JObject>(resultAt0.GetValue("Address")).Value<string>("State"));
            Assert.Equal(6, resultAt1.Value<int>("MaxCityLen"));
            Assert.Equal("London", resultAt1.Value<string>("MinCity"));
            Assert.Equal("London", resultAt1.Value<string>("MaxCity"));
            Assert.Equal("UK", Assert.IsType<JObject>(resultAt1.GetValue("Address")).Value<string>("State"));
        }

        [Theory]
        [InlineData("default/Employees?$apply=groupby((Gender),aggregate(BaseSalary with max as MaxSalary))&$orderby=MaxSalary desc")]
        [InlineData("custom/Employees?$apply=groupby((Gender),aggregate(BaseSalary with max as MaxSalary))&$orderby=MaxSalary desc")]
        public async Task TestGroupByPrimitivePropertyAndAggregatePrimitivePropertyThenOrderByAggregatedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            Assert.Equal("Male", resultAt0.Value<string>("Gender"));
            Assert.Equal(1500m, resultAt0.Value<decimal>("MaxSalary"));
            Assert.Equal("Female", resultAt1.Value<string>("Gender"));
            Assert.Equal(1300m, resultAt1.Value<decimal>("MaxSalary"));
        }

        private Task<HttpResponseMessage> SetupAndFireRequestAsync(string queryUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            var client = CreateClient();

            return client.SendAsync(request);
        }
    }
}
#endif
