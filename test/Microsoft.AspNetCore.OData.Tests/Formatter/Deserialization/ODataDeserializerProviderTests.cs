//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerProviderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;

public class ODataDeserializerProviderTests
{
    private static IODataDeserializerProvider _deserializerProvider = GetServiceProvider().GetRequiredService<IODataDeserializerProvider>();
    private static IEdmModel _edmModel = GetEdmModel();

    [Fact]
    public void ODataDeserializerProvider_Ctor_ThrowsArgumentNull_ServiceProvider()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new ODataDeserializerProvider(null), "serviceProvider");
    }

    [Fact]
    public void GetODataDeserializer_Uri()
    {
        // Arrange
        HttpRequest request = GetRequest(model: null);

        // Act
        IODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(typeof(Uri), request);

        // Assert
        Assert.NotNull(deserializer);
        var referenceLinkDeserializer = Assert.IsType<ODataEntityReferenceLinkDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.EntityReferenceLink, referenceLinkDeserializer.ODataPayloadKind);
    }

    [Theory]
    [InlineData(typeof(Int16))]
    [InlineData(typeof(int))]
    [InlineData(typeof(Decimal))]
    [InlineData(typeof(DateTimeOffset))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(Date))]
    [InlineData(typeof(TimeOfDay))]
    [InlineData(typeof(double))]
    [InlineData(typeof(byte[]))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(int?))]
    public void GetODataDeserializer_Primitive(Type type)
    {
        // Arrange
        HttpRequest request = GetRequest(EdmCoreModel.Instance);

        // Act
        IODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(type, request);

        // Assert
        Assert.NotNull(deserializer);
        ODataPrimitiveDeserializer rawValueDeserializer = Assert.IsType<ODataPrimitiveDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.Property, rawValueDeserializer.ODataPayloadKind);
    }

    [Fact]
    public void GetODataDeserializer_Resource_ForEntity()
    {
        // Arrange
        HttpRequest request = GetRequest(_edmModel);

        // Act
        IODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(typeof(Product), request);

        // Assert
        Assert.NotNull(deserializer);
        ODataResourceDeserializer entityDeserializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.Resource, deserializer.ODataPayloadKind);
        Assert.Equal(entityDeserializer.DeserializerProvider, _deserializerProvider);
    }

    [Fact]
    public void GetODataDeserializer_Resource_ForComplex()
    {
        // Arrange
        HttpRequest request = GetRequest(_edmModel);

        // Act
        IODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(typeof(Address), request);

        // Assert
        Assert.NotNull(deserializer);
        ODataResourceDeserializer complexDeserializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.Resource, deserializer.ODataPayloadKind);
        Assert.Equal(complexDeserializer.DeserializerProvider, _deserializerProvider);
    }

    [Theory]
    [InlineData(typeof(Product[]))]
    [InlineData(typeof(IEnumerable<Product>))]
    [InlineData(typeof(ICollection<Product>))]
    [InlineData(typeof(IList<Product>))]
    [InlineData(typeof(List<Product>))]
  //  [InlineData(typeof(PageResult<Product>))]
    public void GetODataDeserializer_ResourceSet_ForEntityCollection(Type collectionType)
    {
        // Arrange
        HttpRequest request = GetRequest(_edmModel);

        // Act
        IODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(collectionType, request);

        // Assert
        Assert.NotNull(deserializer);
        ODataResourceSetDeserializer resourceSetDeserializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.ResourceSet, deserializer.ODataPayloadKind);
        Assert.Equal(resourceSetDeserializer.DeserializerProvider, _deserializerProvider);
    }

    [Theory]
    [InlineData(typeof(Address[]))]
    [InlineData(typeof(IEnumerable<Address>))]
    [InlineData(typeof(ICollection<Address>))]
    [InlineData(typeof(IList<Address>))]
    [InlineData(typeof(List<Address>))]
  //  [InlineData(typeof(PageResult<Address>))]
    public void GetODataDeserializer_ResourceSet_ForComplexCollection(Type collectionType)
    {
        // Arrange
        HttpRequest request = GetRequest(_edmModel);

        // Act
        IODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(collectionType, request);

        // Assert
        Assert.NotNull(deserializer);
        ODataResourceSetDeserializer resourceSetDeserializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.ResourceSet, deserializer.ODataPayloadKind);
        Assert.Equal(resourceSetDeserializer.DeserializerProvider, _deserializerProvider);
    }

    [Theory]
    [InlineData(typeof(DeltaSet<>))]
    [InlineData(typeof(EdmChangedObjectCollection))]
    public void GetODataDeserializer_DeltaResourceSet_ForDeltaSet(Type deltaType)
    {
        // Arrange
        HttpRequest request = GetRequest(_edmModel);

        // Act
        IODataDeserializer deserializer = _deserializerProvider.GetODataDeserializer(deltaType, request);

        // Assert
        Assert.NotNull(deserializer);
        ODataDeltaResourceSetDeserializer setDeserializer = Assert.IsType<ODataDeltaResourceSetDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.Delta, setDeserializer.ODataPayloadKind);
        Assert.Equal(setDeserializer.DeserializerProvider, _deserializerProvider);
    }

    [Fact]
    public void GetODataDeserializer_ReturnsSameDeserializer_ForSameType()
    {
        // Arrange
        HttpRequest request = GetRequest(_edmModel);

        // Act
        IODataDeserializer firstCallDeserializer = _deserializerProvider.GetODataDeserializer(typeof(Product), request);
        IODataDeserializer secondCallDeserializer = _deserializerProvider.GetODataDeserializer(typeof(Product), request);

        // Assert
        Assert.Same(firstCallDeserializer, secondCallDeserializer);
    }

    [Theory]
    [InlineData(typeof(ODataActionParameters))]
    [InlineData(typeof(ODataUntypedActionParameters))]
    public void GetODataDeserializer_ActionPayload(Type resourceType)
    {
        // Arrange
        HttpRequest request = GetRequest(model: null);

        // Act
        ODataActionPayloadDeserializer basicActionPayload
            = _deserializerProvider.GetODataDeserializer(resourceType, request) as ODataActionPayloadDeserializer;

        // Assert
        Assert.NotNull(basicActionPayload);
    }

    [Fact]
    public void GetODataDeserializer_ThrowsArgumentNull_ForType()
    {
        // Arrange
        HttpRequest request = GetRequest(model: null);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => _deserializerProvider.GetODataDeserializer(type: null, request: request),
            "type");
    }

    [Fact]
    public void GetODataDeserializer_ThrowsArgumentNull_ForRequest()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => _deserializerProvider.GetODataDeserializer(typeof(int), request: null),
            "request");
    }

    [Fact]
    public void GetEdmTypeDeserializer_ThrowsArgument_EdmType()
    {
        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => _deserializerProvider.GetEdmTypeDeserializer(edmType: null),
            "edmType");
    }

    [Fact]
    public void GetEdmTypeDeserializer_Caches_CreateDeserializerOutput()
    {
        // Arrange
        IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;

        // Act
        var deserializer1 = _deserializerProvider.GetEdmTypeDeserializer(edmType);
        var deserializer2 = _deserializerProvider.GetEdmTypeDeserializer(edmType);

        // Assert
        Assert.Same(deserializer1, deserializer2);
    }

    [Fact]
    public void GetEdmTypeDeserializer_ReturnsCorrectDeserializer_ForEdmUntyped()
    {
        // Arrange
        IEdmTypeReference edmType = EdmUntypedStructuredTypeReference.NullableTypeReference;

        // Act
        var deserializer = _deserializerProvider.GetEdmTypeDeserializer(edmType);

        // Assert
        ODataResourceDeserializer resourceSerializer = Assert.IsType<ODataResourceDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.Resource, resourceSerializer.ODataPayloadKind);
    }

    [Fact]
    public void GetEdmTypeDeserializer_ReturnsCorrectDeserializer_ForCollectionOfEdmUntyped()
    {
        // Arrange
        IEdmTypeReference edmType = EdmUntypedHelpers.NullableUntypedCollectionReference;

        // Act
        var deserializer = _deserializerProvider.GetEdmTypeDeserializer(edmType);

        // Assert
        ODataResourceSetDeserializer setSerializer = Assert.IsType<ODataResourceSetDeserializer>(deserializer);
        Assert.Equal(ODataPayloadKind.ResourceSet, setSerializer.ODataPayloadKind);
    }

    private static IServiceProvider GetServiceProvider()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IODataDeserializerProvider, ODataDeserializerProvider>();

        // Deserializers.
        services.AddSingleton<ODataResourceDeserializer>();
        services.AddSingleton<ODataDeltaResourceSetDeserializer>();
        services.AddSingleton<ODataEnumDeserializer>();
        services.AddSingleton<ODataPrimitiveDeserializer>();
        services.AddSingleton<ODataResourceSetDeserializer>();
        services.AddSingleton<ODataCollectionDeserializer>();
        services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
        services.AddSingleton<ODataActionPayloadDeserializer>();

        services.AddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();

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
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<Product>("Products");
        return builder.GetEdmModel();
    }

    private class Product
    {
        public int Id { get; set; }
        public Address Location { get; set; }
    }

    private class Address
    {
        public string Street { get; set; }
    }
}
