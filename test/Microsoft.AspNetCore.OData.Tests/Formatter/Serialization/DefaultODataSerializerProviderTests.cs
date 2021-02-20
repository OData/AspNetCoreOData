// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.AspNetCore.OData.Tests.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class DefaultODataSerializerProviderTests
    {
        private static ODataSerializerProvider _serializerProvider = GetServiceProvider().GetRequiredService<ODataSerializerProvider>();

        private static IEdmModel _edmModel = GetEdmModel();

        private static IEdmModel _edmDollarCountModel = GetDollarCountEdmModel();

        public static TheoryDataSet<Type, EdmPrimitiveTypeKind> EdmPrimitiveMappingData
        {
            get
            {
                return new TheoryDataSet<Type, EdmPrimitiveTypeKind>
                {
                    { typeof(byte[]), EdmPrimitiveTypeKind.Binary },
                    { typeof(bool), EdmPrimitiveTypeKind.Boolean },
                    { typeof(byte), EdmPrimitiveTypeKind.Byte },
                    { typeof(DateTime), EdmPrimitiveTypeKind.DateTimeOffset },
                    { typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset },
                    { typeof(Date), EdmPrimitiveTypeKind.Date },
                    { typeof(TimeOfDay), EdmPrimitiveTypeKind.TimeOfDay },
                    { typeof(decimal), EdmPrimitiveTypeKind.Decimal },
                    { typeof(double), EdmPrimitiveTypeKind.Double },
                    { typeof(Guid), EdmPrimitiveTypeKind.Guid },
                    { typeof(short), EdmPrimitiveTypeKind.Int16 },
                    { typeof(int), EdmPrimitiveTypeKind.Int32 },
                    { typeof(long), EdmPrimitiveTypeKind.Int64 },
                    { typeof(sbyte), EdmPrimitiveTypeKind.SByte },
                    { typeof(float), EdmPrimitiveTypeKind.Single },
                    { typeof(Stream), EdmPrimitiveTypeKind.Stream },
                    { typeof(string), EdmPrimitiveTypeKind.String },
                    { typeof(TimeSpan), EdmPrimitiveTypeKind.Duration },
                };
            }
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Type()
        {
            // Arrange
            HttpRequest request = GetRequest(model: null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializerProvider.GetODataPayloadSerializer(type: null, request: request),
               "type");
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Request()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializerProvider.GetODataPayloadSerializer(typeof(int), request: null),
               "request");
        }

        [Theory]
        [MemberData(nameof(EdmPrimitiveMappingData))]
        public void GetODataSerializer_Primitive(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            // Arrange
            HttpRequest request = GetRequest(EdmCoreModel.Instance);

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type, request);

            // Assert
            Assert.NotEqual(EdmPrimitiveTypeKind.None, edmPrimitiveTypeKind);
            Assert.NotNull(serializer);
            ODataPrimitiveSerializer primitiveSerializer = Assert.IsType<ODataPrimitiveSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Property, primitiveSerializer.ODataPayloadKind);
        }

        [Theory]
        [MemberData(nameof(EdmPrimitiveMappingData))]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForValueRequests(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            // Arrange
            ODataPath odataPath = new ODataPath(new ValueSegment(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32)));
            HttpRequest request = GetRequest(EdmCoreModel.Instance);
            request.ODataFeature().Path = odataPath;

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type, request);

            // Assert
            Assert.NotEqual(EdmPrimitiveTypeKind.None, edmPrimitiveTypeKind);
            Assert.NotNull(serializer);
            Assert.Equal(ODataPayloadKind.Value, serializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataSerializer_Enum()
        {
            // Arrange
            HttpRequest request = GetRequest(_edmModel);

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(typeof(TestEnum), request);

            // Assert
            Assert.NotNull(serializer);
            var enumSerializer = Assert.IsType<ODataEnumSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Property, enumSerializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForEnumValueRequests()
        {
            // Arrange
            ODataPath odataPath = new ODataPath(new ValueSegment(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32)));
            HttpRequest request = GetRequest(_edmModel);
            request.ODataFeature().Path = odataPath;

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(typeof(TestEnum), request);

            // Assert
            Assert.NotNull(serializer);
            var rawValueSerializer = Assert.IsType<ODataRawValueSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Value, rawValueSerializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData("DollarCountEntities/$count", typeof(DollarCountEntity))]
        [InlineData("DollarCountEntities(5)/StringCollectionProp/$count", typeof(string))]
        [InlineData("DollarCountEntities(5)/EnumCollectionProp/$count", typeof(Color))]
        [InlineData("DollarCountEntities(5)/TimeSpanCollectionProp/$count", typeof(TimeSpan))]
        [InlineData("DollarCountEntities(5)/ComplexCollectionProp/$count", typeof(DollarCountComplex))]
        [InlineData("DollarCountEntities(5)/EntityCollectionProp/$count", typeof(DollarCountEntity))]
        [InlineData("UnboundFunctionReturnsPrimitveCollection()/$count", typeof(int))]
        [InlineData("UnboundFunctionReturnsEnumCollection()/$count", typeof(Color))]
        [InlineData("UnboundFunctionReturnsDateTimeOffsetCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("UnboundFunctionReturnsDateCollection()/$count", typeof(Date))]
        [InlineData("UnboundFunctionReturnsComplexCollection()/$count", typeof(DollarCountComplex))]
        [InlineData("UnboundFunctionReturnsEntityCollection()/$count", typeof(DollarCountEntity))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsPrimitveCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsEnumCollection()/$count", typeof(Color))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsDateTimeOffsetCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()/$count", typeof(DollarCountComplex))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/$count", typeof(DollarCountEntity))]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForDollarCountRequests(string uri, Type elementType)
        {
            // Arrange
            IEdmModel model = _edmDollarCountModel;

            Type type = typeof(ICollection<>).MakeGenericType(elementType);
            ODataUriParser parser = new ODataUriParser(model, new Uri(uri, UriKind.Relative));
            var path = parser.ParsePath();

            var request = RequestFactory.Create(model);
            request.ODataFeature().Path = path;

            // Act
            var serializer = _serializerProvider.GetODataPayloadSerializer(type, request);

            // Assert
            Assert.NotNull(serializer);
            var rawValueSerializer = Assert.IsType<ODataRawValueSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Value, rawValueSerializer.ODataPayloadKind);
        }

        public static IEdmModel GetDollarCountEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var entityCollection = builder.EntitySet<DollarCountEntity>("DollarCountEntities").EntityType.Collection;

            // Add unbound functions that return collection.
            FunctionConfiguration function = builder.Function("UnboundFunctionReturnsPrimitveCollection");
            function.IsComposable = true;
            function.ReturnsCollection<int>();

            function = builder.Function("UnboundFunctionReturnsEnumCollection");
            function.IsComposable = true;
            function.ReturnsCollection<Color>();

            function = builder.Function("UnboundFunctionReturnsDateTimeOffsetCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = builder.Function("UnboundFunctionReturnsDateCollection");
            function.IsComposable = true;
            function.ReturnsCollection<Date>();

            function = builder.Function("UnboundFunctionReturnsComplexCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DollarCountComplex>();

            function = builder.Function("UnboundFunctionReturnsEntityCollection");
            function.IsComposable = true;
            function.ReturnsCollectionFromEntitySet<DollarCountEntity>("DollarCountEntities");

            // Add bound functions that return collection.
            function = entityCollection.Function("BoundFunctionReturnsPrimitveCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = entityCollection.Function("BoundFunctionReturnsEnumCollection");
            function.IsComposable = true;
            function.ReturnsCollection<Color>();

            function = entityCollection.Function("BoundFunctionReturnsDateTimeOffsetCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DateTimeOffset>();

            function = entityCollection.Function("BoundFunctionReturnsComplexCollection");
            function.IsComposable = true;
            function.ReturnsCollection<DollarCountComplex>();

            function = entityCollection.Function("BoundFunctionReturnsEntityCollection");
            function.IsComposable = true;
            function.ReturnsCollectionFromEntitySet<DollarCountEntity>("DollarCountEntities");

            return builder.GetEdmModel();
        }

        private class DollarCountEntity
        {
            public int Id { get; set; }
            public string[] StringCollectionProp { get; set; }
            public Color[] EnumCollectionProp { get; set; }
            public TimeSpan[] TimeSpanCollectionProp { get; set; }
            public DollarCountComplex[] ComplexCollectionProp { get; set; }
            public DollarCountEntity[] EntityCollectionProp { get; set; }
            public int[] DollarCountNotAllowedCollectionProp { get; set; }
        }

        private class DollarCountComplex
        {
            public string StringProp { get; set; }
        }

        [Fact]
        public void GetODataSerializer_Resource_ForEntity()
        {
            // Arrange
            HttpRequest request = GetRequest(_edmModel);

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(typeof(Product), request);

            // Assert
            Assert.NotNull(serializer);
            var entitySerializer = Assert.IsType<ODataResourceSerializer>(serializer);
            Assert.Equal(entitySerializer.SerializerProvider, _serializerProvider);
            Assert.Equal(ODataPayloadKind.Resource, entitySerializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataSerializer_Resource_ForComplex()
        {
            // Arrange
            HttpRequest request = GetRequest(_edmModel);

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(typeof(Address), request);

            // Assert
            Assert.NotNull(serializer);
            var complexSerializer = Assert.IsType<ODataResourceSerializer>(serializer);
            Assert.Equal(complexSerializer.SerializerProvider, _serializerProvider);
            Assert.Equal(ODataPayloadKind.Resource, complexSerializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData(typeof(Product[]))]
        [InlineData(typeof(IEnumerable<Product>))]
        [InlineData(typeof(ICollection<Product>))]
        [InlineData(typeof(IList<Product>))]
        [InlineData(typeof(List<Product>))]
        [InlineData(typeof(PageResult<Product>))]
        public void GetODataSerializer_ResourceSet_ForEntityCollection(Type collectionType)
        {
            // Arrange
            HttpRequest request = GetRequest(_edmModel);

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(collectionType, request);

            // Assert
            Assert.NotNull(serializer);
            var resourceSetSerializer = Assert.IsType<ODataResourceSetSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.ResourceSet, resourceSetSerializer.ODataPayloadKind);
            Assert.Same(resourceSetSerializer.SerializerProvider, _serializerProvider);
        }

        [Theory]
        [InlineData(typeof(Address[]))]
        [InlineData(typeof(IEnumerable<Address>))]
        [InlineData(typeof(ICollection<Address>))]
        [InlineData(typeof(IList<Address>))]
        [InlineData(typeof(List<Address>))]
        [InlineData(typeof(PageResult<Address>))]
        public void GetODataSerializer_ResourceSet_ForComplexCollection(Type collectionType)
        {
            // Arrange
            HttpRequest request = GetRequest(_edmModel);

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(collectionType, request);

            // Assert
            Assert.NotNull(serializer);
            var resourceSetSerializer = Assert.IsType<ODataResourceSetSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.ResourceSet, resourceSetSerializer.ODataPayloadKind);
            Assert.Same(resourceSetSerializer.SerializerProvider, _serializerProvider);
        }

        [Theory]
        [InlineData(typeof(ODataError), typeof(ODataErrorSerializer))]
        [InlineData(typeof(Uri), typeof(ODataEntityReferenceLinkSerializer))]
        [InlineData(typeof(ODataEntityReferenceLink), typeof(ODataEntityReferenceLinkSerializer))]
        [InlineData(typeof(Uri[]), typeof(ODataEntityReferenceLinksSerializer))]
        [InlineData(typeof(List<Uri>), typeof(ODataEntityReferenceLinksSerializer))]
        [InlineData(typeof(ODataEntityReferenceLinks), typeof(ODataEntityReferenceLinksSerializer))]
        public void GetODataSerializer_Returns_ExpectedSerializerType(Type payloadType, Type expectedSerializerType)
        {
            // Arrange
            HttpRequest request = GetRequest(EdmCoreModel.Instance);

            // Act
            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(payloadType, request);

            // Assert
            Assert.NotNull(serializer);
            Assert.IsType(expectedSerializerType, serializer);
        }

        [Fact]
        public void GetODataSerializer_ReturnsSameSerializer_ForSameType()
        {
            // Arrange
            HttpRequest request = GetRequest(_edmModel);

            // Act
            ODataSerializer firstCallSerializer = _serializerProvider.GetODataPayloadSerializer(typeof(Product), request);
            ODataSerializer secondCallSerializer = _serializerProvider.GetODataPayloadSerializer(typeof(Product), request);

            // Assert
            Assert.Same(firstCallSerializer, secondCallSerializer);
        }

        [Fact]
        public void GetEdmTypeSerializer_ThrowsArgumentNull_EdmType()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => _serializerProvider.GetEdmTypeSerializer(edmType: null),
                "edmType");
        }

        [Fact]
        public void GetEdmTypeSerializer_Caches_CreateEdmTypeSerializerOutput()
        {
            // Arrange
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;

            // Act
            ODataSerializer serializer1 = _serializerProvider.GetEdmTypeSerializer(edmType);
            ODataSerializer serializer2 = _serializerProvider.GetEdmTypeSerializer(edmType);

            // Assert
            Assert.Same(serializer2, serializer1);
        }

        private static IServiceProvider GetServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<ODataSerializerProvider, DefaultODataSerializerProvider>();

            // Serializers.
            services.AddSingleton<ODataEnumSerializer>();
            services.AddSingleton<ODataPrimitiveSerializer>();
            services.AddSingleton<ODataDeltaResourceSetSerializer>();
            services.AddSingleton<ODataResourceSetSerializer>();
            services.AddSingleton<ODataCollectionSerializer>();
            services.AddSingleton<ODataResourceSerializer>();
            services.AddSingleton<ODataServiceDocumentSerializer>();
            services.AddSingleton<ODataEntityReferenceLinkSerializer>();
            services.AddSingleton<ODataEntityReferenceLinksSerializer>();
            services.AddSingleton<ODataErrorSerializer>();
            services.AddSingleton<ODataMetadataSerializer>();
            services.AddSingleton<ODataRawValueSerializer>();

            return services.BuildServiceProvider();
        }

        private HttpRequest GetRequest(IEdmModel model)
        {
            HttpContext context = new DefaultHttpContext();
            context.ODataFeature().Model = model;
            return context.Request;
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            EdmEntityType product = new EdmEntityType("NS", "Product");
            model.AddElement(product);
            model.SetAnnotationValue(product, new ClrTypeAnnotation(typeof(Product)));

            EdmComplexType address = new EdmComplexType("NS", "Address");
            model.AddElement(address);
            model.SetAnnotationValue(address, new ClrTypeAnnotation(typeof(Address)));

            EdmEnumType enumType = new EdmEnumType("NS", "TestEnum");
            enumType.AddMember(new EdmEnumMember(enumType, "FirstValue", new EdmEnumMemberValue(0)));
            enumType.AddMember(new EdmEnumMember(enumType, "FirstValue", new EdmEnumMemberValue(1)));
            model.AddElement(enumType);
            model.SetAnnotationValue(enumType, new ClrTypeAnnotation(typeof(TestEnum)));

            return model;
        }

        private class Product
        { }

        private class Address
        { }

        private enum TestEnum
        {
            FirstValue,
            SecondValue
        }
    }
}
