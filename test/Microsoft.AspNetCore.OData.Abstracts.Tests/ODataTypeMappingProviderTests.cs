// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Abstracts.Annotations;
using Microsoft.AspNetCore.OData.Abstracts.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using Xunit;

namespace Microsoft.AspNetCore.OData.Abstracts.Tests
{
    public class ODataTypeMappingProviderTests
    {
        private static IEdmModel EdmModel = GetEdmModel();
        private ODataTypeMappingProvider _provider = new ODataTypeMappingProvider(new TestAssemblyResolver());

        #region GetEdmType
        [Theory]
        [InlineData(typeof(string), "Edm.String", true)]
        [InlineData(typeof(bool), "Edm.Boolean", false)]
        [InlineData(typeof(bool?), "Edm.Boolean", true)]
        [InlineData(typeof(byte), "Edm.Byte", false)]
        [InlineData(typeof(byte?), "Edm.Byte", true)]
        [InlineData(typeof(decimal), "Edm.Decimal", false)]
        [InlineData(typeof(decimal?), "Edm.Decimal", true)]
        [InlineData(typeof(double), "Edm.Double", false)]
        [InlineData(typeof(double?), "Edm.Double", true)]
        [InlineData(typeof(Guid), "Edm.Guid", false)]
        [InlineData(typeof(Guid?), "Edm.Guid", true)]
        [InlineData(typeof(short), "Edm.Int16", false)]
        [InlineData(typeof(short?), "Edm.Int16", true)]
        [InlineData(typeof(int), "Edm.Int32", false)]
        [InlineData(typeof(int?), "Edm.Int32", true)]
        [InlineData(typeof(long), "Edm.Int64", false)]
        [InlineData(typeof(long?), "Edm.Int64", true)]
        [InlineData(typeof(sbyte), "Edm.SByte", false)]
        [InlineData(typeof(sbyte?), "Edm.SByte", true)]
        [InlineData(typeof(float), "Edm.Single", false)]
        [InlineData(typeof(float?), "Edm.Single", true)]
        [InlineData(typeof(DateTimeOffset), "Edm.DateTimeOffset", false)]
        [InlineData(typeof(DateTimeOffset?), "Edm.DateTimeOffset", true)]
        [InlineData(typeof(TimeSpan), "Edm.Duration", false)]
        [InlineData(typeof(TimeSpan?), "Edm.Duration", true)]
        [InlineData(typeof(Date), "Edm.Date", false)]
        [InlineData(typeof(Date?), "Edm.Date", true)]
        [InlineData(typeof(TimeOfDay), "Edm.TimeOfDay", false)]
        [InlineData(typeof(TimeOfDay?), "Edm.TimeOfDay", true)]
        [InlineData(typeof(byte[]), "Edm.Binary", true)]
        [InlineData(typeof(Stream), "Edm.Stream", true)]
        public void GetEdmTypeWorksAsExpectedForStandardPrimitive(Type clrType, string name, bool nullable)
        {
            // Arrange & Act
            IEdmTypeReference edmType = _provider.GetEdmType(EdmModel, clrType);

            // Assert
            Assert.NotNull(edmType);
            Assert.True(edmType.IsPrimitive());
            Assert.Equal(name, edmType.FullName());
            Assert.Equal(nullable, edmType.IsNullable);
        }

        [Theory]
        [InlineData(typeof(XElement), "Edm.String", true)]
        [InlineData(typeof(ushort), "Edm.Int32", false)]
        [InlineData(typeof(ushort?), "Edm.Int32", true)]
        [InlineData(typeof(uint), "Edm.Int64", false)]
        [InlineData(typeof(uint?), "Edm.Int64", true)]
        [InlineData(typeof(ulong), "Edm.Int64", false)]
        [InlineData(typeof(ulong?), "Edm.Int64", true)]
        [InlineData(typeof(char[]), "Edm.String", true)]
        [InlineData(typeof(char), "Edm.String", false)]
        [InlineData(typeof(char?), "Edm.String", true)]
        [InlineData(typeof(DateTime), "Edm.DateTimeOffset", false)]
        [InlineData(typeof(DateTime?), "Edm.DateTimeOffset", true)]
        public void GetEdmTypeWorksAsExpectedForNonStandardPrimitive(Type clrType, string name, bool nullable)
        {
            // Arrange & Act
            IEdmTypeReference edmType = _provider.GetEdmType(EdmModel, clrType);

            // Assert
            Assert.NotNull(edmType);
            Assert.True(edmType.IsPrimitive());
            Assert.Equal(name, edmType.FullName());
            Assert.Equal(nullable, edmType.IsNullable);
        }

