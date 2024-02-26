//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetDeserializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class ODataResourceSetDeserializerTests
    {
        private readonly IEdmModel _model;
        private readonly IEdmCollectionTypeReference _customersType;
        private readonly IEdmEntityTypeReference _customerType;
        private readonly IODataSerializerProvider _serializerProvider;
        private readonly IODataDeserializerProvider _deserializerProvider;

        public ODataResourceSetDeserializerTests()
        {
            _model = GetEdmModel();
            _customerType = _model.GetEdmTypeReference(typeof(Customer)).AsEntity();
            _customersType = new EdmCollectionTypeReference(new EdmCollectionType(_customerType));
            _serializerProvider = ODataFormatterHelpers.GetSerializerProvider();
            _deserializerProvider = ODataFormatterHelpers.GetDeserializerProvider();
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataResourceSetDeserializer(deserializerProvider: null), "deserializerProvider");
        }

        [Fact]
        public async Task ReadAsync_ThrowsArgumentNull_MessageReader()
        {
            // Arrange
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => deserializer.ReadAsync(null, null, null), "messageReader");
        }

        [Fact]
        public async Task ReadAsync_ThrowsArgumentNull_ReadContext()
        {
            // Arrange
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ODataMessageReader reader = ODataFormatterHelpers.GetMockODataMessageReader();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => deserializer.ReadAsync(reader, null, null), "readContext");
        }

        [Fact]
        public async Task ReadAsync_Throws_ArgumentMustBeOfType()
        {
            // Arrange
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ODataMessageReader reader = ODataFormatterHelpers.GetMockODataMessageReader();
            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = EdmCoreModel.Instance
            };
            IEdmTypeReference edmType = new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetString(false)));

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<ArgumentException>(
                () => deserializer.ReadAsync(reader, typeof(IList<int>), context),
                "The argument must be of type 'Collection of complex, entity or untyped'. (Parameter 'edmType')");
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            // Arrange
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);

            // Act & Assert
            Assert.Null(deserializer.ReadInline(item: null, edmType: _customersType, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_ReadContext()
        {
            // Arrange
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            IEdmTypeReference typeReference = new Mock<IEdmTypeReference>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadInline(42, typeReference, null), "readContext");
        }

        [Fact]
        public void ReadInline_Throws_ArgumentTypeMustBeResourceSet()
        {
            // Arrange
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            IEdmTypeReference edmType = new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetString(false)));

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => deserializer.ReadInline(42, edmType, new ODataDeserializerContext()),
                "edmType",
                "'[Collection([Edm.String Nullable=False Unicode=True]) Nullable=False]' is not a resource set type. Only resource set are supported.");
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            // Arrange
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => deserializer.ReadInline(item: 42, edmType: _customersType, readContext: new ODataDeserializerContext()),
                "item",
                "The argument must be of type 'ODataResourceSetWrapper'.");
        }

        [Fact]
        public void ReadInline_Calls_ReadResourceSet()
        {
            // Arrange
            IODataDeserializerProvider deserializerProvider = _deserializerProvider;
            Mock<ODataResourceSetDeserializer> deserializer = new Mock<ODataResourceSetDeserializer>(deserializerProvider);
            ODataResourceSetWrapper feedWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            ODataDeserializerContext readContext = new ODataDeserializerContext();
            IEnumerable expectedResult = new object[0];

            deserializer.CallBase = true;
            deserializer.Setup(f => f.ReadResourceSet(feedWrapper, _customerType, readContext)).Returns(expectedResult).Verifiable();

            // Act
            var result = deserializer.Object.ReadInline(feedWrapper, _customersType, readContext);

            // Assert
            deserializer.Verify();
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void ReadResourceSet_ThrowsArgumentNull_ResourceSet()
        {
            // Arrange & Act & Assert
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadResourceSet(null, null, null).GetEnumerator().MoveNext(), "resourceSet");
        }

        [Fact]
        public void ReadResourceSet_Calls_ReadPrimitiveItem()
        {
            // Arrange
            Mock<IODataDeserializerProvider> deserializerProvider = new Mock<IODataDeserializerProvider>();
            Mock<IEdmStructuredTypeReference> structuredTypeRef = new Mock<IEdmStructuredTypeReference>();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            int value = 3;
            ODataPrimitiveWrapper wrapper = new ODataPrimitiveWrapper(new ODataPrimitiveValue(value));
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            resourceSetWrapper.Items.Add(wrapper);

            Mock<ODataResourceSetDeserializer> deserializer = new Mock<ODataResourceSetDeserializer>(deserializerProvider.Object);
            deserializer.CallBase = true;
            deserializer.Setup(d => d.ReadPrimitiveItem(wrapper, structuredTypeRef.Object, readContext)).Returns(value).Verifiable();

            // Act
            var result = deserializer.Object.ReadResourceSet(resourceSetWrapper, structuredTypeRef.Object, readContext);

            // Assert
            Assert.Equal(new int[] { value }, result.OfType<int>());
            deserializer.Verify();
        }

        [Fact]
        public void ReadResourceSet_Calls_ReadResourceItem_ForEachResource()
        {
            // Arrange
            Mock<IODataDeserializerProvider> deserializerProvider = new Mock<IODataDeserializerProvider>();
            Mock<IEdmStructuredTypeReference> structuredTypeRef = new Mock<IEdmStructuredTypeReference>();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            ODataResourceWrapper resourceWrapper1 = new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a1/") });
            ODataResourceWrapper resourceWrapper2 = new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a1/") });
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            resourceSetWrapper.Items.Add(resourceWrapper1);
            resourceSetWrapper.Items.Add(resourceWrapper2);

            Mock<ODataResourceSetDeserializer> deserializer = new Mock<ODataResourceSetDeserializer>(deserializerProvider.Object);
            deserializer.CallBase = true;
            deserializer.Setup(d => d.ReadResourceItem(resourceWrapper1, structuredTypeRef.Object, readContext)).Returns("a1").Verifiable();
            deserializer.Setup(d => d.ReadResourceItem(resourceWrapper2, structuredTypeRef.Object, readContext)).Returns("a2").Verifiable();

            // Act
            var result = deserializer.Object.ReadResourceSet(resourceSetWrapper, structuredTypeRef.Object, readContext);

            // Assert
            Assert.Equal(new[] { "a1", "a2" }, result.OfType<String>());
            deserializer.Verify();
        }

        [Fact]
        public void ReadResourceSet_Calls_ReadInlineForEachResource()
        {
            // Arrange
            Mock<IODataDeserializerProvider> deserializerProvider = new Mock<IODataDeserializerProvider>();
            Mock<ODataEdmTypeDeserializer> entityDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Resource);
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(deserializerProvider.Object);
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a1/") }));
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a2/") }));
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _model
            };

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType, false)).Returns(entityDeserializer.Object);
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[0], _customerType, It.IsAny<ODataDeserializerContext>())).Returns("a1").Verifiable();
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[1], _customerType, It.IsAny<ODataDeserializerContext>())).Returns("a2").Verifiable();

            // Act
            var result = deserializer.ReadResourceSet(resourceSetWrapper, _customerType, readContext);

            // Assert
            Assert.Equal(new[] { "a1", "a2" }, result.OfType<String>());
            entityDeserializer.Verify();
        }

        [Fact]
        public void ReadResourceSet_Calls_ReadResourceSetItem_ForEachResourceSet()
        {
            // Arrange
            Mock<IODataDeserializerProvider> deserializerProvider = new Mock<IODataDeserializerProvider>();
            Mock<IEdmStructuredTypeReference> structuredTypeRef = new Mock<IEdmStructuredTypeReference>();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            ODataResourceSetWrapper setWrapper1 = new ODataResourceSetWrapper(new ODataResourceSet());
            ODataResourceSetWrapper setWrapper2 = new ODataResourceSetWrapper(new ODataResourceSet());
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            resourceSetWrapper.Items.Add(setWrapper1);
            resourceSetWrapper.Items.Add(setWrapper2);

            Mock<ODataResourceSetDeserializer> deserializer = new Mock<ODataResourceSetDeserializer>(deserializerProvider.Object);
            deserializer.CallBase = true;
            deserializer.Setup(d => d.ReadResourceSetItem(setWrapper1, structuredTypeRef.Object, readContext)).Returns("a1").Verifiable();
            deserializer.Setup(d => d.ReadResourceSetItem(setWrapper2, structuredTypeRef.Object, readContext)).Returns("a2").Verifiable();

            // Act
            var result = deserializer.Object.ReadResourceSet(resourceSetWrapper, structuredTypeRef.Object, readContext);

            // Assert
            Assert.Equal(new[] { "a1", "a2" }, result.OfType<String>());
            deserializer.Verify();
        }

        [Fact]
        public void ReadPrimitiveItem_ThrowsArgumentNull_ResourceWrapper()
        {
            // Arrange & Act & Assert
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadPrimitiveItem(null, null, null), "primitiveWrapper");
        }

        [Fact]
        public void ReadResourceItem_ThrowsArgumentNull_ResourceWrapper()
        {
            // Arrange & Act & Assert
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadResourceItem(null, null, null), "resourceWrapper");
        }

        [Fact]
        public void ReadResourceSetItem_ThrowsArgumentNull_ResourceSetWrapper()
        {
            // Arrange & Act & Assert
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadResourceSetItem(null, null, null), "resourceSetWrapper");
        }

        [Fact]
        public void ReadResourceItem_Throws_TypeCannotBeDeserialized()
        {
            Mock<IODataDeserializerProvider> deserializerProvider = new Mock<IODataDeserializerProvider>();
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(deserializerProvider.Object);
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(new ODataResource());
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType, false)).Returns<ODataEdmTypeDeserializer>(null);

            ExceptionAssert.Throws<SerializationException>(
                () => deserializer.ReadResourceItem(resourceWrapper, _customerType, readContext),
                "'Microsoft.AspNetCore.OData.Tests.Models.Customer' cannot be deserialized using the OData input formatter.");
        }

        [Fact]
        public async Task ReadAsync_ReturnTypedCollection_WithTypeMode()
        {
            // Arrange
            HttpContent content = new StringContent("{ 'value': [ {'City' : 'Redmond' }, {'City' : 'Issaquah' } ] }");
            HeaderDictionary headerDict = new HeaderDictionary
            {
                { "Content-Type", "application/json" }
            };

            IODataRequestMessage request = ODataMessageWrapperHelper.Create(await content.ReadAsStreamAsync(), headerDict);
            ODataMessageReader reader = new ODataMessageReader(request, new ODataMessageReaderSettings(), _model);
            IEdmComplexType addressType = _model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "Address");
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _model,
                ResourceType = typeof(IList<Address>),
                ResourceEdmType = new EdmCollectionTypeReference(new EdmCollectionType(addressType.ToEdmTypeReference(true))),
            };

            // Act
            var result = await deserializer.ReadAsync(reader, typeof(IList<Address>), readContext);

            // Assert
            IEnumerable<Address> addresses = (result as IEnumerable).Cast<Address>();

            Assert.Equal(2, addresses.Count());
            Assert.Collection(addresses,
                e => Assert.Equal("Redmond", e.City),
                e => Assert.Equal("Issaquah", e.City));
        }

        [Fact]
        public async Task ReadAsync_ReturnsEdmUntypedCollection_WithTypeMode()
        {
            // Arrange
            HttpContent content = new StringContent("{ 'value': [ 4, {'@odata.type':'Microsoft.AspNetCore.OData.Tests.Models.Address', 'City' : 'Redmond' } ] }");
            HeaderDictionary headerDict = new HeaderDictionary
            {
                { "Content-Type", "application/json" }
            };

            IODataRequestMessage request = ODataMessageWrapperHelper.Create(await content.ReadAsStreamAsync(), headerDict);
            ODataMessageReader reader = new ODataMessageReader(request, new ODataMessageReaderSettings(), _model);
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _model,
                ResourceType = typeof(EdmUntypedCollection),
                ResourceEdmType = EdmUntypedHelpers.NullableUntypedCollectionReference
            };

            // Act
            var result = await deserializer.ReadAsync(reader, typeof(EdmUntypedCollection), readContext)  as EdmUntypedCollection;

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(4, result[0]);
            Address address = Assert.IsType<Address>(result[1]);

            Assert.Null(address.Street);
            Assert.Equal("Redmond", address.City);
        }

        [Fact]
        public async Task ReadAsync_ReturnsEdmComplexObjectCollection_TypelessMode()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmTypeReference addressType = new EdmComplexTypeReference(model.Address, true);
            IEdmCollectionTypeReference addressCollectionType =
                new EdmCollectionTypeReference(new EdmCollectionType(addressType));

            HttpContent content = new StringContent("{ 'value': [ {'@odata.type':'NS.Address', 'City' : 'Redmond' } ] }");
            HeaderDictionary headerDict = new HeaderDictionary
            {
                { "Content-Type", "application/json" }
            };

            IODataRequestMessage request = ODataMessageWrapperHelper.Create(await content.ReadAsStreamAsync(), headerDict);
            ODataMessageReader reader = new ODataMessageReader(request, new ODataMessageReaderSettings(), model.Model);
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = model.Model,
                ResourceType = typeof(IEdmObject),
                ResourceEdmType = addressCollectionType
            };

            // Act
            IEnumerable result = await deserializer.ReadAsync(reader, typeof(IEdmObject), readContext) as IEnumerable;

            // Assert
            var addresses = result.Cast<EdmComplexObject>();
            Assert.NotNull(addresses);

            EdmComplexObject address = Assert.Single(addresses);
            Assert.Equal(new[] { "City" }, address.GetChangedPropertyNames());

            object city;
            Assert.True(address.TryGetPropertyValue("City", out city));
            Assert.Equal("Redmond", city);
        }

        [Fact(Skip = "See: https://github.com/OData/odata.net/issues/2662")]
        public async Task ReadAsync_ReturnsEdmUntypedObjectCollection_UntypedMode()
        {
            // Arrange
            HttpContent content = new StringContent("{ 'value': [42, null,  { 'Any/name' : 'Redmond' } ] }");
            HeaderDictionary headerDict = new HeaderDictionary
            {
                { "Content-Type", "application/json" }
            };

            IODataRequestMessage request = ODataMessageWrapperHelper.Create(await content.ReadAsStreamAsync(), headerDict);
            ODataMessageReader reader = new ODataMessageReader(request, new ODataMessageReaderSettings(), _model);
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _model,
                ResourceType = typeof(IEdmObject),
                ResourceEdmType = EdmUntypedHelpers.NullableUntypedCollectionReference
            };

            // Act
            EdmUntypedCollection result = await deserializer.ReadAsync(reader, typeof(IEdmObject), readContext) as EdmUntypedCollection;

            // Assert
            Assert.NotNull(result);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task ReadAsync_Roundtrip_ComplexCollection()
        {
            // Arrange
            Address[] addresses = new[]
                {
                    new Address { City ="Redmond", StreetAddress ="A", State ="123"},
                    new Address { City ="Seattle", StreetAddress ="S", State ="321"}
                };

            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(_deserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, _model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), _model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = _model };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = _model };

            // Act
            await serializer.WriteObjectAsync(addresses, addresses.GetType(), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readAddresses = await deserializer.ReadAsync(messageReader, typeof(Address[]), readContext) as IEnumerable;

            // Assert
            Assert.Equal(addresses, readAddresses.Cast<Address>(), new AddressComparer());
        }

        [Fact]
        public void ReadResourceSet_Returns_UntypedCollection()
        {
            // Arrange
            ODataResourceSetWrapper setWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            setWrapper.Items.Add(new ODataPrimitiveWrapper(new ODataPrimitiveValue(1115)));
            setWrapper.Items.Add(null);
            setWrapper.Items.Add(new ODataResourceWrapper(new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "City", Value = "Redmond" }
                }
            }));
            setWrapper.Items.Add(new ODataResourceWrapper(new ODataResource
            {
                TypeName = "Microsoft.AspNetCore.OData.Tests.Models.Address",
                Properties = new[]
                {
                    new ODataProperty { Name = "City", Value = "Redmond" }
                }
            }));
            setWrapper.Items.Add(new ODataResourceSetWrapper(new ODataResourceSet()));

            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(_deserializerProvider);

            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = _model };

            // Act
            IEdmTypeReference edmType = EdmUntypedHelpers.NullableUntypedCollectionReference;
            EdmUntypedCollection untypedCollection = deserializer.ReadInline(setWrapper, edmType, readContext) as EdmUntypedCollection;

            // Assert
            Assert.Equal(5, untypedCollection.Count);
            Assert.Collection(untypedCollection,
                e => Assert.Equal(1115, e),
                e => Assert.Null(e),
                e =>
                {
                    // Without type name, goes to 'EdmUntypedObject'
                    KeyValuePair<string, object> property = Assert.Single(Assert.IsType<EdmUntypedObject>(e));
                    Assert.Equal("City", property.Key);
                    Assert.Equal("Redmond", property.Value);
                },
                e =>
                {
                    // Without type name, goes to corresponding CLR type
                    Address address = Assert.IsType<Address>(e);
                    Assert.Equal("Redmond", address.City);
                },
                e =>
                {
                    Assert.Empty(Assert.IsType<EdmUntypedCollection>(e));
                }
                );
        }

        private class AddressComparer : IEqualityComparer<Address>
        {
            public bool Equals(Address x, Address y)
            {
                return x.City == y.City && x.State == y.State && x.StreetAddress == y.StreetAddress && x.ZipCode == y.ZipCode;
            }

            public int GetHashCode(Address obj)
            {
                throw new NotImplementedException();
            }
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("customers");
            builder.ComplexType<Address>();
            return builder.GetEdmModel();
        }
    }
}
