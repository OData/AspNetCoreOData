//-----------------------------------------------------------------------------
// <copyright file="EntitySetAggregationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.EntitySetAggregation
{
    // The test can't work in EFCore, because it's not supported with Groupby and select many on collection.
    // Later, we'd switch it to EF6.
    public class EntitySetAggregationTests : WebODataTestBase<EntitySetAggregationTests.TestsStartup>
    {
        public class TestsStartup : TestStartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                // Use the sql server got the access error.
                services.AddDbContext<EntitySetAggregationContext>(opt => opt.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EntitySetAggregationContext;Trusted_Connection=True;"));
                //services.AddDbContext<EntitySetAggregationContext>(opt => opt.UseInMemoryDatabase("EntitySetAggregationTest"));

                services.ConfigureControllers(typeof(CustomersController));

                IEdmModel edmModel = EntitySetAggregationEdmModel.GetEdmModel();
                services.AddControllers().AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(null)
                    .AddRouteComponents("aggregation", edmModel));
            }
        }

        private const string AggregationTestBaseUrl = "aggregation/Customers";

        public EntitySetAggregationTests(WebODataTestFixture<TestsStartup> factory)
            : base(factory)
        {
        }

        [Theory(Skip = "See the comments above")]
        [InlineData("sum",600)]
        [InlineData("min", 25)]
        [InlineData("max", 225)]
        [InlineData("average", 100)]
        public async Task AggregationOnEntitySetWorks(string method, int expected)
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=aggregate(Orders(Price with " + method + " as TotalPrice))";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var value = result["value"];
            var orders = value.First["Orders"];
            var TotalPrice = orders.First["TotalPrice"].ToObject<int>();

            Assert.Equal(expected, TotalPrice);
        }

        [Theory(Skip = "See the comments above")]
        [InlineData("?$apply=aggregate(Orders(Price with sum as TotalPrice, Id with sum as TotalId))")]
        [InlineData("?$apply=aggregate(Orders(Price with sum as TotalPrice), Orders(Id with sum as TotalId))")]
        public async Task MultipleAggregationOnEntitySetWorks(string queryString)
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + queryString;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["value"];
            var orders = value.First["Orders"];
            var totalPrice = orders.First["TotalPrice"].ToObject<int>();
            var totalId = orders.First["TotalId"].ToObject<int>();

            // OBS: DB uses sequential ID
            // Each Customer has 2 orders that cost 25*CustomerId and 75*CustomerId
            Assert.Equal(1 * (25 + 75) + 2 * (25 + 75) + 3 * (25 + 75), totalPrice);
            // Sum of the 6 Orders IDs
            Assert.Equal(1 + 2 + 3 + 4 + 5 + 6, totalId); 
        }

#if NET6_0_OR_GREATER

        [Fact]
        public async Task GroupByWithAggregationAndOrderByWorks()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=groupby((Name),aggregate(Orders(Price with sum as TotalPrice)))&$orderby=Name desc";
            string expectedResult = "{\"value\":[{\"Name\":\"Customer1\",\"Orders\":[{\"TotalPrice\":200}]},{\"Name\":\"Customer0\",\"Orders\":[{\"TotalPrice\":400}]}]}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stringObject = await response.Content.ReadAsStringAsync();
            
            Assert.Equal(expectedResult, stringObject.ToString());

            var result = await response.Content.ReadAsObject<JObject>();    
            var value = result["value"];
            var orders = value.First["Orders"];
            var totalPrice = orders.First["TotalPrice"].ToObject<int>();

            Assert.Equal(200, totalPrice);
        }

        [Fact]
        public async Task AggregationWithFilterWorks()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=filter((contains(Name,'Customer1')))/aggregate(Orders(Price with sum as TotalPrice))";
            string expectedResult = "{\"value\":[{\"Orders\":[{\"TotalPrice\":200}]}]}";
            
                  HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stringObject = await response.Content.ReadAsStringAsync();

            Assert.Equal(expectedResult, stringObject.ToString());
        }
          
        public async Task GroupByWithAggregationAndFilterByWorks()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=groupby((Name),aggregate(Orders(Price with sum as TotalPrice)))&$filter=Name eq 'Customer1'";
            string expectedResult = "{\"value\":[{\"Name\":\"Customer1\",\"Orders\":[{\"TotalPrice\":200}]}]}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var stringObject = await response.Content.ReadAsStringAsync();

            Assert.Equal(expectedResult, stringObject.ToString());
        }

