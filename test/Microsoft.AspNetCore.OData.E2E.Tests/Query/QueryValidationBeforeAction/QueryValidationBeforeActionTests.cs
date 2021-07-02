// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.Query.QueryValidationBeforeAction;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    /// <summary>
    /// EnableQuery attribute works correctly when controller returns ActionResult.
    /// </summary>
    public class QueryValidationBeforeActionTests : WebApiTestBase<QueryValidationBeforeActionTests>
    {

        public QueryValidationBeforeActionTests(WebApiTestFixture<QueryValidationBeforeActionTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                services.ConfigureControllers(typeof(CustomersController));

                IEdmModel model = QueryValidationBeforeActionEdmModel.GetEdmModel();
                services.AddControllers().AddOData(options => options.AddModel("odata", model).EnableODataQuery(2));
            };
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
            HttpResponseMessage response = await CreateClient().SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Output.WriteLine(responseContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }

}
