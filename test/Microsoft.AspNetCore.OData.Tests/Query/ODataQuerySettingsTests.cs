//-----------------------------------------------------------------------------
// <copyright file="ODataQuerySettingsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Query;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class ODataQuerySettingsTests
{
    [Fact]
    public void MatchesPatternTimeout_Default_IsOneSecond()
    {
        // Arrange & Act
        var settings = new ODataQuerySettings();

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(1), settings.MatchesPatternTimeout);
        Assert.Equal(ODataQuerySettings.DefaultMatchesPatternTimeout, settings.MatchesPatternTimeout);
    }

    [Fact]
    public void MatchesPatternTimeout_CanBeSetAndCleared()
    {
        // Arrange
        var settings = new ODataQuerySettings();

        // Act & Assert
        settings.MatchesPatternTimeout = TimeSpan.FromMilliseconds(100);
        Assert.Equal(TimeSpan.FromMilliseconds(100), settings.MatchesPatternTimeout);

        settings.MatchesPatternTimeout = null;
        Assert.Null(settings.MatchesPatternTimeout);
    }

    [Fact]
    public void MatchesPatternTimeout_SetToZero_AppliesNoLimit()
    {
        // Arrange
        var settings = new ODataQuerySettings();

        // Act - a non-positive span opts out of the bound, consistent with the millisecond companion.
        settings.MatchesPatternTimeout = TimeSpan.Zero;

        // Assert
        Assert.Null(settings.MatchesPatternTimeout);
    }

    [Fact]
    public void MatchesPatternTimeout_SetToNegative_AppliesNoLimit()
    {
        // Arrange
        var settings = new ODataQuerySettings();

        // Act - a negative span opts out of the bound, consistent with the millisecond companion.
        settings.MatchesPatternTimeout = TimeSpan.FromMilliseconds(-1);

        // Assert
        Assert.Null(settings.MatchesPatternTimeout);
    }

    [Fact]
    public void MatchesPatternTimeout_NonPositiveValue_OptsOutLikeMillisecondCompanion()
    {
        // Arrange & Act - a non-positive TimeSpan opts out exactly as the attribute's millisecond
        // companion does for 0 / negative values, so both surfaces share one contract.
        var zeroSpan = new ODataQuerySettings { MatchesPatternTimeout = TimeSpan.Zero };
        var negativeSpan = new ODataQuerySettings { MatchesPatternTimeout = TimeSpan.FromSeconds(-5) };
        var zeroMilliseconds = new EnableQueryAttribute { MatchesPatternTimeoutMilliseconds = 0 };
        var negativeMilliseconds = new EnableQueryAttribute { MatchesPatternTimeoutMilliseconds = -5 };

        // Assert - every non-positive assignment lands on the same "no limit" state.
        Assert.Null(zeroSpan.MatchesPatternTimeout);
        Assert.Null(negativeSpan.MatchesPatternTimeout);
        Assert.Null(zeroMilliseconds.MatchesPatternTimeout);
        Assert.Null(negativeMilliseconds.MatchesPatternTimeout);
    }

    [Fact]
    public void CopyFrom_CopiesMatchesPatternTimeout()
    {
        // Arrange
        var source = new ODataQuerySettings { MatchesPatternTimeout = TimeSpan.FromMilliseconds(100) };
        var target = new ODataQuerySettings();

        // Act
        target.CopyFrom(source);

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(100), target.MatchesPatternTimeout);

        // Arrange & Act - copying an explicit null source value clears the target.
        var clearedSource = new ODataQuerySettings { MatchesPatternTimeout = null };
        var populatedTarget = new ODataQuerySettings { MatchesPatternTimeout = TimeSpan.FromMilliseconds(100) };
        populatedTarget.CopyFrom(clearedSource);

        // Assert
        Assert.Null(populatedTarget.MatchesPatternTimeout);
    }
}
