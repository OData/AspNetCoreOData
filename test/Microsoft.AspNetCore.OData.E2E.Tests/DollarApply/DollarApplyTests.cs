//-----------------------------------------------------------------------------
// <copyright file="DollarApplyTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

// Conditional compilation due to known bug affecting EF Core and lower
#if NET6_0_OR_GREATER
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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

            services.AddDbContext<DollarApplyDbContext>(opt => opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.ConfigureControllers(typeof(SalesController));

            services.AddControllers().AddOData(options =>
            {
                options.EnableQueryFeatures();
                options.AddRouteComponents("default", model);
                options.AddRouteComponents("custom", model, (nestedServices) =>
                {
                    nestedServices.AddSingleton<IAggregationBinder, TestAggregationBinder>();
                });
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
        public async Task TestGroupBySinglePropertyAsync(string queryUrl)
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
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal("2022-4", resultAt3.Value<string>("Quarter"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Year))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Year))")]
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
        public async Task TestGroupBySingleNestedPropertyAsync(string queryUrl)
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
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id))")]
        public async Task TestGroupByMultipleNestedPropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            var resultAt6 = Assert.IsType<JObject>(result[6]);
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Sugar", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Paper", resultAt6.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name))")]
        public async Task TestGroupByMultiNestedPropertyAsync(string queryUrl)
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
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id))")]
        public async Task TestGroupByHybridNestedPropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Food", resultAt2.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Non-Food", resultAt3.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Food", resultAt4.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal("Non-Food", resultAt5.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt5.Value<JObject>("Customer").Value<string>("Id"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name))")]
        public async Task TestGroupByNestedAndNonNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("2022-1", resultAt3.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("2022-4", resultAt4.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("2022-3", resultAt5.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt5.Value<JObject>("Product").Value<string>("Name"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Amount with sum as SumAmount)")]
        [InlineData("custom/Sales?$apply=aggregate(Amount with sum as SumAmount)")]
        public async Task TestAggregateSinglePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.Single(result);
            Assert.Equal(24m, resultAt0.Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Product/TaxRate with min as MinTaxRate)")]
        [InlineData("custom/Sales?$apply=aggregate(Product/TaxRate with min as MinTaxRate)")]
        public async Task TestAggregateSingleNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.Single(result);
            Assert.Equal(0.06m, resultAt0.Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate)")]
        [InlineData("custom/Sales?$apply=aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate)")]
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
        public async Task TestAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.Single(result);
            Assert.Equal(8, resultAt0.Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Product/Name with countdistinct as DistinctProducts)")]
        [InlineData("custom/Sales?$apply=aggregate(Product/Name with countdistinct as DistinctProducts)")]
        public async Task TestAggregateSingleNestedPropertyWithCountDistinctAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.Single(result);
            Assert.Equal(3, resultAt0.Value<int>("DistinctProducts"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupBySinglePropertyAndAggregateSinglePropertyAsync(string queryUrl)
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
        public async Task TestGroupBySinglePropertyAndAggregateSingleNestedPropertyAsync(string queryUrl)
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
        public async Task TestGroupBySinglePropertyAndAggregateMultiplePropertiesAsync(string queryUrl)
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
        public async Task TestGroupByMultiplePropertiesAndAggregateSinglePropertyAsync(string queryUrl)
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
        public async Task TestGroupByMultiplePropertiesAndAggregateSingleNestedPropertyAsync(string queryUrl)
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
        public async Task TestGroupBySingleNestedPropertyAndAggregateSinglePropertyAsync(string queryUrl)
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
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(8m, resultAt0.Value<decimal>("SumAmount"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(4m, resultAt1.Value<decimal>("SumAmount"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(12m, resultAt2.Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupBySingleNestedPropertyAndAggregateSingleNestedPropertyAsync(string queryUrl)
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
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MinTaxRate"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MinTaxRate"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupBySingleNestedPropertyAndAggregateMultiplePropertiesAsync(string queryUrl)
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
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(2m, resultAt0.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(2m, resultAt1.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(6m, resultAt2.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MaxTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateSinglePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            var resultAt6 = Assert.IsType<JObject>(result[6]);
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1m, resultAt0.Value<decimal>("SumAmount"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2m, resultAt1.Value<decimal>("SumAmount"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(4m, resultAt2.Value<decimal>("SumAmount"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(8m, resultAt3.Value<decimal>("SumAmount"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(4m, resultAt4.Value<decimal>("SumAmount"));
            Assert.Equal("Sugar", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2m, resultAt5.Value<decimal>("SumAmount"));
            Assert.Equal("Paper", resultAt6.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(3m, resultAt6.Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateSingleNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            var resultAt6 = Assert.IsType<JObject>(result[6]);
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MinTaxRate"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MinTaxRate"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MinTaxRate"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.06m, resultAt3.Value<decimal>("MinTaxRate"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.14m, resultAt4.Value<decimal>("MinTaxRate"));
            Assert.Equal("Sugar", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.06m, resultAt5.Value<decimal>("MinTaxRate"));
            Assert.Equal("Paper", resultAt6.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.14m, resultAt6.Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            var resultAt6 = Assert.IsType<JObject>(result[6]);
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1m, resultAt0.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2m, resultAt1.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(4m, resultAt2.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(8m, resultAt3.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt3.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(4m, resultAt4.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt4.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Sugar", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2m, resultAt5.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt5.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Paper", resultAt6.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1.5m, resultAt6.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt6.Value<decimal>("MaxTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupBySingleMultiNestedPropertyAndAggregateSinglePropertyAsync(string queryUrl)
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
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(8m, resultAt0.Value<decimal>("SumAmount"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(16m, resultAt1.Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupBySingleMultiNestedPropertyAndAggregateSingleNestedPropertyAsync(string queryUrl)
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
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MinTaxRate"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupBySingleMultiNestedPropertyAndAggregateMultiplePropertiesAsync(string queryUrl)
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
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(2m, resultAt0.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(4m, resultAt1.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MaxTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregateSinglePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1m, resultAt0.Value<decimal>("SumAmount"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(6m, resultAt1.Value<decimal>("SumAmount"));
            Assert.Equal("Food", resultAt2.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(8m, resultAt2.Value<decimal>("SumAmount"));
            Assert.Equal("Non-Food", resultAt3.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(4m, resultAt3.Value<decimal>("SumAmount"));
            Assert.Equal("Food", resultAt4.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2m, resultAt4.Value<decimal>("SumAmount"));
            Assert.Equal("Non-Food", resultAt5.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt5.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(3m, resultAt5.Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregateSingleNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MinTaxRate"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MinTaxRate"));
            Assert.Equal("Food", resultAt2.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MinTaxRate"));
            Assert.Equal("Non-Food", resultAt3.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.14m, resultAt3.Value<decimal>("MinTaxRate"));
            Assert.Equal("Food", resultAt4.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.06m, resultAt4.Value<decimal>("MinTaxRate"));
            Assert.Equal("Non-Food", resultAt5.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt5.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(0.14m, resultAt5.Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByHybridNestedPropertiesAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1m, resultAt0.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(3m, resultAt1.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Food", resultAt2.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(8m, resultAt2.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Non-Food", resultAt3.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(4m, resultAt3.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt3.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Food", resultAt4.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2m, resultAt4.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt4.Value<decimal>("MaxTaxRate"));
            Assert.Equal("Non-Food", resultAt5.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt5.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1.5m, resultAt5.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt5.Value<decimal>("MaxTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with sum as SumAmount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with sum as SumAmount))")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregateSinglePropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(1m, resultAt0.Value<decimal>("SumAmount"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(4m, resultAt1.Value<decimal>("SumAmount"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(4m, resultAt2.Value<decimal>("SumAmount"));
            Assert.Equal("2022-1", resultAt3.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(8m, resultAt3.Value<decimal>("SumAmount"));
            Assert.Equal("2022-4", resultAt4.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(6m, resultAt4.Value<decimal>("SumAmount"));
            Assert.Equal("2022-3", resultAt5.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(1m, resultAt5.Value<decimal>("SumAmount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Product/TaxRate with min as MinTaxRate))")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregateSingleNestedPropertyAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-1", resultAt3.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.06m, resultAt3.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-4", resultAt4.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.14m, resultAt4.Value<decimal>("MinTaxRate"));
            Assert.Equal("2022-3", resultAt5.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(0.14m, resultAt5.Value<decimal>("MinTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name),aggregate(Amount with average as AverageAmount,Product/TaxRate with max as MaxTaxRate))")]
        public async Task TestGroupByNestedAndNonNestedPropertyAndAggregateMultiplePropertiesAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(1m, resultAt0.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt0.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(2m, resultAt1.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt1.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(4m, resultAt2.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt2.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-1", resultAt3.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(8m, resultAt3.Value<decimal>("AverageAmount"));
            Assert.Equal(0.06m, resultAt3.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-4", resultAt4.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(3m, resultAt4.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt4.Value<decimal>("MaxTaxRate"));
            Assert.Equal("2022-3", resultAt5.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(1m, resultAt5.Value<decimal>("AverageAmount"));
            Assert.Equal(0.14m, resultAt5.Value<decimal>("MaxTaxRate"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate($count as SalesCount))")]
        public async Task TestGroupBySinglePropertyAndAggregateDollarCountAsync(string queryUrl)
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
        public async Task TestGroupBySingleNestedPropertyAndAggregateDollarCountAsync(string queryUrl)
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
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(4, resultAt0.Value<int>("SalesCount"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(2, resultAt1.Value<int>("SalesCount"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(2, resultAt2.Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Name,Customer/Id),aggregate($count as SalesCount))")]
        public async Task TestGroupByMultipleNestedPropertiesAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(7, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            var resultAt6 = Assert.IsType<JObject>(result[6]);
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt0.Value<int>("SalesCount"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt1.Value<int>("SalesCount"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C1", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt2.Value<int>("SalesCount"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt3.Value<int>("SalesCount"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C2", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt4.Value<int>("SalesCount"));
            Assert.Equal("Sugar", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt5.Value<int>("SalesCount"));
            Assert.Equal("Paper", resultAt6.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal("C3", resultAt6.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2, resultAt6.Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate($count as SalesCount))")]
        public async Task TestGroupBySingleMultiNestedPropertyAndAggregateDollarCountAsync(string queryUrl)
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
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(4, resultAt0.Value<int>("SalesCount"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(4, resultAt1.Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name,Customer/Id),aggregate($count as SalesCount))")]
        public async Task TestGroupByMultipleHybridNestedPropertiesAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt0.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt0.Value<int>("SalesCount"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C1", resultAt1.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2, resultAt1.Value<int>("SalesCount"));
            Assert.Equal("Food", resultAt2.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt2.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt2.Value<int>("SalesCount"));
            Assert.Equal("Non-Food", resultAt3.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C2", resultAt3.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt3.Value<int>("SalesCount"));
            Assert.Equal("Food", resultAt4.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt4.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(1, resultAt4.Value<int>("SalesCount"));
            Assert.Equal("Non-Food", resultAt5.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal("C3", resultAt5.Value<JObject>("Customer").Value<string>("Id"));
            Assert.Equal(2, resultAt5.Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter,Product/Name),aggregate($count as SalesCount))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter,Product/Name),aggregate($count as SalesCount))")]
        public async Task TestGroupByNestedAndNonNestedPropertiesAndAggregateDollarCountAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            Assert.Equal(6, result.Count);
            var resultAt0 = Assert.IsType<JObject>(result[0]);
            var resultAt1 = Assert.IsType<JObject>(result[1]);
            var resultAt2 = Assert.IsType<JObject>(result[2]);
            var resultAt3 = Assert.IsType<JObject>(result[3]);
            var resultAt4 = Assert.IsType<JObject>(result[4]);
            var resultAt5 = Assert.IsType<JObject>(result[5]);
            Assert.Equal("2022-1", resultAt0.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt0.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(1, resultAt0.Value<int>("SalesCount"));
            Assert.Equal("2022-2", resultAt1.Value<string>("Quarter"));
            Assert.Equal("Sugar", resultAt1.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(2, resultAt1.Value<int>("SalesCount"));
            Assert.Equal("2022-3", resultAt2.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt2.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(1, resultAt2.Value<int>("SalesCount"));
            Assert.Equal("2022-1", resultAt3.Value<string>("Quarter"));
            Assert.Equal("Coffee", resultAt3.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(1, resultAt3.Value<int>("SalesCount"));
            Assert.Equal("2022-4", resultAt4.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt4.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(2, resultAt4.Value<int>("SalesCount"));
            Assert.Equal("2022-3", resultAt5.Value<string>("Quarter"));
            Assert.Equal("Paper", resultAt5.Value<JObject>("Product").Value<string>("Name"));
            Assert.Equal(1, resultAt5.Value<int>("SalesCount"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Quarter),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        [InlineData("custom/Sales?$apply=groupby((Quarter),aggregate(Product/Name with countdistinct as DistinctProducts))")]
        public async Task TestGroupBySinglePropertyAndAggregateSingleNestedPropertyWithCountDistinctAsync(string queryUrl)
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
        public async Task TestGroupByMultiplePropertiesAndAggregateSingleNestedPropertyWithCountDistinctAsync(string queryUrl)
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
        public async Task TestAggregateSinglePropertyWithCustomAggregateFunctionAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.Single(result);
            Assert.Equal(2.32992949004287, resultAt0.Value<double>("StdDev"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=aggregate(Product/TaxRate with custom.stdev as StdDev)")]
        [InlineData("custom/Sales?$apply=aggregate(Product/TaxRate with custom.stdev as StdDev)")]
        public async Task TestAggregateSingleNestedPropertyWithCustomAggregateFunctionAsync(string queryUrl)
        {
            // Arrange & Act
            var response = await SetupAndFireRequestAsync(queryUrl);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsObject<JObject>();
            var result = content.GetValue("value") as JArray;
            Assert.NotNull(result);
            var resultAt0 = Assert.Single(result);
            Assert.Equal(0.042761798705987904, resultAt0.Value<double>("StdDev"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with custom.stdev as StdDev))")]
        [InlineData("custom/Sales?$apply=groupby((Product/Category/Name),aggregate(Amount with custom.stdev as StdDev))")]
        public async Task TestGroupBySingleMultiNestedPropertyAndAggregateSinglePropertyWithCustomAggregateFunctionAsync(string queryUrl)
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
            Assert.Equal("Non-Food", resultAt0.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(1.4142135623730951, resultAt0.Value<double>("StdDev"));
            Assert.Equal("Food", resultAt1.Value<JObject>("Product").Value<JObject>("Category").Value<string>("Name"));
            Assert.Equal(2.8284271247461903, resultAt1.Value<double>("StdDev"));
        }

        [Theory]
        [InlineData("default/Sales?$apply=groupby((Customer/Id),aggregate(Product/TaxRate with custom.stdev as StdDev))")]
        [InlineData("custom/Sales?$apply=groupby((Customer/Id),aggregate(Product/TaxRate with custom.stdev as StdDev))")]
        public async Task TestGroupBySingleNestedPropertyAndAggregateSingleNestedPropertyWithCustomAggregateFunctionAsync(string queryUrl)
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

        // TODO: Add tests for aggregating dynamic properties

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
