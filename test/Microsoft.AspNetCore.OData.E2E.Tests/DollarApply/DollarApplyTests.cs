//-----------------------------------------------------------------------------
// <copyright file="DollarApplyTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

// Conditional compilation due to known bug affecting EF Core and lower
#if NET6_0_OR_GREATER
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Expressions;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Wrapper;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
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
            services.ConfigureControllers(
                typeof(InMemorySalesController),
                typeof(SqlSalesController),
                typeof(EmployeesController),
                typeof(InMemoryProductsController),
                typeof(SqlProductsController));

            services.AddControllers().AddOData(options =>
            {
                options.EnableQueryFeatures();
                options.EnableAttributeRouting = true;

                // Due to how route matching works, `defaultsql` and `customsql` must be registered before `default` and `custom`
                options.AddRouteComponents("defaultsql", model);
                options.AddRouteComponents("customsql", model, (nestedServices) =>
                {
                    nestedServices.AddSingleton<IAggregationBinder, TestAggregationBinder>();
                    nestedServices.AddSingleton<IComputeBinder, TestComputeBinder>();
                });

                options.AddRouteComponents("default", model);
                options.AddRouteComponents("custom", model, (nestedServices) =>
                {
                    nestedServices.AddSingleton<IAggregationBinder, TestAggregationBinder>();
                    nestedServices.AddSingleton<IComputeBinder, TestComputeBinder>();
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByPrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Year))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultipleNestedPropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name,Customer/Id))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByHybridNestedPropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name,Customer/Id))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedAndNonNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Product/Name))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=aggregate(Amount with sum as SumAmount)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(24m, Assert.Single(result).Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=aggregate(Product/TaxRate with min as MinTaxRate)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(0.06m, Assert.Single(result).Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestAggregateMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate)";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestAggregateDollarCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=aggregate($count as SalesCount)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(8, Assert.Single(result).Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestAggregateNestedPropertyWithCountDistinctAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=aggregate(Product/Name with countdistinct as DistinctProducts)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, Assert.Single(result).Value<int>("DistinctProducts"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByPrimitivePropertyAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter),aggregate(Amount with sum as SumAmount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByPrimitivePropertyAndAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter),aggregate(Product/TaxRate with min as MinTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByPrimitivePropertyAndAggregateMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiplePropertiesAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with sum as SumAmount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiplePropertiesAndAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Year),aggregate(Product/TaxRate with min as MinTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiplePropertiesAndAggregateMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Year),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedPropertyAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name),aggregate(Amount with sum as SumAmount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedPropertyAndAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedPropertyAndAggregateMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with sum as SumAmount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiNestedPropertyAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with sum as SumAmount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiNestedPropertyAndAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name),aggregate(Product/TaxRate with min as MinTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiNestedPropertyAndAggregateMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with sum as SumAmount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregateMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with sum as SumAmount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregateMultiplePropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByPrimitivePropertyAndAggregateDollarCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter),aggregate($count as SalesCount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiplePropertiesAndAggregateDollarCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Year),aggregate($count as SalesCount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedPropertyAndAggregateDollarCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name),aggregate($count as SalesCount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateDollarCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate($count as SalesCount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiNestedPropertyAndAggregateDollarCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name),aggregate($count as SalesCount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultipleHybridNestedPropertiesAndAggregateDollarCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate($count as SalesCount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedAndNonNestedPropertiesAndAggregateDollarCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Product/Name),aggregate($count as SalesCount))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByPrimitivePropertyAndAggregateNestedPropertyWithCountDistinctAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter),aggregate(Product/Name with countdistinct as DistinctProducts))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiplePropertiesAndAggregateNestedPropertyWithCountDistinctAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter,Year),aggregate(Product/Name with countdistinct as DistinctProducts))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestAggregatePrimitivePropertyWithCustomAggregateFunctionAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=aggregate(Amount with custom.stdev as StdDev)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2.32992949004287, Assert.Single(result).Value<double>("StdDev"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestAggregateNestedPropertyWithCustomAggregateFunctionAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=aggregate(Product/TaxRate with custom.stdev as StdDev)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(0.042761798705987904, Assert.Single(result).Value<double>("StdDev"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByMultiNestedPropertyAndAggregatePrimitivePropertyWithCustomAggregateFunctionAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with custom.stdev as StdDev))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByNestedPropertyAndAggregateNestedPropertyWithCustomAggregateFunctionAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Customer/Id),aggregate(Product/TaxRate with custom.stdev as StdDev))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByDynamicPrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Gender))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestAggregateSingleDynamicPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=aggregate(Commission with average as AverageCommission)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(280, Assert.Single(result).Value<decimal>("AverageCommission"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByDynamicPrimitivePropertyAndAggregateDynamicPrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Gender),aggregate(Commission with sum as SumCommission))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByDynamicPrimitivePropertyAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Gender),aggregate(BaseSalary with max as MaxSalary))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByPrimitivePropertyAndAggregatePrimitivePropertyThenGroupByPrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter),aggregate(Amount with min as MinAmount))/groupby((Quarter))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeNestedStringPropertyLengthThenAggregateComputedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength with sum as CombinedProductNameLength)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(42, Assert.Single(result).Value<int>("CombinedProductNameLength"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeNestedStringPropertyLengthThenAggregateSumOfComputedAndPrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/aggregate(ProductNameLength add Id with sum as CombinedProductNameLength)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(78, Assert.IsType<JObject>(Assert.Single(result)).Value<int>("CombinedProductNameLength"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeNestedStringPropertyLengthThenGroupByNestedPropertyAndAggregatePrimitiveAndComputedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute(length(Product/Name) as ProductNameLength)/groupby((Product/Name),aggregate(Id with sum as Total, ProductNameLength with max as MaxProductNameLength))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByMultipleNestedPropertiesThenGroupByNestedPropertyAndAggregateNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Address/City,Address/State))/groupby((Address/State),aggregate(Address/City with max as MaxCity))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByMultipleMultiNestedPropertiesThenGroupByMultiNestedPropertyAndAggregateMultiNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Company/VP/Address/City,Company/VP/Address/State))/groupby((Company/VP/Address/State),aggregate(Company/VP/Address/City with max as MaxCity))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByMultipleMultiNestedPropertiesThenGroupByMultiNestedPropertyAndAggregateMultiNestedPropertyWithMaxAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Company/VP/Name,Company/VP/BaseSalary))/groupby((Company/VP/BaseSalary),aggregate(Company/VP/Name with max as MaxName))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByMultipleMultiNestedPropertiesThenGroupByMultiNestedPropertyAndAverageMultiNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Company/VP/Name,Company/VP/BaseSalary))/groupby((Company/VP/Name),aggregate(Company/VP/BaseSalary with average as AverageBaseSalary))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByNestedPropertyAndAggregatePrimitivePropertyThenGroupByNestedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Address/State),aggregate(BaseSalary with min as MinBaseSalary))/groupby((Address/State))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestComputeStringPropertyLengthThenAggregateComputedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=compute(length(Name) as NameLen)/aggregate(NameLen with sum as TotalLen)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(55, Assert.IsType<JObject>(Assert.Single(result)).Value<int>("TotalLen"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestComputeStringPropertyLengthThenAggregateSumOfComputedPropertyAndPrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=compute(length(Name) as NameLen)/aggregate(NameLen add Id with sum as TotalLen)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(70, Assert.IsType<JObject>(Assert.Single(result)).Value<int>("TotalLen"));
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestComputeNestedStringPropertyLengthThenGroupByNestedPropertyAndAggregateComputedAndPrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=compute(length(Address/State) as StateLen)/groupby((Address/State),aggregate(Id with sum as Total,StateLen with max as MaxStateLen))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestComputeStringPropertyLengthThenGroupByComputedPropertyAndAggregatePrimitivePropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=compute(length(Name) as NameLen)/groupby((NameLen),aggregate(Id with sum as Total))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestComputeNestedStringPropertyLengthThenGroupByNestedPropertyAndAggregateComputedAndNestedPropertiesAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=compute(length(Address/City) as CityLength)/groupby((Address/State),aggregate(Address/City with max as MaxCity,Address/City with min as MinCity,CityLength with max as MaxCityLen))";

            // Act
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
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestGroupByPrimitivePropertyAndAggregatePrimitivePropertyThenOrderByAggregatedPropertyAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=groupby((Gender),aggregate(BaseSalary with max as MaxSalary))&$orderby=MaxSalary desc";

            // Act
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

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByChainingAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Year,Quarter),aggregate(Amount with average as AverageAmount, Amount with sum as SumAmount))/groupby((Year),aggregate(AverageAmount with average as AnnualAverageAmount,SumAmount with sum as AnnualSumAmount))";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Matches("\\{\"Year\":2022,\"AnnualSumAmount\":24(?:\\.0+)?,\"AnnualAverageAmount\":3(?:\\.0+)?\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        public async Task TestComputeWithSubstringAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Employees?$apply=compute(substring(Name, 1, 4) as comp)&$top=1&$count=true";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(content = "{\"@odata.count\":4,\"value\":[{\"comp\":\"ancy\",\"Id\":1,\"Name\":\"Nancy Davolio\",\"BaseSalary\":1300}]}", content);
        }

        #region https://github.com/OData/AspNetCoreOData/issues/420 & Related

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByAndAggregateThenFilterAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Customer/Id),aggregate($count as SalesCount,Amount with sum as SumAmount))&$filter=SumAmount gt 5";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"SumAmount\":12(?:\\.0+)?,\"SalesCount\":2,\"Customer\":\\{\"Id\":\"C2\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":7(?:\\.0+)?,\"SalesCount\":3,\"Customer\":\\{\"Id\":\"C1\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByAndAggregateThenFilterTransformationAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Customer/Id),aggregate($count as SalesCount,Amount with sum as SumAmount))/filter(SumAmount gt 5)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"SumAmount\":12(?:\\.0+)?,\"SalesCount\":2,\"Customer\":\\{\"Id\":\"C2\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":7(?:\\.0+)?,\"SalesCount\":3,\"Customer\":\\{\"Id\":\"C1\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByAndAggregateThenOrderByAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Customer/Id),aggregate($count as SalesCount,Amount with sum as SumAmount))&$orderby=SumAmount desc";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Matches("\\{\"SumAmount\":12(?:\\.0+)?,\"SalesCount\":2,\"Customer\":\\{\"Id\":\"C2\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":7(?:\\.0+)?,\"SalesCount\":3,\"Customer\":\\{\"Id\":\"C1\"\\}\\}", content);
            Assert.Matches("\\{\"SumAmount\":5(?:\\.0+)?,\"SalesCount\":3,\"Customer\":\\{\"Id\":\"C3\"\\}\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestFilterTransformationThenGroupByAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=filter((endswith(Quarter, '-3') eq true))/groupby((Quarter))";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Matches("\\{\"Quarter\":\"2022-3\"\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByThenTopWithCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter))&$count=true&$top=3";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var parsedContent = JObject.Parse(content);
            var count = parsedContent.Value<int>("@odata.count");
            Assert.Equal(4, count);
            var result = parsedContent.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\"\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\"\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\"\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestGroupByAndAggregateThenOrderByWithCountAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=groupby((Quarter),aggregate($count as SalesCount))&$count=true&$orderby=Quarter";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var parsedContent = JObject.Parse(content);
            var count = parsedContent.Value<int>("@odata.count");
            Assert.Equal(4, count);
            var result = parsedContent.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"SalesCount\":2\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\",\"SalesCount\":2\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"SalesCount\":2\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-4\",\"SalesCount\":2\\}", content);
        }

        #endregion https://github.com/OData/AspNetCoreOData/issues/420 & Related

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestEntitySetAggregationWorksAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Products?$apply=groupby((Category/Name),aggregate(Sales(Amount with sum as SalesTotal,$count as SalesCount)))";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Matches("\\{\"Category\":\\{\"Name\":\"Food\"\\},\"Sales\":\\[\\{\"SalesCount\":4,\"SalesTotal\":16(?:\\.0+)?\\}\\]\\}", content);
            Assert.Matches("\\{\"Category\":\\{\"Name\":\"Non-Food\"\\},\"Sales\":\\[\\{\"SalesCount\":4,\"SalesTotal\":8(?:\\.0+)?\\}\\]\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeWithCommonOperatorsAsync(string routePrefix)
        {
            // Arrange
            // Tax = Amount * Product/TaxRate, Discount = Amount / 10, TotalPrice = Amount + Tax, SalesPrice = TotalPrice - Discount
            var queryUrl = $"{routePrefix}/Sales?$apply=compute((Amount add (Amount mul Product/TaxRate)) sub (Amount div 10) as SalesPrice)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(8, result.Count);
            Assert.Matches("\\{\"SalesPrice\":1\\.04(?:0+)?,\"Id\":1,\"Year\":2022,\"Quarter\":\"2022-1\",\"Amount\":1(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":1\\.92(?:0+)?,\"Id\":2,\"Year\":2022,\"Quarter\":\"2022-2\",\"Amount\":2(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":3\\.84(?:0+)?,\"Id\":3,\"Year\":2022,\"Quarter\":\"2022-3\",\"Amount\":4(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":7\\.68(?:0+)?,\"Id\":4,\"Year\":2022,\"Quarter\":\"2022-1\",\"Amount\":8(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":4\\.16(?:0+)?,\"Id\":5,\"Year\":2022,\"Quarter\":\"2022-4\",\"Amount\":4(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":1\\.92(?:0+)?,\"Id\":6,\"Year\":2022,\"Quarter\":\"2022-2\",\"Amount\":2(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":1\\.04(?:0+)?,\"Id\":7,\"Year\":2022,\"Quarter\":\"2022-3\",\"Amount\":1(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":2\\.08(?:0+)?,\"Id\":8,\"Year\":2022,\"Quarter\":\"2022-4\",\"Amount\":2(?:\\.0+)?\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeWithCommonOperatorsThenAggregateAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute((Amount add (Amount mul Product/TaxRate)) sub (Amount div 10) as SalesPrice)/aggregate(SalesPrice with average as AverageSalesPrice)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Matches("\\{\"AverageSalesPrice\":2\\.96(?:0+)?\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeWithCommonOperatorsThenGroupByAggregateAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute((Amount add (Amount mul Product/TaxRate)) sub (Amount div 10) as SalesPrice)/groupby((Quarter),aggregate(SalesPrice with average as QtrAvgSalesPrice))";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"QtrAvgSalesPrice\":4\\.36(?:0+)?\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\",\"QtrAvgSalesPrice\":1\\.92(?:0+)?\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"QtrAvgSalesPrice\":2\\.44(?:0+)?\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-4\",\"QtrAvgSalesPrice\":3\\.12(?:0+)?\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeChainingAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute(Amount mul Product/TaxRate as Tax)" +
                "/compute(Amount add Tax as Total,Amount div 10 as Discount)" +
                "/compute(Total sub Discount as SalesPrice)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(8, result.Count);
            Assert.Matches("\\{\"SalesPrice\":1\\.04(?:0+)?,\"Discount\":0\\.1(?:0+)?,\"Total\":1\\.14(?:0+)?,\"Tax\":0\\.14(?:0+)?,\"Id\":1,\"Year\":2022,\"Quarter\":\"2022-1\",\"Amount\":1(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":1\\.92(?:0+)?,\"Discount\":0\\.2(?:0+)?,\"Total\":2\\.12(?:0+)?,\"Tax\":0\\.12(?:0+)?,\"Id\":2,\"Year\":2022,\"Quarter\":\"2022-2\",\"Amount\":2(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":3\\.84(?:0+)?,\"Discount\":0\\.4(?:0+)?,\"Total\":4\\.24(?:0+)?,\"Tax\":0\\.24(?:0+)?,\"Id\":3,\"Year\":2022,\"Quarter\":\"2022-3\",\"Amount\":4(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":7\\.68(?:0+)?,\"Discount\":0\\.8(?:0+)?,\"Total\":8\\.48(?:0+)?,\"Tax\":0\\.48(?:0+)?,\"Id\":4,\"Year\":2022,\"Quarter\":\"2022-1\",\"Amount\":8(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":4\\.16(?:0+)?,\"Discount\":0\\.4(?:0+)?,\"Total\":4\\.56(?:0+)?,\"Tax\":0\\.56(?:0+)?,\"Id\":5,\"Year\":2022,\"Quarter\":\"2022-4\",\"Amount\":4(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":1\\.92(?:0+)?,\"Discount\":0\\.2(?:0+)?,\"Total\":2\\.12(?:0+)?,\"Tax\":0\\.12(?:0+)?,\"Id\":6,\"Year\":2022,\"Quarter\":\"2022-2\",\"Amount\":2(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":1\\.04(?:0+)?,\"Discount\":0\\.1(?:0+)?,\"Total\":1\\.14(?:0+)?,\"Tax\":0\\.14(?:0+)?,\"Id\":7,\"Year\":2022,\"Quarter\":\"2022-3\",\"Amount\":1(?:\\.0+)?\\}", content);
            Assert.Matches("\\{\"SalesPrice\":2\\.08(?:0+)?,\"Discount\":0\\.2(?:0+)?,\"Total\":2\\.28(?:0+)?,\"Tax\":0\\.28(?:0+)?,\"Id\":8,\"Year\":2022,\"Quarter\":\"2022-4\",\"Amount\":2(?:\\.0+)?\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeChainingThenAggregateAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute(Amount mul Product/TaxRate as Tax)" +
                "/compute(Amount add Tax as Total,Amount div 10 as Discount)" +
                "/compute(Total sub Discount as SalesPrice)" +
                "/aggregate(SalesPrice with average as AverageSalesPrice,Amount with average as AverageAmount)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Matches("\\{\"AverageAmount\":3(?:\\.0+)?,\"AverageSalesPrice\":2\\.96(?:0+)?\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeChainingThenGroupByAndAggregateAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute(Amount mul Product/TaxRate as Tax)" +
                "/compute(Amount add Tax as Total,Amount div 10 as Discount)" +
                "/compute(Total sub Discount as SalesPrice)" +
                "/groupby((Quarter),aggregate(SalesPrice with average as QtrAvgSalesPrice,Amount with average as QtrAvgAmount))";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Matches("\\{\"Quarter\":\"2022-1\",\"QtrAvgAmount\":4\\.5(?:0+)?,\"QtrAvgSalesPrice\":4\\.36(?:0+)?\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-2\",\"QtrAvgAmount\":2(?:\\.0+)?,\"QtrAvgSalesPrice\":1\\.92(?:0+)?\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-3\",\"QtrAvgAmount\":2\\.5(?:0+)?,\"QtrAvgSalesPrice\":2\\.44(?:0+)?\\}", content);
            Assert.Matches("\\{\"Quarter\":\"2022-4\",\"QtrAvgAmount\":3(?:\\.0+)?,\"QtrAvgSalesPrice\":3\\.12(?:0+)?\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeThenAggregateThenComputeAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute(Amount mul Product/TaxRate as Tax,Amount div 10 as Discount)" +
                "/aggregate(Tax with average as AverageTax,Discount with average as AverageDiscount,Amount with average as AverageAmount)" +
                "/compute(AverageAmount add AverageTax as AverageTotal,AverageAmount add AverageTax sub AverageDiscount as AverageSalesPrice)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Matches("\\{\"AverageSalesPrice\":2\\.96(?:0+)?,\"AverageTotal\":3\\.26(?:0+)?,\"AverageAmount\":3(?:\\.0+)?,\"AverageDiscount\":0\\.3(?:0+)?,\"AverageTax\":0\\.26(?:0+)?\\}", content);
        }

        [Theory]
        [InlineData("default")]
        [InlineData("custom")]
        [InlineData("defaultsql")]
        [InlineData("customsql")]
        public async Task TestComputeThenGroupByAndAggregateThenComputeAsync(string routePrefix)
        {
            // Arrange
            var queryUrl = $"{routePrefix}/Sales?$apply=compute(Amount mul Product/TaxRate as Tax,Amount div 10 as Discount)" +
                "/groupby((Quarter),aggregate(Tax with average as QtrAvgTax,Discount with average as QtrAvgDiscount,Amount with average as QtrAvgAmount))" +
                "/compute(QtrAvgAmount add QtrAvgTax as QtrAvgTotal,QtrAvgAmount add QtrAvgTax sub QtrAvgDiscount as QtrAvgSalesPrice)";

            // Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(content).GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Matches("\\{\"QtrAvgSalesPrice\":4\\.36(?:0+)?,\"QtrAvgTotal\":4\\.81(?:0+)?,\"Quarter\":\"2022-1\",\"QtrAvgAmount\":4\\.5(?:0+)?,\"QtrAvgDiscount\":0\\.45(?:0+)?,\"QtrAvgTax\":0\\.31(?:0+)?\\}", content);
            Assert.Matches("\\{\"QtrAvgSalesPrice\":1\\.92(?:0+)?,\"QtrAvgTotal\":2\\.12(?:0+)?,\"Quarter\":\"2022-2\",\"QtrAvgAmount\":2(?:\\.0+)?,\"QtrAvgDiscount\":0\\.2(?:0+)?,\"QtrAvgTax\":0\\.12(?:0+)?\\}", content);
            Assert.Matches("\\{\"QtrAvgSalesPrice\":2\\.44(?:0+)?,\"QtrAvgTotal\":2\\.69(?:0+)?,\"Quarter\":\"2022-3\",\"QtrAvgAmount\":2\\.5(?:0+)?,\"QtrAvgDiscount\":0\\.25(?:0+)?,\"QtrAvgTax\":0\\.19(?:0+)?\\}", content);
            Assert.Matches("\\{\"QtrAvgSalesPrice\":3\\.12(?:0+)?,\"QtrAvgTotal\":3\\.42(?:0+)?,\"Quarter\":\"2022-4\",\"QtrAvgAmount\":3(?:\\.0+)?,\"QtrAvgDiscount\":0\\.3(?:0+)?,\"QtrAvgTax\":0\\.42(?:0+)?\\}", content);
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
