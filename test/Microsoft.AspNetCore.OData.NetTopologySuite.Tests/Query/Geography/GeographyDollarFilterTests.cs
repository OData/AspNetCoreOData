//-----------------------------------------------------------------------------
// <copyright file="GeographyDollarFilterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;
using Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geography;

public class GeographyDollarFilterTests : WebApiTestBase<GeographyDollarFilterTests>
{
    public GeographyDollarFilterTests(WebApiTestFixture<GeographyDollarFilterTests> fixture) : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var model = GeographyDollarFilterEdmModel.GetEdmModel();

        services.ConfigureControllers(
            typeof(SitesController));

        string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=DollarFilterNetTopologySuiteGeographyDb;";
        services.AddDbContext<GeographyDollarFilterDbContext>(
            options => options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.UseNetTopologySuite();
            }));

        services.AddControllers().AddOData(
            options =>
            options.EnableQueryFeatures().AddRouteComponents(
                routePrefix: string.Empty,
                model: model,
                configureServices: (nestedServices) =>
                {
                    nestedServices.AddSingleton<IFilterBinder, ExtendedFilterBinder>();
                    nestedServices.AddODataNetTopologySuite();
                }));
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    [Fact]
    public async Task GeoDistance_Filter_With_GeographyLiteral_Returns_Single_ResultAsync()
    {
        // Arrange
        var queryUrl = $"/Sites?$filter=geo.distance(Location,geography'SRID=4326;POINT(-122.123889 47.669444)') lt 0.05";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        //Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var valueProp = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        // value array
        Assert.Equal(JsonValueKind.Array, valueProp.ValueKind);
        var site1 = Assert.Single(valueProp.EnumerateArray().ToArray());

        Assert.Equal(1, site1.GetProperty("Id").GetInt32());

        // Location
        var location = site1.GetProperty("Location");
        Assert.Equal("Point", location.GetProperty("type").GetString());

        var locationCoords = location.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, locationCoords.Length);
        Assert.Equal(-122.123889, locationCoords[0].GetDouble(), 6);
        Assert.Equal(47.669444, locationCoords[1].GetDouble(), 6);

        var locationCrs = location.GetProperty("crs");
        Assert.Equal("name", locationCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", locationCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);

        // Route
        var route = site1.GetProperty("Route");
        Assert.Equal("LineString", route.GetProperty("type").GetString());

        var routeCoords = route.GetProperty("coordinates").EnumerateArray().Select(a => a.EnumerateArray().ToArray()).ToArray();
        Assert.Equal(3, routeCoords.Length);

        Assert.Equal(-122.20, routeCoords[0][0].GetDouble(), 2);
        Assert.Equal(47.65, routeCoords[0][1].GetDouble(), 2);

        Assert.Equal(-122.18, routeCoords[1][0].GetDouble(), 2);
        Assert.Equal(47.66, routeCoords[1][1].GetDouble(), 2);

        Assert.Equal(-122.16, routeCoords[2][0].GetDouble(), 2);
        Assert.Equal(47.67, routeCoords[2][1].GetDouble(), 2);

        var routeCrs = route.GetProperty("crs");
        Assert.Equal("name", routeCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", routeCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeoLength_Filter_With_GeographyLiteral_Returns_Single_ResultAsync()
    {
        // Arrange
        var queryUrl = $"/Sites?$filter=geo.length(Route) gt 4000";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var valueProp = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        // value array
        Assert.Equal(JsonValueKind.Array, valueProp.ValueKind);
        var site2 = Assert.Single(valueProp.EnumerateArray().ToArray());

        // Id
        Assert.Equal(2, site2.GetProperty("Id").GetInt32());

        // Location
        var location = site2.GetProperty("Location");
        Assert.Equal("Point", location.GetProperty("type").GetString());

        var locationCoords = location.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, locationCoords.Length);
        Assert.Equal(-122.335167, locationCoords[0].GetDouble(), 6);
        Assert.Equal(47.608013, locationCoords[1].GetDouble(), 6);

        var locationCrs = location.GetProperty("crs");
        Assert.Equal("name", locationCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", locationCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);

        // Route
        var route = site2.GetProperty("Route");
        Assert.Equal("LineString", route.GetProperty("type").GetString());

        var routeCoords = route.GetProperty("coordinates").EnumerateArray().Select(a => a.EnumerateArray().ToArray()).ToArray();
        Assert.Equal(3, routeCoords.Length);
        Assert.Equal(-122.10, routeCoords[0][0].GetDouble(), 2);
        Assert.Equal(47.60, routeCoords[0][1].GetDouble(), 2);
        Assert.Equal(-122.08, routeCoords[1][0].GetDouble(), 2);
        Assert.Equal(47.62, routeCoords[1][1].GetDouble(), 2);
        Assert.Equal(-122.06, routeCoords[2][0].GetDouble(), 2);
        Assert.Equal(47.62, routeCoords[2][1].GetDouble(), 2);

        var routeCrs = route.GetProperty("crs");
        Assert.Equal("name", routeCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", routeCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeoIntersects_Filter_With_GeographyLiteral_Returns_Single_ResultAsync()
    {
        // Arrange
        // NOTE: SQL Server's geography requires the polygon to be counter-clockwise
        var queryUrl = $"/Sites?$filter=geo.intersects(Location,geography'SRID=4326;POLYGON((-122.345 47.606,-122.325 47.606,-122.325 47.610,-122.345 47.610,-122.345 47.606))')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var valueProp = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        // value array
        Assert.Equal(JsonValueKind.Array, valueProp.ValueKind);
        var site2 = Assert.Single(valueProp.EnumerateArray().ToArray());

        // Id
        Assert.Equal(2, site2.GetProperty("Id").GetInt32());

        // Location
        var location = site2.GetProperty("Location");
        Assert.Equal("Point", location.GetProperty("type").GetString());

        var locationCoords = location.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, locationCoords.Length);
        Assert.Equal(-122.335167, locationCoords[0].GetDouble(), 6);
        Assert.Equal(47.608013, locationCoords[1].GetDouble(), 6);

        var locationCrs = location.GetProperty("crs");
        Assert.Equal("name", locationCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", locationCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);

        // Route
        var route = site2.GetProperty("Route");
        Assert.Equal("LineString", route.GetProperty("type").GetString());

        var routeCoords = route.GetProperty("coordinates").EnumerateArray().Select(a => a.EnumerateArray().ToArray()).ToArray();
        Assert.Equal(3, routeCoords.Length);
        Assert.Equal(-122.10, routeCoords[0][0].GetDouble(), 2);
        Assert.Equal(47.60, routeCoords[0][1].GetDouble(), 2);
        Assert.Equal(-122.08, routeCoords[1][0].GetDouble(), 2);
        Assert.Equal(47.62, routeCoords[1][1].GetDouble(), 2);
        Assert.Equal(-122.06, routeCoords[2][0].GetDouble(), 2);
        Assert.Equal(47.62, routeCoords[2][1].GetDouble(), 2);

        var routeCrs = route.GetProperty("crs");
        Assert.Equal("name", routeCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", routeCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }
}
