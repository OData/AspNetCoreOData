//-----------------------------------------------------------------------------
// <copyright file="MinimalAPIMatchesPatternTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

public class MinimalAPIMatchesPatternTest : IClassFixture<MinimalTestFixture<MinimalAPIMatchesPatternTest>>
{
    private HttpClient _client;

    public MinimalAPIMatchesPatternTest(MinimalTestFixture<MinimalAPIMatchesPatternTest> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMiniTodoTaskRepository, MiniTodoTaskInMemoryRepository>();
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = MinimalEdmModel.GetEdmModel();

        // Relies on the default matchesPattern time span. A page size is set so the collection is
        // materialized during query execution; the limit is larger than the data set.
        app.MapGet("matchespattern/todos", (IMiniTodoTaskRepository db) => db.GetTodos())
            .AddODataQueryEndpointFilter(querySetup: s => s.PageSize = 100)
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(opt => opt.EnableAll().SetCaseInsensitive(true));

        // Opts out of the matchesPattern time span while paging the collection.
        app.MapGet("matchespattern/optout/todos", (IMiniTodoTaskRepository db) => db.GetTodos())
            .AddODataQueryEndpointFilter(querySetup: s =>
            {
                s.PageSize = 100;
                s.MatchesPatternTimeout = null;
            })
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(opt => opt.EnableAll().SetCaseInsensitive(true));

        // A page-less endpoint (no page size) returns an un-materialized IQueryable, so the bounded
        // matchesPattern evaluation runs during response serialization rather than during query execution.
        app.MapGet("matchespattern/pageless/todos", (IMiniTodoTaskRepository db) => db.GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(opt => opt.EnableAll().SetCaseInsensitive(true));
    }

    [Fact]
    public async Task DefaultTimeout_MatchesPattern_ReturnsMatchingTodos()
    {
        // Arrange & Act - a benign pattern returns the matching item under the default time span.
        var response = await _client.GetAsync("/matchespattern/todos?$filter=matchesPattern(Owner,'^Pe')");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"Owner\":\"Peter\"", content);
        Assert.DoesNotContain("\"Owner\":\"John\"", content);
    }

    [Fact]
    public async Task DefaultTimeout_MatchesPatternRequiringExtensiveBacktracking_IsBounded()
    {
        // Arrange - both arguments are literals, so the evaluation is independent of the stored data.
        // The '+' characters are percent-encoded so they survive as literals in the query string.
        var haystack = new string('a', 40) + "X";
        var queryUrl = $"/matchespattern/todos?$filter=matchesPattern('{haystack}','(a%2B)%2B$')";

        // Act
        var response = await _client.GetAsync(queryUrl);
        var content = await response.Content.ReadAsStringAsync();

        // Assert - the request completes with a Bad Request once the default time span elapses.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Assert - the body carries the same SerializableError payload the controller pipeline
        // produces (EnableQueryAttribute.CreateErrorResponse): a top-level message plus exception detail.
        Assert.False(string.IsNullOrWhiteSpace(content));
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        var message = root.EnumerateObject()
            .Where(p => string.Equals(p.Name, "message", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Value.GetString())
            .FirstOrDefault();
        Assert.False(string.IsNullOrEmpty(message), "Expected a non-empty 'message' in the 400 body.");

        var exceptionType = root.EnumerateObject()
            .Where(p => string.Equals(p.Name, "type", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Value.GetString())
            .FirstOrDefault();
        Assert.Equal(typeof(RegexMatchTimeoutException).FullName, exceptionType);
    }

    [Fact]
    public async Task OptedOut_MatchesPattern_ReturnsMatchingTodos()
    {
        // Arrange & Act - opting out keeps benign patterns working end to end.
        var response = await _client.GetAsync("/matchespattern/optout/todos?$filter=matchesPattern(Owner,'^Pe')");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"Owner\":\"Peter\"", content);
        Assert.DoesNotContain("\"Owner\":\"John\"", content);
    }

    [Fact]
    public async Task DefaultTimeout_PagelessFilter_MatchesPatternRequiringExtensiveBacktracking_SurfacesDuringSerialization()
    {
        // Arrange - the page-less endpoint (no page size) returns an un-materialized IQueryable, so the bounded
        // matchesPattern evaluation runs during response serialization. The evaluation is still bounded by the
        // default time span, but because the response has already begun the aborted evaluation breaks the response
        // stream instead of completing as 400 (Bad Request). This documents the current behavior of the page-less
        // path. Both arguments are literals, so the evaluation is independent of the stored data; the '+'
        // characters are percent-encoded so they survive as literals in the query string.
        var haystack = new string('a', 40) + "X";
        var queryUrl = $"/matchespattern/pageless/todos?$filter=matchesPattern('{haystack}','(a%2B)%2B$')";

        // Act & Assert - reading the response surfaces the bounded, aborted evaluation as a broken stream.
        await Assert.ThrowsAsync<HttpRequestException>(() => _client.GetAsync(queryUrl));
    }

    [Fact]
    public async Task DefaultTimeout_PagelessFilter_MatchesPattern_ReturnsMatchingTodos()
    {
        // Arrange & Act - the same page-less endpoint returns the matching item for a benign pattern, confirming
        // the streamed page-less path is healthy and only the degenerate pattern is affected.
        var response = await _client.GetAsync("/matchespattern/pageless/todos?$filter=matchesPattern(Owner,'^Pe')");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"Owner\":\"Peter\"", content);
        Assert.DoesNotContain("\"Owner\":\"John\"", content);
    }
}
