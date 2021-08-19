//-----------------------------------------------------------------------------
// <copyright file="SkipTokenQueryValidatorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator
{
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
        public void ValidateSkipTokenQueryValidator_ThrowsNotAllowedQueryOption()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings();

            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int), null);
            context.DefaultQuerySettings.EnableSkipToken = false;
            SkipTokenQueryOption query = new SkipTokenQueryOption("abc", context);

            SkipTokenQueryValidator validator = new SkipTokenQueryValidator();

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => validator.Validate(query, settings),
                "Query option 'SkipToken' is not allowed. To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.");
        }
    }
}
