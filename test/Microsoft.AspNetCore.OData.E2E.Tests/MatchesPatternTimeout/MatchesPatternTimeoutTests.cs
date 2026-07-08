//-----------------------------------------------------------------------------
// <copyright file="MatchesPatternTimeoutTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MatchesPatternTimeout;

public class MatchesPatternTimeoutTests : WebApiTestBase<MatchesPatternTimeoutTests>
{
    public MatchesPatternTimeoutTests(WebApiTestFixture<MatchesPatternTimeoutTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel model = MatchesPatternTimeoutEdmModel.GetEdmModel();

        services.ConfigureControllers(
            typeof(ProductsController),
            typeof(BoundedProductsController),
            typeof(DefaultBoundedProductsController),
            typeof(AttributeBoundedProductsController));

        services.AddControllers().AddOData(opt =>
            opt.Filter().OrderBy().Select().Count().AddRouteComponents("odata", model));
    }

    [Fact]
    public async Task DefaultSettings_MatchesPattern_ReturnsMatchingProducts()
    {
        // Arrange
        var queryUrl = "odata/Products?$filter=matchesPattern(Name,'^Al')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count); // Alpha and Alabama start with "Al"
        Assert.Equal(1, (result[0] as JObject)["Id"]);
        Assert.Equal(4, (result[1] as JObject)["Id"]);
    }

    [Fact]
    public async Task ConfiguredTimeout_MatchesPattern_ReturnsSameMatchingProducts()
    {
        // Arrange
        var queryUrl = "odata/BoundedProducts?$filter=matchesPattern(Name,'^Al')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert - the four argument overload produces identical results for a benign pattern.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count);
        Assert.Equal(1, (result[0] as JObject)["Id"]);
        Assert.Equal(4, (result[1] as JObject)["Id"]);
    }

    [Fact]
    public async Task ConfiguredTimeout_MatchesPatternWithNoMatches_ReturnsEmpty()
    {
        // Arrange
        var queryUrl = "odata/BoundedProducts?$filter=matchesPattern(Name,'^Zzz')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Empty(result);
    }

    [Fact]
    public async Task ConfiguredTimeout_MatchesPatternRequiringExtensiveBacktracking_IsBounded()
    {
        // Arrange - evaluation of this pattern against the data set is bounded by the configured time span.
        // The '+' characters are percent-encoded so they survive as literals in the query string.
        var queryUrl = "odata/BoundedProducts?$filter=matchesPattern(Name,'(a%2B)%2B$')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert - the request completes with a Bad Request once the configured time span elapses.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DefaultTimeout_MatchesPatternRequiringExtensiveBacktracking_IsBounded()
    {
        // Arrange - the set relies on the default matchesPattern time span (no explicit configuration) and
        // only sets a page size, so the collection is materialized during query execution. The '+' characters
        // are percent-encoded so they survive as literals in the query string.
        var queryUrl = "odata/DefaultBoundedProducts?$filter=matchesPattern(Name,'(a%2B)%2B$')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert - the request completes with a Bad Request once the default time span elapses.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DefaultTimeout_MatchesPattern_ReturnsMatchingProducts()
    {
        // Arrange - a benign pattern returns the full result under the default time span.
        var queryUrl = "odata/DefaultBoundedProducts?$filter=matchesPattern(Name,'^Al')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count); // Alpha and Alabama start with "Al"
        Assert.Equal(1, (result[0] as JObject)["Id"]);
        Assert.Equal(4, (result[1] as JObject)["Id"]);
    }

    [Fact]
    public async Task ConfiguredTimeout_DoesNotAffectOtherFunctions()
    {
        // Arrange
        var queryUrl = "odata/BoundedProducts?$filter=startswith(Name,'Be')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert - the time span only affects matchesPattern, so other functions bind and run unchanged.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        var single = Assert.Single(result); // Beta
        Assert.Equal(2, (single as JObject)["Id"]);
    }

    [Fact]
    public async Task AttributeConfiguredTimeout_MatchesPatternRequiringExtensiveBacktracking_IsBounded()
    {
        // Arrange - the set is configured through [EnableQuery(MatchesPatternTimeoutMilliseconds = 100)] on the
        // attribute itself. The '+' characters are percent-encoded so they survive as literals in the query string.
        var queryUrl = "odata/AttributeBoundedProducts?$filter=matchesPattern(Name,'(a%2B)%2B$')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert - the attribute-configured time span bounds the evaluation and completes as Bad Request.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AttributeConfiguredTimeout_MatchesPattern_ReturnsMatchingProducts()
    {
        // Arrange - a benign pattern returns the full result under the attribute-configured time span.
        var queryUrl = "odata/AttributeBoundedProducts?$filter=matchesPattern(Name,'^Al')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count); // Alpha and Alabama start with "Al"
        Assert.Equal(1, (result[0] as JObject)["Id"]);
        Assert.Equal(4, (result[1] as JObject)["Id"]);
    }
}
