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
}
