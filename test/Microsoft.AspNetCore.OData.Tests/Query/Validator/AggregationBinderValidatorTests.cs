//-----------------------------------------------------------------------------
// <copyright file="AggregationBinderValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator
{
    public class AggregationBinderValidatorTests
    {
        [Fact]
        public void ValidateGroupByExpressionType_DefaultGroupByWrapper_DoesNotThrow()
        {
            // Arrange
            Type defaultGroupByWrapper = typeof(GroupByWrapper);

            // Act & Assert
            AggregationBinderValidator.ValidateGroupByExpressionType(defaultGroupByWrapper);
        }

        [Fact]
        public void ValidateGroupByExpressionType_ValidGroupByWrapper_DoesNotThrow()
        {
            // Arrange
            Type validGroupByWrapper = typeof(ValidGroupByWrapper);

            // Act & Assert
            AggregationBinderValidator.ValidateGroupByExpressionType(validGroupByWrapper);
        }

        [Fact]
        public void ValidateGroupByExpressionType_InvalidGroupByWrapperWithoutInheritance_ThrowsInvalidOperationException()
        {
            // Arrange
            Type invalidGroupByWrapper = typeof(InvalidGroupByWrapperWithoutInheritance);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AggregationBinderValidator.ValidateGroupByExpressionType(invalidGroupByWrapper));
            Assert.Equal(
                $"The type '{typeof(InvalidGroupByWrapperWithoutInheritance).FullName}' does not inherit from 'Microsoft.AspNetCore.OData.Query.Wrapper.DynamicTypeWrapper'.",
                exception.Message);
        }

        [Fact]
        public void ValidateGroupByExpressionType_InvalidGroupByWrapperWithoutInterface_ThrowsInvalidOperationException()
        {
            // Arrange
            Type invalidGroupByWrapper = typeof(InvalidGroupByWrapperWithoutIGroupByWrapperOfTInterface);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AggregationBinderValidator.ValidateGroupByExpressionType(invalidGroupByWrapper));
            Assert.Equal(
                $"The type '{typeof(InvalidGroupByWrapperWithoutIGroupByWrapperOfTInterface).FullName}' does not implement 'Microsoft.AspNetCore.OData.Query.Wrapper.IGroupByWrapper{{T}}' interface.",
                exception.Message);
        }

        [Fact]
        public void ValidateFlattenedExpressionType_DefaultFlatteningWrapper_DoesNotThrow()
        {
            // Arrange
            Type defaultFlatteningWrapper = typeof(FlatteningWrapper<TestSale>);

            // Act & Assert
            AggregationBinderValidator.ValidateFlattenedExpressionType(defaultFlatteningWrapper);
        }

        [Fact]
        public void ValidateFlattenedExpressionType_ValidFlatteningWrapper_DoesNotThrow()
        {
            // Arrange
            Type validFlatteningWrapper = typeof(ValidFlatteningWrapper<TestSale>);

            // Act & Assert
            AggregationBinderValidator.ValidateFlattenedExpressionType(validFlatteningWrapper);
        }

        [Fact]
        public void ValidateFlattenedExpressionType_InvalidFlatteningWrapperWithoutInheritance_ThrowsInvalidOperationException()
        {
            // Arrange
            Type invalidFlatteningWrapper = typeof(InvalidFlatteningWrapperWithoutInheritance);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AggregationBinderValidator.ValidateFlattenedExpressionType(invalidFlatteningWrapper));
            Assert.Equal(
                $"The type '{typeof(InvalidFlatteningWrapperWithoutInheritance).FullName}' does not inherit from 'Microsoft.AspNetCore.OData.Query.Wrapper.DynamicTypeWrapper'.",
                exception.Message);
        }

        [Fact]
        public void ValidateFlattenedExpressionType_InvalidFlatteningWrapperWithoutIGroupByWrapperOfTInterface_ThrowsInvalidOperationException()
        {
            // Arrange
            Type invalidFlatteningWrapper = typeof(InvalidFlatteningWrapperWithoutIGroupByWrapperOfTInterface);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AggregationBinderValidator.ValidateFlattenedExpressionType(invalidFlatteningWrapper));
            Assert.Equal(
                $"The type '{typeof(InvalidFlatteningWrapperWithoutIGroupByWrapperOfTInterface).FullName}' does not implement 'Microsoft.AspNetCore.OData.Query.Wrapper.IGroupByWrapper{{T}}' interface.",
                exception.Message);
        }

        [Fact]
        public void ValidateFlattenedExpressionType_InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface_ThrowsInvalidOperationException()
        {
            // Arrange
            Type invalidFlatteningWrapper = typeof(InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AggregationBinderValidator.ValidateFlattenedExpressionType(invalidFlatteningWrapper));
            Assert.Equal(
                $"The type '{typeof(InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface).FullName}' does not implement 'Microsoft.AspNetCore.OData.Query.Wrapper.IFlatteningWrapper{{T}}' interface.",
                exception.Message);
        }

        [Fact]
        public void ValidateAggregationFlatteningResult_NullResult_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AggregationBinderValidator.ValidateFlatteningResult(null));
        }

        [Fact]
        public void ValidateAggregationFlatteningResult_ValidResult_DoesNotThrow()
        {
            // Arrange
            var wrapperType = typeof(FlatteningWrapper<>).MakeGenericType(typeof(TestSale));
            var validFlatteningResult = new AggregationFlatteningResult
            {
                RedefinedContextParameter = Expression.Parameter(wrapperType, "$it"),
                FlattenedExpression = Expression.Constant(1),
                FlattenedPropertiesMapping = new Dictionary<SingleValueNode, Expression>
                {
                    { new ConstantNode(1), Expression.Constant(1) }
                }
            };

            // Act & Assert
            AggregationBinderValidator.ValidateFlatteningResult(validFlatteningResult);
        }

        [Fact]
        public void ValidateAggregationFlatteningResult_NullFlattenedExpression_ThrowsInvalidOperationException()
        {
            // Arrange
            var wrapperType = typeof(FlatteningWrapper<>).MakeGenericType(typeof(TestSale));
            var invalidFlatteningResult = new AggregationFlatteningResult
            {
                RedefinedContextParameter = Expression.Parameter(wrapperType, "$it"),
                FlattenedExpression = null,
                FlattenedPropertiesMapping = new Dictionary<SingleValueNode, Expression>
                {
                    { new ConstantNode(1), Expression.Constant(1) }
                }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => AggregationBinderValidator.ValidateFlatteningResult(invalidFlatteningResult));
            Assert.Equal("The 'FlattenedExpression' property must be set. (Parameter 'flatteningResult')", exception.Message);
        }

        [Fact]
        public void ValidateAggregationFlatteningResult_NullRedefinedContextParameter_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidFlatteningResult = new AggregationFlatteningResult
            {
                RedefinedContextParameter = null,
                FlattenedExpression = Expression.Constant(1),
                FlattenedPropertiesMapping = new Dictionary<SingleValueNode, Expression>
                {
                    { new ConstantNode(1), Expression.Constant(1) }
                }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => AggregationBinderValidator.ValidateFlatteningResult(invalidFlatteningResult));
            Assert.Equal(
                "The 'RedefinedContextParameter' property must be set when the 'FlattenedExpression' property is set. (Parameter 'flatteningResult')",
                exception.Message);
        }

        [Fact]
        public void ValidateAggregationFlatteningResult_NullFlattenedPropertiesMapping_ThrowsInvalidOperationException()
        {
            // Arrange
            var wrapperType = typeof(FlatteningWrapper<>).MakeGenericType(typeof(TestSale));
            var invalidFlatteningResult = new AggregationFlatteningResult
            {
                RedefinedContextParameter = Expression.Parameter(wrapperType, "$it"),
                FlattenedExpression = Expression.Constant(1),
                FlattenedPropertiesMapping = null
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => AggregationBinderValidator.ValidateFlatteningResult(invalidFlatteningResult));
            Assert.Equal(
                "The 'FlattenedPropertiesMapping' property must be set when the 'FlattenedExpression' property is set. (Parameter 'flatteningResult')",
                exception.Message);
        }

        [Fact]
        public void ValidateAggregationFlatteningResult_EmptyFlattenedPropertiesMapping_ThrowsInvalidOperationException()
        {
            // Arrange
            Type wrapperType = typeof(FlatteningWrapper<>).MakeGenericType(typeof(TestSale));
            var invalidFlatteningResult = new AggregationFlatteningResult
            {
                RedefinedContextParameter = Expression.Parameter(wrapperType, "$it"),
                FlattenedExpression = Expression.Constant(1),
                FlattenedPropertiesMapping = new Dictionary<SingleValueNode, Expression>()
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => AggregationBinderValidator.ValidateFlatteningResult(invalidFlatteningResult));
            Assert.Equal(
                "The 'FlattenedPropertiesMapping' property must be set when the 'FlattenedExpression' property is set. (Parameter 'flatteningResult')",
                exception.Message);
        }
    }

    internal class ValidGroupByWrapper : DynamicTypeWrapper, IGroupByWrapper<AggregationPropertyContainer>
    {
        public AggregationPropertyContainer GroupByContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AggregationPropertyContainer Container { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Dictionary<string, object> Values => throw new NotImplementedException();
    }

    // Does not inherit from DynamicTypeWrapper
    internal class InvalidGroupByWrapperWithoutInheritance : IGroupByWrapper<AggregationPropertyContainer>
    {
        public AggregationPropertyContainer GroupByContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AggregationPropertyContainer Container { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    // Does not implement IGroupByWrapper<AggregationPropertyContainer>
    internal class InvalidGroupByWrapperWithoutIGroupByWrapperOfTInterface : DynamicTypeWrapper
    {
        public override Dictionary<string, object> Values => throw new NotImplementedException();
    }

    internal class ValidFlatteningWrapper<TestSale> : DynamicTypeWrapper, IGroupByWrapper<AggregationPropertyContainer>, IFlatteningWrapper<TestSale>
    {
        public AggregationPropertyContainer GroupByContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AggregationPropertyContainer Container { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Dictionary<string, object> Values => throw new NotImplementedException();
        public TestSale Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    // Does not inherit from DynamicTypeWrapper
    internal class InvalidFlatteningWrapperWithoutInheritance : IGroupByWrapper<AggregationPropertyContainer>, IFlatteningWrapper<TestSale>
    {
        public AggregationPropertyContainer GroupByContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AggregationPropertyContainer Container { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TestSale Source { get; set; }
    }

    // Does not implement IGroupByWrapper<T>
    internal class InvalidFlatteningWrapperWithoutIGroupByWrapperOfTInterface : DynamicTypeWrapper, IFlatteningWrapper<TestSale>
    {
        public override Dictionary<string, object> Values => throw new NotImplementedException();
        public TestSale Source { get; set; }
    }

    // Does not implement IFlatteningWrapper<T>
    internal class InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface : DynamicTypeWrapper, IGroupByWrapper<AggregationPropertyContainer>
    {
        public override Dictionary<string, object> Values => throw new NotImplementedException();
        public AggregationPropertyContainer GroupByContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public AggregationPropertyContainer Container { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    // Test model
    internal class TestSale
    {
        public int Id { get; set; }
    }
}
