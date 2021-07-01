// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class UriParserExtension_PrefixFreeEnumTests : WebApiTestBase<UriParserExtension_PrefixFreeEnumTests>
    {

        public UriParserExtension_PrefixFreeEnumTests(WebApiTestFixture<UriParserExtension_PrefixFreeEnumTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                services.ConfigureControllers(typeof(CustomersController), typeof(OrdersController), typeof(MetadataController));

                IEdmModel model = UriParserExtensionEdmModel.GetEdmModel();
                services.AddControllers().AddOData(opt => opt.AddModel("odata", model,
                    services =>
                    {
                        services.AddSingleton<ODataUriResolver>(sp => new StringAsEnumResolver() { EnableCaseInsensitive = true });
                    }
                    ));
            };
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

        [Fact]
        public async Task EnableEnumPrefixFreeTest()
        {
            // Enum with prefix
            HttpClient client = CreateClient();

            var prefixUri = $"odata/Customers/Default.GetCustomerByGender(gEnDeR=Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.Gender'mAlE')";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, prefixUri);
            HttpResponseMessage response = await client.SendAsync(request);

            string prefixResponse = await response.Content.ReadAsStringAsync();
            Output.WriteLine($"prefixResponse: {prefixResponse}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Enum prefix free
            var prefixFreeUri = $"odata/Customers/Default.GetCustomerByGender(GeNdEr='MaLe')";
            request = new HttpRequestMessage(HttpMethod.Get, prefixFreeUri);
            response = await client.SendAsync(request);

            string prefixFreeResponse = await response.Content.ReadAsStringAsync();
            Output.WriteLine($"prefixFreeResponse: {prefixFreeResponse}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(prefixResponse, prefixFreeResponse);
        }

        [Fact]
        public async Task EnableEnumPrefixFreeTestThrows()
        {
            // Enum with prefix
            HttpClient client = CreateClient();

            var prefixUri = $"odata/Customers/Default.GetCustomerByGender(GeNdEr=Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.Gender'UnknownValue')";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, prefixUri);

            try
            {
                await client.SendAsync(request);
            }
            catch(ODataException ex)
            {
                Assert.Equal("The parameter value (Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.Gender'UnknownValue') from request is not valid. " +
                    "The parameter value should be format of type 'Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.Gender'.", ex.Message);
            }

            // Enum prefix free
            var prefixFreeUri = $"odata/Customers/Default.GetCustomerByGender(gEnDeR='UnknownValue')";
            request = new HttpRequestMessage(HttpMethod.Get, prefixFreeUri);

            try
            {
                await client.SendAsync(request);
            }
            catch(ODataException ex)
            {
                Assert.Equal("The parameter value ('UnknownValue') from request is not valid. " +
                    "The parameter value should be format of type 'Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension.Gender'.", ex.Message);
            }
        }

    }

}