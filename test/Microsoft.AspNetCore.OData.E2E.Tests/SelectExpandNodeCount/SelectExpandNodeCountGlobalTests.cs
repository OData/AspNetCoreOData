//-----------------------------------------------------------------------------
// <copyright file="SelectExpandNodeCountGlobalTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SelectExpandNodeCount
{
    public class SelectExpandNodeCountGlobalTests : WebApiTestBase<SelectExpandNodeCountGlobalTests>
    {
        public SelectExpandNodeCountGlobalTests(WebApiTestFixture<SelectExpandNodeCountGlobalTests> fixture)
            : base(fixture)
        {
        }

        // Configures the global projection-item limit through ODataOptions.SetMaxSelectExpandNodeCount. The endpoint
        // (DefaultProjectionItemsController) declares [EnableQuery] with no explicit MaxSelectExpandNodeCount, so the
        // global value is the one that applies.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(DefaultProjectionItemsController));

            IEdmModel model = SelectExpandNodeCountEdmModel.GetEdmModel();
            services.AddControllers().AddOData(options =>
            {
                options.EnableQueryFeatures();
                options.SetMaxSelectExpandNodeCount(5);
                options.AddRouteComponents("odata", model);
            });
        }

        [Fact]
        public async Task DefaultEndpoint_UsesGlobalMaxSelectExpandNodeCount_WhenNoEndpointSetting()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act & Assert: 6 select items exceed the global limit (5) and are rejected.
            HttpResponseMessage exceeding = await client.GetAsync("odata/DefaultProjectionItems?$select=Id,P1,P2,P3,P4,P5");
            Assert.Equal(HttpStatusCode.BadRequest, exceeding.StatusCode);
            Assert.Contains(Error.Format(SRResources.MaxSelectExpandNodeCountExceeded, 5, "MaxSelectExpandNodeCount"), await exceeding.Content.ReadAsStringAsync());

            // Act & Assert: 5 select items are within the global limit and succeed.
            HttpResponseMessage within = await client.GetAsync("odata/DefaultProjectionItems?$select=Id,P1,P2,P3,P4");
            Assert.Equal(HttpStatusCode.OK, within.StatusCode);
        }
    }
}
