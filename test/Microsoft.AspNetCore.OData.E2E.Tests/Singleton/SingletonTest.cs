// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Singleton
{
    public class SingletonTest : WebApiTestBase<SingletonTest>
    {
        public SingletonTest(WebApiTestFixture<SingletonTest> fixture)
            : base(fixture)
        {
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            var controllers = new[]
            {
                typeof(MetadataController),
                typeof(UmbrellaController),
                typeof(MonstersIncController),
                typeof(PartnersController),
                typeof(ODataEndpointController)
            };

            services.ConfigureControllers(controllers);
            services.AddControllers().AddOData(opt => opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select()
                .AddModel("odata", SingletonEdmModel.GetEdmModel()));
        }

        [Fact]
        public async Task SingletonShouldShowInServiceDocument()
        {
            // Arrange
            string requestUri = $"odata";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();

            Assert.Contains("{\"name\":\"Partners\",\"kind\":\"EntitySet\",\"url\":\"Partners\"}", result);
            Assert.Contains("{\"name\":\"Umbrella\",\"kind\":\"Singleton\",\"url\":\"Umbrella\"}", result);
            Assert.Contains("{\"name\":\"MonstersInc\",\"kind\":\"Singleton\",\"url\":\"MonstersInc\"}", result);
        }

        [Fact]
        public async Task TestRoutes()
        {
            // Arrange
            string requestUri = "$odata";
            HttpClient client = CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();
            string contentOfString = await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task NotCountable()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("odata/Umbrella/Partners/$count");

            // Assert
            string contentOfString = await response.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, string.Format(
                @"The response code is incorrect, expanded: 400, but actually: {0}, response: {1}.",
                response.StatusCode,
                contentOfString));

            Assert.Contains("\"message\":\"The query specified in the URI is not valid. The property 'Partners' cannot be used for $count.\"",
                contentOfString);
        }

        [Theory]
        [InlineData("odata/MonstersInc/Branches/$count", 2)]
        [InlineData("odata/MonstersInc/Branches/$count?$filter=City eq 'Shanghai'", 1)]
        public async Task QueryBranchesCount(string url, int expectedCount)
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(url);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(expectedCount == int.Parse(responseString),
                string.Format("Expected: {0}; Actual: {1}; Request URL: {2}", expectedCount, responseString, url));
        }

