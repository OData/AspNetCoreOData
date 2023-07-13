//-----------------------------------------------------------------------------
// <copyright file="IAsyncEnumerableTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IAsyncEnumerableTests
{
    public class IAsyncEnumerableTests : WebODataTestBase<IAsyncEnumerableTests.TestsStartup>
    {
       public class TestsStartup : TestStartupBase
       {
            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddDbContext<IAsyncEnumerableContext>(opt => opt.UseInMemoryDatabase("IAsyncEnumerableTest"));

                services.ConfigureControllers(typeof(CustomersController));

                IEdmModel edmModel = IAsyncEnumerableEdmModel.GetEdmModel();
                services.AddControllers().AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(null)
                    .AddRouteComponents("odata", edmModel));

                services.AddControllers().AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(null)
                    .AddRouteComponents("v1", edmModel));
            }
       }

        public IAsyncEnumerableTests(WebODataTestFixture<TestsStartup> factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task UsingAsAsyncEnumerableWorks()
        {
            // Arrange
            string queryUrl = "odata/Customers";
            var expectedResult = "{\"@odata.context\":\"http://localhost/odata/$metadata#Customers\",\"value\":[{\"Id\":1,\"Name\":\"Customer0\",\"Address\":{\"Name\":\"City1\",\"Street\":\"Street1\"}},{\"Id\":2,\"Name\":\"Customer1\",\"Address\":{\"Name\":\"City0\",\"Street\":\"Street0\"}},{\"Id\":3,\"Name\":\"Customer0\",\"Address\":{\"Name\":\"City1\",\"Street\":\"Street1\"}}]}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);     
            var resultObject = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResult, resultObject);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();
            Assert.Equal(3, customers.Count);
        }

        [Fact]
        public async Task UsingAsAsyncEnumerableWorksWithoutEFCore()
        {
            // Arrange
            string queryUrl = "v1/Customers";
            var expectedResult = "{\"@odata.context\":\"http://localhost/v1/$metadata#Customers\",\"value\":[{\"Id\":1,\"Name\":\"Customer1\",\"Address\":{\"Name\":\"City1\",\"Street\":\"Street1\"}},{\"Id\":2,\"Name\":\"Customer2\",\"Address\":{\"Name\":\"City2\",\"Street\":\"Street2\"}}]}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var resultObject = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResult, resultObject);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();
            Assert.Equal(2, customers.Count);
        }
    }
}
