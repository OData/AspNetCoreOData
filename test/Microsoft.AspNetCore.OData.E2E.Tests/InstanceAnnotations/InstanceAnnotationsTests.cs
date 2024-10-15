//-----------------------------------------------------------------------------
// <copyright file="InstanceAnnotationsTests.cs" company=".NET Foundation">
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations
{
    public class InstanceAnnotationsTests : WebApiTestBase<InstanceAnnotationsTests>
    {
        private readonly ITestOutputHelper output;

        public InstanceAnnotationsTests(WebApiTestFixture<InstanceAnnotationsTests> fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = InstanceAnnotationsEdmModel.GetEdmModel();

            services.ConfigureControllers(typeof(CustomersController));

            services.AddControllers().AddOData(opt =>
                opt.EnableQueryFeatures().AddRouteComponents("odata", edmModel));
        }

        [Fact]
        public async Task QueryEntitySetWithTopLevelInstanceAnnotation_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers?$top=2&$select=name");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers(Name)\"," +
                "\"@NS.TestAnnotation\":1978," +
                "\"value\":[" +
                  "{\"Name\":\"Peter\"}," +
                  "{\"Name\":\"Sam\"}" +
                "]" +
              "}", payloadBody);
        }

        [Fact]
        public async Task QueryEntityWithOutAnyInstanceAnnotation_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/1");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"Peter\"," +
                "\"Age\":19," +
                "\"Magics\":[1,2]," +
                "\"Location\":{\"City\":\"City 1\",\"Street\":\"Street 1\"}," +
                "\"FavoriteSports\":{\"LikeMost\":\"Soccer\",\"Like\":[\"Basketball\",\"Badminton\"]}" +
              "}", payloadBody);
        }

        [Fact]
        public async Task QueryEntityWithInstanceAnnotationOnTypeOnly_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/2");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers/$entity\"," +
                "\"@NS.CUSTOMER2.Primitive\":22," +
                "\"Id\":2," +
                "\"Name\":\"Sam\"," +
                "\"Age\":40," +
                "\"Magics\":[15]," +
                "\"Location\":{\"City\":\"City 2\",\"Street\":\"Street 2\"}," +
                "\"FavoriteSports\":{\"LikeMost\":\"Badminton\",\"Like\":[\"Soccer\",\"Tennis\"]}}", payloadBody);
        }

        [Fact]
        public async Task QueryEntityWithSimpleInstanceAnnotationOnPropertyOnly_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/3");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers/$entity\"," +
                "\"Id\":3," +
                "\"Name\":\"John\"," +
                "\"Age@NS.CUSTOMER3.Primitive\":33," +
                "\"NS.CUSTOMER3.Collection@odata.type\":\"#Collection(String)\"," +
                "\"Age@NS.CUSTOMER3.Collection\":[\"abc\",\"xyz\"]," +
                "\"Age\":34," +
                "\"Magics\":[98,81]," +
                "\"Location\":{\"City\":\"City 3\",\"Street\":\"Street 3\"}," +
                "\"FavoriteSports\":{\"LikeMost\":\"Swimming\",\"Like\":[\"Tennis\"]}}", payloadBody);
        }

        [Fact]
        public async Task QueryEntityWithInstanceAnnotationOnTypeAndProperty_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/4");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers/$entity\"," +
                "\"@NS.CUSTOMER4.Complex\":{" +
                  "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress\"," +
                  "\"City\":\"Shanghai\"," +
                  "\"Street\":\"1199 RD\"" +
                "}," +
                "\"Id\":4," +
                "\"Name\":\"Kerry\"," +
                "\"Age\":29," +
                "\"Magics@NS.CUSTOMER4.Enum\":\"Badminton\"," + // Should contain an odata.type annotation for NS.CUSTOMER4.ENUM
                "\"Magics\":[6,4,5]," +
                "\"Location\":{\"City\":\"City 4\",\"Street\":\"Street 4\"}," +
                "\"FavoriteSports\":{\"LikeMost\":\"Tennis\",\"Like\":[\"Soccer\"]}}", payloadBody);
        }

        [Fact]
        public async Task QueryEntityWithNestedInstanceAnnotation_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/5");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers/$entity\"," +
                "\"Id\":5," +
                "\"Name@NS.CUSTOMER5.Complex\":{" +
                  "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress\"," +
                  "\"@NS.CUSTOMER5.NESTED.Primitive\":1101," +
                  "\"City\":\"Mars\"," +
                  "\"Street@NS.CUSTOMER5.NESTED.Primitive\":987," +
                  "\"Street\":\"1115 Star\"" +
                "}," +
                "\"Name\":\"Alex\"," +
                "\"Age\":8," +
                "\"Magics\":[9,10]," +
                "\"Location\":{\"City\":\"City 5\",\"Street\":\"Street 5\"}," +
                "\"FavoriteSports\":{\"LikeMost\":\"Baseball\",\"Like\":[\"Baseball\"]}}", payloadBody);
        }

        [Fact]
        public async Task QueryPrimitiveValuePropertyWithInstanceAnnotation_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/3/age");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers(3)/Age\"," +
                "\"@NS.CUSTOMER3.Primitive\":33," +
                "\"NS.CUSTOMER3.Collection@odata.type\":\"#Collection(String)\"," +
                "\"@NS.CUSTOMER3.Collection\":[\"abc\",\"xyz\"]," +
                "\"value\":34}", payloadBody);
        }

        [Fact(Skip = "https://github.com/OData/odata.net/issues/3001")]
        public async Task QueryCollectionValuePropertyWithInstanceAnnotation_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/4/magics");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers(4)/Magics\"," +
                "\"@NS.CUSTOMER4.Enum\":\"Badminton\"," +
                "\"value\":[6,4,5]}", payloadBody);
        }

        [Fact]
        public async Task QueryPropertyValueWithResourceInstanceAnnotation_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/5/name");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers(5)/Name\"," +
                "\"@NS.CUSTOMER5.Complex\":{" +
                  "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress\"," +
                  "\"@NS.CUSTOMER5.NESTED.Primitive\":1101," +
                  "\"City\":\"Mars\"," +
                  "\"Street@NS.CUSTOMER5.NESTED.Primitive\":987," +
                  "\"Street\":\"1115 Star\"" +
                "}," +
                "\"value\":\"Alex\"}", payloadBody);
        }

        [Fact]
        public async Task QueryPropertyValueWithInstanceAnnotationOnPropertyAndInstanceAnnotationOnType_Works()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/customers/6/location");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payloadBody = await response.Content.ReadAsStringAsync();

            // Instance annotations merged
            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Customers(6)/Location\"," +
                "\"@NS.CUSTOMER6.Primitive\":1115," +
                "\"@NS.CUSTOMER6.Location.Primitive\":71," +
                "\"City\":\"City 6\"," +
                "\"Street\":\"Street 6\"}", payloadBody);
        }

        [Fact]
        public async Task CreateEntityWithSimpleInstanceAnnotationOnType_WorksRoundTrip()
        {
            // Arrange
            string payload = @"{
                ""Name"": ""AnnotationOnTypeName1"",
                ""Age"": 71,
                ""Magics"": [ 3, 42 ],
                ""Location"": { ""Street"": ""1 Microsoft Way"", ""City"": ""Redmond"" },
                ""FavoriteSports"":{
                            ""LikeMost"":""Badminton"",
                            ""Like"":[""Basketball"",""Tennis""]
                    },
                ""@NS.Primitive"":44,
                ""@NS.Resource"":{""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress"", ""Street"": ""148TH AVE"", ""City"": ""Seattle"" }
            }";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/customers");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;

            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"@NS.Primitive\":44,", result);
            Assert.Contains("\"@NS.Resource\":{", result);
        }

        [Fact]
        public async Task CreateEntityWithSimpleInstanceAnnotationOnProperty_WorksRoundTrip()
        {
            // Arrange
            string payload = @"{
                ""Name"": ""AnnotationOnPropertyName1"",
                ""Age@NS.Primitive"": 74,
                ""Age@NS.CollectionTerm"": [1,2,3],
                ""Age"": 71,
                ""Magics@NS.Resource"":{""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress"", ""Street"": ""228TH ST"", ""City"": ""Issaquah"" },
                ""Magics"": [ 3, 42 ],
                ""Location"": { ""Street"": ""1 Microsoft Way"", ""City"": ""Redmond"" },
                ""FavoriteSports"":{
                            ""LikeMost"":""Badminton"",
                            ""Like"":[""Basketball"",""Tennis""]
                    },
                ""@NS.Primitive"":45
            }";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/customers");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;

            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseResult = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"@NS.Primitive\":45,", responseResult);
            Assert.Contains("\"Age@NS.CollectionTerm\":[1,2,3],", responseResult);
            Assert.Contains(",\"Magics@NS.Resource\":{\"@odata.type\":\"#Microsoft.As", responseResult); // the expect string is input intentionally.
        }

        [Fact]
        public async Task CreateEntityWithAdvancedInstanceAnnotations_WorksRoundTrip()
        {
            // Arrange
            string payload = @"{
                ""Name"": ""AdvancedAnnotations"",
                ""Age"": 54,
                ""Magics@NS.CollectionResources"":[
                    {""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress"", ""Street"": ""228TH ST"", ""City"": ""Issaquah"" },
                    {""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress"", ""Street"": ""228TH ST"", ""City"": ""Issaquah"" }],
                ""Magics"": [ 13, 14 ],
                ""Location"": { ""Street"": ""1 Microsoft Way"", ""City"": ""Redmond"" },
                ""@NS.Primitive"":520
            }";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/customers");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;

            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseResult = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"@NS.Primitive\":520,", responseResult);
            Assert.Contains("\"NS.CollectionResources@odata.type\":\"#Collection(Untyped)\"", responseResult);
            Assert.Contains(",\"Magics@NS.CollectionResources\":[{\"@odata.type\":\"#Microsoft.As", responseResult); // the expect string is input intentionally.
        }

        [Fact]
        public async Task CreateEntityWithUntypedResourceValueInstanceAnnotations_WorksRoundTrip()
        {
            // Arrange
            string payload = @"{
                ""Name"": ""UntypedAnnotations"",
                ""Age"": 101,
                ""Magics@NS.Collection"":[
                    { ""Street"": ""1199 RD"", ""City"": ""Xin"", ""Region"": ""Mei"" },
                    null,
                    {""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress"", ""Street"": ""Ren RD"", ""City"": ""Shang"" }],
                ""Magics"": [ 97 ]
            }";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/customers");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;

            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseResult = await response.Content.ReadAsStringAsync();

            // When fix the issue at ODL https://github.com/OData/odata.net/issues/2994, add more codes to verify the untyped instance annotation serialization.
            Assert.NotNull(responseResult);
        }

        [Fact]
        public async Task UpdateEntityWithSimpleInstanceAnnotationOnPropertyButWithoutValue_WorksRoundTrip()
        {
            // Arrange
            string payload = @"{
                ""Name"": ""NewName"",
                ""Age@NS.BirthYear"": 2077,
                ""Age@NS.CollectionTerm"": [71,72,73],
                ""Age"": 71,
                ""Magics@NS.StringCollection"":[""Skyline"",7,""Beaver""],
                ""Magics@NS.Resource"":{""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.InstanceAnnotations.InsAddress"", ""Street"": ""228TH ST"", ""City"": ""Earth"" },
                ""@NS.Primitive"":777
            }";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, "odata/customers/77");
            request.Headers.Add("accept", "application/json");
            request.Headers.Add("Prefer", @"odata.include-annotations=""*""");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;

            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
