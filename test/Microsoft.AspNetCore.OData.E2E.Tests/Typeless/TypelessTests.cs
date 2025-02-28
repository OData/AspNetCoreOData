//-----------------------------------------------------------------------------
// <copyright file="TypelessTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless;

public class TypelessTests : WebApiTestBase<TypelessTests>
{
    public TypelessTests(WebApiTestFixture<TypelessTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var model = TypelessEdmModel.GetModel();

        services.ConfigureControllers(typeof(TypelessOrdersController), typeof(TypelessDeltaController), typeof(TypedDeltaController));
        services.AddControllers().AddOData(
            options => options.EnableQueryFeatures().AddRouteComponents(TypelessEdmModel.GetModel()));
    }

    [Theory]
    [InlineData("TypelessDelta/GetChanges()")]
    [InlineData("TypedDelta/GetChanges()")] // Typed scenario included for comparison
    public async Task TestPropertiesNotSetInDeltaAreNotIncludedInPayloadAsync(string requestUri)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.EndsWith("/$metadata#ChangeSets/$delta\",\"value\":[" +
            "{\"Id\":1," +
            "\"Changed\":{" +
            "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Typeless.Customer\"," +
            "\"Id\":1," +
            "\"Orders@delta\":[{\"Id\":1},{\"@odata.removed\":{\"reason\":\"deleted\"},\"@odata.id\":\"http://tempuri.org/Orders(2)\",\"Id\":2}]}}," +
            "{\"Id\":2," +
            "\"Changed\":{" +
            "\"@odata.type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.Typeless.Order\"," +
            "\"Id\":1," +
            "\"Amount\":310}}]}",
            content);
    }

    [Fact]
    public async Task TestNavigationPropertyNotAutoExpandedAsync()
    {
        // Arrange
        var requestUri = "Orders";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.EndsWith("/$metadata#Orders\",\"value\":[" +
            "{\"Id\":1,\"Amount\":310,\"OrderDate\":\"2025-02-07T11:59:59Z\"}," +
            "{\"Id\":2,\"Amount\":290,\"OrderDate\":\"2025-02-14T11:59:59Z\"}]}",
            content);
    }

    [Fact]
    public async Task TestNavigationPropertyExpandedAsync()
    {
        // Arrange
        var requestUri = "Orders?$expand=Customer";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.EndsWith("/$metadata#Orders(Customer())\",\"value\":[" +
            "{\"Id\":1,\"Amount\":310,\"OrderDate\":\"2025-02-07T11:59:59Z\",\"Customer\":{\"Id\":1,\"Name\":\"Sue\",\"CreditLimit\":1300}}," +
            "{\"Id\":2,\"Amount\":290,\"OrderDate\":\"2025-02-14T11:59:59Z\",\"Customer\":{\"Id\":2,\"Name\":\"Joe\",\"CreditLimit\":1700}}]}",
            content);
    }

    [Fact]
    public async Task TestNavigationPropertyExpandedWithNestedSelectAsync()
    {
        // Arrange
        var requestUri = "Orders?$expand=Customer($select=Name)";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.EndsWith("/$metadata#Orders(Customer(Name))\",\"value\":[" +
            "{\"Id\":1,\"Amount\":310,\"OrderDate\":\"2025-02-07T11:59:59Z\",\"Customer\":{\"Name\":\"Sue\"}}," +
            "{\"Id\":2,\"Amount\":290,\"OrderDate\":\"2025-02-14T11:59:59Z\",\"Customer\":{\"Name\":\"Joe\"}}]}",
            content);
    }

    [Fact]
    public async Task TestSelectAndNavigationPropertyExpandedWithNestedSelectAsync()
    {
        // Arrange
        var requestUri = "Orders(1)?$select=Id,Amount&$expand=Customer($select=Id,CreditLimit)";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.EndsWith("/$metadata#Orders(Id,Amount,Customer(Id,CreditLimit))/$entity\"," +
            "\"Id\":1,\"Amount\":310,\"Customer\":{\"Id\":1,\"CreditLimit\":1300}}",
            content);
    }
}
