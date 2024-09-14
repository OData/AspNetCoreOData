//-----------------------------------------------------------------------------
// <copyright file="TruncatedCollectionOfTTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Tests.Commons;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Container;

public class TruncatedCollectionTest
{
    [Fact]
    public void Ctor_ThrowsArgumentNull_Collection_Enumerable_Source()
    {
        ExceptionAssert.ThrowsArgumentNull(() => new TruncatedCollection<int>(source: null, pageSize: 10), "source");
    }

    [Fact]
    public void Ctor_ThrowsArgumentNull_Collection_Queryable_Source()
    {
        ExceptionAssert.ThrowsArgumentNull(() => new TruncatedCollection<int>(source: null, pageSize: 10, parameterize: false), "source");
    }

    [Fact]
    public void Ctor_ThrowsArgumentGreater_Collection()
    {
        ExceptionAssert.ThrowsArgumentGreaterThanOrEqualTo(
            () => new TruncatedCollection<int>(source: new int[0], pageSize: 0), "pageSize", "1", "0");
    }

    [Fact]
    public void CtorTruncatedCollection_SetsProperties()
    {
        // Arrange & Act
        IEnumerable<int> source = new[] { 1, 2, 3, 5, 7 };
        TruncatedCollection<int> collection = new TruncatedCollection<int>(source, 3, 5);

        // Arrange
        Assert.Equal(3, collection.PageSize);
        Assert.Equal(5, collection.TotalCount);
        Assert.True(collection.IsTruncated);
        Assert.Equal(3, collection.Count);
        Assert.Equal(new[] { 1, 2, 3 }, collection);
    }

    [Fact]
    [Obsolete]
    public void CtorTruncatedCollection_WithQueryable_SetsProperties()
    {
        // Arrange & Act
        var source = new[] { 1, 2, 3, 5, 7 }.AsQueryable();
        TruncatedCollection<int> collection = new TruncatedCollection<int>(source, 3, 5);

        // Arrange
        Assert.Equal(3, collection.PageSize);
        Assert.Equal(5, collection.TotalCount);
        Assert.True(collection.IsTruncated);
        Assert.Equal(3, collection.Count);
        Assert.Equal(new[] { 1, 2, 3 }, collection);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, false)]
    [InlineData(10, false)]
    public void Property_IsTruncated(int pageSize, bool expectedResult)
    {
        TruncatedCollection<int> collection = new TruncatedCollection<int>(new[] { 1, 2, 3 }, pageSize);
        Assert.Equal(expectedResult, collection.IsTruncated);
    }

    [Fact]
    public void Property_PageSize()
    {
        int pageSize = 42;
        TruncatedCollection<int> collection = new TruncatedCollection<int>(new[] { 1, 2, 3 }, pageSize);
        Assert.Equal(pageSize, collection.PageSize);
    }

    [Fact]
    public void GetEnumerator_Truncates_IfPageSizeIsLessThanCollectionSize()
    {
        TruncatedCollection<int> collection = new TruncatedCollection<int>(new[] { 1, 2, 3 }, pageSize: 2);

        Assert.Equal(new[] { 1, 2 }, collection);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(42)]
    public void GetEnumerator_DoesNotTruncate_IfPageSizeIsGreaterThanOrEqualToCollectionSize(int pageSize)
    {
        TruncatedCollection<int> collection = new TruncatedCollection<int>(new[] { 1, 2, 3 }, pageSize);

        Assert.Equal(new[] { 1, 2, 3 }, collection);
    }
}
