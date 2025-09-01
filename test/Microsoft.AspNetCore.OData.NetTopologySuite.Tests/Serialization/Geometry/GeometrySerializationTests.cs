//-----------------------------------------------------------------------------
// <copyright file="GeometrySerializationTests.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Geometry;

public class GeometrySerializationTests : WebApiTestBase<GeometrySerializationTests>
{
    public GeometrySerializationTests(WebApiTestFixture<GeometrySerializationTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var model = GeometrySerializationEdmModel.GetEdmModel();

        services.ConfigureControllers(
            typeof(PlantsController));

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
    public async Task GeometryPoint_Serialization_Returns_GeoJson_With_CRS0()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Plants(1)/Location");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var geo = root.TryGetProperty("value", out var valueEl) ? valueEl : root;

        Assert.Equal("Point", geo.GetProperty("type").GetString());

        var coords = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, coords.Length);
        Assert.Equal(2.0, coords[0].GetDouble(), 6);
        Assert.Equal(2.0, coords[1].GetDouble(), 6);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeometryLineString_Serialization_Returns_GeoJson_With_CRS0()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Plants(1)/Track");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var geo = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        Assert.Equal("LineString", geo.GetProperty("type").GetString());

        var coords = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2, coords.Length);

        var p0 = coords[0].EnumerateArray().ToArray();
        var p1 = coords[1].EnumerateArray().ToArray();
        Assert.Equal(-2.0, p0[0].GetDouble(), 6);
        Assert.Equal(4.0, p0[1].GetDouble(), 6);
        Assert.Equal(12.0, p1[0].GetDouble(), 6);
        Assert.Equal(6.0, p1[1].GetDouble(), 6);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeometryPolygon_Serialization_Returns_GeoJson_With_CRS0()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Plants(1)/Zone");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var geo = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        Assert.Equal("Polygon", geo.GetProperty("type").GetString());

        var rings = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(3, rings.Length); // outer + 2 holes

        // Outer ring first two points
        var outer = rings[0].EnumerateArray().ToArray();
        var v0 = outer[0].EnumerateArray().ToArray();
        var v1 = outer[1].EnumerateArray().ToArray();
        Assert.Equal(0.0, v0[0].GetDouble(), 6);
        Assert.Equal(10.0, v0[1].GetDouble(), 6);
        Assert.Equal(10.0, v1[0].GetDouble(), 6);
        Assert.Equal(10.0, v1[1].GetDouble(), 6);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeometryMultiPoint_Serialization_Returns_GeoJson_With_CRS0()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Plants(1)/Locations");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var geo = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        Assert.Equal("MultiPoint", geo.GetProperty("type").GetString());

        var coords = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Single(coords);

        var p0 = coords[0].EnumerateArray().ToArray();
        Assert.Equal(2.0, p0[0].GetDouble(), 6);
        Assert.Equal(2.0, p0[1].GetDouble(), 6);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeometryMultiLineString_Serialization_Returns_GeoJson_With_CRS0()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Plants(1)/Tracks");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var geo = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        Assert.Equal("MultiLineString", geo.GetProperty("type").GetString());

        var lines = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Single(lines);

        var line = lines[0].EnumerateArray().ToArray();
        Assert.Equal(2, line.Length);

        var p0 = line[0].EnumerateArray().ToArray();
        var p1 = line[1].EnumerateArray().ToArray();
        Assert.Equal(-2.0, p0[0].GetDouble(), 6);
        Assert.Equal(4.0, p0[1].GetDouble(), 6);
        Assert.Equal(12.0, p1[0].GetDouble(), 6);
        Assert.Equal(6.0, p1[1].GetDouble(), 6);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeometryMultiPolygon_Serialization_Returns_GeoJson_With_CRS0()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Plants(1)/Zones");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var geo = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        Assert.Equal("MultiPolygon", geo.GetProperty("type").GetString());

        var polys = geo.GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Single(polys);

        var rings = polys[0].EnumerateArray().ToArray();
        Assert.Equal(3, rings.Length);

        var outer = rings[0].EnumerateArray().ToArray();
        var v0 = outer[0].EnumerateArray().ToArray();
        Assert.Equal(0.0, v0[0].GetDouble(), 6);
        Assert.Equal(10.0, v0[1].GetDouble(), 6);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeometryCollection_Serialization_Returns_GeoJson_With_CRS0()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Plants(1)/Layout");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var geo = (doc.RootElement.TryGetProperty("value", out var v) ? v : doc.RootElement);

        Assert.Equal("GeometryCollection", geo.GetProperty("type").GetString());

        var geoms = geo.GetProperty("geometries").EnumerateArray().ToArray();
        Assert.Equal(6, geoms.Length);

        Assert.Equal("Point", geoms[0].GetProperty("type").GetString());
        Assert.Equal("LineString", geoms[1].GetProperty("type").GetString());
        Assert.Equal("Polygon", geoms[2].GetProperty("type").GetString());
        Assert.Equal("MultiPoint", geoms[3].GetProperty("type").GetString());
        Assert.Equal("MultiLineString", geoms[4].GetProperty("type").GetString());
        Assert.Equal("MultiPolygon", geoms[5].GetProperty("type").GetString());

        var p = geoms[0].GetProperty("coordinates").EnumerateArray().ToArray();
        Assert.Equal(2.0, p[0].GetDouble(), 6);
        Assert.Equal(2.0, p[1].GetDouble(), 6);

        var crs = geo.GetProperty("crs");
        Assert.Equal("name", crs.GetProperty("type").GetString(), StringComparer.OrdinalIgnoreCase);
        Assert.Contains("0", crs.GetProperty("properties").GetProperty("name").GetString()!, StringComparison.OrdinalIgnoreCase);
    }
}
