using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.LegacyFilter;

public class LegacyFilterTests : WebApiTestBase<LegacyFilterTests>
{
    public LegacyFilterTests(WebApiTestFixture<LegacyFilterTests> fixture) : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var model = LegacyFilterEdmModel.GetEdmModel();
        Spatial.SpatialImplementation.CurrentImplementation.Operations = new LegacySpatialOperations();

        services.ConfigureControllers(
            typeof(SitesController));

        services.AddControllers().AddOData(
            options =>
            options.EnableQueryFeatures().AddRouteComponents(
                routePrefix: string.Empty,
                model: model,
                configureServices: (nestedServices) => nestedServices.AddSingleton<IFilterBinder, LegacyFilterBinder>()));
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
    public async Task Test2Async()
    {
        // Arrange
        var queryUrl = $"/Sites?$filter=geo.distance(Location,geometry'POINT(-122.123889 47.669444)') gt 240.15";
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
    }
}