#if false
#region Singleton
        [Theory]
        [InlineData("expCon", "Umbrella")]
        [InlineData("expAttr", "MonstersInc")]
        [InlineData("conCon", "Umbrella")]
        [InlineData("conAttr", "MonstersInc")]
        public async Task SingletonCRUD(string model, string singletonName)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/{1}", model, singletonName);

            // Reset data source
            await ResetDataSource(model, singletonName);
            await ResetDataSource(model, "Partners");

            // GET singleton
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            dynamic result = JObject.Parse(await response.Content.ReadAsStringAsync());

            // PUT singleton with {"ID":1,"Name":"singletonName","Revenue":2000,"Category":"IT"}
            result["Revenue"] = 2000;
            response = await HttpClientExtensions.PutAsJsonAsync(this.Client, requestUri, result);
            response.EnsureSuccessStatusCode();

            // GET singleton/Revenue
            response = await this.Client.GetAsync(requestUri + "/Revenue");
            result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(2000, (int)result["value"]);

            // PATCH singleton with {"@odata.type":"#Microsoft.Test.E2E.AspNet.OData.Singleton.Company","Revenue":3000}
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Revenue"":3000}}", typeof(Company)));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // GET singleton
            response = await this.Client.GetAsync(requestUri);
            result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(3000, (int)result["Revenue"]);

            // Negative: Add singleton
            // POST singleton
            var company = new Company();
            response = await this.Client.PostAsJsonAsync(requestUri, company);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Negative: Delete singleton
            // DELETE singleton
            response = await this.Client.DeleteAsync(requestUri);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Navigation link CRUD, singleton is navigation source and entityset is navigation target
        [Theory]
        [InlineData("expCon", "Umbrella")]
        [InlineData("expAttr", "MonstersInc")]
        [InlineData("conCon", "Umbrella")]
        [InlineData("conAttr", "MonstersInc")]
        public async Task SingletonNavigationLinkCRUD(string model, string singletonName)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/{1}/Partners", model, singletonName);

            // Reset data source
            await ResetDataSource(model, singletonName);
            await ResetDataSource(model, "Partners");

            // GET singleton/Partners
            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            var json = await response.Content.ReadAsObject<JObject>();
            var result = json.GetValue("value") as JArray;
            Assert.Empty(result);

            string navigationLinkUri = string.Format(requestUri + "/$ref");

            // POST singleton/Partners/$ref
            string idLinkBase = string.Format(this.BaseAddress + "/{0}/Partners", model);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, navigationLinkUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "(1)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // POST singleton/Partners/$ref
            request = new HttpRequestMessage(HttpMethod.Post, navigationLinkUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "(2)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // POST singleton/Partners/$ref
            request = new HttpRequestMessage(HttpMethod.Post, navigationLinkUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "(3)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET singleton/Partners
            response = await this.Client.GetAsync(requestUri);
            json = await response.Content.ReadAsObject<JObject>();
            result = json.GetValue("value") as JArray;
            Assert.Equal<int>(3, result.Count);

            // Add Partner to Company by "Deep Insert"
            // POST singleton/Partners
            Partner partner = new Partner() { ID = 100, Name = "NewHire" };
            response = await this.Client.PostAsJsonAsync(requestUri, partner);
            response.EnsureSuccessStatusCode();

            // GET singleton/Partners
            response = await this.Client.GetAsync(requestUri);
            json = await response.Content.ReadAsObject<JObject>();
            result = json.GetValue("value") as JArray;
            Assert.Equal<int>(4, result.Count);

            // Unrelate Partners(3) from Company
            // DELETE singleton/Partners(3)/$ref
            request = new HttpRequestMessage(HttpMethod.Delete, requestUri + "(3)/$ref");
            response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET singleton/Partners
            response = await this.Client.GetAsync(requestUri);
            json = await response.Content.ReadAsObject<JObject>();
            result = json.GetValue("value") as JArray;
            Assert.Equal<int>(3, result.Count);

            // GET singleton/Microsoft.Test.E2E.AspNet.OData.Singleton.GetPartnersCount()
            requestUri = string.Format(BaseAddress + "/{0}/{1}/{2}.GetPartnersCount()", model, singletonName, NameSpace);
            response = await this.Client.GetAsync(requestUri);
            json = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(3, (int)json["value"]);
        }

        // Navigation link CRUD, where entityset is navigation source and singleton is navigation target
        [Theory]
        [InlineData("expAttr", "application/json;odata.metadata=full")]
        [InlineData("conAttr", "application/json;odata.metadata=minimal")]
        public async Task EntitySetNavigationLinkCRUD(string model, string format)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/Partners(1)/Company", model);
            string navigationUri = requestUri + "/$ref";
            string formatQuery = string.Format("?$format={0}", format);

            //Reset data source
            await ResetDataSource(model, "MonstersInc");
            await ResetDataSource(model, "Partners");

            // PUT Partners(1)/Company/$ref
            string idLinkBase = string.Format(this.BaseAddress + "/{0}/MonstersInc", model);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, navigationUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            var response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET Partners(1)/Company
            response = await this.Client.GetAsync(requestUri);
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("MonstersInc", (string)result["Name"]);

            // PUT Partners(1)/Company
            result["Revenue"] = 2000;
            response = await HttpClientExtensions.PutAsJsonAsync(this.Client, requestUri, result);
            response.EnsureSuccessStatusCode();

            // GET Partners(1)/Company/Revenue
            response = await this.Client.GetAsync(requestUri + formatQuery);
            result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(2000, (int)result["Revenue"]);

            // PATCH Partners(1)/Company
            request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            request.Content = new StringContent(string.Format(@"{{""@odata.type"":""#{0}"",""Revenue"":3000}}", typeof(Company)));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // GET Partners(1)/Company/Revenue
            response = await this.Client.GetAsync(requestUri + formatQuery);
            result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(3000, (int)result["Revenue"]);

            // DELETE Partners(1)/Company/$ref
            response = await Client.DeleteAsync(navigationUri);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // GET Partners(1)/Company
            response = await this.Client.GetAsync(requestUri + formatQuery);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Negative: POST Partners(1)/Company
            var company = new Company();
            response = await this.Client.PostAsJsonAsync(requestUri, company);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("expCon", "Umbrella", "application/json;odata.metadata=full")]
        [InlineData("expAttr", "MonstersInc", "application/json;odata.metadata=minimal")]
        [InlineData("conCon", "Umbrella", "application/json;odata.metadata=full")]
        [InlineData("conAttr", "MonstersInc", "application/json;odata.metadata=minimal")]
        public async Task SingletonDerivedTypeTest(string model, string singletonName, string format)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/{1}/{2}.SubCompany", model, singletonName, NameSpace);
            string formatQuery = string.Format("$format={0}", format);

            await ResetDataSource(model, singletonName);

            // PUT singleton/Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany
            var company = new { ID = 100, Name = "UmbrellaInSouthPole", Category = CompanyCategory.Communication.ToString(), Revenue = 1000, Location = "South Pole", Description = "The Umbrella In South Pole", Partners = new List<Partner>(), Branches = new List<Office>(), Office = new Office() { City = "South", Address = "999" } };
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(JsonConvert.SerializeObject(company));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // GET singleton/Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany/Location
            response = await this.Client.GetAsync(requestUri + "/Location?" + formatQuery);
            var result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(company.Location, (string)result["value"]);

            // Query complex type
            // GET GET singleton/Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany/Office
            response = await this.Client.GetAsync(requestUri + "/Office?" + formatQuery);
            result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(company.Office.City, (string)result["City"]);

            // GET singleton/Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany?$select=Location
            response = await this.Client.GetAsync(requestUri + "?$select=Location&" + formatQuery);
            result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(company.Location, (string)result["Location"]);
        }

        [Theory]
        [InlineData("expCon", "Umbrella")]
        [InlineData("expAttr", "MonstersInc")]
        [InlineData("conCon", "Umbrella")]
        [InlineData("conAttr", "MonstersInc")]
        public async Task SingletonQueryOptionsTest(string model, string singletonName)
        {
            string requestUri = string.Format(this.BaseAddress + "/{0}/{1}", model, singletonName);

            await ResetDataSource(model, singletonName);

            // GET /singleton?$select=Name
            var response = await this.Client.GetAsync(requestUri + "?$select=Name");
            var result = await response.Content.ReadAsObject<JObject>();
            int i = 0;
            foreach (var pro in result.Properties())
            {
                i++;
            }
            Assert.Equal(2, i);

            // POST /singleton/Partners
            Partner partner = new Partner() { ID = 100, Name = "NewHire" };
            response = await this.Client.PostAsJsonAsync(requestUri + "/Partners", partner);
            response.EnsureSuccessStatusCode();

            // POST /singleton/Partners
            partner = new Partner() { ID = 101, Name = "NewHire2" };
            response = await this.Client.PostAsJsonAsync(requestUri + "/Partners", partner);
            response.EnsureSuccessStatusCode();

            // GET /singleton?$expand=Partners($select=Name)
            response = await this.Client.GetAsync(requestUri + "?$expand=Partners($select=Name)");
            result = await response.Content.ReadAsObject<JObject>();
            var json = result.GetValue("Partners") as JArray;
            Assert.Equal(2, json.Count);

            // PUT Partners(1)/Company/$ref
            var navigationUri = string.Format(this.BaseAddress + "/{0}/Partners(1)/Company/$ref", model);
            string idLinkBase = string.Format(this.BaseAddress + "/{0}/{1}", model, singletonName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, navigationUri);
            request.Content = new StringContent("{ \"@odata.id\" : \"" + idLinkBase + "\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            response = await Client.SendAsync(request);

            // GET /Partners(1)?$expand=Company($select=Name)
            requestUri = string.Format(this.BaseAddress + "/{0}/Partners(1)", model);
            response = await this.Client.GetAsync(requestUri + "?$expand=Company($select=Name)");
            result = await response.Content.ReadAsObject<JObject>();
            var company = result.GetValue("Company") as JObject;
            Assert.Equal(singletonName, company.GetValue("Name"));
        }
#endregion
#endif
    }
}