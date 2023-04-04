//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
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
                typeof(SkipTokenPagingS2CustomersController),
                typeof(SkipTokenPagingS3CustomersController));
            services.AddControllers().AddOData(opt => opt.Expand().OrderBy().SkipToken().AddRouteComponents("{a}", model));
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SkipTokenPagingCustomer>("SkipTokenPagingS1Customers");
            builder.EntitySet<SkipTokenPagingCustomer>("SkipTokenPagingS2Customers");
            builder.EntitySet<SkipTokenPagingCustomer>("SkipTokenPagingS3Customers");

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
            var skipTokenTestData = new List<(int, decimal?, int, decimal?)>
            {
                (1, null, 3, null),
                (5, null, 2, 2),
                (7, 5, 9, 25),
                (4, 30, 6, 35)
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
            var skipTokenTestData = new List<(int, decimal?)>
            {
                (6, 35),
                (9, 25),
                (2, 2),
                (3, null)
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
            var skipTokenTestData = new List<(int, string, decimal?)>
            {
                (2, "B", null),
                (6, "C", null),
                (11, "F", 35),
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
            var skipTokenTestData = new List<(int, string, decimal?)>
            {
                (6, "C", null),
                (5, "A", 30),
                (10, "D", 50),
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
        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullableDateTimeProperty()
        {
            HttpClient client = CreateClient();
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<(int, DateTime?, int, DateTime?)>
            {
                (1, null, 3, null),
                (5, null, 2, new DateTime(2023, 1, 2)),
                (7, new DateTime(2023, 1, 5), 9, new DateTime(2023, 1, 25)),
                (4, new DateTime(2023, 1, 30), 6, new DateTime(2023, 2, 4))
            };

            string requestUri = "/prefix/SkipTokenPagingS3Customers?$orderby=CustomerSince";

            foreach (var testData in skipTokenTestData)
            {
                int idAt0 = testData.Item1;
                DateTime? customerSinceAt0 = testData.Item2;
                int idAt1 = testData.Item3;
                DateTime? customerSinceAt1 = testData.Item4;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipTokenStart = string.Concat(
                    "$skiptoken=CustomerSince-",
                    customerSinceAt1 != null ? customerSinceAt1.Value.ToString("yyyy-MM-dd") : "null");
                string skipTokenEnd = string.Concat(",Id-", idAt1);

                // Act
                response = await client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);
                Assert.Equal(idAt0, (pageResult[0] as JObject)["Id"].ToObject<int>());
                Assert.Equal(customerSinceAt0, (pageResult[0] as JObject)["CustomerSince"].ToObject<DateTime?>());
                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                Assert.Equal(customerSinceAt1, (pageResult[1] as JObject)["CustomerSince"].ToObject<DateTime?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.Contains("/prefix/SkipTokenPagingS3Customers?$orderby=CustomerSince&" + skipTokenStart, nextPageLink);
                Assert.EndsWith(skipTokenEnd, nextPageLink);

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
            Assert.Equal(new DateTime(2023, 2, 19), (pageResult[0] as JObject)["CustomerSince"].ToObject<DateTime?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }

        [Fact]
        public async Task VerifySkipTokenPagingOrderedByNullableDateTimePropertyDescending()
        {
            HttpClient client = CreateClient();
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<(int, DateTime?)>
            {
                (6, new DateTime(2023, 2, 4)),
                (9, new DateTime(2023, 1, 25)),
                (2, new DateTime(2023, 1, 2)),
                (3, null)
            };

            string requestUri = "/prefix/SkipTokenPagingS3Customers?$orderby=CustomerSince desc";

            foreach (var testData in skipTokenTestData)
            {
                int idAt1 = testData.Item1;
                DateTime? customerSinceAt1 = testData.Item2;

                // Arrange
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                string skipTokenStart = string.Concat(
                    "$skiptoken=CustomerSince-",
                    customerSinceAt1 != null ? customerSinceAt1.Value.ToString("yyyy-MM-dd") : "null");
                string skipTokenEnd = string.Concat(",Id-", idAt1);

                // Act
                response = await client.SendAsync(request);
                content = await response.Content.ReadAsObject<JObject>();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                pageResult = content["value"] as JArray;
                Assert.NotNull(pageResult);
                Assert.Equal(2, pageResult.Count);
                Assert.Equal(idAt1, (pageResult[1] as JObject)["Id"].ToObject<int>());
                Assert.Equal(customerSinceAt1, (pageResult[1] as JObject)["CustomerSince"].ToObject<DateTime?>());

                string nextPageLink = content["@odata.nextLink"].ToObject<string>();
                Assert.NotNull(nextPageLink);
                Assert.Contains("/prefix/SkipTokenPagingS3Customers?$orderby=CustomerSince%20desc&" + skipTokenStart, nextPageLink);
                Assert.EndsWith(skipTokenEnd, nextPageLink);

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
            Assert.Null((pageResult[0] as JObject)["CustomerSince"].ToObject<DateTime?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }
    }

    public class SkipTokenPagingEdgeCaseTests : WebApiTestBase<SkipTokenPagingEdgeCaseTests>
    {
        public SkipTokenPagingEdgeCaseTests(WebApiTestFixture<SkipTokenPagingEdgeCaseTests> fixture)
            : base(fixture)
        {
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel model = GetEdmModel();
            services.ConfigureControllers(
                typeof(SkipTokenPagingEdgeCase1CustomersController));
            services.AddControllers().AddOData(opt => opt.Expand().OrderBy().SkipToken().AddRouteComponents("{a}", model));
        }

        protected static IEdmModel GetEdmModel()
        {
            var csdl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">" +
                "<edmx:DataServices>" +
                "<Schema Namespace=\"" + typeof(SkipTokenPagingEdgeCase1Customer).Namespace + "\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<EntityType Name=\"SkipTokenPagingEdgeCase1Customer\">" +
                "<Key>" +
                "<PropertyRef Name=\"Id\" />" +
                "</Key>" +
                "<Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                "<Property Name=\"CreditLimit\" Type=\"Edm.Decimal\" Scale=\"Variable\" Nullable=\"false\" />" + // Property is nullable on CLR type
                "</EntityType>" +
                "</Schema>" +
                "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                "<EntityContainer Name=\"Container\">" +
                "<EntitySet Name=\"SkipTokenPagingEdgeCase1Customers\" EntityType=\"" + typeof(SkipTokenPagingEdgeCase1Customer).FullName + "\" />" +
                "</EntityContainer>" +
                "</Schema>" +
                "</edmx:DataServices>" +
                "</edmx:Edmx>";

            IEdmModel model;

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(csdl)))
            using (var reader = XmlReader.Create(memoryStream))
            {
                model = CsdlReader.Parse(reader);
            }

            return model;
        }

        [Fact]
        public async Task VerifySkipTokenPagingForPropertyNullableOnClrTypeButNotNullableOnEdmType()
        {
            HttpClient client = CreateClient();
            HttpRequestMessage request;
            HttpResponseMessage response;
            JObject content;
            JArray pageResult;

            // NOTE: Using a loop in this test (as opposed to parameterized tests using xunit Theory attribute)
            // is intentional. The next-link in one response is used in the next request
            // so we need to control the execution order (unlike Theory attribute where order is random)
            var skipTokenTestData = new List<(int, decimal?, int, decimal?)>
            {
                (2, 2, 7, 5),
                (9, 25, 4, 30),
            };

            string requestUri = "/prefix/SkipTokenPagingEdgeCase1Customers?$orderby=CreditLimit";

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
                Assert.EndsWith("/prefix/SkipTokenPagingEdgeCase1Customers?$orderby=CreditLimit&" + skipToken, nextPageLink);

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
            Assert.Equal(6, (pageResult[0] as JObject)["Id"].ToObject<int>());
            Assert.Equal(35, (pageResult[0] as JObject)["CreditLimit"].ToObject<decimal?>());
            Assert.Null(content.GetValue("@odata.nextLink"));
        }
    }
}
