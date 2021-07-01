// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.E2E.Tests.Typeless;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class TypelessDeltaSerializationTests : WebApiTestBase<TypelessDeltaSerializationTests>
    {

        public TypelessDeltaSerializationTests(WebApiTestFixture<TypelessDeltaSerializationTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                IEdmModel edmModel = GetEdmModel();
                services.ConfigureControllers(typeof(TypelessDeltaCustomersController));
                services.AddControllers().AddOData(opt => opt.Expand().AddModel("odata", edmModel));
            };
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var customers = builder.EntitySet<TypelessCustomer>("TypelessDeltaCustomers");
            customers.EntityType.Property(c => c.Name).IsRequired();
            var orders = builder.EntitySet<TypelessOrder>("TypelessDeltaOrders");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full")]
        public async Task TypelessDeltaWorksInAllFormats(string acceptHeader)
        {
            // Arrange
            string url = "odata/TypelessDeltaCustomers?$deltatoken=abc";
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            JObject returnedObject = await response.Content.ReadAsObject<JObject>();
            Assert.True(((dynamic)returnedObject).value.Count == 15);

            //Verification of content to validate Payload
            for (int i = 0 ; i < 10 ; i++)
            {
                string name = string.Format("Name {0}", i);
                Assert.True(name.Equals(((dynamic)returnedObject).value[i]["Name"].Value));
            }

            for (int i=10 ; i < 15 ; i++)
            {
                string contextUrl = "http://localhost/odata/$metadata#TypelessDeltaCustomers/$deletedEntity";
                Assert.True(contextUrl.Equals(((dynamic)returnedObject).value[i]["@odata.context"].Value));
                Assert.True(i.ToString().Equals(((dynamic)returnedObject).value[i]["id"].Value));
            }
        }

    }

}
