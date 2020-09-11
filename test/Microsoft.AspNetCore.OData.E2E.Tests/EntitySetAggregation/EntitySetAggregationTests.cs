// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.EntitySetAggregation
{
    public class EntitySetAggregationTests : WebODataTestBase<EntitySetAggregationTests.TestsStartup>
    {
        public class TestsStartup : TestStartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                // Use the sql server got the access error.
                //services.AddDbContext<ProductsContext>(opt => opt.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=CastProductsContext;Trusted_Connection=True;"));
                services.AddDbContext<EntitySetAggregationContext>(opt => opt.UseInMemoryDatabase("EntitySetAggregationTest"));

                services.AddControllers()
                    .ConfigureApplicationPartManager(pm =>
                    {
                        pm.FeatureProviders.Add(new WebODataControllerFeatureProvider(typeof(CustomersController)));
                    });

                IEdmModel edmModel = EntitySetAggregationEdmModel.GetEdmModel();
                services.AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5)
                    .AddModel("aggregation", edmModel));
            }
        }

        private const string AggregationTestBaseUrl = "aggregation/Customers";

        public EntitySetAggregationTests(WebODataTestFixture<TestsStartup> factory)
            : base(factory)
        {
        }

        [Theory]
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
            HttpResponseMessage response = Client.SendAsync(request).Result;

            // Assert
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var value = result["value"];
            var orders = value.First["Orders"];
            var TotalPrice = orders.First["TotalPrice"].ToObject<int>();

            Assert.Equal(expected, TotalPrice);
        }

        [Theory]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
}
