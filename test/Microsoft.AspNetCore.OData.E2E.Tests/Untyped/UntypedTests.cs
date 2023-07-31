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
                    "<ComplexType Name=\"InModelAddress\">" +
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
                "<EntitySet Name=\"Managers\" EntityType=\"Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelPerson\" />" +
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

        public static TheoryDataSet<string, string> ManagersQueryCases
        {
            get
            {
                var data = new TheoryDataSet<string, string>
                {
                    {
                        "application/json;odata.metadata=full",
                        "{" +
                           "\"@odata.context\":\"http://localhost/odata/$metadata#Managers\"," +
                           "\"value\":[" +
                             "{" +
                               "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelPerson\"," +
                               "\"@odata.id\":\"http://localhost/odata/Managers(1)\"," +
                               "\"@odata.editLink\":\"Managers(1)\"," +
                               "\"Id\":1," +
                               "\"Name\":\"Sun\"," +
                               "\"Data\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\",\"City\":\"Shanghai\",\"Street\":\"Fengjin RD\"}," +
                               "\"Infos\":[1,\"abc\",3]," +
                               "\"D_Data\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\",\"City\":\"Shanghai\",\"Street\":\"Fengjin RD\"}," +
                               "\"D_Infos\":[1,\"abc\",3]" +
                             "}," +
                             "{" +
                               "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelPerson\"," +
                               "\"@odata.id\":\"http://localhost/odata/Managers(2)\"," +
                               "\"@odata.editLink\":\"Managers(2)\"," +
                               "\"Id\":2," +
                               "\"Name\":\"Sun\"," +
                               "\"Data\":[" +
                                 "42," +
                                 "null," +
                                 "\"abc\"," +
                                 "{\"ACity\":\"Shanghai\",\"AData\":[42,\"Red\"]}" +
                                "]," +
                                "\"Infos\":[42,\"Red\"]," +
                                "\"D_Data\":{\"D_City\":[]}," +
                                "\"D_Infos\":[{\"k\":\"v\"}]" +
                              "}" +
                            "]" +
                          "}"
                    },
                    {
                        "application/json;odata.metadata=minimal",
                        "{" +
                          "\"@odata.context\":\"http://localhost/odata/$metadata#Managers\"," +
                          "\"value\":[" +
                            "{" +
                              "\"Id\":1," +
                              "\"Name\":\"Sun\"," +
                              "\"Data\":{\"City\":\"Shanghai\",\"Street\":\"Fengjin RD\"}," +
                              "\"Infos\":[1,\"abc\",3]," +
                              "\"D_Data\":{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\",\"City\":\"Shanghai\",\"Street\":\"Fengjin RD\"}," +
                              "\"D_Infos\":[1,\"abc\",3]" +
                            "}," +
                            "{" +
                              "\"Id\":2," +
                              "\"Name\":\"Sun\"," +
                              "\"Data\":[" +
                                "42," +
                                "null," +
                                "\"abc\"," +
                                "{\"ACity\":\"Shanghai\",\"AData\":[42,\"Red\"]}" +
                              "]," +
                              "\"Infos\":[42,\"Red\"]," +
                              "\"D_Data\":{\"D_City\":[]}," +
                              "\"D_Infos\":[{\"k\":\"v\"}]" +
                            "}" +
                          "]" +
                        "}"
                    },
                    {
                        "application/json;odata.metadata=none",
                        "{" +
                          "\"value\":[" +
                            "{" +
                              "\"Id\":1," +
                              "\"Name\":\"Sun\"," +
                              "\"Data\":{\"City\":\"Shanghai\",\"Street\":\"Fengjin RD\"}," +
                              "\"Infos\":[1,\"abc\",3]," +
                              "\"D_Data\":{\"City\":\"Shanghai\",\"Street\":\"Fengjin RD\"}," +
                              "\"D_Infos\":[1,\"abc\",3]" +
                            "}," +
                            "{" +
                              "\"Id\":2," +
                              "\"Name\":\"Sun\"," +
                              "\"Data\":[" +
                                "42," +
                                "null," +
                                "\"abc\"," +
                                "{\"ACity\":\"Shanghai\",\"AData\":[42,\"Red\"]}" +
                              "]," +
                              "\"Infos\":[42,\"Red\"]," +
                              "\"D_Data\":{\"D_City\":[]}," +
                              "\"D_Infos\":[{\"k\":\"v\"}]" +
                            "}" +
                          "]" +
                        "}"
                    }
                };

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(ManagersQueryCases))]
        public async Task QueryUntypedEntitySet_OnDifferentMetadataLevel(string format, string expected)
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync($"odata/managers?$format={format}");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal(expected, payloadBody);
        }

        public static TheoryDataSet<string, string> DeclaredPropertyQueryCases
        {
            get
            {
                var data = new TheoryDataSet<string, string>
                {
                    // for Edm.Untyped
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
                        "{\"@odata.context\":\"http://localhost/odata/$metadata#People(4)/Data/Edm.Untyped\"," +
                            "\"ZipCode\":\"<--->\",\"Location\":\"******\"" +
                        "}"
                    },
                    {
                        "odata/people/5/data",
                        "{" +
                          "\"@odata.context\":\"http://localhost/odata/$metadata#People(5)/Data/Edm.Untyped\"," +
                          "\"value\":[" +
                            "null," +
                            "42," +
                            "{" +
                              "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\"," +
                              "\"City\":\"Redmond\"," +
                              "\"Street\":\"134TH AVE\"" +
                            "}" +
                          "]" +
                        "}"
                    },

                    // for Collection(Edm.Untyped)
                    {
                        "odata/people/1/infos",
                        "{" +
                          "\"@odata.context\":\"http://localhost/odata/$metadata#People(1)/Infos/Edm.Untyped\"," +
                          "\"value\":[1,2,3]" +
                        "}"
                    },
                    {
                        "odata/people/2/infos",
                        "{" +
                          "\"@odata.context\":\"http://localhost/odata/$metadata#People(2)/Infos/Edm.Untyped\"," +
                          "\"value\":[\"Blue\",\"Green\",\"Apple\"]" +
                        "}"
                    },
                    {
                        "odata/people/3/infos",
                        "{" +
                          "\"@odata.context\":\"http://localhost/odata/$metadata#People(3)/Infos/Edm.Untyped\"," +
                          "\"value\":[{\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\",\"City\":\"Issaq\",\"Street\":\"Klahanie Way\"}]" +
                        "}"
                    },
                    {
                        "odata/people/4/infos",
                        "{" +
                          "\"@odata.context\":\"http://localhost/odata/$metadata#People(4)/Infos/Edm.Untyped\"," +
                          "\"value\":[{\"ZipCode\":\"<===>\",\"Location\":\"Info-Locations\"}]" +
                        "}"
                    },
                    {
                        "odata/people/5/infos",
                        "{" +
                          "\"@odata.context\":\"http://localhost/odata/$metadata#People(5)/Infos/Edm.Untyped\"," +
                          "\"value\":[{\"ZipCode\":\"<===>\",\"Location\":\"!@#$\"}]" +
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

        [Fact]
        public async Task QuerySinglePeople_WithDeclaredOrUndeclaredEnum_OnUntypedAndDynamicProperty()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("odata/people/22");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":22," +
                "\"Name\":\"Yin\"," +
                "\"EnumDynamic1@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelColor\"," +
                "\"EnumDynamic1\":\"Blue\"," +
                "\"EnumDynamic2\":\"Apple\"," +
                "\"Data@odata.type\":\"#String\"," +
                "\"Data\":\"Apple\"," +
                "\"Infos\":[\"Blue\",\"Green\",\"Apple\"]" +
              "}", payloadBody);
        }

        [Fact]
        public async Task QuerySinglePeople_WithCollectionInCollection_OnDeclaredUntypedProperty()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("odata/people/99");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":99," +
                "\"Name\":\"Chuan\"," +
                "\"Data\":[" +
                  "null," +
                  "[" +
                    "42," +
                    "{" +
                       "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\"," +
                       "\"City\":\"Redmond\"," +
                       "\"Street\":\"134TH AVE\"" +
                     "}" +
                   "]" +
                 "]," +
                 "\"Infos\":[" +
                   "[" +
                     "{\"ZipCode\":\"NoAValidZip\",\"Location\":\"OnEarth\"}," + // a resource whose type is not defined in the Edm model, so there's no @odata.type
                     "null," +
                     "[" +
                       "[" +
                         "[" +
                           "{" +
                             "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\"," +
                             "\"City\":\"Issaquah\"," +
                             "\"Street\":\"80TH ST\"" +
                           "}" +
                         "]" +
                       "]" +
                     "]" +
                   "]," +
                   "42" +
                 "]," +
                 "\"Dp\":[" +
                   "{" +
                     "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\"," +
                     "\"City\":\"BlackCastle\"," +
                     "\"Street\":\"To Castle Rd\"" +
                   "}" +
                 "]" +
               "}", payloadBody);
        }

        [Fact]
        public async Task CreatePerson_WithPrimitiveUntypedValueODataTyped_Works_RoundTrip()
        {
            // Arrange
            const string payload = @"{
  ""data@odata.type"": ""#Edm.Guid"",
  ""data"":""40EE4E85-C443-41B2-9611-C55F97D80E84"",
  ""id"": 90
}";
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/people");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":90," +
                "\"Name\":null," +
                "\"Data@odata.type\":\"#Guid\"," +
                "\"Data\":\"40ee4e85-c443-41b2-9611-c55f97d80e84\"," +
                "\"Infos\":[]" +
              "}", payloadBody);
        }

        [Fact]
        public async Task CreatePerson_WithEnumUntypedValueODataTyped_Works_RoundTrip()
        {
            // Arrange
            const string payload = @"{
  ""data@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelColor"",
  ""data"":""Blue"",
  ""id"": 91
}";
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/people");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":91," +
                "\"Name\":null," +
                "\"Data\":\"Blue\"," +
                "\"Infos\":[]" +
              "}", payloadBody);
        }

        [Fact]
        public async Task CreatePerson_WithResourceUntypedValueODataTyped_Works_RoundTrip()
        {
            // Arrange
            const string payload = @"{
  ""data"":{
    ""@odata.type"":""#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress"",
    ""City"":""Redmond"",
    ""Street"":""156TH AVE""
  },
  ""id"": 92
}";
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/people");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":92," +
                "\"Name\":null," +
                "\"Data\":{\"City\":\"Redmond\",\"Street\":\"156TH AVE\"}," +
                "\"Infos\":[]" +
              "}", payloadBody);
        }

        [Fact]
        public async Task CreatePerson_WithPrimitiveCollectionUntypedValueODataTyped_Works_RoundTrip()
        {
            // Arrange
            const string payload = @"{
  ""data"":[
    4,
    {
      ""@odata.type"":""#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress"",
      ""City"":""Earth"",
      ""Street"":""Min AVE""
    }
  ],
  ""id"": 94
}";
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/people");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":94," +
                "\"Name\":null," +
                "\"Data\":[" +
                  "4," +
                  "{" +
                    "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Untyped.InModelAddress\"," +
                    "\"City\":\"Earth\"," +
                    "\"Street\":\"Min AVE\"" +
                  "}" +
                "]," +
                "\"Infos\":[]" +
              "}", payloadBody);
        }

        [Fact]
        public async Task CreatePerson_WithCollectionItemUntypedValueODataTyped_Works_RoundTrip()
        {
            // Arrange
            const string payload = @"{
  ""data@odata.type"":""#Collection(Edm.Int32)"",
  ""data"":[
    4,
    5
  ],
  ""id"": 93
}";
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/people");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#People/$entity\"," +
                "\"Id\":93," +
                "\"Name\":null," +
                "\"Data\":[4,5]," +
                "\"Infos\":[]" +
              "}", payloadBody);
        }

        [Fact]
        public async Task CreatePerson_Works_RoundTrip()
        {
            // Arrange
            const string payload = @"{
  ""infos"":[
       [42],
       {""k1"": ""abc"", ""k2"": 42, ""k:3"": { ""a1"": 2, ""b2"": null}, ""k/4"": [null, 42]}
  ],
  ""data"":{
    ""type"":""LineString"",""coordinates"":[
      [
        1.0,1.0
      ],[
        3.0,3.0
      ],[
        4.0,4.0
      ],[
        0.0,0.0
      ]
    ],""crs"":{
      ""type"":""name"",""properties"":{
        ""name"":""EPSG:4326""
      }
    }
  },
  ""id"": 98,
  ""name"":""Sam"",
  ""dynamic_p"": [
        null,
        {
            ""X1"": ""Red"",
            ""Data"": {
                ""D1"": 42
            }
        },
        ""finance"",
        ""hr"",
        ""legal"",
        43
    ]
}";
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/people");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"coordinates\":[[1.0,1.0],[3.0,3.0],[4.0,4.0],[0.0,0.0]],\"cr", payloadBody);
            Assert.Contains("\"k:3\":{\"a1@odata.type\":\"#Decimal\",\"a1\":2,\"b2\":null},\"k/4\":[null,42]}],\"dyna", payloadBody);
        }
    }
}
