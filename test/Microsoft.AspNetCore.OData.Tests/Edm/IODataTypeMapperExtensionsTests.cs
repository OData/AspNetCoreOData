//-----------------------------------------------------------------------------
// <copyright file="IODataTypeMapperExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class IODataTypeMapperExtensionsTests
    {
        [Fact]
        public void GetPrimitiveType_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            IODataTypeMapper mapper = null;
            ExceptionAssert.ThrowsArgumentNull(() => mapper.GetPrimitiveType(primitiveType: null), "mapper");

            // Arrange & Act & Assert
            mapper = new Mock<IODataTypeMapper>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => mapper.GetPrimitiveType(primitiveType: null), "primitiveType");
        }

        [Fact]
        public void GetPrimitiveType_Calls_GetPrimitiveTypeOnInterface()
        {
            // Arrange
            Mock<IEdmPrimitiveType> primitive = new Mock<IEdmPrimitiveType>();
            Mock<IEdmPrimitiveTypeReference> primitiveRef = new Mock<IEdmPrimitiveTypeReference>();
            primitiveRef.Setup(x => x.Definition).Returns(primitive.Object);
            primitiveRef.SetupGet(x => x.IsNullable).Returns(false);

            Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
            mapper.Setup(s => s.GetClrPrimitiveType(primitive.Object, false)).Verifiable();

            // Act
            mapper.Object.GetPrimitiveType(primitiveRef.Object);

            // Assert
            mapper.Verify();
        }

        [Fact]
        public void GetEdmType_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            IODataTypeMapper mapper = null;
            ExceptionAssert.ThrowsArgumentNull(() => mapper.GetEdmType(null, null), "mapper");
        }

        [Fact]
        public void GetEdmType_Calls_GetEdmTypeReferenceOnInterface()
        {
            // Arrange
            Mock<IEdmModel> model = new Mock<IEdmModel>();
            Type type = typeof(int);

            Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
            mapper.Setup(s => s.GetEdmTypeReference(model.Object, type)).Verifiable();

            // Act
            mapper.Object.GetEdmType(model.Object, type);

            // Assert
            mapper.Verify();
        }

        [Fact]
        public void GetClrType_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            IODataTypeMapper mapper = null;
            ExceptionAssert.ThrowsArgumentNull(() => mapper.GetClrType(null, null), "mapper");

            // Arrange & Act & Assert
            mapper = new Mock<IODataTypeMapper>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => mapper.GetClrType(null, null), "edmType");
        }

        [Fact]
        public void GetClrType_Calls_GetClrTypeOnInterface()
        {
            // Arrange
            Mock<IEdmModel> model = new Mock<IEdmModel>();
            Mock<IEdmPrimitiveType> primitive = new Mock<IEdmPrimitiveType>();
            Mock<IEdmPrimitiveTypeReference> primitiveRef = new Mock<IEdmPrimitiveTypeReference>();
            primitiveRef.Setup(x => x.Definition).Returns(primitive.Object);
            primitiveRef.SetupGet(x => x.IsNullable).Returns(false);

            Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
            mapper.Setup(s => s.GetClrType(model.Object, primitive.Object, false, AssemblyResolverHelper.Default)).Verifiable();

            // Act
            mapper.Object.GetClrType(model.Object, primitiveRef.Object);

            // Assert
            mapper.Verify();
        }

        [Fact]
        public void GetClrTypeWithAssemblyResolver_Calls_GetClrTypeOnInterface()
        {
            // Arrange
            Mock<IAssemblyResolver> resolver = new Mock<IAssemblyResolver>();
            Mock<IEdmModel> model = new Mock<IEdmModel>();
            Mock<IEdmPrimitiveType> primitive = new Mock<IEdmPrimitiveType>();
            Mock<IEdmPrimitiveTypeReference> primitiveRef = new Mock<IEdmPrimitiveTypeReference>();
            primitiveRef.Setup(x => x.Definition).Returns(primitive.Object);
            primitiveRef.SetupGet(x => x.IsNullable).Returns(false);

            Mock<IODataTypeMapper> mapper = new Mock<IODataTypeMapper>();
            mapper.Setup(s => s.GetClrType(model.Object, primitive.Object, false, resolver.Object)).Verifiable();

            // Act
            mapper.Object.GetClrType(model.Object, primitiveRef.Object, resolver.Object);

            // Assert
            mapper.Verify();
        }
    }
}
