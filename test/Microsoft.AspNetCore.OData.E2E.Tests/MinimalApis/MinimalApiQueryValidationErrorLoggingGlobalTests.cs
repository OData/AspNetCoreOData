//-----------------------------------------------------------------------------
// <copyright file="MinimalApiQueryValidationErrorLoggingGlobalTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

/// <summary>
/// End-to-end tests that verify configuring logging once through <c>AddOData</c> enables the diagnostic for
/// every minimal API endpoint, and that an individual endpoint still overrides the global value through
/// <see cref="ODataMiniOptions.SetQueryValidationErrorLogging(bool)"/>.
/// </summary>
public class MinimalApiQueryValidationErrorLoggingGlobalTests : IClassFixture<MinimalTestFixture<MinimalApiQueryValidationErrorLoggingGlobalTests>>
{
    private readonly HttpClient _client;

    public static readonly CapturingLoggerProvider LoggerProvider = new CapturingLoggerProvider();

    public MinimalApiQueryValidationErrorLoggingGlobalTests(MinimalTestFixture<MinimalApiQueryValidationErrorLoggingGlobalTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(LoggerProvider);
        });

        // Enable logging once for every minimal API endpoint.
        services.AddOData(o => o.SetQueryValidationErrorLogging(true));
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = MinimalEdmModel.GetEdmModel();

        // No per-endpoint logging configuration; the endpoint picks up the global value.
        app.MapGet("global/todos", () => GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model);

        // The endpoint overrides the global value and opts out.
        app.MapGet("optout/todos", () => GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(o => o.SetQueryValidationErrorLogging(false));
    }

    [Fact]
    public async Task GlobalLoggingEnabled_UnknownSelectProperty_WritesDiagnostic()
    {
        // An endpoint that does not configure logging picks up the global configuration.
        LoggerProvider.Clear();

        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/global/todos?$select=NoSuchProperty"));

        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("global/todos", entry.GetFieldValue("Endpoint"));
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
        Assert.NotNull(entry.Exception);
    }

    [Fact]
    public async Task GlobalLoggingEnabled_EndpointOptOut_WritesNoDiagnostic()
    {
        // An endpoint that sets SetQueryValidationErrorLogging(false) overrides the global enabled value.
        LoggerProvider.Clear();

        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/optout/todos?$select=NoSuchProperty"));

        Assert.Empty(GetQueryValidationEntries());
    }

    private static IEnumerable<MiniTodo> GetTodos()
    {
        return new List<MiniTodo>
        {
            new MiniTodo { Id = 1, Owner = "Sam", Title = "Clean House", IsDone = false },
        };
    }

    private static IReadOnlyList<CapturedLogEntry> GetQueryValidationEntries()
    {
        return LoggerProvider.Entries
            .Where(entry => entry.Category == typeof(ODataQueryEndpointFilter).FullName)
            .ToList();
    }
}
