// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.Validation;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class DeltaOfTValidationTests : WebApiTestBase<DeltaOfTValidationTests>
    {

        public DeltaOfTValidationTests(WebApiTestFixture<DeltaOfTValidationTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                services.ConfigureControllers(typeof(PatchCustomersController));
                services.AddControllers().AddOData(opt => opt.AddModel("odata", GetModel()));
            };
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<PatchCustomer> patchCustomer = builder.EntitySet<PatchCustomer>("PatchCustomers");
            patchCustomer.EntityType.Property(p => p.ExtraProperty).IsRequired();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest, "The field ExtraProperty must match the regular expression 'Some value'")]
        [InlineData(HttpStatusCode.OK, "")]
        public async Task CanValidatePatches(HttpStatusCode statusCode, string message)
        {
            // Arrange
            object payload = null;
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    payload = new { Id = 5, Name = "Some name", ExtraProperty = "Another value" };
                    break;

                case HttpStatusCode.OK:
                    payload = new { };
                    break;
            }
            string payloadStr = JsonSerializer.Serialize(payload);

            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "odata/PatchCustomers(5)");
            request.Content = new StringContent(payloadStr);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payloadStr.Length;

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(statusCode, response.StatusCode);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var result = await response.Content.ReadAsStringAsync();
                Assert.Contains(message, result);
            }
        }

    }

}
