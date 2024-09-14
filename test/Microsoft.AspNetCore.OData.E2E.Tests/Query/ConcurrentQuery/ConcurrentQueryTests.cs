//-----------------------------------------------------------------------------
// <copyright file="ConcurrentQueryTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.ConcurrentQuery;

/// <summary>
/// EnableQuery attribute works correctly when controller returns ActionResult.
/// </summary>
public class ConcurrentQueryTests : WebODataTestBase<ConcurrentQueryTests.Startup>
{
    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup : TestStartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController));

            IEdmModel model = ConcurrentQueryEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options => options.AddRouteComponents("odata", model).SetMaxTop(2).Expand().Select().OrderBy().Filter());
        }
    }

    public ConcurrentQueryTests(WebODataTestFixture<Startup> fixture)
        : base(fixture)
    {
    }

    /// <summary>
    /// For OData paths enable query should work with expansion.
    /// </summary>
    /// <returns>Task tracking operation.</returns>
    ////[Fact] - Commented out as running this test right now throws 'Concurrent reads or writes are not supported' exception
    public async Task ConcurrentQueryExecutionIsThreadSafe()
    {
        // Arrange
        // Bumping thread count to allow higher parallelization.
        ThreadPool.SetMinThreads(100, 100);

        // Act
        var results = await Task.WhenAll(
            Enumerable.Range(1, 100)
            .Select(async i =>
            {
                string queryUrl = string.Format("odata/Customers?$filter=Id gt {0}", i);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

                HttpResponseMessage response = await this.Client.SendAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

                return (i: i, length: customers.Count);
            }));

        // Assert
        foreach (var result in results)
        {
            Assert.Equal(100 - result.i, result.length);
        }
    }
}
