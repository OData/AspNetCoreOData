//-----------------------------------------------------------------------------
// <copyright file="ODataModelBinderConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter;

public class ODataModelBinderConverterTests
{
    [Fact]
    public void Convert_ForNullValues()
    {
        var result = ODataModelBinderConverter.Convert(null, null, null, "IsActive", null, null);
        Assert.Null(result);

        result = ODataModelBinderConverter.Convert(new ODataNullValue(), null, null, "IsActive", null, null);
        Assert.Null(result);
    }

    /// <summary>
    /// The set of potential values to test against
    /// <see cref="ODataModelBinderConverter.Convert(object, IEdmTypeReference, Type, string, OData.Formatter.Deserialization.ODataDeserializerContext, IServiceProvider)"/>.
    /// </summary>
    public static TheoryDataSet<object, EdmPrimitiveTypeKind, Type, object> ODataModelBinderConverter_Works_TestData
    {
        get
        {
            return new TheoryDataSet<object, EdmPrimitiveTypeKind, Type, object>
            {
                { "true", EdmPrimitiveTypeKind.Boolean, typeof(bool), true },
                { true, EdmPrimitiveTypeKind.Boolean, typeof(bool), true },
                { 5, EdmPrimitiveTypeKind.Int32, typeof(int),  5 },
                { new Guid("C2AEFDF2-B533-4971-8B6A-A539373BFC32"), EdmPrimitiveTypeKind.Guid, typeof(Guid), new Guid("C2AEFDF2-B533-4971-8B6A-A539373BFC32") }
            };
        }
    }

    /// <summary>
    /// Tests the <see cref="ODataModelBinderConverter.Convert(object, IEdmTypeReference, Type, string, OData.Formatter.Deserialization.ODataDeserializerContext, IServiceProvider)"/>
    /// method to ensure proper operation against primitive types.
    /// </summary>
    /// <param name="odataValue">The value as it would come across the wire.</param>
    /// <param name="edmTypeKind">The <see cref="EdmPrimitiveTypeKind"/> to test for.</param>
    /// <param name="clrType">The CLR type to convert to.</param>
    /// <param name="expectedResult">The expected value in the correct type to check against.</param>
    /// <remarks>Contributed by Robert McLaws (@robertmclaws).</remarks>
    [Theory]
    [MemberData(nameof(ODataModelBinderConverter_Works_TestData))]
    public void Convert_CheckPrimitives(object odataValue, EdmPrimitiveTypeKind edmTypeKind, Type clrType, object expectedResult)
    {
        var edmTypeReference = new EdmPrimitiveTypeReference(EdmCoreModel.Instance.GetPrimitiveType(edmTypeKind), false);
        var value = new ConstantNode(odataValue, odataValue.ToString(), edmTypeReference);
        var result = ODataModelBinderConverter.Convert(value, edmTypeReference, clrType, "IsActive", null, null);
        Assert.NotNull(result);
        Assert.IsType(clrType, result);
        Assert.Equal(expectedResult, result);
    }

    /// <summary>
    /// The set of potential type definition values to test against
    /// <see cref="ODataModelBinderConverter.Convert(object, IEdmTypeReference, Type, string, OData.Formatter.Deserialization.ODataDeserializerContext, IServiceProvider)"/>.
    /// </summary>
    public static TheoryDataSet<object, EdmPrimitiveTypeKind, Type, object> ODataModelBinderConverter_Works_TypeDefinitionTestData
    {
        get
        {
            return new TheoryDataSet<object, EdmPrimitiveTypeKind, Type, object>
            {
                { "100500", EdmPrimitiveTypeKind.String, typeof(BigInteger), new BigInteger(100500) },
                { 300, EdmPrimitiveTypeKind.Int32, typeof(BigInteger), new BigInteger(300) },
                { new BigInteger(222), EdmPrimitiveTypeKind.String, typeof(BigInteger),  new BigInteger(222) }
            };
        }
    }

