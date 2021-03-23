// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance
{
    public class ComplexTypeInheritanceSerializeTest : WebApiTestBase<ComplexTypeInheritanceSerializeTest>
    {
        public ComplexTypeInheritanceSerializeTest(WebApiTestFixture<ComplexTypeInheritanceSerializeTest> fixture)
            :base(fixture)
        {
        }

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(InheritanceCustomersController));

            var edmModel1 = GetEdmModel();
            services.AddOData(opt => opt.AddModel("odata", edmModel1));
        }

        [Fact]
        public async Task CanQueryInheritanceComplexInComplexProperty()
        {
            // Arrange
            string requestUri = "odata/InheritanceCustomers?$format=application/json;odata.metadata=full";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.OK,
                response.StatusCode,
                requestUri,
                contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(2, contentOfJObject.Count);
            Assert.Equal(5, contentOfJObject["value"].Count());

            Assert.Equal(new[]
            {
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.InheritanceAddress",
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.InheritanceAddress",
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.InheritanceUsAddress",
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.InheritanceCnAddress",
                "#Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.InheritanceCnAddress"
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

    public class InheritanceCustomersController : ODataController
    {
        private readonly IList<InheritanceCustomer> _customers;
        public InheritanceCustomersController()
        {
            InheritanceAddress address = new InheritanceAddress
            {
                City = "Tokyo",
                Street = "Tokyo Rd"
            };

            InheritanceAddress usAddress = new InheritanceUsAddress
            {
                City = "Redmond",
                Street = "One Microsoft Way",
                ZipCode = 98052
            };

            InheritanceAddress cnAddress = new InheritanceCnAddress
            {
                City = "Shanghai",
                Street = "ZiXing Rd",
                PostCode = "200241"
            };

            _customers = Enumerable.Range(1, 5).Select(e =>
                new InheritanceCustomer
                {
                    Id = e,
                    Location = new InheritanceLocation
                    {
                        Name = "Location #" + e,
                        Address = e < 3 ? address : e < 4 ? usAddress : cnAddress
                    }
                }).ToList();
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_customers);
        }
    }

    public class InheritanceCustomer
    {
        public int Id { get; set; }

        public InheritanceLocation Location { get; set; }
    }

    public class InheritanceLocation
    {
        public string Name { get; set; }

        public InheritanceAddress Address { get; set; }
    }

    public class InheritanceAddress
    {
        public string City { get; set; }

        public string Street { get; set; }
    }

    public class InheritanceUsAddress : InheritanceAddress
    {
        public int ZipCode { get; set; }
    }

    public class InheritanceCnAddress : InheritanceAddress
    {
        public string PostCode { get; set; }
    }
}
