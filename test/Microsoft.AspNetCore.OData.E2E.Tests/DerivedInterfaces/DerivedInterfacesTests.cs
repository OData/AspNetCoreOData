//-----------------------------------------------------------------------------
// <copyright file="DerivedInterfacesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DerivedInterfaces
{
    public class DerivedTypeTests : WebApiTestBase<DerivedTypeTests>
    {
        public DerivedTypeTests(WebApiTestFixture<DerivedTypeTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController));
            services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", GetEdmModel()).Count().Filter().OrderBy().Expand().SetMaxTop(null).Select());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task QueryCustomersFiltered()
        {
            // Arrange
            string requestUri = "/odata/Customers?$filter=Order/Id eq 11";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string py = await response.Content.ReadAsStringAsync();
                        
            // Assert
            Assert.DoesNotContain("error", py);
            Assert.True(response.IsSuccessStatusCode);

            string expectedContent = "\"Id\":2,\"Name\":\"Customer 1\"";
            Assert.Contains(expectedContent, expectedContent);
        } 
    }
}
