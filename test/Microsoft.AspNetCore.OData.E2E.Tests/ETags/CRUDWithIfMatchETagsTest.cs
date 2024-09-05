//-----------------------------------------------------------------------------
// <copyright file="CRUDWithIfMatchETagsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags;

public class CRUDWithIfMatchETagsTest : WebApiTestBase<CRUDWithIfMatchETagsTest>
{
    public CRUDWithIfMatchETagsTest(WebApiTestFixture<CRUDWithIfMatchETagsTest> fixture)
        :base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel edmModel = GetEdmModel();
        services.ConfigureControllers(typeof(ETagsCustomersController));
        services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", edmModel));
        services.AddControllers(opt => opt.Filters.Add(new ETagActionFilterAttribute()));
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        EntitySetConfiguration<ETagsCustomer> eTagsCustomersSet = builder.EntitySet<ETagsCustomer>("ETagsCustomers");
        EntityTypeConfiguration<ETagsCustomer> eTagsCustomers = eTagsCustomersSet.EntityType;
        eTagsCustomers.Property(c => c.Id).IsConcurrencyToken();
        eTagsCustomers.Property(c => c.Name).IsConcurrencyToken();
        return builder.GetEdmModel();
    }

    // Be noted, the id=0 is for "DeletedUpdated", don't change it otherwise there will be a conflict with other tests.
    [Fact]
    public async Task DeleteUpdatedEntityWithIfMatchShouldReturnPreconditionFailed()
    {
        // Arrange
        string eTag;
        HttpClient client = CreateClient();

        // Act - 1
        var getUri = "odata/ETagsCustomers?$format=json";
        using (HttpResponseMessage getResponse = await client.GetAsync(getUri))
        {
            // Assert - 1
            Assert.True(getResponse.IsSuccessStatusCode);
            var json = await getResponse.Content.ReadAsObject<JObject>();
            var result = json.GetValue("value") as JArray;

            eTag = result[0]["@odata.etag"].ToString();
            Assert.False(string.IsNullOrEmpty(eTag));
        }

        // Act - 2
        var putUri = "odata/ETagsCustomers(0)";
        var putContent = JObject.Parse(string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 0, "Customer Name 0 updated", "This is note 0 updated"));
        using (HttpResponseMessage response = await client.PutAsJsonAsync(putUri, putContent))
        {
            // Assert - 2
            response.EnsureSuccessStatusCode();
        }

        // Act - 3
        var deleteUri = "odata/ETagsCustomers(0)";
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, deleteUri);
        deleteRequest.Headers.IfMatch.ParseAdd(eTag);
        using (HttpResponseMessage response = await client.SendAsync(deleteRequest))
        {
            // Assert -3
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
        }
    }

    // Be noted, the id=1 is for "PutUpdated", don't change it otherwise there will be a conflict with other tests.
    [Fact]
    public async Task PutUpdatedEntityWithIfMatchShouldReturnPreconditionFailed()
    {
        // Arrange - 1
        string requestUri = "odata/ETagsCustomers(1)?$format=json";
        HttpClient client = CreateClient();

        // Act - 1
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert - 1
        Assert.True(response.IsSuccessStatusCode);

        JObject result = await response.Content.ReadAsObject<JObject>();
        var etagInHeader = response.Headers.ETag.ToString();
        var etagInPayload = (string)result["@odata.etag"];
        Assert.True(etagInPayload == etagInHeader,
            string.Format("The etag value in payload is not the same as the one in Header, in payload it is: {0}, but in header, {1}.", etagInPayload, etagInHeader));

        // Arrange - 2
        requestUri = "odata/ETagsCustomers(1)";
        request = new HttpRequestMessage(HttpMethod.Put, requestUri);
        string payload = string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 1, "Customer Name 1 updated", "This is note 1 updated");
        request.Content = new StringContent(payload);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        // Act - 2
        response = await client.SendAsync(request);

        // Assert - 2
        Assert.True(response.IsSuccessStatusCode);

        // Arrange - 3
        requestUri = "odata/ETagsCustomers(1)";
        request = new HttpRequestMessage(HttpMethod.Put, requestUri);
        payload = string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 1, "Customer Name 1 updated again", "This is note 1 updated again");
        request.Content = new StringContent(payload);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Headers.IfMatch.ParseAdd(etagInPayload);

        // Act - 3
        response = await client.SendAsync(request);

        // Assert - 3
        Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
    }

    // Be noted, the id=2 is for "PatchUpdated", don't change it otherwise there will be a conflict with other tests.
    [Fact]
    public async Task PatchUpdatedEntityWithIfMatchShouldReturnPreconditionFailed()
    {
        // Arrange (1)
        string requestUri = "odata/ETagsCustomers(2)?$format=json";
        HttpClient client = CreateClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act (1)
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert (1)
        Assert.True(response.IsSuccessStatusCode);
        var etagInHeader = response.Headers.ETag.ToString();
        JObject result = await response.Content.ReadAsObject<JObject>();
        var etagInPayload = (string)result["@odata.etag"];
        Assert.True(etagInPayload == etagInHeader,
            string.Format("The etag value in payload is not the same as the one in Header, in payload it is: {0}, but in header, {1}.", etagInPayload, etagInHeader));

        // Arrange (2)
        requestUri = "odata/ETagsCustomers(2)";
        request = new HttpRequestMessage(HttpMethod.Put, requestUri);
        string payload = string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}"",""Notes"":[""{3}""]}}", typeof(ETagsCustomer), 2, "Customer Name 2 updated", "This is note 2 updated");
        request.Content = new StringContent(payload);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        // Act (2)
        response = await client.SendAsync(request);

        // Assert (2)
        Assert.True(response.IsSuccessStatusCode);

        // Arrange (3)
        requestUri = "odata/ETagsCustomers(2)";
        request = new HttpRequestMessage(new HttpMethod("Patch"), requestUri);
        payload = string.Format(@"{{""@odata.type"":""#{0}"",""Id"":{1},""Name"":""{2}""}}", typeof(ETagsCustomer), 2, "Customer Name 2 updated again");
        request.Content = new StringContent(payload);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        request.Headers.IfMatch.ParseAdd(etagInPayload);

        // Act (3)
        response = await client.SendAsync(request);

        // Assert (3)
        Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
    }

    [Fact]
    public async Task GetEntityWithIfNoneMatchShouldReturnNotModifiedETagsTest()
    {
        // Arrange
        HttpClient client = CreateClient();
        string eTag;

        // DeleteUpdatedEntryWithIfMatchETagsTests will change #"0" customer
        // PutUpdatedEntryWithIfMatchETagsTests will change #"1"customer
        // PatchUpdatedEntryWithIfMatchETagsTest will change #"2" customer
        // So, this case uses "4"
        int customerId = 4;
        var getUri = "odata/ETagsCustomers?$format=json";

        // Act
        using (var response = await client.GetAsync(getUri))
        {
            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json.GetValue("value") as JArray;
            Assert.NotNull(result);

            eTag = result[customerId]["@odata.etag"].ToString();
            Assert.False(string.IsNullOrEmpty(eTag));
        }

        // Arrange & Act
        var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, $"odata/ETagsCustomers({customerId})");
        getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);
        using (var response = await client.SendAsync(getRequestWithEtag))
        {
            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
        }
    }
}
