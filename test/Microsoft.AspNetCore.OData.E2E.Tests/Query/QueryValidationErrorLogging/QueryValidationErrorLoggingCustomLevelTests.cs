//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLoggingCustomLevelTests.cs" company=".NET Foundation">
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
/// End-to-end tests that verify <see cref="ODataOptions.QueryValidationErrorLogLevel"/> sets the level of the
/// diagnostic once for every <see cref="EnableQueryAttribute"/> action.
/// </summary>
public class QueryValidationErrorLoggingCustomLevelTests : WebODataTestBase<QueryValidationErrorLoggingCustomLevelTests.Startup>
{
    public class Startup : TestStartupBase
    {
        public static readonly CapturingLoggerProvider LoggerProvider = new CapturingLoggerProvider();

        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(PlainCustomersController));

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddProvider(LoggerProvider);
            });

            IEdmModel model = QueryValidationErrorLoggingEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options =>
            {
                options.EnableQueryValidationErrorLogging = true; // Enable once for every [EnableQuery] action.
                options.QueryValidationErrorLogLevel = LogLevel.Error; // Record at a non-default level for every action.
                options.AddRouteComponents("odata", model).Select().Expand();
            });
        }
    }

    public QueryValidationErrorLoggingCustomLevelTests(WebODataTestFixture<Startup> fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task PlainEntitySet_UnknownSelectProperty_WritesDiagnosticAtGlobalLevel()
    {
        // A plain [EnableQuery] attribute records the diagnostic at the global level (Error).
        Startup.LoggerProvider.Clear();

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/PlainCustomers?$select=NoSuchProperty"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        CapturedLogEntry entry = Assert.Single(GetQueryValidationEntries());
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("$select=NoSuchProperty", entry.GetFieldValue("QueryOptions"));
        Assert.Contains("NoSuchProperty", entry.GetFieldValue("Reason"));
        Assert.NotNull(entry.Exception);
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
