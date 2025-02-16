//-----------------------------------------------------------------------------
// <copyright file="UntypedTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DynamicModel;

public class DynamicModelTests : WebApiTestBase<DynamicModelTests>
{
    public DynamicModelTests(WebApiTestFixture<DynamicModelTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel edmModel = DynamicEdmModel.GetEdmModel();

        services.ConfigureControllers(typeof(DynamicController), typeof(MetadataController));
        services.AddSingleton<DynamicDataSource>();

        services.AddControllers().AddOData(opt =>
            opt.EnableQueryFeatures()
                .AddRouteComponents("odata", edmModel));
    }

    [Theory]
    [InlineData("",
        "{\"@odata.context\":\"http://localhost/odata/$metadata#Products\",\"value\":[{\"Name\":\"abc\",\"ID\":1},{\"Name\":\"def\",\"ID\":2}]}")]
    [InlineData("?$expand=ContainedDetailInfo($select=Title)",
        "{\"@odata.context\":\"http://localhost/odata/$metadata#Products(ContainedDetailInfo(Title))\",\"value\":[{\"Name\":\"abc\",\"ID\":1,\"ContainedDetailInfo\":{\"Title\":\"abc_containeddetailinfo\"}},{\"Name\":\"def\",\"ID\":2,\"ContainedDetailInfo\":{\"Title\":\"def_containeddetailinfo\"}}]}")]
    [InlineData("?$select=Name",
        "{\"@odata.context\":\"http://localhost/odata/$metadata#Products(Name)\",\"value\":[{\"Name\":\"abc\"},{\"Name\":\"def\"}]}")]
    public async Task QueryUntypedEntitySet(string options, string expected)
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"odata/Products{options}");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        string payloadBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(expected, payloadBody);
    }

    [Theory]
    [InlineData("",
        "{\"@odata.context\":\"http://localhost/odata/$metadata#Products/$entity\",\"Name\":\"abc\",\"ID\":1}")]
    [InlineData("?$expand=ContainedDetailInfo($select=Title)",
        "{\"@odata.context\":\"http://localhost/odata/$metadata#Products(ContainedDetailInfo(Title))/$entity\",\"Name\":\"abc\",\"ID\":1,\"ContainedDetailInfo\":{\"Title\":\"abc_containeddetailinfo\"}}")]
    [InlineData("?$select=Name",
        "{\"@odata.context\":\"http://localhost/odata/$metadata#Products(Name)/$entity\",\"Name\":\"abc\"}")]
    public async Task QueryUntypedEntity(string options, string expected)
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"odata/Products(1){options}");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);

        string payloadBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(expected, payloadBody);
    }
}