//-----------------------------------------------------------------------------
// <copyright file="GeographySerializationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Geography;

public class GeographySerializationTests : WebApiTestBase<GeographySerializationTests>
{
    public GeographySerializationTests(WebApiTestFixture<GeographySerializationTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var model = GeographySerializationEdmModel.GetEdmModel();

        services.ConfigureControllers(
            typeof(SitesController), typeof(WarehousesController));

        services.AddControllers().AddOData(
            options =>
            options.EnableQueryFeatures().AddRouteComponents(
                routePrefix: string.Empty,
                model: model,
                configureServices: (services) => services.AddODataNetTopologySuite()));
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
    public async Task GeographyPoint_Serialization_Succeeds_As_GeoJson_With_CRS4326Async()
    {
        // Arrange
        var queryUrl = $"/Sites(1)/Marker";
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

        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        // Validate GeoJSON shape
        Assert.Equal("Point", geo.GetProperty("type").GetString());

        var coords = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, coords.Length);

        // GeoJSON uses [lon, lat] ordering.
        Assert.Equal(37.30750, coords[0].GetDouble(), 5);   // lon (X)
        Assert.Equal(-0.15083, coords[1].GetDouble(), 5);  // lat (Y)

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeographyLineString_Serialization_Returns_GeoJson_With_CRS4326()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Sites(1)/Route");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        Assert.Equal("LineString", geo.GetProperty("type").GetString());

        var coords = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, coords.Length);

        var p0 = coords[0].EnumerateArray().ToArray();
        var p1 = coords[1].EnumerateArray().ToArray();
        Assert.Equal(37.30750, p0[0].GetDouble(), 5);
        Assert.Equal(-0.15083, p0[1].GetDouble(), 5);
        Assert.Equal(37.32890, p1[0].GetDouble(), 5);
        Assert.Equal(-0.1647, p1[1].GetDouble(), 4);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeographyPolygon_Serialization_Returns_GeoJson_With_CRS4326()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Sites(1)/Park");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        Assert.Equal("Polygon", geo.GetProperty("type").GetString());

        // coordinates: [ outerRing[], hole1[], hole2[] ]
        var rings = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(3, rings.Length);

        // Outer ring first two positions
        var outer = rings[0].EnumerateArray().ToArray();
        var v0 = outer[0].EnumerateArray().ToArray();
        var v1 = outer[1].EnumerateArray().ToArray();
        Assert.Equal(37.0000, v0[0].GetDouble(), 4);
        Assert.Equal(0.1010, v0[1].GetDouble(), 4);
        Assert.Equal(37.0020, v1[0].GetDouble(), 4);
        Assert.Equal(0.1010, v1[1].GetDouble(), 4);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeographyMultiPoint_Serialization_Returns_GeoJson_With_CRS4326()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Sites(1)/Markers");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        Assert.Equal("MultiPoint", geo.GetProperty("type").GetString());

        var coords = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Single(coords);
        var p0 = coords[0].EnumerateArray().ToArray();
        Assert.Equal(37.30750, p0[0].GetDouble(), 5);
        Assert.Equal(-0.15083, p0[1].GetDouble(), 5);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeographyMultiLineString_Serialization_Returns_GeoJson_With_CRS4326()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Sites(1)/Routes");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        Assert.Equal("MultiLineString", geo.GetProperty("type").GetString());

        // coordinates: [ lineString[] ]
        var lines = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Single(lines);

        var line = lines[0].EnumerateArray().ToArray();
        Assert.Equal(2, line.Length);

        var p0 = line[0].EnumerateArray().ToArray();
        var p1 = line[1].EnumerateArray().ToArray();
        Assert.Equal(37.30750, p0[0].GetDouble(), 5);
        Assert.Equal(-0.15083, p0[1].GetDouble(), 5);
        Assert.Equal(37.32890, p1[0].GetDouble(), 5);
        Assert.Equal(-0.1647, p1[1].GetDouble(), 4);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeographyMultiPolygon_Serialization_Returns_GeoJson_With_CRS4326()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Sites(1)/Parks");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        Assert.Equal("MultiPolygon", geo.GetProperty("type").GetString());

        // coordinates: [ polygon[] ]
        var polys = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Single(polys);

        // first polygon rings
        var rings = polys[0].EnumerateArray().ToArray();
        Assert.Equal(3, rings.Length); // outer + 2 holes

        var outer = rings[0].EnumerateArray().ToArray();
        var v0 = outer[0].EnumerateArray().ToArray();
        Assert.Equal(37.0000, v0[0].GetDouble(), 4);
        Assert.Equal(0.1010, v0[1].GetDouble(), 4);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeographyGeometryCollection_Serialization_Returns_GeoJson_With_CRS4326()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Sites(1)/Features");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        Assert.Equal("GeometryCollection", geo.GetProperty("type").GetString());

        var geoms = geo.GetProperty("geometries").EnumerateArray().ToArray();
        Assert.Equal(6, geoms.Length);

        Assert.Equal("Point", geoms[0].GetProperty("type").GetString());
        Assert.Equal("LineString", geoms[1].GetProperty("type").GetString());
        Assert.Equal("Polygon", geoms[2].GetProperty("type").GetString());
        Assert.Equal("MultiPoint", geoms[3].GetProperty("type").GetString());
        Assert.Equal("MultiLineString", geoms[4].GetProperty("type").GetString());
        Assert.Equal("MultiPolygon", geoms[5].GetProperty("type").GetString());

        // Spot-check first point coords
        var p = geoms[0].GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(37.30750, p[0].GetDouble(), 5);
        Assert.Equal(-0.15083, p[1].GetDouble(), 5);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeographyPoint_Serialization_Succeeds_ForGeographyAttributeOnTypeAsync()
    {
        // Arrange
        var queryUrl = $"/Warehouses(1)/Location";
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

        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        // Validate GeoJSON shape
        Assert.Equal("Point", geo.GetProperty("type").GetString());

        var coords = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, coords.Length);

        // GeoJSON uses [lon, lat] ordering.
        Assert.Equal(37.30750, coords[0].GetDouble(), 5);   // lon (X)
        Assert.Equal(-0.15083, coords[1].GetDouble(), 5);  // lat (Y)

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeographyLineString_Serialization_Succeeds_ForGeographyAttributeOnTypeAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Warehouses(1)/Route");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var geo = root.TryGetProperty("value", out var valueElement) ? valueElement : root;

        Assert.Equal("LineString", geo.GetProperty("type").GetString());

        var coords = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, coords.Length);

        var p0 = coords[0].EnumerateArray().ToArray();
        var p1 = coords[1].EnumerateArray().ToArray();
        Assert.Equal(37.30750, p0[0].GetDouble(), 5);
        Assert.Equal(-0.15083, p0[1].GetDouble(), 5);
        Assert.Equal(37.32890, p1[0].GetDouble(), 5);
        Assert.Equal(-0.1647, p1[1].GetDouble(), 4);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("4326", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }
}
