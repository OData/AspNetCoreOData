//-----------------------------------------------------------------------------
// <copyright file="MinimalApiQueryValidationErrorLoggingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
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
/// End-to-end tests that verify diagnostics are recorded through the real minimal API request pipeline when a
/// query fails validation and logging is enabled per endpoint through
/// <see cref="ODataMiniOptions.SetQueryValidationErrorLogging(bool)"/>. Logging is off by default; configuring it
/// once for all endpoints through <c>AddOData</c> is covered by
/// <see cref="MinimalApiQueryValidationErrorLoggingGlobalTests"/>.
/// </summary>
public class MinimalApiQueryValidationErrorLoggingTests : IClassFixture<MinimalTestFixture<MinimalApiQueryValidationErrorLoggingTests>>
{
    private readonly HttpClient _client;

    public static readonly CapturingLoggerProvider LoggerProvider = new CapturingLoggerProvider();

    public MinimalApiQueryValidationErrorLoggingTests(MinimalTestFixture<MinimalApiQueryValidationErrorLoggingTests> factory)
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
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = MinimalEdmModel.GetEdmModel();

        // Logging enabled on the endpoint; the default level is Warning.
        app.MapGet("logging/todos", () => GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(o => o.Select().Expand().SetQueryValidationErrorLogging(true));

        // Logging left at its default (off) on the endpoint.
        app.MapGet("plain/todos", () => GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model);
    }

    [Fact]
    public async Task LoggingEnabledEndpoint_UnknownSelectProperty_WritesDiagnostic()
    {
        // Arrange
        LoggerProvider.Clear();

        // Act
        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/logging/todos?$select=NoSuchProperty"));

        // Assert
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("logging/todos", entry.GetFieldValue("Endpoint"));
        Assert.Contains("MiniTodo", entry.GetFieldValue("QueryType"));
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
        Assert.NotNull(entry.Exception);
        Assert.Contains("NoSuchProperty", entry.Exception.Message);
        Assert.Contains(" at ", entry.Exception.ToString());
    }

    [Fact]
    public async Task LoggingEnabledEndpoint_UnknownExpandProperty_WritesDiagnostic()
    {
        // Arrange
        LoggerProvider.Clear();

        // Act
        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/logging/todos?$expand=NoSuchNavigation"));

        // Assert
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal("$expand=NoSuchNavigation", entry.GetFieldValue("QueryOptions"));
        Assert.NotNull(entry.Exception);
        Assert.Contains("NoSuchNavigation", entry.Exception.Message);
    }

    [Fact]
    public async Task LoggingEnabledEndpoint_UnknownSelectPropertyWithoutDollarPrefix_WritesDiagnostic()
    {
        // The '$' prefix on system query options is optional and enabled by default, so the attempted
        // select set must be captured for the no-'$' form as well.
        LoggerProvider.Clear();

        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/logging/todos?select=NoSuchProperty"));

        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.NotNull(entry.Exception);
        Assert.Contains("NoSuchProperty", entry.Exception.Message);
    }

    [Fact]
    public async Task PlainEndpoint_UnknownSelectProperty_WritesNoDiagnosticByDefault()
    {
        // An endpoint that does not enable logging writes nothing, even though the query still fails validation.
        LoggerProvider.Clear();

        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/plain/todos?$select=NoSuchProperty"));

        Assert.Empty(GetQueryValidationEntries());
    }

    [Fact]
    public async Task LoggingEnabledEndpoint_ValidSelectProperty_WritesNoDiagnostic()
    {
        // Arrange
        LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/logging/todos?$select=Owner");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(GetQueryValidationEntries());
    }

    [Fact]
    public async Task LoggingEnabledEndpoint_SelectiveAcrossSequentialRequests()
    {
        // A bad request writes exactly one diagnostic.
        LoggerProvider.Clear();
        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/logging/todos?$select=NoSuchProperty"));
        Assert.Single(GetQueryValidationEntries());

        // A subsequent valid request writes no diagnostic.
        LoggerProvider.Clear();
        HttpResponseMessage validResponse = await _client.GetAsync("/logging/todos?$select=Owner");
        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);
        Assert.Empty(GetQueryValidationEntries());

        // A repeated bad request writes the diagnostic again.
        LoggerProvider.Clear();
        await Assert.ThrowsAsync<ODataException>(() => _client.GetAsync("/logging/todos?$select=NoSuchProperty"));
        Assert.Single(GetQueryValidationEntries());
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
