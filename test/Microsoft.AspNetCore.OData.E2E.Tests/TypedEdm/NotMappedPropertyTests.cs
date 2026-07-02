//-----------------------------------------------------------------------------
// <copyright file="NotMappedPropertyTests.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.E2E.Tests.TypedEdm;

/// <summary>
/// E2E tests verifying that CLR properties excluded from the EDM model via
/// <see cref="System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute"/> are not
/// included in <c>$skiptoken</c> values when a client orders by those property names on an open type.
/// </summary>
public class NotMappedPropertyTests : WebApiTestBase<NotMappedPropertyTests>
{
    public NotMappedPropertyTests(WebApiTestFixture<NotMappedPropertyTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(NotMappedPropertyController), typeof(MetadataController));
        services.AddControllers()
            .AddOData(opt =>
                opt.AddRouteComponents("auth", GetEdmModel())
                   .Count().Filter().OrderBy().SetMaxTop(null).Select().SkipToken());
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<UserAccount>("Accounts");
        // [NotMapped] on UserAccount.PasswordHash causes the convention builder to omit it
        return builder.GetEdmModel();
    }

    /// <summary>
    /// Confirms that <c>PasswordHash</c>, decorated with <c>[NotMapped]</c>, is absent from the
    /// EDM model — establishing the baseline the remaining tests rely on.
    /// </summary>
    [Fact]
    public void PasswordHash_IsNotDeclared_InEdmModel()
    {
        IEdmModel model = GetEdmModel();
        IEdmEntityType entityType = model.SchemaElements
            .OfType<IEdmEntityType>()
            .Single(e => e.Name == "UserAccount");

        Assert.Null(entityType.FindProperty("PasswordHash"));
    }

    /// <summary>
    /// Ordering by a <c>[NotMapped]</c> CLR property must not include its value
    /// in the <c>$skiptoken</c> of <c>@odata.nextLink</c>.
    /// </summary>
    [Fact]
    public async Task OrderByNotMappedProperty_DoesNotIncludeValue_InSkipToken()
    {
        // $orderby=PasswordHash — the OData parser accepts this on an open type and emits
        // a SingleValueOpenPropertyAccessNode. The skip-token must not include the CLR value.
        HttpRequestMessage request = new HttpRequestMessage(
            HttpMethod.Get,
            "auth/accounts?$orderby=PasswordHash");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

        HttpResponseMessage response = await CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JObject payload = await response.Content.ReadAsObject<JObject>();
        string nextLink = (string)payload["@odata.nextLink"];

        Assert.NotNull(nextLink); // paging is active — a next link must be present
        Assert.DoesNotContain("hash_for_alice", nextLink, System.StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash_for_bob",   nextLink, System.StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash_for_carol", nextLink, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Regression guard: ordering by a declared EDM property must still produce a correct skip token.
    /// </summary>
    [Fact]
    public async Task OrderByDeclaredProperty_StillProducesCorrectSkipToken()
    {
        HttpRequestMessage request = new HttpRequestMessage(
            HttpMethod.Get,
            "auth/accounts?$orderby=Name");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

        HttpResponseMessage response = await CreateClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JObject payload = await response.Content.ReadAsObject<JObject>();
        string nextLink = (string)payload["@odata.nextLink"];

        Assert.NotNull(nextLink);
        Assert.Contains("$skiptoken=Name-", nextLink, System.StringComparison.OrdinalIgnoreCase);
        // "Bob" is the last item on page 1 (Alice → Bob sorted ascending)
        Assert.Contains("Bob", System.Net.WebUtility.UrlDecode(nextLink));
    }

    /// <summary>
    /// Verifies that all pages can be traversed when ordering by a <c>[NotMapped]</c> property —
    /// the server must not return an error on any page.
    /// </summary>
    [Fact]
    public async Task OrderByNotMappedProperty_AllPagesTraversableWithoutError()
    {
        string requestUri = "auth/accounts?$orderby=PasswordHash";
        HttpClient client = CreateClient();
        int pageCount = 0;

        while (requestUri != null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject payload = await response.Content.ReadAsObject<JObject>();
            requestUri = (string)payload["@odata.nextLink"];
            pageCount++;

            Assert.True(pageCount <= 10, "Unexpected number of pages.");
        }

        // 3 accounts with PageSize=2 → 2 pages
        Assert.Equal(2, pageCount);
    }
}
