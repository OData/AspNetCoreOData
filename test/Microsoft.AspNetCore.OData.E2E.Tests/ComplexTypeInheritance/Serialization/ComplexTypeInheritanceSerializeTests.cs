// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Serialization;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class ComplexTypeInheritanceSerializationTests : WebApiTestBase<ComplexTypeInheritanceSerializationTests>
    {

        public ComplexTypeInheritanceSerializationTests(WebApiTestFixture<ComplexTypeInheritanceSerializationTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (services) =>
            {
                services.ConfigureControllers(typeof(InheritanceCustomersController));

                var edmModel1 = GetEdmModel();
                services.AddControllers().AddOData(opt => opt.AddModel("odata", edmModel1));
            };
        }

        // following the Fixture convention.

        [Fact]
        public async Task CanQueryInheritanceComplexInComplexProperty()
        {
            // Arrange
            string requestUri = "odata/InheritanceCustomers?$format=application/json;odata.metadata=full";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string responseContent = await response.Content.ReadAsStringAsync();
            Output.WriteLine(responseContent);

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.OK,
                response.StatusCode,
                requestUri,
                responseContent));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(2, contentOfJObject.Count);
            Assert.Equal(5, contentOfJObject["value"].Count());

            Assert.Equal(new[]
            {
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Serialization.InheritanceAddress",
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Serialization.InheritanceAddress",
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Serialization.InheritanceUsAddress",
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Serialization.InheritanceCnAddress",
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Serialization.InheritanceCnAddress"
            },
            contentOfJObject["value"].Select(e => e["Location"]["Address"]["@odata.type"]).Select(c => (string)c));
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<InheritanceCustomer>("InheritanceCustomers");
            builder.ComplexType<InheritanceLocation>();
            return builder.GetEdmModel();
        }

    }

}
