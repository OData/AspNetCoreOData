//-----------------------------------------------------------------------------
// <copyright file="QueryValidationBeforeActionTests.cs" company=".NET Foundation">
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
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.QueryValidationBeforeAction;

/// <summary>
/// EnableQuery attribute works correctly when controller returns ActionResult.
/// </summary>
public class QueryValidationBeforeActionTests : WebODataTestBase<QueryValidationBeforeActionTests.Startup>
{
    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup : TestStartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController));

            IEdmModel model = QueryValidationBeforeActionEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options => options.AddRouteComponents("odata", model).SetMaxTop(2).Expand().Select().OrderBy().Filter());
        }
    }

    public QueryValidationBeforeActionTests(WebODataTestFixture<Startup> fixture)
        : base(fixture)
    {
    }

    /// <summary>
    /// For bad queries query execution should happen (and fail) before action being called.
    /// </summary>
    /// <returns>Task tracking operation.</returns>
    [Fact]
    public async Task QueryExecutionBeforeActionBadQuery()
    {
        // Arrange (Allowed top is 10, we are sending 100)
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/Customers?$top=100");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

        // Act
        HttpResponseMessage response = await this.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
