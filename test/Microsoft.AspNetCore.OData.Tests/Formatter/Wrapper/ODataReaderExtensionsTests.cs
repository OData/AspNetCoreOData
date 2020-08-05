// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Wrapper
{
    public class ODataReaderExtensionsTests
    {
        private static readonly EdmModel Model;

        static ODataReaderExtensionsTests()
        {
            Model = new EdmModel();

            // Address
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmCoreModel.Instance.GetString(false));
            Model.AddElement(address);

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            Model.AddElement(customer);
            customer.AddKeys(customer.AddStructuralProperty("CustomerID", EdmCoreModel.Instance.GetInt32(false)));
            customer.AddStructuralProperty("Location", new EdmComplexTypeReference(address, false));

            // Order
            EdmEntityType order = new EdmEntityType("NS", "Order");
            Model.AddElement(order);
            order.AddKeys(order.AddStructuralProperty("OrderId", EdmCoreModel.Instance.GetInt32(false)));
            order.AddStructuralProperty("Price", EdmCoreModel.Instance.GetInt32(false));

            var orderNav = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Order",
                    Target = order,
                    TargetMultiplicity = EdmMultiplicity.ZeroOrOne
                });

            var ordresNav = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Orders",
                    Target = customer,
                    TargetMultiplicity = EdmMultiplicity.Many
                });

            EdmEntityContainer defaultContainer = new EdmEntityContainer("NS", "Container");
            Model.AddElement(defaultContainer);
            EdmEntitySet customers = defaultContainer.AddEntitySet("Customers", customer);
            EdmEntitySet orders = defaultContainer.AddEntitySet("Orders", order);
            customers.AddNavigationTarget(orderNav, orders);
            customers.AddNavigationTarget(ordresNav, orders);
        }

        [Fact]
        public void ReadResourceWorksAsExpected()
        {
            // Arrange
            const string payload =
            "{" +
                "\"@odata.context\":\"http://localhost/$metadata#Customers/$entity\"," +
                "\"CustomerID\": 7," +
                "\"Location\": { \"Street\":\"154TH AVE\"}," +
                "\"Order\": {\"OrderId\": 8, \"Price\": 82 }," +
                "\"Orders\": []" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, ODataReader> func = mr => mr.CreateODataResourceReader(customers, customers.EntityType());
            ODataItemBase item = ReadPayload(payload, Model, func);

            // Assert
            Assert.NotNull(item);
            ODataResourceWrapper resource = Assert.IsType<ODataResourceWrapper>(item);
            Assert.Equal(3, resource.NestedResourceInfos.Count);
            Assert.Equal(new[] { "Location", "Order", "Orders" }, resource.NestedResourceInfos.Select(n => n.NestedResourceInfo.Name));
        }

        [Fact]
        public void ReadResourceSetWorksAsExpected()
        {
            // Arrange
            const string payload =
            "{" +
                "\"@odata.context\":\"http://localhost/$metadata#Customers\"," +
                "\"value\": [" +
                 "{" +
                    "\"CustomerID\": 7," +
                    "\"Location\": { \"Street\":\"154TH AVE\"}," +
                    "\"Order\": {\"OrderId\": 8, \"Price\": 82 }," +
                    "\"Orders\": []" +
                   "}" +
                "]" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, ODataReader> func = mr => mr.CreateODataResourceSetReader(customers, customers.EntityType());
            ODataItemBase item = ReadPayload(payload, Model, func);

            // Assert
            Assert.NotNull(item);
            ODataResourceSetWrapper resourceSet = Assert.IsType<ODataResourceSetWrapper>(item);
            ODataResourceWrapper resource = Assert.Single(resourceSet.Resources);
            Assert.Equal(new[] { "Location", "Order", "Orders" }, resource.NestedResourceInfos.Select(n => n.NestedResourceInfo.Name));
        }

        private ODataItemBase ReadPayload(string payload, IEdmModel edmModel, Func<ODataMessageReader, ODataReader> createReader)
        {
            var message = new InMemoryMessage()
            {
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(payload))
            };
            message.SetHeader("Content-Type", "application/json;odata.metadata=minimal");

            ODataMessageReaderSettings readerSettings = new ODataMessageReaderSettings()
            {
                BaseUri = new Uri("http://localhost/$metadata"),
                EnableMessageStreamDisposal = true,
                Version = ODataVersion.V4,
            };

            using (var msgReader = new ODataMessageReader((IODataResponseMessage)message, readerSettings, edmModel))
            {
                ODataReader reader = createReader(msgReader);
                return reader.ReadResourceOrResourceSet();
            }
        }
    }
}