#endif

        [Fact(Skip = "See the comments above")]
        public async Task AggregationOnEntitySetWorksWithPropertyAggregation()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=aggregate(Id with sum as TotalId, Orders(Price with sum as TotalPrice))";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["value"];
            var totalId = value.First["TotalId"].ToObject<int>();
            var orders = value.First["Orders"];
            var totalPrice = orders.First["TotalPrice"].ToObject<int>();

            // OBS: DB uses sequential ID
            // Each Customer has 2 orders that cost 25*CustomerId and 75*CustomerId
            Assert.Equal(1 * (25 + 75) + 2 * (25 + 75) + 3 * (25 + 75), totalPrice);
            // Sum of the first 3 Customers IDs
            Assert.Equal(1 + 2 + 3, totalId); 
        }

        [Fact(Skip = "See the comments above")]
        public async Task TestAggregationOnEntitySetsWithArithmeticOperators()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=aggregate(Orders(Price mul Price with sum as TotalPrice))";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["value"];
            var orders = value.First["Orders"];
            var TotalPrice = orders.First["TotalPrice"].ToObject<int>();

            Assert.Equal((1 + 4 + 9) * (25 * 25 + 75 * 75), TotalPrice);
        }

        [Fact(Skip = "See the comments above")]
        public async Task TestAggregationOnEntitySetsWithArithmeticOperatorsAndPropertyNavigation()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=aggregate(Orders(SaleInfo/Quantity mul SaleInfo/UnitPrice with sum as TotalPrice))";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();
            var value = result["value"];
            var orders = value.First["Orders"];
            var TotalPrice = orders.First["TotalPrice"].ToObject<int>();

            Assert.Equal(1 * (25 + 75) + 2 * (25 + 75) + 3 * (25 + 75), TotalPrice);
        }

        [Fact(Skip = "See the comments above")]
        public async Task AggregationOnEntitySetWorksWithGroupby()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=groupby((Name), aggregate(Orders(Price with sum as TotalPrice)))";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var value = result["value"];

            Assert.Equal("Customer0", value[0]["Name"].ToObject<string>());
            Assert.Equal("Customer1", value[1]["Name"].ToObject<string>());

            var customerZeroOrders = value[0]["Orders"];
            var customerZeroPrice = customerZeroOrders.First["TotalPrice"].ToObject<int>();
            Assert.Equal(1 * (25 + 75) + 3 * (25 + 75), customerZeroPrice);

            var customerOneOrders = value[1]["Orders"];
            var customerOnePrice = customerOneOrders.First["TotalPrice"].ToObject<int>();
            Assert.Equal(2 * (25 + 75), customerOnePrice);
        }
    }

    public class NestedComplexPropertyAggregationTests : WebApiTestBase<NestedComplexPropertyAggregationTests>
    {
        public NestedComplexPropertyAggregationTests(WebApiTestFixture<NestedComplexPropertyAggregationTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Employee>("Employees");

            services.ConfigureControllers(typeof(EmployeesController));

            services.AddControllers().AddOData(options => options.Select().Filter().OrderBy().Expand().Count().SkipToken().SetMaxTop(null)
                .AddRouteComponents("aggregation", builder.GetEdmModel()));
        }

        private const string AggregationTestBaseUrl = "aggregation/Employees";

        [Fact]
        public async Task GroupByComplexProperty()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=groupby((NextOfKin/Name))";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = this.CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(3, results.Count);
            Assert.Equal("NoK 1", (results[0]["NextOfKin"] as JObject)["Name"].ToString());
            Assert.Equal("NoK 2", (results[1]["NextOfKin"] as JObject)["Name"].ToString());
            Assert.Equal("NoK 3", (results[2]["NextOfKin"] as JObject)["Name"].ToString());
        }

        [Fact]
        public async Task GroupByNestedComplexProperty()
        {
            // Arrange
            string queryUrl = AggregationTestBaseUrl + "?$apply=groupby((NextOfKin/PhysicalAddress/City))";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = this.CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var results = result["value"] as JArray;
            Assert.Equal(2, results.Count);
            Assert.Equal("Redmond", ((results[0]["NextOfKin"] as JObject)["PhysicalAddress"] as JObject)["City"].ToString());
            Assert.Equal("Nairobi", ((results[1]["NextOfKin"] as JObject)["PhysicalAddress"] as JObject)["City"].ToString());
        }
    }
}
