//-----------------------------------------------------------------------------
// <copyright file="TypelessDeltaSerializationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless;

public class TypelessDeltaSerializationTests : WebApiTestBase<TypelessDeltaSerializationTests>
{
    public TypelessDeltaSerializationTests(WebApiTestFixture<TypelessDeltaSerializationTests> fixture)
        :base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        IEdmModel edmModel = GetEdmModel();
        services.ConfigureControllers(typeof(TypelessDeltaCustomersController));
        services.AddControllers().AddOData(opt => opt.Expand().AddRouteComponents("odata", edmModel));
    }

    private static IEdmModel GetEdmModel()
    {
        ODataModelBuilder builder = new ODataConventionModelBuilder();
        var customers = builder.EntitySet<TypelessCustomer>("TypelessDeltaCustomers");
        customers.EntityType.Property(c => c.Name).IsRequired();
        var orders = builder.EntitySet<TypelessOrder>("TypelessDeltaOrders");
        return builder.GetEdmModel();
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/json;odata.metadata=minimal")]
    [InlineData("application/json;odata.metadata=full")]
    public async Task TypelessDeltaWorksInAllFormats(string acceptHeader)
    {
        // Arrange
        string url = "odata/TypelessDeltaCustomers?$deltatoken=abc";
        HttpClient client = CreateClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
        request.Headers.Add("OData-Version", "4.01");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(response.Content);
        JObject returnedObject = await response.Content.ReadAsObject<JObject>();
        Assert.True(((dynamic)returnedObject).value.Count == 15);

        //Verification of content to validate Payload
        for (int i = 0 ; i < 10 ; i++)
        {
            string name = string.Format("Name {0}", i);
            Assert.True(name.Equals(((dynamic)returnedObject).value[i]["Name"].Value));
        }

        for (int i=10 ; i < 15 ; i++)
        {
            Assert.True(i.ToString().Equals(((dynamic)returnedObject).value[i]["@id"].Value));
        }
    }
}

public class TypelessDeltaCustomersController : ODataController
{
    public IEdmEntityType DeltaCustomerType
    {
        get
        {
            return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessCustomer") as IEdmEntityType;
        }
    }

    public IEdmEntityType DeltaOrderType
    {
        get
        {
            return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessOrder") as IEdmEntityType;
        }
    }

    public IEdmComplexType DeltaAddressType
    {
        get
        {
            return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessAddress") as IEdmComplexType;
        }
    }

    public IActionResult Get()
    {
        EdmChangedObjectCollection changedCollection = new EdmChangedObjectCollection(DeltaCustomerType);
        //Changed or Modified objects are represented as EdmDeltaResourceObjects
        for (int i = 0; i < 10; i++)
        {
            dynamic typelessCustomer = new EdmDeltaResourceObject(DeltaCustomerType);
            typelessCustomer.Id = i;
            typelessCustomer.Name = string.Format("Name {0}", i);
            typelessCustomer.FavoriteNumbers = Enumerable.Range(0, i).ToArray();
            changedCollection.Add(typelessCustomer);
        }

        //Deleted objects are represented as EdmDeltaDeletedObjects
        for (int i = 10; i < 15; i++)
        {
            dynamic typelessCustomer = new EdmDeltaDeletedResourceObject(DeltaCustomerType);
            typelessCustomer.Id = new Uri(i.ToString(), UriKind.RelativeOrAbsolute);
            typelessCustomer.Reason = DeltaDeletedEntryReason.Deleted;
            changedCollection.Add(typelessCustomer);
        }

        return Ok(changedCollection);
    }
}
