//-----------------------------------------------------------------------------
// <copyright file="ETagTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags;

public class ETagTests : WebApiTestBase<ETagTests>
{
    public ETagTests(WebApiTestFixture<ETagTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(ETagSimpleThingsController), typeof(MetadataController));
        services.AddControllers().AddOData(options => options.Select().AddRouteComponents(GetEdmModel()));

        services.AddControllers(opt => opt.Filters.Add(new ETagActionFilterAttribute()));
    }

    private static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.EntitySet<ETagSimpleThing>("ETagSimpleThings");
        modelBuilder.ComplexType<ETagComplexThing>();

        return modelBuilder.GetEdmModel();
    }

    [Fact]
    public async Task TestETagInPayloadForEntityWithNestedComplexProperty()
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "ETagSimpleThings";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count);
        var resultAt0 = Assert.IsType<JObject>(result[0]);
        var resultAt1 = Assert.IsType<JObject>(result[1]);
        Assert.Equal("W/\"MA==\"", resultAt0.GetValue("@odata.etag"));
        Assert.Equal("W/\"MA==\"", resultAt1.GetValue("@odata.etag"));
    }

    [Fact]
    public async Task TestETagInHeaderForEntityWithNestedComplexProperty()
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "ETagSimpleThings(1)";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(response.Headers.ETag);
        var etagInHeader = response.Headers.ETag.ToString();
        Assert.Equal("W/\"MA==\"", etagInHeader);
    }

    [Fact]
    public async Task TestMetadataForEntityWithNestedComplexProperty()
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "$metadata";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("<EntitySet Name=\"ETagSimpleThings\" EntityType=\"Microsoft.AspNetCore.OData.E2E.Tests.ETags.ETagSimpleThing\">" +
            "<Annotation Term=\"Org.OData.Core.V1.OptimisticConcurrency\">" +
            "<Collection>" +
            "<PropertyPath>RowChangeNumber</PropertyPath>" +
            "</Collection>" +
            "</Annotation>" +
            "</EntitySet>",
            content);
    }
}
