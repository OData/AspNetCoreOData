// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataDeltaResourceSetSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer[] _customers;
        EdmChangedObjectCollection _deltaFeedCustomers;
        ODataDeltaResourceSetSerializer _serializer;
        IEdmCollectionTypeReference _customersType;
        private ODataPath _path;
        ODataSerializerContext _writeContext;

        public ODataDeltaResourceSetSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            _model.SetAnnotationValue(_customerSet.EntityType(), new ClrTypeAnnotation(typeof(Customer)));
            _path = new ODataPath(new EntitySetSegment(_customerSet));
            _customers = new[] {
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 10,
                    HomeAddress = new Address()
                    {
                        Street = "Street",
                        ZipCode = null,
                    }
                },
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 42,
                }
            };

            _deltaFeedCustomers = new EdmChangedObjectCollection(_customerSet.EntityType());
            EdmDeltaEntityObject newCustomer = new EdmDeltaEntityObject(_customerSet.EntityType());
            newCustomer.TrySetPropertyValue("ID", 10);
            newCustomer.TrySetPropertyValue("FirstName", "Foo");
            EdmDeltaComplexObject newCustomerAddress = new EdmDeltaComplexObject(_model.FindType("Default.Address") as IEdmComplexType);
            newCustomerAddress.TrySetPropertyValue("Street", "Street");
            newCustomerAddress.TrySetPropertyValue("ZipCode", null);
            newCustomer.TrySetPropertyValue("HomeAddress", newCustomerAddress);
            _deltaFeedCustomers.Add(newCustomer);

            _customersType = _model.GetEdmTypeReference(typeof(Customer[])).AsCollection();
            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model, Path = _path };
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataDeltaResourceSetSerializer(serializerProvider: null), "serializerProvider");
        }

        [Fact]
        public async Task WriteObjectAsync_ThrowsArgumentNull_MessageWriter()
        {
            // Arrange
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(provider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(graph: null, type: null, messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public async Task WriteObjectAsync_ThrowsArgumentNull_WriteContext()
        {
            // Arrange
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(provider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public async Task WriteObjectAsync_Throws_EntitySetMissingDuringSerialization()
        {
            // Arrange
            object graph = new object();
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(provider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteObjectAsync(graph: graph, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: new ODataSerializerContext()),
                "The related entity set could not be found from the OData path. The related entity set is required to serialize the payload.");
        }

        [Fact]
        public async Task WriteObjectAsync_Calls_WriteDeltaFeedInline()
        {
            // Arrange
            object graph = new object();
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            Mock<ODataDeltaResourceSetSerializer> serializer = new Mock<ODataDeltaResourceSetSerializer>(provider.Object);
            serializer.CallBase = true;
            serializer
                .Setup(s => s.WriteDeltaFeedInlineAsync(graph, It.Is<IEdmTypeReference>(e => _customersType.IsEquivalentTo(e)),
                    It.IsAny<ODataWriter>(), _writeContext))
                .Returns(Task.CompletedTask)
                .Verifiable();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessageAsync message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings()
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(message, settings, _model);

            // Act
            await serializer.Object.WriteObjectAsync(graph, typeof(Customer[]), messageWriter, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_ThrowsArgumentNull_Writer()
        {
            // Arrange
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(provider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteDeltaFeedInlineAsync(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_ThrowsArgumentNull_WriteContext()
        {
            // Arrange
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(provider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteDeltaFeedInlineAsync(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_ThrowsSerializationException_CannotSerializerNull()
        {
            // Arrange
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(provider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteDeltaFeedInlineAsync(graph: null, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "Cannot serialize a null 'deltafeed'.");
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_ThrowsSerializationException_IfGraphIsNotEnumerable()
        {
            // Arrange
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(provider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteDeltaFeedInlineAsync(graph: 42, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "ODataDeltaResourceSetSerializer cannot write an object of type 'System.Int32'.");
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_Throws_NullElementInCollection_IfFeedContainsNullElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(provider.Object);

            // Act
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteDeltaFeedInlineAsync(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "Collections cannot contain null elements.");
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_Throws_TypeCannotBeSerialized_IfFeedContainsEntityThatCannotBeSerialized()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var request = RequestFactory.Create();
            serializerProvider.Setup(s => s.GetODataPayloadSerializer(typeof(int), request)).Returns<ODataSerializer>(null);
            IEnumerable instance = new object[] { 42 };
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(serializerProvider.Object);

            // Act
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteDeltaFeedInlineAsync(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "ODataDeltaResourceSetSerializer cannot write an object of type 'System.Object[]'.");
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_Calls_CreateODataDeltaFeed()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            Mock<ODataDeltaResourceSetSerializer> serializer = new Mock<ODataDeltaResourceSetSerializer>(provider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(new ODataDeltaResourceSet()).Verifiable();

            // Act
            await serializer.Object.WriteDeltaFeedInlineAsync(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_Throws_CannotSerializerNull_IfCreateODataDeltaFeedReturnsNull()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            Mock<ODataDeltaResourceSetSerializer> serializer = new Mock<ODataDeltaResourceSetSerializer>(provider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns<ODataDeltaResourceSet>(null);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.Object.WriteDeltaFeedInlineAsync(instance, _customersType, writer, _writeContext),
                "Cannot serialize a null 'deltafeed'.");
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_Writes_CreateODataFeedOutput()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataDeltaResourceSet deltafeed = new ODataDeltaResourceSet();
            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            Mock<ODataDeltaResourceSetSerializer> serializer = new Mock<ODataDeltaResourceSetSerializer>(provider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(s => s.WriteStartAsync(deltafeed)).Verifiable();

            // Act
            await serializer.Object.WriteDeltaFeedInlineAsync(instance, _customersType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_Can_WriteCollectionOfIEdmChangedObjects()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            IEdmCollectionTypeReference feedType = new EdmCollectionTypeReference(new EdmCollectionType(edmType));
            Mock<IEdmChangedObject> edmObject = new Mock<IEdmChangedObject>();
            edmObject.Setup(e => e.GetEdmType()).Returns(edmType);

            var mockWriter = new Mock<ODataWriter>();

            Mock<ODataSerializerProvider> provider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSerializer> customerSerializer = new Mock<ODataResourceSerializer>(provider.Object);
            customerSerializer.Setup(s => s.WriteDeltaObjectInlineAsync(edmObject.Object, edmType, mockWriter.Object, _writeContext)).Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(customerSerializer.Object);

            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(serializerProvider.Object);

            // Act
            await serializer.WriteDeltaFeedInlineAsync(new[] { edmObject.Object }, feedType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_WritesEachEntityInstance()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializeProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSerializer> customerSerializer = new Mock<ODataResourceSerializer>(serializeProvider.Object);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();
            customerSerializer.Setup(s => s.WriteDeltaObjectInlineAsync(_deltaFeedCustomers[0], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();
            _serializer = new ODataDeltaResourceSetSerializer(provider);

            // Act
            await _serializer.WriteDeltaFeedInlineAsync(_deltaFeedCustomers, _customersType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_Sets_NextPageLink_OnWriteEndAsync()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataDeltaResourceSet deltafeed = new ODataDeltaResourceSet { NextPageLink = new Uri("http://nextlink.com/") };
            Mock<ODataSerializerProvider> serializeProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataDeltaResourceSetSerializer> serializer = new Mock<ODataDeltaResourceSetSerializer>(serializeProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStartAsync(It.Is<ODataDeltaResourceSet>(f => f.NextPageLink == null))).Verifiable();
            mockWriter
                .Setup(m => m.WriteEndAsync())
                .Callback(() =>
                {
                    Assert.Equal("http://nextlink.com/", deltafeed.NextPageLink.AbsoluteUri);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await serializer.Object.WriteDeltaFeedInlineAsync(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public async Task WriteDeltaFeedInlineAsync_Sets_DeltaLink()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataDeltaResourceSet deltafeed = new ODataDeltaResourceSet { DeltaLink = new Uri("http://deltalink.com/") };
            Mock<ODataSerializerProvider> serializeProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataDeltaResourceSetSerializer> serializer = new Mock<ODataDeltaResourceSetSerializer>(serializeProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStartAsync(deltafeed));
            mockWriter
                .Setup(m => m.WriteEndAsync())
                .Callback(() =>
                {
                    Assert.Equal("http://deltalink.com/", deltafeed.DeltaLink.AbsoluteUri);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await serializer.Object.WriteDeltaFeedInlineAsync(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void CreateODataDeltaFeed_Sets_CountValueForPageResult()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializeProvider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(serializeProvider.Object);
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataDeltaResourceSet feed = serializer.CreateODataDeltaFeed(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(ExpectedCountValue, feed.Count);
        }

        [Fact]
        public void CreateODataDeltaFeed_Sets_NextPageLinkForPageResult()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializeProvider = new Mock<ODataSerializerProvider>();
            ODataDeltaResourceSetSerializer serializer = new ODataDeltaResourceSetSerializer(serializeProvider.Object);
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataDeltaResourceSet feed = serializer.CreateODataDeltaFeed(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(expectedNextLink, feed.NextPageLink);
        }

        public class Customer
        {
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Address HomeAddress { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string ZipCode { get; set; }
        }
    }
}