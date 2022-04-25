using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.NonEdm
{
    public class ConfigureServiceCollectionTest : WebApiTestBase<ConfigureServiceCollectionTest>
    {
        public ConfigureServiceCollectionTest(WebApiTestFixture<ConfigureServiceCollectionTest> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController));
            services.AddControllers().AddOData(opt =>
            {
                opt.EnableQueryFeatures();
                opt.ConfigureServiceCollection(services =>
                {
                    services.AddSingleton<ODataUriResolver>(sp => new StringAsEnumResolver() { EnableCaseInsensitive = true });
                });
            });
        }

        [Fact]
        public async Task EnableConfigureServiceCollectionTest()
        {
            using (var response = await CreateClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, $"api/Customers?$filter=Gender eq 'MaLe'")))
            {
                var values = await response.Content.ReadAsObject<JArray>();
                Assert.Equal(3, values.Count);
            }
        }
    }
}
