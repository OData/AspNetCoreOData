//-----------------------------------------------------------------------------
// <copyright file="TruncatedCollectionOfTTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Tests.Commons;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Container
{
    public class TruncatedCollectionTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Collection_Queryable_Source()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new TruncatedCollection<int>(source: null, pageSize: 10, parameterize: false), "source");
        }

        [Fact]
        public void CtorTruncatedCollection_SetsProperties()
        {
            // Arrange & Act
            IEnumerable<int> source = new[] { 1, 2, 3, 5, 7 };
            TruncatedCollection<int> collection = new TruncatedCollection<int>(source, 3, 5);

            // Ensure that the list is enumerated to make all lazy properties available.
            var x = collection.ToList();
            // Arrange
            Assert.Equal(3, collection.PageSize);
            Assert.Equal(5, collection.TotalCount);
            Assert.True(collection.IsTruncated);
            Assert.Equal(3, collection.Count());
            Assert.Equal(new[] { 1, 2, 3 }, collection);
        }

    }
}
