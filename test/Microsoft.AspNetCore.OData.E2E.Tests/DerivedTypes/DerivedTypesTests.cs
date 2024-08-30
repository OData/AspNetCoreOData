//-----------------------------------------------------------------------------
// <copyright file="DerivedTypesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes;

public class DerivedTypeTests : WebApiTestBase<DerivedTypeTests>
{
    public DerivedTypeTests(WebApiTestFixture<DerivedTypeTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(CustomersController));
        services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", GetEdmModel()).Count().Filter().OrderBy().Expand().SetMaxTop(null).Select());
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<Customer>("Customers");
        builder.EntitySet<Customer>("Employees");
        builder.EntityType<Order>();
        builder.EntityType<VipCustomer>().DerivesFrom<Customer>();
        builder.EntityType<EnterpriseCustomer>().DerivesFrom<Customer>();
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task RestrictEntitySetToDerivedTypeInstances()
    {
        // Arrange
        string requestUri = "/odata/Customers/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.VipCustomer";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);
        string py = await response.Content.ReadAsStringAsync();
        // Assert
        Assert.True(response.IsSuccessStatusCode);

        string expectedContent = "\"value\":[{\"Id\":2,\"Name\":\"Customer 2\",\"LoyaltyCardNo\":\"9876543210\"}]";
        Assert.Contains(expectedContent, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("Customers(2)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.VipCustomer")]
    [InlineData("Customers/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.VipCustomer(2)")] // So far, we don't support key after the type cast.
    public async Task RestrictEntityToDerivedTypeInstance(string path)
    {
        // Arrange: Key preceeds name of the derived type
        string requestUri = $"odata/{path}";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        string expectedContent = "\"Id\":2,\"Name\":\"Customer 2\",\"LoyaltyCardNo\":\"9876543210\"";
        Assert.Contains(expectedContent, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReturnNotFound_ForKeyNotAssociatedWithDerivedType()
    {
        // Arrange: Customer with Id 1 is not a VipCustomer
        string requestUri = "/odata/Customers(1)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.VipCustomer";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestrictEntitySetToDerivedTypeInstances_ThenExpandNavProperty()
    {
        // Arrange
        string requestUri = "/odata/Customers/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.VipCustomer?$expand=Orders";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        string expectedContent = "\"value\":[{\"Id\":2,\"Name\":\"Customer 2\",\"LoyaltyCardNo\":\"9876543210\"," +
            "\"Orders\":[{\"Id\":2,\"Amount\":230},{\"Id\":3,\"Amount\":150}]}]";
        Assert.Contains(expectedContent, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("Customers(2)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.VipCustomer?$expand=Orders")]
    [InlineData("Customers/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.VipCustomer(2)?$expand=Orders")]
    public async Task RestrictEntityToDerivedTypeInstance_ThenExpandNavProperty(string pathAndQuery)
    {
        // Arrange: Key preceeds name of the derived type
        string requestUri = $"/odata/{pathAndQuery}";

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        string expectedContent = "\"Id\":2,\"Name\":\"Customer 2\",\"LoyaltyCardNo\":\"9876543210\"," +
            "\"Orders\":[{\"Id\":2,\"Amount\":230},{\"Id\":3,\"Amount\":150}]";
        Assert.Contains(expectedContent, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("Customers(4)")]
    [InlineData("Customers(4)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer")]
    public async Task RequestFullMetadataForDerivedTypeInstance(string odataPath)
    {
        // Arrange
        var requestUri = $"/odata/{odataPath}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var enterpriseCustomer = await response.Content.ReadAsObject<JObject>();

        Assert.Equal(10, enterpriseCustomer.Count);

        var odataContext = enterpriseCustomer.Value<string>("@odata.context");
        var odataType = enterpriseCustomer.Value<string>("@odata.type");
        var odataId = enterpriseCustomer.Value<string>("@odata.id");
        var odataEditLink = enterpriseCustomer.Value<string>("@odata.editLink");
        var id = enterpriseCustomer.Value<int?>("Id");
        var name = enterpriseCustomer.Value<string>("Name");
        var relationshipManagerAssociationLink = enterpriseCustomer.Value<string>("RelationshipManager@odata.associationLink");
        var relationshipManagerNavigationLink = enterpriseCustomer.Value<string>("RelationshipManager@odata.navigationLink");

        Assert.NotNull(odataContext);
        Assert.NotNull(odataType);
        Assert.NotNull(odataId);
        Assert.NotNull(odataEditLink);
        Assert.NotNull(id);
        Assert.NotNull(name);
        Assert.NotNull(relationshipManagerAssociationLink);
        Assert.NotNull(relationshipManagerNavigationLink);

        Assert.EndsWith("$metadata#Customers/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer/$entity", odataContext);
        Assert.Equal("#Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer", odataType);
        Assert.EndsWith("Customers(4)", odataId);
        Assert.EndsWith("Customers(4)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer", odataEditLink);
        Assert.Equal(4, id);
        Assert.Equal("Customer 4", name);
        Assert.EndsWith("Customers(4)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer/RelationshipManager/$ref", relationshipManagerAssociationLink);
        Assert.EndsWith("Customers(4)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer/RelationshipManager", relationshipManagerNavigationLink);
    }

    [Theory]
    [InlineData("Customers(4)", 4)]
    [InlineData("Customers(4)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer", 3)]
    public async Task RequestMinimalMetadataForDerivedTypeInstance(string odataPath, int propertyCount)
    {
        // Arrange
        var requestUri = $"/odata/{odataPath}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var enterpriseCustomer = await response.Content.ReadAsObject<JObject>();

        Assert.Equal(propertyCount, enterpriseCustomer.Count);

        var odataContext = enterpriseCustomer.Value<string>("@odata.context");
        var id = enterpriseCustomer.Value<int?>("Id");
        var name = enterpriseCustomer.Value<string>("Name");

        Assert.NotNull(odataContext);
        Assert.NotNull(id);
        Assert.NotNull(name);

        Assert.EndsWith("$metadata#Customers/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer/$entity", odataContext);
        Assert.Equal(4, id);
        Assert.Equal("Customer 4", name);
    }

    [Theory]
    [InlineData("Customers(4)")]
    [InlineData("Customers(4)/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.EnterpriseCustomer")]
    public async Task RequestNoMetadataForDerivedTypeInstance(string odataPath)
    {
        // Arrange
        var requestUri = $"/odata/{odataPath}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
        var client = CreateClient();

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var enterpriseCustomer = await response.Content.ReadAsObject<JObject>();

        Assert.Equal(2, enterpriseCustomer.Count);

        var id = enterpriseCustomer.Value<int?>("Id");
        var name = enterpriseCustomer.Value<string>("Name");

        Assert.NotNull(id);
        Assert.NotNull(name);

        Assert.Equal(4, id);
        Assert.Equal("Customer 4", name);
    }
}
