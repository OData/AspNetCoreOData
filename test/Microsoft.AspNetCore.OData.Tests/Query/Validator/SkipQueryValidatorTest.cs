//-----------------------------------------------------------------------------
// <copyright file="SkipQueryValidatorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class SkipQueryValidatorTest
{
    private SkipQueryValidator _validator;
    private ODataQueryContext _context;

    public SkipQueryValidatorTest()
    {
        _validator = new SkipQueryValidator();
        _context = ValidationTestHelper.CreateCustomerContext();
    }

    [Fact]
    public void ValidateSkipQueryValidator_ThrowsOnNullOption()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ArgumentNullException>(() => _validator.Validate(null, new ODataValidationSettings()));
    }

    [Fact]
    public void TryValidateSkipQueryValidator_ReturnsFalseWithError_OnNullOption()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings();

        // Act
        var result = _validator.TryValidate(null, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("Value cannot be null. (Parameter 'skipQueryOption')", errors.First().Message);
    }

    [Fact]
    public void ValidateSkipQueryValidator_ThrowsOnNullSettings()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ArgumentNullException>(() => _validator.Validate(new SkipQueryOption("2", _context), null));
    }

    [Fact]
    public void TryValidateSkipQueryValidator_ReturnsFalseWithError_OnNullSettings()
    {
        // Arrange
        SkipQueryOption option = new SkipQueryOption("2", _context);

        // Act
        var result = _validator.TryValidate(option, null, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("Value cannot be null. (Parameter 'validationSettings')", errors.First().Message);
    }

    [Fact]
    public void ValidateSkipQueryValidator_ThrowsWhenLimitIsExceeded()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings()
        {
            MaxSkip = 10
        };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => _validator.Validate(new SkipQueryOption("11", _context), settings),
            "The limit of '10' for Skip query has been exceeded. The value from the incoming request is '11'.");
    }

    [Fact]
    public void TryValidateSkipQueryValidator_ReturnsFalseWithError_WhenLimitIsExceeded()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings
        {
            MaxSkip = 10
        };
        SkipQueryOption option = new SkipQueryOption("11", _context);

        // Act
        var result = _validator.TryValidate(option, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("The limit of '10' for Skip query has been exceeded. The value from the incoming request is '11'.", errors.First().Message);
    }

    [Fact]
    public void ValidateSkipQueryValidator_PassWhenLimitIsReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings()
        {
            MaxSkip = 10
        };

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(new SkipQueryOption("10", _context), settings));
    }

    [Fact]
    public void TryValidateSkipQueryValidator_ReturnsTrueWithNoError_WhenLimitIsReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings
        {
            MaxSkip = 10
        };
        SkipQueryOption option = new SkipQueryOption("10", _context);

        // Act
        var result = _validator.TryValidate(option, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateSkipQueryValidator_PassWhenLimitIsNotReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings()
        {
            MaxSkip = 10
        };

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(new SkipQueryOption("9", _context), settings));
    }

    [Fact]
    public void TryValidateSkipQueryValidator_ReturnsTrueWithNoError_WhenLimitIsNotReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings
        {
            MaxSkip = 10
        };
        SkipQueryOption option = new SkipQueryOption("9", _context);

        // Act
        var result = _validator.TryValidate(option, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void GetSkipQueryValidator_Returns_Validator()
    {
        // Arrange & Act & Assert
        ODataQueryContext context = null;
        Assert.NotNull(context.GetSkipQueryValidator());

        // Arrange & Act & Assert
        context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        Assert.NotNull(context.GetSkipQueryValidator());

        // Arrange & Act & Assert
        IServiceProvider services = new ServiceCollection()
            .AddSingleton<SkipQueryValidator>().BuildServiceProvider();
        context.RequestContainer = services;
        Assert.NotNull(context.GetSkipQueryValidator());
    }
}
