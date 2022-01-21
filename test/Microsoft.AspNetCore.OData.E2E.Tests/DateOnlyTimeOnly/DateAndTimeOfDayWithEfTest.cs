//-----------------------------------------------------------------------------
// <copyright file="DateOnlyAndTimeOnlyWithEfTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyTimeOnly
{
    public class DateOnlyAndTimeOnlyWithEfTest : WebApiTestBase<DateOnlyAndTimeOnlyWithEfTest>
    {
        public DateOnlyAndTimeOnlyWithEfTest(WebApiTestFixture<DateOnlyAndTimeOnlyWithEfTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=DateOnlyAndTimeOnlyModelContext8";
            services.AddDbContext<DateAndOnlyTimeOnlyModelContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connectionString));

            services.ConfigureControllers(typeof(MetadataController), typeof(DateOnlyTimeOnlyModelsController));

            services.AddControllers().AddOData(opt => opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select()
                .AddRouteComponents("odata", BuildEdmModel()));
        }

        [Fact]
        public async Task MetadataDocument_IncludesDateOnlyAndTimeOnlyProperties()
        {
            // Arrange
            string Uri = "odata/$metadata";
            string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
"<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">\r\n" +
"  <edmx:DataServices>\r\n" +
"    <Schema Namespace=\"Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyTimeOnly\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityType Name=\"DateOnyTimeOnlyModel\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"Id\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"Birthday\" Type=\"Edm.Date\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"PublishDay\" Type=\"Edm.Date\" />\r\n" +
"        <Property Name=\"EndDay\" Type=\"Edm.Date\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"CreatedTime\" Type=\"Edm.TimeOfDay\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"EndTime\" Type=\"Edm.TimeOfDay\" />\r\n" +
"        <Property Name=\"ResumeTime\" Type=\"Edm.TimeOfDay\" Nullable=\"false\" />\r\n" +
"      </EntityType>\r\n" +
"    </Schema>\r\n" +
"    <Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityContainer Name=\"Container\">\r\n" +
"        <EntitySet Name=\"DateOnlyTimeOnlyModels\" EntityType=\"Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyTimeOnly.DateOnyTimeOnlyModel\" />\r\n" +
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
        public async Task CanQueryEntitySet_WithDateOnlyAndTimeOnlyProperties()
        {
            // Arrange
            string Uri = "odata/DateOnlyTimeOnlyModels";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(5, result["value"].Count());

            // test one for each entity
            Assert.Equal("2012-03-07", result["value"][0]["EndDay"]);
            Assert.Equal(JValue.CreateNull(), result["value"][1]["PublishDay"]);
            Assert.Equal("03:13:06.0080000", result["value"][2]["ResumeTime"]);
            Assert.Equal(JValue.CreateNull(), result["value"][3]["EndTime"]);
            Assert.Equal("00:05:03.0050000", result["value"][4]["CreatedTime"]);
        }

        [Fact]
        public async Task CanQuerySingleEntity_WithDateOnlyAndTimeOnlyProperties()
        {
            // Arrange
            string Uri = "odata/DateOnlyTimeOnlyModels(2)";

            string expect = @"{
  ""@odata.context"": ""http://localhost/odata/$metadata#DateOnlyTimeOnlyModels/$entity"",
  ""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyTimeOnly.DateOnyTimeOnlyModel"",
  ""@odata.id"": ""http://localhost/odata/DateOnlyTimeOnlyModels(2)"",
  ""@odata.editLink"": ""DateOnlyTimeOnlyModels(2)"",
  ""Id"": 2,
  ""Birthday@odata.type"": ""#Date"",
  ""Birthday"": ""2012-03-07"",
  ""PublishDay"": null,
  ""EndDay@odata.type"": ""#Date"",
  ""EndDay"": ""2013-04-08"",
  ""CreatedTime@odata.type"": ""#TimeOfDay"",
  ""CreatedTime"": ""00:02:03.0050000"",
  ""EndTime"": null,
  ""ResumeTime@odata.type"": ""#TimeOfDay"",
  ""ResumeTime"": ""02:12:05.0070000""
}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(JObject.Parse(expect), result);
        }

        [Fact]
        public async Task CanSelect_OnDateOnlyAndTimeOnlyProperties()
        {
            // Arrange
            string Uri = "odata/DateOnlyTimeOnlyModels(3)?$select=Birthday,PublishDay,CreatedTime,ResumeTime";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("2013-04-08", result["Birthday"]);
            Assert.Equal("2018-09-18", result["PublishDay"]);
            Assert.Equal("00:03:03.0050000", result["CreatedTime"]);
            Assert.Equal("03:13:06.0080000", result["ResumeTime"]);
        }

        [Theory]
        [InlineData("?$filter=year(Birthday) eq 2015", "5")]
        [InlineData("?$filter=month(PublishDay) eq 11", "1")]
        [InlineData("?$filter=day(EndDay) ne 09", "1,2,4,5")]
        [InlineData("?$filter=Birthday gt 2013-04-08", "4,5")]
        [InlineData("?$filter=PublishDay eq null", "2,4")] // the following four cases are for nullable
        [InlineData("?$filter=PublishDay eq 2018-09-18", "3")]
        [InlineData("?$filter=PublishDay ne 2018-09-18", "1,5")]
        [InlineData("?$filter=PublishDay lt 2019-12-31", "1,3")]
        [InlineData("?$filter=EndTime ne null", "1,3,5")]
        [InlineData("?$filter=CreatedTime eq 00:01:03.0050000", "1")]
        [InlineData("?$filter=hour(EndTime) eq 01", "1")]
        [InlineData("?$filter=minute(EndTime) eq 15", "5")]
        [InlineData("?$filter=second(EndTime) eq 06", "3")]
        [InlineData("?$filter=EndTime eq null", "2,4")]
        [InlineData("?$filter=EndTime ge 00:03:05.0790000", "1,3,5")]
        public async Task CanFilter_OnDateOnlyAndTimeOnlyProperties(string filter, string expect)
        {
            // Arrange
            string Uri = "odata/DateOnlyTimeOnlyModels" + filter;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            string payload = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode);

            JObject result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(expect, string.Join(",", result["value"].Select(e => e["Id"].ToString())));
        }

        [Theory]
        [InlineData("?$orderby=Birthday", "1,2,3,4,5")]
        [InlineData("?$orderby=Birthday desc", "5,4,3,2,1")]
        [InlineData("?$orderby=PublishDay", "2,4,1,3,5")]
        [InlineData("?$orderby=PublishDay desc", "5,3,1,2,4")]
        [InlineData("?$orderby=CreatedTime", "1,2,3,4,5")]
        [InlineData("?$orderby=CreatedTime desc", "5,4,3,2,1")]
        public async Task CanOrderBy_OnDateOnlyAndTimeOnlyProperties(string orderby, string expect)
        {
            // Arrange
            string Uri = "odata/DateOnlyTimeOnlyModels" + orderby;
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
        public async Task PostEntity_WithDateOnlyAndTimeOnlyProperties()
        {
            // Arrange
            const string Payload = "{" +
                "\"Id\":99," +
                "\"Birthday\":\"2099-01-01\"," +
                "\"CreatedTime\":\"14:13:15.1790000\"," +
                "\"EndDay\":\"1990-12-22\"}";

            string Uri = "odata/DateOnlyTimeOnlyModels";
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
        public async Task PutEntity_WithDateOnlyAndTimeOnlyProperties()
        {
            // Arrange
            const string Payload = "{" +
                "\"Birthday\":\"2199-01-02\"," +
                "\"CreatedTime\":\"14:13:15.1790000\"}";

            string Uri = "odata/DateOnlyTimeOnlyModels(3)";
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
            builder.EntitySet<DateOnyTimeOnlyModel>("DateOnlyTimeOnlyModels");
            return builder.GetEdmModel();
        }
    }

    public class DateOnlyTimeOnlyModelsController : ODataController
    {
      //  private DateAndOnlyTimeOnlyModelContext _db;
        private static IList<DateOnyTimeOnlyModel> _dateTimes = Enumerable.Range(1, 5).Select(i =>
            new DateOnyTimeOnlyModel
            {
                Id = i,
                Birthday = new DateOnly(2010 + i, 1 + i, 5 + i),

                PublishDay = i % 2 == 0 ? null : new DateOnly(2015 + i, 12 - i, 15 + i),

                EndDay = new DateOnly(2010 + i + 1, 2 + i, 6 + i),

                CreatedTime = new TimeOnly(0, i, 3, 5),

                EndTime = i % 2 == 0 ? null : new TimeOnly(i, 10 + i, 3 + i, 5 + i),

                ResumeTime = new TimeOnly(i, 10 + i, 3 + i, 5 + i)

            }).ToList();

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_dateTimes);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            DateOnyTimeOnlyModel dtm = _dateTimes.FirstOrDefault(e => e.Id == key);
            if (dtm == null)
            {
                return NotFound();
            }

            return Ok(dtm);
        }

        public IActionResult Post([FromBody]DateOnyTimeOnlyModel dt)
        {
            Assert.NotNull(dt);

            Assert.Equal(99, dt.Id);
            Assert.Equal(new DateOnly(2099, 1, 1), dt.Birthday);
            Assert.Equal(new TimeOnly(14, 13, 15, 179), dt.CreatedTime);
            Assert.Equal(new DateOnly(1990, 12, 22), dt.EndDay);

            return Created(dt);
        }

        public IActionResult Put(int key, [FromBody]Delta<DateOnyTimeOnlyModel> dt)
        {
            Assert.Equal(new[] { "Birthday", "CreatedTime" }, dt.GetChangedPropertyNames());

            // Birthday
            object value;
            bool success = dt.TryGetPropertyValue("Birthday", out value);
            Assert.True(success);
            DateOnly dateOnly = Assert.IsType<DateOnly>(value);
            Assert.Equal(new DateOnly(2199, 1, 2), dateOnly);

            // CreatedTime
            success = dt.TryGetPropertyValue("CreatedTime", out value);
            Assert.True(success);
            TimeOnly timeOnly = Assert.IsType<TimeOnly>(value);
            Assert.Equal(new TimeOnly(14, 13, 15, 179), timeOnly);
            return Updated(dt);
        }
    }

    // EF Core 6 doesn't support DateOnly and TimeOnly yet
    public class DateAndOnlyTimeOnlyModelContext : DbContext
    {
        public DateAndOnlyTimeOnlyModelContext(DbContextOptions<DateAndOnlyTimeOnlyModelContext> options)
            : base(options)
        {
        }

        public DbSet<DateOnyTimeOnlyModel> DateTimes { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<DateAndTimeOfDayModel>().Property(c => c.EndDay).HasColumnType("date");
        //    modelBuilder.Entity<DateAndTimeOfDayModel>().Property(c => c.DeliverDay).HasColumnType("date");
        //}
    }

    public class DateOnyTimeOnlyModel
    {
        public int Id { get; set; }

        public DateOnly Birthday { get; set; }

        public DateOnly? PublishDay { get; set; }

        public DateOnly EndDay { get; set; }

        public TimeOnly CreatedTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public TimeOnly ResumeTime { get; set; }
    }
}
#endif