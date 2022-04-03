//-----------------------------------------------------------------------------
// <copyright file="TypeCacheItemTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class TypeCacheItemTests
    {
        #region TryFindEdmType
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(TypeCacheItemTests))]
        public void AddAndFindEdmType_Returns_CachedInstance(Type testType)
        {
            // Arrange
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            TypeCacheItem cache = new TypeCacheItem();

            // Act
            bool found = cache.TryFindEdmType(testType, out _);
            Assert.False(found); // not found

            cache.AddClrToEdmMap(testType, edmType);
            found = cache.TryFindEdmType(testType, out IEdmTypeReference acutal);

            // Assert
            Assert.True(found);
            Assert.Same(edmType, acutal);
        }

        [Fact]
        public void AddClrToEdmMap_Cached_OnlyOneInstance()
        {
            // Arrange
            TypeCacheItem cache = new TypeCacheItem();
            Action cacheCallAndVerify = () =>
            {
                IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
                cache.AddClrToEdmMap(typeof(TypeCacheItemTests), edmType);
                Assert.Single(cache.ClrToEdmTypeCache);
            };

            // Act & Assert
            cacheCallAndVerify();

            // 5 is a magic number, it doesn't matter, just want to call it multiple times.
            for (int i = 0; i < 5; i++)
            {
                cacheCallAndVerify();
            }

            cacheCallAndVerify();
        }

        #endregion

        #region TryFindClrType

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(TypeCacheItemTests))]
        public void AddAndGetClrType_Returns_CorrectType(Type testType)
        {
            // Arrange
            IEdmType edmType = new Mock<IEdmType>().Object;
            TypeCacheItem cache = new TypeCacheItem();

            // Act
            bool found = cache.TryFindClrType(edmType, true, out _);
            Assert.False(found);

            cache.AddEdmToClrMap(edmType, true, testType);

            found = cache.TryFindClrType(edmType, true, out Type actualType);

            // Act & Assert
            Assert.True(found);
            Assert.Same(testType, actualType);
        }

        [Fact]
        public void AddEdmToClrMap_Cached_OnlyOneInstance()
        {
            // Arrange
            TypeCacheItem cache = new TypeCacheItem();
            IEdmType edmType = new Mock<IEdmType>().Object;

            Action cacheCallAndVerify = () =>
            {
                cache.AddEdmToClrMap(edmType, true, typeof(int?));
                cache.AddEdmToClrMap(edmType, false, typeof(int));

                KeyValuePair<IEdmType, (Type, Type)> item = Assert.Single(cache.EdmToClrTypeCache);
                Assert.Same(edmType, item.Key);
                Assert.Equal(typeof(int), item.Value.Item1);
                Assert.Equal(typeof(int?), item.Value.Item2);
            };

            // Act & Assert
            cacheCallAndVerify();

            // 5 is a magic number, it doesn't matter, just want to call it multiple times.
            for (int i = 0; i < 5; i++)
            {
                cacheCallAndVerify();
            }

            cacheCallAndVerify();
        }
        #endregion
    }
}
