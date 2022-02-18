//-----------------------------------------------------------------------------
// <copyright file="DerivedETagTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags
{
    public class DerivedETagTests : WebApiTestBase<DerivedETagTests>
    {
        public DerivedETagTests(WebApiTestFixture<DerivedETagTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel1 = GetEdmModel();
            IEdmModel edmModel2 = GetDerivedEdmModel();
            services.ConfigureControllers(typeof(ETagsCustomersController), typeof(ETagsDerivedCustomersSingletonController), typeof(ETagsDerivedCustomersController));
            services.AddControllers().AddOData(opt => opt.Select().AddRouteComponents("odata", edmModel1).AddRouteComponents("derivedEtag", edmModel2));
            services.AddControllers(opt => opt.Filters.Add(new ETagActionFilterAttribute()));
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            eTagsCustomersSet.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsCustomersSet.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            EntitySetConfiguration<ETagsCustomer> eTagsDerivedCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsDerivedCustomers");
            eTagsDerivedCustomersSet.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsDerivedCustomersSet.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            SingletonConfiguration<ETagsCustomer> eTagsCustomerSingleton = builder.Singleton<ETagsCustomer>("ETagsDerivedCustomersSingleton");
            eTagsCustomerSingleton.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsCustomerSingleton.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            return builder.GetEdmModel();
        }

        private static IEdmModel GetDerivedEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
            eTagsCustomersSet.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsCustomersSet.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            EntitySetConfiguration<ETagsDerivedCustomer> eTagsDerivedCustomersSet = builder.EntitySet<ETagsDerivedCustomer>("ETagsDerivedCustomers");
            eTagsDerivedCustomersSet.HasRequiredBinding(c => c.RelatedCustomer, eTagsCustomersSet);
            eTagsDerivedCustomersSet.HasRequiredBinding(c => c.ContainedCustomer, eTagsCustomersSet);
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task DerivedTypesHaveSameETagsTest()
        {
            // Arrange - Base
            HttpClient client = CreateClient();
            string requestUri = "odata/ETagsCustomers?$select=Id";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsObject<JObject>();
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());

            // Arrange - Derived
            requestUri = "odata/ETagsDerivedCustomers?$select=Id";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            jsonResult = await response.Content.ReadAsObject<JObject>();
            var derivedEtags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            
            Assert.True(String.Concat(jsonETags) == String.Concat(derivedEtags), "Derived Types has different etags than base type");
        }

        [Fact]
        public async Task SingletonsHaveSameETagsTest()
        {
            // Arrange - Base
            HttpClient client = CreateClient();

            string requestUri = "odata/ETagsCustomers?$select=Id";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsObject<JObject>();
            var jsonETags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());

            // Arrange - Derived
            requestUri = "odata/ETagsDerivedCustomersSingleton?$select=Id";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            jsonResult = await response.Content.ReadAsObject<JObject>();
            var singletonEtag = jsonResult.GetValue("@odata.etag").ToString();

            Assert.True(jsonETags.FirstOrDefault() == singletonEtag, "Singleton has different etags than Set");
        }

        [Fact]
        public async Task DerivedEntitySetsHaveETagsTest()
        {
            // Arrange
            HttpClient client = CreateClient();
            string requestUri = "derivedEtag/ETagsDerivedCustomers?$select=Id";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd("application/json");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var jsonResult = await response.Content.ReadAsObject<JObject>();
            var derivedEtags = jsonResult.GetValue("value").Select(e => e["@odata.etag"].ToString());
            Assert.Equal(10, derivedEtags.Count());
            Assert.Equal("W/\"bnVsbA==\"", derivedEtags.First());
        }
    }
}