        [Theory]
        [InlineData(typeof(Geography), "Edm.Geography")]
        [InlineData(typeof(GeographyPoint), "Edm.GeographyPoint")]
        [InlineData(typeof(GeographyLineString), "Edm.GeographyLineString")]
        [InlineData(typeof(GeographyPolygon), "Edm.GeographyPolygon")]
        [InlineData(typeof(GeographyCollection), "Edm.GeographyCollection")]
        [InlineData(typeof(GeographyMultiLineString), "Edm.GeographyMultiLineString")]
        [InlineData(typeof(GeographyMultiPoint), "Edm.GeographyMultiPoint")]
        [InlineData(typeof(GeographyMultiPolygon), "Edm.GeographyMultiPolygon")]
        [InlineData(typeof(Geometry), "Edm.Geometry")]
        [InlineData(typeof(GeometryPoint), "Edm.GeometryPoint")]
        [InlineData(typeof(GeometryLineString), "Edm.GeometryLineString")]
        [InlineData(typeof(GeometryPolygon), "Edm.GeometryPolygon")]
        [InlineData(typeof(GeometryCollection), "Edm.GeometryCollection")]
        [InlineData(typeof(GeometryMultiLineString), "Edm.GeometryMultiLineString")]
        [InlineData(typeof(GeometryMultiPoint), "Edm.GeometryMultiPoint")]
        [InlineData(typeof(GeometryMultiPolygon), "Edm.GeometryMultiPolygon")]
        public void GetEdmTypeWorksAsExpectedForSpatialPrimitive(Type clrType, string name)
        {
            // Arrange & Act
            IEdmTypeReference edmType = _provider.GetEdmType(EdmModel, clrType);

            // Assert
            Assert.NotNull(edmType);
            Assert.True(edmType.IsPrimitive());
            Assert.Equal(name, edmType.FullName());
            Assert.True(edmType.IsNullable);
        }

        [Theory]
        [InlineData(typeof(Address), "NS.Address")]
        [InlineData(typeof(CnAddress), "NS.CnAddress")]
        [InlineData(typeof(Customer), "Microsoft.AspNetCore.OData.Abstracts.Tests.Customer")]
        public void GetEdmTypeWorksAsExpectedForSchemaType(Type clrType, string typeName)
        {
            // Arrange
            IEdmType expectedEdmType = EdmModel.FindType(typeName);
            Assert.NotNull(expectedEdmType); // Guard

            // Arrange & Act
            IEdmTypeReference edmType = _provider.GetEdmType(EdmModel, clrType);

            // Assert
            Assert.NotNull(edmType);
            Assert.Same(expectedEdmType, edmType.Definition);
            Assert.True(edmType.IsNullable);
        }
        #endregion

        #region GetClrType
        [Theory]
        [InlineData(EdmPrimitiveTypeKind.String, typeof(string))]
        [InlineData(EdmPrimitiveTypeKind.Boolean, typeof(bool))]
        [InlineData(EdmPrimitiveTypeKind.Byte, typeof(byte))]
        [InlineData(EdmPrimitiveTypeKind.Decimal, typeof(decimal))]
        [InlineData(EdmPrimitiveTypeKind.Double, typeof(double))]
        [InlineData(EdmPrimitiveTypeKind.Guid, typeof(Guid))]
        [InlineData(EdmPrimitiveTypeKind.Int16, typeof(short))]
        [InlineData(EdmPrimitiveTypeKind.Int32, typeof(int))]
        [InlineData(EdmPrimitiveTypeKind.Int64, typeof(long))]
        [InlineData(EdmPrimitiveTypeKind.SByte, typeof(sbyte))]
        [InlineData(EdmPrimitiveTypeKind.Single, typeof(float))]
        [InlineData(EdmPrimitiveTypeKind.Binary, typeof(byte[]))]
        [InlineData(EdmPrimitiveTypeKind.Stream, typeof(Stream))]
        [InlineData(EdmPrimitiveTypeKind.DateTimeOffset, typeof(DateTimeOffset))]
        [InlineData(EdmPrimitiveTypeKind.Duration, typeof(TimeSpan))]
        [InlineData(EdmPrimitiveTypeKind.Date, typeof(Date))]
        [InlineData(EdmPrimitiveTypeKind.TimeOfDay, typeof(TimeOfDay))]
        public void GetClrTypeWorksAsExpectedForStandardPrimitive(EdmPrimitiveTypeKind kind, Type expected)
        {
            // #1 Arrange & Act & Assert for nullable equasls to false
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetPrimitive(kind, false);
            Type clrType = _provider.GetClrType(EdmModel, primitiveType);
            Assert.Equal(expected, clrType);

            // #2 Arrange & Act & Assert for nullable equals to true
            primitiveType = EdmCoreModel.Instance.GetPrimitive(kind, true);
            clrType = _provider.GetClrType(EdmModel, primitiveType);
            if (expected.IsValueType)
            {
                Type generic = typeof(Nullable<>);
                expected = generic.MakeGenericType(expected);
                Assert.Same(expected, clrType);
            }
            else
            {
                Assert.Same(expected, clrType);
            }
        }