    /// <summary>
    /// Tests the <see cref="ODataModelBinderConverter.Convert(object, IEdmTypeReference, Type, string, OData.Formatter.Deserialization.ODataDeserializerContext, IServiceProvider)"/>
    /// method to ensure proper operation against type definition, backed by primitive type.
    /// </summary>
    /// <param name="odataValue">The value as it would come across the wire.</param>
    /// <param name="edmTypeKind">The <see cref="EdmPrimitiveTypeKind"/> to test for.</param>
    /// <param name="clrType">The CLR type to convert to.</param>
    /// <param name="expectedResult">The expected value in the correct type to check against.</param>
    /// <remarks>Contributed by Jevgenijs Fjodorovics (@jfshark).</remarks>
    [Theory]
    [MemberData(nameof(ODataModelBinderConverter_Works_TypeDefinitionTestData))]
    public void Convert_CheckTypeDefinitionPrimitives(object odataValue, EdmPrimitiveTypeKind edmTypeKind, Type clrType, object expectedResult)
    {
        var edmTypeDefinition = new EdmTypeDefinition(clrType.Namespace, clrType.Name, edmTypeKind);
        var edmTypeDefReference = new EdmTypeDefinitionReference(edmTypeDefinition, false);

        var result = ODataModelBinderConverter.Convert(odataValue, edmTypeDefReference, clrType, "IsActive", null, null);
        Assert.NotNull(result);
        Assert.IsType(clrType, result);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void ConvertTo_Converts_InputValue()
    {
        // Arrange & Act & Assert
        Assert.Null(ODataModelBinderConverter.ConvertTo(null, null, null));

        // Arrange & Act & Assert
        Assert.Null(ODataModelBinderConverter.ConvertTo("null", typeof(int?), null));

        // Arrange & Act & Assert
        Assert.Equal(Color.Red, ODataModelBinderConverter.ConvertTo("NS.Color'Red'", typeof(Color), null));

        ExceptionAssert.Throws<InvalidOperationException>(
            () => ODataModelBinderConverter.ConvertTo("NS.Color'Unknown'", typeof(Color), null),
            "The binding value 'Unknown' cannot be bound to the enum type 'Color'.");

        Assert.Equal(Color.Red, ODataModelBinderConverter.ConvertTo("NS.Color'Red'", typeof(Color), null));

        // Arrange & Act & Assert
        Assert.Equal(new Date(2021, 6, 17), ODataModelBinderConverter.ConvertTo("2021-06-17", typeof(Date?), null));

        // Arrange & Act & Assert
        Assert.Equal(42, ODataModelBinderConverter.ConvertTo("42", typeof(int), null));
    }

    [Fact]
    public void ConvertResourceOrResourceSetCanConvert_SingleResource()
    {
        // Arrange
        string odataValue = "{\"Id\": 9, \"Name\": \"Sam\"}";
        IEdmModel model = GetEdmModel();
        IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
        IEdmTypeReference edmTypeReference = new EdmEntityTypeReference(customerType, false);
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/", opt => opt.AddRouteComponents("odata", model));
        request.ODataFeature().RoutePrefix = "odata";
        ODataDeserializerContext context = new ODataDeserializerContext
        {
            Model = model,
            Request = request,
            ResourceType = typeof(Customer),
            ResourceEdmType = edmTypeReference,
        };

        // Act
        object value = ODataModelBinderConverter.ConvertResourceOrResourceSet(odataValue, edmTypeReference, context);

        // Assert
        Assert.NotNull(value);

        Customer customer = Assert.IsType<Customer>(value);
        Assert.Equal(9, customer.Id);
        Assert.Equal("Sam", customer.Name);
    }

    [Fact]
    public void ConvertResourceOrResourceSetCanConvert_ResourceSet()
    {
        // Arrange
        string odataValue = "[{\"Id\": 9, \"Name\": \"Sam\"}, {\"Id\": 18, \"Name\": \"Peter\"}]";
        IEdmModel model = GetEdmModel();
        IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
        IEdmTypeReference edmTypeReference = new EdmEntityTypeReference(customerType, false);
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/", opt => opt.AddRouteComponents("odata", model));
        request.ODataFeature().RoutePrefix = "odata";
        ODataDeserializerContext context = new ODataDeserializerContext
        {
            Model = model,
            Request = request,
            ResourceType = typeof(Customer),
            ResourceEdmType = edmTypeReference,
        };

        IEdmTypeReference setType = new EdmCollectionTypeReference(new EdmCollectionType(edmTypeReference));

        // Act
        object value = ODataModelBinderConverter.ConvertResourceOrResourceSet(odataValue, setType, context);

        // Assert
        Assert.NotNull(value);

        IEnumerable<Customer> customers = Assert.IsAssignableFrom<IEnumerable<Customer>>(value);
        Assert.Collection(customers,
            e =>
            {
                Assert.Equal(9, e.Id);
                Assert.Equal("Sam", e.Name);
            },
            e =>
            {
                Assert.Equal(18, e.Id);
                Assert.Equal("Peter", e.Name);
            });
    }

    [Fact]
    public void ConvertResourceOrResourceSetCanConvert_ResourceId()
    {
        // Arrange
        string odataValue = "{\"@odata.id\":\"http://localhost/odata/Customers(81)\"}";
        IEdmModel model = GetEdmModel();
        IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
        IEdmTypeReference edmTypeReference = new EdmEntityTypeReference(customerType, false);
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/", opt => opt.AddRouteComponents("odata", model));
        request.ODataFeature().RoutePrefix = "odata";
        ODataDeserializerContext context = new ODataDeserializerContext
        {
            Model = model,
            Request = request,
            ResourceType = typeof(Customer),
            ResourceEdmType = edmTypeReference,
        };

        // Act
        object value = ODataModelBinderConverter.ConvertResourceOrResourceSet(odataValue, edmTypeReference, context);

        // Assert
        Assert.NotNull(value);

        Customer customer = Assert.IsType<Customer>(value);
        Assert.Equal(81, customer.Id);
    }

    [Fact]
    public void ConvertResourceOrResourceSetCanConvert_ResourceWithMulitipleKeys()
    {
        // Arrange
        string odataValue = "{\"@odata.id\":\"http://localhost/odata/CustomerWithKeys(First='abc',Last='efg')\"}";
        IEdmModel model = GetEdmModel();
        IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "CustomerWithKeys");
        IEdmTypeReference edmTypeReference = new EdmEntityTypeReference(customerType, false);
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/", opt => opt.AddRouteComponents("odata", model));
        request.ODataFeature().RoutePrefix = "odata";
        ODataDeserializerContext context = new ODataDeserializerContext
        {
            Model = model,
            Request = request,
            ResourceType = typeof(CustomerWithKeys),
            ResourceEdmType = edmTypeReference,
        };

        // Act
        object value = ODataModelBinderConverter.ConvertResourceOrResourceSet(odataValue, edmTypeReference, context);

        // Assert
        Assert.NotNull(value);

        CustomerWithKeys customer = Assert.IsType<CustomerWithKeys>(value);
        Assert.Equal("abc", customer.First);
        Assert.Equal("efg", customer.Last);
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<Customer>("Customers");
        builder.EntitySet<CustomerWithKeys>("CustomerWithKeys");
        builder.EntityType<CustomerWithKeys>().HasKey(c => new { c.First, c.Last });
        return builder.GetEdmModel();
    }

    private class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private class CustomerWithKeys
    {
        public string First { get; set; }
        public string Last { get; set; }
    }

    private enum Color
    {
        Red
    }
}
