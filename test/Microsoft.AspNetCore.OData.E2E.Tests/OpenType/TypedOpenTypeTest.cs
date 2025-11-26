//-----------------------------------------------------------------------------
// <copyright file="TypedOpenTypeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.OpenType;

// this convention is used to stop the conventions for a certain route prefix.
// Here, we stop to apply routing conventions for "attributeRouting" route prefix.
public class StopODataRoutingConvention : IODataControllerActionConvention
{
    public int Order => int.MinValue;

    public bool AppliesToAction(ODataControllerActionContext context)
    {
        if (context.Prefix == "attributeRouting")
        {
            return true;
        }

        return false;
    }

    public bool AppliesToController(ODataControllerActionContext context)
    {
        return true;
    }
}

public class TypedOpenTypeTest : WebApiTestBase<TypedOpenTypeTest>
{
    public TypedOpenTypeTest(WebApiTestFixture<TypedOpenTypeTest> fixture)
        :base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(AccountsController), typeof(ODataEndpointController));

        IEdmModel model1 = OpenComplexTypeEdmModel.GetTypedConventionModel();

        services.AddControllers().AddOData(opt =>
        {
            opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select()
            .AddRouteComponents("convention", model1)
            .AddRouteComponents("attributeRouting", model1)
            .AddRouteComponents("explicit", OpenComplexTypeEdmModel.GetTypedExplicitModel())
            .Conventions.Add(new StopODataRoutingConvention());

            // simply suppress the route number from conventional routing
            opt.RouteOptions.EnableUnqualifiedOperationCall = false;
            opt.RouteOptions.EnableKeyAsSegment = false;
        });
    }

    [Theory]
    [InlineData("convention", "application/json;odata.metadata=full")]
    [InlineData("convention", "application/json;odata.metadata=minimal")]
    [InlineData("convention", "application/json;odata.metadata=none")]
    [InlineData("attributeRouting", "application/json;odata.metadata=full")]
    [InlineData("attributeRouting", "application/json;odata.metadata=minimal")]
    [InlineData("attributeRouting", "application/json;odata.metadata=none")]
    public async Task QueryEntitySet(string mode, string format)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        string requestUri = $"{mode}/Accounts?$format={format}";

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsObject<JObject>();

        var results = json.GetValue("value") as JArray;
        Assert.Equal<int>(3, results.Count);

        var age = results[1]["AccountInfo"]["Age"];
        Assert.Equal(20, age);

        var gender = (string)results[2]["AccountInfo"]["Gender"];
        Assert.Equal("Female", gender);

        var countryOrRegion = results[1]["Address"]["CountryOrRegion"].ToString();
        Assert.Equal("AnyCountry", countryOrRegion);

        var tag1 = results[0]["Tags"]["Tag1"];
        Assert.Equal("Value 1", tag1);
        var tag2 = results[0]["Tags"]["Tag2"];
        Assert.Equal("Value 2", tag2);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task QueryEntity(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);
        string requestUri = $"{mode}/Accounts(1)";

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var result = await response.Content.ReadAsObject<JObject>();

        var age = result["AccountInfo"]["Age"];
        Assert.Equal(10, age);

        var gender = (string)result["AccountInfo"]["Gender"];
        Assert.Equal("Male", gender);

        var countryOrRegion = result["Address"]["CountryOrRegion"].ToString();
        Assert.Equal("US", countryOrRegion);

        var tag1 = result["Tags"]["Tag1"];
        Assert.Equal("Value 1", tag1);
        var tag2 = result["Tags"]["Tag2"];
        Assert.Equal("Value 2", tag2);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task QueryPropertyFromDerivedOpenEntity(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);
        string requestUri = $"{mode}/Accounts(1)/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.PremiumAccount/Since";

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("2014-05-22T00:00:00+08:00", content);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("explicit")]
    public async Task QueryOpenComplexTypePropertyAccountInfo(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);
        string requestUri = $"{mode}/Accounts(1)/AccountInfo";

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsObject<JObject>();

        var nickName = json.GetValue("NickName").ToString();
        Assert.Equal("NickName1", nickName);

        var age = json.GetValue("Age");
        Assert.Equal(10, age);

        var gender = (string)json.GetValue("Gender");
        Assert.Equal("Male", gender);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task QueryOpenComplexTypePropertyAddress(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);
        string requestUri = $"{mode}/Accounts(1)/Address";

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsObject<JObject>();

        var city = json.GetValue("City").ToString();
        Assert.Equal("Redmond", city);

        var countryOrRegion = json.GetValue("CountryOrRegion").ToString();
        Assert.Equal("US", countryOrRegion);

        // Property defined in the derived type.
        var countryCode = json.GetValue("CountryCode").ToString();
        Assert.Equal("US", countryCode);
    }

    [Theory]
    [InlineData("application/json;odata.metadata=full")]
    [InlineData("application/json;odata.metadata=minimal")]
    [InlineData("application/json;odata.metadata=none")]
    public async Task QueryDerivedOpenComplexType(string format)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);
        string requestUri = "attributeRouting/Accounts(1)/Address/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress?$format=" + format;

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsObject<JObject>();

        var city = json.GetValue("City").ToString();
        Assert.Equal("Redmond", city);

        var countryOrRegion = json.GetValue("CountryOrRegion").ToString();
        Assert.Equal("US", countryOrRegion);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task QueryOpenComplexTypePropertyTags(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);
        string requestUri = $"{mode}/Accounts(1)/Tags";

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsObject<JObject>();

        var tag1 = json.GetValue("Tag1").ToString();
        Assert.Equal("Value 1", tag1);

        var tag2 = json.GetValue("Tag2").ToString();
        Assert.Equal("Value 2", tag2);
    }

    [Theory]
    [InlineData("application/json;odata.metadata=full")]
    [InlineData("application/json;odata.metadata=minimal")]
    [InlineData("application/json;odata.metadata=none")]
    public async Task QueryNonDynamicProperty(string format)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);
        string requestUri = $"attributeRouting/Accounts(1)/Address/City?$format=" + format;

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsObject<JObject>();
        var city = json.GetValue("value").ToString();
        Assert.Equal("Redmond", city);
    }

    #region Update

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task PatchEntityWithOpenComplexType(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        string patchUri = $"{mode}/Accounts(2)";
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), patchUri);
        string payload = @"{
                '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.Account',
                'AccountInfo':{'NickName':'NewNickName1','Age':40,'Gender': 'Male'},
                'Address':{'CountryOrRegion':'United States'},
                'Tags':{'Tag1':'New Value'},
                'ShipAddresses@odata.type':'#Collection(Microsoft.AspNetCore.OData.E2E.Tests.OpenType.Address)',
                'ShipAddresses':[],
                'OwnerGender@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.Gender',
                'OwnerGender':null
              }";
        request.Content = new StringContent(payload);

        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        // Act
        using (var patchResponse = await client.SendAsync(request))
        {
            // Assert
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

            var content = await patchResponse.Content.ReadAsObject<JObject>();

            var accountInfo = content["AccountInfo"];
            Assert.Equal("NewNickName1", accountInfo["NickName"]);
            Assert.Equal(40, accountInfo["Age"]);

            Assert.Equal("Male", (string)accountInfo["Gender"]);

            var address = content["Address"];
            Assert.Equal("United States", address["CountryOrRegion"]);

            var tags = content["Tags"];
            Assert.Equal("New Value", tags["Tag1"]);
            JsonAssert.DoesNotContainProperty("OwnerGender", content);
            Assert.Empty(((JArray)content["ShipAddresses"]));
        }

        // Arrange
        string requestUri = $"/{mode}/Accounts(2)";

        // Act
        HttpResponseMessage response = await client.GetAsync(requestUri);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var result = await response.Content.ReadAsObject<JObject>();

        var updatedAccountinfo = result["AccountInfo"];
        Assert.Equal("NewNickName1", updatedAccountinfo["NickName"]);
        Assert.Equal(40, updatedAccountinfo["Age"]);
        Assert.Equal("Male", updatedAccountinfo["Gender"]);

        var updatedAddress = result["Address"];
        Assert.Equal("United States", updatedAddress["CountryOrRegion"]);

        var updatedTags = result["Tags"];
        Assert.Equal("New Value", updatedTags["Tag1"]);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task PutEntityWithOpenComplexType(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        string putUri = $"{mode}/Accounts(2)";
        var putContent = JObject.Parse(@"{'Id':2,'Name':'NewName2',
            'AccountInfo':{'NickName':'NewNickName1','Age':11,'Gender@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.Gender','Gender':'Male'},
            'Address':{'City':'Redmond','Street':'1 Microsoft Way','CountryOrRegion':'United States'},
            'Tags':{'Tag1':'New Value'}}");

        // Act
        using (HttpResponseMessage putResponse = await client.PutAsJsonAsync(putUri, putContent))
        {
            // Assert
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

            var content = await putResponse.Content.ReadAsObject<JObject>();

            var accountInfo = content["AccountInfo"];
            Assert.Equal("NewNickName1", accountInfo["NickName"]);
            Assert.Equal(11, accountInfo["Age"]);

            Assert.Equal("Male", accountInfo["Gender"]);

            var address = content["Address"];
            Assert.Equal("United States", address["CountryOrRegion"]);

            var tags = content["Tags"];
            Assert.Equal("New Value", tags["Tag1"]);
        }

        // Arrange
        string requestUri = putUri;

        HttpResponseMessage response = await client.GetAsync(requestUri);
        Assert.True(response.IsSuccessStatusCode);

        // Act
        var result = await response.Content.ReadAsObject<JObject>();

        // Assert
        var updatedAccountinfo = result["AccountInfo"];
        Assert.Equal("NewNickName1", updatedAccountinfo["NickName"]);
        Assert.Equal(11, updatedAccountinfo["Age"]);

        var updatedAddress = result["Address"];
        Assert.Equal("United States", updatedAddress["CountryOrRegion"]);

        var updatedTags = result["Tags"];
        Assert.Equal("New Value", updatedTags["Tag1"]);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task PatchOpenComplexTypeProperty(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        // Get ~/Accounts(1)/Address
        var requestUri = $"{mode}/Accounts(1)/Address";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
        Assert.Equal("Redmond", content["City"]);
        Assert.Equal("1 Microsoft Way", content["Street"]);
        Assert.Equal("US", content["CountryCode"]);
        Assert.Equal("US", content["CountryOrRegion"]); // dynamic property

        // Patch ~/Accounts(1)/Address
        request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
        request.Content = new StringContent(
            @"{
                    '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress',
                    'City':'NewCity',
                    'OtherProperty@odata.type':'#DateOnly',
                    'OtherProperty':'2016-02-01'
              }");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Get ~/Accounts(1)/Address
        request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(7, content.Count); // @odata.context + 3 declared properties + 2 dynamic properties + 1 @odata.type
        Assert.Equal("NewCity", content["City"]); // updated
        Assert.Equal("1 Microsoft Way", content["Street"]);
        Assert.Equal("US", content["CountryCode"]);
        Assert.Equal("US", content["CountryOrRegion"]);
        Assert.Equal("2016-02-01", content["OtherProperty"]);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task PatchOpenComplexTypeProperty_WithDifferentType(string mode)
    {
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        // Get ~/Accounts(1)/Address
        var requestUri = $"{mode}/Accounts(1)/Address";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
        Assert.Equal("Redmond", content["City"]);
        Assert.Equal("1 Microsoft Way", content["Street"]);
        Assert.Equal("US", content["CountryCode"]);
        Assert.Equal("US", content["CountryOrRegion"]); // dynamic property

        // Patch ~/Accounts(1)/Address
        request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
        request.Content = new StringContent(
            @"{
                    '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.Address',
                    'City':'NewCity',
                    'OtherProperty@odata.type':'#DateOnly',
                    'OtherProperty':'2016-02-01'
              }");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Get ~/Accounts(1)/Address
        request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(6, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties + 1 @odata.type
        Assert.Equal("NewCity", content["City"]); // updated
        Assert.Equal("1 Microsoft Way", content["Street"]);

        Assert.Equal("US", content["CountryOrRegion"]);
        Assert.Equal("2016-02-01", content["OtherProperty"]);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task PatchOpenDerivedComplexTypeProperty(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        // Get ~/Accounts(1)/Address
        var requestUri = $"{mode}/Accounts(1)/Address/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
        Assert.Equal("Redmond", content["City"]);
        Assert.Equal("1 Microsoft Way", content["Street"]);
        Assert.Equal("US", content["CountryCode"]);
        Assert.Equal("US", content["CountryOrRegion"]);

        // Arrange
        // Patch ~/Accounts(1)/Address/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress
        request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
        request.Content = new StringContent(
            @"{
                    '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress',
                    'CountryCode':'NewCountryCode',
                    'CountryOrRegion':'NewCountry'
              }");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Arrange
        // Get ~/Accounts(1)/Address/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress
        request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
        Assert.Equal("Redmond", content["City"]);
        Assert.Equal("1 Microsoft Way", content["Street"]);
        Assert.Equal("NewCountryCode", content["CountryCode"]); // updated
        Assert.Equal("NewCountry", content["CountryOrRegion"]);  // updated
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task PutOpenComplexTypeProperty(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        // Get ~/Accounts(1)/Address
        var requestUri = $"{mode}/Accounts(1)/Address";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
        Assert.Equal("Redmond", content["City"]);
        Assert.Equal("1 Microsoft Way", content["Street"]);
        Assert.Equal("US", content["CountryCode"]);
        Assert.Equal("US", content["CountryOrRegion"]); // dynamic property

        // Put ~/Accounts(1)/Address
        request = new HttpRequestMessage(HttpMethod.Put, requestUri);
        request.Content = new StringContent(
            @"{
                    '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.Address',
                    'City':'NewCity',
                    'Street':'NewStreet',
                    'OtherProperty@odata.type':'#DateOnly',
                    'OtherProperty':'2016-02-01'
              }");
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Arrange
        // Get ~/Accounts(1)/Address
        request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(6, content.Count); // @odata.context + 3 declared properties + 2 new dynamic properties
        Assert.Equal("NewCity", content["City"]); // updated
        Assert.Equal("NewStreet", content["Street"]); // updated
        Assert.Equal("US", content["CountryCode"]);
        Assert.Null(content["CountryOrRegion"]);
        Assert.Equal("2016-02-01", content["OtherProperty"]);
    }
    #endregion

    #region Insert

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task InsertEntityWithOpenComplexTypeProperty(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        var postUri = $"{mode}/Accounts";

        var postContent = JObject.Parse(
@"{
'Id':4,
'Name':'Name4',
'AccountInfo':
{
    'NickName':'NickName4','Age':40,'Gender@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.Gender','Gender':'Male'
},
'Address':
{
    '@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GlobalAddress',
    'City':'London','Street':'Baker street','CountryOrRegion':'UnitedKindom','CountryCode':'Code'
},
'Tags':{'Tag1':'Value 1','Tag2':'Value 2'},
'AnotherGender@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.OpenType.Gender',
'AnotherGender':'Female'
}");
        using (HttpResponseMessage response = await client.PostAsJsonAsync(postUri, postContent))
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var json = await response.Content.ReadAsObject<JObject>();

            var age = json["AccountInfo"]["Age"];
            Assert.Equal(40, age);

            var gender = (string)json["AccountInfo"]["Gender"];
            Assert.Equal("Male", gender);

            var countryOrRegion = json["Address"]["CountryOrRegion"];
            Assert.Equal("UnitedKindom", countryOrRegion);

            var countryCode = json["Address"]["CountryCode"];
            Assert.Equal("Code", countryCode);

            var tag1 = json["Tags"]["Tag1"];
            Assert.Equal("Value 1", tag1);
            var tag2 = json["Tags"]["Tag2"];
            Assert.Equal("Value 2", tag2);
            var anotherGender = (string)json["AnotherGender"];
            Assert.Equal("Female", anotherGender);
        }
    }

    #endregion

    #region Delete

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task DeleteEntityWithOpenComplexTypeProperty(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        var deleteUri = $"/{mode}/Accounts(1)";

        // Act & Assert
        using (HttpResponseMessage response = await client.DeleteAsync(deleteUri))
        {
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        // Arrange
        var requestUri = $"/{mode}/Accounts?$format={1}";

        // Act & Assert
        using (HttpResponseMessage response = await client.GetAsync(requestUri))
        {
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsObject<JObject>();

            var results = json.GetValue("value") as JArray;
            Assert.Equal(2, results.Count);
        }
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task DeleteOpenComplexTypeProperty(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        // Get ~/Accounts(1)/Address
        var requestUri = $"/{mode}/Accounts(1)/Address";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsObject<JObject>();
        Assert.Equal(5, content.Count); // @odata.context + 3 declared properties + 1 dynamic properties
        Assert.Equal("Redmond", content["City"]);
        Assert.Equal("1 Microsoft Way", content["Street"]);
        Assert.Equal("US", content["CountryCode"]);
        Assert.Equal("US", content["CountryOrRegion"]); // dynamic property

        // Arrange & Act & Assert
        // Delete ~/Accounts(1)/Address
        request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Arrange & Act & Assert
        // Get ~/Accounts(1)/Address
        request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    #endregion

    #region Function & Action

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task GetAddressFunction(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        string requestUri = $"/{mode}/Accounts(1)/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.GetAddressFunction()";

        HttpResponseMessage response = await client.GetAsync(requestUri);
        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsObject<JObject>();

        var city = json.GetValue("City").ToString();
        Assert.Equal("Redmond", city);

        var countryOrRegion = json.GetValue("CountryOrRegion");
        Assert.Equal("US", countryOrRegion);

        var countryCode = json.GetValue("CountryCode");
        Assert.Equal("US", countryCode);
    }

    [Theory]
    [InlineData("convention")]
    [InlineData("attributeRouting")]
    public async Task IncreaseAgeAction(string mode)
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        string requestUri = $"/{mode}/Accounts(1)/Microsoft.AspNetCore.OData.E2E.Tests.OpenType.IncreaseAgeAction";
        var requestForPost = new HttpRequestMessage(HttpMethod.Post, requestUri);
        requestForPost.Content = new StringContent(string.Empty);
        requestForPost.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        HttpResponseMessage response = await client.SendAsync(requestForPost);
        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsObject<JObject>();

        var nickName = json.GetValue("NickName").ToString();
        Assert.Equal("NickName1", nickName);

        var age = json.GetValue("Age");
        Assert.Equal(11, age);
    }

    [Fact]
    public async Task TestRoutes()
    {
        // Arrange
        string requestUri = "$odata";
        HttpClient client = CreateClient();

        // Act
        var response = await client.GetAsync(requestUri);

        // Assert
        response.EnsureSuccessStatusCode();
        string payload = await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task UpdateAddressAction()
    {
        // Arrange
        HttpClient client = CreateClient();
        await ResetDatasource(client);

        string uri = "/attributeRouting/UpdateAddressAction";
        var content = new { Address = new { Street = "Street 11", City = "City 11", CountryOrRegion = "CountryOrRegion 11" }, ID = 1 };

        var response = await client.PostAsJsonAsync(uri, content);
        Assert.True(response.IsSuccessStatusCode);

        string getUri = "/attributeRouting/Accounts(1)";

        HttpResponseMessage getResponse = await client.GetAsync(getUri);
        Assert.True(getResponse.IsSuccessStatusCode);

        var result = await getResponse.Content.ReadAsObject<JObject>();

        var city = result["Address"]["City"].ToString();
        Assert.Equal("City 11", city);
        var country = result["Address"]["CountryOrRegion"].ToString();
        Assert.Equal("CountryOrRegion 11", country);
    }
    #endregion
    private async Task<HttpResponseMessage> ResetDatasource(HttpClient client)
    {
        var uriReset = "ResetDataSource";
        var response = await client.PostAsync(uriReset, null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        return response;
    }
}
