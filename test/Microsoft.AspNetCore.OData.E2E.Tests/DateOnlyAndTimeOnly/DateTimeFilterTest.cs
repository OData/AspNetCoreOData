//-----------------------------------------------------------------------------
// <copyright file="DateTimeFilterTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyAndTimeOnly;

public class DateTimeFilterTest : WebApiTestBase<DateTimeFilterTest>
{
    public DateTimeFilterTest(WebApiTestFixture<DateTimeFilterTest> fixture)
        :base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TestEntityInMemoryContext>(opt => opt.UseLazyLoadingProxies().UseInMemoryDatabase("TestEntityInMemoryContextList"));

        services.ConfigureControllers(typeof(TestEntitiesController));

        services.AddControllers()
        .AddOData(o => o
            .AddRouteComponents("", GetEdmModel())
            .Count().Expand().Filter().Select().SetMaxTop(1000).OrderBy()
            .TimeZone = TimeZoneInfo.Utc);
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<TestEntity>("TestEntities");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task QueryEntitiesWithDateTimeUsingDifferentTimeZone()
    {
        // Arrange
        string requestUri = $"TestEntities?$select=Label,Stamp&$filter=Stamp ge 2021-01-02T00:00:00Z and Stamp lt 2021-01-03T00:00:00Z";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        HttpClient client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string payload = await response.Content.ReadAsStringAsync();

        Assert.Equal("{\"@odata.context\":\"http://localhost/$metadata#TestEntities(Label,Stamp)\"," +
            "\"value\":[" +
              "{\"Label\":\"DAY 2\",\"Stamp\":\"2021-01-02T00:00:00Z\"}," +
              "{\"Label\":\"DAY 2\",\"Stamp\":\"2021-01-02T23:59:59Z\"}" +
            "]" +
          "}", payload);
    }
}

public class TestEntity
{
    public int Id { get; set; }
    public string Label { get; set; }
    public DateTime Stamp { get; set; }
}

public class TestEntityInMemoryContext : DbContext
{
    public TestEntityInMemoryContext(DbContextOptions<TestEntityInMemoryContext> options) : base(options)
    { }

    public DbSet<TestEntity> TestEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SPECIFY ALL DATETIME VALUES AS UTC
        var utcConverter = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        var allProps = modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties());

        foreach (var property in allProps)
        {
            if ((property.ClrType == typeof(DateTime)) || (property.ClrType == typeof(DateTime?))) { property.SetValueConverter(utcConverter); }
        }
    }
}

public class TestEntitiesController : ODataController
{
    private readonly TestEntityInMemoryContext _ctx;

    public TestEntitiesController(TestEntityInMemoryContext ctx)
    {
        _ctx = ctx;
        if (!_ctx.TestEntities.Any())
        {
            _ctx.TestEntities.Add(new TestEntity { Id = 1, Label = "DAY 1", Stamp = new DateTime(2021, 01, 01, 00, 00, 00) });
            _ctx.TestEntities.Add(new TestEntity { Id = 2, Label = "DAY 1", Stamp = new DateTime(2021, 01, 01, 23, 59, 59) });
            _ctx.TestEntities.Add(new TestEntity { Id = 3, Label = "DAY 2", Stamp = new DateTime(2021, 01, 02, 00, 00, 00) });
            _ctx.TestEntities.Add(new TestEntity { Id = 4, Label = "DAY 2", Stamp = new DateTime(2021, 01, 02, 23, 59, 59) });
            _ctx.TestEntities.Add(new TestEntity { Id = 5, Label = "DAY 3", Stamp = new DateTime(2021, 01, 03, 00, 00, 00) });
            _ctx.TestEntities.Add(new TestEntity { Id = 6, Label = "DAY 3", Stamp = new DateTime(2021, 01, 03, 23, 59, 59) });

            _ctx.SaveChanges();
        }
    }

    [HttpGet]
    [EnableQuery]
    public IEnumerable<TestEntity> Get()
    {
        return _ctx.TestEntities;
    }
}
