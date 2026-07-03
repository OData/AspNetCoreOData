//-----------------------------------------------------------------------------
// <copyright file="OpenSkipTokenQueryTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SkipToken;

public class OpenSkipTokenQueryTests : WebApiTestBase<OpenSkipTokenQueryTests>
{
    public OpenSkipTokenQueryTests(WebApiTestFixture<OpenSkipTokenQueryTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(OpenSkipTokenCustomersController), typeof(MetadataController));
        services.AddControllers()
            .AddOData(opt =>
                opt.AddRouteComponents("open", GetEdmModel())
                   .Count().Filter().OrderBy().SetMaxTop(null).Select().SkipToken());
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<OpenSkipTokenCustomer>("Customers");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task SkipToken_IntDynamicProperty_FollowsAllPagesCorrectly()
    {
        await FollowAllPages(
            "open/customers?$orderby=Score",
            new[] { new[] { 8, 2 }, new[] { 7, 5 }, new[] { 3, 6 }, new[] { 1, 4 } });
    }

    [Fact]
    public async Task SkipToken_StringDynamicProperty_FollowsAllPagesCorrectly()
    {
        await FollowAllPages(
            "open/customers?$orderby=Tag",
            new[] { new[] { 2, 5 }, new[] { 8, 1 }, new[] { 4, 7 }, new[] { 3, 6 } });
    }

    [Fact]
    public async Task SkipToken_BoolDynamicProperty_FollowsAllPagesCorrectly()
    {
        await FollowAllPages(
            "open/customers?$orderby=IsActive",
            new[] { new[] { 2, 4 }, new[] { 7, 1 }, new[] { 3, 5 }, new[] { 6, 8 } });
    }

    [Fact]
    public async Task SkipToken_IntDynamicProperty_DescendingFollowsAllPagesCorrectly()
    {
        await FollowAllPages(
            "open/customers?$orderby=Score desc",
            new[] { new[] { 4, 1 }, new[] { 6, 3 }, new[] { 5, 7 }, new[] { 2, 8 } });
    }

    [Fact]
    public async Task SkipToken_BoolDynamicProperty_DescendingFollowsAllPagesCorrectly()
    {
        await FollowAllPages(
            "open/customers?$orderby=IsActive desc",
            new[] { new[] { 1, 3 }, new[] { 5, 6 }, new[] { 8, 2 }, new[] { 4, 7 } });
    }

    [Fact]
    public async Task SkipToken_EnumAsStringDynamicProperty_FollowsAllPagesCorrectly()
    {
        await FollowAllPages(
            "open/customers?$orderby=Status",
            new[] { new[] { 5, 6 }, new[] { 1, 3 }, new[] { 8, 2 }, new[] { 4, 7 } });
    }

    [Fact]
    public async Task SkipToken_TwoDynamicProperties_FollowsAllPagesCorrectly()
    {
        await FollowAllPages(
            "open/customers?$orderby=Tag,Score",
            new[] { new[] { 8, 2 }, new[] { 5, 7 }, new[] { 1, 4 }, new[] { 3, 6 } });
    }

    [Fact]
    public async Task SkipToken_DynamicAndDeclaredProperty_FollowsAllPagesCorrectly()
    {
        await FollowAllPages(
            "open/customers?$orderby=IsActive,Name",
            new[] { new[] { 2, 4 }, new[] { 7, 1 }, new[] { 3, 5 }, new[] { 6, 8 } });
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task FollowAllPages(string firstRequestUri, int[][] expectedPagesIds)
    {
        HttpClient client = CreateClient();
        string requestUri = firstRequestUri;

        for (int page = 0; page < expectedPagesIds.Length; page++)
        {
            Assert.NotNull(requestUri);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            HttpResponseMessage response = await client.SendAsync(request);

            string body = await response.Content.ReadAsStringAsync();
            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"Page {page + 1}: GET {requestUri} returned {response.StatusCode}: {body}");
            JObject payload = await response.Content.ReadAsObject<JObject>();

            int[] actualIds = ((JArray)payload["value"])
                .Select(item => (int)item["Id"])
                .ToArray();

            Assert.True(
                expectedPagesIds[page].SequenceEqual(actualIds),
                $"Page {page + 1}: expected [{string.Join(",", expectedPagesIds[page])}] " +
                $"but got [{string.Join(",", actualIds)}]");

            requestUri = (string)payload["@odata.nextLink"];
        }

        // The last expected page must be the final page — no further next link.
        Assert.True(requestUri == null, $"Expected no further next link after {expectedPagesIds.Length} pages but got: {requestUri}");
    }
}
