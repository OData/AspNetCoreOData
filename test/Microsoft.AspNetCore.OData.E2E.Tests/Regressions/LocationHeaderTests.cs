//-----------------------------------------------------------------------------
// <copyright file="LocationHeaderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Regressions;

public class LocationHeaderTests : WebApiTestBase<LocationHeaderTests>
{
    public LocationHeaderTests(WebApiTestFixture<LocationHeaderTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel edmModel = GetEdmModel();
        services.ConfigureControllers(typeof(HandleController));
        services.AddControllers().AddOData(opt => opt.AddRouteComponents("location", edmModel));
    }

    [Fact]
    public async Task CreateCustomerWithSingleKey_ReturnsCorrectLocationHeaderEscapedUri()
    {
        // Arrange
        string payload = @"{""Name"":""abc""}";
        var postContent = new StringContent(payload);
        postContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        postContent.Headers.ContentLength = payload.Length;

        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsync("location/customers", postContent);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Content);

        Assert.True(response.Headers.Contains("Location"), "The response should contain Header 'Location'");

        string locationHeader = response.Headers.GetValues("Location").Single();

        Assert.Equal("http://localhost/location/Customers('abc%2F%24%2B%2F-8')", locationHeader);
    }

    [Fact]
    public async Task CreateOrderWithCompositeKey_ReturnsCorrectLocationHeaderEscapedUri()
    {
        // Arrange
        string payload = @"{""Title"":""xzy""}";
        var postContent = new StringContent(payload);
        postContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        postContent.Headers.ContentLength = payload.Length;

        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsync("location/orders", postContent);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Content);

        Assert.True(response.Headers.Contains("Location"), "The response should contain Header 'Location'");

        string locationHeader = response.Headers.GetValues("Location").Single();

        Assert.Equal("http://localhost/location/Orders(Id1='xzy%2F',Id2='%2Fxzy')", locationHeader);
    }

    public static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<LocCustomer>("Customers");
        builder.EntitySet<LocOrder>("Orders");
        return builder.GetEdmModel();
    }
}

public class HandleController : ODataController
{
    [HttpPost("location/customers")]
    public IActionResult CreateCustomer([FromBody]LocCustomer customer)
    {
        customer.Id = $"{customer.Name}/$+/-8"; // insert slash middle
        return Created(customer);
    }

    [HttpPost("location/orders")]
    public IActionResult CreateOrder([FromBody] LocOrder order)
    {
        order.Id1 = $"{order.Title}/"; // append slash at end
        order.Id2 = $"/{order.Title}"; // insert slash at beginning
        return Created(order);
    }
}

public class LocCustomer
{
    public string Id { get; set; }

    public string Name { get; set; }
}

public class LocOrder
{
    [Key]
    public string Id1 { get; set; }

    [Key]
    public string Id2 { get; set; }

    public string Title { get; set; }
}
