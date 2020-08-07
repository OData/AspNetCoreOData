// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.Spatial;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class EdmClrTypeMapExtensionsTests
    {
        private static IEdmModel EdmModel = GetEdmModel();

        #region PrimitiveType
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
        public void GetEdmPrimitiveTypeReferenceWorksAsExpectedForStandardPrimitive(Type clrType, string name, bool nullable)
        {
            // Arrange & Act
            IEdmPrimitiveTypeReference primitiveTypeReference = clrType.GetEdmPrimitiveTypeReference();
            IEdmPrimitiveType primitiveType = clrType.GetEdmPrimitiveType();

            // Assert
            Assert.NotNull(primitiveTypeReference);
            Assert.Same(primitiveTypeReference.Definition, primitiveType);
            Assert.Equal(name, primitiveTypeReference.FullName());
            Assert.Equal(nullable, primitiveTypeReference.IsNullable);
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
        public void GetEdmPrimitiveTypeReferenceWorksAsExpectedForNonStandardPrimitive(Type clrType, string name, bool nullable)
        {
            // Arrange & Act
            IEdmPrimitiveTypeReference primitiveTypeReference = clrType.GetEdmPrimitiveTypeReference();
            IEdmPrimitiveType primitiveType = clrType.GetEdmPrimitiveType();

            // Assert
            Assert.NotNull(primitiveTypeReference);
            Assert.Same(primitiveTypeReference.Definition, primitiveType);
            Assert.Equal(name, primitiveTypeReference.FullName());
            Assert.Equal(nullable, primitiveTypeReference.IsNullable);
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
            IEdmPrimitiveTypeReference primitiveTypeReference = clrType.GetEdmPrimitiveTypeReference();
            IEdmPrimitiveType primitiveType = clrType.GetEdmPrimitiveType();

            // Assert
            Assert.NotNull(primitiveTypeReference);
            Assert.Same(primitiveTypeReference.Definition, primitiveType);
            Assert.Equal(name, primitiveTypeReference.FullName());
            Assert.True(primitiveTypeReference.IsNullable);
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(typeof(int), typeof(int), false)]
        [InlineData(typeof(int?), typeof(int?), false)]
        [InlineData(typeof(object), typeof(object), false)]
        [InlineData(typeof(Address), typeof(Address), false)]
        // non-standard primitive types
        [InlineData(typeof(XElement), typeof(string), true)]
        [InlineData(typeof(ushort), typeof(int), true)]
        [InlineData(typeof(ushort?), typeof(int?), true)]
        [InlineData(typeof(uint), typeof(long), true)]
        [InlineData(typeof(uint?), typeof(long?), true)]
        [InlineData(typeof(ulong), typeof(long), true)]
        [InlineData(typeof(ulong?), typeof(long?), true)]
        [InlineData(typeof(char[]), typeof(string), true)]
        [InlineData(typeof(char), typeof(string), true)]
        [InlineData(typeof(char?), typeof(string), true)]
        [InlineData(typeof(DateTime), typeof(DateTimeOffset), true)]
        [InlineData(typeof(DateTime?), typeof(DateTimeOffset?), true)]
        public void IsNonstandardEdmPrimitiveWorksAsExpectedForNonstandardType(Type clrType, Type expectType, bool isNonstandard)
        {
            // Arrange & Act
            Type actual = clrType.IsNonstandardEdmPrimitive(out bool isNonstandardEdmPrimtive);

            // Assert
            Assert.Equal(expectType, actual);
            Assert.Equal(isNonstandard, isNonstandardEdmPrimtive);
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
            Type clrType = EdmModel.GetClrType(primitiveType);
            Assert.Equal(expected, clrType);

            // #2 Arrange & Act & Assert for nullable equals to true
            primitiveType = EdmCoreModel.Instance.GetPrimitive(kind, true);
            clrType = EdmModel.GetClrType(primitiveType);
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
        [InlineData(EdmPrimitiveTypeKind.GeographyMultiLineString, typeof(GeographyMultiLineString))]
        [InlineData(EdmPrimitiveTypeKind.GeographyMultiPoint, typeof(GeographyMultiPoint))]
        [InlineData(EdmPrimitiveTypeKind.GeographyMultiPolygon, typeof(GeographyMultiPolygon))]
        [InlineData(EdmPrimitiveTypeKind.Geometry, typeof(Geometry))]
        [InlineData(EdmPrimitiveTypeKind.GeometryPoint, typeof(GeometryPoint))]
        [InlineData(EdmPrimitiveTypeKind.GeometryLineString, typeof(GeometryLineString))]
        [InlineData(EdmPrimitiveTypeKind.GeometryPolygon, typeof(GeometryPolygon))]
        [InlineData(EdmPrimitiveTypeKind.GeometryCollection, typeof(GeometryCollection))]
        [InlineData(EdmPrimitiveTypeKind.GeometryMultiLineString, typeof(GeometryMultiLineString))]
        [InlineData(EdmPrimitiveTypeKind.GeometryMultiPoint, typeof(GeometryMultiPoint))]
        [InlineData(EdmPrimitiveTypeKind.GeometryMultiPolygon, typeof(GeometryMultiPolygon))]
        public void GetClrTypeWorksAsExpectedForSpatialPrimitive(EdmPrimitiveTypeKind kind, Type type)
        {
            // Arrange
            IEdmPrimitiveTypeReference primitiveType1 = EdmCoreModel.Instance.GetPrimitive(kind, true);
            IEdmPrimitiveTypeReference primitiveType2 = EdmCoreModel.Instance.GetPrimitive(kind, false);

            // Act
            Type clrType1 = EdmModel.GetClrType(primitiveType1);
            Type clrType2 = EdmModel.GetClrType(primitiveType2);

            // Assert
            Assert.Same(clrType1, clrType2);
            Assert.Same(type, clrType1);
        }

        [Theory]
        [InlineData("NS.Address", typeof(Address))] // use ClrTypeAnnotation
        [InlineData("NS.CnAddress", typeof(CnAddress))]
        [InlineData("Microsoft.AspNetCore.OData.Tests.Edm.Customer", typeof(Customer))] // use the full name match
        public void GetClrTypeWorksAsExpectedForSchemaStrucutralType(string typeName, Type expected)
        {
            // Arrange
            IEdmType edmType = EdmModel.FindType(typeName);
            Assert.NotNull(edmType); // Guard

            // #1. Act & Assert
            IEdmTypeReference edmTypeReference = edmType.ToEdmTypeReference(true);
            Type clrType = EdmModel.GetClrType(edmTypeReference);
            Assert.Same(expected, clrType);

            // #2. Act & Assert
            edmTypeReference = edmType.ToEdmTypeReference(false);
            clrType = EdmModel.GetClrType(edmTypeReference);
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
            Type clrType = EdmModel.GetClrType(edmTypeReference);
            Assert.Same(typeof(Color?), clrType);

            // #2. Act & Assert
            edmTypeReference = edmType.ToEdmTypeReference(false);
            clrType = EdmModel.GetClrType(edmTypeReference);
            Assert.Same(typeof(Color), clrType);
        }

        #endregion

        #region GetEdmType

        [Fact]
        public void GetEdmTypeReferenceReturnsNullForUnknownType()
        {
            // Arrange & Act & Assert
            Assert.Null(EdmModel.GetEdmTypeReference(typeof(TypeNotInModel)));
            Assert.Null(EdmModel.GetEdmType(typeof(TypeNotInModel)));
        }

        [Theory]
        [InlineData(typeof(IEnumerable<BaseType>), "NS.BaseType")]
        [InlineData(typeof(IEnumerable<Derived1Type>), "NS.Derived1Type")]
        [InlineData(typeof(Derived2Type[]), "NS.Derived2Type")]
        public void GetEdmTypeReferenceReturnsCollectionForIEnumerableOfT(Type clrType, string typeName)
        {
            // Arrange & Act
            IEdmType edmType = EdmModel.GetEdmType(clrType);

            // Assert
            Assert.Equal(EdmTypeKind.Collection, edmType.TypeKind);
            Assert.Equal(typeName, (edmType as IEdmCollectionType).ElementType.FullName());
        }

        [Theory]
        [InlineData(typeof(string), "Edm.String")]
        [InlineData(typeof(int?), "Edm.Int32")]
        [InlineData(typeof(Address), "NS.Address")]
        [InlineData(typeof(CnAddress), "NS.CnAddress")]
        [InlineData(typeof(Customer), "Microsoft.AspNetCore.OData.Tests.Edm.Customer")]
        [InlineData(typeof(BaseType), "NS.BaseType")]
        [InlineData(typeof(Derived1Type), "NS.Derived1Type")]
        [InlineData(typeof(Derived2Type), "NS.Derived2Type")]
        [InlineData(typeof(SubDerivedType), "NS.SubDerivedType")]
        public void GetEdmTypeReferenceWorksAsExpectedForEdmType(Type clrType, string typeName)
        {
            // Arrange
            IEdmType expectedEdmType = EdmModel.FindType(typeName);
            Assert.NotNull(expectedEdmType); // Guard

            // Arrange & Act
            IEdmTypeReference edmTypeRef = EdmModel.GetEdmTypeReference(clrType);
            IEdmType edmType = EdmModel.GetEdmType(clrType);

            // Assert
            Assert.NotNull(edmTypeRef);
            Assert.Same(expectedEdmType, edmTypeRef.Definition);
            Assert.Same(expectedEdmType, edmType);
            Assert.True(edmTypeRef.IsNullable);
        }

        [Fact]
        public void GetEdmTypeWorksAsExpectedForSchemaEnumType()
        {
            // Arrange
            IEdmType expectedType = EdmModel.FindType("NS.Color");
            Assert.NotNull(expectedType); // Guard

            // #1. Act & Assert
            IEdmTypeReference colorType = EdmModel.GetEdmTypeReference(typeof(Color));
            Assert.Same(expectedType, colorType.Definition);
            Assert.False(colorType.IsNullable);

            // #2. Act & Assert
            colorType = EdmModel.GetEdmTypeReference(typeof(Color?));
            Assert.Same(expectedType, colorType.Definition);
            Assert.True(colorType.IsNullable);
        }
        #endregion

        [Theory]
        [InlineData(typeof(Customer), "Customer")]
        [InlineData(typeof(int), "Int32")]
        [InlineData(typeof(IEnumerable<int>), "IEnumerable_1OfInt32")]
        [InlineData(typeof(IEnumerable<Func<int, string>>), "IEnumerable_1OfFunc_2OfInt32_String")]
        [InlineData(typeof(List<Func<int, string>>), "List_1OfFunc_2OfInt32_String")]
        public void EdmFullName(Type clrType, string expectedName)
        {
            // Arrange & Act & Assert
            Assert.Equal(expectedName, clrType.EdmName());
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();
            EdmComplexType address = new EdmComplexType("NS", "Address");
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

            var customer = new EdmEntityType("Microsoft.AspNetCore.OData.Tests.Edm", "Customer");
            model.AddElement(customer);

            // Inheritance
            var baseEntity = new EdmEntityType("NS", "BaseType");
            var derived1Entity = new EdmEntityType("NS", "Derived1Type", baseEntity);
            var derived2Entity = new EdmEntityType("NS", "Derived2Type", baseEntity);
            var subDerivedEntity = new EdmEntityType("NS", "SubDerivedType", derived1Entity);
            model.AddElements(new[] { baseEntity, derived1Entity, derived2Entity, subDerivedEntity });
            model.SetAnnotationValue(baseEntity, new ClrTypeAnnotation(typeof(BaseType)));
            model.SetAnnotationValue(derived1Entity, new ClrTypeAnnotation(typeof(Derived1Type)));
            model.SetAnnotationValue(derived2Entity, new ClrTypeAnnotation(typeof(Derived2Type)));
            model.SetAnnotationValue(subDerivedEntity, new ClrTypeAnnotation(typeof(SubDerivedType)));

            return model;
        }
    }

    public class Customer
    { }

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

    public class BaseType
    { }

    public class Derived1Type : BaseType
    { }

    public class Derived2Type : BaseType
    { }

    public class SubDerivedType : Derived1Type
    { }

    public class TypeNotInModel
    { }
}
