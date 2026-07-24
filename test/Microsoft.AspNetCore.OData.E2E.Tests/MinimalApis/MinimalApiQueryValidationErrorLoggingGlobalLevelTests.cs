//-----------------------------------------------------------------------------
// <copyright file="MinimalApiQueryValidationErrorLoggingGlobalLevelTests.cs" company=".NET Foundation">
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
/// End-to-end tests that verify configuring the level once through <c>AddOData</c> with
/// <see cref="ODataMiniOptions.SetQueryValidationErrorLogLevel(LogLevel)"/> applies to every minimal API
/// endpoint that records query validation diagnostics.
/// </summary>
public class MinimalApiQueryValidationErrorLoggingGlobalLevelTests : IClassFixture<MinimalTestFixture<MinimalApiQueryValidationErrorLoggingGlobalLevelTests>>
{
    private readonly HttpClient _client;

    public static readonly CapturingLoggerProvider LoggerProvider = new CapturingLoggerProvider();

    public MinimalApiQueryValidationErrorLoggingGlobalLevelTests(MinimalTestFixture<MinimalApiQueryValidationErrorLoggingGlobalLevelTests> factory)
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

        // Enable logging and set the level once for every minimal API endpoint.
        services.AddOData(o => o.SetQueryValidationErrorLogging(true).SetQueryValidationErrorLogLevel(LogLevel.Error));
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = MinimalEdmModel.GetEdmModel();

        // No per-endpoint configuration; the endpoint picks up the global level.
        app.MapGet("globallevel/todos", () => GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model);
    }

    [Fact]
    public async Task GlobalLogLevel_UnknownSelectProperty_WritesDiagnosticAtGlobalLevel()
    {
        // The endpoint records the diagnostic at the level configured once through AddOData.
        LoggerProvider.Clear();

        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/globallevel/todos?$select=NoSuchProperty"));

        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("globallevel/todos", entry.GetFieldValue("Endpoint"));
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
        Assert.NotNull(entry.Exception);
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
