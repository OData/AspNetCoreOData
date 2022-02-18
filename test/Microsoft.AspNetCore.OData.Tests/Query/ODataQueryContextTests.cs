//-----------------------------------------------------------------------------
// <copyright file="ODataQueryContextTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class ODataQueryContextTests
    {
        [Fact]
        public void CtorODataQueryContext_TakingClrType_Throws_With_Null_Model()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataQueryContext(model: null, elementClrType: typeof(int)),
                    "model");
        }

        [Fact]
        public void CtorODataQueryContext_TakingClrType_Throws_With_Null_Type()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataQueryContext(EdmCoreModel.Instance, elementClrType: null),
                    "elementClrType");
        }

        [Fact]
        public void CtorODataQueryContext_TakingClrType_SetsProperties()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();
            IEdmModel model = builder.GetEdmModel();

            // Act
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));

            // Assert
            Assert.Same(model, context.Model);
            Assert.True(context.ElementClrType == typeof(Customer));
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(Order))]
        public void CtorODataQueryContext_TakingClrType_Throws_For_UnknownType(Type elementType)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();
            IEdmModel model = builder.GetEdmModel();

            // Act && Assert
            ExceptionAssert.ThrowsArgument(() => new ODataQueryContext(model, elementType),
                "elementClrType",
                Error.Format("The given model does not contain the type '{0}'.", elementType.FullName));
        }

        [Fact]
        public void CtorODataQueryContext_TakingClrTypeAndPath_SetsProperties()
        {
            // Arrange
            ODataModelBuilder odataModel = new ODataModelBuilder().Add_Customer_EntityType();
            string setName = typeof(Customer).Name;
            odataModel.EntitySet<Customer>(setName);
            IEdmModel model = odataModel.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(setName);
            IEdmEntityType entityType = entitySet.EntityType();
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));

            // Act
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer), path);

            // Assert
            Assert.Same(model, context.Model);
            Assert.Same(entityType, context.ElementType);
            Assert.Same(entitySet, context.NavigationSource);
            Assert.Same(typeof(Customer), context.ElementClrType);
        }

        [Fact]
        public void CtorODataQueryContext_TakingEdmType_ThrowsArgumentNull_Model()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataQueryContext(model: null, elementType: new Mock<IEdmType>().Object),
                "model");
        }

        [Fact]
        public void CtorODataQueryContext_TakingEdmType_ThrowsArgumentNull_ElementType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataQueryContext(EdmCoreModel.Instance, elementType: null),
                "elementType");
        }

        [Fact]
        public void CtorODataQueryContext_TakingEdmType_InitializesProperties()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmType elementType = new Mock<IEdmType>().Object;

            // Act
            var context = new ODataQueryContext(model, elementType);

            // Assert
            Assert.Same(model, context.Model);
            Assert.Same(elementType, context.ElementType);
            Assert.Null(context.ElementClrType);
        }

        [Fact]
        public void CtorODataQueryContext_TakingEdmTypeAndPath_SetsProperties()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmEntityType entityType = new Mock<IEdmEntityType>().Object;
            IEdmEntityContainer entityContiner = new Mock<IEdmEntityContainer>().Object;
            EdmEntitySet entitySet = new EdmEntitySet(entityContiner, "entitySet", entityType);
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));

            // Act
            ODataQueryContext context = new ODataQueryContext(model, entityType, path);

            // Assert
            Assert.Same(model, context.Model);
            Assert.Same(entityType, context.ElementType);
            Assert.Same(entitySet, context.NavigationSource);
            Assert.Null(context.ElementClrType);
        }

        [Theory]
        // Edm primitive kinds
        [InlineData(typeof(byte[]))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(Date))]
        [InlineData(typeof(TimeOfDay))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(double))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(short))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(float))]
        [InlineData(typeof(string))]
        [InlineData(typeof(TimeSpan))]
        // additional types not considered Edm primitives
        // but which we permit in $skip and $top
        [InlineData(typeof(int?))]
        [InlineData(typeof(char))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        public void CtorODataQueryContext_TakingClrType_WithPrimitiveTypes(Type type)
        {
            // Arrange & Act
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, type);

            // Assert
            Assert.True(context.ElementClrType == type);
        }

        [Theory]
        [InlineData(typeof(FlagsEnum))]
        [InlineData(typeof(SimpleEnum))]
        [InlineData(typeof(SimpleEnum?))]
        [InlineData(typeof(LongEnum))]
        [InlineData(typeof(FlagsEnum?))]
        public void CtorODataQueryContext_TakingClrType_WithEnumTypes(Type type)
        {
            // Arrange
            Type enumType = Nullable.GetUnderlyingType(type) ?? type;

            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEnumType(enumType);
            IEdmModel model = builder.GetEdmModel();

            // Act
            ODataQueryContext context = new ODataQueryContext(model, type);

            // Assert
            Assert.True(context.ElementClrType == type);
        }
    }
}
