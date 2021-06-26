// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ConcurrentQuery
{
    /// <summary>
    /// Ensures that concurrent execution of EnableQuery is thread-safe.
    /// </summary>
    public class ConcurrentQueryTests : WebApiTestBase<ConcurrentQueryTests>
    {
        public ConcurrentQueryTests(WebApiTestFixture<ConcurrentQueryTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController));

            var model = ConcurrentQueryEdmModel.GetEdmModel();
            services.AddControllers().AddOData(opt => opt.AddModel("concurrentquery", model)
                .Count().Filter().OrderBy().Expand().SetMaxTop(null));
        }

        /// <summary>
        /// For OData paths enable query should work with expansion.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [Fact]
        public async Task ConcurrentQueryExecutionIsThreadSafe()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Bumping thread count to allow higher parallelization.
            ThreadPool.SetMinThreads(100, 100);

            // Act
            var results = await Task.WhenAll(
                Enumerable.Range(1, 100)
                .Select(async i =>
                {
                    string queryUrl = $"concurrentquery/Customers?$filter=Id gt {i}";
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
                    request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

                    HttpResponseMessage response = await client.SendAsync(request);

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
}
