//-----------------------------------------------------------------------------
// <copyright file="UriResolverDependencyTestWithOldDefaultConfig.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension
{
    public class UnqualifiedCallTestWithOldDefaultConfig : WebApiTestBase<UnqualifiedCallTestWithOldDefaultConfig>
    {
        public UnqualifiedCallTestWithOldDefaultConfig(WebApiTestFixture<UnqualifiedCallTestWithOldDefaultConfig> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel model = UriParserExtenstionEdmModel.GetEdmModel();

            services.ConfigureControllers(typeof(CustomersController), typeof(OrdersController), typeof(MetadataController));

            services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", model).RouteOptions.EnableUnqualifiedOperationCall = false);
        }

        public static TheoryDataSet<string, string, HttpStatusCode> urisForOldDefaultConfig
        {
            get
            {
                return new TheoryDataSet<string, string, HttpStatusCode>()
                {
                    // bad cases
                    { "Get", "Customers(1)/CalculateSalary(month=2)", HttpStatusCode.NotFound },
               //     { "Post", "Customers(1)/UpdateAddress", HttpStatusCode.NotFound },
              //      { "Get", "CuStOmRrS(1)/Default.CaLcUlAtESaLaRy(MoNtH=2)", HttpStatusCode.NotFound },
              //      { "Post", "CuUtOmRrS(1)/Default.UpDaTeAdDrEsS", HttpStatusCode.NotFound },

                    // good cases
                    { "Get", "Customers(1)/Default.CalculateSalary(month=2)", HttpStatusCode.OK },
                    { "Post", "Customers(1)/Default.UpdateAddress", HttpStatusCode.OK },
                };
            }
        }

        [Theory]
        [MemberData(nameof(urisForOldDefaultConfig))]
        public async Task ParseUriWithOldDefaultRestored(string method, string uri, HttpStatusCode expectedStatusCode)
        {
            // Case Insensitive
            HttpClient client = CreateClient();

            var caseInsensitiveUri = $"odata/{uri}";
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), caseInsensitiveUri);
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}
