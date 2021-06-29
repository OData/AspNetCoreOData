// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Deltas
{
    public class DeltaHelperTests
    {
        [Fact]
        public void IsDeltaOfT_Returns_BooleanAsExpected()
        {
            // Arrange & Act & Assert
            Assert.False(DeltaHelper.IsDeltaOfT(null));
            Assert.False(DeltaHelper.IsDeltaOfT(typeof(int)));
            Assert.False(DeltaHelper.IsDeltaOfT(typeof(DeltaSet<>)));
        }

        [Fact]
        public void IsDeltaResourceSet_Returns_BooleanAsExpected()
        {
            // Arrange & Act & Assert
            Assert.False(DeltaHelper.IsDeltaResourceSet(null));
            Assert.False(DeltaHelper.IsDeltaResourceSet(42));
            Assert.True(DeltaHelper.IsDeltaResourceSet(new DeltaSet<DeltaHelperTests>()));

            IEdmEntityType entityType = new Mock<IEdmEntityType>().Object;
            Assert.True(DeltaHelper.IsDeltaResourceSet(new EdmChangedObjectCollection(entityType)));
        }
    }
}