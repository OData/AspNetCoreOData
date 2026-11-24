//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Query.Models;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class ComputeQueryValidatorTests
{
    private ComputeQueryValidator _validator = new ComputeQueryValidator();
    private ODataQueryContext _context = ValidationTestHelper.CreateCustomerContext();

    [Theory]
    [InlineData("Category/QueryableProducts/$count($filter=Category/EnumerableProducts/any(a:a/Category/EnumerableProducts/any(b:b/ProductName eq 'x'))) as Total")]
    [InlineData("Category/QueryableProducts/$count($filter=Category/EnumerableProducts/all(a:a/Category/EnumerableProducts/all(b:b/ProductName eq 'x'))) as Total")]
    [InlineData("Category/QueryableProducts/$count($filter=Category/EnumerableProducts/any(a:a/Category/EnumerableProducts/all(b:b/ProductName eq 'x'))) as Total")]
    [InlineData("Category/QueryableProducts/$count($filter=Category/EnumerableProducts/all(a:a/Category/EnumerableProducts/any(b:b/ProductName eq 'x'))) as Total")]
    [InlineData("Category/QueryableProducts/any(a:a/Category/EnumerableProducts/$count($filter=Category/QueryableProducts/any(b:b/ProductName eq 'x')) gt 0) as Flag")]
    [InlineData("1 as First,Category/QueryableProducts/$count($filter=Category/EnumerableProducts/any(a:a/Category/EnumerableProducts/any(b:b/ProductName eq 'x')) in (true)) as Total")]
    public void AnyAllExpressionDepth_ComputeChecksCountFiltersAtInheritedDepth(string expression)
    {
        var context = ValidationTestHelper.CreateDerivedProductsContext();
        var option = new ComputeQueryOption(expression, context);

        var error = Assert.Throws<ODataException>(() => _validator.Validate(option, new ODataValidationSettings()));

        Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", error.Message);
        _validator.Validate(option, new ODataValidationSettings { MaxAnyAllExpressionDepth = 2 });
    }

    [Theory]
    [InlineData("Category/QueryableProducts/$count as Total")]
    [InlineData("Category/QueryableProducts/$count($filter=ProductName eq 'x') as Total")]
    [InlineData("Category/QueryableProducts/$count($filter=Category/EnumerableProducts/any()) as Total")]
    [InlineData("Category/QueryableProducts/$count($filter=Category/EnumerableProducts/any(a:a/ProductName eq 'x')) as First,Category/QueryableProducts/$count($filter=Category/EnumerableProducts/all(a:a/ProductName eq 'x')) as Second")]
    public void AnyAllExpressionDepth_ComputeAllowsIndependentCountFilters(string expression)
    {
        var context = ValidationTestHelper.CreateDerivedProductsContext();

        _validator.Validate(new ComputeQueryOption(expression, context), new ODataValidationSettings());
    }

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
    [InlineData("Contacts/$count($filter=NotFilterableProperty eq 'x') as C")]
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
    [InlineData("Contacts/$count as C")]
    [InlineData("Contacts/$count($filter=Name eq 'x') as C")]
    [InlineData("Contacts/$count($search=red AND blue) as C")]
    [InlineData("Contacts/$count($filter=Name eq 'x';$search=red AND blue) as C")]
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
    public void ValidateComputeQueryValidator_ThrowsWhenFunctionCallDepthExceeded()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<QueryCompositionCustomer>("Customers");
        var context = new ODataQueryContext(builder.GetEdmModel(), typeof(QueryCompositionCustomer), null);
        var option = new ComputeQueryOption("tolower(toupper(Name)) as Normalized", context);
        var settings = new ODataValidationSettings { MaxFunctionCallDepth = 1 };

        var exception = Assert.Throws<ODataException>(() => _validator.Validate(option, settings));

        Assert.Contains("MaxFunctionCallDepth", exception.Message);
    }

    [Theory]
    [InlineData("tolower(toupper(Name)) as Normalized", 2)]
    [InlineData("tolower(Name) as Lower,toupper(Name) as Upper", 1)]
    public void ValidateComputeQueryValidator_DoesNotThrowWhenFunctionCallDepthPermits(string compute, int maxFunctionCallDepth)
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<QueryCompositionCustomer>("Customers");
        var context = new ODataQueryContext(builder.GetEdmModel(), typeof(QueryCompositionCustomer), null);
        var option = new ComputeQueryOption(compute, context);
        var settings = new ODataValidationSettings { MaxFunctionCallDepth = maxFunctionCallDepth };

        _validator.Validate(option, settings);
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
    public void ValidateComputeQueryValidator_ThrowsForDisallowedLogicalOperatorInCountFilter()
    {
        // Arrange
        var option = new ComputeQueryOption("Contacts/$count($filter=Name eq 'x') as C", _context);
        var settings = new ODataValidationSettings { AllowedLogicalOperators = AllowedLogicalOperators.All & ~AllowedLogicalOperators.Equal };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "Logical operator 'Equal' is not allowed. To allow it, set the 'AllowedLogicalOperators' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsForDisallowedArithmeticOperatorInCountFilter()
    {
        // Arrange
        var option = new ComputeQueryOption("Contacts/$count($filter=AmountSpent mul 2 gt 10) as C", _context);
        var settings = new ODataValidationSettings { AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~AllowedArithmeticOperators.Multiply };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "Arithmetic operator 'Multiply' is not allowed. To allow it, set the 'AllowedArithmeticOperators' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsForDisallowedFunctionInCountFilter()
    {
        // Arrange
        var option = new ComputeQueryOption("Contacts/$count($filter=length(Name) gt 1) as C", _context);
        var settings = new ODataValidationSettings { AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.Length };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "Function 'length' is not allowed. To allow it, set the 'AllowedFunctions' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Theory]
    [InlineData("Contacts/$count($filter=Name eq 'x') as C")]
    [InlineData("Contacts/$count($search=red AND blue) as C")]
    public void ValidateComputeQueryValidator_ThrowsWhenNodeCountExceededInCountClause(string compute)
    {
        // Arrange - the count, collection navigation and range variable alone fit within this limit.
        var option = new ComputeQueryOption(compute, _context);
        var settings = new ODataValidationSettings { MaxNodeCount = 3 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "The node count limit of '3' has been exceeded. To increase the limit, set the 'MaxNodeCount' property on EnableQueryAttribute or ODataValidationSettings.");
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsWhenFunctionCallDepthExceededInCountFilter()
    {
        // Arrange
        var option = new ComputeQueryOption("Contacts/$count($filter=length(trim(Name)) gt 1) as C", _context);
        var settings = new ODataValidationSettings { MaxFunctionCallDepth = 1 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "The function call depth limit of '1' has been exceeded. 'MaxFunctionCallDepth' can be configured on ODataValidationSettings, ODataQuerySettings or EnableQueryAttribute.");
    }

    [Fact]
    public void ValidateComputeQueryValidator_ThrowsWhenAnyAllDepthExceededInCountFilter()
    {
        // Arrange
        var option = new ComputeQueryOption("Contacts/any(c: c/Contacts/$count($filter=Contacts/any(d: d/Name ne null)) gt 0) as C", _context);
        var settings = new ODataValidationSettings { MaxAnyAllExpressionDepth = 1 };

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => _validator.Validate(option, settings),
            "The Any/All nesting limit of '1' has been exceeded. 'MaxAnyAllExpressionDepth' can be configured on ODataValidationSettings or EnableQueryAttribute.");
    }
}
