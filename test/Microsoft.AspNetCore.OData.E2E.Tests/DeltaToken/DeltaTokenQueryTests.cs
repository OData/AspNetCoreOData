//-----------------------------------------------------------------------------
// <copyright file="DeltaTokenQueryTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken;

public class DeltaTokenQueryTests : WebApiTestBase<DeltaTokenQueryTests>
{
    public DeltaTokenQueryTests(WebApiTestFixture<DeltaTokenQueryTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(TestCustomersController), typeof(TestOrdersController));
        services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", GetEdmModel()).Count().Filter().OrderBy().Expand().SetMaxTop(null).Select());
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<TestCustomer>("TestCustomers");
        builder.EntitySet<TestOrder>("TestOrders");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task DeltaVerifyReslt()
    {
        HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, "odata/TestCustomers?$deltaToken=abc");
        get.Headers.Add("Accept", "application/json;odata.metadata=minimal");
        get.Headers.Add("OData-Version", "4.01");
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.SendAsync(get);
        Assert.True(response.IsSuccessStatusCode);
        dynamic results = await response.Content.ReadAsObject<JObject>();

        Assert.True(results.value.Count == 7, "There should be 7 entries in the response");

        var changeEntity = results.value[0];
        Assert.True(((JToken)changeEntity).Count() == 9, "The changed customer should have 6 properties plus type written. But now it contains non-changed properties, it's regression bug?");
        string changeEntityType = changeEntity["@type"].Value as string;
        Assert.True(changeEntityType != null, "The changed customer should have type written");
        Assert.True(changeEntityType.Contains("#Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken.TestCustomerWithAddress"), "The changed order should be a TestCustomerWithAddress");
        Assert.True(changeEntity.Id.Value == 1, "The ID Of changed customer should be 1.");
        Assert.True(changeEntity.OpenProperty.Value == 10, "The OpenProperty property of changed customer should be 10.");
        Assert.True(changeEntity.NullOpenProperty.Value == null, "The NullOpenProperty property of changed customer should be null.");
        Assert.True(changeEntity.Name.Value == "Name", "The Name of changed customer should be 'Name'");
        Assert.True(((JToken)changeEntity.Address).Count() == 3, "The changed entity's Address should have 2 properties written. But now it contains non-changed properties, it's regression bug?");
        Assert.True(changeEntity.Address.State.Value == "State", "The changed customer's Address.State should be 'State'.");
        Assert.True(changeEntity.Address.ZipCode.Value == (int?)null, "The changed customer's Address.ZipCode should be null.");

        var phoneNumbers = changeEntity.PhoneNumbers;
        Assert.True(((JToken)phoneNumbers).Count() == 2, "The changed customer should have 2 phone numbers");
        Assert.True(phoneNumbers[0].Value == "123-4567", "The first phone number should be '123-4567'");
        Assert.True(phoneNumbers[1].Value == "765-4321", "The second phone number should be '765-4321'");

        var newCustomer = results.value[1];
        Assert.True(((JToken)newCustomer).Count() == 5, "The new customer should have 3 properties written, But now it contains 2 non-changed properties, it's regression bug?");
        Assert.True(newCustomer.Id.Value == 10, "The ID of the new customer should be 10");
        Assert.True(newCustomer.Name.Value == "NewCustomer", "The name of the new customer should be 'NewCustomer'");

        var places = newCustomer.FavoritePlaces;
        Assert.True(((JToken)places).Count() == 2, "The new customer should have 2 favorite places");

        var place1 = places[0];
        Assert.True(((JToken)place1).Count() == 3, "The first favorite place should have 2 properties written.But now it contains non-changed properties, it's regression bug?");
        Assert.True(place1.State.Value == "State", "The first favorite place's state should be 'State'.");
        Assert.True(place1.ZipCode.Value == (int?)null, "The first favorite place's Address.ZipCode should be null.");

