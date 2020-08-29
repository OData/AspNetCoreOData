// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class ClrTypeCacheTest
    {
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(int?))]
        public void GetEdmType_Returns_CachedInstance(Type clrType)
        {
            // Arrange
            ClrTypeCache cache = new ClrTypeCache();
            IEdmModel model = EdmCoreModel.Instance;

            // Act
            IEdmTypeReference edmType1 = cache.GetEdmType(clrType, model);
            IEdmTypeReference edmType2 = cache.GetEdmType(clrType, model);

            // Assert
            Assert.NotNull(edmType1);
            Assert.Same(edmType1, edmType2);
        }
    }
}
