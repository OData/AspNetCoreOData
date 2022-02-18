//-----------------------------------------------------------------------------
// <copyright file="TypelessSerializationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless
{
    public class TypelessSerializationTests : WebApiTestBase<TypelessSerializationTests>
    {
        public TypelessSerializationTests(WebApiTestFixture<TypelessSerializationTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = GetEdmModel();
            services.ConfigureControllers(typeof(TypelessCustomersController));
            services.AddControllers().AddOData(opt => opt.Expand().AddRouteComponents("odata", edmModel));
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var customers = builder.EntitySet<TypelessCustomer>("TypelessCustomers");
            customers.EntityType.Property(c => c.Name).IsRequired();
            var orders = builder.EntitySet<TypelessOrder>("TypelessOrders");
            customers.EntityType.Collection.Action("PrimitiveCollection").ReturnsCollection<int>();
            customers.EntityType.Collection.Action("ComplexObjectCollection").ReturnsCollection<TypelessAddress>();
            customers.EntityType.Collection.Action("EntityCollection").ReturnsCollectionFromEntitySet<TypelessOrder>("TypelessOrders");
            customers.EntityType.Collection.Action("SinglePrimitive").Returns<int>();
            customers.EntityType.Collection.Action("SingleComplexObject").Returns<TypelessAddress>();
            customers.EntityType.Collection.Action("SingleEntity").ReturnsFromEntitySet<TypelessOrder>("TypelessOrders");
            customers.EntityType.Collection.Action("EnumerableOfIEdmObject").ReturnsFromEntitySet<TypelessOrder>("TypelessOrders");

            var typelessAction = customers.EntityType.Collection.Action("TypelessParameters");
            typelessAction.Parameter<TypelessAddress>("address");
            typelessAction.Parameter<int>("value");
            typelessAction.CollectionParameter<TypelessAddress>("addresses");
            typelessAction.CollectionParameter<int>("values");
            typelessAction.Returns<TypelessAddress>();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full")]
        public async Task TypelessWorksInAllFormats(string acceptHeader)
        {
            // Arrange
            string url = "odata/TypelessCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            HttpClient client = CreateClient();

            // Act & Assert
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData("PrimitiveCollection")]
        [InlineData("ComplexObjectCollection")]
        [InlineData("EntityCollection")]
        [InlineData("SinglePrimitive")]
        [InlineData("SingleComplexObject")]
        [InlineData("SingleEntity")]
        public async Task TypelessWorksForAllKindsOfDataTypes(string actionName)
        {
            // Arrange
            object expectedPayload = null;
            expectedPayload = (actionName == "PrimitiveCollection") ? new { value = Enumerable.Range(1, 10) } : expectedPayload;
            expectedPayload = (actionName == "ComplexObjectCollection") ? new { value = CreateAddresses(10) } : expectedPayload;
            expectedPayload = (actionName == "EntityCollection") ? new { value = CreateOrders(10) } : expectedPayload;
            expectedPayload = (actionName == "SinglePrimitive") ? new { value = 10 } : expectedPayload;
            expectedPayload = (actionName == "SingleComplexObject") ? CreateAddress(10) : expectedPayload;
            expectedPayload = (actionName == "SingleEntity") ? CreateOrder(10) : expectedPayload;

            HttpClient client = CreateClient();
            string url = "odata/TypelessCustomers/Default." + actionName;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            JToken result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(JToken.FromObject(expectedPayload), result, JToken.EqualityComparer);
        }

        [Fact]
        public async Task RoundTripEntityWorks()
        {
            // Arrange
            int i = 10;
            JObject typelessCustomer = new JObject();
            typelessCustomer["Id"] = i;
            typelessCustomer["Name"] = string.Format("Name {0}", i);
            typelessCustomer["Orders"] = CreateOrders(i);
            typelessCustomer["Addresses"] = CreateAddresses(i);
            typelessCustomer["FavoriteNumbers"] = new JArray(Enumerable.Range(0, i).ToArray());
            HttpClient client = CreateClient();

            string url = "odata/TypelessCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            string payload = typelessCustomer.ToString();
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            // Arrange
            HttpRequestMessage getRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}({1})?$expand=Orders", url, i));
            getRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage getResponse = await client.SendAsync(getRequest);

            // Assert
            Assert.True(getResponse.IsSuccessStatusCode);
            Assert.NotNull(getResponse.Content);
            JObject returnedObject = await getResponse.Content.ReadAsObject<JObject>();
            Assert.Equal(typelessCustomer, returnedObject, JToken.EqualityComparer);
        }

        [Fact]
        public async Task TypelessActionParametersRoundtrip()
        {
            // Arrange
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/TypelessCustomers/Default.TypelessParameters");
            object obj = new { address = CreateAddress(5), value = 5, addresses = CreateAddresses(10), values = Enumerable.Range(0, 5) };
            string payload = (JToken.FromObject(obj) as JObject).ToString();
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        private static JArray CreateAddresses(int i)
        {
            JArray addresses = new JArray();
            for (int j = 0; j < i; j++)
            {
                JObject complexObject = CreateAddress(j);
                addresses.Add(complexObject);
            }
            return addresses;
        }

        private static JArray CreateOrders(int i)
        {
            JArray orders = new JArray();
            for (int j = 0; j < i; j++)
            {
                JObject order = new JObject();
                order["Id"] = j;
                order["ShippingAddress"] = CreateAddress(j);
                orders.Add(order);
            }
            return orders;
        }

        private static JObject CreateOrder(int j)
        {
            JObject order = new JObject();
            order["Id"] = j;
            order["ShippingAddress"] = CreateAddress(j);
            return order;
        }

        private static JObject CreateAddress(int j)
        {
            JObject address = new JObject();
            address["FirstLine"] = "First line " + j;
            address["SecondLine"] = "Second line " + j;
            address["ZipCode"] = j;
            address["City"] = "City " + j;
            address["State"] = "State " + j;
            return address;
        }
    }

    public class TypelessCustomersController : ODataController
    {
        private static IEdmEntityObject postedCustomer = null;

        public IEdmEntityType CustomerType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessCustomer") as IEdmEntityType;
            }
        }

        public IEdmEntityType OrderType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessOrder") as IEdmEntityType;
            }
        }

        public IEdmComplexType AddressType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.AspNetCore.OData.E2E.Tests.Typeless.TypelessAddress") as IEdmComplexType;
            }
        }

        public IActionResult Get()
        {
            IEdmEntityObject[] typelessCustomers = new EdmEntityObject[20];
            for (int i = 0; i < 20; i++)
            {
                dynamic typelessCustomer = new EdmEntityObject(CustomerType);
                typelessCustomer.Id = i;
                typelessCustomer.Name = string.Format("Name {0}", i);
                typelessCustomer.Orders = CreateOrders(i);
                typelessCustomer.Addresses = CreateAddresses(i);
                typelessCustomer.FavoriteNumbers = Enumerable.Range(0, i).ToArray();
                typelessCustomers[i] = typelessCustomer;
            }

            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmEntityTypeReference(CustomerType, isNullable: false)));

            return Ok(new EdmEntityObjectCollection(entityCollectionType, typelessCustomers.ToList()));
        }

        public IActionResult Get([FromODataUri] int key)
        {
            object id;
            if (postedCustomer == null || !postedCustomer.TryGetPropertyValue("Id", out id) || key != (int)id)
            {
                return BadRequest("The key isn't the one posted to the customer");
            }

            ODataQueryContext context = new ODataQueryContext(Request.GetModel(), CustomerType, path: null);
            ODataQueryOptions query = new ODataQueryOptions(context, Request);
            if (query.SelectExpand != null)
            {
                Request.ODataFeature().SelectExpandClause = query.SelectExpand.SelectExpandClause;
            }

            return Ok(postedCustomer);
        }

        public IActionResult Post([FromBody]IEdmEntityObject customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("customer is null");
            }
            postedCustomer = customer;
            object id;
            customer.TryGetPropertyValue("Id", out id);

            IEdmEntitySet entitySet = Request.GetModel().EntityContainer.FindEntitySet("TypelessCustomers");
            return Created(Request.CreateODataLink(new EntitySetSegment(entitySet),
                new KeySegment(new[] { new KeyValuePair<string, object>("Id", id) }, entitySet.EntityType(), null)), customer);
        }

        [HttpPost]
        public IActionResult PrimitiveCollection()
        {
            return Ok(Enumerable.Range(1, 10));
        }

        [HttpPost]
        public IActionResult ComplexObjectCollection()
        {
            return Ok(CreateAddresses(10));
        }

        [HttpPost]
        public IActionResult EntityCollection()
        {
            return Ok(CreateOrders(10));
        }

        [HttpPost]
        public IActionResult SinglePrimitive()
        {
            return Ok(10);
        }

        [HttpPost]
        public IActionResult SingleComplexObject()
        {
            return Ok(CreateAddress(10));
        }

        [HttpPost]
        public IActionResult SingleEntity()
        {
            return Ok(CreateOrder(10));
        }

        public IActionResult EnumerableOfIEdmObject()
        {
            IList<IEdmEntityObject> result = Enumerable.Range(0, 10).Select(i => (IEdmEntityObject)CreateOrder(i)).ToList();
            return Ok(result);
        }

        [HttpPost]
        public IActionResult TypelessParameters(ODataUntypedActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("parameters is null");
            }
            object address;
            object addresses;
            object value;
            object values;
            if (!parameters.TryGetValue("address", out address) || address as IEdmComplexObject == null ||
                !parameters.TryGetValue("addresses", out addresses) || addresses as IEnumerable == null ||
                !parameters.TryGetValue("value", out value) || (int)value != 5 ||
                !parameters.TryGetValue("values", out values) || values as IEnumerable == null ||
                !(values as IEnumerable).Cast<int>().SequenceEqual(Enumerable.Range(0, 5)))
            {
                return BadRequest("Address is not present or is not a complex object");
            }
            return Ok(address as IEdmComplexObject);
        }

        private dynamic CreateAddresses(int i)
        {
            EdmComplexObject[] addresses = new EdmComplexObject[i];
            for (int j = 0; j < i; j++)
            {
                dynamic complexObject = CreateAddress(j);
                addresses[j] = complexObject;
            }
            var collection = new EdmComplexObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(AddressType, false))), addresses);
            return collection;
        }

        private dynamic CreateOrders(int i)
        {
            EdmEntityObject[] orders = new EdmEntityObject[i];
            for (int j = 0; j < i; j++)
            {
                dynamic order = new EdmEntityObject(OrderType);
                order.Id = j;
                order.ShippingAddress = CreateAddress(j);
                orders[j] = order;
            }
            var collection = new EdmEntityObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(OrderType, false))), orders);
            return collection;
        }

        private dynamic CreateOrder(int j)
        {
            dynamic order = new EdmEntityObject(OrderType);
            order.Id = j;
            order.ShippingAddress = CreateAddress(j);
            return order;
        }

        private dynamic CreateAddress(int j)
        {
            dynamic address = new EdmComplexObject(AddressType);
            address.FirstLine = "First line " + j;
            address.SecondLine = "Second line " + j;
            address.ZipCode = j;
            address.City = "City " + j;
            address.State = "State " + j;
            return address;
        }
    }

    public class TypelessCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<TypelessOrder> Orders { get; set; }
        public virtual IList<TypelessAddress> Addresses { get; set; }
        public virtual IList<int> FavoriteNumbers { get; set; }
    }

    public class TypelessOrder
    {
        public int Id { get; set; }
        public TypelessAddress ShippingAddress { get; set; }
    }

    public class TypelessAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
