// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing
{
    public class ODataActionTests : WebApiTestBase<ODataActionTests>
    {
        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = GetEdmModel();
            IEdmModel typelessEdmModel = GetTypelessModel();

            services.ConfigureControllers(typeof(CustomersController), typeof(UntypedCustomersController));
            services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", edmModel).AddRouteComponents("typeless", typelessEdmModel));
        }

        public ODataActionTests(WebApiTestFixture<ODataActionTests> fixture)
           : base(fixture)
        { }

        [Fact]
        public async Task CanDispatch_ActionPayload_ToBoundAction()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/Customers(1)/org.odata.DoSomething");
            request.Headers.Add("accept", "application/json");
            string payload = @"{
                ""p1"": 1,
                ""p2"": { ""StreetAddress"": ""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 },
                ""p3"": [ ""one"", ""two"" ],
                ""p4"": [ { ""StreetAddress"": ""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } ],
                ""color"": ""Red"",
                ""colors"": [""Red"", null, ""Green""]
            }";

            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = payload.Length;
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":true", responseString);
        }

        private const string EntityPayload = @"{
                ""Customer"": {""@odata.type"":""#org.odata.Customer"", ""ID"":101,""Name"":""Avatar"" } ,
                ""Customers"": [
                    {""@odata.type"":""#org.odata.Customer"", ""ID"":901,""Name"":""John"" } ,
                    {""@odata.type"":""#org.odata.SubCustomer"", ""ID"":902,""Name"":""Mike"", ""Price"": 9.9 }
                ]
            }";

        [Fact]
        public async Task CanDispatch_ActionPayloadWithEntity_ToBoundAction()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "odata/Customers/org.odata.MyAction");
            request.Headers.Add("accept", "application/json");

            request.Content = new StringContent(EntityPayload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            request.Content.Headers.ContentLength = EntityPayload.Length;
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":true", responseString);
        }

        [Fact]
        public async Task CanDispatch_ActionPayloadWithEntity_ToBoundAction_Typeless()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "typeless/UntypedCustomers/NS.MyAction");
            request.Headers.Add("accept", "application/json");
            HttpClient client = CreateClient();

            request.Content = new StringContent(EntityPayload);
            request.Content.Headers.ContentLength = EntityPayload.Length;
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/typeless/$metadata#Edm.Boolean\",\"value\":true", responseString);
        }

        [Theory]
        [InlineData("org.odata.DoSomething", "DoSomething")]
        [InlineData("customize.CNSAction", "CNSAction")]
        public async Task Response_Includes_ActionLink_WithAcceptHeader(string fullname, string name)
        {
            // Arrange
            string editLink = "http://localhost/odata/Customers(1)";
            string expectedTarget = editLink + "/" + fullname;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/Customers(1)");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            using (JsonDocument document = JsonDocument.Parse(responseString))
            {
                bool found = document.RootElement.TryGetProperty($"#{fullname}", out JsonElement action);
                Assert.True(found);

                bool titleFound = action.TryGetProperty("title", out JsonElement title);
                Assert.True(titleFound);
                Assert.Equal(name, title.GetString());

                bool targetFound = action.TryGetProperty("target", out JsonElement target);
                Assert.True(titleFound);
                Assert.Equal(expectedTarget, target.GetString());
            }
        }

        [Fact]
        public async Task Response_Includes_ActionLink_WithDollarFormat()
        {
            // Arrange
            string requestUri = "odata/Customers?$format=application/json;odata.metadata=full";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            using (JsonDocument document = JsonDocument.Parse(responseString))
            {
                JsonElement valueElement = document.RootElement.GetProperty("value");
                foreach (var item in valueElement.EnumerateArray())
                {
                    bool idFound = item.TryGetProperty($"ID", out JsonElement idNode);
                    Assert.True(idFound);
                    int id = idNode.GetInt32();

                    bool actionfound = item.TryGetProperty($"#org.odata.DoSomething", out JsonElement action);
                    Assert.True(actionfound);

                    bool targetFound = action.TryGetProperty("target", out JsonElement target);
                    Assert.True(targetFound);

                    Assert.Equal($"http://localhost/odata/Customers({id})/org.odata.DoSomething", target.GetString());
                }
            }
        }

        [Fact]
        public async Task Response_Includes_ActionLinkForFeed_WithAcceptHeader()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "odata/Customers");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            using (JsonDocument document = JsonDocument.Parse(responseString))
            {
                bool found = document.RootElement.TryGetProperty($"#org.odata.MyAction", out JsonElement action);
                Assert.True(found);

                bool titleFound = action.TryGetProperty("title", out JsonElement title);
                Assert.True(titleFound);
                Assert.Equal("MyAction", title.GetString());

                bool targetFound = action.TryGetProperty("target", out JsonElement target);
                Assert.True(titleFound);
                Assert.Equal("http://localhost/odata/Customers/org.odata.MyAction", target.GetString());
            }
        }

        [Fact]
        public async Task Response_Includes_ActionLinkForFeed_WithDollarFormat()
        {
            // Arrange
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("odata/Customers?$format=application/json;odata.metadata=full");
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("\"target\":\"http://localhost/odata/Customers/org.odata.MyAction\"", responseString);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.ContainerName = "Container";
            builder.Namespace = "org.odata";
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;
            ActionConfiguration action = customer.Action("DoSomething");
            action.Parameter<int>("p1");
            action.Parameter<Address>("p2");
            action.Parameter<Color?>("color");
            action.CollectionParameter<string>("p3");
            action.CollectionParameter<Address>("p4");
            action.CollectionParameter<Color?>("colors");

            action = customer.Collection.Action("MyAction");
            action.EntityParameter<Customer>("Customer");
            action.CollectionEntityParameter<Customer>("Customers");

            action = customer.Action("CNSAction");
            action.Namespace = "customize";
            return builder.GetEdmModel();
        }

        private static IEdmModel GetTypelessModel()
        {
            var model = new EdmModel();

            // entity type customer
            EdmEntityType customer = new EdmEntityType("org.odata", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(customer);

            EdmEntityType subCustomer = new EdmEntityType("org.odata", "SubCustomer", customer);
            customer.AddKeys(subCustomer.AddStructuralProperty("Price", EdmPrimitiveTypeKind.Double));
            model.AddElement(subCustomer);

            EdmAction action = new EdmAction("NS", "MyAction", null, isBound: true, entitySetPathExpression: null);
            action.AddParameter("bindingParameter",
                new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, isNullable: false))));
            action.AddParameter("Customer", new EdmEntityTypeReference(customer, isNullable: true));
            action.AddParameter("Customers",
                new EdmCollectionTypeReference(
                    new EdmCollectionType(new EdmEntityTypeReference(customer, isNullable: true))));
            model.AddElement(action);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            container.AddEntitySet("UntypedCustomers", customer);

            model.AddElement(container);
            return model;
        }

        public class Customer
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class SubCustomer : Customer
        {
            public double Price { get; set; }
        }

        public class Address
        {
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public int ZipCode { get; set; }
        }

        public enum Color
        {
            Red,
            Blue,
            Green
        }
    }

    // Controllers
    public class CustomersController : ODataController
    {
        [HttpGet]
        public IActionResult Get()
        {
            var customers = Enumerable.Range(1, 6).Select(i => new ODataActionTests.Customer
            {
                ID = i,
                Name = "Name " + i
            }).ToList();

            return Ok(customers);
        }

        [HttpGet]
        public ODataActionTests.Customer Get(int key)
        {
            return new ODataActionTests.Customer { ID = key, Name = "Name" + key.ToString() };
        }

        [HttpPost]
        public bool CNSAction(int key, ODataActionParameters parameters)
        {
            return true;
        }

        [HttpPost]
        public bool DoSomething(int key, ODataActionParameters parameters)
        {
            Assert.Equal(1, key);
            Assert.Equal(1, parameters["p1"]);
            ValidateAddress(parameters["p2"] as ODataActionTests.Address);
            ValidateNumbers((parameters["p3"] as IEnumerable<string>).ToList());
            ValidateAddresses((parameters["p4"] as IEnumerable<ODataActionTests.Address>).ToList());
            Assert.Equal(ODataActionTests.Color.Red, parameters["color"]);

            Assert.NotNull(parameters["colors"]);
            IList<ODataActionTests.Color?> colors = (parameters["colors"] as IEnumerable<ODataActionTests.Color?>).ToList();
            Assert.NotNull(colors);

            Assert.Equal(ODataActionTests.Color.Red, colors[0]);
            Assert.Null(colors[1]);
            Assert.Equal(ODataActionTests.Color.Green, colors[2]);

            return true;
        }

        [HttpPost]
        public bool MyAction(ODataActionParameters parameters)
        {
            Assert.True(parameters.ContainsKey("Customer"));
            ODataActionTests.Customer customer = parameters["Customer"] as ODataActionTests.Customer;
            Assert.NotNull(customer);
            Assert.Equal(101, customer.ID);
            Assert.Equal("Avatar", customer.Name);

            Assert.True(parameters.ContainsKey("Customers"));
            ValidateCustomers((parameters["Customers"] as IEnumerable<ODataActionTests.Customer>).ToList());

            return true;
        }

        private void ValidateAddress(ODataActionTests.Address address)
        {
            Assert.NotNull(address);
            Assert.Equal("1 Microsoft Way", address.StreetAddress);
            Assert.Equal("Redmond", address.City);
            Assert.Equal("WA", address.State);
            Assert.Equal(98052, address.ZipCode);
        }

        private void ValidateNumbers(IList<string> numbers)
        {
            Assert.NotNull(numbers);
            Assert.Equal(2, numbers.Count);
            Assert.Equal("one", numbers[0]);
            Assert.Equal("two", numbers[1]);
        }

        private void ValidateAddresses(IList<ODataActionTests.Address> addresses)
        {
            Assert.NotNull(addresses);
            Assert.Single(addresses);
            ValidateAddress(addresses[0]);
        }

        private void ValidateCustomers(IList<ODataActionTests.Customer> customers)
        {
            Assert.NotNull(customers);
            Assert.Equal(2, customers.Count);

            ODataActionTests.Customer customer = Assert.IsType<ODataActionTests.Customer>(customers[0]);
            Assert.NotNull(customer);
            Assert.Equal(901, customer.ID);
            Assert.Equal("John", customer.Name);

            ODataActionTests.SubCustomer subCustomer = Assert.IsType<ODataActionTests.SubCustomer>(customers[1]);
            Assert.NotNull(subCustomer);
            Assert.Equal(902, subCustomer.ID);
            Assert.Equal("Mike", subCustomer.Name);
            Assert.Equal(9.9, subCustomer.Price);
        }
    }

    public class UntypedCustomersController : ODataController
    {
        [HttpPost]
        public bool MyAction(ODataUntypedActionParameters parameters)
        {
            Assert.True(parameters.ContainsKey("Customer"));
            dynamic customer = parameters["Customer"] as EdmEntityObject;
            Assert.NotNull(customer);
            Assert.Equal(101, customer.ID);
            Assert.Equal("Avatar", customer.Name);

            Assert.True(parameters.ContainsKey("Customers"));
            IEnumerable<IEdmObject> customers = parameters["Customers"] as EdmEntityObjectCollection;

            Assert.Equal(2, customers.Count());
            EdmEntityObject entity = customers.First() as EdmEntityObject;
            IEdmTypeReference typeReference = entity.GetEdmType();
            Assert.Equal("org.odata.Customer", typeReference.FullName());

            customer = customers.First();
            Assert.NotNull(customer);
            Assert.Equal(901, customer.ID);
            Assert.Equal("John", customer.Name);

            entity = customers.Last() as EdmEntityObject;
            typeReference = entity.GetEdmType();
            Assert.Equal("org.odata.SubCustomer", typeReference.FullName());
            customer = customers.Last();
            Assert.NotNull(customer);
            Assert.Equal(902, customer.ID);
            Assert.Equal("Mike", customer.Name);
            Assert.Equal(9.9, customer.Price);

            return true;
        }
    }
}