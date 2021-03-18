// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension
{
    public class EnumPrefixFreeTest : WebApiTestBase<EnumPrefixFreeTest>
    {
        public EnumPrefixFreeTest(WebApiTestFixture<EnumPrefixFreeTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController), typeof(OrdersController), typeof(MetadataController));

            IEdmModel model = UriParserExtenstionEdmModel.GetEdmModel();
            services.AddOData(opt => opt.AddModel("odata", model,
                builder =>
                {
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(ODataUriResolver), sp => new StringAsEnumResolver() { EnableCaseInsensitive = true });
                }
                ));
        }

        public static TheoryDataSet<string, string, int> EnumPrefixFreeCases
        {
            get
            {
                // Create data with case insensitive parameter name and case insensitive enum value.
                // Enum type prefix, if present, is still required to be case sensitive since it is type-related.
                return new TheoryDataSet<string, string, int>()
                {
                    { "gEnDeR=Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.Gender'mAlE'", "GeNdEr='MaLe'", (int)HttpStatusCode.OK },
                    { "GeNdEr=Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.Gender'UnknownValue'", "gEnDeR='UnknownValue'", (int)HttpStatusCode.BadRequest },
                };
            }
        }

        [Theory]
        [MemberData(nameof(EnumPrefixFreeCases))]
        public async Task EnableEnumPrefixFreeTest(string prefix, string prefixFree, int statusCode)
        {
            // Enum with prefix
            HttpClient client = CreateClient();

            var prefixUri = $"odata/Customers/Default.GetCustomerByGender({prefix})";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, prefixUri);
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(statusCode, (int)response.StatusCode);
            string prefixResponse = await response.Content.ReadAsStringAsync();

            // Enum prefix free
            var prefixFreeUri = $"odata/Customers/Default.GetCustomerByGender({prefixFree})";
            request = new HttpRequestMessage(HttpMethod.Get, prefixFreeUri);
            response = await client.SendAsync(request);

            Assert.Equal(statusCode, (int)response.StatusCode);
            string prefixFreeResponse = await response.Content.ReadAsStringAsync();

            if (statusCode == (int)HttpStatusCode.OK)
            {
                Assert.Equal(prefixResponse, prefixFreeResponse);
            }
        }
    }
}