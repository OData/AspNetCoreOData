//-----------------------------------------------------------------------------
// <copyright file="DefaultQueryConfigurationsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class DefaultQueryConfigurationsTests
    {
        [Fact]
        public void MaxSelectExpandNodeCount_HasDefaultValue_Of500()
        {
            // Arrange & Act
            DefaultQueryConfigurations configurations = new DefaultQueryConfigurations();

            // Assert
            Assert.Equal(500, configurations.MaxSelectExpandNodeCount);
        }

        [Theory]
        [InlineData(0)]     // 0 disables the check
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(1000)]
        public void MaxSelectExpandNodeCount_AcceptsZeroAndPositiveValues(int value)
        {
            // Arrange
            DefaultQueryConfigurations configurations = new DefaultQueryConfigurations();

            // Act
            configurations.MaxSelectExpandNodeCount = value;

            // Assert
            Assert.Equal(value, configurations.MaxSelectExpandNodeCount);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void MaxSelectExpandNodeCount_Throws_ForNegativeValue(int value)
        {
            // Arrange
            DefaultQueryConfigurations configurations = new DefaultQueryConfigurations();

            // Act & Assert
            ArgumentOutOfRangeException exception = ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => configurations.MaxSelectExpandNodeCount = value);
            Assert.Contains("Value must be greater than or equal to 0", exception.Message);
        }
    }
}
