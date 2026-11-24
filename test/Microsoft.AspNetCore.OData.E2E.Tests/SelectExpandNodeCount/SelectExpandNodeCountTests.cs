//-----------------------------------------------------------------------------
// <copyright file="SelectExpandNodeCountTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SelectExpandNodeCount;

public class SelectExpandNodeCountTests : WebApiTestBase<SelectExpandNodeCountTests>
{
    public SelectExpandNodeCountTests(WebApiTestFixture<SelectExpandNodeCountTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(
            typeof(LimitedProjectionItemsController),
            typeof(DefaultProjectionItemsController),
            typeof(LevelsProjectionItemsController));

        IEdmModel model = SelectExpandNodeCountEdmModel.GetEdmModel();
        services.AddControllers().AddOData(options =>
        {
            options.EnableQueryFeatures();
            options.AddRouteComponents("odata", model);
            options.AddRouteComponents("batch", model, new DefaultODataBatchHandler());
        });
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        app.UseODataBatching();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }

    // Each request below resolves to more than the endpoint's configured MaxSelectExpandNodeCount of 5.
    [Theory]
    [InlineData("$select=Id,P1,P2,P3,P4,P5")] // 6 select items
    [InlineData("$select=Id,P1,P2,P3,P4&$expand=Related")] // 5 select + 1 expand items
    [InlineData("$expand=Related($select=Id,P1,P2,P3,P4,P5)")] // 1 expand + 6 nested select items
    [InlineData("$compute=Id add 1 as C1,Id add 2 as C2,Id add 3 as C3,Id add 4 as C4,Id add 5 as C5,Id add 6 as C6&$select=C1,C2,C3,C4,C5,C6")] // 6 computed select items
    public async Task LimitedEndpoint_RequestExceedingMaxSelectExpandNodeCount_ReturnsBadRequest(string query)
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "odata/LimitedProjectionItems?" + query.Replace(" ", "%20");

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains(Error.Format(SRResources.MaxSelectExpandNodeCountExceeded, 5, "MaxSelectExpandNodeCount"), body);
    }

    // Each request below resolves to at most the endpoint's configured MaxSelectExpandNodeCount of 5.
    [Theory]
    [InlineData("")] // 0 items
    [InlineData("$select=Id")] // 1 select item
    [InlineData("$select=Id,P1,P2,P3,P4")] // 5 select items (exact boundary)
    [InlineData("$expand=Related")] // 1 expand item
    [InlineData("$select=Id,P1,P2,P3&$expand=Related")] // 4 select + 1 expand items
    [InlineData("$compute=Id add 1 as C1,Id add 2 as C2,Id add 3 as C3&$select=C1,C2,C3")] // 3 computed select items
    public async Task LimitedEndpoint_RequestWithinMaxSelectExpandNodeCount_ReturnsOk(string query)
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = string.IsNullOrEmpty(query)
            ? "odata/LimitedProjectionItems"
            : "odata/LimitedProjectionItems?" + query.Replace(" ", "%20");

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // The same requests that the limited endpoint rejects are accepted here, proving the limit is per-configuration
    // and the default (500) is not exceeded by these requests.
    [Theory]
    [InlineData("$select=Id,P1,P2,P3,P4,P5")] // 6 select items
    [InlineData("$expand=Related($select=Id,P1,P2,P3,P4,P5)")] // 1 expand + 6 nested select items
    [InlineData("$compute=Id add 1 as C1,Id add 2 as C2,Id add 3 as C3,Id add 4 as C4,Id add 5 as C5,Id add 6 as C6&$select=C1,C2,C3,C4,C5,C6")] // 6 computed select items
    public async Task DefaultEndpoint_RequestWithinDefaultMaxSelectExpandNodeCount_ReturnsOk(string query)
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "odata/DefaultProjectionItems?" + query.Replace(" ", "%20");

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // $levels replicates the expanded node and its nested $select once per level, so the endpoint's configured
    // MaxSelectExpandNodeCount of 12 is measured against the level-weighted tree: $expand=Related($select=<k items>;$levels=L)
    // resolves to L x (1 + k) items. Each request below exceeds 12 and is rejected before the projection is built.
    [Theory]
    [InlineData("$expand=Related($select=P1,P2,P3;$levels=4)")] // 4 x (1 + 3) = 16 items
    [InlineData("$expand=Related($select=P1,P2,P3,P4,P5;$levels=3)")] // 3 x (1 + 5) = 18 items
    [InlineData("$expand=Related($select=P1,P2,P3,P4,P5;$levels=4)")] // 4 x (1 + 5) = 24 items
    public async Task LevelsEndpoint_RequestExceedingMaxSelectExpandNodeCountViaLevels_ReturnsBadRequest(string query)
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "odata/LevelsProjectionItems?" + query.Replace(" ", "%20");

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains(Error.Format(SRResources.MaxSelectExpandNodeCountExceeded, 12, "MaxSelectExpandNodeCount"), body);
    }

    // Each request below resolves to at most the endpoint's configured MaxSelectExpandNodeCount of 12 once weighted by
    // $levels (L x (1 + k) items), so it is accepted. Includes the exact boundary (12) at two different level counts,
    // and $levels=1, which is not scaled. All $levels values stay within the endpoint's MaxExpansionDepth of 4 so that
    // the accepted projections serialize a shallow chain.
    [Theory]
    [InlineData("$expand=Related($select=P1,P2;$levels=4)")] // 4 x (1 + 2) = 12 items (exact boundary)
    [InlineData("$expand=Related($select=P1,P2,P3,P4,P5;$levels=2)")] // 2 x (1 + 5) = 12 items (exact boundary)
    [InlineData("$expand=Related($select=P1;$levels=4)")] // 4 x (1 + 1) = 8 items
    [InlineData("$expand=Related($select=P1,P2,P3;$levels=1)")] // 1 x (1 + 3) = 4 items ($levels=1 is not scaled)
    public async Task LevelsEndpoint_RequestWithinMaxSelectExpandNodeCountViaLevels_ReturnsOk(string query)
    {
        // Arrange
        HttpClient client = CreateClient();
        string requestUri = "odata/LevelsProjectionItems?" + query.Replace(" ", "%20");

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LevelsEndpoint_SameProjectionIsAcceptedWithoutLevelsAndRejectedWithLevels()
    {
        // Arrange: the same nested $select is within the limit on its own (1 + 3 = 4 items) but exceeds it once $levels
        // scales it (4 x (1 + 3) = 16 items). This isolates $levels as the sole cause of the rejection.
        HttpClient client = CreateClient();
        const string baseQuery = "odata/LevelsProjectionItems?$expand=Related($select=P1,P2,P3";

        // Act
        HttpResponseMessage withoutLevels = await client.GetAsync(baseQuery + ")");
        HttpResponseMessage withLevels = await client.GetAsync(baseQuery + ";$levels=4)");

        // Assert
        Assert.Equal(HttpStatusCode.OK, withoutLevels.StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest, withLevels.StatusCode);
        string body = await withLevels.Content.ReadAsStringAsync();
        Assert.Contains(Error.Format(SRResources.MaxSelectExpandNodeCountExceeded, 12, "MaxSelectExpandNodeCount"), body);
    }

    [Fact]
    public async Task BatchRequest_InnerQueryExceedingDefaultMaxSelectExpandNodeCount_InnerRequestReturnsBadRequest()
    {
        // Arrange
        // 300 computed select items at the root plus one expand and 300 nested computed select items => 601 items (> 500).
        // The root alone (300) is within the limit, so this also proves the items accumulate across expansion levels.
        HttpClient client = CreateClient();
        HttpRequestMessage request = CreateBatchGetRequest(BuildComputeQuery(300));

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // the batch envelope itself succeeds
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("400 Bad Request", body);
        Assert.Contains(Error.Format(SRResources.MaxSelectExpandNodeCountExceeded, 500, "MaxSelectExpandNodeCount"),  body);
    }

    [Fact]
    public async Task BatchRequest_InnerQueryWithinDefaultMaxSelectExpandNodeCount_InnerRequestReturnsOk()
    {
        // Arrange
        // 100 computed select items at the root plus one expand and 100 nested computed select items => 201 items (<= 500).
        HttpClient client = CreateClient();
        HttpRequestMessage request = CreateBatchGetRequest(BuildComputeQuery(100));

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("200 OK", body);
        Assert.DoesNotContain("400 Bad Request", body);
    }

    private static string BuildComputeQuery(int aliasCount)
    {
        string aliases = string.Join(",", Enumerable.Range(0, aliasCount).Select(i => $"Id add {i} as C{i}"));
        string select = string.Join(",", Enumerable.Range(0, aliasCount).Select(i => $"C{i}"));
        return $"$compute={aliases}&$select={select}&$expand=Related($compute={aliases};$select={select})";
    }

    private static HttpRequestMessage CreateBatchGetRequest(string innerQuery)
    {
        string boundary = "batch_" + Guid.NewGuid();
        string body =
            "--" + boundary + "\r\n" +
            "Content-Type: application/http\r\n" +
            "Content-Transfer-Encoding: binary\r\n" +
            "\r\n" +
            "GET DefaultProjectionItems?" + innerQuery.Replace(" ", "%20") + " HTTP/1.1\r\n" +
            "Accept: application/json\r\n" +
            "\r\n" +
            "--" + boundary + "--\r\n";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "batch/$batch");
        request.Content = new StringContent(body);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=" + boundary);
        return request;
    }
}
