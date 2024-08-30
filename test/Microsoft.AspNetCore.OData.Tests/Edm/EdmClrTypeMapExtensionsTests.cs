//-----------------------------------------------------------------------------
// <copyright file="EdmClrTypeMapExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm;

public class EdmClrTypeMapExtensionsTests
{
    [Fact]
    public void GetEdmPrimitiveTypeReference_Calls_GetPrimitiveTypeOnMapper()
    {
        // Arrange
        Type type = typeof(int);
        Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
        mapper.Setup(x => x.GetEdmPrimitiveType(type)).Verifiable();

        EdmModel model = new EdmModel();
        model.SetTypeMapper(mapper.Object);

        // Act
        model.GetEdmPrimitiveTypeReference(type);

        // Assert
        mapper.Verify();
    }

    [Fact]
    public void GetClrPrimitiveType_Calls_GetPrimitiveTypeOnMapper()
    {
        // Arrange
        Mock<IEdmPrimitiveType> primitiveType = new Mock<IEdmPrimitiveType>();
        Mock<IEdmPrimitiveTypeReference> edmType = new Mock<IEdmPrimitiveTypeReference>();
        edmType.Setup(x => x.Definition).Returns(primitiveType.Object);
        edmType.Setup(x => x.IsNullable).Returns(true);

        Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
        mapper.Setup(x => x.GetClrPrimitiveType(edmType.Object.PrimitiveDefinition(), edmType.Object.IsNullable)).Verifiable();

        EdmModel model = new EdmModel();
        model.SetTypeMapper(mapper.Object);

        // Act
        model.GetClrPrimitiveType(edmType.Object);

        // Assert
        mapper.Verify();
    }

    [Theory]
    [InlineData(null, null, false)]
    [InlineData(typeof(int), typeof(int), false)]
    [InlineData(typeof(int?), typeof(int?), false)]
    [InlineData(typeof(object), typeof(object), false)]
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
    public void IsNonstandardEdmPrimitive_WorksAsExpected_ForNonstandardType(Type clrType, Type expectType, bool isNonstandard)
    {
        // Arrange
        EdmModel model = new EdmModel();
        model.SetTypeMapper(DefaultODataTypeMapper.Default);

        // Act
        Type actual = model.IsNonstandardEdmPrimitive(clrType, out bool isNonstandardEdmPrimtive);

        // Assert
        Assert.Equal(expectType, actual);
        Assert.Equal(isNonstandard, isNonstandardEdmPrimtive);
    }

    [Fact]
    public void GetEdmTypeReference_Calls_GetEdmTypeReferenceOnMapper()
    {
        // Arrange
        Type type = typeof(int);
        EdmModel model = new EdmModel();

        Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
        mapper.Setup(x => x.GetEdmTypeReference(model, type)).Verifiable();
        model.SetTypeMapper(mapper.Object);

        // Act
        model.GetEdmTypeReference(type);

        // Assert
        mapper.Verify();
    }

    [Fact]
    public void GetEdmType_Calls_GetEdmTypeReferenceOnMapper()
    {
        // Arrange
        Type type = typeof(int);
        EdmModel model = new EdmModel();

        Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
        mapper.Setup(x => x.GetEdmTypeReference(model, type)).Verifiable();
        model.SetTypeMapper(mapper.Object);

        // Act
        model.GetEdmType(type);

        // Assert
        mapper.Verify();
    }

    [Fact]
    public void GetClrType_Calls_GetClrTypeOnMapper()
    {
        // Arrange
        Mock<IEdmType> edmType = new Mock<IEdmType>();
        Mock<IEdmTypeReference> edmTypeRef = new Mock<IEdmTypeReference>();
        edmTypeRef.Setup(x => x.Definition).Returns(edmType.Object);
        edmTypeRef.Setup(x => x.IsNullable).Returns(true);

        EdmModel model = new EdmModel();

        Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
        mapper.Setup(x => x.GetClrType(model, edmType.Object, true, AssemblyResolverHelper.Default)).Verifiable();
        model.SetTypeMapper(mapper.Object);

        // Act
        model.GetClrType(edmTypeRef.Object);

        // Assert
        mapper.Verify();
    }

    [Fact]
    public void GetClrTypeWithResolver_Calls_GetClrTypeOnMapper()
    {
        // Arrange
        Mock<IAssemblyResolver> resolver = new Mock<IAssemblyResolver>();
        Mock<IEdmType> edmType = new Mock<IEdmType>();
        Mock<IEdmTypeReference> edmTypeRef = new Mock<IEdmTypeReference>();
        edmTypeRef.Setup(x => x.Definition).Returns(edmType.Object);
        edmTypeRef.Setup(x => x.IsNullable).Returns(true);

        EdmModel model = new EdmModel();

        Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
        mapper.Setup(x => x.GetClrType(model, edmType.Object, true, resolver.Object)).Verifiable();
        model.SetTypeMapper(mapper.Object);

        // Act
        model.GetClrType(edmTypeRef.Object, resolver.Object);

        // Assert
        mapper.Verify();
    }

    [Fact]
    public void GetClrTypeUsingEdmType_Calls_GetClrTypeOnMapper()
    {
        // Arrange
        Mock<IEdmType> edmType = new Mock<IEdmType>();

        EdmModel model = new EdmModel();
        Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
        mapper.Setup(x => x.GetClrType(model, edmType.Object, true, AssemblyResolverHelper.Default)).Verifiable();
        model.SetTypeMapper(mapper.Object);

        // Act
        model.GetClrType(edmType.Object);

        // Assert
        mapper.Verify();
    }

    [Fact]
    public void GetClrTypeUsingEdmTypeWithResolver_Calls_GetClrTypeOnMapper()
    {
        // Arrange
        Mock<IAssemblyResolver> resolver = new Mock<IAssemblyResolver>();
        Mock<IEdmType> edmType = new Mock<IEdmType>();

        EdmModel model = new EdmModel();

        Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
        mapper.Setup(x => x.GetClrType(model, edmType.Object, true, resolver.Object)).Verifiable();
        model.SetTypeMapper(mapper.Object);

        // Act
        model.GetClrType(edmType.Object, resolver.Object);

        // Assert
        mapper.Verify();
    }

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
}
