// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class DateAndTimeOfDayWithEfTests : WebApiTestBase<DateAndTimeOfDayWithEfTests>
    {

        public DateAndTimeOfDayWithEfTests(WebApiTestFixture<DateAndTimeOfDayWithEfTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (services) =>
            {
                string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EfDateAndTimeOfDayModelContext8";
                services.AddDbContext<EfDateAndTimeOfDayModelContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connectionString));

                services.ConfigureControllers(typeof(MetadataController), typeof(DateAndTimeOfDayModelsController));

                services.AddControllers().AddOData(opt => opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select()
                    .AddModel("odata", BuildEdmModel()));
            };
        }

        [Fact]
        public async Task MetadataDocument_IncludesDateAndTimeOfDayProperties()
        {
            // Arrange
            string Uri = "odata/$metadata";
            string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
"<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">\r\n" +
"  <edmx:DataServices>\r\n" +
"    <Schema Namespace=\"Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityType Name=\"DateAndTimeOfDayModel\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"Id\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"EndDay\" Type=\"Edm.Date\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"DeliverDay\" Type=\"Edm.Date\" />\r\n" +
"        <Property Name=\"ResumeTime\" Type=\"Edm.TimeOfDay\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"Birthday\" Type=\"Edm.Date\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"PublishDay\" Type=\"Edm.Date\" />\r\n" +
"        <Property Name=\"CreatedTime\" Type=\"Edm.TimeOfDay\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"EndTime\" Type=\"Edm.TimeOfDay\" />\r\n" +
"      </EntityType>\r\n" +
"    </Schema>\r\n" +
"    <Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityContainer Name=\"Container\">\r\n" +
"        <EntitySet Name=\"DateAndTimeOfDayModels\" EntityType=\"Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay.DateAndTimeOfDayModel\" />\r\n" +
"      </EntityContainer>\r\n" +
"    </Schema>\r\n" +
"  </edmx:DataServices>\r\n" +
"</edmx:Edmx>";

            // Remove indentation
            expected = Regex.Replace(expected, @"\r\n\s*<", @"<");
            HttpClient client = CreateClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expected, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CanQueryEntitySet_WithDateAndTimeOfDayProperties()
        {
            // Arrange
            string Uri = "odata/DateAndTimeOfDayModels";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Output.WriteLine(responseContent);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var result = JObject.Parse(responseContent);

            Assert.Equal(5, result["value"].Count());

            // test one for each entity
            Assert.Equal("2015-12-23", result["value"][0]["EndDay"]);
            Assert.Equal(JValue.CreateNull(), result["value"][1]["DeliverDay"]);
            Assert.Equal("08:06:04.0030000", result["value"][2]["ResumeTime"]);
            Assert.Equal(JValue.CreateNull(), result["value"][3]["EndTime"]);
            Assert.Equal("05:03:05.0790000", result["value"][4]["CreatedTime"]);
        }

        [Fact]
        public async Task CanQuerySingleEntity_WithDateAndTimeOfDayProperties()
        {
            // Arrange
            string Uri = "odata/DateAndTimeOfDayModels(2)";

            string expect = @"{
  ""@odata.context"": ""{XXXX}/odata/$metadata#DateAndTimeOfDayModels/$entity"",
  ""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay.DateAndTimeOfDayModel"",
  ""@odata.id"": ""{XXXX}/odata/DateAndTimeOfDayModels(2)"",
  ""@odata.editLink"": ""DateAndTimeOfDayModels(2)"",
  ""EndDay@odata.type"": ""#Date"",
  ""EndDay"": ""2015-12-24"",
  ""DeliverDay"": null,
  ""ResumeTime@odata.type"": ""#TimeOfDay"",
  ""ResumeTime"": ""08:06:04.0030000"",
  ""Id"": 2,
  ""Birthday@odata.type"": ""#Date"",
  ""Birthday"": ""2017-12-22"",
  ""PublishDay@odata.type"": ""#Date"",
  ""PublishDay"": ""2016-03-22"",
  ""CreatedTime@odata.type"": ""#TimeOfDay"",
  ""CreatedTime"": ""02:03:05.0790000"",
  ""EndTime"": null
}";
            expect = expect.Replace("{XXXX}", "http://localhost");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Output.WriteLine(responseContent);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var result = JObject.Parse(responseContent);
            Assert.Equal(JObject.Parse(expect), result);
        }

        [Fact]
        public async Task CanSelect_OnDateAndTimeOfDayProperties()
        {
            // Arrange
            string Uri = "odata/DateAndTimeOfDayModels(3)?$select=Birthday,PublishDay,DeliverDay,CreatedTime,ResumeTime";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("2018-12-22", result["Birthday"]);
            Assert.Equal(JValue.CreateNull(), result["PublishDay"]);
            Assert.Equal("2017-12-22", result["DeliverDay"]);
            Assert.Equal("03:03:05.0790000", result["CreatedTime"]);
            Assert.Equal("08:06:04.0030000", result["ResumeTime"]);
        }

        [Theory]
        [InlineData("?$filter=year(Birthday) eq 2017", "2")]
        [InlineData("?$filter=month(PublishDay) eq 01", "4")]
        [InlineData("?$filter=day(EndDay) ne 27", "1,2,3,4")]
        [InlineData("?$filter=Birthday gt 2017-12-22", "3,4,5")]
        [InlineData("?$filter=PublishDay eq null", "1,3,5")] // the following four cases are for nullable
        [InlineData("?$filter=PublishDay eq 2016-03-22", "2")]
        [InlineData("?$filter=PublishDay ne 2016-03-22", "1,3,4,5")]
        [InlineData("?$filter=PublishDay lt 2016-03-22", "4")]
        [InlineData("?$filter=EndTime ne null", "1,3,5")]
        // [InlineData("?$filter=CreatedTime eq 04:03:05.0790000", "4")] // EFCore could not be translated.
        // [InlineData("?$filter=hour(EndTime) eq 11", "1")] // EFCore could not be translated.
        // [InlineData("?$filter=minute(EndTime) eq 06", "3")] // EFCore could not be translated.
        // [InlineData("?$filter=second(EndTime) eq 10", "5")] // EFCore could not be translated.
        [InlineData("?$filter=EndTime eq null", "2,4")]
        // [InlineData("?$filter=EndTime ge 02:03:05.0790000", "1,3,5")] // EFCore could not be translated.
        public async Task CanFilter_OnDateAndTimeOfDayProperties(string filter, string expect)
        {
            // Arrange
            string Uri = "odata/DateAndTimeOfDayModels" + filter;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            JObject result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(expect, string.Join(",", result["value"].Select(e => e["Id"].ToString())));
        }

        [Theory]
        [InlineData("?$orderby=Birthday", "1,2,3,4,5")]
        [InlineData("?$orderby=Birthday desc", "5,4,3,2,1")]
        [InlineData("?$orderby=PublishDay", "1,3,5,4,2")]
        [InlineData("?$orderby=PublishDay desc", "2,4,5,3,1")]
        [InlineData("?$orderby=CreatedTime", "1,2,3,4,5")]
        [InlineData("?$orderby=CreatedTime desc", "5,4,3,2,1")]
        public async Task CanOrderBy_OnDateAndTimeOfDayProperties(string orderby, string expect)
        {
            // Arrange
            string Uri = "odata/DateAndTimeOfDayModels" + orderby;
            var request = new HttpRequestMessage(HttpMethod.Get, Uri);
            HttpClient client = CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(5, result["value"].Count());

            Assert.Equal(expect, string.Join(",", result["value"].Select(e => e["Id"].ToString())));
        }

        [Fact]
        public async Task PostEntity_WithDateAndTimeOfDayTimeProperties()
        {
            // Arrange
            const string Payload = "{" +
                "\"Id\":99," +
                "\"Birthday\":\"2099-01-01\"," +
                "\"CreatedTime\":\"14:13:15.1790000\"," +
                "\"EndDay\":\"1990-12-22\"}";

            string Uri = "odata/DateAndTimeOfDayModels";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Uri);

            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = Payload.Length;
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PutEntity_WithDateAndTimeOfDayProperties()
        {
            // Arrange
            const string Payload = "{" +
                "\"Birthday\":\"2199-01-02\"," +
                "\"CreatedTime\":\"14:13:15.1790000\"}";

            string Uri = "odata/DateAndTimeOfDayModels(3)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, Uri);

            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = Payload.Length;
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private static IEdmModel BuildEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<DateAndTimeOfDayModel>("DateAndTimeOfDayModels");

            var type = builder.EntityType<DateAndTimeOfDayModel>();
            type.Property(c => c.EndDay).AsDate();
            type.Property(c => c.DeliverDay).AsDate();
            type.Property(c => c.ResumeTime).AsTimeOfDay();

            return builder.GetEdmModel();
        }

    }

}
