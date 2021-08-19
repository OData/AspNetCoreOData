//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ServerSidePaging
{
    public class ServerSidePagingTests : WebApiTestBase<ServerSidePagingTests>
    {
        public ServerSidePagingTests(WebApiTestFixture<ServerSidePagingTests> fixture)
            : base(fixture)
        {
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = GetEdmModel();
            services.ConfigureControllers(typeof(ServerSidePagingCustomersController));
            services.AddControllers().AddOData(opt => opt.Expand().AddRouteComponents("{a}", edmModel));
        }

        protected static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ServerSidePagingOrder>("ServerSidePagingOrders").EntityType.HasRequired(d => d.ServerSidePagingCustomer);
            builder.EntitySet<ServerSidePagingCustomer>("ServerSidePagingCustomers").EntityType.HasMany(d => d.ServerSidePagingOrders);
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ValidNextLinksGenerated()
        {
            // Arrange
            string requestUri = "/prefix/ServerSidePagingCustomers?$expand=ServerSidePagingOrders";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();

            // Assert
            // Customer 1 => 6 Orders, Customer 2 => 5 Orders, Customer 3 => 4 Orders, ...
            // NextPageLink will be expected on the Customers collection as well as
            // the Orders child collection on Customer 1
            using (JsonDocument document = JsonDocument.Parse(content))
            {
                bool found = document.RootElement.TryGetProperty("value", out JsonElement value);
                Assert.True(found);

                foreach (JsonElement item in value.EnumerateArray())
                {
                    found = item.TryGetProperty("Id", out JsonElement id);
                    Assert.True(found);

                    // only the Orders child collection on Customer 1
                    bool odersNextLink = item.TryGetProperty("ServerSidePagingOrders@odata.nextLink", out JsonElement ordersNextLink);
                    int idValue = id.GetInt32();
                    if (idValue == 1)
                    {
                        Assert.True(odersNextLink);
                        Assert.Equal("http://localhost/prefix/ServerSidePagingCustomers/1/ServerSidePagingOrders?$skip=5", ordersNextLink.GetString());
                    }
                    else
                    {
                        Assert.False(odersNextLink);
                    }
                }

                bool nextLinkFound = document.RootElement.TryGetProperty("@odata.nextLink", out JsonElement nextLink);
                Assert.True(nextLinkFound);
                Assert.Equal("http://localhost/prefix/ServerSidePagingCustomers?$expand=ServerSidePagingOrders&$skip=5", nextLink.GetString());
            }
        }
    }
}
