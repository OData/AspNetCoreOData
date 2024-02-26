//-----------------------------------------------------------------------------
// <copyright file="SkipTokenQueryTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SkipToken
{
    public class SkipTokenQueryTests : WebApiTestBase<SkipTokenQueryTests>
    {
        public SkipTokenQueryTests(WebApiTestFixture<SkipTokenQueryTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(SkipTokenControllers), typeof(MetadataController));
            services.AddControllers()
                .AddOData(
                    opt => opt.AddRouteComponents("all", GetEdmModel())
                           .AddRouteComponents("odata", GetEdmModel())
                           .Count().Filter().OrderBy().Expand().SetMaxTop(null).Select().SkipToken());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<StCustomer>("Customers");
            builder.EntitySet<StOrder>("Orders").EntityType.HasKey(c => new { c.RegId, c.Id});
            return builder.GetEdmModel();
        }

        public static TheoryDataSet<string, int[], string> Customers_SkipTokenGeneratorCases
            => new TheoryDataSet<string, int[], string>
            {
                {
                    "odata/customers",
                    new int[] { 1, 2 },
                    "http://localhost/odata/customers?$skiptoken=Id-2"
                },
                {
                    "odata/customers?$orderby=id",
                    new int[] { 1, 2 },
                    "http://localhost/odata/customers?$orderby=id&$skiptoken=Id-2"
                },
                {
                    "odata/customers?$orderby=id asc",
                    new int[] { 1, 2 },
                    "http://localhost/odata/customers?$orderby=id%20asc&$skiptoken=Id-2"
                },
                {
                    "odata/customers?$orderby=id desc",
                    new int[] { 5, 4 },
                    "http://localhost/odata/customers?$orderby=id%20desc&$skiptoken=Id-4"
                },
                {
                    "odata/customers?$orderby=favoritePlace/state",
                    new int[] { 2, 4 },
                    "http://localhost/odata/customers?$orderby=favoritePlace%2Fstate&$skiptoken=%27AJ%27,Id-4"
                },
                {
                    "odata/customers?$orderby=substring(favoritePlace/state,1,1)",
                    new int[] { 3, 2 },
                    "http://localhost/odata/customers?$orderby=substring%28favoritePlace%2Fstate%2C1%2C1%29&$skiptoken=%27J%27,Id-2"
                },
                {
                    "odata/customers?$orderby=substring(favoritePlace/state,1,1),substring(name,1,2)",
                    new int[] { 3, 2 },
                    "http://localhost/odata/customers?$orderby=substring%28favoritePlace%2Fstate%2C1%2C1%29%2Csubstring%28name%2C1%2C2%29&$skiptoken=%27J%27,%27ar%27,Id-2"
                },
                {
                    "odata/customers?$orderby=phoneNumbers/$count",
                    new int[] { 2, 3 },
                    "http://localhost/odata/customers?$orderby=phoneNumbers%2F%24count&$skiptoken=2,Id-3"
                },
                {
                    "odata/customers?$orderby=tolower(name),magic&$compute=age div id as magic",
                    new int[] { 4, 3 },
                    "http://localhost/odata/customers?$orderby=tolower%28name%29%2Cmagic&$compute=age%20div%20id%20as%20magic&$skiptoken=%27apply%27,magic-333,Id-3"
                },
                {
                    "odata/customers?$orderby=tolower(name)&$select=id,age",
                    new int[] { 4, 3 },
                    "http://localhost/odata/customers?$orderby=tolower%28name%29&$select=id%2Cage&$skiptoken=%27apply%27,Id-3"
                },
                {
                    "odata/customers?$orderby=tolower(Detail)&$select=id,age",
                    new int[] { 2, 3 },
                    "http://localhost/odata/customers?$orderby=tolower%28Detail%29&$select=id%2Cage&$skiptoken=%27regular%27,Id-3"
                },
                {
                    "odata/customers?$orderby=birthday&$select=id,birthday",
                    new int[] { 3, 5 },
                    "http://localhost/odata/customers?$orderby=birthday&$select=id%2Cbirthday&$skiptoken=Birthday-1978-11-15T13%3A15%3A18.023Z,Id-5"
                },
                {
                    "odata/customers?$orderby=year(birthday) desc&$select=id,birthday",
                    new int[] { 4, 2 },
                    "http://localhost/odata/customers?$orderby=year%28birthday%29%20desc&$select=id%2Cbirthday&$skiptoken=2001,Id-2"
                },
                {
                    "odata/customers?$orderby=magicNumber desc&$select=id,magicNumber",
                    new int[] { 4, 5 },
                    "http://localhost/odata/customers?$orderby=magicNumber%20desc&$select=id%2CmagicNumber&$skiptoken=MagicNumber--85,Id-5"
                }
            };

        [Theory]
        [MemberData(nameof(Customers_SkipTokenGeneratorCases))]
        public async Task EnableSkipToken_GenerateNextLinkWithSkipToken_ForSingleKey(string requestUri, int[] ids, string nextLink)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            JObject payloadBody = await response.Content.ReadAsObject<JObject>();
            (int[] actualIds, string actualNextLink) = GetActual(payloadBody);

            Assert.True(ids.SequenceEqual(actualIds));
            Assert.Equal(nextLink, actualNextLink);
        }

        private static (int[], string nextLink) GetActual(JObject payload)
        {
            JArray value = payload["value"] as JArray;
            Assert.NotNull(value);

            int[] ids = new int[value.Count()];
            for (int i = 0; i < value.Count(); i++)
            {
                JObject item = value[i] as JObject;
                ids[i] = (int)item["Id"];
            }

            return (ids, (string)payload["@odata.nextLink"]);
        }

        public static TheoryDataSet<string, string[], string> Orders_SkipTokenGeneratorCases
            => new TheoryDataSet<string, string[], string>
            {
                {
                    "odata/orders",
                    new string[] { "A8|2", "A8|3"},
                    "http://localhost/odata/orders?$skiptoken=Id-3,RegId-%27A8%27"
                },
                {
                    "odata/orders?$orderby=id",
                    new string[] { "A8|2", "A8|3" },
                    "http://localhost/odata/orders?$orderby=id&$skiptoken=Id-3,RegId-%27A8%27"
                },
                {
                    "odata/orders?$orderby=RegId desc",
                    new string[] { "A9|5", "A9|11" },
                    "http://localhost/odata/orders?$orderby=RegId%20desc&$skiptoken=RegId-%27A9%27,Id-11"
                },
                {
                    "odata/orders?$orderby=Location/city desc",
                    new string[] { "A9|5", "A9|11"},
                    "http://localhost/odata/orders?$orderby=Location%2Fcity%20desc&$skiptoken=%27Settle%27,Id-11,RegId-%27A9%27"
                },
                {
                    "odata/orders?$orderby=toupper(Location/Region)",
                    new string[] { "A8|2", "A9|13"},
                    "http://localhost/odata/orders?$orderby=toupper%28Location%2FRegion%29&$skiptoken=%27AA%27,Id-13,RegId-%27A9%27"
                }
            };

        [Theory]
        [MemberData(nameof(Orders_SkipTokenGeneratorCases))]
        public async Task EnableSkipToken_GenerateNextLinkWithSkipToken_ForCompositeKey(string requestUri, string[] keys, string nextLink)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            JObject payloadBody = await response.Content.ReadAsObject<JObject>();
            (string[] actualIds, string actualNextLink) = GetOrderActual(payloadBody);

            Assert.True(keys.SequenceEqual(actualIds));
            Assert.Equal(nextLink, actualNextLink);
        }

        private static (string[], string nextLink) GetOrderActual(JObject payload)
        {
            JArray value = payload["value"] as JArray;
            Assert.NotNull(value);

            string[] ids = new string[value.Count()];
            for (int i = 0; i < value.Count(); i++)
            {
                JObject item = value[i] as JObject;
                ids[i] = $"{(string)item["RegId"]}|{(int)item["Id"]}";
            }

            return (ids, (string)payload["@odata.nextLink"]);
        }

        [Fact]
        public async Task EnableSkipToken_WithSkipToken_CallNextLinkRecursively_ForSingleKey()
        {
            // Arrange
            string[][] expected = new string[3][];
            expected[0] = new string[] { "2|Earth", "3|Apply" };
            expected[1] = new string[] { "1|Mars", "4|Apple" };
            expected[2] = new string[] { "5|Kare" };

            string requestUri = "odata/customers?$orderby=phoneNumbers/$count&$select=id,name";
            
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            int index = 0;
            while (requestUri != null)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

                // Act
                response = await client.SendAsync(request);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);

                JObject payloadBody = await response.Content.ReadAsObject<JObject>();
                (string[] actualIds, string actualNextLink) = GetActualCustomerInfo(payloadBody);

                Assert.True(expected[index].SequenceEqual(actualIds));

                requestUri = actualNextLink;
                index++;
            }

            Assert.Equal(3, index);
        }

        private static (string[], string nextLink) GetActualCustomerInfo(JObject payload)
        {
            JArray value = payload["value"] as JArray;
            Assert.NotNull(value);

            string[] ids = new string[value.Count()];
            for (int i = 0; i < value.Count(); i++)
            {
                JObject item = value[i] as JObject;
                ids[i] = $"{(int)item["Id"]}|{(string)item["Name"]}";
            }

            JProperty nextLinkProperty = payload.Property("@odata.nextLink");
            if (nextLinkProperty != null)
            {
                return (ids, (string)nextLinkProperty.Value);
            }
            else
            {
                return (ids, null);
            }
        }

        [Fact]
        public async Task EnableSkipToken_WithSkipToken_CallNextLinkRecursively_ForCompositeKey()
        {
            // Arrange
            string[][] expected = new string[5][];
            expected[0] = new string[] { "A9|5|87|435|Settle",   "A9|11|66|726|Settle" };
            expected[1] = new string[] { "A9|65|42|2730|Settle", "A8|3|78|234|Reedy"   };
            expected[2] = new string[] { "A9|12|42|504|Reedy",   "A8|31|78|2418|Reedy" };
            expected[3] = new string[] { "A8|2|18|36|Perry",     "A9|13|17|221|Perry" };
            expected[4] = new string[] { "A9|83|14|1162|Perry" };

            string requestUri = "odata/orders?$orderby=Location/city desc,magic&$compute=id mul amount as magic,location/city as city&$select=regid,id,amount,magic,city";

            HttpClient client = CreateClient();
            HttpResponseMessage response;

            int index = 0;
            while (requestUri != null)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

                // Act
                response = await client.SendAsync(request);

                // Assert
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(response.Content);

                JObject payloadBody = await response.Content.ReadAsObject<JObject>();
                (string[] actualIds, string actualNextLink) = GetActualOrderInfo(payloadBody);

                Assert.True(expected[index].SequenceEqual(actualIds));

                requestUri = actualNextLink;
                index++;
            }

            Assert.Equal(5, index);
        }

        private static (string[], string nextLink) GetActualOrderInfo(JObject payload)
        {
            JArray value = payload["value"] as JArray;
            Assert.NotNull(value);

            string[] ids = new string[value.Count()];
            for (int i = 0; i < value.Count(); i++)
            {
                JObject item = value[i] as JObject;
                ids[i] = $"{(string)item["RegId"]}|{(int)item["Id"]}|{(int)item["Amount"]}|{(int)item["magic"]}|{(string)item["city"]}";
            }

            JProperty nextLinkProperty = payload.Property("@odata.nextLink");
            if (nextLinkProperty != null)
            {
                return (ids, (string)nextLinkProperty.Value);
            }
            else
            {
                return (ids, null);
            }
        }
    }
}
