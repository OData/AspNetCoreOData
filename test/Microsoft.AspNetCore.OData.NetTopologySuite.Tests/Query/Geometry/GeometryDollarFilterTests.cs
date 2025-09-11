//-----------------------------------------------------------------------------
// <copyright file="GeometryDollarFilterTests.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geometry;

public class GeometryDollarFilterTests : WebApiTestBase<GeometryDollarFilterTests>
{
    public GeometryDollarFilterTests(WebApiTestFixture<GeometryDollarFilterTests> fixture) : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var model = GeometryDollarFilterEdmModel.GetEdmModel();

        services.ConfigureControllers(
            typeof(PlantsController));

        string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=DollarFilterNetTopologySuiteGeometryDb;";
        services.AddDbContext<GeometryDollarFilterDbContext>(
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
    public async Task GeoDistance_Filter_With_GeometryLiteral_Returns_Single_ResultAsync()
    {
        // Arrange: SRID=0 planar point nearer Plant 1 location (15, 72)
        var queryUrl = $"/Plants?$filter=geo.distance(Location,geometry'SRID=0;POINT(7 13)') lt 60";
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
        var valueProp = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        // value array
        Assert.Equal(JsonValueKind.Array, valueProp.ValueKind);
        var plant1 = Assert.Single(valueProp.EnumerateArray().ToArray());
        Assert.Equal(1, plant1.GetProperty("Id").GetInt32());

        // Location
        var location = plant1.GetProperty("Location");
        Assert.Equal("Point", location.GetProperty("type").GetString());

        var locationCoords = location.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, locationCoords.Length);
        Assert.Equal(15.0, locationCoords[0].GetDouble(), 6); // X
        Assert.Equal(72.0, locationCoords[1].GetDouble(), 6); // Y

        var locationCrs = location.GetProperty("crs");
        Assert.Equal("name", locationCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", locationCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);

        // Route for Plant 1 (8,66) → (22,72) → (36,76)
        var route = plant1.GetProperty("Route");
        Assert.Equal("LineString", route.GetProperty("type").GetString());

        var routeCoords = route.GetProperty("coordinates").EnumerateArray()
                               .Select(a => a.EnumerateArray().ToArray()).ToArray();
        Assert.Equal(3, routeCoords.Length);

        Assert.Equal(8.0, routeCoords[0][0].GetDouble(), 2);
        Assert.Equal(66.0, routeCoords[0][1].GetDouble(), 2);

        Assert.Equal(22.0, routeCoords[1][0].GetDouble(), 2);
        Assert.Equal(72.0, routeCoords[1][1].GetDouble(), 2);

        Assert.Equal(36.0, routeCoords[2][0].GetDouble(), 2);
        Assert.Equal(76.0, routeCoords[2][1].GetDouble(), 2);

        var routeCrs = route.GetProperty("crs");
        Assert.Equal("name", routeCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", routeCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeoLength_Filter_With_GeometryLiteral_Returns_Single_ResultAsync()
    {
        // Arrange:
        // Plant 1 route length ≈ 29.8; Plant 2 ≈ 25.5 (in arbitrary planar units).
        // Use a threshold that returns only Plant 1.
        var queryUrl = $"/Plants?$filter=geo.length(Route) gt 28";
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
        var valueProp = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        // value array
        Assert.Equal(JsonValueKind.Array, valueProp.ValueKind);
        var plant1 = Assert.Single(valueProp.EnumerateArray().ToArray());

        // Id
        Assert.Equal(1, plant1.GetProperty("Id").GetInt32());

        // Location (15, 72)
        var location = plant1.GetProperty("Location");
        Assert.Equal("Point", location.GetProperty("type").GetString());

        var locationCoords = location.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, locationCoords.Length);
        Assert.Equal(15.0, locationCoords[0].GetDouble(), 6);
        Assert.Equal(72.0, locationCoords[1].GetDouble(), 6);

        var locationCrs = location.GetProperty("crs");
        Assert.Equal("name", locationCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", locationCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);

        // Route for Plant 1 (8,66) → (22,72) → (36,76)
        var route = plant1.GetProperty("Route");
        Assert.Equal("LineString", route.GetProperty("type").GetString());

        var routeCoords = route.GetProperty("coordinates").EnumerateArray()
                               .Select(a => a.EnumerateArray().ToArray()).ToArray();
        Assert.Equal(3, routeCoords.Length);

        Assert.Equal(8.0, routeCoords[0][0].GetDouble(), 2);
        Assert.Equal(66.0, routeCoords[0][1].GetDouble(), 2);

        Assert.Equal(22.0, routeCoords[1][0].GetDouble(), 2);
        Assert.Equal(72.0, routeCoords[1][1].GetDouble(), 2);

        Assert.Equal(36.0, routeCoords[2][0].GetDouble(), 2);
        Assert.Equal(76.0, routeCoords[2][1].GetDouble(), 2);

        var routeCrs = route.GetProperty("crs");
        Assert.Equal("name", routeCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", routeCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeoIntersects_Filter_With_GeometryLiteral_Returns_Single_ResultAsync()
    {
        // Arrange:
        // A small counter-clockwise rectangle around Plant 2 location (46, 61).
        var queryUrl =
            $"/Plants?$filter=geo.intersects(Location,geometry'SRID=0;POLYGON((44 60.8,48 60.8,48 61.2,44 61.2,44 60.8))')";
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
        var valueProp = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        // value array
        Assert.Equal(JsonValueKind.Array, valueProp.ValueKind);
        var plant2 = Assert.Single(valueProp.EnumerateArray().ToArray());

        // Id
        Assert.Equal(2, plant2.GetProperty("Id").GetInt32());

        // Location (46, 61)
        var location = plant2.GetProperty("Location");
        Assert.Equal("Point", location.GetProperty("type").GetString());

        var locationCoords = location.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, locationCoords.Length);
        Assert.Equal(46.0, locationCoords[0].GetDouble(), 6);
        Assert.Equal(61.0, locationCoords[1].GetDouble(), 6);

        var locationCrs = location.GetProperty("crs");
        Assert.Equal("name", locationCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", locationCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);

        // Route for Plant 2 (65,52) → (82,56) → (90,56)
        var route = plant2.GetProperty("Route");
        Assert.Equal("LineString", route.GetProperty("type").GetString());

        var routeCoords = route.GetProperty("coordinates").EnumerateArray()
                               .Select(a => a.EnumerateArray().ToArray()).ToArray();
        Assert.Equal(3, routeCoords.Length);

        Assert.Equal(65.0, routeCoords[0][0].GetDouble(), 2);
        Assert.Equal(52.0, routeCoords[0][1].GetDouble(), 2);

        Assert.Equal(82.0, routeCoords[1][0].GetDouble(), 2);
        Assert.Equal(56.0, routeCoords[1][1].GetDouble(), 2);

        Assert.Equal(90.0, routeCoords[2][0].GetDouble(), 2);
        Assert.Equal(56.0, routeCoords[2][1].GetDouble(), 2);

        var routeCrs = route.GetProperty("crs");
        Assert.Equal("name", routeCrs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", routeCrs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }
}
