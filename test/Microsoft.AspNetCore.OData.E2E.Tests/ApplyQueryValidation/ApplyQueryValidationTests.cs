//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryValidationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ApplyQueryValidation;

/// <summary>
/// End-to-end tests asserting that property restrictions are enforced consistently across
/// <c>$filter</c>, <c>$apply</c> (filter/groupby/aggregate/compute) and top-level <c>$compute</c>.
/// A property marked as not filterable or configured as not selectable is expected to be rejected
/// with <see cref="HttpStatusCode.BadRequest"/> regardless of which query option references it, while
/// the equivalent request against an allowed property is expected to succeed with
/// <see cref="HttpStatusCode.OK"/>.
/// </summary>
public class ApplyQueryValidationTests : WebApiTestBase<ApplyQueryValidationTests>
{
    public ApplyQueryValidationTests(WebApiTestFixture<ApplyQueryValidationTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel edmModel = ApplyQueryValidationEdmModel.GetEdmModel();

        services.ConfigureControllers(typeof(ApplyValidationItemsController));

        services.AddControllers().AddOData(options =>
            options.EnableQueryFeatures().AddRouteComponents("odata", edmModel));
    }

    // Each case references a restricted property (not filterable, or not selectable) through a
    // query option. The expected/consistent behavior is a 400 response, matching the equivalent
    // $filter/$select. The $filter case is the regression anchor: it already returns 400 today.
    [Theory]
    [InlineData("$apply=filter(RestrictedName eq 'R1')")]
    [InlineData("$apply=groupby((NotSelectableName))")]
    [InlineData("$apply=aggregate(RestrictedAmount with max as MaxRestricted)")]
    [InlineData("$apply=aggregate(Related(RestrictedAmount with sum as SumRestricted))")]
    [InlineData("$apply=compute(RestrictedName eq 'R1' as Flag)")]
    [InlineData("$compute=RestrictedName eq 'R1' as Flag")]
    [InlineData("$filter=RestrictedName eq 'R1'")]
    public async Task QueryRestrictedProperty_ThroughApplyFilterComputeOrSelect_ReturnsBadRequest(string query)
    {
        // Arrange
        string queryUrl = $"odata/ApplyValidationItems?{query}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Each case references an allowed property through the same query options. These already succeed
    // today and must keep succeeding, proving the restriction is selective rather than all-or-nothing.
    [Theory]
    [InlineData("$apply=filter(Name eq 'Alpha')")]
    [InlineData("$apply=groupby((Name))")]
    [InlineData("$apply=aggregate(Amount with sum as TotalAmount)")]
    [InlineData("$apply=aggregate(Related(Amount with sum as SumAmount))")]
    [InlineData("$apply=aggregate($count as Total)")]
    [InlineData("$apply=compute(Amount mul 2 as DoubleAmount)")]
    [InlineData("$compute=Amount mul 2 as DoubleAmount")]
    public async Task QueryAllowedProperty_ThroughApplyFilterOrCompute_ReturnsOk(string query)
    {
        // Arrange
        string queryUrl = $"odata/ApplyValidationItems?{query}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
