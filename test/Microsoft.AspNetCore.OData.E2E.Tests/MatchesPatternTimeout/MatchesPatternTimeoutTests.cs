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
            typeof(AttributeBoundedProductsController),
            typeof(ClampedProductsController));

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
    public async Task DefaultTimeout_PagelessEnableQuery_MatchesPatternRequiringExtensiveBacktracking_SurfacesDuringSerialization()
    {
        // Arrange - a plain [EnableQuery] action with no page size returns an un-materialized IQueryable, so the
        // bounded matchesPattern evaluation runs during response serialization rather than during query execution.
        // The evaluation is still bounded by the default time span, but because the response has already begun the
        // aborted evaluation breaks the response stream instead of completing as 400 (Bad Request). This documents
        // the current behavior of the page-less path; the paged, $count and single-result paths complete as 400.
        // The '+' characters are percent-encoded so they survive as literals in the query string.
        var queryUrl = "odata/Products?$filter=matchesPattern(Name,'(a%2B)%2B$')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act & Assert - reading the response surfaces the bounded, aborted evaluation as a broken stream.
        await Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync(request));
    }

    [Fact]
    public async Task DefaultSettings_PagelessEnableQueryWithoutMatchesPattern_ReturnsAllProducts()
    {
        // Arrange - the same page-less action returns the full collection when no matchesPattern predicate is
        // used, confirming the streamed page-less path is healthy and only the degenerate pattern is affected.
        var queryUrl = "odata/Products";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(5, result.Count);
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

    [Fact]
    public async Task TimeoutAboveRegexMaximum_MatchesPattern_IsClampedAndStillExecutes()
    {
        // Arrange - the set is configured with a matchesPattern time span (30 days) larger than the maximum
        // Regex accepts. Without clamping, binding this constant into Regex.IsMatch throws
        // ArgumentOutOfRangeException for every matchesPattern query (surfacing as 400 on the controller path).
        // The setter clamps the span to Regex's maximum, so a benign pattern executes and returns results.
        var queryUrl = "odata/ClampedProducts?$filter=matchesPattern(Name,'^Al')";
        var request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert - the clamped span is a valid Regex timeout, so the query succeeds instead of failing.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = (await response.Content.ReadAsObject<JObject>())["value"] as JArray;
        Assert.Equal(2, result.Count); // Alpha and Alabama start with "Al"
        Assert.Equal(1, (result[0] as JObject)["Id"]);
        Assert.Equal(4, (result[1] as JObject)["Id"]);
    }
}
