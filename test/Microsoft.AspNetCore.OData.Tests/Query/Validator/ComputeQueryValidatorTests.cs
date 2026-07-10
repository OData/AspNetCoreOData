//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
    public void ValidateComputeQueryValidator_ThrowsOnNullSettings()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => _validator.Validate(new ComputeQueryOption("substring(Name, 0, 1) as FirstChar", _context), null), "validationSettings");
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
}
