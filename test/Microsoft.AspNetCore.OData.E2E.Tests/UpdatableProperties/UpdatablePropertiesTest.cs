//-----------------------------------------------------------------------------
// <copyright file="UpdatablePropertiesTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UpdatableProperties;

public class UpdatablePropertiesTest : WebApiTestBase<UpdatablePropertiesTest>
{
    public UpdatablePropertiesTest(WebApiTestFixture<UpdatablePropertiesTest> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var edmModel = GetEdmModel();
        services.ConfigureControllers(typeof(MetadataController), typeof(CustomersController));
        services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", edmModel));
    }

    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Customer>("Customers");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task Patch_LeavesNestedResourceUnchanged_WhenItIsRemovedFromUpdatableProperties()
    {
        // Arrange
        var content = @"
{
'Name':'Updated Name',
'Address':{ 'City':'Berlin','Street':'Alexanderplatz'}
}";
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, "odata/Customers/1");
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = content.Length;

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();

        // The scalar property in the list is applied.
        Assert.Contains("Updated Name", responseString);

        // The nested resource keeps its original values.
        Assert.Contains("Redmond", responseString);
        Assert.Contains("One Microsoft Way", responseString);
        Assert.DoesNotContain("Berlin", responseString);
        Assert.DoesNotContain("Alexanderplatz", responseString);
    }

    [Fact]
    public async Task Patch_UpdatesNestedResource_WhenItIsInUpdatableProperties()
    {
        // Arrange
        var content = @"
{
'Name':'Updated Name',
'Address':{ 'City':'Berlin','Street':'Alexanderplatz'}
}";
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, "odata/Customers/2");
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = content.Length;

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();

        // Both the scalar and the nested resource are applied.
        Assert.Contains("Updated Name", responseString);
        Assert.Contains("Berlin", responseString);
        Assert.Contains("Alexanderplatz", responseString);
        Assert.DoesNotContain("Redmond", responseString);
    }

    [Fact]
    public async Task Put_LeavesNestedResourceUnchanged_WhenItIsRemovedFromUpdatableProperties()
    {
        // Arrange
        var content = @"
{
'Id':1,
'Name':'Updated Name',
'Address':{ 'City':'Berlin','Street':'Alexanderplatz'}
}";
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Put, "odata/Customers/1");
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = content.Length;

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();

        // The scalar property in the list is applied, the nested resource is neither replaced nor reset.
        Assert.Contains("Updated Name", responseString);
        Assert.Contains("Redmond", responseString);
        Assert.Contains("One Microsoft Way", responseString);
        Assert.DoesNotContain("Berlin", responseString);
        Assert.DoesNotContain("Alexanderplatz", responseString);
    }

    [Fact]
    public async Task Put_UpdatesNestedResource_WhenItIsInUpdatableProperties()
    {
        // Arrange
        var content = @"
{
'Id':2,
'Name':'Updated Name',
'Address':{ 'City':'Berlin','Street':'Alexanderplatz'}
}";
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Put, "odata/Customers/2");
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = content.Length;

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();

        // Both the scalar and the nested resource are applied.
        Assert.Contains("Updated Name", responseString);
        Assert.Contains("Berlin", responseString);
        Assert.Contains("Alexanderplatz", responseString);
        Assert.DoesNotContain("Redmond", responseString);
    }

    [Fact]
    public async Task Patch_LeavesNavigationPropertyUnchanged_WhenItIsRemovedFromUpdatableProperties()
    {
        // Arrange - key 3 removes the single-valued Order navigation property from the updatable set.
        var content = @"
{
'Name':'Updated Name',
'Order':{ 'Description':'Hacked Order','Amount':999}
}";
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, "odata/Customers/3");
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = content.Length;

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();

        // The scalar property is applied.
        Assert.Contains("Updated Name", responseString);

        // The navigation property keeps its original value.
        Assert.Contains("Original Order", responseString);
        Assert.DoesNotContain("Hacked Order", responseString);
    }

    [Fact]
    public async Task Patch_UpdatesNavigationProperty_WhenItIsInUpdatableProperties()
    {
        // Arrange - key 2 applies the delta with the full set of properties.
        var content = @"
{
'Order':{ 'Description':'Updated Order','Amount':555}
}";
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, "odata/Customers/2");
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = content.Length;

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();

        // The navigation property is applied.
        Assert.Contains("Updated Order", responseString);
        Assert.DoesNotContain("Original Order", responseString);
    }

    [Fact]
    public async Task Patch_AppliesOnlyAllowListedProperty_WhenUpdatablePropertiesClearedAndReAdded()
    {
        // Arrange - key 4 clears the updatable set and re-adds only Name.
        var content = @"
{
'Name':'Updated Name',
'Address':{ 'City':'Berlin','Street':'Alexanderplatz'}
}";
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, "odata/Customers/4");
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = content.Length;

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();

        // Only the allow-listed scalar property is applied; the nested resource is left unchanged.
        Assert.Contains("Updated Name", responseString);
        Assert.Contains("Redmond", responseString);
        Assert.DoesNotContain("Berlin", responseString);
        Assert.DoesNotContain("Alexanderplatz", responseString);
    }

    [Fact]
    public async Task Patch_LeavesRemovedNestedResourceUnchanged_WhenMultipleNestedResourcesSent()
    {
        // Arrange - key 1 removes Address but leaves the Order navigation property updatable.
        var content = @"
{
'Name':'Updated Name',
'Address':{ 'City':'Berlin','Street':'Alexanderplatz'},
'Order':{ 'Description':'Updated Order','Amount':555}
}";
        var client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Patch, "odata/Customers/1");
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = content.Length;

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();

        // The scalar property is applied.
        Assert.Contains("Updated Name", responseString);

        // The removed nested resource (Address) keeps its original values.
        Assert.Contains("Redmond", responseString);
        Assert.DoesNotContain("Berlin", responseString);
        Assert.DoesNotContain("Alexanderplatz", responseString);

        // The nested resource still in the list (Order) is applied.
        Assert.Contains("Updated Order", responseString);
        Assert.DoesNotContain("Original Order", responseString);
    }
}
