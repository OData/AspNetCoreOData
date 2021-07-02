// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.ETags.EFCore;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{
    public class ETags_CurrencyTokenEfContextTests : WebApiTestBase<ETags_CurrencyTokenEfContextTests>
    {

        public ETags_CurrencyTokenEfContextTests(WebApiTestFixture<ETags_CurrencyTokenEfContextTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=ETagCurrencyTokenEfContext8";
                services.AddDbContext<ETagCurrencyTokenEfContext>(opt => opt.UseLazyLoadingProxies().UseSqlServer(connectionString));

                IEdmModel edmModel = GetEdmModel();
                services.ConfigureControllers(typeof(DominiosController));
                services.AddControllers().AddOData(opt => opt.AddModel("odata", edmModel).Expand().Select());
            };
        }



        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Dominio>("Dominios");
            builder.EntitySet<ETags.EFCore.Server>("Servers");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task NestedDollarSelectWorksOnCurrencyTokenProperty()
        {
            string expect = "{\r\n" +
"  \"@odata.context\":\"http://localhost/odata/$metadata#Dominios(ServerAutenticazione(Id,RECVER))\",\"value\":[\r\n" +
"    {\r\n" +
"      \"@odata.etag\":\"W/\\\"bnVsbA==\\\"\",\"Id\":\"1\",\"Descrizione\":\"Test1\",\"ServerAutenticazioneId\":\"1\",\"RECVER\":null,\"ServerAutenticazione\":{\r\n" +
"        \"@odata.etag\":\"W/\\\"bnVsbA==\\\"\",\"Id\":\"1\",\"RECVER\":null\r\n" +
"      }\r\n" +
"    },{\r\n" +
"      \"@odata.etag\":\"W/\\\"MTA=\\\"\",\"Id\":\"2\",\"Descrizione\":\"Test2\",\"ServerAutenticazioneId\":\"2\",\"RECVER\":10,\"ServerAutenticazione\":{\r\n" +
"        \"@odata.etag\":\"W/\\\"NQ==\\\"\",\"Id\":\"2\",\"RECVER\":5\r\n" +
"      }\r\n" +
"    }\r\n" +
"  ]\r\n" +
"}";
            // Remove indentation
            expect = Regex.Replace(expect, @"\r\n\s*([""{}\]])", "$1");

            var getUri = "odata/Dominios?$expand=ServerAutenticazione($select=Id,RECVER)";
            HttpClient client = CreateClient();

            var response = await client.GetAsync(getUri);

            response.EnsureSuccessStatusCode();

            Assert.NotNull(response.Content);

            var payload = await response.Content.ReadAsStringAsync();

            Assert.Equal(expect, payload);
        }
    }
}
