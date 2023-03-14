//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ServerSidePaging
{
    public class ServerSidePagingTests : WebApiTestBase<ServerSidePagingTests>
    {
        public ServerSidePagingTests(WebApiTestFixture<ServerSidePagingTests> fixture)
            : base(fixture)
        {
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = GetEdmModel();
            services.ConfigureControllers(
                typeof(ServerSidePagingCustomersController),
                typeof(ServerSidePagingEmployeesController));
            services.AddControllers().AddOData(opt => opt.Expand().OrderBy().AddRouteComponents("{a}", edmModel));
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ServerSidePagingOrder>("ServerSidePagingOrders").EntityType.HasRequired(d => d.ServerSidePagingCustomer);
            builder.EntitySet<ServerSidePagingCustomer>("ServerSidePagingCustomers").EntityType.HasMany(d => d.ServerSidePagingOrders);

            var getEmployeesHiredInPeriodFunction = builder.EntitySet<ServerSidePagingEmployee>(
                "ServerSidePagingEmployees").EntityType.Collection.Function("GetEmployeesHiredInPeriod");
            getEmployeesHiredInPeriodFunction.Parameter(typeof(DateTime), "fromDate");
            getEmployeesHiredInPeriodFunction.Parameter(typeof(DateTime), "toDate");
            getEmployeesHiredInPeriodFunction.ReturnsCollectionFromEntitySet<ServerSidePagingEmployee>("ServerSidePagingEmployees");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ValidNextLinksGenerated()
        {
            // Arrange
            string requestUri = "/prefix/ServerSidePagingCustomers?$expand=ServerSidePagingOrders";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            // Assert
            // Customer 1 => 6 Orders, Customer 2 => 5 Orders, Customer 3 => 4 Orders, ...
            // NextPageLink will be expected on the Customers collection as well as
            // the Orders child collection on Customer 1
            using (JsonDocument document = JsonDocument.Parse(content))
            {
                bool found = document.RootElement.TryGetProperty("value", out JsonElement value);
                Assert.True(found);

                foreach (JsonElement item in value.EnumerateArray())
                {
                    found = item.TryGetProperty("Id", out JsonElement id);
                    Assert.True(found);

                    // only the Orders child collection on Customer 1
                    bool odersNextLink = item.TryGetProperty("ServerSidePagingOrders@odata.nextLink", out JsonElement ordersNextLink);
                    int idValue = id.GetInt32();
                    if (idValue == 1)
                    {
                        Assert.True(odersNextLink);
                        Assert.Equal("http://localhost/prefix/ServerSidePagingCustomers/1/ServerSidePagingOrders?$skip=5", ordersNextLink.GetString());
                    }
                    else
                    {
                        Assert.False(odersNextLink);
                    }
                }

                bool nextLinkFound = document.RootElement.TryGetProperty("@odata.nextLink", out JsonElement nextLink);
                Assert.True(nextLinkFound);
                Assert.Equal("http://localhost/prefix/ServerSidePagingCustomers?$expand=ServerSidePagingOrders&$skip=5", nextLink.GetString());
            }
        }

        [Fact]
        public async Task VerifyParametersInNextPageLinkInEdmFunctionResponseBodyAreInSameCaseAsInRequestUrl()
        {
            // Arrange
            var requestUri = "/prefix/ServerSidePagingEmployees/" +
                "GetEmployeesHiredInPeriod(fromDate=@fromDate,toDate=@toDate)" +
                "?@fromDate=2023-01-07T00:00:00%2B00:00&@toDate=2023-05-07T00:00:00%2B00:00";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient();

            // Act
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("\"@odata.nextLink\":", content);
            Assert.Contains(
                "/prefix/ServerSidePagingEmployees/GetEmployeesHiredInPeriod(fromDate=@fromDate,toDate=@toDate)" +
                "?%40fromDate=2023-01-07T00%3A00%3A00%2B00%3A00&%40toDate=2023-05-07T00%3A00%3A00%2B00%3A00&$skip=3",
                content);
        }
    }

    public class SkipTokenPagingTests : WebApiTestBase<SkipTokenPagingTests>
    {
        public SkipTokenPagingTests(WebApiTestFixture<SkipTokenPagingTests> fixture)
            : base(fixture)
        {
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel model = GetEdmModel();
            services.ConfigureControllers(
                typeof(SkipTokenPagingS1CustomersController),
                typeof(SkipTokenPagingS2CustomersController));
            services.AddControllers().AddOData(opt => opt.Expand().OrderBy().SkipToken().AddRouteComponents("{a}", model));
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SkipTokenPagingCustomer>("SkipTokenPagingS1Customers");
            builder.EntitySet<SkipTokenPagingCustomer>("SkipTokenPagingS2Customers");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullableProperty()
        {
            HttpClient client = CreateClient();
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, decimal?, int, decimal?>>
            {
                Tuple.Create<int, decimal?, int, decimal?> (1, null, 3, null),
                Tuple.Create<int, decimal?, int, decimal?> (5, null, 2, 2),
                Tuple.Create<int, decimal?, int, decimal?> (7, 5, 9, 25),
                Tuple.Create<int, decimal?, int, decimal?>(4, 30, 6, 35)
            };

            string requestUri = "/prefix/SkipTokenPagingS1Customers?$orderby=CreditLimit";

            foreach (var testData in skipTokenTestData)
            {
                int idAt0 = testData.Item1;
                decimal? creditLimitAt0 = testData.Item2;
                int idAt1 = testData.Item3;
                decimal? creditLimitAt1 = testData.Item4;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=CreditLimit-", creditLimitAt1 != null ? creditLimitAt1.ToString() : "null", ",Id-", idAt1);

                // Act
                response = await client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);
                Assert.Equal(idAt0, (pageResult[0] as JObject)["Id"].ToObject<int>());
                Assert.Equal(creditLimitAt0, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                Assert.Equal(creditLimitAt1, (pageResult[1] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingS1Customers?$orderby=CreditLimit&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(8, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Equal(50, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullablePropertyDescending()
        {
            HttpClient client = CreateClient();
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, decimal?>>
            {
                Tuple.Create<int, decimal ?> (6, 35),
                Tuple.Create<int, decimal?> (9, 25),
                Tuple.Create<int, decimal?> (2, 2),
                Tuple.Create<int, decimal?>(3, null)
            };

            string requestUri = "/prefix/SkipTokenPagingS1Customers?$orderby=CreditLimit desc";

            foreach (var testData in skipTokenTestData)
            {
                int idAt1 = testData.Item1;
                decimal? creditLimitAt1 = testData.Item2;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=CreditLimit-", creditLimitAt1 != null ? creditLimitAt1.ToString() : "null", ",Id-", idAt1);

                // Act
                response = await client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);
                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                Assert.Equal(creditLimitAt1, (pageResult[1] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingS1Customers?$orderby=CreditLimit%20desc&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(5, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Null((pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNonNullablePropertyThenByNullableProperty()
        {
            HttpClient client = CreateClient();
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, string, decimal?>>
            {
                Tuple.Create<int, string, decimal?> (2, "B", null),
                Tuple.Create<int, string, decimal?> (6, "C", null),
                Tuple.Create<int, string, decimal?> (11, "F", 35),
            };

            string requestUri = "/prefix/SkipTokenPagingS2Customers?$orderby=Grade,CreditLimit";

            foreach (var testData in skipTokenTestData)
            {
                int idAt3 = testData.Item1;
                string gradeAt3 = testData.Item2;
                decimal? creditLimitAt3 = testData.Item3;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=Grade-%27", gradeAt3, "%27,CreditLimit-", creditLimitAt3 != null ? creditLimitAt3.ToString() : "null", ",Id-", idAt3);

                // Act
                response = await client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(4, pageResult.Count);
                Assert.Equal(idAt3, (pageResult[3] as JObject)["Id"].ToObject<int>());
                Assert.Equal(gradeAt3, (pageResult[3] as JObject)["Grade"].ToObject<string>());
                Assert.Equal(creditLimitAt3, (pageResult[3] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingS2Customers?$orderby=Grade%2CCreditLimit&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(13, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Equal("F", (pageResult[0] as JObject)["Grade"].ToObject<string>());
            Assert.Equal(55, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullablePropertyThenByNonNullableProperty()
        {
            HttpClient client = CreateClient();
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<Tuple<int, string, decimal?>>
            {
                Tuple.Create<int, string, decimal?> (6, "C", null),
                Tuple.Create<int, string, decimal?> (5, "A", 30),
                Tuple.Create<int, string, decimal?> (10, "D", 50),
            };

            string requestUri = "/prefix/SkipTokenPagingS2Customers?$orderby=CreditLimit,Grade";

            foreach (var testData in skipTokenTestData)
            {
                int idAt3 = testData.Item1;
                string gradeAt3 = testData.Item2;
                decimal? creditLimitAt3 = testData.Item3;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipToken = string.Concat("$skiptoken=CreditLimit-", creditLimitAt3 != null ? creditLimitAt3.ToString() : "null", ",Grade-%27", gradeAt3, "%27", ",Id-", idAt3);

                // Act
                response = await client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(4, pageResult.Count);
                Assert.Equal(idAt3, (pageResult[3] as JObject)["Id"].ToObject<int>());
                Assert.Equal(gradeAt3, (pageResult[3] as JObject)["Grade"].ToObject<string>());
                Assert.Equal(creditLimitAt3, (pageResult[3] as JObject)["CreditLimit"].ToObject<decimal?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.EndsWith("/prefix/SkipTokenPagingS2Customers?$orderby=CreditLimit%2CGrade&" + skipToken, nextPageLink);

                requestUri = nextPageLink;
            }

            // Fetch last page
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await client.SendAsync(request);

            content = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            pageResult = content["value"] as JArray;
            Assert.NotNull(pageResult);
            Assert.Single(pageResult);
            Assert.Equal(13, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Equal("F", (pageResult[0] as JObject)["Grade"].ToObject<string>());
            Assert.Equal(55, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }
    }
}
