//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class ComputeQueryValidatorTests
{
    private ComputeQueryValidator _validator = new ComputeQueryValidator();
    private ODataQueryContext _context = ValidationTestHelper.CreateCustomerContext();

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsOnNullOption()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => _validator.Validate(null, new ODataValidationSettings()), "computeQueryOption");
    }

    [Fact]
    public void TryValidateComputeQueryValidator_ReturnsValidationErrorOnNullOption()
    {
        // Arrange & Act & Assert
        var result = _validator.TryValidate(null, new ODataValidationSettings(), out IEnumerable<ODataException> validationErrors);
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("Value cannot be null. (Parameter 'computeQueryOption')", validationErrors.First().Message);
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsOnNullSettings()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => _validator.Validate(new ComputeQueryOption("substring(Name, 0, 1) as FirstChar", _context), null), "validationSettings");
    }

    [Fact]
    public void TryValidateComputeQueryValidator_ReturnsValidationErrorOnNullSettings()
    {
        // Arrange & Act & Assert
        var result = _validator.TryValidate(new ComputeQueryOption("substring(Name, 0, 1) as FirstChar", _context), null, out IEnumerable<ODataException> validationErrors);
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("Value cannot be null. (Parameter 'validationSettings')", validationErrors.First().Message);
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsIfWithoutAsInComputeClause()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(() =>
            _validator.Validate(
                new ComputeQueryOption("test add p12m", _context),
                new ODataValidationSettings()),
            "'as' expected at position 13 in 'test add p12m'.");
    }

    [Fact]
    public void TryValidateComputeQueryValidator_ReturnsValidationErrorIfWithoutAsInComputeClause()
    {
        // Arrange & Act & Assert
        var result = _validator.TryValidate(
            new ComputeQueryOption("test add p12m", _context),
            new ODataValidationSettings(),
            out IEnumerable<ODataException> validationErrors);

        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("'as' expected at position 13 in 'test add p12m'.", validationErrors.First().Message);
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsIfUnknownPropertyInComputeClause()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(() =>
            _validator.Validate(
                new ComputeQueryOption("test add p12m as Any", _context),
                new ODataValidationSettings()),
            "Could not find a property named 'test' on type 'Microsoft.AspNetCore.OData.Tests.Query.Models.QueryCompositionCustomer'.");
    }

    [Fact]
    public void TryValidateComputeQueryValidator_ReturnsValidationErrorIfUnknownPropertyInComputeClause()
    {
        // Arrange & Act & Assert
        var result = _validator.TryValidate(
            new ComputeQueryOption("test add p12m as Any", _context),
            new ODataValidationSettings(),
            out IEnumerable<ODataException> validationErrors);
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("Could not find a property named 'test' on type 'Microsoft.AspNetCore.OData.Tests.Query.Models.QueryCompositionCustomer'.", validationErrors.First().Message);
    }

    [Fact]
    public void ValidateComputeQueryValidator_DoesNotThrow()
    {
        // Arrange & Act & Assert
        var result = _validator.TryValidate(
            new ComputeQueryOption("substring(Name, 0, 1) as FirstChar", _context),
            new ODataValidationSettings(), out IEnumerable<ODataException> validationErrors);
        Assert.True(result);
        Assert.Empty(validationErrors);
    }
}
