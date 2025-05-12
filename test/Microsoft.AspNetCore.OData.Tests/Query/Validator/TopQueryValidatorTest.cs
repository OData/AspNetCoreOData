//-----------------------------------------------------------------------------
// <copyright file="TopQueryValidatorTest.cs" company=".NET Foundation">
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
using Microsoft.OData.ModelBuilder.Config;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class TopQueryValidatorTest
{
    private TopQueryValidator _validator;
    private ODataQueryContext _context;

    public TopQueryValidatorTest()
    {
        _validator = new TopQueryValidator();
        _context = ValidationTestHelper.CreateCustomerContext();
    }

    [Fact]
    public void ValidateTopQueryValidator_ThrowsOnNullOption()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ArgumentNullException>(() => _validator.Validate(null, new ODataValidationSettings()));
    }

    [Fact]
    public void TryValidateTopQueryValidator_ReturnsFalseWithError_OnNullOption()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings();

        // Act
        var result = _validator.TryValidate(null, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("Value cannot be null. (Parameter 'topQueryOption')", errors.First().Message);
    }

    [Fact]
    public void ValidateTopQueryValidator_ThrowsOnNullSettings()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ArgumentNullException>(() => _validator.Validate(new TopQueryOption("2", _context), null));
    }

    [Fact]
    public void TryValidateTopQueryValidator_ReturnsFalseWithError_OnNullSettings()
    {
        // Arrange
        TopQueryOption option = new TopQueryOption("2", _context);

        // Act
        var result = _validator.TryValidate(option, null, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("Value cannot be null. (Parameter 'validationSettings')", errors.First().Message);
    }

    [Fact]
    public void ValidateTopQueryValidator_ThrowsWhenLimitIsExceeded()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings()
        {
            MaxTop = 10
        };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => _validator.Validate(new TopQueryOption("11", _context), settings),
            "The limit of '10' for Top query has been exceeded. The value from the incoming request is '11'.");
    }

    [Fact]
    public void TryValidateTopQueryValidator_ReturnsFalseWithError_WhenLimitIsExceeded()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings
        {
            MaxTop = 10
        };
        TopQueryOption option = new TopQueryOption("11", _context);

        // Act
        var result = _validator.TryValidate(option, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("The limit of '10' for Top query has been exceeded. The value from the incoming request is '11'.", errors.First().Message);
    }

    [Fact]
    public void ValidateTopQueryValidator_PassWhenLimitIsReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings()
        {
            MaxTop = 10
        };

        // Act
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(new TopQueryOption("10", _context), settings));
    }

    [Fact]
    public void TryValidateTopQueryValidator_ReturnsTrueWithNoError_WhenLimitIsReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings
        {
            MaxTop = 10
        };
        TopQueryOption option = new TopQueryOption("10", _context);

        // Act
        var result = _validator.TryValidate(option, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateTopQueryValidator_PassWhenLimitIsNotReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings()
        {
            MaxTop = 10
        };

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(new TopQueryOption("9", _context), settings));
    }

    [Fact]
    public void TryValidateTopQueryValidator_ReturnsTrueWithNoError_WhenLimitIsNotReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings
        {
            MaxTop = 10
        };
        TopQueryOption option = new TopQueryOption("9", _context);

        // Act
        var result = _validator.TryValidate(option, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateTopQueryValidator_PassWhenQuerySettingsLimitIsNotReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings()
        {
            MaxTop = 20
        };
        ModelBoundQuerySettings modelBoundQuerySettings = new ModelBoundQuerySettings();
        modelBoundQuerySettings.MaxTop = 20;
        ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
        context.Model.SetAnnotationValue(context.ElementType as IEdmStructuredType, modelBoundQuerySettings);

        // Act & Assert
        ExceptionAssert.DoesNotThrow(() => _validator.Validate(new TopQueryOption("20", context), settings));
    }

    [Fact]
    public void TryValidateTopQueryValidator_ReturnsTrueWithNoError_WhenQuerySettingsLimitIsNotReached()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings
        {
            MaxTop = 20
        };
        ModelBoundQuerySettings modelBoundQuerySettings = new ModelBoundQuerySettings
        {
            MaxTop = 20
        };
        ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
        context.Model.SetAnnotationValue(context.ElementType as IEdmStructuredType, modelBoundQuerySettings);
        TopQueryOption option = new TopQueryOption("20", context);

        // Act
        var result = _validator.TryValidate(option, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateTopQueryValidator_ThrowsWhenQuerySettingsLimitIsExceeded()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings()
        {
            MaxTop = 20
        };
        ModelBoundQuerySettings modelBoundQuerySettings = new ModelBoundQuerySettings();
        modelBoundQuerySettings.MaxTop = 10;
        ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
        context.Model.SetAnnotationValue(context.ElementType as IEdmStructuredType, modelBoundQuerySettings);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => _validator.Validate(new TopQueryOption("11", context), settings),
            "The limit of '10' for Top query has been exceeded. The value from the incoming request is '11'.");
    }

    [Fact]
    public void TryValidateTopQueryValidator_ReturnsFalseWithError_WhenQuerySettingsLimitIsExceeded()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings
        {
            MaxTop = 20
        };
        ModelBoundQuerySettings modelBoundQuerySettings = new ModelBoundQuerySettings
        {
            MaxTop = 10
        };
        ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
        context.Model.SetAnnotationValue(context.ElementType as IEdmStructuredType, modelBoundQuerySettings);
        TopQueryOption option = new TopQueryOption("11", context);

        // Act
        var result = _validator.TryValidate(option, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("The limit of '10' for Top query has been exceeded. The value from the incoming request is '11'.", errors.First().Message);
    }


    [Fact]
    public void GetTopQueryValidator_Returns_Validator()
    {
        // Arrange & Act & Assert
        ODataQueryContext context = null;
        Assert.NotNull(context.GetTopQueryValidator());

        // Arrange & Act & Assert
        context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        Assert.NotNull(context.GetTopQueryValidator());

        // Arrange & Act & Assert
        IServiceProvider services = new ServiceCollection()
            .AddSingleton<TopQueryValidator>().BuildServiceProvider();
        context.RequestContainer = services;
        Assert.NotNull(context.GetTopQueryValidator());
    }
}
