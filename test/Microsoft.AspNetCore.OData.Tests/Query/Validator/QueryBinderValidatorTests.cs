//-----------------------------------------------------------------------------
// <copyright file="QueryBinderValidatorTests.cs" company=".NET Foundation">
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
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class QueryBinderValidatorTests
{
    [Fact]
    public void ValidateGroupByExpressionType_DefaultGroupByWrapper_DoesNotThrow()
    {
        // Arrange
        Type defaultGroupByWrapper = typeof(GroupByWrapper);

        // Act & Assert
        QueryBinderValidator.ValidateGroupByExpressionType(defaultGroupByWrapper);
    }

    [Fact]
    public void ValidateGroupByExpressionType_ValidGroupByWrapper_DoesNotThrow()
    {
        // Arrange
        Type validGroupByWrapper = typeof(ValidGroupByWrapper);

        // Act & Assert
        QueryBinderValidator.ValidateGroupByExpressionType(validGroupByWrapper);
    }

    [Fact]
    public void ValidateGroupByExpressionType_InvalidGroupByWrapperWithoutInheritance_ThrowsInvalidOperationException()
    {
        // Arrange
        Type invalidGroupByWrapper = typeof(InvalidGroupByWrapperWithoutInheritance);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => QueryBinderValidator.ValidateGroupByExpressionType(invalidGroupByWrapper));
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
        var exception = Assert.Throws<InvalidOperationException>(() => QueryBinderValidator.ValidateGroupByExpressionType(invalidGroupByWrapper));
        Assert.Equal(
            $"The type '{typeof(InvalidGroupByWrapperWithoutIGroupByWrapperOfTInterface).FullName}' does not implement 'Microsoft.AspNetCore.OData.Query.Wrapper.IGroupByWrapper{{TContainer,TWrapper}}' interface.",
            exception.Message);
    }

    [Fact]
    public void ValidateFlattenedExpressionType_DefaultFlatteningWrapper_DoesNotThrow()
    {
        // Arrange
        Type defaultFlatteningWrapper = typeof(FlatteningWrapper<TestSale>);

        // Act & Assert
        QueryBinderValidator.ValidateFlattenedExpressionType(defaultFlatteningWrapper);
    }

    [Fact]
    public void ValidateFlattenedExpressionType_ValidFlatteningWrapper_DoesNotThrow()
    {
        // Arrange
        Type validFlatteningWrapper = typeof(ValidFlatteningWrapper<TestSale>);

        // Act & Assert
        QueryBinderValidator.ValidateFlattenedExpressionType(validFlatteningWrapper);
    }

    [Fact]
    public void ValidateFlattenedExpressionType_InvalidFlatteningWrapperWithoutInheritance_ThrowsInvalidOperationException()
    {
        // Arrange
        Type invalidFlatteningWrapper = typeof(InvalidFlatteningWrapperWithoutInheritance);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => QueryBinderValidator.ValidateFlattenedExpressionType(invalidFlatteningWrapper));
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
        var exception = Assert.Throws<InvalidOperationException>(() => QueryBinderValidator.ValidateFlattenedExpressionType(invalidFlatteningWrapper));
        Assert.Equal(
            $"The type '{typeof(InvalidFlatteningWrapperWithoutIGroupByWrapperOfTInterface).FullName}' does not implement 'Microsoft.AspNetCore.OData.Query.Wrapper.IGroupByWrapper{{TContainer,TWrapper}}' interface.",
            exception.Message);
    }

    [Fact]
    public void ValidateFlattenedExpressionType_InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface_ThrowsInvalidOperationException()
    {
        // Arrange
        Type invalidFlatteningWrapper = typeof(InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => QueryBinderValidator.ValidateFlattenedExpressionType(invalidFlatteningWrapper));
        Assert.Equal(
            $"The type '{typeof(InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface).FullName}' does not implement 'Microsoft.AspNetCore.OData.Query.Wrapper.IFlatteningWrapper{{T}}' interface.",
            exception.Message);
    }

    [Fact]
    public void ValidateAggregationFlatteningResult_NullResult_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => QueryBinderValidator.ValidateFlatteningResult(null));
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
        QueryBinderValidator.ValidateFlatteningResult(validFlatteningResult);
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
        var exception = Assert.Throws<ArgumentException>(() => QueryBinderValidator.ValidateFlatteningResult(invalidFlatteningResult));
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
        var exception = Assert.Throws<ArgumentException>(() => QueryBinderValidator.ValidateFlatteningResult(invalidFlatteningResult));
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
        var exception = Assert.Throws<ArgumentException>(() => QueryBinderValidator.ValidateFlatteningResult(invalidFlatteningResult));
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
        var exception = Assert.Throws<ArgumentException>(() => QueryBinderValidator.ValidateFlatteningResult(invalidFlatteningResult));
        Assert.Equal(
            "The 'FlattenedPropertiesMapping' property must be set when the 'FlattenedExpression' property is set. (Parameter 'flatteningResult')",
            exception.Message);
    }

    [Fact]
    public void ValidateComputeExpressionType_DefaultComputeWrapper_DoesNotThrow()
    {
        // Arrange
        Type defaultComputeWrapper = typeof(ComputeWrapper<TestSale>);

        // Act & Assert
        QueryBinderValidator.ValidateComputeExpressionType(defaultComputeWrapper);
    }

    [Fact]
    public void ValidateComputeExpressionType_ValidComputeWrapper_DoesNotThrow()
    {
        // Arrange
        Type validComputeWrapper = typeof(ValidComputeWrapper<TestSale>);

        // Act & Assert
        QueryBinderValidator.ValidateComputeExpressionType(validComputeWrapper);
    }

    [Fact]
    public void ValidateComputeExpressionType_InvalidComputeWrapperWithoutInheritance_ThrowsInvalidOperationException()
    {
        // Arrange
        Type invalidComputeWrapper = typeof(InvalidComputeWrapperWithoutInheritance);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => QueryBinderValidator.ValidateComputeExpressionType(invalidComputeWrapper));
        Assert.Equal(
            $"The type '{typeof(InvalidComputeWrapperWithoutInheritance).FullName}' does not inherit from 'Microsoft.AspNetCore.OData.Query.Wrapper.DynamicTypeWrapper'.",
            exception.Message);
    }

    [Fact]
    public void ValidateComputeExpressionType_InvalidComputeWrapperWithoutIGroupByWrapperOfTInterface_ThrowsInvalidOperationException()
    {
        // Arrange
        Type invalidComputeWrapper = typeof(InvalidComputeWrapperWithoutIGroupByWrapperOfTInterface);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => QueryBinderValidator.ValidateComputeExpressionType(invalidComputeWrapper));
        Assert.Equal(
            $"The type '{typeof(InvalidComputeWrapperWithoutIGroupByWrapperOfTInterface).FullName}' does not implement 'Microsoft.AspNetCore.OData.Query.Wrapper.IGroupByWrapper{{TContainer,TWrapper}}' interface.",
            exception.Message);
    }

    [Fact]
    public void ValidateComputeExpressionType_InvalidComputeWrapperWithoutIComputeWrapperOfTInterface_ThrowsInvalidOperationException()
    {
        // Arrange
        Type invalidComputeWrapper = typeof(InvalidComputeWrapperWithoutIComputeWrapperOfTInterface);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => QueryBinderValidator.ValidateComputeExpressionType(invalidComputeWrapper));
        Assert.Equal(
            $"The type '{typeof(InvalidComputeWrapperWithoutIComputeWrapperOfTInterface).FullName}' does not implement 'Microsoft.AspNetCore.OData.Query.Wrapper.IComputeWrapper{{T}}' interface.",
            exception.Message);
    }
}

