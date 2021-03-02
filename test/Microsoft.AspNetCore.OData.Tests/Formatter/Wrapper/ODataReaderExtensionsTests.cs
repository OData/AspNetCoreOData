// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            customer.AddStructuralProperty("Name", EdmCoreModel.Instance.GetString(true));
            customer.AddStructuralProperty("Location", new EdmComplexTypeReference(address, false));

            // Order
            EdmEntityType order = new EdmEntityType("NS", "Order");
            Model.AddElement(order);
            order.AddKeys(order.AddStructuralProperty("OrderId", EdmCoreModel.Instance.GetInt32(false)));
            order.AddStructuralProperty("Price", EdmCoreModel.Instance.GetInt32(false));

            // VipOrder
            EdmEntityType vipOrder = new EdmEntityType("NS", "VipOrder", order);
            Model.AddElement(vipOrder);
            vipOrder.AddKeys(vipOrder.AddStructuralProperty("Email", EdmCoreModel.Instance.GetString(false)));

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
                    Target = order,
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
        public async Task ReadResourceWorksAsExpected()
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
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(customers, customers.EntityType());
            ODataItemWrapper item = await ReadPayloadAsync(payload, Model, func);

            // Assert
            Assert.NotNull(item);
            ODataResourceWrapper resource = Assert.IsType<ODataResourceWrapper>(item);
            Assert.Equal(3, resource.NestedResourceInfos.Count);
            Assert.Equal(new[] { "Location", "Order", "Orders" }, resource.NestedResourceInfos.Select(n => n.NestedResourceInfo.Name));
        }

        [Fact]
        public async Task ReadResourceSetWorksAsExpected()
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
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceSetReaderAsync(customers, customers.EntityType());
            ODataItemWrapper item = await ReadPayloadAsync(payload, Model, func);

            // Assert
            Assert.NotNull(item);
            ODataResourceSetWrapper resourceSet = Assert.IsType<ODataResourceSetWrapper>(item);
            ODataResourceWrapper resource = Assert.Single(resourceSet.Resources);
            Assert.Equal(new[] { "Location", "Order", "Orders" }, resource.NestedResourceInfos.Select(n => n.NestedResourceInfo.Name));
        }

        [Fact]
        public async Task ReadResourceSetWithNestedResourceSetWorksAsExpected()
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
                    "\"Orders\": [" +
                        "{\"OrderId\": 8, \"Price\": 82 }," +
                        "{\"@odata.type\": \"#NS.VipOrder\",\"OrderId\": 9, \"Price\": 42, \"Email\": \"abc@efg.com\" }" +
                      "]" +
                   "}" +
                "]" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceSetReaderAsync(customers, customers.EntityType());
            ODataItemWrapper item = await ReadPayloadAsync(payload, Model, func);

            // Assert
            Assert.NotNull(item);
            ODataResourceSetWrapper resourceSet = Assert.IsType<ODataResourceSetWrapper>(item);
            ODataResourceWrapper resource = Assert.Single(resourceSet.Resources);
            Assert.Equal(new[] { "Location", "Order", "Orders" }, resource.NestedResourceInfos.Select(n => n.NestedResourceInfo.Name));

            ODataNestedResourceInfoWrapper orders = resource.NestedResourceInfos.First(n => n.NestedResourceInfo.Name == "Orders");
            Assert.Null(orders.NestedResource); // not a child resource
            Assert.Null(orders.NestedLinks); // not a child reference link(s)
            Assert.NotNull(orders.NestedResourceSet);

            ODataResourceSetWrapper ordersSet = Assert.IsType<ODataResourceSetWrapper>(orders.NestedResourceSet);
            Assert.Equal(2, ordersSet.Resources.Count);
            Assert.Collection(ordersSet.Resources,
                r =>
                {
                    Assert.Equal("NS.Order", r.Resource.TypeName);
                    Assert.Equal(82, r.Resource.Properties.First(p => p.Name == "Price").Value);
                },
                r =>
                {
                    Assert.Equal("NS.VipOrder", r.Resource.TypeName);
                    Assert.Equal("abc@efg.com", r.Resource.Properties.First(p => p.Name == "Email").Value);
                });
        }

        [Fact]
        public async Task ReadDeltaResourceSetWorksAsExpected()
        {
            // Arrange
            string payload = "{\"@context\":\"http://example.com/$metadata#Customers/$delta\"," +
                "\"value\":[" +
                  "{" +
                    "\"@removed\":{\"reason\":\"changed\"}," +
                    "\"CustomerID\":1," +
                    "\"Orders@delta\":[" +
                      "{" +
                        "\"@removed\":{\"reason\":\"deleted\"}," +
                        "\"OrderId\":10" +
                      "}," +
                      "{" +
                        "\"@type\":\"#NS.VipOrder\"," +
                        "\"OrderId\":9," +
                        "\"Email\":\"a@abc.com\"" +
                      "}" +
                    "]" +
                  "}" +
                "]" +
              "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataDeltaResourceSetReaderAsync(customers, customers.EntityType());
            ODataItemWrapper item = await ReadPayloadAsync(payload, Model, func, ODataVersion.V401);

            // Assert
            Assert.NotNull(item);

            // --- DeltaResourceSet
            //      |--- DeleteResource (1)
            //          |--- NestedResourceInfo (1-1)
            //               |--- DeltaResourceSet
            //                     |--- DelteResource
            //                     |--- Normal Resource
            ODataDeltaResourceSetWrapper deltaResourceSet = Assert.IsType<ODataDeltaResourceSetWrapper>(item);
            ODataItemWrapper deltaItem = Assert.Single(deltaResourceSet.DeltaItems);
            ODataDeletedResourceWrapper deletedResource = Assert.IsType<ODataDeletedResourceWrapper>(deltaItem);
            Assert.Equal(DeltaDeletedEntryReason.Changed, deletedResource.DeletedResource.Reason);

            ODataNestedResourceInfoWrapper nestedResourceInfo = Assert.Single(deletedResource.NestedResourceInfos);
            Assert.Equal("Orders", nestedResourceInfo.NestedResourceInfo.Name);
            Assert.True(nestedResourceInfo.NestedResourceInfo.IsCollection);

            ODataDeltaResourceSetWrapper ordersDeltaResourceSet = Assert.IsType<ODataDeltaResourceSetWrapper>(nestedResourceInfo.NestedResourceSet);
            Assert.Equal(2, ordersDeltaResourceSet.DeltaItems.Count);
            ODataDeletedResourceWrapper deletedResource1 = Assert.IsType<ODataDeletedResourceWrapper>(ordersDeltaResourceSet.DeltaItems.ElementAt(0));
            Assert.Equal(DeltaDeletedEntryReason.Deleted, deletedResource1.DeletedResource.Reason);

            ODataResourceWrapper resource2 = Assert.IsType<ODataResourceWrapper>(ordersDeltaResourceSet.DeltaItems.ElementAt(1));
            Assert.Equal("NS.VipOrder", resource2.Resource.TypeName);
            Assert.Collection(resource2.Resource.Properties,
                p =>
                {
                    Assert.Equal("OrderId", p.Name);
                    Assert.Equal(9, p.Value);
                },
                p =>
                {
                    Assert.Equal("Email", p.Name);
                    Assert.Equal("a@abc.com", p.Value);
                });
        }

        [Fact]
        public async Task ReadDeletedLinkInDeltaResourceSetWorksAsExpected()
        {
            // Arrange
            string payload = "{" +
                    "\"@odata.context\":\"http://localhost/$metadata#Customers/$delta\"," +
                    "\"@odata.count\":5," +
                    "\"value\":[" +
                       "{" +
                          "\"@odata.id\":\"Customers(42)\"," +
                          "\"Name\":\"Sammy\"" +
                       "}," +
                       "{" +
                          "\"@odata.context\":\"http://localhost/$metadata#Customers/$deletedLink\"," +
                          "\"source\":\"Customers(39)\"," +
                          "\"relationship\":\"Orders\"," +
                          "\"target\":\"Orders(10643)\"" +
                       "}," +
                       "{" +
                          "\"@odata.context\":\"http://localhost/$metadata#Customers/$link\"," +
                          "\"source\":\"Customers(32)\"," +
                          "\"relationship\":\"Orders\"," +
                          "\"target\":\"Orders(10645)\"" +
                       "}," +
                       "{" +
                          "\"@odata.context\":\"http://localhost/$metadata#Orders/$entity\"," +
                          "\"@odata.id\":\"Orders(10643)\"," +
                          "\"Price\": 82" +
                       "}," +
                       "{" +
                          "\"@odata.context\":\"http://localhost/$metadata#Customers/$deletedEntity\"," +
                          "\"id\":\"Customers(21)\"," +
                          "\"reason\":\"deleted\"" +
                       "}" +
                    "]," +
                    "\"@odata.deltaLink\":\"Customers?$expand=Orders&$deltatoken=8015\"" +
                  "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataDeltaResourceSetReaderAsync(customers, customers.EntityType());
            ODataItemWrapper item = await ReadPayloadAsync(payload, Model, func);

            // Assert
            Assert.NotNull(item);

            // --- DeltaResourceSet
            //      |--- Resource (1)
            //      |--- DeltaDeletedLink
            //      |--- DeltaLink
            //      |--- Resource (2)
            //      |--- DeletedResource (1)
            //      |
            ODataDeltaResourceSetWrapper deltaResourceSet = Assert.IsType<ODataDeltaResourceSetWrapper>(item);

            // Resources
            Assert.Equal(5, deltaResourceSet.DeltaItems.Count);
            Assert.Collection(deltaResourceSet.DeltaItems,
                e =>
                {
                    // 1) Resource
                    ODataResourceWrapper resource1 = Assert.IsType<ODataResourceWrapper>(e);
                    Assert.Equal("Customers(42)", resource1.Resource.Id.OriginalString);
                    Assert.Equal("Sammy", resource1.Resource.Properties.First(p => p.Name == "Name").Value);
                },
                e =>
                {
                    // 2) Deleted Link
                    ODataDeltaDeletedLinkWrapper deletedLinkWrapper = Assert.IsType<ODataDeltaDeletedLinkWrapper>(e);
                    Assert.Equal("Customers(39)", deletedLinkWrapper.DeltaDeletedLink.Source.OriginalString);
                    Assert.Equal("Orders(10643)", deletedLinkWrapper.DeltaDeletedLink.Target.OriginalString);
                    Assert.Equal("Orders", deletedLinkWrapper.DeltaDeletedLink.Relationship);
                },
                e =>
                {
                    // 3) Added Link
                    ODataDeltaLinkWrapper linkWrapper = Assert.IsType<ODataDeltaLinkWrapper>(e);
                    Assert.Equal("Customers(32)", linkWrapper.DeltaLink.Source.OriginalString);
                    Assert.Equal("Orders(10645)", linkWrapper.DeltaLink.Target.OriginalString);
                    Assert.Equal("Orders", linkWrapper.DeltaLink.Relationship);
                },
                e =>
                {
                    // 4) Resource
                    ODataResourceWrapper resource2 = Assert.IsType<ODataResourceWrapper>(e);
                    Assert.Equal("Orders(10643)", resource2.Resource.Id.OriginalString);
                    Assert.Equal(82, resource2.Resource.Properties.First(p => p.Name == "Price").Value);
                },
                e =>
                {
                    // 5) Deleted resource
                    ODataDeletedResourceWrapper deletedResource = Assert.IsType<ODataDeletedResourceWrapper>(e);
                    Assert.Equal("Customers(21)", deletedResource.DeletedResource.Id.OriginalString);
                    Assert.Equal(DeltaDeletedEntryReason.Deleted, deletedResource.DeletedResource.Reason);
                });
        }

        [Theory]
        [InlineData(ODataVersion.V4, "\"Order@odata.bind\":\"http://svc/Orders(7)\"")]
        [InlineData(ODataVersion.V401, "\"Order\": {\"@id\": \"http://svc/Orders(8)\"}")]
        public async Task ReadSingleEntityReferenceLinkWorksAsExpected(ODataVersion version, string referenceLink)
        {
            // Arrange
            string payload = "{" + // -> ResourceStart
                "\"@odata.context\":\"http://localhost/$metadata#Customers/$entity\"," +
                "\"CustomerID\": 7," +
                referenceLink +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(customers, customers.EntityType());
            ODataItemWrapper item = await ReadPayloadAsync(payload, Model, func, version);

            // Assert
            Assert.NotNull(item);
            ODataResourceWrapper resource = Assert.IsType<ODataResourceWrapper>(item);
            ODataNestedResourceInfoWrapper nestedResourceInfo = Assert.Single(resource.NestedResourceInfos);
            Assert.Equal("Order", nestedResourceInfo.NestedResourceInfo.Name);
            if (version == ODataVersion.V401)
            {
                // --- Resource
                //     |--- NestedResourceInfo (Order)
                //          |--- Resource
                ODataResourceWrapper order = Assert.IsType<ODataResourceWrapper>(nestedResourceInfo.NestedResource);
                Assert.Equal("http://svc/Orders(8)", order.Resource.Id.OriginalString);
                Assert.Empty(order.Resource.Properties);
            }
            else
            {
                // --- Resource
                //     |--- NestedResourceInfo (Order)
                //          |--- EntityReferenceLink
                ODataEntityReferenceLinkWrapper orderLink = Assert.IsType<ODataEntityReferenceLinkWrapper>(Assert.Single(nestedResourceInfo.NestedLinks));
                Assert.Equal("http://svc/Orders(7)", orderLink.EntityReferenceLink.Url.OriginalString);
            }
        }

        [Fact]
        public async Task ReadEntityReferenceLinksSetWorksAsExpected_V40()
        {
            // Arrange
            string payload = "{" + // -> ResourceStart
                "\"@odata.context\":\"http://localhost/$metadata#Customers/$entity\"," +
                "\"CustomerID\": 7," +
                "\"Orders@odata.bind\":[" +  // -> NestedResourceInfoStart
                    "\"http://svc/Orders(2)\"," +
                    "\"http://svc/Orders(3)\"," +
                    "\"http://svc/Orders(4)\"" +
                "]" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(customers, customers.EntityType());
            ODataItemWrapper item = await ReadPayloadAsync(payload, Model, func, ODataVersion.V4);

            // Assert
            Assert.NotNull(item);

            // --- Resource
            //     |--- NestedResourceInfo
            //           |--- EntityReferceLink (2)
            //           |--- EntityReferceLink (3)
            //           |--- EntityReferceLink (4)
            ODataResourceWrapper resource = Assert.IsType<ODataResourceWrapper>(item);

            ODataNestedResourceInfoWrapper nestedResourceInfo = Assert.Single(resource.NestedResourceInfos);
            Assert.Equal("Orders", nestedResourceInfo.NestedResourceInfo.Name);
            Assert.True(nestedResourceInfo.NestedResourceInfo.IsCollection);

            Assert.NotNull(nestedResourceInfo.NestedLinks);
            Assert.Equal(3, nestedResourceInfo.NestedLinks.Count);
            Assert.Collection(nestedResourceInfo.NestedLinks,
                r =>
                {
                    Assert.Equal("http://svc/Orders(2)", r.EntityReferenceLink.Url.OriginalString);
                },
                r =>
                {
                    Assert.Equal("http://svc/Orders(3)", r.EntityReferenceLink.Url.OriginalString);
                },
                r =>
                {
                    Assert.Equal("http://svc/Orders(4)", r.EntityReferenceLink.Url.OriginalString);
                });
        }

        [Fact]
        public async Task ReadEntityReferenceLinksSetWorksAsExpected_V401()
        {
            // Arrange
            string payload = "{" + // -> ResourceStart
                "\"@odata.context\":\"http://localhost/$metadata#Customers/$entity\"," +
                "\"CustomerID\": 7," +
                "\"Orders\":[" +  // -> NestedResourceInfoStart
                    "{ \"@id\": \"http://svc/Orders(2)\" }," +
                    "{ \"@id\": \"http://svc/Orders(3)\" }," +
                    "{ \"@id\": \"http://svc/Orders(4)\" }" +
                "]" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(customers, customers.EntityType());
            ODataItemWrapper item = await ReadPayloadAsync(payload, Model, func, ODataVersion.V401);

            // Assert
            Assert.NotNull(item);

            // --- Resource
            //     |--- NestedResourceInfo
            //        |--- NestedResourceSet
            //              |--- Resource (2)
            //              |--- Resource (3)
            //              |--- Resource (4)
            ODataResourceWrapper resource = Assert.IsType<ODataResourceWrapper>(item);

            ODataNestedResourceInfoWrapper nestedResourceInfo = Assert.Single(resource.NestedResourceInfos);
            Assert.Equal("Orders", nestedResourceInfo.NestedResourceInfo.Name);
            Assert.True(nestedResourceInfo.NestedResourceInfo.IsCollection);

            ODataResourceSetWrapper ordersResourceSet = Assert.IsType<ODataResourceSetWrapper>(nestedResourceInfo.NestedResourceSet);
            Assert.Equal(3, ordersResourceSet.Resources.Count);
            Assert.Collection(ordersResourceSet.Resources,
                r =>
                {
                    Assert.Equal("http://svc/Orders(2)", r.Resource.Id.OriginalString);
                },
                r =>
                {
                    Assert.Equal("http://svc/Orders(3)", r.Resource.Id.OriginalString);
                },
                r =>
                {
                    Assert.Equal("http://svc/Orders(4)", r.Resource.Id.OriginalString);
                });
        }

        private async Task<ODataItemWrapper> ReadPayloadAsync(string payload,
            IEdmModel edmModel, Func<ODataMessageReader, Task<ODataReader>> createReader, ODataVersion version = ODataVersion.V4)
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
                Version = version,
            };

            using (var msgReader = new ODataMessageReader((IODataRequestMessageAsync)message, readerSettings, edmModel))
            {
                ODataReader reader = await createReader(msgReader);
                return await reader.ReadResourceOrResourceSetAsync();
            }
        }
    }
}