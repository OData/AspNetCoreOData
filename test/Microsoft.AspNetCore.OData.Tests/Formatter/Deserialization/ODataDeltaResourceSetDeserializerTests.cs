// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class ODataDeltaResourceSetDeserializerTests
    {
        [Fact]
        public void ReadDeltaResourceSet_Calls_ReadInlineForEachDeltaItem()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmEntityType customer = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            IEdmStructuredTypeReference elementType = new EdmEntityTypeReference(customer, true);

            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            Mock<ODataEdmTypeDeserializer> resourceDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Resource);

            ODataDeltaResourceSetDeserializer deserializer = new ODataDeltaResourceSetDeserializer(deserializerProvider.Object);

            ODataDeltaResourceSetWrapper deltaResourceSetWrapper = new ODataDeltaResourceSetWrapper(new ODataDeltaResourceSet());

            Uri source = new Uri("Customers(8)", UriKind.RelativeOrAbsolute);
            Uri target = new Uri("Orders(10645)", UriKind.RelativeOrAbsolute);
            deltaResourceSetWrapper.DeltaItems.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a1/") }));
            deltaResourceSetWrapper.DeltaItems.Add(new ODataResourceWrapper(new ODataDeletedResource { Id = new Uri("http://a2/") }));
            ODataDeserializerContext readContext = new ODataDeserializerContext()
            {
                Model = model,
                ResourceType = typeof(DeltaSet<>)
            };

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(elementType, false)).Returns(resourceDeserializer.Object);
            resourceDeserializer.Setup(d => d.ReadInline(deltaResourceSetWrapper.DeltaItems[0], elementType, It.IsAny<ODataDeserializerContext>())).Returns("entry1").Verifiable();
            resourceDeserializer.Setup(d => d.ReadInline(deltaResourceSetWrapper.DeltaItems[1], elementType, It.IsAny<ODataDeserializerContext>())).Returns("entry2").Verifiable();

            // Act
            var result = deserializer.ReadDeltaResourceSet(deltaResourceSetWrapper, elementType, readContext);

            // Assert
            Assert.Equal(new[] { "entry1", "entry2" }, result.OfType<String>());
            resourceDeserializer.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReadDeltaResource_Returns_DeletedResource(bool typed)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmEntityType customer = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            IEdmStructuredTypeReference elementType = new EdmEntityTypeReference(customer, true);

            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            ODataResourceDeserializer resourceDeserializer = new ODataResourceDeserializer(deserializerProvider.Object);

            Uri id = new Uri("Customers(8)", UriKind.RelativeOrAbsolute);
            ODataDeletedResource customerDeleted = new ODataDeletedResource(id, DeltaDeletedEntryReason.Deleted)
            {
                Properties = new List<ODataProperty>
                {
                    new ODataProperty { Name = "FirstName", Value = "Peter" },
                    new ODataProperty { Name = "LastName", Value = "John" }
                }
            };
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(customerDeleted);
            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = model,
            };

            if (typed)
            {
                context.ResourceType = typeof(DeltaSet<>);
            }
            else
            {
                context.ResourceType = typeof(EdmChangedObjectCollection);
            }

            deserializerProvider.Setup(d => d.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>(), false)).Returns(resourceDeserializer);
            ODataDeltaResourceSetDeserializer deserializer = new ODataDeltaResourceSetDeserializer(deserializerProvider.Object);

            // Act
            object result = deserializer.ReadDeltaResource(resourceWrapper, elementType, context);

            // Assert
            Action<Delta> testPropertyAction = d =>
            {
                d.TryGetPropertyValue("FirstName", out object firstName);
                Assert.Equal("Peter", firstName);
                d.TryGetPropertyValue("LastName", out object lastName);
                Assert.Equal("John", lastName);
            };

            if (typed)
            {
                DeltaDeletedResource<Customer> deltaDeletedResource = Assert.IsType<DeltaDeletedResource<Customer>>(result);
                Assert.Equal(id, deltaDeletedResource.Id);
                Assert.Equal(DeltaDeletedEntryReason.Deleted, deltaDeletedResource.Reason);
                testPropertyAction(deltaDeletedResource);
            }
            else
            {
                EdmDeltaDeletedResourceObject deltaDeletedResource = Assert.IsType<EdmDeltaDeletedResourceObject>(result);
                Assert.Equal(id, deltaDeletedResource.Id);
                Assert.Equal(DeltaDeletedEntryReason.Deleted, deltaDeletedResource.Reason);
                testPropertyAction(deltaDeletedResource);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReadDeltaDeletedLink_Returns_DeletedDeltaLink(bool typed)
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            ODataDeltaResourceSetDeserializer deserializer = new ODataDeltaResourceSetDeserializer(deserializerProvider.Object);

            Uri source = new Uri("Customers(8)", UriKind.RelativeOrAbsolute);
            Uri target = new Uri("Orders(10645)", UriKind.RelativeOrAbsolute);
            ODataDeltaDeletedLink deletedLink = new ODataDeltaDeletedLink(source, target, "Orders");
            ODataDeltaDeletedLinkWrapper wrapper = new ODataDeltaDeletedLinkWrapper(deletedLink);

            IEdmModel model = GetEdmModel();
            IEdmEntityType customer = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            IEdmStructuredTypeReference elementType = new EdmEntityTypeReference(customer, true);

            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = model,
                ResourceType = typeof(DeltaSet<>)
            };

            if (typed)
            {
                context.ResourceType = typeof(DeltaSet<>);
            }
            else
            {
                context.ResourceType = typeof(EdmChangedObjectCollection);
            }

            // Act
            object deltaLinkObject = deserializer.ReadDeltaDeletedLink(wrapper, elementType, context);

            // Assert
            if (typed)
            {
                DeltaDeletedLink<Customer> deltaDeletedLink = Assert.IsType<DeltaDeletedLink<Customer>>(deltaLinkObject);
                Assert.Equal(source, deltaDeletedLink.Source);
                Assert.Equal(target, deltaDeletedLink.Target);
                Assert.Equal("Orders", deltaDeletedLink.Relationship);
            }
            else
            {
                EdmDeltaDeletedLink deltaDeletedLink = Assert.IsType<EdmDeltaDeletedLink>(deltaLinkObject);
                Assert.Equal(source, deltaDeletedLink.Source);
                Assert.Equal(target, deltaDeletedLink.Target);
                Assert.Equal("Orders", deltaDeletedLink.Relationship);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReadDeltaLink_Returns_DeltaLink(bool typed)
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            ODataDeltaResourceSetDeserializer deserializer = new ODataDeltaResourceSetDeserializer(deserializerProvider.Object);

            Uri source = new Uri("Customers(8)", UriKind.RelativeOrAbsolute);
            Uri target = new Uri("Orders(10645)", UriKind.RelativeOrAbsolute);
            ODataDeltaLink link = new ODataDeltaLink(source, target, "Orders");
            ODataDeltaLinkWrapper wrapper = new ODataDeltaLinkWrapper(link);

            IEdmModel model = GetEdmModel();
            IEdmEntityType customer = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            IEdmStructuredTypeReference elementType = new EdmEntityTypeReference(customer, true);

            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = model,
                ResourceType = typeof(DeltaSet<>)
            };

            if (typed)
            {
                context.ResourceType = typeof(DeltaSet<>);
            }
            else
            {
                context.ResourceType = typeof(EdmChangedObjectCollection);
            }

            // Act
            object deltaLinkObject = deserializer.ReadDeltaLink(wrapper, elementType, context);

            // Assert
            if (typed)
            {
                DeltaLink<Customer> deltaLink = Assert.IsType<DeltaLink<Customer>>(deltaLinkObject);
                Assert.Equal(source, deltaLink.Source);
                Assert.Equal(target, deltaLink.Target);
                Assert.Equal("Orders", deltaLink.Relationship);
            }
            else
            {
                EdmDeltaLink deltaLink = Assert.IsType<EdmDeltaLink>(deltaLinkObject);
                Assert.Equal(source, deltaLink.Source);
                Assert.Equal(target, deltaLink.Target);
                Assert.Equal("Orders", deltaLink.Relationship);
            }
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }

        public class Customer
        {
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Address HomeAddress { get; set; }

            public IList<Order> Orders { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string ZipCode { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
        }
    }
}