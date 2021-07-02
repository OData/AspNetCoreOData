// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.E2E.Tests.Extensions;
using Microsoft.AspNetCore.OData.E2E.Tests.Typeless;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{
    public class TypelessSerializationTests : WebApiTestBase<TypelessSerializationTests>
    {
        public TypelessSerializationTests(WebApiTestFixture<TypelessSerializationTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                IEdmModel edmModel = GetEdmModel();
                services.ConfigureControllers(typeof(TypelessCustomersController));
                services.AddControllers().AddOData(opt => opt.Expand().AddModel("odata", edmModel));
            };
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

}
