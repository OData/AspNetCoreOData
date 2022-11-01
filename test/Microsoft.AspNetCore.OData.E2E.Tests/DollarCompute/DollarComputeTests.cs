//-----------------------------------------------------------------------------
// <copyright file="DollarComputeTests.cs" company=".NET Foundation">
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarCompute
{
    public class DollarComputeTests : WebApiTestBase<DollarComputeTests>
    {
        private readonly ITestOutputHelper output;

        public DollarComputeTests(WebApiTestFixture<DollarComputeTests> fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = DollarComputeEdmModel.GetEdmModel();

            services.ConfigureControllers(typeof(CustomersController));

            services.AddControllers().AddOData(opt =>
                opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select().AddRouteComponents("odata", edmModel));
        }

        [Theory]
        [InlineData("$filter=Total lt 30&$compute=Price mul Qty as Total", new [] { 1, 3 })]
        [InlineData("$filter=Total gt 30&$compute=Price mul Qty as Total", new [] { 2, 4, 5})]
        [InlineData("$filter=MainZipCode lt 90&$compute=Location/ZipCode div 1000 as MainZipCode", new[] { 2, 4, 5 })]
        [InlineData("$filter=FirstChar eq 'S'&$compute=substring(Name, 0, 1) as FirstChar", new[] { 2 })]
        public async Task QueryForResourceSet_IncludesDollarCompute_UsedInDollarFilter(string query, int[] ids)
        {
            // Arrange
            string queryUrl = $"odata/Customers?{query}";
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

            JObject payloadBody = await response.Content.ReadAsObject<JObject>();

            int[] actualIds = GetIds(payloadBody);
            Assert.True(ids.SequenceEqual(actualIds));
        }

        [Theory]
        [InlineData("$orderby=Total&$compute=Price mul Qty as Total", new[] { 1, 3, 2, 4, 5 })]
        [InlineData("$orderby=Total desc&$compute=Price mul Qty as Total", new[] { 5, 4, 2, 3, 1 })]
        [InlineData("$orderby=MainZipCode&$compute=Location/ZipCode div 1000 as MainZipCode", new[] { 5, 2, 4, 1, 3 })]
        [InlineData("$orderby=FirstChar&$compute=substring(Name, 0, 1) as FirstChar", new[] { 5, 3, 4, 1, 2 })]
        public async Task QueryForResourceSet_IncludesDollarCompute_UsedInDollarOrder(string query, int[] ids)
        {
            // Arrange
            string queryUrl = $"odata/Customers?{query}";
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

            JObject payloadBody = await response.Content.ReadAsObject<JObject>();

            int[] actualIds = GetIds(payloadBody);
            Assert.True(ids.SequenceEqual(actualIds));
        }

        private static int[] GetIds(JObject payload)
        {
            JArray value = payload["value"] as JArray;
            Assert.NotNull(value);

            int[] ids = new int[value.Count()];
            for (int i = 0; i < value.Count(); i++)
            {
                JObject item = value[i] as JObject;
                ids[i] = (int)item["Id"];
            }

            return ids;
        }

        [Fact]
        public async Task QueryForResourceSet_IncludesDollarCompute_UsedInDollarSelect()
        {
            // Arrange
            string queryUrl = "odata/Customers?$select=Name,Total,MainZipCode,FirstChar&$compute=Price mul Qty as Total,Location/ZipCode div 1000 as MainZipCode,substring(Name, 0, 1) as FirstChar";
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
                  "\"value\":[" +
                    "{\"Name\":\"Peter\",\"Total\":19.9,\"MainZipCode\":98,\"FirstChar\":\"P\"}," +
                    "{\"Name\":\"Sam\",\"Total\":44.85,\"MainZipCode\":32,\"FirstChar\":\"S\"}," +
                    "{\"Name\":\"John\",\"Total\":27.96,\"MainZipCode\":98,\"FirstChar\":\"J\"}," +
                    "{\"Name\":\"Kerry\",\"Total\":59.85,\"MainZipCode\":88,\"FirstChar\":\"K\"}," +
                    "{\"Name\":\"Alex\",\"Total\":180.2,\"MainZipCode\":12,\"FirstChar\":\"A\"}" +
                  "]" +
                "}", payload);
        }

        [Fact]
        public async Task QueryForResource_IncludesDollarCompute_UsedSelectAllInDollarSelect()
        {
            // Arrange
            string queryUrl = "odata/Customers(2)?$select=*&$compute=Age sub 20 as Age20Ago,toupper(Name) as UpperChar";
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
            Assert.Equal("{\"Id\":2,\"Name\":\"Sam\",\"Age\":40,\"Price\":2.99,\"Qty\":15,\"Age20Ago\":20,\"UpperChar\":\"SAM\",\"Location\":{\"Street\":\"Street 2\",\"ZipCode\":32509}}", payload);
        }

        [Fact]
        public async Task QueryForAnResource_IncludesDollarCompute_InNestedDollarSelect()
        {
            // Arrange
            string queryUrl = "odata/Customers(4)?$select=Location($select=StateCode;$compute=ZipCode div 1000 as StateCode)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert

            Assert.NotNull(response);
            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            Assert.Equal("{\"Location\":{\"StateCode\":88}}", payload);
        }

        [Fact]
        public async Task QueryForAnResource_IncludesDollarCompute_InTopLevelAndNestedDollarSelect()
        {
            // Arrange
            string queryUrl = "odata/Customers(4)?$select=Total,Location($select=StateCode;$compute=ZipCode div 1000 as StateCode)&$compute=Price mul Qty as Total";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            Assert.Equal("{\"Total\":59.85,\"Location\":{\"StateCode\":88}}", payload);
        }

        [Fact]
        public async Task QueryForAnResource_IncludesDollarCompute_InNestedDollarExpand_WithDollarSelectDollarFilter()
        {
            // Arrange
            string queryUrl = "odata/Customers(3)?$expand=Sales($select=Amount,TaxRate,Tax;$filter=Tax lt 4.0;$compute=Amount mul TaxRate as Tax)";
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

            Assert.Equal("{\"Id\":3," +
                "\"Name\":\"John\"," +
                "\"Age\":34," +
                "\"Price\":6.99," +
                "\"Qty\":4," +
                "\"Location\":{\"Street\":\"Street 3\",\"ZipCode\":98052}," +
                "\"Sales\":[" +
                  "{\"Amount\":3,\"TaxRate\":0.31,\"Tax\":0.9299999999999999}," +
                  "{\"Amount\":4,\"TaxRate\":0.41,\"Tax\":1.64}," +
                  "{\"Amount\":5,\"TaxRate\":0.51,\"Tax\":2.55}," +
                  "{\"Amount\":6,\"TaxRate\":0.61,\"Tax\":3.66}" +
                "]" +
              "}", payload);
        }

        [Fact]
        public async Task QueryForAnResource_IncludesDollarCompute_InNestedDollarExpand_WithDollarSelectDollarFilterDollarOrderBy()
        {
            // Arrange
            string queryUrl = "odata/Customers(4)?$expand=Sales($select=Amount,TaxRate,Tax;$orderby=Tax;$filter=Tax gt 4.0;$compute=Amount mul TaxRate as Tax)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            string payload = await response.Content.ReadAsStringAsync();

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            Assert.Equal("{\"Id\":4," +
                "\"Name\":\"Kerry\"," +
                "\"Age\":29," +
                "\"Price\":3.99," +
                "\"Qty\":15," +
                "\"Location\":{\"Street\":\"Street 4\",\"ZipCode\":88309}," +
                "\"Sales\":[" +
                  "{\"Amount\":7,\"TaxRate\":0.71,\"Tax\":4.97}," +
                  "{\"Amount\":8,\"TaxRate\":0.8099999999999999,\"Tax\":6.4799999999999995}," +
                  "{\"Amount\":9,\"TaxRate\":0.9099999999999999,\"Tax\":8.19}," +
                  "{\"Amount\":10,\"TaxRate\":1.01,\"Tax\":10.1}" +
                "]" +
              "}", payload);
        }

        [Fact]
        public async Task QuerySales_ThrowsNotAllowed_IncludesDollarCompute_WithAllowedQueryOptionsNone()
        {
            // Arrange
            string queryUrl = "odata/sales?$compute=Amount mul TaxRate as Tax";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            string payload = await response.Content.ReadAsStringAsync();

            Assert.Contains("The query specified in the URI is not valid. " +
                "Query option 'Compute' is not allowed. To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
