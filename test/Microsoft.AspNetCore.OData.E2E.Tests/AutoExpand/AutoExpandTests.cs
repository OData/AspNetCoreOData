//-----------------------------------------------------------------------------
// <copyright file="AutoExpandTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
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
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AutoExpand
{
    public class AutoExpandTests : WebApiTestBase<AutoExpandTests>
    {
        private readonly ITestOutputHelper output;

        public AutoExpandTests(WebApiTestFixture<AutoExpandTests> fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = AutoExpandEdmModel.GetEdmModel();

            services.ConfigureControllers(typeof(CustomersController),
                typeof(PeopleController),
                typeof(NormalOrdersController),
                typeof(EnableQueryMenusController),
                typeof(QueryOptionsOfTMenusController));

            services.AddControllers().AddOData(opt =>
                opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select().AddRouteComponents("autoexpand", edmModel));
        }

        [Theory]
        [InlineData("?$select=Order", 3)]
        [InlineData("?$select=Id", 4)]
        [InlineData("?$expand=Order & $select=Id", 4)]
        [InlineData("?$expand=Friend", 4)]
        [InlineData("", 4)]
        public async Task QueryForAnResource_Includes_AutoExpandNavigationProperty(string url, int propCount)
        {
            // Arrange
            string queryUrl = $"autoexpand/Customers(5){url}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var customer = await response.Content.ReadAsObject<JObject>();
            this.output.WriteLine(customer.ToString());
            Assert.Equal(customer.Properties().Count(), propCount);
            VerifyOrderAndChoiceOrder(customer);
            VerifyHomeAddress(customer);

            // level one
            JObject friend = customer["Friend"] as JObject;
            JObject order = friend["Order"] as JObject;
            Assert.NotNull(order);
            Assert.Null(order["Choice"]);

            // level two
            friend = friend["Friend"] as JObject;
            Assert.Null(friend["Order"]);
        }

        private static void VerifyHomeAddress(JObject customer)
        {
            JObject homeAddress = customer["HomeAddress"] as JObject;
            Assert.NotNull(homeAddress);

            Assert.Equal("UsStreet 5", homeAddress["Street"]);
            Assert.Equal("UsCity 5", homeAddress["City"]);

            JObject countryOrRegion = homeAddress["CountryOrRegion"] as JObject;
            Assert.NotNull(countryOrRegion);

            JObject zipCode = homeAddress["ZipCode"] as JObject;
            Assert.NotNull(zipCode);
        }

        [Fact]
        public async Task QueryForAnResource_Includes_DerivedAutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = "autoexpand/Customers(8)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal("{" +
                "\"Id\":8," +
                "\"HomeAddress\":{" +
                  "\"Street\":\"CnStreet 8\",\"City\":\"CnCity 8\"," +
                  "\"CountryOrRegion\":{\"Id\":108,\"Name\":\"C and R 108\"}" +  // CnAddress only auto-expands the CountryOrRegion
                "}," +
                "\"Order\":{" +
                  "\"Id\":8," +
                  "\"Choice\":{\"Id\":8,\"Amount\":8000.0}," +
                  "\"SpecialChoice\":{\"Id\":800,\"Amount\":16000.0}" +
                "}," +
                "\"Friend\":{" +
                  "\"Id\":7," +
                  "\"HomeAddress\":{" +
                    "\"Street\":\"UsStreet 7\",\"City\":\"UsCity 7\"," +
                    "\"CountryOrRegion\":{\"Id\":107,\"Name\":\"C and R 107\"}," +  // UsAddress auto-expands the CountryOrRegion & ZipCode
                    "\"ZipCode\":{\"Id\":2007,\"Code\":\"Code 7\"}" +
                  "}," +
                  "\"Order\":{\"Id\":7}," +
                  "\"Friend\":{" +
                    "\"Id\":6," +
                    "\"HomeAddress\":{\"Street\":\"CnStreet 6\",\"City\":\"CnCity 6\"}" +
                  "}" +
                "}" +
              "}", payload);
        }

        [Fact]
        public async Task QueryForAnResource_Includes_MultiDerivedAutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = "autoexpand/Customers(9)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var customer = await response.Content.ReadAsObject<JObject>();
            VerifyOrderAndChoiceOrder(customer, special: true, vip: true);
            this.output.WriteLine(customer.ToString());

            // level one
            JObject friend = customer["Friend"] as JObject;
            JObject order = friend["Order"] as JObject;
            Assert.NotNull(order);
            Assert.Null(order["Choice"]);
        }

        [Fact]
        public async Task QueryForAnResource_LevelsWithAutoExpandInSameNavigationProperty()
        {
            // Arrange
            string queryUrl = "autoexpand/Customers(5)?$expand=Friend($levels=0)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var customer = await response.Content.ReadAsObject<JObject>();
            Assert.NotNull(customer);
            VerifyOrderAndChoiceOrder(customer);
            Assert.Null(customer["Friend"]);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("3", 3)]
        [InlineData("max", 4)]
        public async Task LevelsWithAutoExpandInDifferentNavigationProperty(string level, int levelNumber)
        {
            // Arrange
            string queryUrl = $"autoexpand/People?$expand=Friend($levels={level})";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseJson = await response.Content.ReadAsObject<JObject>();
            this.output.WriteLine(responseJson.ToString());
            var people = responseJson["value"] as JArray;
            var he = people[8] as JObject;
            JObject friend = he;
            for (int i = 1; i <= levelNumber; i++)
            {
                friend = friend["Friend"] as JObject;
                Assert.NotNull(friend);
                if (i + 2 <= levelNumber)
                {
                    VerifyOrderAndChoiceOrder(friend);
                }
            }
            Assert.Null(friend["Friend"]);
        }

        private static void VerifyOrderAndChoiceOrder(JObject customer, bool special = false, bool vip = false)
        {
            JObject order = customer["Order"] as JObject;
            Assert.NotNull(order);

            JObject choice = order["Choice"] as JObject;
            Assert.NotNull(choice);
            Assert.Equal((int)order["Id"] * 1000, choice["Amount"]);

            if (special)
            {
                choice = order["SpecialChoice"] as JObject;
                Assert.NotNull(choice);
                Assert.Equal((int)order["Id"] * 2000, choice["Amount"]);
            }

            if (vip)
            {
                choice = order["VipChoice"] as JObject;
                Assert.NotNull(choice);
                Assert.Equal((int)order["Id"] * 3000, choice["Amount"]);
            }
        }

        [Theory]
        [InlineData("autoexpand/NormalOrders")]
        [InlineData("autoexpand/NormalOrders(2)")]
        public async Task DerivedAutoExpandNavigationPropertyTest(string url)
        {
            // Arrange
            string queryUrl = url;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Contains("OrderDetail", result);
        }

        [Theory]
        [InlineData("autoexpand/NormalOrders?$select=Id", true)]
        [InlineData("autoexpand/NormalOrders", false)]
        [InlineData("autoexpand/NormalOrders(2)?$select=Id", true)]
        [InlineData("autoexpand/NormalOrders(2)", false)]
        [InlineData("autoexpand/NormalOrders(3)?$select=Id", true)]
        [InlineData("autoexpand/NormalOrders(3)", false)]
        public async Task DisableAutoExpandWhenSelectIsPresentTest(string url, bool isSelectPresent)
        {
            // Arrange
            string queryUrl = url;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            if (isSelectPresent)
            {
                Assert.DoesNotContain("NotShownOrderDetail4", result);
            }
            else
            {
                Assert.Contains("NotShownOrderDetail4", result);
            }
        }

        [Theory]
        [InlineData("autoexpand/NormalOrders(2)?$expand=LinkOrder($select=Id)", true)]
        [InlineData("autoexpand/NormalOrders(2)?$expand=LinkOrder", false)]
        public async Task DisableAutoExpandWhenSelectIsPresentDollarExpandTest(string url, bool isSelectPresent)
        {
            // Arrange
            string queryUrl = url;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Contains("NotShownOrderDetail4", result);
            if (isSelectPresent)
            {
                Assert.DoesNotContain("NotShownOrderDetail2", result);
            }
            else
            {
                Assert.Contains("NotShownOrderDetail2", result);
            }
        }

        [Fact]
        public async Task QueryForProperty_Includes_AutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = "autoexpand/Customers(8)/HomeAddress";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            this.output.WriteLine(payload);
            Assert.Equal("{" +
                "\"Street\":\"CnStreet 8\"," +
                "\"City\":\"CnCity 8\"," +
                "\"CountryOrRegion\":{\"Id\":108,\"Name\":\"C and R 108\"}" +
              "}", payload);
        }

        [Fact]
        public async Task QueryForProperty_Includes_AutoExpandNavigationPropertyOnDerivedType()
        {
            // Arrange
            string queryUrl = "autoexpand/Customers(9)/HomeAddress";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            this.output.WriteLine(payload);
            Assert.Equal("{" +
                "\"Street\":\"UsStreet 9\"," +
                "\"City\":\"UsCity 9\"," +
                "\"CountryOrRegion\":{\"Id\":109,\"Name\":\"C and R 109\"}," +
                "\"ZipCode\":{\"Id\":2009,\"Code\":\"Code 9\"}" +
              "}", payload);
        }

        [Theory]
        [InlineData("EnableQueryMenus")]
        [InlineData("QueryOptionsOfTMenus")]
        public async Task NonDefaultMaxExpansionDepthAppliesToAutoExpand(string entitySet)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"autoexpand/{entitySet}");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            var client = CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(
                "{" +
                $"\"@odata.context\":\"http://localhost/autoexpand/$metadata#{entitySet}(Tabs(Items(Notes())))\"," +
                "\"value\":[{\"Id\":1,\"Tabs\":[{\"Id\":1,\"Items\":[{\"Id\":1,\"Notes\":[{\"Id\":1}]}]}]}]" +
                "}",
                content);
        }
    }
}
