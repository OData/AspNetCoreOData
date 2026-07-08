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
    public void MatchesPatternTimeout_SetToZero_Throws()
    {
        // Arrange
        var settings = new ODataQuerySettings();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.MatchesPatternTimeout = TimeSpan.Zero);
    }

    [Fact]
    public void MatchesPatternTimeout_SetToNegative_Throws()
    {
        // Arrange
        var settings = new ODataQuerySettings();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.MatchesPatternTimeout = TimeSpan.FromMilliseconds(-1));
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
