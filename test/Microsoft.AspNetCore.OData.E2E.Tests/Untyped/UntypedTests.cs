//-----------------------------------------------------------------------------
// <copyright file="UntypedTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Untyped
{
    public class UntypedTests : WebApiTestBase<UntypedTests>
    {
        private readonly ITestOutputHelper output;

        public UntypedTests(WebApiTestFixture<UntypedTests> fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = UntypedEdmModel.GetEdmModel();

            services.ConfigureControllers(typeof(UntypedController), typeof(MetadataController));

            services.AddControllers().AddOData(opt =>
                opt.EnableQueryFeatures()
                .AddRouteComponents("odata", edmModel));
        }

        [Fact]
        public async Task Untyped_Metadata()
        {
            string expect = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
              "<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">" +
                "<edmx:DataServices>" +
                  "<Schema Namespace=\"Microsoft.AspNetCore.OData.E2E.Tests.Untyped\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
                    "<EntityType Name=\"InModelPerson\" OpenType=\"true\">" +
                      "<Key>" +
                         "<PropertyRef Name=\"Id\" />" +
                      "</Key>" +
                      "<Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />" +
                      "<Property Name=\"Name\" Type=\"Edm.String\" />" +
                      "<Property Name=\"Data\" Type=\"Edm.Untyped\" />" +
                      "<Property Name=\"Infos\" Type=\"Collection(Edm.Untyped)\" />" +
                    "</EntityType>" +
                    "<ComplexType Name =\"InModelAddress\">" +
                      "<Property Name=\"City\" Type=\"Edm.String\" />" +
                      "<Property Name=\"Street\" Type=\"Edm.String\" />" +
                    "</ComplexType>" +
                    "<EnumType Name=\"InModelColor\">" +
                      "<Member Name=\"Red\" Value=\"0\" />" +
                      "<Member Name=\"Green\" Value=\"1\" />" +
                      "<Member Name=\"Blue\" Value=\"2\" />" +
                    "</EnumType>" +
                  "</Schema>" +
                "<Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">" +
              "<EntityContainer Name=\"Container\">" +
                "<EntitySet Name=\"People\" EntityType=\"Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelPerson\" />" +
              "</EntityContainer>" +
            "</Schema>" +
          "</edmx:DataServices>" +
        "</edmx:Edmx>";

            // Remove indentation
            expect = Regex.Replace(expect, @"\r\n\s*<", @"<");

            var requestUri = "odata/$metadata";
            HttpClient client = CreateClient();
            HttpResponseMessage response = await client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(expect, responseContent);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=full")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=none")]
        public async Task QueryUntypedEntitySet(string format)
        {

        }

        [Fact]
        public async Task QuerySinglePeople_ContainsBasic2()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("odata/people");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#People\"," +
                "\"value\":[" +
                  "{" +
                    "\"Id\":1," +
                    "\"Name\":\"Kerry\"," +
                    "\"Dynamic1\":13," +
                    "\"Dynamic2\":true," +
                    "\"Data\":13," +
                    "\"Infos\":[1,2,3]" +
                  "}," +
                  "{" +
                    "\"Id\":2," +
                    "\"Name\":\"Xu\"," +
                    "\"EnumDynamic1\":\"Blue\"," +
                    "\"EnumDynamic2\":\"Apple\"," +
                    "\"Data\":'Red'," +
                    "\"Infos\":[\"Blue\",\"Green\",\"Apple\"]" +
                  "}," +
                  "{" +
                     "\"Id\":3," +
                     "\"Name\":\"Mars\"," +
                     "\"Data\":{" +
                       "\"City\":\"Redmond\"," +
                       "\"Street\":\"134TH AVE\"" +
                     "}," +
                     "\"Infos\":[" +
                       "{\"City\":\"Issaq\",\"Street\":\"Klahanie Way\"}" +
                     "]," +
                     "\"ComplexDynamic1\":{" +
                       "\"City\":\"RedCity\"," +
                       "\"Street\":\"Mos Rd\"" +
                     "}," +
                     "\"ComplexDynamic2\":{" +
                       "\"Value\":\"AnyDynanicValue\"" +
                     "}" +
                   "}," +
                   "{\"Id\":4,\"Name\":\"Wu\",\"Data\":{\"Value\":\"<--->\"},\"Infos\":[{\"Value\":\"<===>\"}],\"ComplexDynamic1\":{\"City\":\"RedCity\",\"Street\":\"Mos Rd\"},\"ComplexDynamic2\":{\"Value\":\"AnyDynanicValue\"}},{\"Id\":5,\"Name\":\"Wen\",\"Data\":{\"Value\":\"<--->\"},\"Infos\":[{\"Value\":\"<===>\"}],\"ComplexDynamic1\":{\"City\":\"RedCity\",\"Street\":\"Mos Rd\"},\"ComplexDynamic2\":{\"Value\":\"AnyDynanicValue\"}}]}", payloadBody);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task QuerySinglePeople_ContainsBasic(int id)
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync($"odata/people/{id}");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            string expected = null;
            if (id == 1)
            {
                expected = "{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"Kerry\"," +
                "\"Dynamic1\":13," +
                "\"Dynamic2\":true," +
                "\"Data\":13," +
                "\"Infos\":[1,2,3]" +
              "}";
            }
            else if (id == 2)
            {
                expected =
              "{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"Kerry\"," +
                "\"Dynamic1\":13," +
                "\"Dynamic2\":true," +
                "\"Data\":13," +
                "\"Infos\":[1,2,3]" +
              "}";
            }
            else if (id == 3)
            {
                // ...add later
            }
            else if (id == 4)
            {
                // ...add later
            }

            Assert.Equal(expected, payloadBody);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        public async Task QuerySinglePeople_OnSingleDeclaredUntypedProperty(int id)
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync($"odata/people/{id}/data");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            string expected = null;
            if (id == 1)
            {
                expected = "{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"Kerry\"," +
                "\"Dynamic1\":13," +
                "\"Dynamic2\":true," +
                "\"Data\":13," +
                "\"Infos\":[1,2,3]" +
              "}";
            }
            else if (id == 2)
            {
                expected =
              "{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"Kerry\"," +
                "\"Dynamic1\":13," +
                "\"Dynamic2\":true," +
                "\"Data\":13," +
                "\"Infos\":[1,2,3]" +
              "}";
            }
            else if (id == 3)
            {
                // ...add later
            }
            else if (id == 4)
            {
                // ...add later
            }

            Assert.Equal(expected, payloadBody);
        }

        public static TheoryDataSet<string, string> DeclaredPropertyQueryCases
        {
            get
            {
                var data = new TheoryDataSet<string, string>
                {
                    {
                        "odata/people/1/data",
                        "{\"@odata.context\":\"http://localhost/odata/$metadata#People(1)/Data\"," +
                            "\"value\":13" +
                        "}"
                    },
                    {
                        "odata/people/2/data",
                        "{\"@odata.context\":\"http://localhost/odata/$metadata#People(2)/Data\"," +
                            "\"value\":\"Red\"" +
                        "}"
                    },
                    {
                        "odata/people/3/data",
                        "{\"@odata.context\":\"http://localhost/odata/$metadata#People(3)/Data\"," +
                            "\"City\":\"Redmond\",\"Street\":\"134TH AVE\"" +
                        "}"
                    },
                    {
                        "odata/people/4/data",
                        "{\"@odata.context\":\"http://localhost/odata/$metadata#People(3)/Data\"," +
                            "\"City\":\"Redmond\",\"Street\":\"134TH AVE\"" +
                        "}"
                    }
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(DeclaredPropertyQueryCases))]
        public async Task QuerySinglePeople_OnDeclaredUntypedProperty(string request, string expected)
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal(expected, payloadBody);
        }
    }
}
