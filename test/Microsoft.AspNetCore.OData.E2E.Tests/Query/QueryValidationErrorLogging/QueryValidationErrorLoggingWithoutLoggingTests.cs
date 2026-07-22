//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLoggingWithoutLoggingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.QueryValidationErrorLogging;

/// <summary>
/// End-to-end tests verifying that enabling query validation error logging is safe when the application
/// has not configured any capturing logging: the request still produces the unchanged bad request response.
/// </summary>
public class QueryValidationErrorLoggingWithoutLoggingTests : WebODataTestBase<QueryValidationErrorLoggingWithoutLoggingTests.Startup>
{
    public class Startup : TestStartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(LoggingCustomersController));

            IEdmModel model = QueryValidationErrorLoggingEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options => options.AddRouteComponents("odata", model).Select().Expand());
        }
    }

    public QueryValidationErrorLoggingWithoutLoggingTests(WebODataTestFixture<Startup> fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task LoggingEnabled_NoLoggingConfigured_ReturnsUnchangedBadRequest()
    {
        // Act
        HttpResponseMessage response = await this.Client.SendAsync(CreateGet("odata/LoggingCustomers?$select=NoSuchProperty"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("NoSuchProperty", body);
    }

    private static HttpRequestMessage CreateGet(string url)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        return request;
    }
}
