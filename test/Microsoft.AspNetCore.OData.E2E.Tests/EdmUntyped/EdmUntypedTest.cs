//-----------------------------------------------------------------------------
// <copyright file="EdmUntypedTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.EdmUntyped
{
    /// <summary>
    /// EdmUntyped here means the property type is "Edm.Untyped" type.
    /// All properties defined using "Edm.Untyped" type are un-declared properties.
    /// </summary>
    public class EdmUntypedTest : WebApiTestBase<EdmUntypedTest>
    {
        public EdmUntypedTest(WebApiTestFixture<EdmUntypedTest> fixture)
           : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = GetEdmModel();
            services.ConfigureControllers(typeof(MetadataController), typeof(BillsController));
            services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", edmModel));
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Bill>("Bills");
            var edmModel = builder.GetEdmModel();
            return edmModel;
        }

        [Fact]
        public async Task EdmUntyped_QueryMetadata_WorksAsExpected()
        {
            // Arrange
            var requestUri = "odata/$metadata";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                string.Format("Response status code, expected: {0}, actual: {1}, request url: {2}",
                HttpStatusCode.OK, response.StatusCode, requestUri));

            Assert.Contains("<Property Name=\"HomeAddress\" Type=\"Microsoft.AspNetCore.OData.E2E.Tests.EdmUntyped.Address\" />", responseString);
            Assert.Contains("<Property Name=\"Addresses\" Type=\"Collection(Microsoft.AspNetCore.OData.E2E.Tests.EdmUntyped.Address)\" />", responseString);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EdmUntyped_Post_UsingDifferentCase_WorksAsExpected(bool caseSensitive)
        {
            // Arrange
            string id = caseSensitive ? "ID" : "id";
            string name = caseSensitive ? "Name" : "nAme";
            string frequency = caseSensitive ? "Frequency" : "frequency";
            string contactGuid = caseSensitive ? "ContactGuid" : "contactguid";
            string weight = caseSensitive ? "Weight" : "weight";
            string homeAddress = caseSensitive ? "HomeAddress" : "homeADDRESS";
            string addresses = caseSensitive ? "Addresses" : "addresses";

            // in the controller, we will verify the following content to make sure it works fine.
            // if you change the content, please change the verification codes in the controller as well.
            string content = $@"
{{
    '{id}':921,
    '{name}':'Fan',
    '{frequency}': 'BiWeekly',
    '{contactGuid}': '21EC2020-3AEA-1069-A2DD-08002B30309D',
    '{weight}': 3.14,
    '{homeAddress}': {{ 'Street':'MyStreet','City':'MyCity'}},
    '{addresses}':[
        {{ 'Street':'Street-1','City':'City-1'}},
        {{ 'Street':'Street-2','City':'City-2'}}
    ]
}}";
            var requestUri = "odata/Bills";
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = content.Length;

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                string.Format("Response status code, expected: {0}, actual: {1}, request url: {2}",
                HttpStatusCode.OK, response.StatusCode, requestUri));

            string responseString = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":true}", responseString);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EdmUntyped_Patch_UsingDifferentCase_WorksAsExpected(bool caseSensitive)
        {
            // Arrange
            string frequency = caseSensitive ? "Frequency" : "frequency";
            string contactGuid = caseSensitive ? "ContactGuid" : "contactguid";
            string weight = caseSensitive ? "Weight" : "weight";
            string homeAddress = caseSensitive ? "HomeAddress" : "homeADDRESS";
            string addresses = caseSensitive ? "Addresses" : "addresses";

            // in the controller, we will verify the following content to make sure it works fine.
            // if you change the content, please change the verification codes in the controller as well.
            string content = $@"
{{
    '{frequency}': 'BiWeekly',
    '{contactGuid}': '21EC2020-3AEA-1069-A2DD-08002B30309D',
    '{weight}': 6.24,
    '{homeAddress}': {{ 'Street':'YouStreet','City':'YouCity'}},
    '{addresses}':[
        {{ 'Street':'Street-3','City':'City-3'}},
        {{ 'Street':'Street-4','City':'City-4'}}
    ]
}}";
            var requestUri = "odata/Bills/2";
            HttpClient client = CreateClient();

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, requestUri);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = content.Length;

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                string.Format("Response status code, expected: {0}, actual: {1}, request url: {2}",
                HttpStatusCode.OK, response.StatusCode, requestUri));

            string responseString = await response.Content.ReadAsStringAsync();

            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":false}", responseString);
        }
    }
}
