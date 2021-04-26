// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension
{
    public class UnqualifiedCallTest : WebApiTestBase<UnqualifiedCallTest>
    {
        public UnqualifiedCallTest(WebApiTestFixture<UnqualifiedCallTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController), typeof(OrdersController), typeof(MetadataController));

            IEdmModel model = UriParserExtenstionEdmModel.GetEdmModel();
            services.AddControllers().AddOData(opt => opt.AddModel("odata", model));
        }

        public static TheoryDataSet<string, string, string> UnqualifiedCallCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    { "Get", "Customers(1)/Default.CalculateSalary(month=2)", "Customers(1)/CalculateSalary(month=2)" },
                    { "Post", "Customers(1)/Default.UpdateAddress", "Customers(1)/UpdateAddress" },
                };
            }
        }

        public static TheoryDataSet<string, string, string> UnqualifiedCallAndCaseInsensitiveCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    { "Get", "Customers(1)/Default.CalculateSalary(month=2)", "CuStOmErS(1)/CaLcUlAtESaLaRy(MONTH=2)" },
                    { "Post", "Customers(1)/Default.UpdateAddress", "cUsToMeRs(1)/upDaTeAdDrEsS" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(UnqualifiedCallCases))]
        public async Task EnableUnqualifiedCallTest(string method, string qualifiedFunction, string unqualifiedFunction)
        {
            // Case sensitive
            HttpClient client = CreateClient();

            var qualifiedFunctionUri = $"odata/{qualifiedFunction}";
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), qualifiedFunctionUri);
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string qualifiedFunctionResponse = await response.Content.ReadAsStringAsync();

            // Case Insensitive
            var unqualifiedFunctionUri = $"odata/{unqualifiedFunction}";
            request = new HttpRequestMessage(new HttpMethod(method), unqualifiedFunctionUri);
            response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string unqualifiedFunctionResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(qualifiedFunctionResponse, unqualifiedFunctionResponse);
        }

        [Theory]
        [MemberData(nameof(UnqualifiedCallAndCaseInsensitiveCases))]
        public async Task EnableUnqualifiedCallAndCaseInsensitiveTest(string method, string qualifiedSensitiveFunction,
            string unqualifiedInsensitiveFunction)
        {
            // Case sensitive
            HttpClient client = CreateClient();
            var qualifiedSensitiveFunctionUri = $"odata/{qualifiedSensitiveFunction}";
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), qualifiedSensitiveFunctionUri);
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string qualifiedSensitiveFunctionResponse = await response.Content.ReadAsStringAsync();

            // Case Insensitive
            var unqualifiedInsensitiveFunctionUri = $"odata/{unqualifiedInsensitiveFunction}";
            request = new HttpRequestMessage(new HttpMethod(method), unqualifiedInsensitiveFunctionUri);
            response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string unqualifiedInsensitiveFunctionResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(qualifiedSensitiveFunctionResponse, unqualifiedInsensitiveFunctionResponse);
        }
    }
}