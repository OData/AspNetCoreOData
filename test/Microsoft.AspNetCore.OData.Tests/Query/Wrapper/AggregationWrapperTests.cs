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
        public void TestInvalidFlatteningWrapperWithNoIFlatteningOfTInterfaceReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(typeof(InvalidFlatteningWrapperWithoutIFlatteningWrapperOfTInterface).IsFlatteningWrapper());
        }
    }
}
