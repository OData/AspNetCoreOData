// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Edm;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Formatter.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataResourceSetSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer[] _customers;
        ODataResourceSetSerializer _serializer;
        IEdmCollectionTypeReference _customersType;
        IEdmCollectionTypeReference _addressesType;
        ODataSerializerContext _writeContext;

        public ODataResourceSetSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            IEdmComplexType addressType = _model.SchemaElements.OfType<IEdmComplexType>()
                .First(c => c.Name == "Address");
            _model.SetAnnotationValue(_customerSet.EntityType(), new ClrTypeAnnotation(typeof(Customer)));
            _model.SetAnnotationValue(addressType, new ClrTypeAnnotation(typeof(Address)));
            _customers = new[] {
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 10,
                },
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 42,
                }
            };

            _customersType = _model.GetEdmTypeReference(typeof(Customer[])).AsCollection();
            _addressesType = _model.GetEdmTypeReference(typeof(Address[])).AsCollection();
            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model };
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataResourceSetSerializer(serializerProvider: null), "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            // Arrange & Act
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            // Arrange & Act
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Calls_WriteObjectInline()
        {
            // Arrange
            object graph = new object();
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer
                .Setup(s => s.WriteObjectInline(graph, It.Is<IEdmTypeReference>(e => _customersType.IsEquivalentTo(e)),
                    It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();

            // Act
            serializer.Object.WriteObject(graph, typeof(Customer[]), ODataTestUtil.GetMockODataMessageWriter(), _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObject_CanWriteTopLevelResourceSetContainsNullComplexElement()
        {
            // Arrange
            ODataSerializerProvider serializerProvider = GetServiceProvider().GetService<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider);
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter writer = new ODataMessageWriter(message, settings);
            IList<Address> addresses = new[]
            {
                new Address { City = "Redmond" },
                null,
                new Address { City = "Shanghai" }
            };

            ODataSerializerContext writeContext = new ODataSerializerContext { Model = _model };

            // Act
            serializer.WriteObject(addresses, typeof(IList<Address>), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata#Collection(Default.Address)\"," +
                "\"value\":[" +
                  "{\"Street\":null,\"City\":\"Redmond\",\"State\":null,\"CountryOrRegion\":null,\"ZipCode\":null}," +
                  "null," +
                  "{\"Street\":null,\"City\":\"Shanghai\",\"State\":null,\"CountryOrRegion\":null,\"ZipCode\":null}" +
                  "]}", result);
        }

        [Fact]
        public void WriteObject_CanWrite_TopLevelResourceSet_ContainsEmptyCollectionOfDynamicComplexElement()
        {
            // Arrange
            ODataSerializerProvider serializerProvider = GetServiceProvider().GetService<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider);
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter writer = new ODataMessageWriter(message, settings);
            IList<SimpleOpenAddress> addresses = new[]
            {
                new SimpleOpenAddress
                {
                    City = "Redmond",
                    Street = "Microsoft Rd",
                    Properties = new Dictionary<string, object>
                    {
                        { "StringProp", "abc" },
                        { "Locations", new SimpleOpenAddress[] {} } // empty collection of complex
                    }
                }
            };

            var builder = new ODataConventionModelBuilder();
            builder.ComplexType<SimpleOpenAddress>();
            IEdmModel model = builder.GetEdmModel();
            ODataSerializerContext writeContext = new ODataSerializerContext { Model = model };

            // Act
            serializer.WriteObject(addresses, typeof(IList<SimpleOpenAddress>), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            string result = JObject.Parse(new StreamReader(stream).ReadToEnd()).ToString();

            // Assert
            Assert.Equal(@"{
  ""@odata.context"": ""http://any/$metadata#Collection(Microsoft.AspNetCore.OData.Tests.Formatter.Models.SimpleOpenAddress)"",
  ""value"": [
    {
      ""Street"": ""Microsoft Rd"",
      ""City"": ""Redmond"",
      ""StringProp"": ""abc"",
      ""Locations@odata.type"": ""#Collection(Microsoft.AspNetCore.OData.Tests.Formatter.Models.SimpleOpenAddress)"",
      ""Locations"": []
    }
  ]
}", result);
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_Writer()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObjectInline(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_WriteContext()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObjectInline(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_CannotSerializerNull()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(graph: null, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "Cannot serialize a null 'ResourceSet'.");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_IfGraphIsNotEnumerable()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(graph: 42, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "ODataResourceSetSerializer cannot write an object of type 'System.Int32'.");
        }

        [Fact]
        public void WriteObjectInline_Throws_NullElementInCollection_IfResourceSetContainsNullElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "Collections cannot contain null elements.");
        }

        [Fact]
        public void WriteObjectInline_DoesnotThrow_NullElementInCollection_IfResourceSetContainsNullComplexElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            ODataSerializerContext writeContext = new ODataSerializerContext { NavigationSource = null, Model = _model };

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => serializer.WriteObjectInline(instance, _addressesType, new Mock<ODataWriter>().Object, writeContext));
        }

        [Fact]
        public void WriteObjectInline_Throws_TypeCannotBeSerialized_IfResourceSetContainsEntityThatCannotBeSerialized()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var request = RequestFactory.Create();
            serializerProvider.Setup(s => s.GetODataPayloadSerializer(typeof(int), request)).Returns<ODataSerializer>(null);
            IEnumerable instance = new object[] { 42 };
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "'Default.Customer' cannot be serialized using the OData output formatter.");
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateResourceSet()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(new ODataResourceSet()).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Throws_CannotSerializerNull_IfCreateResourceSetReturnsNull()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns<ODataResourceSet>(null);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.Object.WriteObjectInline(instance, _customersType, writer, _writeContext),
                "Cannot serialize a null 'ResourceSet'.");
        }

        [Fact]
        public void WriteObjectInline_Writes_CreateResourceSetOutput()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet();
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(s => s.WriteStart(resourceSet)).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_WritesEachEntityInstance()
        {
            // Arrange
            Mock<ODataEdmTypeSerializer> customerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            customerSerializer.Setup(s => s.WriteObjectInline(_customers[0], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();
            customerSerializer.Setup(s => s.WriteObjectInline(_customers[1], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();

            _serializer = new ODataResourceSetSerializer(provider);

            // Act
            _serializer.WriteObjectInline(_customers, _customersType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Can_WriteCollectionOfIEdmObjects()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(edmType));
            Mock<IEdmObject> edmObject = new Mock<IEdmObject>();
            edmObject.Setup(e => e.GetEdmType()).Returns(edmType);

            var mockWriter = new Mock<ODataWriter>();

            Mock<ODataEdmTypeSerializer> customSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            customSerializer.Setup(s => s.WriteObjectInline(edmObject.Object, edmType, mockWriter.Object, _writeContext)).Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(customSerializer.Object);

            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act
            serializer.WriteObjectInline(new[] { edmObject.Object }, collectionType, mockWriter.Object, _writeContext);

            // Assert
            customSerializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_CountQueryOption_OnWriteStart()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet { Count = 1000 };
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(It.Is<ODataResourceSet>(f => f.Count == 1000))).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_NextPageLink_OnWriteEnd()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet { NextPageLink = new Uri("http://nextlink.com/") };
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(It.Is<ODataResourceSet>(f => f.NextPageLink == null))).Verifiable();
            mockWriter
                .Setup(m => m.WriteEnd())
                .Callback(() =>
                {
                    Assert.Equal("http://nextlink.com/", resourceSet.NextPageLink.AbsoluteUri);
                })
                .Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void CreateResource_Sets_CountValueForPageResult()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(ExpectedCountValue, resourceSet.Count);
        }

        [Fact]
        public void CreateResource_Sets_NextPageLinkForPageResult()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(expectedNextLink, resourceSet.NextPageLink);
        }

        [Fact]
        public void CreateResourceSet_Sets_CountValueFromContext()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            const long ExpectedCountValue = 1000;
            var request = RequestFactory.Create();
            request.ODataFeature().TotalCount = ExpectedCountValue;
            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(ExpectedCountValue, resourceSet.Count);
        }

        [Fact]
        public void CreateResourceSet_Sets_NextPageLinkFromContext()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            Uri expectedNextLink = new Uri("http://nextlink.com");
            var request = RequestFactory.Create();
            request.ODataFeature().NextLink = expectedNextLink;
            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(expectedNextLink, resourceSet.NextPageLink);
        }

        [Fact]
        public void CreateODataFeed_Sets_DeltaLinkFromContext()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            Uri expectedDeltaLink = new Uri("http://deltalink.com");
            var request = RequestFactory.Create();
            request.ODataFeature().DeltaLink = expectedDeltaLink;
            var result = new object[0];

            // Act
            ODataResourceSet feed = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(expectedDeltaLink, feed.DeltaLink);
        }

        [Fact]
        public void CreateResource_Ignores_NextPageLink_ForInnerResourceSets()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            Uri nextLink = new Uri("http://somelink");
            var request = RequestFactory.Create();
            request.ODataFeature().NextLink = nextLink;
            var result = new object[0];
            IEdmNavigationProperty navProp = _customerSet.EntityType().NavigationProperties().First();
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            ResourceContext entity = new ResourceContext
            {
                SerializerContext =
                    new ODataSerializerContext { Request = request, NavigationSource = _customerSet, Model = _model }
            };
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpandClause, navProp);

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, nestedContext);

            // Assert
            Assert.Null(resourceSet.NextPageLink);
        }

        [Fact]
        public void CreateResourceSet_Ignores_CountValue_ForInnerResourceSets()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            var request = RequestFactory.Create();
            request.ODataFeature().TotalCount = 42;
            var result = new object[0];
            IEdmNavigationProperty navProp = _customerSet.EntityType().NavigationProperties().First();
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            ResourceContext entity = new ResourceContext
            {
                SerializerContext =
                    new ODataSerializerContext { Request = request, NavigationSource = _customerSet, Model = _model }
            };
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpandClause, navProp);

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, nestedContext);

            // Assert
            Assert.Null(resourceSet.Count);
        }

        [Fact]
        public void CreateResourceSet_SetsODataOperations()
        {
            // Arrange
            var request = RequestFactory.Create(method: "get", uri: "http://IgnoreMetadataPath", setupAction: null);

            IEdmModel model = GetEdmModelWithOperations(out IEdmEntityType customerType, out IEdmEntitySet customers);
            IEdmCollectionTypeReference customersType = new EdmCollectionTypeReference(new EdmCollectionType(customerType.AsReference()));

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

           // Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            ODataSerializerContext context = new ODataSerializerContext
            {
                NavigationSource = customers,
                Request = request,
                Model = model,
                MetadataLevel = ODataMetadataLevel.Full,
            };
            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, customersType, context);

            // Assert
            Assert.Single(resourceSet.Actions);
            Assert.Equal(3, resourceSet.Functions.Count());
        }

        private IEdmModel GetEdmModelWithOperations(out IEdmEntityType customerType, out IEdmEntitySet customers)
        {
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customerType = customer;
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);

            EdmAction upgradeAll = new EdmAction("NS", "UpgradeAll", returnType: null, isBound: true, entitySetPathExpression: null);
            upgradeAll.AddParameter("entityset",
                new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            model.AddElement(upgradeAll);

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference stringType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

            EdmFunction IsAnyUpgraded = new EdmFunction(
                "NS",
                "IsAnyUpgraded",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            EdmCollectionType edmCollectionType = new EdmCollectionType(new EdmEntityTypeReference(customer, false));
            IsAnyUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(edmCollectionType));
            model.AddElement(IsAnyUpgraded);

            EdmFunction isCustomerUpgradedWithParam = new EdmFunction(
                "NS",
                "IsUpgradedWithParam",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            isCustomerUpgradedWithParam.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            isCustomerUpgradedWithParam.AddParameter("city", EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false));
            model.AddElement(isCustomerUpgradedWithParam);

            EdmFunction isAllUpgraded = new EdmFunction("NS", "IsAllUpgraded", returnType, isBound: true,
                entitySetPathExpression: null, isComposable: false);
            isAllUpgraded.AddParameter("entityset",
                new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            isAllUpgraded.AddParameter("param", intType);
            model.AddElement(isAllUpgraded);

            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            getSalaray.AddParameter("minSalary", intType);
            getSalaray.AddOptionalParameter("maxSalary", intType);
            getSalaray.AddOptionalParameter("aveSalary", intType, "129");
            model.AddElement(getSalaray);

            EdmEntityContainer container = new EdmEntityContainer("NS", "ModelWithInheritance");
            model.AddElement(container);
            customers = container.AddEntitySet("Customers", customer);

            return model;
        }

        [Fact]
        public void SetODataFeatureTotalCountValueNull()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            var request = RequestFactory.Create();
            request.ODataFeature().TotalCount = null;

            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Null(resourceSet.Count);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Minimal)]
        [InlineData(ODataMetadataLevel.None)]
        public void CreateODataOperation_OmitsOperations_WhenNonFullMetadata(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            ResourceSetContext resourceSetContext = new ResourceSetContext();
            ODataSerializerContext serializerContext = new ODataSerializerContext
            {
                MetadataLevel = metadataLevel
            };
            // Act

            ODataOperation operation = serializer.CreateODataOperation(function, resourceSetContext, serializerContext);

            // Assert
            Assert.Null(operation);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateODataOperations_CreateOperations(bool followConventions)
        {
            // Arrange
            string expectedTarget = "aa://Target";
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<FeedCustomer>("Customers");
            var function = builder.EntityType<FeedCustomer>().Collection.Function("MyFunction").Returns<int>();
            IEdmModel model = builder.GetEdmModel();

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            IEdmFunction edmFunction = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "MyFunction");

            Func<ResourceSetContext, Uri> functionLinkFactory = a => new Uri(expectedTarget);
            var operationLinkBuilder = new OperationLinkBuilder(functionLinkFactory, followConventions);
            model.SetOperationLinkBuilder(edmFunction, operationLinkBuilder);

            var request = RequestFactory.Create(method: "get", uri: "http://any", setupAction: null);
            ResourceSetContext resourceSetContext = new ResourceSetContext
            {
                EntitySetBase = customers,
                Request = request,
            };

            ODataSerializerContext serializerContext = new ODataSerializerContext
            {
                NavigationSource = customers,
                Request = request,
                Model = model,
                MetadataLevel = ODataMetadataLevel.Full,
            };
            string expectedMetadataPrefix = "http://any/$metadata";

            // Act
            ODataOperation actualOperation = serializer.CreateODataOperation(edmFunction, resourceSetContext, serializerContext);

            // Assert
            Assert.NotNull(actualOperation);
            string expectedMetadata = expectedMetadataPrefix + "#Default.MyFunction";
            ODataOperation expectedFunction = new ODataFunction
            {
                Metadata = new Uri(expectedMetadata),
                Target = new Uri(expectedTarget),
                Title = "MyFunction"
            };

            AssertEqual(expectedFunction, actualOperation);
        }

        private static void AssertEqual(ODataOperation expected, ODataOperation actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            AssertEqual(expected.Metadata, actual.Metadata);
            AssertEqual(expected.Target, actual.Target);
            Assert.Equal(expected.Title, actual.Title);
        }

        private static void AssertEqual(Uri expected, Uri actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(expected.AbsoluteUri, actual.AbsoluteUri);
        }

        private static IServiceProvider GetServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<ODataSerializerProvider, DefaultODataSerializerProvider>();

            // Serializers.
            services.AddSingleton<ODataEnumSerializer>();
            services.AddSingleton<ODataPrimitiveSerializer>();
            services.AddSingleton<ODataResourceSetSerializer>();
            services.AddSingleton<ODataCollectionSerializer>();
            services.AddSingleton<ODataResourceSerializer>();

            return services.BuildServiceProvider();
        }

        public class FeedCustomer
        {
            public int Id { get; set; }
        }
    }
}
