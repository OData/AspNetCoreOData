//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
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
        public async Task WriteObjectAsync_ThrowsArgumentNull_MessageWriter()
        {
            // Arrange & Act
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(graph: null, type: null, messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public async Task WriteObjectAsync_ThrowsArgumentNull_WriteContext()
        {
            // Arrange & Act
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectAsync(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public async Task WriteObjectAsync_Calls_WriteObjectInline()
        {
            // Arrange
            object graph = new object();//Enumerable.Empty<object>();
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer
                .Setup(s => s.WriteObjectInlineAsync(graph, It.Is<IEdmTypeReference>(e => _customersType.IsEquivalentTo(e)),
                        It.IsAny<ODataWriter>(), _writeContext))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await serializer.Object.WriteObjectAsync(graph, typeof(Customer[]), ODataTestUtil.GetMockODataMessageWriter(), _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public async Task WriteObjectAsync_CanWriteTopLevelResourceSetContainsNullComplexElement()
        {
            // Arrange
            IODataSerializerProvider serializerProvider = GetServiceProvider().GetService<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider);
            MemoryStream stream = new MemoryStream();
            IODataResponseMessageAsync message = new ODataMessageWrapper(stream);

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
            await serializer.WriteObjectAsync(addresses, typeof(IList<Address>), writer, writeContext);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WriteObjectAsync_CanWrite_TopLevelResourceSet_ContainsEmptyCollectionOfDynamicComplexElement(bool containsAnnotation)
        {
            // Arrange
            IODataSerializerProvider serializerProvider = GetServiceProvider().GetService<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider);
            MemoryStream stream = new MemoryStream();
            IODataResponseMessageAsync message = new ODataMessageWrapper(stream);
            message.PreferenceAppliedHeader().AnnotationFilter = "*";

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), },
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
            if (containsAnnotation)
            {
                writeContext.InstanceAnnotations = new Dictionary<string, object>
                {
                    { "NS.TestAnnotation", "Xiao" }
                };
            }

            // Act
            await serializer.WriteObjectAsync(addresses, typeof(IList<SimpleOpenAddress>), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            JObject result = JObject.Parse(await new StreamReader(stream).ReadToEndAsync());//.ToString();

            // Assert
            if (containsAnnotation)
            {
                Assert.Equal(JObject.Parse(@"{
                    ""@odata.context"": ""http://any/$metadata#Collection(Microsoft.AspNetCore.OData.Tests.Formatter.Models.SimpleOpenAddress)"",
                    ""@NS.TestAnnotation"": ""Xiao"",
                    ""value"": [
                      {
                          ""Street"": ""Microsoft Rd"",
                          ""City"": ""Redmond"",
                          ""StringProp"": ""abc"",
                          ""Locations@odata.type"": ""#Collection(Microsoft.AspNetCore.OData.Tests.Formatter.Models.SimpleOpenAddress)"",
                          ""Locations"": []
                      }
                    ]
                }"), result);
            }
            else
            {

                Assert.Equal(JObject.Parse(@"{
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
                }"), result);
            }
        }

        [Fact]
        public async Task WriteObjectInlineAsync_ThrowsArgumentNull_Writer()
        {
            // Arrange
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectInlineAsync(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public async Task WriteObjectInlineAsync_ThrowsArgumentNull_WriteContext()
        {
            // Arrange
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => serializer.WriteObjectInlineAsync(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public async Task WriteObjectInlineAsync_ThrowsSerializationException_CannotSerializerNull()
        {
            // Arrange
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteObjectInlineAsync(graph: null, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "Cannot serialize a null 'ResourceSet'.");
        }

        [Fact]
        public async Task WriteObjectInlineAsync_ThrowsSerializationException_IfGraphIsNotEnumerable()
        {
            // Arrange
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteObjectInlineAsync(graph: 42, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "ODataResourceSetSerializer cannot write an object of type 'System.Int32'.");
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Throws_NullElementInCollection_IfResourceSetContainsNullElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteObjectInlineAsync(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "Collections cannot contain null elements.");
        }

        [Fact]
        public void WriteObjectInlineAsync_DoesnotThrow_NullElementInCollection_IfResourceSetContainsNullComplexElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            ODataSerializerContext writeContext = new ODataSerializerContext { NavigationSource = null, Model = _model };

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => serializer.WriteObjectInlineAsync(instance, _addressesType, new Mock<ODataWriter>().Object, writeContext).Wait());
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Throws_TypeCannotBeSerialized_IfResourceSetContainsEntityThatCannotBeSerialized()
        {
            // Arrange
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            var request = RequestFactory.Create();
            serializerProvider.Setup(s => s.GetODataPayloadSerializer(typeof(int), request)).Returns<ODataSerializer>(null);
            IEnumerable instance = new object[] { 42 };
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.WriteObjectInlineAsync(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "'Default.Customer' cannot be serialized using the OData output formatter.");
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Calls_CreateResourceSet()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(new ODataResourceSet()).Verifiable();

            // Act
            await serializer.Object.WriteObjectInlineAsync(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Throws_CannotSerializerNull_IfCreateResourceSetReturnsNull()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns<ODataResourceSet>(null);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => serializer.Object.WriteObjectInlineAsync(instance, _customersType, writer, _writeContext),
                "Cannot serialize a null 'ResourceSet'.");
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Writes_CreateResourceSetOutput()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet();
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(s => s.WriteStartAsync(resourceSet)).Verifiable();

            // Act
            await serializer.Object.WriteObjectInlineAsync(instance, _customersType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public async Task WriteObjectInlineAsync_WritesEachEntityInstance()
        {
            // Arrange
            Mock<ODataEdmTypeSerializer> customerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            IODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            customerSerializer.Setup(s => s.WriteObjectInlineAsync(_customers[0], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();
            customerSerializer.Setup(s => s.WriteObjectInlineAsync(_customers[1], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();

            _serializer = new ODataResourceSetSerializer(provider);

            // Act
            await _serializer.WriteObjectInlineAsync(_customers, _customersType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public async Task WriteObjectInlineAsync_WritesEachResourceSetItemInstance_Untyped()
        {
            // Arrange
            IList<object> lists = new List<object>()
            {
                new List<object>{1},
                new List<object>{2}
            };
            ODataSerializerContext writeContext = new ODataSerializerContext();

            IEdmTypeReference edmType = EdmUntypedHelpers.NullableUntypedCollectionReference;

            Mock<ODataEdmTypeSerializer> customerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.ResourceSet);
            IODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            customerSerializer.Setup(s => s.WriteObjectInlineAsync(lists[0], edmType, mockWriter.Object, writeContext)).Verifiable();
            customerSerializer.Setup(s => s.WriteObjectInlineAsync(lists[1], edmType, mockWriter.Object, writeContext)).Verifiable();

            _serializer = new ODataResourceSetSerializer(provider);

            // Act
            await _serializer.WriteObjectInlineAsync(lists, edmType, mockWriter.Object, writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public async Task WriteObjectInlineAsync_WritesEachResourceItemInstance_Untyped()
        {
            // Arrange
            IList<object> lists = new List<object>()
            {
                new object(),
                new object()
            };
            ODataSerializerContext writeContext = new ODataSerializerContext();

            IEdmTypeReference edmType = EdmUntypedHelpers.NullableUntypedCollectionReference;
            IEdmTypeReference elementType = edmType.AsCollection().ElementType();

            Mock<ODataEdmTypeSerializer> customerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            IODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            customerSerializer.Setup(s => s.WriteObjectInlineAsync(lists[0], elementType, mockWriter.Object, writeContext)).Verifiable();
            customerSerializer.Setup(s => s.WriteObjectInlineAsync(lists[1], elementType, mockWriter.Object, writeContext)).Verifiable();

            _serializer = new ODataResourceSetSerializer(provider);

            // Act
            await _serializer.WriteObjectInlineAsync(lists, edmType, mockWriter.Object, writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Can_WriteCollectionOfIEdmObjects()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(edmType));
            Mock<IEdmObject> edmObject = new Mock<IEdmObject>();
            edmObject.Setup(e => e.GetEdmType()).Returns(edmType);

            var mockWriter = new Mock<ODataWriter>();

            Mock<ODataEdmTypeSerializer> customSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            customSerializer.Setup(s => s.WriteObjectInlineAsync(edmObject.Object, edmType, mockWriter.Object, _writeContext)).Verifiable();

            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(customSerializer.Object);

            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act
            await serializer.WriteObjectInlineAsync(new[] { edmObject.Object }, collectionType, mockWriter.Object, _writeContext);

            // Assert
            customSerializer.Verify();
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Sets_CountQueryOption_OnWriteStartAsync()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet { Count = 1000 };
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStartAsync(It.Is<ODataResourceSet>(f => f.Count == 1000))).Verifiable();

            // Act
            await serializer.Object.WriteObjectInlineAsync(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public async Task WriteObjectInlineAsync_Sets_NextPageLink_OnWriteEndAsync()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet { NextPageLink = new Uri("http://nextlink.com/") };
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            Mock<ODataResourceSerializer> resourceSerializer = new Mock<ODataResourceSerializer>(serializerProvider.Object);
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(resourceSerializer.Object);
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(serializerProvider.Object);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStartAsync(It.Is<ODataResourceSet>(f => f.NextPageLink == null))).Verifiable();
            mockWriter
                .Setup(m => m.WriteEndAsync())
                .Callback(() =>
                {
                    Assert.Equal("http://nextlink.com/", resourceSet.NextPageLink.AbsoluteUri);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await serializer.Object.WriteObjectInlineAsync(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void CreateResource_Sets_CountValueForPageResult()
        {
            // Arrange
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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
            IEdmModel model = GetEdmModelWithOperations(out IEdmEntityType customerType, out IEdmEntitySet customers);
            IEdmCollectionTypeReference customersType = new EdmCollectionTypeReference(new EdmCollectionType(customerType.AsReference()));

            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            var request = RequestFactory.Create(method: "Get", uri: "http://IgnoreMetadataPath", opt => opt.AddRouteComponents(model));

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
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);
            var request = RequestFactory.Create();
            request.ODataFeature().TotalCount = null;

            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Null(resourceSet.Count);
        }

        [Fact]
        public void CreateODataOperation_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => serializer.CreateODataOperation(null, null, null), "operation");

            // Act & Assert
            IEdmOperation operation = new Mock<IEdmOperation>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => serializer.CreateODataOperation(operation, null, null), "resourceSetContext");

            // Act & Assert
            ResourceSetContext context = new ResourceSetContext();
            ExceptionAssert.ThrowsArgumentNull(() => serializer.CreateODataOperation(operation, context, null), "writeContext");
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Minimal)]
        [InlineData(ODataMetadataLevel.None)]
        public void CreateODataOperation_OmitsOperations_WhenNonFullMetadata(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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
            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
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

            var request = RequestFactory.Create(method: "Get", uri: "http://any", opt => opt.AddRouteComponents(model));
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

            services.AddSingleton<IODataSerializerProvider, ODataSerializerProvider>();

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
