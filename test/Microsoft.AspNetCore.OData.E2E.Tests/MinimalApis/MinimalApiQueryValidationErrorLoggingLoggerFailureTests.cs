//-----------------------------------------------------------------------------
// <copyright file="MinimalApiQueryValidationErrorLoggingLoggerFailureTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
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
/// End-to-end test that verifies a logging provider which throws while recording the diagnostic never changes
/// the outcome of a failed query on the minimal API path: the original validation exception is still the one
/// that propagates, not the exception raised by the logger. The provider throws only for the diagnostic's own
/// category, so the rest of the framework's logging is unaffected.
/// </summary>
public class MinimalApiQueryValidationErrorLoggingLoggerFailureTests : IClassFixture<MinimalTestFixture<MinimalApiQueryValidationErrorLoggingLoggerFailureTests>>
{
    private readonly HttpClient _client;

    public MinimalApiQueryValidationErrorLoggingLoggerFailureTests(MinimalTestFixture<MinimalApiQueryValidationErrorLoggingLoggerFailureTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(new ThrowingLoggerProvider(typeof(ODataQueryEndpointFilter).FullName));
        });
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = MinimalEdmModel.GetEdmModel();

        // Logging is enabled on the endpoint, but the only registered logging provider throws when it writes the
        // diagnostic. The default level is Warning.
        app.MapGet("logging/todos", () => GetTodos())
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(o => o.Select().Expand().SetQueryValidationErrorLogging(true));
    }

    [Fact]
    public async Task LoggingEnabledEndpoint_WhenLoggerThrows_OriginalValidationExceptionStillPropagates()
    {
        // The logger throws while writing the diagnostic, but that must not replace the validation failure:
        // the original ODataException is still the exception that surfaces from the request.
        ODataException exception = await Assert.ThrowsAsync<ODataException>(
            () => _client.GetAsync("/logging/todos?$select=NoSuchProperty"));

        Assert.Contains("NoSuchProperty", exception.Message);
    }

    private static IEnumerable<MiniTodo> GetTodos()
    {
        return new List<MiniTodo>
        {
            new MiniTodo { Id = 1, Owner = "Sam", Title = "Clean House", IsDone = false },
        };
    }
}
