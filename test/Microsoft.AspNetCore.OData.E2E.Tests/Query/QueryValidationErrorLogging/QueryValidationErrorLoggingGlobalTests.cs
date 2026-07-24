//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLoggingGlobalTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.QueryValidationErrorLogging;

/// <summary>
/// End-to-end tests that verify <see cref="ODataOptions.EnableQueryValidationErrorLogging"/> configures the
/// diagnostic once for every <see cref="EnableQueryAttribute"/> action, and that an individual attribute
/// still overrides the global value.
/// </summary>
public class QueryValidationErrorLoggingGlobalTests : WebODataTestBase<QueryValidationErrorLoggingGlobalTests.Startup>
{
    public class Startup : TestStartupBase
    {
        public static readonly CapturingLoggerProvider LoggerProvider = new CapturingLoggerProvider();

        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(LoggingCustomersController), typeof(PlainCustomersController), typeof(OptOutCustomersController));

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddProvider(LoggerProvider);
            });

            IEdmModel model = QueryValidationErrorLoggingEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options =>
            {
                options.EnableQueryValidationErrorLogging = true; // Enable once for every [EnableQuery] action.
                options.AddRouteComponents("odata", model).Select().Expand();
            });
        }
    }

    public QueryValidationErrorLoggingGlobalTests(WebODataTestFixture<Startup> fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task PlainEntitySet_UnknownSelectProperty_WritesDiagnostic_WhenConfiguredGlobally()
    {
        // A plain [EnableQuery] attribute that does not set the flag picks up the global configuration.
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/PlainCustomers?$select=NoSuchProperty"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("PlainCustomers", entry.GetFieldValue("Endpoint"));
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
        Assert.NotNull(entry.Exception);
    }

    [Fact]
    public async Task OptOutEntitySet_UnknownSelectProperty_WritesNoDiagnostic_WhenConfiguredGlobally()
    {
        // An attribute that sets EnableQueryValidationErrorLogging = false overrides the global enabled value.
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/OptOutCustomers?$select=NoSuchProperty"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetQueryValidationEntries());
    }

    [Fact]
    public async Task AttributeEnabledEntitySet_UnknownSelectProperty_WritesDiagnostic_WhenConfiguredGlobally()
    {
        // An attribute that sets EnableQueryValidationErrorLogging = true logs, consistent with the global value.
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?$select=NoSuchProperty"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
    }

    private static HttpRequestMessage CreateGet(string url)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        return request;
    }

    private static IReadOnlyList<CapturedLogEntry> GetQueryValidationEntries()
    {
        return Startup.LoggerProvider.Entries
            .Where(entry => entry.Category == typeof(EnableQueryAttribute).FullName)
            .ToList();
    }
}
