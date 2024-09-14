//-----------------------------------------------------------------------------
// <copyright file="ModelBoundQuerySettingsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ModelBoundQuerySettings;

public class ModelBoundQuerySettingsTests : WebApiTestBase<ModelBoundQuerySettingsTests>
{
    public ModelBoundQuerySettingsTests(WebApiTestFixture<ModelBoundQuerySettingsTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(AuthorsController));

        services.AddControllers().AddOData(
            options =>
            {
                options.AddRouteComponents("enablequery", ModelBoundQuerySettingsEdmModel.GetEdmModel());
                options.AddRouteComponents("modelboundapi", ModelBoundQuerySettingsEdmModel.GetEdmModelByModelBoundAPI());
            });
    }

    [Theory]
    [InlineData("enablequery/Authors?$filter=Books/any(d: d/BookId eq 7)")]
    [InlineData("modelboundapi/Authors?$filter=Books/any(d: d/BookId eq 7)")]
    public async Task FilterOnNestedCollection(string requestUri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
        HttpClient client = CreateClient();

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsObject<JObject>();

        var authors = content.GetValue("value") as JArray;
        Assert.NotNull(authors);

        var author = Assert.Single(authors) as JObject;
        Assert.Equal(3, author.GetValue("AuthorId"));
    }
}
