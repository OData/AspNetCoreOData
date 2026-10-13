//-----------------------------------------------------------------------------
// <copyright file="CaseQueryValidatorTests.cs" company=".NET Foundation">
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

public class CaseQueryValidatorTests
{
    [Theory]
    [InlineData("$compute", "case(true:Name) as Result")]
    [InlineData("$apply", "compute(case(true:Name) as Result)")]
    public void Case_CanBeAllowedOrDisallowed(string option, string query)
    {
        Validate(option, query, new() { AllowedFunctions = AllowedFunctions.Case });
        Assert.Throws<ODataException>(() => Validate(option, query, new() { AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.Case }));
    }

    [Theory]
    [InlineData("$filter", "case(true:1,false:length(Name)) eq 1")]
    [InlineData("$compute", "case(true:1,false:length(Name)) as Result")]
    [InlineData("$apply", "compute(case(true:1,false:length(Name)) as Result)")]
    public void Case_ValidatesFunctionsInUnselectedBranches(string option, string query)
    {
        ODataException exception = Assert.Throws<ODataException>(() => Validate(option, query, new() { AllowedFunctions = AllowedFunctions.Case }));
        Assert.Contains("length", exception.Message);
        Validate(option, query, new() { AllowedFunctions = AllowedFunctions.Case | AllowedFunctions.Length });
    }

    [Fact]
    public void Case_ValidatesRestrictedPropertiesInResults()
    {
        Assert.Throws<ODataException>(() => Validate("$compute", "case(true:Name,false:NotFilterableProperty) as Result", new()));
    }

    private static void Validate(string option, string query, ODataValidationSettings settings)
    {
        ODataQueryContext context = ValidationTestHelper.CreateCustomerContext();
        context.DefaultQueryConfigurations.EnableFilter = true;
        switch (option)
        {
            case "$filter":
                FilterQueryValidator filterValidator = new();
                filterValidator.Validate(new FilterQueryOption(query, context), settings);
                break;
            case "$compute":
                ComputeQueryValidator computeValidator = new();
                computeValidator.Validate(new ComputeQueryOption(query, context), settings);
                break;
            case "$apply":
                ApplyQueryValidator applyValidator = new();
                applyValidator.Validate(new ApplyQueryOption(query, context), settings);
                break;
        }
    }
}
