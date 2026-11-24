//-----------------------------------------------------------------------------
// <copyright file="MinimalApiSelectExpandNodeCountTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

/// <summary>
/// End-to-end tests that verify the global projection-item limit configured once through <c>AddOData</c> with
/// <see cref="ODataMiniOptions.SetMaxSelectExpandNodeCount(int)"/> applies to every minimal API endpoint, and that
/// an individual endpoint still overrides the global value through the same method on a per-endpoint
/// <c>WithODataOptions</c> call.
/// </summary>
public class MinimalApiSelectExpandNodeCountTests : IClassFixture<MinimalTestFixture<MinimalApiSelectExpandNodeCountTests>>
{
    private readonly HttpClient _client;

    public MinimalApiSelectExpandNodeCountTests(MinimalTestFixture<MinimalApiSelectExpandNodeCountTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        // Configure the global projection-item limit once for every minimal API endpoint.
        services.AddOData(o => o.SetMaxSelectExpandNodeCount(3));
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = MinimalEdmModel.GetEdmModel();

        // No per-endpoint limit; the endpoint picks up the global value (3).
        app.MapGet("global/todos", () => GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(o => o.Select().Expand());

        // The endpoint overrides the global value with a larger per-endpoint limit (10).
        app.MapGet("override/todos", () => GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(o => o.Select().Expand().SetMaxSelectExpandNodeCount(10));
    }

    [Fact]
    public async Task GlobalLimit_ExceedingProjectionItems_Throws()
    {
        // The endpoint has no per-endpoint limit, so 4 $select items exceed the global limit (3).
        ODataException exception = await Assert.ThrowsAsync<ODataException>(
            () => _client.GetAsync("/global/todos?$select=Id,Owner,Title,IsDone"));

        Assert.Contains("MaxSelectExpandNodeCount", exception.Message);
    }

    [Fact]
    public async Task GlobalLimit_WithinProjectionItems_Succeeds()
    {
        // 3 $select items are within the global limit (3).
        HttpResponseMessage response = await _client.GetAsync("/global/todos?$select=Id,Owner,Title");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointLimit_OverridesGlobal_AllowsMoreProjectionItems()
    {
        // The endpoint sets its own limit (10), so 4 $select items that exceed the global limit (3) still succeed.
        HttpResponseMessage response = await _client.GetAsync("/override/todos?$select=Id,Owner,Title,IsDone");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static IEnumerable<MiniTodo> GetTodos()
    {
        return new List<MiniTodo>
        {
            new MiniTodo { Id = 1, Owner = "Sam", Title = "Clean House", IsDone = false },
        };
    }
}
