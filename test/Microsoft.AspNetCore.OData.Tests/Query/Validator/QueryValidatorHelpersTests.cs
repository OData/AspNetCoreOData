//-----------------------------------------------------------------------------
// <copyright file="QueryValidatorHelpersTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

// Directly exercises the shared engine that every validator/option TryValidate delegates to.
// The individual validator suites prove delegation; these tests pin the exception contract
// (which types become validation errors, which propagate) in one place.
public class QueryValidatorHelpersTests
{
    [Fact]
    public void TryValidate_ThrowsArgumentNull_WhenValidateActionIsNull()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => { QueryValidatorHelpers.TryValidate(null, out _); },
            "validate");
    }

    [Fact]
    public void TryValidate_ReturnsTrueWithNoErrors_WhenValidateActionSucceeds()
    {
        // Arrange
        var invoked = false;
        Action validate = () => invoked = true;

        // Act
        var result = QueryValidatorHelpers.TryValidate(validate, out IEnumerable<string> errors);

        // Assert
        Assert.True(invoked); // the wrapped action actually ran
        Assert.True(result);
        Assert.NotNull(errors);
        Assert.Empty(errors);
    }

    public static TheoryDataSet<Exception> HandledValidationExceptions
    {
        get
        {
            // Exactly the exception types QueryValidatorHelpers.TryValidate converts into validation
            // errors. ArgumentNullException is included to prove it is caught via its ArgumentException base.
            return new TheoryDataSet<Exception>
            {
                new ODataException("odata failure"),
                new InvalidOperationException("invalid operation failure"),
                new NotSupportedException("not supported failure"),
                new NotImplementedException("not implemented failure"),
                new ArgumentException("argument failure"),
                new ArgumentNullException("someParam"),
            };
        }
    }

    [Theory]
    [MemberData(nameof(HandledValidationExceptions))]
    public void TryValidate_ReturnsFalseWithMessage_WhenValidateThrowsHandledException(Exception thrown)
    {
        // Arrange
        Action validate = () => throw thrown;

        // Act
        var result = QueryValidatorHelpers.TryValidate(validate, out IEnumerable<string> errors);

        // Assert
        Assert.False(result);
        Assert.NotNull(errors);
        Assert.Single(errors);
        Assert.Equal(thrown.Message, errors.First());
    }

    [Fact]
    public void TryValidate_RethrowsException_WhenValidateThrowsUnhandledException()
    {
        // Arrange - FormatException is not in the handled set, so it must surface to the caller
        // rather than being swallowed into a validation error.
        Action validate = () => throw new FormatException("unexpected failure");

        // Act & Assert
        var exception = Assert.Throws<FormatException>(
            () => { QueryValidatorHelpers.TryValidate(validate, out _); });
        Assert.Equal("unexpected failure", exception.Message);
    }
}
