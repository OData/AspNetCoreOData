//-----------------------------------------------------------------------------
// <copyright file="DateOnlyAndTimeOnlyWithEfTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyAndTimeOnly;

public class DateOnlyAndTimeOnlyWithEfTest : WebApiTestBase<DateOnlyAndTimeOnlyWithEfTest>
{
    public DateOnlyAndTimeOnlyWithEfTest(WebApiTestFixture<DateOnlyAndTimeOnlyWithEfTest> fixture)
        :base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EfDateOnlyAndTimeOnlyModelContext8";
        services.AddDbContext<EfDateOnlyAndTimeOnlyModelContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connectionString));

        services.ConfigureControllers(typeof(MetadataController), typeof(DateOnlyAndTimeOnlyModelsController));

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
"    <Schema Namespace=\"Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyAndTimeOnly\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityType Name=\"DateOnlyAndTimeOnlyModel\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"Id\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"EndDay\" Type=\"Edm.Date\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"DeliverDay\" Type=\"Edm.Date\" />\r\n" +
"        <Property Name=\"ResumeTime\" Type=\"Edm.TimeOfDay\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"Id\" Type=\"Edm.Int32\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"Birthday\" Type=\"Edm.Date\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"PublishDay\" Type=\"Edm.Date\" />\r\n" +
"        <Property Name=\"DateOnly\" Type=\"Edm.Date\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"NullableDateOnly\" Type=\"Edm.Date\" />\r\n" +
"        <Property Name=\"TimeOnly\" Type=\"Edm.TimeOfDay\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"NullableTimeOnly\" Type=\"Edm.TimeOfDay\" />\r\n" +
"        <Property Name=\"CreatedTime\" Type=\"Edm.TimeOfDay\" Nullable=\"false\" />\r\n" +
"        <Property Name=\"EndTime\" Type=\"Edm.TimeOfDay\" />\r\n" +
"      </EntityType>\r\n" +
"    </Schema>\r\n" +
"    <Schema Namespace=\"Default\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityContainer Name=\"Container\">\r\n" +
"        <EntitySet Name=\"DateOnlyAndTimeOnlyModels\" EntityType=\"Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyAndTimeOnly.DateOnlyAndTimeOnlyModel\" />\r\n" +
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
        string Uri = "odata/DateOnlyAndTimeOnlyModels";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(5, result["value"].Count());

        // test one for each entity
        Assert.Equal("2015-12-23", result["value"][0]["EndDay"]);
        Assert.Equal(JValue.CreateNull(), result["value"][1]["DeliverDay"]);
        Assert.Equal("08:06:04.0030000", result["value"][2]["ResumeTime"]);
        Assert.Equal(JValue.CreateNull(), result["value"][3]["EndTime"]);
        Assert.Equal("05:03:05.0790000", result["value"][4]["CreatedTime"]);
    }

    [Fact]
    public async Task CanQuerySingleEntity_WithDateOnlyAndTimeOnlyProperties()
    {
        // Arrange
        string Uri = "odata/DateOnlyAndTimeOnlyModels(2)";

        string expect = @"{
  ""@odata.context"": ""{XXXX}/odata/$metadata#DateOnlyAndTimeOnlyModels/$entity"",
  ""@odata.type"": ""#Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyAndTimeOnly.DateOnlyAndTimeOnlyModel"",
  ""@odata.id"": ""{XXXX}/odata/DateOnlyAndTimeOnlyModels(2)"",
  ""@odata.editLink"": ""DateOnlyAndTimeOnlyModels(2)"",
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
  ""DateOnly@odata.type"":""#Date"",
  ""DateOnly"":""2015-10-22"",
  ""NullableDateOnly@odata.type"":""#Date"",
  ""NullableDateOnly"":""2015-12-24"",
  ""TimeOnly@odata.type"":""#TimeOfDay"",
  ""TimeOnly"":""03:30:00.0000000"",
  ""NullableTimeOnly@odata.type"":""#TimeOfDay"",
  ""NullableTimeOnly"":""07:30:00.0000000"",
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

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JObject.Parse(expect), result);
    }

    [Fact]
    public async Task CanSelect_OnDateOnlyAndTimeOnlyProperties()
    {
        // Arrange
        string Uri = "odata/DateOnlyAndTimeOnlyModels(3)?$select=Birthday,PublishDay,DeliverDay,CreatedTime,ResumeTime";
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
    [InlineData("?$filter=CreatedTime eq 04:03:05.0790000", "4")]
    [InlineData("?$filter=hour(EndTime) eq 11", "1")]
    [InlineData("?$filter=minute(EndTime) eq 06", "3")]
    [InlineData("?$filter=second(EndTime) eq 10", "5")]
    [InlineData("?$filter=EndTime eq null", "2,4")]
    [InlineData("?$filter=EndTime ge 02:03:05.0790000", "1,3,5")]
    public async Task CanFilter_OnDateOnlyAndTimeOnlyProperties(string filter, string expect)
    {
        // Arrange
        string Uri = "odata/DateOnlyAndTimeOnlyModels" + filter;
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
    public async Task CanOrderBy_OnDateOnlyAndTimeOnlyProperties(string orderby, string expect)
    {
        // Arrange
        string Uri = "odata/DateOnlyAndTimeOnlyModels" + orderby;
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
    public async Task PostEntity_WithDateOnlyAndTimeOnlyTimeProperties()
    {
        // Arrange
        const string Payload = "{" +
            "\"Id\":99," +
            "\"Birthday\":\"2099-01-01\"," +
            "\"DateOnly\":\"2099-05-01\"," +
            "\"NullableDateOnly\":\"2099-06-01\"," +
            "\"TimeOnly\":\"10:11:12\"," +
            "\"NullableTimeOnly\":\"13:14:15\"," +
            "\"CreatedTime\":\"14:13:15.1790000\"," +
            "\"EndDay\":\"1990-12-22\"}";

        string Uri = "odata/DateOnlyAndTimeOnlyModels";
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

        var content = await response.Content.ReadAsStringAsync();
        var result = JObject.Parse(content);
        Assert.Equal("2099-01-01", result["Birthday"]);
        Assert.Equal("2099-05-01", result["DateOnly"]);
        Assert.Equal("2099-06-01", result["NullableDateOnly"]);
        Assert.Equal("10:11:12", result["TimeOnly"]);
        Assert.Equal("13:14:15", result["NullableTimeOnly"]);
        Assert.Equal("14:13:15.1790000", result["CreatedTime"]);

        Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#DateOnlyAndTimeOnlyModels/$entity\"," +
            "\"EndDay\":\"1990-12-22\"," +
            "\"DeliverDay\":null," +
            "\"ResumeTime\":\"00:00:00.0000000\"," +
            "\"Id\":99," +
            "\"Birthday\":\"2099-01-01\"," +
            "\"PublishDay\":null," +
            "\"DateOnly\":\"2099-05-01\"," +
            "\"NullableDateOnly\":\"2099-06-01\"," +
            "\"TimeOnly\":\"10:11:12.0000000\"," +
            "\"NullableTimeOnly\":\"13:14:15.0000000\"," +
            "\"CreatedTime\":\"14:13:15.1790000\"," +
            "\"EndTime\":null}", content);
    }

    [Fact]
    public async Task PutEntity_WithDateOnlyAndTimeOnlyProperties()
    {
        // Arrange
        const string Payload = "{" +
            "\"Birthday\":\"2199-01-02\"," +
             "\"DateOnly\":\"2099-05-01\"," +
            "\"NullableDateOnly\":\"2099-06-01\"," +
            "\"TimeOnly\":\"10:11:12\"," +
            "\"NullableTimeOnly\":\"13:14:15\"," +
            "\"CreatedTime\":\"14:13:15.1790000\"}";

        string Uri = "odata/DateOnlyAndTimeOnlyModels(3)";
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

    [Theory]
    [InlineData("?$filter=DateOnly eq null", "")]
    [InlineData("?$filter=DateOnly ne null", "1,2,3,4,5")]
    [InlineData("?$filter=DateOnly eq 2015-12-23", "1")]
    [InlineData("?$filter=DateOnly gt 2015-10-23", "1,3,5")]
    [InlineData("?$filter=DateOnly lt 2015-10-22", "4")]
    [InlineData("?$filter=NullableDateOnly eq null", "4")]
    [InlineData("?$filter=NullableDateOnly ne null", "1,2,3,5")]
    [InlineData("?$filter=NullableDateOnly eq 2015-06-22", "3")]
    [InlineData("?$filter=NullableDateOnly gt 2015-12-20", "2")]
    [InlineData("?$filter=NullableDateOnly lt 2015-12-20", "1,3,5")]
    [InlineData("?$filter=NullableDateOnly le 2015-12-24", "1,2,3,5")]
    [InlineData("?$filter=TimeOnly eq null", "")]
    [InlineData("?$filter=TimeOnly ne null", "1,2,3,4,5")]
    [InlineData("?$filter=TimeOnly eq 11:30:00.0000000", "1,5")]
    [InlineData("?$filter=TimeOnly gt 21:00:59", "3")]
    [InlineData("?$filter=TimeOnly lt 13:43:00", "1,2,4,5")]
    [InlineData("?$filter=NullableTimeOnly eq null", "3")]
    [InlineData("?$filter=NullableTimeOnly ne null", "1,2,4,5")]
    [InlineData("?$filter=NullableTimeOnly eq 21:30:00", "4")]
    [InlineData("?$filter=NullableTimeOnly gt 19:50:00", "4,5")]
    [InlineData("?$filter=NullableTimeOnly lt 07:30:00", "1")]
    public async Task CanFilter_OnDateOnlyAndTimeOnlyProperties_AllVariants(string filter, string expect)
    {
        // Arrange
        string uri = "odata/DateOnlyAndTimeOnlyModels" + filter;
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        JObject result = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(expect, string.Join(",", result["value"].Select(e => e["Id"].ToString())));
    }

    [Theory]
    [InlineData("?$orderby=DateOnly", "4,2,1,3,5")]
    [InlineData("?$orderby=DateOnly desc", "5,3,1,2,4")]
    [InlineData("?$orderby=NullableDateOnly", "4,5,3,1,2")]
    [InlineData("?$orderby=NullableDateOnly desc", "2,1,3,5,4")]
    [InlineData("?$orderby=TimeOnly", "4,2,5,1,3")]
    [InlineData("?$orderby=TimeOnly desc", "3,1,5,2,4")]
    [InlineData("?$orderby=NullableTimeOnly", "3,1,2,4,5")]
    [InlineData("?$orderby=NullableTimeOnly desc", "5,4,2,1,3")]
    public async Task CanOrderBy_OnDateOnlyAndTimeOnlyProperties_AllVariants(string orderby, string expect)
    {
        // Arrange
        string uri = "odata/DateOnlyAndTimeOnlyModels" + orderby;
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
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
    public async Task CanSelect_OnDateOnlyAndTimeOnlyProperties_AllVariants()
    {
        // Arrange
        string uri = "odata/DateOnlyAndTimeOnlyModels(3)?$select=DateOnly,NullableDateOnly,TimeOnly,NullableTimeOnly";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("2015-12-25", result["DateOnly"]);
        Assert.Equal("2015-12-25", result["NullableDateOnly"]);
        Assert.Equal("08:30:00", result["TimeOnly"]);
        Assert.Equal("08:30:00", result["NullableTimeOnly"]);
    }

    [Fact]
    public async Task CanGroupBy_DateOnlyAndTimeOnlyProperties()
    {
        // Arrange
        string uri = "odata/DateOnlyAndTimeOnlyModels?$apply=groupby((DateOnly, TimeOnly), aggregate($count as Cnt))";
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(uri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("?$filter=year(DateOnly) eq 2015", "1,2,3,4,5")]
    [InlineData("?$filter=year(DateOnly) eq null", "")]
    [InlineData("?$filter=month(DateOnly) eq 12", "1,3,5")]
    [InlineData("?$filter=day(DateOnly) eq 22", "2,4")]
    [InlineData("?$filter=year(NullableDateOnly) eq 2015", "1,2,3,5")]
    [InlineData("?$filter=month(NullableDateOnly) eq 6", "3")]
    [InlineData("?$filter=day(NullableDateOnly) eq 22", "1,3,5")]
    [InlineData("?$filter=day(NullableDateOnly) eq null", "4")]
    [InlineData("?$filter=hour(TimeOnly) eq 11", "1,5")]
    [InlineData("?$filter=hour(TimeOnly) eq null", "")]
    [InlineData("?$filter=minute(TimeOnly) eq 30", "1,2,3,4,5")]
    [InlineData("?$filter=minute(TimeOnly) eq 60", "")]
    [InlineData("?$filter=second(TimeOnly) eq 0", "1,2,3,4,5")]
    [InlineData("?$filter=second(TimeOnly) eq 8", "")]
    [InlineData("?$filter=hour(NullableTimeOnly) eq 8", "")]
    [InlineData("?$filter=hour(NullableTimeOnly) eq 7", "2")]
    [InlineData("?$filter=minute(NullableTimeOnly) eq 21", "")]
    [InlineData("?$filter=minute(NullableTimeOnly) eq null", "3")]
    [InlineData("?$filter=minute(NullableTimeOnly) eq 30", "1,2,4,5")]
    [InlineData("?$filter=second(NullableTimeOnly) eq 0", "1,2,4,5")]
    public async Task CanFilter_OnDateOnlyAndTimeOnlyFunctions(string filter, string expect)
    {
        // Arrange
        string uri = "odata/DateOnlyAndTimeOnlyModels" + filter;
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        JObject result = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(expect, string.Join(",", result["value"].Select(e => e["Id"].ToString())));
    }

    [Theory]
    [InlineData("?$orderby=year(DateOnly)", "1,2,3,4,5")]
    [InlineData("?$orderby=month(DateOnly) desc", "1,3,5,2,4")]
    [InlineData("?$orderby=day(DateOnly)", "2,4,1,3,5")]
    [InlineData("?$orderby=year(NullableDateOnly)", "4,5,1,2,3")]
    [InlineData("?$orderby=month(NullableDateOnly) desc", "2,1,3,5,4")]
    [InlineData("?$orderby=day(NullableDateOnly)", "4,5,1,3,2")]
    [InlineData("?$orderby=hour(TimeOnly) desc", "3,1,5,2,4")]
    [InlineData("?$orderby=minute(TimeOnly)", "1,2,3,4,5")]
    [InlineData("?$orderby=second(TimeOnly)", "1,2,3,4,5")]
    [InlineData("?$orderby=hour(NullableTimeOnly) desc", "5,4,2,1,3")]
    [InlineData("?$orderby=minute(NullableTimeOnly)", "3,4,5,1,2")]
    [InlineData("?$orderby=second(NullableTimeOnly)", "3,4,5,1,2")]
    public async Task CanOrderBy_OnDateOnlyAndTimeOnlyFunctions(string orderby, string expect)
    {
        // Arrange
        string uri = "odata/DateOnlyAndTimeOnlyModels" + orderby;
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
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
    public async Task CanSelect_OnDateOnlyAndTimeOnlyFunctions()
    {
        // Arrange
        string uri = "odata/DateOnlyAndTimeOnlyModels(3)?$select=DateOnly,TimeOnly";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        var dateOnly = DateOnly.Parse((string)result["DateOnly"]);
        var timeOnly = TimeOnly.Parse((string)result["TimeOnly"]);
        Assert.Equal(2015, dateOnly.Year);
        Assert.Equal(12, dateOnly.Month);
        Assert.Equal(25, dateOnly.Day);
        Assert.Equal(23, timeOnly.Hour);
        Assert.Equal(30, timeOnly.Minute);
        Assert.Equal(0, timeOnly.Second);
    }

    private static IEdmModel BuildEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<DateOnlyAndTimeOnlyModel>("DateOnlyAndTimeOnlyModels");

        var type = builder.EntityType<DateOnlyAndTimeOnlyModel>();
        type.Property(c => c.EndDay).AsDateOnly();
        type.Property(c => c.DeliverDay).AsDateOnly();
        type.Property(c => c.ResumeTime).AsTimeOnly();

        return builder.GetEdmModel();
    }
}

public class DateOnlyAndTimeOnlyModelsController : ODataController
{
    private EfDateOnlyAndTimeOnlyModelContext _db;

    public DateOnlyAndTimeOnlyModelsController(EfDateOnlyAndTimeOnlyModelContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        if (!context.DateTimes.Any())
        {
            DateTime dt = new DateTime(2015, 12, 22);
            DateOnly dateOnly = DateOnly.FromDateTime(dt);
            TimeOnly timeOnly = TimeOnly.FromDateTime(dt.AddHours(5).AddMinutes(30));

            IList<DateOnlyAndTimeOnlyModel> dateTimes = Enumerable.Range(1, 5).Select(i =>
                new DateOnlyAndTimeOnlyModel
                {
                    // Id = i,
                    Birthday = dt.AddYears(i),
                    EndDay = dt.AddDays(i),
                    DeliverDay = i % 2 == 0 ? (DateTime?)null : dt.AddYears(5 - i),
                    PublishDay = i % 2 != 0 ? (DateTime?)null : dt.AddMonths(5 - i),
                    CreatedTime = new TimeSpan(0, i, 3, 5, 79),
                    EndTime = i % 2 == 0 ? (TimeSpan?)null : new TimeSpan(0, 10 + i, 3 + i, 5 + i, 79 + i),
                    ResumeTime = new TimeSpan(0, 8, 6, 4, 3),
                    DateOnly = i % 2 == 0 ? dateOnly.AddMonths(i * -1) : dateOnly.AddDays(i),
                    TimeOnly = i % 2 == 0 ? timeOnly.AddHours(i * -1) : timeOnly.AddHours(i * 30),
                    NullableDateOnly = i == 4 ? null : i % 2 == 0 ? dateOnly.AddDays(i) : dateOnly.AddMonths(i * -2),
                    NullableTimeOnly = i == 3 ? null : i < 3 ? timeOnly.AddHours(i) : timeOnly.AddHours(i + 60),
                }).ToList();

            foreach (var efDateTime in dateTimes)
            {
                context.DateTimes.Add(efDateTime);
            }

            context.SaveChanges();
        }

        _db = context;
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_db.DateTimes);
    }

    [EnableQuery]
    public IActionResult Get(int key)
    {
        DateOnlyAndTimeOnlyModel dtm = _db.DateTimes.FirstOrDefault(e => e.Id == key);
        if (dtm == null)
        {
            return NotFound();
        }

        return Ok(dtm);
    }

    public IActionResult Post([FromBody]DateOnlyAndTimeOnlyModel dt)
    {
        Assert.NotNull(dt);

        Assert.Equal(99, dt.Id);
        Assert.Equal(new DateTime(2099, 1, 1), dt.Birthday);
        Assert.Equal(new TimeSpan(0, 14, 13, 15, 179), dt.CreatedTime);
        Assert.Equal(new DateTime(1990, 12, 22), dt.EndDay);

        return Created(dt);
    }

    public IActionResult Put(int key, [FromBody]Delta<DateOnlyAndTimeOnlyModel> dt)
    {
        Assert.Equal(new[] { "Birthday", "DateOnly", "NullableDateOnly", "TimeOnly", "NullableTimeOnly", "CreatedTime" }, dt.GetChangedPropertyNames());

        // Birthday
        object value;
        bool success = dt.TryGetPropertyValue("Birthday", out value);
        Assert.True(success);
        DateTime dateTime = Assert.IsType<DateTime>(value);
        Assert.Equal(DateTimeKind.Unspecified, dateTime.Kind);
        Assert.Equal(new DateTime(2199, 1, 2), dateTime);

        // DateOnly
        success = dt.TryGetPropertyValue("DateOnly", out value);
        Assert.True(success);
        DateOnly dateOnly = Assert.IsType<DateOnly>(value);
        Assert.Equal(new DateOnly(2099, 5, 1), dateOnly);

        // NullableDateOnly
        success = dt.TryGetPropertyValue("NullableDateOnly", out value);
        Assert.True(success);
        dateOnly = Assert.IsType<DateOnly>(value);
        Assert.Equal(new DateOnly(2099, 6, 1), dateOnly);

        // TimeOnly
        success = dt.TryGetPropertyValue("TimeOnly", out value);
        Assert.True(success);
        TimeOnly timeOnly = Assert.IsType<TimeOnly>(value);
        Assert.Equal(new TimeOnly(10, 11, 12), timeOnly);

        // NullableTimeOnly
        success = dt.TryGetPropertyValue("NullableTimeOnly", out value);
        Assert.True(success);
        timeOnly = Assert.IsType<TimeOnly>(value);
        Assert.Equal(new TimeOnly(13, 14, 15), timeOnly);

        // CreatedTime
        success = dt.TryGetPropertyValue("CreatedTime", out value);
        Assert.True(success);
        TimeSpan timeSpan = Assert.IsType<TimeSpan>(value);
        Assert.Equal(new TimeSpan(0, 14, 13, 15, 179), timeSpan);
        return Updated(dt);
    }
}

public class EfDateOnlyAndTimeOnlyModelContext : DbContext
{
    public EfDateOnlyAndTimeOnlyModelContext(DbContextOptions<EfDateOnlyAndTimeOnlyModelContext> options)
        : base(options)
    {
    }

    public DbSet<DateOnlyAndTimeOnlyModel> DateTimes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DateOnlyAndTimeOnlyModel>().Property(c => c.EndDay).HasColumnType("date");
        modelBuilder.Entity<DateOnlyAndTimeOnlyModel>().Property(c => c.DeliverDay).HasColumnType("date");
    }
}

public class DateOnlyAndTimeOnlyModel
{
    public int Id { get; set; }

    [Column(TypeName = "date")]
    public DateTime Birthday { get; set; }

    [Column(TypeName = "DaTe")]
    public DateTime? PublishDay { get; set; }

    public DateTime EndDay { get; set; } // will use the Fluent API

    public DateTime? DeliverDay { get; set; } // will use the Fluent API

    public DateOnly DateOnly { get; set; }

    public DateOnly? NullableDateOnly { get; set; }

    public TimeOnly TimeOnly { get; set; }

    public TimeOnly? NullableTimeOnly { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan CreatedTime { get; set; }

    [Column(TypeName = "tIme")]
    public TimeSpan? EndTime { get; set; }

    public TimeSpan ResumeTime { get; set; } // will use the Fluent API
}