        [Theory]
        [InlineData(EdmPrimitiveTypeKind.Geography, typeof(Geography))]
        [InlineData(EdmPrimitiveTypeKind.GeographyPoint, typeof(GeographyPoint))]
        [InlineData(EdmPrimitiveTypeKind.GeographyLineString, typeof(GeographyLineString))]
        [InlineData(EdmPrimitiveTypeKind.GeographyPolygon, typeof(GeographyPolygon))]
        [InlineData(EdmPrimitiveTypeKind.GeographyCollection, typeof(GeographyCollection))]
        [InlineData(EdmPrimitiveTypeKind.GeographyMultiLineString,typeof(GeographyMultiLineString))]
        [InlineData(EdmPrimitiveTypeKind.GeographyMultiPoint, typeof(GeographyMultiPoint))]
        [InlineData(EdmPrimitiveTypeKind.GeographyMultiPolygon, typeof(GeographyMultiPolygon))]
        [InlineData(EdmPrimitiveTypeKind.Geometry, typeof(Geometry))]
        [InlineData(EdmPrimitiveTypeKind.GeometryPoint, typeof(GeometryPoint))]
        [InlineData(EdmPrimitiveTypeKind.GeometryLineString, typeof(GeometryLineString))]
        [InlineData(EdmPrimitiveTypeKind.GeometryPolygon, typeof(GeometryPolygon))]
        [InlineData(EdmPrimitiveTypeKind.GeometryCollection, typeof(GeometryCollection))]
        [InlineData(EdmPrimitiveTypeKind.GeometryMultiLineString,typeof(GeometryMultiLineString))]
        [InlineData(EdmPrimitiveTypeKind.GeometryMultiPoint, typeof(GeometryMultiPoint))]
        [InlineData(EdmPrimitiveTypeKind.GeometryMultiPolygon, typeof(GeometryMultiPolygon))]
        public void GetClrTypeWorksAsExpectedForSpatialPrimitive(EdmPrimitiveTypeKind kind, Type type)
        {
            // Arrange
            IEdmPrimitiveTypeReference primitiveType1 = EdmCoreModel.Instance.GetPrimitive(kind, true);
            IEdmPrimitiveTypeReference primitiveType2 = EdmCoreModel.Instance.GetPrimitive(kind, false);

            // Act
            Type clrType1 = _provider.GetClrType(EdmModel, primitiveType1);
            Type clrType2 = _provider.GetClrType(EdmModel, primitiveType2);

            // Assert
            Assert.Same(clrType1, clrType2);
            Assert.Same(type, clrType1);
        }

        [Theory]
        [InlineData("NS.Address", typeof(Address))] // use ClrTypeAnnotation
        [InlineData("NS.CnAddress", typeof(CnAddress))]
        [InlineData("Microsoft.AspNetCore.OData.Abstracts.Tests.Customer", typeof(Customer))] // use the full name match
        public void GetClrTypeWorksAsExpectedForSchemaStrucutralType(string typeName, Type expected)
        {
            // Arrange
            IEdmType edmType = EdmModel.FindType(typeName);
            Assert.NotNull(edmType); // Guard

            // #1. Act & Assert
            IEdmTypeReference edmTypeReference = edmType.ToEdmTypeReference(true);
            Type clrType = _provider.GetClrType(EdmModel, edmTypeReference);
            Assert.Same(expected, clrType);

            // #2. Act & Assert
            edmTypeReference = edmType.ToEdmTypeReference(false);
            clrType = _provider.GetClrType(EdmModel, edmTypeReference);
            Assert.Same(expected, clrType);
        }

