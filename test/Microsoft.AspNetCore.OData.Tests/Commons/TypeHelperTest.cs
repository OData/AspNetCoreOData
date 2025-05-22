//-----------------------------------------------------------------------------
// <copyright file="TypeHelperTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Commons;

public class TypeHelperTest
{
    [Theory]
    [InlineData(typeof(AggregationWrapper), true)]
    [InlineData(typeof(ComputeWrapper<object>), true)]
    [InlineData(typeof(EntitySetAggregationWrapper), true)]
    [InlineData(typeof(FlatteningWrapper<object>), true)]
    [InlineData(typeof(GroupByWrapper), true)]
    [InlineData(typeof(NoGroupByAggregationWrapper), true)]
    [InlineData(typeof(NoGroupByWrapper), true)]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(SelectExpandWrapper), false)]
    [InlineData(null, false)]
    public void IsDynamicTypeWrapper_with_NonCollections(Type type, bool expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, TypeHelper.IsDynamicTypeWrapper(type));
    }

    [Fact]
    public void IsCollection_ThrowsArgumentNull_ClrType()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => TypeHelper.IsCollection(null), "clrType");
    }

    /// <summary>
    /// Collection types to test.
    /// </summary>
    public static TheoryDataSet<Type, Type> CollectionTypesData
    {
        get
        {
            return new TheoryDataSet<Type, Type>
            {
                { typeof(ICollection<string>), typeof(string) },
                { typeof(IList<string>), typeof(string) },
                { typeof(List<int>), typeof(int) },
                { typeof(CustomBoolCollection), typeof(bool) },
                { typeof(IEnumerable<int>), typeof(int) },
                { typeof(int[]), typeof(int) },
                { typeof(CustomIntCollection), typeof(int) },
            };
        }
    }

    [Theory]
    [MemberData(nameof(CollectionTypesData))]
    public void IsCollection_with_Collections(Type collectionType, Type elementType)
    {
        // Arrange & Act & Assert
        Type type;
        Assert.True(TypeHelper.IsCollection(collectionType, out type));
        Assert.Equal(elementType, type);
        Assert.True(TypeHelper.IsCollection(collectionType));
    }

    [Theory]
    [InlineData(typeof(IDictionary), true)]
    [InlineData(typeof(EdmUntypedObject), true)]
    [InlineData(typeof(IDictionary<string, object>), true)]
    [InlineData(typeof(Dictionary<object, object>), true)]
    [InlineData(typeof(IList<object>), false)]
    public void IsDictionary_with_Dictionary(Type clrType, bool expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, TypeHelper.IsDictionary(clrType));
    }

    [Theory]
    [MemberData(nameof(CollectionTypesData))]
    public void GetInnerElementType(Type collectionType, Type elementType)
    {
        // Arrange & Act & Assert
        Assert.Equal(elementType, TypeHelper.GetInnerElementType(collectionType));
    }

    [Theory]
    [InlineData(typeof(object))]
    [InlineData(typeof(ICollection))]
    [InlineData(typeof(IEnumerable))]
    [InlineData(typeof(string))]
    public void IsCollection_with_NonCollections(Type type)
    {
        // Arrange & Act & Assert
        Assert.False(TypeHelper.IsCollection(type));
    }

    [Theory]
    [InlineData(typeof(int), typeof(int?))]
    [InlineData(typeof(string), typeof(string))]
    [InlineData(typeof(DateTime), typeof(DateTime?))]
    [InlineData(typeof(int?), typeof(int?))]
    [InlineData(typeof(IEnumerable), typeof(IEnumerable))]
    [InlineData(typeof(int[]), typeof(int[]))]
    [InlineData(typeof(string[]), typeof(string[]))]
    public void ToNullable_Returns_ExpectedValue(Type type, Type expectedResult)
    {
        // Arrange & Act & Assert
        Assert.Equal(expectedResult, TypeHelper.ToNullable(type));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(int?), true)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(bool?), true)]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(CustomBoolCollection), true)]
    public void IsNullable(Type type, bool expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, TypeHelper.IsNullable(type));
    }

    [Theory]
    [InlineData(typeof(object), typeof(object), true)]
    [InlineData(typeof(int), typeof(object), false)]
    [InlineData(typeof(long), typeof(int), false)]
    [InlineData(typeof(string), typeof(object), false)]
    [InlineData(typeof(object), typeof(string), true)]
    [InlineData(typeof(CustomBoolCollection), typeof(List<bool>), false)]
    [InlineData(typeof(CustomBoolCollection), typeof(List<int>), false)]
    [InlineData(typeof(CustomAbstractClass), typeof(CustomConcreteClass), true)]
    public void IsTypeAssignableFrom(Type type,Type fromType, bool expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, TypeHelper.IsTypeAssignableFrom(type, fromType));
    }

    [Theory]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(ICollection), false)]
    [InlineData(typeof(MemberTypes), true)]
    [InlineData(typeof(string), false)]
    public void IsEnum(Type type, bool expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, TypeHelper.IsEnum(type));
    }

    [Theory]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(ICollection), false)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(string), false)]
    public void IsDateTime(Type type, bool expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, TypeHelper.IsDateTime(type));
    }

    [Theory]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(ICollection), false)]
    [InlineData(typeof(TimeSpan), true)]
    [InlineData(typeof(string), false)]
    public void IsTimeSpan(Type type, bool expected)
    {
        // Arrange & Act & Assert
        Assert.Equal(expected, TypeHelper.IsTimeSpan(type));
    }

    [Theory]
    [InlineData(typeof(object), null)]
    [InlineData(typeof(int), null)]
    [InlineData(typeof(int[]), typeof(int))]
    [InlineData(typeof(CustomBoolCollection), typeof(bool))]
    [InlineData(typeof(IQueryable<int>), typeof(int))]
    [InlineData(typeof(IEnumerable<bool>), typeof(bool))]
    [InlineData(typeof(string), typeof(char))]
    [InlineData(typeof(Task<int>), null)]
    [InlineData(typeof(Task<string>), typeof(char))]
    [InlineData(typeof(Task<IEnumerable<bool>>), typeof(bool))]
    [InlineData(typeof(IEnumerable<IEnumerable<bool>>), typeof(IEnumerable<bool>))]
    public void GetImplementedIEnumerableType(Type collectionType, Type elementType)
    {
        // Arrange & Act & Assert
        Assert.Equal(elementType, TypeHelper.GetImplementedIEnumerableType(collectionType));
    }

    [Fact]
    public void GetLoadedTypes_ReturnsEmpty_IfNullAssemblyResolver()
    {
        // Arrange & Act & Assert
        Assert.Empty(TypeHelper.GetLoadedTypes(null));
    }

    [Fact]
    public void GetLoadedTypes_ReturnsAsExpected()
    {
        // Arrange
        MockType baseType = new MockType("BaseType").Property(typeof(int), "ID");

        MockType derivedType = new MockType("DerivedType").Property(typeof(int), "DerivedTypeId").BaseType(baseType);

        MockAssembly assembly = new MockAssembly(baseType, derivedType);
        IAssemblyResolver resolver = MockAssembliesResolverFactory.Create(assembly);
        IEnumerable<Type> foundTypes = TypeHelper.GetLoadedTypes(resolver);

        IEnumerable<string> definedNames = assembly.GetTypes().Select(t => t.FullName);
        IEnumerable<string> foundNames = foundTypes.Select(t => t.FullName);

        foreach (string name in definedNames)
        {
            Assert.Contains(name, foundNames);
        }

        Assert.DoesNotContain(typeof(TypeHelperTest), foundTypes);
    }

    [Theory]
    [InlineData(typeof(IAsyncEnumerable<int>))]
    [InlineData(typeof(IAsyncEnumerable<List<bool>>))]
    [InlineData(typeof(CustomGenericAsyncEnumerable<int>))]
    [InlineData(typeof(CustomGenericAsyncEnumerable<CustomInternalClass>))]
    [InlineData(typeof(CustomAsyncEnumerable))]
    public void IsAsyncEnumerableType_ReturnsTrue_ForIAsyncEnumerable(Type type)
    {
        // Arrange & Act
        var result = TypeHelper.IsAsyncEnumerableType(type);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(typeof(List<int>))]
    [InlineData(typeof(IList<int>))]
    [InlineData(typeof(ICollection<int>))]
    [InlineData(typeof(IEnumerable<int>))]
    [InlineData(typeof(IQueryable<int>))]
    [InlineData(typeof(IAsyncEnumerator<int>))]
    [InlineData(typeof(IEnumerable<CustomAsyncEnumerable>))]
    [InlineData(typeof(IEnumerable<CustomInternalClass>))]
    [InlineData(typeof(IEnumerable<CustomGenericAsyncEnumerable<CustomInternalClass>>))]
    [InlineData(typeof(IEnumerable<CustomGenericAsyncEnumerable<int>>))]
    public void IsAsyncEnumerableType_ReturnsFalse_ForNonAsyncEnumerableType(Type type)
    {
        // Arrange & Act
        var result = TypeHelper.IsAsyncEnumerableType(type);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAsyncEnumerableType_ReturnsTrue_ForGenericAsyncEnumerable()
    {
        // Arrange
        var asyncNumbers = ToAsyncEnumerable(Enumerable.Range(1, 10));
        var asyncEnumerable = GenericAsyncEnumerableWithDelay(asyncNumbers, TimeSpan.FromSeconds(1));

        // Act
        var result = TypeHelper.IsAsyncEnumerableType(asyncEnumerable.GetType());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAsyncEnumerableType_ReturnsTrue_ForTypedAsyncEnumerable()
    {
        // Arrange
        var asyncNumbers = ToAsyncEnumerable(Enumerable.Range(1, 5).Select(i => new CustomInternalClass()));
        var asyncEnumerable = TypedAsyncEnumerableWithDelay(asyncNumbers, TimeSpan.FromSeconds(1));

        // Act
        var result = TypeHelper.IsAsyncEnumerableType(asyncEnumerable.GetType());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAsyncEnumerableType_ThrowsArgumentNullException_ForNullType()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => TypeHelper.IsAsyncEnumerableType(null));
    }

    /// <summary>
    /// Custom internal class
    /// </summary>
    internal class CustomInternalClass
    {
    }

    /// <summary>
    /// Custom collection of bool
    /// </summary>
    private sealed class CustomBoolCollection : List<bool>
    {
    }

    /// <summary>
    /// Custom collection of int
    /// </summary>
    private class CustomIntCollection : List<int>
    {
    }

    /// <summary>
    /// Custom abstract class
    /// </summary>
    private abstract class CustomAbstractClass
    {
        public abstract int Area();
    }

    /// <summary>
    /// Custom abstract class
    /// </summary>
    private class CustomConcreteClass : CustomAbstractClass
    {
        public override int Area() { return 42; }
    }

    public IAsyncEnumerator<int> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async IAsyncEnumerable<TEntity> GenericAsyncEnumerableWithDelay<TEntity>(IAsyncEnumerable<TEntity> asyncEnumerable, TimeSpan delay)
    {
        await foreach (var entity in asyncEnumerable)
        {
            await Task.Delay(delay);
            yield return entity;
        }
    }

    private async IAsyncEnumerable<CustomInternalClass> TypedAsyncEnumerableWithDelay(IAsyncEnumerable<CustomInternalClass> asyncEnumerable, TimeSpan delay)
    {
        await foreach (var entity in asyncEnumerable)
        {
            await Task.Delay(delay);
            yield return entity;
        }
    }

    private class CustomGenericAsyncEnumerable<TEntity> : IAsyncEnumerable<TEntity>
    {
        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private class CustomAsyncEnumerable : IAsyncEnumerable<List<int>>
    {
        public IAsyncEnumerator<List<int>> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield();
        }
    }

}
