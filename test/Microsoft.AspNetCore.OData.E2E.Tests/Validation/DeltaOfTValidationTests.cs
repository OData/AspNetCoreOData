//-----------------------------------------------------------------------------
// <copyright file="DeltaOfTValidationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Validation;

public class DeltaOfTValidationTests : WebApiTestBase<DeltaOfTValidationTests>
{
    public DeltaOfTValidationTests(WebApiTestFixture<DeltaOfTValidationTests> fixture)
        :base(fixture)
    {
    }

    // following the Fixture convention.
    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(PatchCustomersController));
        services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", GetModel()));
    }

    private static IEdmModel GetModel()
    {
        ODataModelBuilder builder = new ODataConventionModelBuilder();
        EntitySetConfiguration<PatchCustomer> patchCustomer = builder.EntitySet<PatchCustomer>("PatchCustomers");
        patchCustomer.EntityType.Property(p => p.ExtraProperty).IsRequired();
        return builder.GetEdmModel();
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "The field ExtraProperty must match the regular expression 'Some value'")]
    [InlineData(HttpStatusCode.OK, "")]
    public async Task CanValidatePatches(HttpStatusCode statusCode, string message)
    {
        // Arrange
        object payload = null;
        switch (statusCode)
        {
            case HttpStatusCode.BadRequest:
                payload = new { Id = 5, Name = "Some name", ExtraProperty = "Another value" };
                break;

            case HttpStatusCode.OK:
                payload = new { };
                break;
        }
        string payloadStr = JsonSerializer.Serialize(payload);

        HttpClient client = CreateClient();
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), "odata/PatchCustomers(5)");
        request.Content = new StringContent(payloadStr);
        request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
        request.Content.Headers.ContentLength = payloadStr.Length;

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains(message, result);
        }
    }
}

public class PatchCustomer
{
    public int Id { get; set; }

    public string Name { get; set; }

    [RegularExpression("Some value")]
    public string ExtraProperty { get; set; }
}

public class PatchCustomersController : Controller
{
    [HttpPatch]
    public IActionResult Patch(int key, [FromBody]Delta<PatchCustomer> patch)
    {
        PatchCustomer c = new PatchCustomer() { Id = key, ExtraProperty = "Some value" };
        patch.Patch(c);
        TryValidateModel(c);

        if (ModelState.IsValid)
        {
            return Ok(c);
        }
        else
        {
            return BadRequest(ModelState);
        }
    }
}