        [Fact]
        public void GetClrTypeWorksAsExpectedForSchemaEnumType()
        {
            // Arrange
            IEdmType edmType = EdmModel.FindType("NS.Color");
            Assert.NotNull(edmType); // Guard

            // #1. Act & Assert
            IEdmTypeReference edmTypeReference = edmType.ToEdmTypeReference(true);
            Type clrType = _provider.GetClrType(EdmModel, edmTypeReference);
            Assert.Same(typeof(Color?), clrType);

            // #2. Act & Assert
            edmTypeReference = edmType.ToEdmTypeReference(false);
            clrType = _provider.GetClrType(EdmModel, edmTypeReference);
            Assert.Same(typeof(Color), clrType);
        }

        [Fact]
        public void GetEdmTypeWorksAsExpectedForSchemaEnumType()
        {
            var primitiveType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, true);
            var clrType = _provider.GetClrPrimitiveType(primitiveType);
            Assert.Equal("", clrType.FullName);
        }

        [Fact]
        public void GetEdmTypeWorksAsExpectedForSchemaEnumType2()
        {
            var edmType = _provider.GetEdmPrimitiveType(typeof(ushort?));

            // Assert.Equal("", clrType.FullName());
            var clrType = _provider.GetClrPrimitiveType(edmType);
            // Assert.Equal("", clrType.FullName);

            edmType = _provider.GetEdmPrimitiveType(typeof(string));

            // Assert.Equal("", clrType.FullName());
            clrType = _provider.GetClrPrimitiveType(edmType);
            // Assert.Equal("", clrType.FullName);
        }

        #endregion

        [Theory]
        [InlineData(typeof(int), typeof(int))]
        [InlineData(typeof(int?), typeof(int?))]
        [InlineData(null, null)]
        [InlineData(typeof(XElement), typeof(string))]
        [InlineData(typeof(ushort), typeof(int))]
        [InlineData(typeof(ushort?), typeof(int?))]
        [InlineData(typeof(uint), typeof(long))]
        [InlineData(typeof(uint?), typeof(long?))]
        [InlineData(typeof(ulong), typeof(long))]
        [InlineData(typeof(ulong?), typeof(long?))]
        [InlineData(typeof(char[]), typeof(string))]
        [InlineData(typeof(char), typeof(string))]
        [InlineData(typeof(char?), typeof(string))]
        [InlineData(typeof(DateTime), typeof(DateTimeOffset))]
        [InlineData(typeof(DateTime?), typeof(DateTimeOffset?))]
        [InlineData(typeof(object), typeof(object))]
        [InlineData(typeof(Address), typeof(Address))]
        public void MapToWorksAsExpectedForNonstandardType(Type inType, Type expectType)
        {
            // Arrange & Act
            Type actual = _provider.MapTo(inType);

            // Assert
            Assert.Same(expectType, actual);
        }

        private static IEdmModel GetEdmModel()
        {
            var model = new EdmModel();
            var address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            model.AddElement(address);
            model.SetAnnotationValue(address, new ClrTypeAnnotation(typeof(Address)));
            var cnAddress = new EdmComplexType("NS", "CnAddress", address);
            cnAddress.AddStructuralProperty("Zipcode", EdmPrimitiveTypeKind.String);
            model.AddElement(cnAddress);
            model.SetAnnotationValue(cnAddress, new ClrTypeAnnotation(typeof(CnAddress)));

            var color = new EdmEnumType("NS", "Color");
            model.AddElement(color);
            model.SetAnnotationValue(color, new ClrTypeAnnotation(typeof(Color)));

            var customer = new EdmEntityType("Microsoft.AspNetCore.OData.Abstracts.Tests", "Customer");
            model.AddElement(customer);

            return model;
        }
    }

    public class Address
    {
        public string City { get; set; }
    }

    public class CnAddress : Address
    {
        public string Zipcode { get; set; }
    }

    public enum Color
    {
        Red
    }

    public class Customer
    {
    }
}
