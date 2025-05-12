//-----------------------------------------------------------------------------
// <copyright file="SkipTokenQueryValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class SkipTokenQueryValidatorTests
{
    [Fact]
    public void ValidateSkipTokenQueryValidator_ThrowsOnNullOption()
    {
        // Arrange & Act & Assert
        SkipTokenQueryValidator validator = new SkipTokenQueryValidator();
        ExceptionAssert.Throws<ArgumentNullException>(() => validator.Validate(null, new ODataValidationSettings()));
    }

    [Fact]
    public void TryValidateSkipTokenQueryValidator_ReturnsFalseWithError_OnNullOption()
    {
        // Arrange
        SkipTokenQueryValidator validator = new SkipTokenQueryValidator();

        // Act
        var result = validator.TryValidate(null, new ODataValidationSettings(), out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("Value cannot be null. (Parameter 'skipToken')", errors.First().Message);
    }

    [Fact]
    public void ValidateSkipTokenQueryValidator_ThrowsOnNullSettings()
    {
        // Arrange & Act
        SkipTokenQueryValidator validator = new SkipTokenQueryValidator();

        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int), null);
        SkipTokenQueryOption query = new SkipTokenQueryOption("abc", context);

        // Assert
        ExceptionAssert.Throws<ArgumentNullException>(() => validator.Validate(query, null));
    }

    [Fact]
    public void TryValidateSkipTokenQueryValidator_ReturnsFalseWithError_OnNullSettings()
    {
        // Arrange
        SkipTokenQueryValidator validator = new SkipTokenQueryValidator();
        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int), null);
        SkipTokenQueryOption query = new SkipTokenQueryOption("abc", context);

        // Act
        var result = validator.TryValidate(query, null, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal("Value cannot be null. (Parameter 'validationSettings')", errors.First().Message);
    }

    [Fact]
    public void ValidateSkipTokenQueryValidator_ThrowsNotAllowedQueryOption()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings();

        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int), null);
        context.DefaultQueryConfigurations.EnableSkipToken = false;
        SkipTokenQueryOption query = new SkipTokenQueryOption("abc", context);

        SkipTokenQueryValidator validator = new SkipTokenQueryValidator();

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => validator.Validate(query, settings),
            "Query option 'SkipToken' is not allowed. To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.");
    }

    [Fact]
    public void TryValidateSkipTokenQueryValidator_ReturnsFalseWithError_NotAllowedQueryOption()
    {
        // Arrange
        ODataValidationSettings settings = new ODataValidationSettings();
        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int), null);
        context.DefaultQueryConfigurations.EnableSkipToken = false;
        SkipTokenQueryOption query = new SkipTokenQueryOption("abc", context);
        SkipTokenQueryValidator validator = new SkipTokenQueryValidator();

        // Act
        var result = validator.TryValidate(query, settings, out IEnumerable<ODataException> errors);

        // Assert
        Assert.False(result);
        Assert.Single(errors);
        Assert.Equal(
            "Query option 'SkipToken' is not allowed. To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
            errors.First().Message);
    }
}
