//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLoggingTests.cs" company=".NET Foundation">
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
/// End-to-end tests that verify diagnostics are recorded through the real request pipeline when a query
/// fails validation and <see cref="EnableQueryAttribute.EnableQueryValidationErrorLogging"/> is enabled on the
/// attribute. Logging is off by default; configuring it once for all actions through
/// <see cref="ODataOptions.EnableQueryValidationErrorLogging"/> is covered by
/// <see cref="QueryValidationErrorLoggingGlobalTests"/>.
/// </summary>
public class QueryValidationErrorLoggingTests : WebODataTestBase<QueryValidationErrorLoggingTests.Startup>
{
    public class Startup : TestStartupBase
    {
        public static readonly CapturingLoggerProvider LoggerProvider = new CapturingLoggerProvider();

        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(LoggingCustomersController), typeof(PlainCustomersController), typeof(PostActionLoggingCustomersController), typeof(OptOutCustomersController));

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddProvider(LoggerProvider);
            });

            IEdmModel model = QueryValidationErrorLoggingEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options => options.AddRouteComponents("odata", model).Select().Expand());
        }
    }

    public QueryValidationErrorLoggingTests(WebODataTestFixture<Startup> fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task LoggingEnabledEntitySet_UnknownSelectProperty_WritesDiagnostic()
    {
        // Arrange
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?$select=NoSuchProperty"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("LoggingCustomers", entry.GetFieldValue("Endpoint"));
        Assert.Contains("LoggingCustomer", entry.GetFieldValue("QueryType"));
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
        Assert.NotNull(entry.Exception);
        Assert.Contains("NoSuchProperty", entry.Exception.Message);
        Assert.Contains(" at ", entry.Exception.ToString());
    }

    [Fact]
    public async Task LoggingEnabledEntitySet_UnknownExpandProperty_WritesDiagnostic()
    {
        // Arrange
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?$expand=NoSuchNavigation"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal("$expand=NoSuchNavigation", entry.GetFieldValue("QueryOptions"));
        Assert.NotNull(entry.Exception);
        Assert.Contains("NoSuchNavigation", entry.Exception.Message);
    }

    [Fact]
    public async Task LoggingEnabledEntitySet_UnknownSelectPropertyWithoutDollarPrefix_WritesDiagnostic()
    {
        // The '$' prefix on system query options is optional and enabled by default, so the attempted
        // select set must be captured for the no-'$' form as well.
        Startup.LoggerProvider.Clear();

        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?select=NoSuchProperty"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.NotNull(entry.Exception);
        Assert.Contains("NoSuchProperty", entry.Exception.Message);
    }

    [Fact]
    public async Task LoggingEnabledPostActionValidation_UnknownSelectProperty_CapturesElementTypeAndAttemptedSet()
    {
        // A controller whose result type is only known after the action runs validates the query on the
        // post-action path; the diagnostic must still report the element type and the attempted select set.
        Startup.LoggerProvider.Clear();

        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("postaction/customers?$select=NoSuchProperty"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Contains("LoggingCustomer", entry.GetFieldValue("QueryType"));
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.NotNull(entry.Exception);
        Assert.Contains("NoSuchProperty", entry.Exception.Message);
    }

    [Fact]
    public async Task PlainEntitySet_UnknownSelectProperty_WritesNoDiagnosticByDefault()
    {
        // A plain [EnableQuery] attribute writes nothing because logging is off by default and no global
        // configuration enables it.
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/PlainCustomers?$select=NoSuchProperty"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetQueryValidationEntries());
    }

    [Fact]
    public async Task OptOutEntitySet_UnknownSelectProperty_WritesNoDiagnostic()
    {
        // A controller that explicitly opts out with [EnableQuery(EnableQueryValidationErrorLogging = false)] writes nothing.
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/OptOutCustomers?$select=NoSuchProperty"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetQueryValidationEntries());
    }

    [Fact]
    public async Task LoggingEnabledEntitySet_ValidSelectProperty_WritesNoDiagnostic()
    {
        // Arrange
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?$select=Name"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(GetQueryValidationEntries());
    }

    [Fact]
    public async Task LoggingEnabledEntitySet_SelectiveAcrossSequentialRequests()
    {
        // A bad request writes exactly one diagnostic.
        Startup.LoggerProvider.Clear();
        HttpResponseMessage badResponse = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?$select=NoSuchProperty"));
        Assert.Equal(HttpStatusCode.BadRequest, badResponse.StatusCode);
        Assert.Single(GetQueryValidationEntries());

        // A subsequent valid request writes no diagnostic.
        Startup.LoggerProvider.Clear();
        HttpResponseMessage validResponse = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?$select=Name"));
        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);
        Assert.Empty(GetQueryValidationEntries());

        // A repeated bad request writes the diagnostic again.
        Startup.LoggerProvider.Clear();
        HttpResponseMessage repeatBadResponse = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?$select=NoSuchProperty"));
        Assert.Equal(HttpStatusCode.BadRequest, repeatBadResponse.StatusCode);
        Assert.Single(GetQueryValidationEntries());
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
