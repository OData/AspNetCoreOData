//-----------------------------------------------------------------------------
// <copyright file="ETagsOtherTypesTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags;

public class ETagsOtherTypesTest : WebApiTestBase<ETagsOtherTypesTest>
{
    public ETagsOtherTypesTest(WebApiTestFixture<ETagsOtherTypesTest> fixture)
        :base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel edmModel1 = GetDoubleETagEdmModel();
        IEdmModel edmModel2 = GetShortETagEdmModel();
        services.ConfigureControllers(typeof(ETagsCustomersController));
        services.AddControllers().AddOData(opt => opt.Select().AddRouteComponents("double", edmModel1).AddRouteComponents("short", edmModel2));
    }

    private static IEdmModel GetDoubleETagEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        var customer = builder.EntitySet<ETagsCustomer>("ETagsCustomers").EntityType;
        customer.Property(c => c.DoubleProperty).IsConcurrencyToken();
        customer.Ignore(c => c.StringWithConcurrencyCheckAttributeProperty);
        customer.Ignore(c => c.RowVersion);
        return builder.GetEdmModel();
    }

    private static IEdmModel GetShortETagEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        var customer = builder.EntitySet<ETagsCustomer>("ETagsCustomers").EntityType;
        customer.Ignore(c => c.StringWithConcurrencyCheckAttributeProperty);
        customer.Ignore(c => c.RowVersion);
        customer.Property(c => c.ShortProperty).IsConcurrencyToken();
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task GetEntityWithIfNoneMatchShouldReturnNotModifiedETagsTest_ForDouble()
    {
        // Arrange
        HttpClient client = CreateClient();
        string eTag;

        var getUri = "double/ETagsCustomers?$format=json";

        // Act
        using (var response = await client.GetAsync(getUri))
        {
            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json.GetValue("value") as JArray;
            Assert.NotNull(result);

            // check the first unchanged, the first unchanged could be "3"
            // because #0, #1, #2 will change potentially running in parallel by other tests.
            eTag = result[3]["@odata.etag"].ToString();
            Assert.False(String.IsNullOrEmpty(eTag));
            Assert.Equal("W/\"OC4w\"", eTag);

            EntityTagHeaderValue parsedValue;
            Assert.True(EntityTagHeaderValue.TryParse(eTag, out parsedValue));
            IDictionary<string, object> tags = this.ParseETag(parsedValue);
            KeyValuePair<string, object> pair = Assert.Single(tags);
            Single value = Assert.IsType<Single>(pair.Value);
            Assert.Equal((Single)8.0, value);
        }

        // Arrange
        var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, "double/ETagsCustomers(3)");
        getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);

        // Act
        using (var response = await client.SendAsync(getRequestWithEtag))
        {
            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
        }
    }

    [Fact]
    public async Task GetEntityWithIfNoneMatchShouldReturnNotModifiedETagsTest_ForShort()
    {
        // Arrange
        HttpClient client = CreateClient();
        string eTag;

        var getUri = "short/ETagsCustomers?$format=json";

        // Act
        using (var response = await client.GetAsync(getUri))
        {
            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var json = await response.Content.ReadAsObject<JObject>();
            var result = json.GetValue("value") as JArray;
            Assert.NotNull(result);

            // check the first unchanged, the first unchanged could be "3",
            // because #0, #1, #2 will change potentially running in parallel by other tests.
            eTag = result[3]["@odata.etag"].ToString();
            Assert.False(String.IsNullOrEmpty(eTag));
            Assert.Equal("W/\"MzI3NjQ=\"", eTag);

            EntityTagHeaderValue parsedValue;
            Assert.True(EntityTagHeaderValue.TryParse(eTag, out parsedValue));
            IDictionary<string, object> tags = this.ParseETag(parsedValue);
            KeyValuePair<string, object> pair = Assert.Single(tags);
            int value = Assert.IsType<int>(pair.Value);
            Assert.Equal(32764, value);
        }

        // Arrange
        var getRequestWithEtag = new HttpRequestMessage(HttpMethod.Get, "short/ETagsCustomers(3)");
        getRequestWithEtag.Headers.IfNoneMatch.ParseAdd(eTag);

        // Act
        using (var response = await client.SendAsync(getRequestWithEtag))
        {
            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
        }
    }

    private IDictionary<string, object> ParseETag(EntityTagHeaderValue etagHeaderValue)
    {
        string tag = etagHeaderValue.Tag.Trim('\"');

        // split etag
        string[] rawValues = tag.Split(',');
        IDictionary<string, object> properties = new Dictionary<string, object>();
        for (int index = 0; index < rawValues.Length; index++)
        {
            string rawValue = rawValues[index];

            // base64 decode
            byte[] bytes = Convert.FromBase64String(rawValue);
            string valueString = Encoding.UTF8.GetString(bytes);
            object obj = ODataUriUtils.ConvertFromUriLiteral(valueString, ODataVersion.V4);
            if (obj is ODataNullValue)
            {
                obj = null;
            }
            properties.Add(index.ToString(CultureInfo.InvariantCulture), obj);
        }

        return properties;
    }
}
