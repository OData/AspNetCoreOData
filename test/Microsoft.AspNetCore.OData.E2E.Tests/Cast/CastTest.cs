// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Cast
{
    public class CastTest : WebODataTestBase<CastTest.CastTestStartup>
    {
        public class CastTestStartup : TestStartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                // Use the sql server got the access error.
                // services.AddDbContext<ProductsContext>(opt => opt.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=CastProductsContext;Trusted_Connection=True;"));
                //services.AddDbContext<ProductsContext>(opt => opt.UseInMemoryDatabase("CastProductsContext"));

                services.AddControllers()
                    .ConfigureApplicationPartManager(pm =>
                    {
                        pm.FeatureProviders.Add(new WebODataControllerFeatureProvider(typeof(ProductsController), typeof(MetadataController)));
                    });

                IEdmModel edmModel = CastEdmModel.GetEdmModel();

                services.AddOData(opt =>
                {
                    opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5);

                    foreach (string dataSourceType in CastTest.dataSourceTypes)
                    {
                        opt.AddModel(dataSourceType, edmModel);
                    }
                });
            }
        }

        private static string[] dataSourceTypes = new string[] { "IM", "EF" };// In Memory and Entity Framework

        public CastTest(WebODataTestFixture<CastTestStartup> factory)
            :base(factory)
        {
        }

        public static TheoryDataSet<string, string, int> Combinations
        {
            get
            {
                var combinations = new TheoryDataSet<string, string, int>();
                foreach (string dataSourceType in dataSourceTypes)
                {
                    // To Edm.String
                    combinations.Add(dataSourceType, "?$filter=cast('Name1',Edm.String) eq Name", 1);
                    combinations.Add(dataSourceType, "?$filter=contains(cast(Name,Edm.String),'Name')", 6);
                    combinations.Add(dataSourceType, "?$filter=cast(Microsoft.Test.E2E.AspNet.OData.Cast.Domain'Civil',Edm.String) eq '2'", 6);
                    combinations.Add(dataSourceType, "?$filter=cast(Domain,Edm.String) eq '3'", 2);
                    combinations.Add(dataSourceType, "?$filter=cast(ID,Edm.String) gt '1'", 5);
                    // TODO bug 1889: Cast function reports error if it is used against a collection of primitive value.
                    // Delete $it after the bug if fixed.
                    combinations.Add(dataSourceType, "(1)/DimensionInCentimeter?$filter=cast($it,Edm.String) gt '1'", 2);
                    combinations.Add(dataSourceType, "?$filter=cast(Weight,Edm.String) gt '1.1'", 5);
                    combinations.Add(dataSourceType, "?$filter=contains(cast(ManufacturingDate,Edm.String),'2011')", 1);
                    // TODO bug 1982: The result of casting a value of DateTimeOffset to String is not always the literal representation used in payloads
                    // combinations.Add(dataSourceType, "?$filter=contains(cast(2011-01-01T00:00:00%2B08:00,Edm.String),'2011-01-01')", 3);

                    // To Edm.Int32
                    combinations.Add(dataSourceType, "?$filter=cast(Weight,Edm.Int32) eq 1", 1);
                    combinations.Add(dataSourceType, "?$filter=cast(cast(Name,Edm.Int32),Edm.Int32) eq null", 6);

                    // To DateTimeOffset
                    combinations.Add(dataSourceType, "?$filter=cast(ManufacturingDate,Edm.DateTimeOffset) eq 2011-01-01T00:00:00%2B08:00", 1);
                    combinations.Add(dataSourceType, "?$filter=cast(null,Edm.DateTimeOffset) eq null", 6);

                    // To Enum
                    combinations.Add(dataSourceType, "?$filter=cast('Both',Microsoft.Test.E2E.AspNet.OData.Cast.Domain) eq Domain", 2);
                    combinations.Add(dataSourceType, "?$filter=cast('1',Microsoft.Test.E2E.AspNet.OData.Cast.Domain) eq Domain", 2);
                    combinations.Add(dataSourceType, "?$filter=cast(null,Microsoft.Test.E2E.AspNet.OData.Cast.Domain) eq Domain", 0);

                    //To Derived Structured Types
                    combinations.Add(dataSourceType, "?$filter=cast('Microsoft.Test.E2E.AspNet.OData.Cast.AirPlane')/Speed eq 100", 2);
                    combinations.Add(dataSourceType, "?$filter=cast('Microsoft.Test.E2E.AspNet.OData.Cast.AirPlane')/Speed eq 500", 1);
                    combinations.Add(dataSourceType, "?$filter=cast('Microsoft.Test.E2E.AspNet.OData.Cast.JetPlane')/Company eq 'Boeing'", 1);
                }

                return combinations;
            }
        }

        [Theory]
        [MemberData(nameof(Combinations))]
        public async Task Cast_Query_WorksAsExpected(string dataSourceMode, string dollarFormat, int expectedEntityCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/Products{1}", dataSourceMode, dollarFormat);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            JObject responseString = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                string.Format("Response status code, expected: {0}, actual: {1}, request url: {2}",
                HttpStatusCode.OK, response.StatusCode, requestUri));

            JArray value = responseString["value"] as JArray;
            Assert.True(expectedEntityCount == value.Count,
                string.Format("The entity count in response, expected: {0}, actual: {1}, request url: {2}",
                expectedEntityCount, value.Count, requestUri));
        }
    }
}
