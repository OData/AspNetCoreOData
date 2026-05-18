//-----------------------------------------------------------------------------
// <copyright file="ODataMiniOptionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests;

public class ODataMiniOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        ODataMiniOptions options = new ODataMiniOptions();

        // Assert
        Assert.True(options.EnableNoDollarQueryOptions);
        Assert.True(options.EnableCaseInsensitive);
        Assert.False(options.EnableContinueOnErrorHeader);
        Assert.Equal(TimeZoneInfo.Local, options.TimeZone);
        Assert.Equal(ODataVersionConstraint.DefaultODataVersion, options.Version);
        Assert.Equal(ODataBatchHandler.DefaultMaxReceivedMessageSize, options.MaxReceivedMessageSize);
    }

    [Fact]
    public void SetMaxReceivedMessageSize_SetsValue()
    {
        // Arrange
        ODataMiniOptions options = new ODataMiniOptions();
        long customSize = 50 * 1024 * 1024;

        // Act
        options.SetMaxReceivedMessageSize(customSize);

        // Assert
        Assert.Equal(customSize, options.MaxReceivedMessageSize);
    }

    [Fact]
    public void SetMaxReceivedMessageSize_ReturnsSelf()
    {
        // Arrange
        ODataMiniOptions options = new ODataMiniOptions();

        // Act
        ODataMiniOptions result = options.SetMaxReceivedMessageSize(50 * 1024 * 1024);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void SetMaxReceivedMessageSize_Throws_ForZero()
    {
        // Arrange
        ODataMiniOptions options = new ODataMiniOptions();

        // Act & Assert
        ArgumentOutOfRangeException exception = ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => options.SetMaxReceivedMessageSize(0));
        Assert.Contains("Value must be greater than or equal to 1", exception.Message);
    }

    [Fact]
    public void SetMaxReceivedMessageSize_Throws_ForNegative()
    {
        // Arrange
        ODataMiniOptions options = new ODataMiniOptions();

        // Act & Assert
        ArgumentOutOfRangeException exception = ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => options.SetMaxReceivedMessageSize(-1));
        Assert.Contains("Value must be greater than or equal to 1", exception.Message);
    }

    [Fact]
    public void SetMaxReceivedMessageSize_AcceptsMinimumValue()
    {
        // Arrange
        ODataMiniOptions options = new ODataMiniOptions();

        // Act
        options.SetMaxReceivedMessageSize(1);

        // Assert
        Assert.Equal(1, options.MaxReceivedMessageSize);
    }

    [Fact]
    public void SetContinueOnErrorHeader_SetsCorrectField()
    {
        // Arrange
        ODataMiniOptions options = new ODataMiniOptions();
        Assert.False(options.EnableContinueOnErrorHeader); // Guard
        Assert.True(options.EnableCaseInsensitive); // Guard

        // Act
        options.SetContinueOnErrorHeader(true);

        // Assert
        Assert.True(options.EnableContinueOnErrorHeader);
        Assert.True(options.EnableCaseInsensitive); // Must NOT be affected
    }

    [Fact]
    public void UpdateFrom_CopiesAllFields()
    {
        // Arrange
        ODataMiniOptions source = new ODataMiniOptions();
        source.SetNoDollarQueryOptions(false);
        source.SetCaseInsensitive(false);
        source.SetContinueOnErrorHeader(true);
        source.SetTimeZoneInfo(TimeZoneInfo.Utc);
        source.SetVersion(ODataVersion.V401);
        source.SetMaxReceivedMessageSize(42 * 1024 * 1024);
        source.SetMaxTop(100);

        ODataMiniOptions target = new ODataMiniOptions();

        // Act
        target.UpdateFrom(source);

        // Assert
        Assert.False(target.EnableNoDollarQueryOptions);
        Assert.False(target.EnableCaseInsensitive);
        Assert.True(target.EnableContinueOnErrorHeader);
        Assert.Equal(TimeZoneInfo.Utc, target.TimeZone);
        Assert.Equal(ODataVersion.V401, target.Version);
        Assert.Equal(42 * 1024 * 1024, target.MaxReceivedMessageSize);
        Assert.Equal(100, target.QueryConfigurations.MaxTop.Value);
    }
}
