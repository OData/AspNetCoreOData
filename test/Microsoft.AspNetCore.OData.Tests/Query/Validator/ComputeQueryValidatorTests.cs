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
        var result = _validator.TryValidate(null, new ODataValidationSettings(), out IEnumerable<string> validationErrors);
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("Value cannot be null. (Parameter 'computeQueryOption')", validationErrors.First());
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
        var result = _validator.TryValidate(new ComputeQueryOption("substring(Name, 0, 1) as FirstChar", _context), null, out IEnumerable<string> validationErrors);
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("Value cannot be null. (Parameter 'validationSettings')", validationErrors.First());
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
            out IEnumerable<string> validationErrors);

        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("'as' expected at position 13 in 'test add p12m'.", validationErrors.First());
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

    [Theory]
    [InlineData("NotFilterableProperty eq 'x' as Flag")]
    [InlineData("length(NotFilterableProperty) as Length")]
    [InlineData("Name eq 'a' or NotFilterableProperty eq 'b' as Flag")]
    [InlineData("Address/NotFilterableProperty eq 'x' as Flag")]
    [InlineData("Contacts/all(c: c/NotFilterableProperty ne null) as AllRestricted")]
    [InlineData("Contacts/any(c: c/NotFilterableProperty ne null) as HasRestricted")]
    public void ValidateComputeQueryValidator_ThrowsForRestrictedPropertyInComputeClause(string compute)
    {
        // Arrange & Act & Assert - a not-filterable property referenced through $compute is rejected
        // the same way it is through $filter, regardless of the global enable switches.
        ExceptionAssert.Throws<ODataException>(() =>
            _validator.Validate(
                new ComputeQueryOption(compute, _context),
                new ODataValidationSettings()),
            "The property 'NotFilterableProperty' cannot be used in the $filter query option.");
    }

    [Theory]
    [InlineData("Name eq 'a' as Flag")]
    [InlineData("AmountSpent mul 2 as Double")]
    [InlineData("length(Name) as Length")]
    [InlineData("Address/City eq 'a' as Flag")]
    [InlineData("Contacts/any(c: c/Name ne null) as HasNamed")]
    public void ValidateComputeQueryValidator_DoesNotThrowForUnrestrictedPropertyInComputeClause(string compute)
    {
        // Arrange & Act & Assert - an unrestricted property keeps validating even though the context
        // leaves EnableFilter/EnableSelect at their framework defaults (false).
        ExceptionAssert.DoesNotThrow(() =>
            _validator.Validate(
                new ComputeQueryOption(compute, _context),
                new ODataValidationSettings()));
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsForDisallowedFunction()
    {
        // Arrange - length(...) is used but AllowedFunctions excludes Length. The referenced property
        // (Name) is unrestricted, so only the function allow-list can cause the rejection. This closes
        // the $compute=<disallowed-function> bypass.
        var settings = new ODataValidationSettings { AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.Length };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(new ComputeQueryOption("length(Name) as L", _context), settings),
            "Function 'length' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsForDisallowedArithmeticOperator()
    {
        // Arrange - 'mul' is used but AllowedArithmeticOperators excludes Multiply.
        var settings = new ODataValidationSettings { AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~AllowedArithmeticOperators.Multiply };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(new ComputeQueryOption("AmountSpent mul 2 as D", _context), settings),
            "Arithmetic operator 'Multiply' is not allowed. To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsForDisallowedLogicalOperator()
    {
        // Arrange - 'eq' is used but AllowedLogicalOperators excludes Equal.
        var settings = new ODataValidationSettings { AllowedLogicalOperators = AllowedLogicalOperators.All & ~AllowedLogicalOperators.Equal };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(new ComputeQueryOption("Name eq 'x' as Flag", _context), settings),
            "Logical operator 'Equal' is not allowed. To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsWhenNodeCountExceeded()
    {
        // Arrange - a small MaxNodeCount is exceeded by the compute expression tree.
        var settings = new ODataValidationSettings { MaxNodeCount = 1 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(new ComputeQueryOption("AmountSpent mul 2 as D", _context), settings),
            "The node count limit of '1' has been exceeded. To increase the limit, set the 'MaxNodeCount' property on EnableQueryAttribute or ODataValidationSettings.");
    }

    [Fact]
    public void TryValidateComputeQueryValidator_ReturnsValidationErrorIfUnknownPropertyInComputeClause()
    {
        // Arrange & Act & Assert
        var result = _validator.TryValidate(
            new ComputeQueryOption("test add p12m as Any", _context),
            new ODataValidationSettings(),
            out IEnumerable<string> validationErrors);
        Assert.False(result);
        Assert.Single(validationErrors);
        Assert.Equal("Could not find a property named 'test' on type 'Microsoft.AspNetCore.OData.Tests.Query.Models.QueryCompositionCustomer'.", validationErrors.First());
    }

    [Fact]
    public void ValidateComputeQueryValidator_DoesNotThrow()
    {
        // Arrange & Act & Assert
        var result = _validator.TryValidate(
            new ComputeQueryOption("substring(Name, 0, 1) as FirstChar", _context),
            new ODataValidationSettings(), out IEnumerable<string> validationErrors);
        Assert.True(result);
        Assert.Empty(validationErrors);
    }
}