// Inherits from DynamicTypeWrapper and implements IGroupByWrapper<TContainer, TWrapper>
internal class ValidGroupByWrapper : DynamicTypeWrapper, IGroupByWrapper<AggregationPropertyContainerForValidGroupWrapper, ValidGroupByWrapper>
{
    public AggregationPropertyContainerForValidGroupWrapper GroupByContainer
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public AggregationPropertyContainerForValidGroupWrapper Container
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public override Dictionary<string, object> Values => throw new NotImplementedException();
}

// Does not inherit from DynamicTypeWrapper
internal class InvalidGroupByWrapperWithoutInheritance
    : IGroupByWrapper<AggregationPropertyContainerForInvalidGroupByWrapper, InvalidGroupByWrapperWithoutInheritance>
{
    public AggregationPropertyContainerForInvalidGroupByWrapper GroupByContainer
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public AggregationPropertyContainerForInvalidGroupByWrapper Container
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}

// Does not implement IGroupByWrapper<TContainer, TWrapper>
internal class InvalidGroupByWrapperWithoutIGroupByWrapperOfTInterface : DynamicTypeWrapper
{
    public override Dictionary<string, object> Values => throw new NotImplementedException();
}

// Inherits from DynamicTypeWrapper, implements IGroupByWrapper<TContainer, TWrapper>, and implements IFlatteningWrapper<T>
internal class ValidFlatteningWrapper<TestSale>
    : ValidGroupByWrapper, IGroupByWrapper<AggregationPropertyContainerForValidGroupWrapper, ValidGroupByWrapper>, IFlatteningWrapper<TestSale>
{
    public TestSale Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

// Does not inherit from DynamicTypeWrapper
internal class InvalidFlatteningWrapperWithoutInheritance
    : IGroupByWrapper<AggregationPropertyContainerForValidGroupWrapper, ValidGroupByWrapper>, IFlatteningWrapper<TestSale>
{
    public AggregationPropertyContainerForValidGroupWrapper GroupByContainer
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public AggregationPropertyContainerForValidGroupWrapper Container
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public TestSale Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

// Does not implement IGroupByWrapper<TContainer, TWrapper>
internal class InvalidFlatteningWrapperWithoutIGroupByWrapperOfTInterface : DynamicTypeWrapper, IFlatteningWrapper<TestSale>
{
    public override Dictionary<string, object> Values => throw new NotImplementedException();
    public TestSale Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

// Does not implement IFlatteningWrapper<T>
internal class InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface
    : ValidGroupByWrapper, IGroupByWrapper<AggregationPropertyContainerForValidGroupWrapper, ValidGroupByWrapper>
{
}

// Inherits from DynamicTypeWrapper, implements IGroupByWrapper<TContainer, TWrapper>, and implements IComputeWrapper<T>
internal class ValidComputeWrapper<TestSale>
    : ValidGroupByWrapper, IGroupByWrapper<AggregationPropertyContainerForValidGroupWrapper, ValidGroupByWrapper>, IComputeWrapper<TestSale>
{
    public TestSale Instance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IEdmModel Model { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

// Does not inherit from DynamicTypeWrapper
internal class InvalidComputeWrapperWithoutInheritance
    : IGroupByWrapper<AggregationPropertyContainerForValidGroupWrapper, ValidGroupByWrapper>, IComputeWrapper<TestSale>
{
    public AggregationPropertyContainerForValidGroupWrapper GroupByContainer
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public AggregationPropertyContainerForValidGroupWrapper Container
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public TestSale Instance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IEdmModel Model { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

// Does not implement IGroupByWrapper<TContainer, TWrapper>
internal class InvalidComputeWrapperWithoutIGroupByWrapperOfTInterface : DynamicTypeWrapper, IComputeWrapper<TestSale>
{
    public override Dictionary<string, object> Values => throw new NotImplementedException();
    public TestSale Instance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IEdmModel Model { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}

// Does not implement IComputeWrapper<T>
internal class InvalidComputeWrapperWithoutIComputeWrapperOfTInterface
    : ValidGroupByWrapper, IGroupByWrapper<AggregationPropertyContainerForValidGroupWrapper, ValidGroupByWrapper>
{
}

// Test aggregation property container for valid groupby wrapper
internal class AggregationPropertyContainerForValidGroupWrapper
    : IAggregationPropertyContainer<ValidGroupByWrapper, AggregationPropertyContainerForValidGroupWrapper>
{
    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public object Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ValidGroupByWrapper NestedValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IAggregationPropertyContainer<ValidGroupByWrapper, AggregationPropertyContainerForValidGroupWrapper> Next
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper, bool includeAutoSelected)
    {
        throw new NotImplementedException();
    }
}

// Test aggregation property container for invalid groupby wrapper
internal class AggregationPropertyContainerForInvalidGroupByWrapper
    : IAggregationPropertyContainer<InvalidGroupByWrapperWithoutInheritance, AggregationPropertyContainerForInvalidGroupByWrapper>
{
    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public object Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public InvalidGroupByWrapperWithoutInheritance NestedValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IAggregationPropertyContainer<InvalidGroupByWrapperWithoutInheritance, AggregationPropertyContainerForInvalidGroupByWrapper> Next
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    public void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper, bool includeAutoSelected)
    {
        throw new NotImplementedException();
    }
}

// Test model
internal class TestSale
{
    public int Id { get; set; }
}
