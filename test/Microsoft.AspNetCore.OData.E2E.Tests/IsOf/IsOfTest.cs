//-----------------------------------------------------------------------------
// <copyright file="CastTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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

namespace Microsoft.AspNetCore.OData.E2E.Tests.IsOf;

public class IsOfTest : WebODataTestBase<IsOfTest.IsOfTestStartup>
{
    public class IsOfTestStartup : TestStartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Use the sql server got the access error.
            services.AddDbContext<ProductsContext>(opt => opt.UseInMemoryDatabase("CastProductsContext"));

            services.ConfigureControllers(typeof(ProductsController), typeof(MetadataController));

            IEdmModel edmModel = IsOfEdmModel.GetEdmModel();

            services.AddControllers().AddOData(opt =>
            {
                opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5);

                foreach (string dataSourceType in IsOfTest.dataSourceTypes)
                {
                    opt.AddRouteComponents(dataSourceType, edmModel);
                }
            });
        }
    }

    private static string[] dataSourceTypes = new string[] { "IM", "EF" };// In Memory and Entity Framework

    public IsOfTest(WebODataTestFixture<IsOfTestStartup> factory)
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
                // Unquoted type parameter
                combinations.Add(dataSourceType, "(1)/DimensionInCentimeter?$filter=isof($it,Edm.Int32)", 3);
                combinations.Add(dataSourceType, "?$filter=isof(Microsoft.AspNetCore.OData.E2E.Tests.IsOf.Product)", 6);
                combinations.Add(dataSourceType, "?$filter=isof(Microsoft.AspNetCore.OData.E2E.Tests.IsOf.AirPlane)", 3);
                combinations.Add(dataSourceType, "?$filter=isof(Microsoft.AspNetCore.OData.E2E.Tests.IsOf.JetPlane)", 2);
                combinations.Add(dataSourceType, "?$filter=isof(Location,Microsoft.AspNetCore.OData.E2E.Tests.IsOf.MyAddress)", 6);
                combinations.Add(dataSourceType, "?$filter=isof(Location,Microsoft.AspNetCore.OData.E2E.Tests.IsOf.MyOtherAddress)", 3);

                // Quoted type parameter
                combinations.Add(dataSourceType, "(1)/DimensionInCentimeter?$filter=isof($it,'Edm.Int32')", 3);
                combinations.Add(dataSourceType, "?$filter=isof('Microsoft.AspNetCore.OData.E2E.Tests.IsOf.Product')", 6);
                combinations.Add(dataSourceType, "?$filter=isof('Microsoft.AspNetCore.OData.E2E.Tests.IsOf.AirPlane')", 3);
                combinations.Add(dataSourceType, "?$filter=isof('Microsoft.AspNetCore.OData.E2E.Tests.IsOf.JetPlane')", 2);
                combinations.Add(dataSourceType, "?$filter=isof(Location,'Microsoft.AspNetCore.OData.E2E.Tests.IsOf.MyAddress')", 6);
                combinations.Add(dataSourceType, "?$filter=isof(Location,'Microsoft.AspNetCore.OData.E2E.Tests.IsOf.MyOtherAddress')", 3);
            }

            return combinations;
        }
    }

    [Theory]
    [MemberData(nameof(Combinations))]
    public async Task Cast_Query_WorksAsExpected(string dataSourceMode, string dollarFilter, int expectedEntityCount)
    {
        // Arrange
        var requestUri = string.Format("{0}/Products{1}", dataSourceMode, dollarFilter);

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
