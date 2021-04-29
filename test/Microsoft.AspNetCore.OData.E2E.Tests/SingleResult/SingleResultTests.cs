// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SingleResultTest
{
    public class SingleResultTests : WebApiTestBase<SingleResultTests>
    {
        public SingleResultTests(WebApiTestFixture<SingleResultTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController));

            services.AddControllers().AddOData(opt => opt.AddModel("singleresult", SingleResultEdmModel.GetEdmModel())
                .Count().Filter().OrderBy().Expand().SetMaxTop(null));
        }

        [Fact]
        public async Task SingleResultReturnsCorrentResult()
        {
            // Arrange
            string queryUrl = "singleresult/Customers(8)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"Id\":8,\"Name\":\"name_8\"}", payload);
        }

        [Fact]
        public async Task EmptySingleResultReturnsNotFound()
        {
            // Arrange
            string queryUrl = "singleresult/Customers(100)";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
