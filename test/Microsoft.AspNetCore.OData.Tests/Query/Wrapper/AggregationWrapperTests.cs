//-----------------------------------------------------------------------------
// <copyright file="AggregationWrapperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Query.Validator;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Wrapper
{
    public class AggregationWrapperTests
    {
        [Fact]
        public void TestDefaultGroupByWrapperReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.True(typeof(GroupByWrapper).IsGroupByWrapper());
        }

        [Fact]
        public void TestValidGroupByWrapperReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.True(typeof(ValidGroupByWrapper).IsGroupByWrapper());
        }

        [Fact]
        public void TestInvalidGroupByWrapperWithNoIGroupByWrapperOfTInterfaceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidGroupByWrapperWithoutIGroupByWrapperOfTInterface).IsGroupByWrapper());
        }

        [Fact]
        public void TestInvalidGroupByWrapperWithNoDynamicTypeWrapperInheritanceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidGroupByWrapperWithoutInheritance).IsGroupByWrapper());
        }

        [Fact]
        public void TestDefaultFlatteningWrapperReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.True(typeof(FlatteningWrapper<TestSale>).IsFlatteningWrapper());
        }

        [Fact]
        public void TestValidFlatteningWrapperReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.True(typeof(ValidFlatteningWrapper<TestSale>).IsFlatteningWrapper());
        }

        [Fact]
        public void TestInvalidFlatteningWrapperWithNoDynamicTypeWrapperInheritanceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidFlatteningWrapperWithoutInheritance).IsFlatteningWrapper());
        }

        [Fact]
        public void TestInvalidFlatteningWrapperWithNoIGroupByWrapperOfTInterfaceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidFlatteningWrapperWithoutIGroupByWrapperOfTInterface).IsFlatteningWrapper());
        }

        [Fact]
        public void TestInvalidFlatteningWrapperWithNoIFlatteningWrapperOfTInterfaceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface).IsFlatteningWrapper());
        }

        [Fact]
        public void TestDefaultComputeWrapperReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.True(typeof(ComputeWrapper<TestSale>).IsComputeWrapper(out _));
        }

        [Fact]
        public void TestValidComputeWrapperReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.True(typeof(ValidComputeWrapper<TestSale>).IsComputeWrapper(out _));
        }

        [Fact]
        public void TestInvalidComputeWrapperWithNoDynamicTypeWrapperInheritanceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidComputeWrapperWithoutInheritance).IsComputeWrapper(out _));
        }

        [Fact]
        public void TestInvalidComputeWrapperWithNoIGroupByWrapperOfTInterfaceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidComputeWrapperWithoutIGroupByWrapperOfTInterface).IsComputeWrapper(out _));
        }

        [Fact]
        public void TestInvalidComputeWrapperWithNoIComputeWrapperOfTInterfaceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidComputeWrapperWithoutIComputeWrapperOfTInterface).IsComputeWrapper(out _));
        }
    }
}
