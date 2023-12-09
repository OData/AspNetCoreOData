//-----------------------------------------------------------------------------
// <copyright file="NonEdmTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.NonEdm
{
    public class NonEdmTests : WebApiTestBase<NonEdmTests>
    {
        public NonEdmTests(WebApiTestFixture<NonEdmTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController));
            services.AddControllers().AddOData(opt =>
            {
                opt.EnableQueryFeatures();
                opt.AddRouteComponents(services =>
                {
                    services.AddSingleton<ODataUriResolver>(sp => new StringAsEnumResolver() { EnableCaseInsensitive = true });
                });
            });
        }

        [Fact]
        public async Task NonEdmFilterByEnumString()
        {
            Assert.Equal(5, (await GetResponse<Customer[]>("$filter=Gender eq 'MaLe'")).Length);
        }        

        [Fact]
        public async Task NonEdmFilterByEnumStringWithEnableQueryAttribute()
        {
            Assert.Equal(5, (await GetResponse<Customer[]>("$filter=Gender eq 'MaLe'", "WithEnableQueryAttribute")).Length);
        }

        [Fact]
        public async Task NonEdmSumFilteredByEnumString()
        {
            var response = await GetResponse<JArray>("$apply=filter(Gender eq 'female')/aggregate(Id with sum as Sum)");
            Assert.Equal(30, response.Single()["Sum"].Value<int>());
        }

        [Fact]
        public async Task NonEdmSelectTopFilteredByEnumString()
        {
            var response = await GetResponse<Customer[]>("$filter(Gender eq 'female')&$orderby=Id desc&$select=Name&$top=1&$skip=1");
            Assert.Equal("Customer #9", response.Single().Name);
        }

        private async Task<T> GetResponse<T>(string queryOptions, string method = null)
        {
            using var response = await CreateClient().SendAsync(
                new HttpRequestMessage(HttpMethod.Get, $"api/Customers/{method ?? string.Empty}?{queryOptions}"));
            return await response.Content.ReadAsObject<T>();
        }
    }
}