        var place2 = places[1];
        Assert.True(((JToken)place2).Count() == 5, "The second favorite place should have 5 properties written.");
        Assert.True(place2.City.Value == "City2", "The second favorite place's Address.City should be 'City2'.");
        Assert.True(place2.State.Value == "State2", "The second favorite place's Address.State should be 'State2'.");
        Assert.True(place2.ZipCode.Value == 12345, "The second favorite place's Address.ZipCode should be 12345.");
        Assert.True(place2.OpenProperty.Value == 10, "The second favorite place's Address.OpenProperty should be 10.");
        Assert.True(place2.NullOpenProperty.Value == null, "The second favorite place's Address.NullOpenProperty should be null.");

        var newOrder = results.value[2];
        Assert.True(((JToken)newOrder).Count() == 4, "The new order should have 2 properties plus context written, , But now it contains one non-changed properties, it's regression bug?");
        string newOrderContext = newOrder["@context"].Value as string;
        Assert.True(newOrderContext != null, "The new order should have a context written");
        Assert.True(newOrderContext.Contains("$metadata#TestOrders"), "The new order should come from the TestOrders entity set");
        Assert.True(newOrder.Id.Value == 27, "The ID of the new order should be 27");
        Assert.True(newOrder.Amount.Value == 100, "The amount of the new order should be 100");

        var deletedEntity = results.value[3];
        Assert.True(deletedEntity["@id"].Value == "7", "The ID of the deleted customer should be 7");
        Assert.True(deletedEntity["@removed"].reason.Value == "changed", "The reason for the deleted customer should be 'changed'");

        var deletedOrder = results.value[4];
        string deletedOrderContext = deletedOrder["@context"].Value as string;
        Assert.True(deletedOrderContext != null, "The deleted order should have a context written");
        Assert.True(deletedOrderContext.Contains("$metadata#TestOrders"), "The deleted order should come from the TestOrders entity set");
        Assert.True(deletedOrder["@id"].Value == "12", "The ID of the deleted order should be 12");
        Assert.True(deletedOrder["@removed"].reason.Value == "deleted", "The reason for the deleted order should be 'deleted'");

        var deletedLink = results.value[5];
        Assert.True(deletedLink.source.Value == "http://localhost/odata/TestCustomers(1)", "The source of the deleted link should be 'http://localhost/odata/TestCustomers(1)'");
        Assert.True(deletedLink.target.Value == "http://localhost/odata/TestOrders(12)", "The target of the deleted link should be 'http://localhost/odata/TestOrders(12)'");
        Assert.True(deletedLink.relationship.Value == "Orders", "The relationship of the deleted link should be 'Orders'");

        var addedLink = results.value[6];
        Assert.True(addedLink.source.Value == "http://localhost/odata/TestCustomers(10)", "The source of the added link should be 'http://localhost/odata/TestCustomers(10)'");
        Assert.True(addedLink.target.Value == "http://localhost/odata/TestOrders(27)", "The target of the added link should be 'http://localhost/odata/TestOrders(27)'");
        Assert.True(addedLink.relationship.Value == "Orders", "The relationship of the added link should be 'Orders'");
    }

    [Fact]
    public async Task DeltaVerifyReslt_ContainsDynamicComplexProperties()
    {
        HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, "odata/TestOrders?$deltaToken=abc");
        get.Headers.Add("Accept", "application/json;odata.metadata=minimal");
        get.Headers.Add("OData-Version", "4.01");
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.SendAsync(get);
        Assert.True(response.IsSuccessStatusCode);

        string result = await response.Content.ReadAsStringAsync();
        Assert.Equal("{\"@context\":\"http://localhost/odata/$metadata#TestOrders/$delta\"," +
        "\"value\":[" +
          "{" +
            "\"Id\":1," +
            "\"Amount\":42," +
            "\"Location\":{" +
              "\"State\":\"State\"," +
              "\"City\":null," +
              "\"ZipCode\":null," +
              "\"OpenProperty\":10," +
              "\"key-samplelist\":{" +
                "\"@type\":\"#Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken.TestAddress\"," +
                "\"State\":\"sample state\"," +
                "\"City\":null," +
                "\"ZipCode\":9," +
                "\"title\":\"sample title\"" +
              "}" +
            "}" +
          "}" +
        "]" +
      "}",
            result);
    }
}
