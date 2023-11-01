//-----------------------------------------------------------------------------
// <copyright file="DefaultODataTypeMapperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.Spatial;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class DefaultODataTypeMapperTests
    {
        private static IEdmModel EdmModel = GetEdmModel();
        private DefaultODataTypeMapper _mapper = new DefaultODataTypeMapper();

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
        public void GetEdmPrimitiveType_ForClrType_WorksAsExpected_ForStandardPrimitive(Type clrType, string name, bool nullable)
        {
            // Arrange & Act
            IEdmPrimitiveTypeReference primitiveTypeReference = _mapper.GetEdmPrimitiveType(clrType);

            // Assert
            Assert.NotNull(primitiveTypeReference);
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
        public void GetEdmPrimitiveType_ForClrType_WorksAsExpected_ForNonStandardPrimitive(Type clrType, string name, bool nullable)
        {
            // Arrange & Act
            IEdmPrimitiveTypeReference primitiveTypeReference = _mapper.GetEdmPrimitiveType(clrType);

            // Assert
            Assert.NotNull(primitiveTypeReference);
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
        public void GetEdmPrimitiveType_ForClrType_WorksAsExpected_ForSpatialPrimitive(Type clrType, string name)
        {
            // Arrange & Act
            IEdmPrimitiveTypeReference primitiveTypeReference = _mapper.GetEdmPrimitiveType(clrType);

            // Assert
            Assert.NotNull(primitiveTypeReference);
            Assert.Equal(name, primitiveTypeReference.FullName());
            Assert.True(primitiveTypeReference.IsNullable);
        }

        [Theory]
        [InlineData(EdmPrimitiveTypeKind.String, typeof(string), typeof(string))]
        [InlineData(EdmPrimitiveTypeKind.Boolean, typeof(bool?), typeof(bool))]
        [InlineData(EdmPrimitiveTypeKind.Byte, typeof(byte?), typeof(byte))]
        [InlineData(EdmPrimitiveTypeKind.Decimal, typeof(decimal?), typeof(decimal))]
        [InlineData(EdmPrimitiveTypeKind.Double, typeof(double?), typeof(double))]
        [InlineData(EdmPrimitiveTypeKind.Guid, typeof(Guid?), typeof(Guid))]
        [InlineData(EdmPrimitiveTypeKind.Int16, typeof(short?), typeof(short))]
        [InlineData(EdmPrimitiveTypeKind.Int32, typeof(int?), typeof(int))]
        [InlineData(EdmPrimitiveTypeKind.Int64, typeof(long?), typeof(long))]
        [InlineData(EdmPrimitiveTypeKind.SByte, typeof(sbyte?), typeof(sbyte))]
        [InlineData(EdmPrimitiveTypeKind.Single, typeof(float?), typeof(float))]
        [InlineData(EdmPrimitiveTypeKind.DateTimeOffset, typeof(DateTimeOffset?), typeof(DateTimeOffset))]
        [InlineData(EdmPrimitiveTypeKind.Duration, typeof(TimeSpan?), typeof(TimeSpan))]
        [InlineData(EdmPrimitiveTypeKind.Date, typeof(Date?), typeof(Date))]
        [InlineData(EdmPrimitiveTypeKind.TimeOfDay, typeof(TimeOfDay?), typeof(TimeOfDay))]
        [InlineData(EdmPrimitiveTypeKind.Binary, typeof(byte[]), typeof(byte[]))]
        [InlineData(EdmPrimitiveTypeKind.Stream, typeof(Stream), typeof(Stream))]
        public void GetClrPrimitiveType_ForEdmType_WorksAsExpected_ForStandardPrimitive(EdmPrimitiveTypeKind kind, Type nullExpected, Type nonNullExpected)
        {
            // Arrange & Act & Assert
            IEdmPrimitiveType primitiveType = EdmCoreModel.Instance.GetPrimitiveType(kind);
            Type clrType = _mapper.GetClrPrimitiveType(primitiveType, true);
            Assert.Equal(nullExpected, clrType);

            // Arrange & Act & Assert
            clrType = _mapper.GetClrPrimitiveType(primitiveType, false);
            Assert.Equal(nonNullExpected, clrType);
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
        public void GetClrPrimitiveType_ForEdmType_WorksAsExpected_ForSpatialPrimitive(EdmPrimitiveTypeKind kind, Type expected)
        {
            // Arrange & Act & Assert
            IEdmPrimitiveType primitiveType = EdmCoreModel.Instance.GetPrimitiveType(kind);
            Type clrType = _mapper.GetClrPrimitiveType(primitiveType, true);
            Assert.Equal(expected, clrType);

            // Arrange & Act & Assert
            clrType = _mapper.GetClrPrimitiveType(primitiveType, false);
            Assert.Equal(expected, clrType);
        }

        [Theory]
        [InlineData(EdmPrimitiveTypeKind.None)]
        [InlineData(EdmPrimitiveTypeKind.PrimitiveType)]
        public void GetPrimitiveType_ForEdmType_WorksAsExpected_ForNotUsedKind(EdmPrimitiveTypeKind kind)
        {
            // Arrange & Act & Assert
            IEdmPrimitiveType primitiveType = EdmCoreModel.Instance.GetPrimitiveType(kind);
            Type clrType = _mapper.GetClrPrimitiveType(primitiveType, true);
            Assert.Null(clrType);

            // Arrange & Act & Assert
            clrType = _mapper.GetClrPrimitiveType(primitiveType, false);
            Assert.Null(clrType);
        }
        #endregion

        #region GetClrType
        [Fact]
        public void GetClrType_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            Mock<IEdmType> edmType = new Mock<IEdmType>();
            edmType.Setup(x => x.TypeKind).Returns(EdmTypeKind.Entity);
            ExceptionAssert.ThrowsArgumentNull(() => _mapper.GetClrType(null, edmType.Object, true, null), "edmModel");

            IEdmModel model = new Mock<IEdmModel>().Object;
            IAssemblyResolver resolver = new Mock<IAssemblyResolver>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => _mapper.GetClrType(model, null, true, resolver), "edmType");
        }

        [Fact]
        public void FindClrType_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            Mock<IEdmType> edmType = new Mock<IEdmType>();
            edmType.Setup(x => x.TypeKind).Returns(EdmTypeKind.Entity);
            ExceptionAssert.ThrowsArgumentNull(() => DefaultODataTypeMapper.FindClrType(null, edmType.Object, null), "edmModel");

            IEdmModel model = new Mock<IEdmModel>().Object;
            IAssemblyResolver resolver = new Mock<IAssemblyResolver>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => DefaultODataTypeMapper.FindClrType(model, null, resolver), "edmType");

            ExceptionAssert.ThrowsArgumentNull(() => DefaultODataTypeMapper.FindClrType(model, edmType.Object, null), "assembliesResolver");
        }

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
        public void GetClrType_WorksAsExpected_ForStandardPrimitive(EdmPrimitiveTypeKind kind, Type expected)
        {
            // #1 Arrange & Act & Assert for nullable equals to false
            IEdmPrimitiveType primitiveType = EdmCoreModel.Instance.GetPrimitiveType(kind);
            Type clrType = _mapper.GetClrType(EdmModel, primitiveType, false, assembliesResolver: null);
            Assert.Equal(expected, clrType);

            // #2 Arrange & Act & Assert for nullable equals to true
            clrType = _mapper.GetClrType(EdmModel, primitiveType, true, assembliesResolver: null);
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
        public void GetClrType_WorksAsExpected_ForSpatialPrimitive(EdmPrimitiveTypeKind kind, Type type)
        {
            // Arrange
            IEdmPrimitiveType primitiveType = EdmCoreModel.Instance.GetPrimitiveType(kind);

            // Act
            Type clrType1 = _mapper.GetClrType(EdmModel, primitiveType, false, assembliesResolver: null);
            Type clrType2 = _mapper.GetClrType(EdmModel, primitiveType, true, assembliesResolver: null);

            // Assert
            Assert.Same(clrType1, clrType2);
            Assert.Same(type, clrType1);
        }

        [Theory]
        [InlineData("NS.Address", typeof(MyAddress))] // use ClrTypeAnnotation
        [InlineData("NS.CnAddress", typeof(CnMyAddress))]
        [InlineData("Microsoft.AspNetCore.OData.Tests.Edm.MyCustomer", typeof(MyCustomer))] // use the full name match
        public void GetClrType_WorksAsExpected_ForSchemaStrucutralType(string typeName, Type expected)
        {
            // Arrange
            IEdmType edmType = EdmModel.FindType(typeName);
            Assert.NotNull(edmType); // Guard

            // #1. Act & Assert
            Type clrType = _mapper.GetClrType(EdmModel, edmType, true, new AssemblyResolver());
            Assert.Same(expected, clrType);

            // #2. Act & Assert
            clrType = _mapper.GetClrType(EdmModel, edmType, false, new AssemblyResolver());
            Assert.Same(expected, clrType);
        }

        [Fact]
        public void GetClrType_WorksAsExpected_ForSchemaEnumType()
        {
            // Arrange
            IEdmType edmType = EdmModel.FindType("NS.Color");
            Assert.NotNull(edmType); // Guard

            // #1. Act & Assert
            Type clrType = _mapper.GetClrType(EdmModel, edmType, true, null);
            Assert.Same(typeof(MyColor?), clrType);

            // #2. Act & Assert
            clrType = _mapper.GetClrType(EdmModel, edmType, false, null);
            Assert.Same(typeof(MyColor), clrType);
        }

        #endregion

        #region GetEdmType
        [Fact]
        public void GetEdmType_ThrowsArgumentNull_ModelAndClrType()
        {
            // Arrange & Act
            IEdmModel model = null;
            ExceptionAssert.ThrowsArgumentNull(() => _mapper.GetEdmTypeReference(model, typeof(TypeNotInModel)), "edmModel");

            model = new Mock<IEdmModel>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => _mapper.GetEdmTypeReference(model, null), "clrType");
        }

        [Fact]
        public void GetEdmTypeReference_ReturnsNull_ForUnknownType()
        {
            // Arrange & Act & Assert
            Assert.Null(_mapper.GetEdmTypeReference(EdmModel, typeof(TypeNotInModel)));
        }

        [Theory]
        [InlineData(typeof(IEnumerable<BaseType>), "NS.BaseType")]
        [InlineData(typeof(IEnumerable<Derived1Type>), "NS.Derived1Type")]
        [InlineData(typeof(Derived2Type[]), "NS.Derived2Type")]
        public void GetEdmTypeReference_ReturnsCollection_ForIEnumerableOfT(Type clrType, string typeName)
        {
            // Arrange & Act
            IEdmType edmType = _mapper.GetEdmType(EdmModel, clrType);

            // Assert
            Assert.Equal(EdmTypeKind.Collection, edmType.TypeKind);
            Assert.Equal(typeName, (edmType as IEdmCollectionType).ElementType.FullName());
        }

        [Theory]
        [InlineData(typeof(string), "Edm.String")]
        [InlineData(typeof(int?), "Edm.Int32")]
        [InlineData(typeof(MyAddress), "NS.Address")]
        [InlineData(typeof(CnMyAddress), "NS.CnAddress")]
        [InlineData(typeof(MyCustomer), "Microsoft.AspNetCore.OData.Tests.Edm.MyCustomer")]
        [InlineData(typeof(BaseType), "NS.BaseType")]
        [InlineData(typeof(Derived1Type), "NS.Derived1Type")]
        [InlineData(typeof(Derived2Type), "NS.Derived2Type")]
        [InlineData(typeof(SubDerivedType), "NS.SubDerivedType")]
        public void GetEdmTypeReference_WorksAsExpected_ForEdmType(Type clrType, string typeName)
        {
            // Arrange
            IEdmType expectedEdmType = EdmModel.FindType(typeName);
            Assert.NotNull(expectedEdmType); // Guard

            // Arrange & Act
            IEdmTypeReference edmTypeRef = _mapper.GetEdmTypeReference(EdmModel, clrType);
            IEdmType edmType = _mapper.GetEdmType(EdmModel, clrType);

            // Assert
            Assert.NotNull(edmTypeRef);
            Assert.Same(expectedEdmType, edmTypeRef.Definition);
            Assert.Same(expectedEdmType, edmType);
            Assert.True(edmTypeRef.IsNullable);
        }

        [Fact]
        public void GetEdmTypeReference_WorksAsExpected_ForSchemaEnumType()
        {
            // Arrange
            IEdmType expectedType = EdmModel.FindType("NS.Color");
            Assert.NotNull(expectedType); // Guard

            // #1. Act & Assert
            IEdmTypeReference colorType = _mapper.GetEdmTypeReference(EdmModel, typeof(MyColor));
            Assert.Same(expectedType, colorType.Definition);
            Assert.False(colorType.IsNullable);

            // #2. Act & Assert
            colorType = _mapper.GetEdmTypeReference(EdmModel, typeof(MyColor?));
            Assert.Same(expectedType, colorType.Definition);
            Assert.True(colorType.IsNullable);
        }
        #endregion

        [Theory]
        [InlineData(typeof(MyCustomer), "MyCustomer")]
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

            // ComplexType: Address
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            model.AddElement(address);
            model.SetAnnotationValue(address, new ClrTypeAnnotation(typeof(MyAddress)));

            // ComplexType: CnAddress
            var cnAddress = new EdmComplexType("NS", "CnAddress", address);
            cnAddress.AddStructuralProperty("Zipcode", EdmPrimitiveTypeKind.String);
            model.AddElement(cnAddress);
            model.SetAnnotationValue(cnAddress, new ClrTypeAnnotation(typeof(CnMyAddress)));

            // EnumType: Color
            var color = new EdmEnumType("NS", "Color");
            model.AddElement(color);
            model.SetAnnotationValue(color, new ClrTypeAnnotation(typeof(MyColor)));

            // EntityType: MyCustomer
            var customer = new EdmEntityType("Microsoft.AspNetCore.OData.Tests.Edm", "MyCustomer");
            model.AddElement(customer);

            // Inheritance EntityType
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

        public class MyAddress
        {
            public string City { get; set; }
        }

        public class CnMyAddress : MyAddress
        {
            public string Zipcode { get; set; }
        }

        public enum MyColor
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

        public class AssemblyResolver : IAssemblyResolver
        {
            public IEnumerable<Assembly> Assemblies
            {
                get
                {
                    yield return typeof(AssemblyResolver).Assembly;
                }
            }
        }
    }

    public class MyCustomer
    { }
}
