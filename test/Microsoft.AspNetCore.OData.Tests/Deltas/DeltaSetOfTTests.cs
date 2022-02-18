//-----------------------------------------------------------------------------
// <copyright file="DeltaSetOfTTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Deltas
{
    public class DeltaSetOfTTests
    {
        [Fact]
        public void DeltaSet_Returns_StructuredType()
        {
            // Arrange
            DeltaSet<DeltaSetOfTTests> set = new DeltaSet<DeltaSetOfTTests>();

            // Act & Assert
            Assert.Equal(typeof(DeltaSetOfTTests), set.StructuredType);
        }

        [Fact]
        public void DeltaSet_Returns_ExpectedClrType()
        {
            // Arrange
            DeltaSet<DeltaSetOfTTests> set = new DeltaSet<DeltaSetOfTTests>();

            // Act & Assert
            Assert.Equal(typeof(DeltaSetOfTTests), set.ExpectedClrType);
        }

        //[Fact]
        //public void PatchOnDeltaSet_ThrowsArgumentNull_OriginalSet()
        //{
        //    // Arrange
        //    DeltaSet<DeltaSetOfTTests> set = new DeltaSet<DeltaSetOfTTests>();

        //    // Act & Assert
        //    ExceptionAssert.ThrowsArgumentNull(() => set.Patch(null), "originalSet");
        //}
    }
}
