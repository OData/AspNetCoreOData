//-----------------------------------------------------------------------------
// <copyright file="DollarSearchTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarSearch;

public class DollarSearchTests : WebApiTestBase<DollarSearchTests>
{
    private readonly ITestOutputHelper output;

    public DollarSearchTests(WebApiTestFixture<DollarSearchTests> fixture, ITestOutputHelper output)
        : base(fixture)
    {
        this.output = output;
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel edmModel = DollarSearchEdmModel.GetEdmModel();

        services.ConfigureControllers(typeof(ProductsController));

        services.AddControllers().AddOData(opt =>
            opt.Count()
            .Filter()
            .OrderBy()
            .Expand()
            .SetMaxTop(null)
            .Select()

            // route with ISearchBinder registered
            .AddRouteComponents("odata", edmModel, services => services.AddSingleton<ISearchBinder, DollarSearchBinder>())

            // route without ISearchBinder
            .AddRouteComponents("nonsearch", edmModel));
    }

    [Fact]
    public async Task QueryForProducts_IncludesDollarSearch_OnRouteWithoutISeachBinder()
    {
        // Arrange
        string queryUrl = $"nonsearch/Products?$search=office";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();
        HttpResponseMessage response;

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        JObject payloadBody = await response.Content.ReadAsObject<JObject>();

        int[] actualIds = GetIds(payloadBody);
        Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(actualIds));
    }

    [Theory]
    [InlineData("$search=office", new[] { 2, 5 })]
    [InlineData("$search=food", new[] { 1, 3 })]
    [InlineData("$search=device", new[] { 4, 6 })]
    [InlineData("$search=NOT device", new[] { 1, 2, 3, 5 })]
    [InlineData("$search=food OR device", new[] { 1,3, 4, 6 })]
    [InlineData("$search=food AND device", new int[] { })]
    [InlineData("$search=unknown", new int[] { })]
    public async Task QueryForProducts_IncludesDollarSearch_OnCategory(string query, int[] ids)
    {
        // Arrange
        string queryUrl = $"odata/Products?{query}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();
        HttpResponseMessage response;

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        JObject payloadBody = await response.Content.ReadAsObject<JObject>();

        int[] actualIds = GetIds(payloadBody);
        Assert.True(ids.SequenceEqual(actualIds));
    }

    [Theory]
    [InlineData("$search=white", new[] { 1, 5 })]
    [InlineData("$search=Blue", new[] { 2 })]
    [InlineData("$search=rED", new[] { 4, 6 })]
    [InlineData("$search=Brown", new int[] { 3 })]
    [InlineData("$search=green", new int[] { })]
    [InlineData("$search=NOT blue", new int[] { 1, 3, 4, 5, 6 })]
    public async Task QueryForProducts_IncludesDollarSearch_OnEnumColor(string query, int[] ids)
    {
        // Arrange
        string queryUrl = $"odata/Products?{query}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();
        HttpResponseMessage response;

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        JObject payloadBody = await response.Content.ReadAsObject<JObject>();

        int[] actualIds = GetIds(payloadBody);
        Assert.True(ids.SequenceEqual(actualIds));
    }

    [Theory]
    [InlineData("$search=food AND White", new[] { 1 })]
    [InlineData("$search=(office OR food) AND White", new[] { 1, 5 })]
    [InlineData("$search=(office OR food) AND NOT white", new[] { 2, 3 })]
    public async Task QueryForProducts_IncludesDollarSearch_OnCategoryAndEnumColor(string query, int[] ids)
    {
        // Arrange
        string queryUrl = $"odata/Products?{query}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();
        HttpResponseMessage response;

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        JObject payloadBody = await response.Content.ReadAsObject<JObject>();

        int[] actualIds = GetIds(payloadBody);
        Assert.True(ids.SequenceEqual(actualIds));
    }

    [Fact]
    public async Task QueryForProducts_IncludesDollarSearch_WithOtherQueryOptions()
    {
        // Arrange
        string queryUrl = $"odata/Products?$search=food OR office&$filter=Id lt 4&$select=Name&$expand=Category";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();
        HttpResponseMessage response;

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        string payloadBody = await response.Content.ReadAsStringAsync();

        Assert.Equal("{\"value\":[" +
            "{\"Name\":\"Sugar\",\"Category\":{\"Id\":1,\"Name\":\"Food\"}}," +
            "{\"Name\":\"Pencil\",\"Category\":{\"Id\":2,\"Name\":\"Office\"}}," +
            "{\"Name\":\"Coffee\",\"Category\":{\"Id\":1,\"Name\":\"Food\"}}" +
          "]}", payloadBody);
    }

    [Fact]
    public async Task QueryForProductsForNestedNavigationProperty_WithoutDollarSearchBinder()
    {
        // Arrange
        string queryUrl = $"odata/Products/1?$expand=Tags($select=Name)&$select=Id";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();
        HttpResponseMessage response;

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        string payloadBody = await response.Content.ReadAsStringAsync();

        Assert.Equal("{\"Id\":1,\"Tags\":[{\"Name\":\"Telemetry\"},{\"Name\":\"SDK\"},{\"Name\":\"Deprecated\"}]}", payloadBody);
    }

    [Theory]
    [InlineData("$expand=Tags($search=SDK)", new[] { 3 })]
    [InlineData("$expand=Tags($search=NOT SDK)", new[] { 1, 4 })]
    public async Task QueryForProducts_IncludesDollarSearchOnNavigation_OnName(string query, int[] ids)
    {
        // Arrange
        string queryUrl = $"odata/Products/1?{query}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        HttpClient client = CreateClient();
        HttpResponseMessage response;

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        JObject payloadBody = await response.Content.ReadAsObject<JObject>();

        int[] actualIds = GetIds(payloadBody, "Tags");
        Assert.True(ids.SequenceEqual(actualIds));
    }

    private static int[] GetIds(JObject payload, string propertyName = "value")
    {
        JArray value = payload[propertyName] as JArray;
        Assert.NotNull(value);

        int[] ids = new int[value.Count()];
        for (int i = 0; i < value.Count(); i++)
        {
            JObject item = value[i] as JObject;
            ids[i] = (int)item["Id"];
        }

        return ids;
    }
}
