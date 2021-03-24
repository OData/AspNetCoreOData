// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DynamicProperties
{
    public class DynamicPropertiesTest : WebApiTestBase<DynamicPropertiesTest>
    {
        public DynamicPropertiesTest(WebApiTestFixture<DynamicPropertiesTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(DynamicCustomersController), typeof(DynamicSingleCustomerController)/*, typeof(ODataEndpointController)*/);
            services.AddOData(opt => opt.AddModel("odata", GetEdmModel()));
        }

        //[Fact]
        // If test the routes, enable this test and see the payload. Remember to include the ODataEndpointController.
        //public async Task TestRoutes()
        //{
        //    // Arrange
        //    string requestUri = "$odata";
        //    HttpClient client = CreateClient();

        //    // Act
        //    var response = await client.GetAsync(requestUri);

        //    // Assert
        //    response.EnsureSuccessStatusCode();
        //    string payload = await response.Content.ReadAsStringAsync();
        //}

        [Theory]
        [InlineData("DynamicCustomers(1)/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty_1")]
        [InlineData("DynamicCustomers(2)/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount_2")]
        [InlineData("DynamicCustomers(3)/Order/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromOrder_3")]
        [InlineData("DynamicCustomers(4)/Microsoft.AspNetCore.OData.E2E.Tests.DynamicProperties.DynamicVipCustomer/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty_4")]
        [InlineData("DynamicCustomers(5)/Microsoft.AspNetCore.OData.E2E.Tests.DynamicProperties.DynamicVipCustomer/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount_5")]
        [InlineData("DynamicSingleCustomer/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty")]
        [InlineData("DynamicSingleCustomer/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount")]
        [InlineData("DynamicSingleCustomer/Order/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromOrder")]
        [InlineData("DynamicCustomers(1)/Id", "Id_1")]
        public async Task AccessPropertyTest(string uri, string expected)
        {
            string requestUri = $"odata/{uri}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("Age")]
        [InlineData("Order")]
        [InlineData("Account")]
        [InlineData("SecondAccount")]
        public async Task AccessNormalPropertyWithGenericRoute(string property)
        {
            // Arrange
            string requestUri = $"odata/DynamicCustomers(9)/{property}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains($"GetProperty_{property}_9", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("Get")]
        [InlineData("Patch")]
        [InlineData("Delete")]
        public async Task AccessDynamicPropertyWithOtherMethodsTest(string method)
        {
            // Arrange
            string requestUri = "odata/DynamicCustomers(9)/DynamicPropertyName";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("DynamicPropertyName_GetDynamicProperty_9", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("Put")]
        [InlineData("Post")]
        public async Task AccessDynamicPropertyWithWrongMethodReturnsMethodNotAllowed(string method)
        {
            // Arranget
            string requestUri = "odata/DynamicCustomers(1)/DynamicPropertyName";

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Theory]
        [InlineData("DynamicCustomers(2)/SecondAccount/DynamicPropertyName")]
        [InlineData("DynamicSingleCustomer/SecondAccount/DynamicPropertyName")]
        public async Task AccessDynamicPropertyWithoutImplementMethod(string uri)
        {
            // Arrange
            string requestUri = $"odata/{uri}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DynamicCustomer>("DynamicCustomers");
            builder.Singleton<DynamicSingleCustomer>("DynamicSingleCustomer");
            return builder.GetEdmModel();
        }
    }
}
