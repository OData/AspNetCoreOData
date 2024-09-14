//-----------------------------------------------------------------------------
// <copyright file="AlternateKeysTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AlternateKeys;

public class AlternateKeysTest : WebApiTestBase<AlternateKeysTest>
{
    public AlternateKeysTest(WebApiTestFixture<AlternateKeysTest> fixture)
        :base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        var controllers = new[]
        {
            typeof (CustomersController), typeof (OrdersController), typeof (PeopleController),
            typeof (CompaniesController), typeof (MetadataController)
        };

        services.ConfigureControllers(controllers);

        IEdmModel model = AlternateKeysEdmModel.GetEdmModel();

        services.AddControllers().AddOData(opt => opt.Count().Filter().OrderBy().Expand().SetMaxTop(null)
            .AddRouteComponents("odata", model,
            services => services.AddSingleton<ODataUriResolver>(sp => new AlternateKeysODataUriResolver(model))));
    }

    [Fact]
    public async Task AlteranteKeysMetadata()
    {
        string expect = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
"<edmx:Edmx Version=\"4.0\" xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\">\r\n" +
"  <edmx:DataServices>\r\n" +
"    <Schema Namespace=\"NS\" xmlns=\"http://docs.oasis-open.org/odata/ns/edm\">\r\n" +
"      <EntityType Name=\"Customer\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"ID\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"ID\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Name\" Type=\"Edm.String\" />\r\n" +
"        <Property Name=\"SSN\" Type=\"Edm.String\" />\r\n" +
"        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n" +
"          <Collection>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"SSN\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"SSN\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"          </Collection>\r\n" +
"        </Annotation>\r\n" +
"      </EntityType>\r\n" +
"      <EntityType Name=\"Order\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"OrderId\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"OrderId\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Name\" Type=\"Edm.String\" />\r\n" +
"        <Property Name=\"Token\" Type=\"Edm.Guid\" />\r\n" +
"        <Property Name=\"Amount\" Type=\"Edm.Int32\" />\r\n" +
"        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n" +
"          <Collection>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Name\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Name\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Token\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Token\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"          </Collection>\r\n" +
"        </Annotation>\r\n" +
"      </EntityType>\r\n" +
"      <EntityType Name=\"Person\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"ID\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"ID\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Country_Region\" Type=\"Edm.String\" />\r\n" +
"        <Property Name=\"Passport\" Type=\"Edm.String\" />\r\n" +
"        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n" +
"          <Collection>\r\n" +
"            <Record Type=\"OData.Community.Keys.V1.AlternateKey\">\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Country_Region\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Country_Region\" />\r\n" +
"                  </Record>\r\n" +
"                  <Record Type=\"OData.Community.Keys.V1.PropertyRef\">\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Passport\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Passport\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"          </Collection>\r\n" +
"        </Annotation>\r\n" +
"      </EntityType>\r\n" +
"      <ComplexType Name=\"Address\">\r\n" +
"        <Property Name=\"Street\" Type=\"Edm.String\" />\r\n" +
"        <Property Name=\"City\" Type=\"Edm.String\" />\r\n" +
"      </ComplexType>\r\n" +
"      <EntityType Name=\"Company\">\r\n" +
"        <Key>\r\n" +
"          <PropertyRef Name=\"ID\" />\r\n" +
"        </Key>\r\n" +
"        <Property Name=\"ID\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Code\" Type=\"Edm.Int32\" />\r\n" +
"        <Property Name=\"Location\" Type=\"NS.Address\" />\r\n" +
"        <Annotation Term=\"OData.Community.Keys.V1.AlternateKeys\">\r\n" +
"          <Collection>\r\n" +
"            <Record>\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record>\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Code\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Code\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"            <Record>\r\n" +
"              <PropertyValue Property=\"Key\">\r\n" +
"                <Collection>\r\n" +
"                  <Record>\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"City\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Location/City\" />\r\n" +
"                  </Record>\r\n" +
"                  <Record>\r\n" +
"                    <PropertyValue Property=\"Alias\" String=\"Street\" />\r\n" +
"                    <PropertyValue Property=\"Name\" PropertyPath=\"Location/Street\" />\r\n" +
"                  </Record>\r\n" +
"                </Collection>\r\n" +
"              </PropertyValue>\r\n" +
"            </Record>\r\n" +
"          </Collection>\r\n" +
"        </Annotation>\r\n" +
"      </EntityType>\r\n" +
"      <EntityContainer Name=\"Default\">\r\n" +
"        <EntitySet Name=\"Customers\" EntityType=\"NS.Customer\" />\r\n" +
"        <EntitySet Name=\"Orders\" EntityType=\"NS.Order\" />\r\n" +
"        <EntitySet Name=\"People\" EntityType=\"NS.Person\" />\r\n" +
"        <EntitySet Name=\"Companies\" EntityType=\"NS.Company\" />\r\n" +
"      </EntityContainer>\r\n" +
"    </Schema>\r\n" +
"  </edmx:DataServices>\r\n" +
"</edmx:Edmx>";

        // Remove indentation
        expect = Regex.Replace(expect, @"\r\n\s*<", @"<");

        var requestUri = "odata/$metadata";
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.GetAsync(requestUri);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string responseContent = await response.Content.ReadAsStringAsync();

        Assert.Equal(expect, responseContent);
    }

    [Fact]
    public async Task QueryEntityWithSingleAlternateKeysWorks()
    {
        // query with alternate keys
        string expect = "{" +
                        "\"@odata.context\":\"http://localhost/odata/$metadata#Edm.String\",\"value\":\"special-SSN\"" +
                        "}";

        var requestUri = "odata/Customers(SSN='special-SSN')";
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();

        Assert.Equal(expect, responseContent);
    }

    public static TheoryDataSet<string, string> SingleAlternateKeysCases
    {
        get
        {
            var data = new TheoryDataSet<string, string>();
            for (int i = 1; i <= 5; i++)
            {
                data.Add("Customers(" + i + ")", "Customers(SSN='SSN-" + i + "-" + (100 + i) + "')");
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(SingleAlternateKeysCases))]
    public async Task EntityWithSingleAlternateKeys_ReturnsSame_WithPrimitiveKey(string declaredKeys, string alternatekeys)
    {
        // query with declared key
        var requestUri = $"odata/{declaredKeys}";
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();
        string primitiveResponse = await response.Content.ReadAsStringAsync();

        // query with alternate key
        requestUri = $"odata/{alternatekeys}";
        response = await client.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();
        string alternatekeyResponse = await response.Content.ReadAsStringAsync();

        Assert.Equal(primitiveResponse, alternatekeyResponse);
    }

    [Fact]
    public async Task QueryEntityWithMultipleAlternateKeys_Returns_SameEntityWithPrimitiveKey()
    {
        // query with declared key
        var requestUri = "odata/Orders(2)";
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();
        string primitiveResponse = await response.Content.ReadAsStringAsync();

        // query with one alternate key
        requestUri = "odata/Orders(Name='Order-2')";
        response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        string nameResponse = await response.Content.ReadAsStringAsync();

        // query with another alternate key
        requestUri = "odata/Orders(Token=75036B94-C836-4946-8CC8-054CF54060EC)";
        response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        string tokenResponse = await response.Content.ReadAsStringAsync();

        Assert.Equal(primitiveResponse, nameResponse);
        Assert.Equal(primitiveResponse, tokenResponse);
    }

    [Fact]
    public async Task QueryEntityWithComposedAlternateKeys_Returns_SameEntityWithPrimitiveKey()
    {
        // query with declared key
        var requestUri = "odata/People(2)";
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();
        string primitiveResponse = await response.Content.ReadAsStringAsync();

        // query with composed alternate keys
        requestUri = "odata/People(Country_Region='United States',Passport='9999')";
        response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        string composedResponse = await response.Content.ReadAsStringAsync();

        Assert.Equal(primitiveResponse, composedResponse);
    }

    [Fact]
    public async Task QueryFailedIfMissingAnyOfComposedAlternateKeys()
    {
        // Since this request matched "odata/People({key})", and key value is not valid.
        // It throws exception
        var requestUri = "odata/People(Country_Region='United States')";
        HttpClient client = CreateClient();

        try
        {
            var response = await client.GetAsync(requestUri);
        }
        catch (ODataException ex)
        {
            Assert.Equal("The key value (Country_Region='United States') from request is not valid. The key value should be format of type 'Edm.Int32'.", ex.Message);
        }
    }

    /* ODL has the bug to parse the route template with complex property path expression.
    [Fact]
    public async Task QueryEntityWithComplexPropertyAlternateKeys_Returns_SameEntityWithPrimitiveKey()
    {
        HttpClient client = CreateClient();

        // query with declared key
        var requestUri = "odata/Companies(2)";
        HttpResponseMessage response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        string responseContent1 = await response.Content.ReadAsStringAsync();

        // query with complex alternate key
        requestUri = "odata/Companies(Code=30)";
        response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        string responseContent2 = await response.Content.ReadAsStringAsync();
        Assert.Equal(responseContent1, responseContent2);

        // query with complex alternate key
        requestUri = "odata/Companies(City='Guangzhou',Street='Xiaoxiang Rd')";
        response = await client.GetAsync(requestUri);
        string responseContent3 = await response.Content.ReadAsStringAsync();
        Assert.Equal(responseContent2, responseContent3);
    }
    */

    [Fact]
    public async Task CanUpdateEntityWithSingleAlternateKeys()
    {
        string expect = "{" +
                        "\"@odata.context\":\"http://localhost/odata/$metadata#Customers/$entity\",\"ID\":6,\"Name\":\"Updated Customer Name\",\"SSN\":\"SSN-6-T-006\"" +
                        "}";

        var requestUri = "odata/Customers(SSN='SSN-6-T-006')";

        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
        const string content = @"{'Name':'Updated Customer Name'}";
        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        HttpClient client = CreateClient();

        HttpResponseMessage response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        string responseContent = await response.Content.ReadAsStringAsync();

        Assert.Equal(expect, responseContent);
    }
}
