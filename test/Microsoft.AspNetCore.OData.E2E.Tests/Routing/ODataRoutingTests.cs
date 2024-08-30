//-----------------------------------------------------------------------------
// <copyright file="ODataRoutingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing;

public class ODataRoutingTests : WebApiTestBase<ODataRoutingTests>
{
    public ODataRoutingTests(WebApiTestFixture<ODataRoutingTests> fixture)
       : base(fixture)
    {
    }

    // following the Fixture convention.
    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(ProductsController), typeof(CategoriesController));

        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<Product>("Products");
        builder.EntitySet<Category>("Categories");
        var model = builder.GetEdmModel();

        services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", model));
    }

    [Theory]
    [InlineData("1", true, "\"Id\":1,\"Name\":\"OData Product\"}")]
    [InlineData("11", true, "\"Id\":11,\"Name\":\"OData Product\"}")]
    [InlineData("42", true, "\"Id\":42,\"Name\":\"OData Product\"}")]
    [InlineData("other", false, "{\"id\":11,\"name\":\"Non-OData Product\"}")]
    public async Task RequestContainsValidValues_CanRouteToCorrectEndpoint(string key, bool isODataRouting, string expect)
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        var response = await client.GetAsync($"/odata/products/{key}");

        // Assert
        response.EnsureSuccessStatusCode();

        string payload = await response.Content.ReadAsStringAsync();
        if (isODataRouting)
        {
            expect = $"{{\"@odata.context\":\"http://localhost/odata/$metadata#Products/$entity\",{expect}";
        }

        Assert.Equal(expect, payload);
    }

    [Theory]
    [InlineData("1", true, "\"Id\":1,\"Name\":\"OData Category\"}")]
    [InlineData("11", true, "\"Id\":11,\"Name\":\"OData Category\"}")]
    [InlineData("42", true, "\"Id\":42,\"Name\":\"OData Category\"}")]
    [InlineData("other", false, "{\"id\":11,\"name\":\"Non-OData Category\"}")]
    public async Task RequestContainsValidValues_CanRouteToCorrectEndpoint_UsingDifferentOrder(string key, bool isODataRouting, string expect)
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        var response = await client.GetAsync($"/odata/categories/{key}");

        // Assert
        response.EnsureSuccessStatusCode();

        string payload = await response.Content.ReadAsStringAsync();
        if (isODataRouting)
        {
            expect = $"{{\"@odata.context\":\"http://localhost/odata/$metadata#Categories/$entity\",{expect}";
        }

        Assert.Equal(expect, payload);
    }

    [Theory]
    [InlineData("odata/products/\"1\"")]
    [InlineData("odata/products/\"other\"")]
    [InlineData("odata/products/other1")]
    [InlineData("odata/products/a1other")]
    [InlineData("odata/categories/\"1\"")]
    [InlineData("odata/categories/\"other\"")]
    [InlineData("odata/categories/other1")]
    [InlineData("odata/categories/a1other")]
    public async Task RequestContainsInValidValues_CanRouteToCorrectEndpoint(string request)
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        var response = await client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class ProductsController : ODataController
{
    // This method will have the following routing:
    // ~/odata/products({key})
    // ~/odata/products/{key}
    public IActionResult Get(int key)
    {
        return Ok(new Product { Id = key, Name = "OData Product" });
    }

    // Be NOTED: "GetOther" is added after Get() by test, don't change the order
    [HttpGet("/odata/products/other")]
    public IActionResult GetOther()
    {
        return Ok(new Product { Id = 11, Name = "Non-OData Product" });
    }
}

public class CategoriesController : ODataController
{
    // Be NOTED: "GetOther" is added before Get() by test, don't change the order
    [HttpGet("/odata/categories/other")]
    public IActionResult GetOther()
    {
        return Ok(new Category { Id = 11, Name = "Non-OData Category" });
    }

    // This method will have the following routing:
    // ~/odata/categories({key})
    // ~/odata/categories/{key}
    public IActionResult Get(int key)
    {
        return Ok(new Category { Id = key, Name = "OData Category"});
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class  Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}
