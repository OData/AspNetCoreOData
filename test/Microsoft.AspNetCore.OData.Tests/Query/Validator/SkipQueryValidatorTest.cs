// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator
{
    public class SkipQueryValidatorTest
    {
        private SkipQueryValidator _validator;
        private ODataQueryContext _context;

        public SkipQueryValidatorTest()
        {
            _validator = new SkipQueryValidator();
            _context = ValidationTestHelper.CreateCustomerContext();
        }

        [Fact]
        public void ValidateSkipQueryValidator_ThrowsOnNullOption()
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ArgumentNullException>(() => _validator.Validate(null, new ODataValidationSettings()));
        }

        [Fact]
        public void ValidateSkipQueryValidator_ThrowsOnNullSettings()
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ArgumentNullException>(() => _validator.Validate(new SkipQueryOption("2", _context), null));
        }

        [Fact]
        public void ValidateSkipQueryValidator_ThrowsWhenLimitIsExceeded()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxSkip = 10
            };

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(new SkipQueryOption("11", _context), settings),
                "The limit of '10' for Skip query has been exceeded. The value from the incoming request is '11'.");
        }

        [Fact]
        public void ValidateSkipQueryValidator_PassWhenLimitIsReached()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxSkip = 10
            };

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(new SkipQueryOption("10", _context), settings));
        }

        [Fact]
        public void ValidateSkipQueryValidator_PassWhenLimitIsNotReached()
        {
            // Arrange
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                MaxSkip = 10
            };

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(new SkipQueryOption("9", _context), settings));
        }

        [Fact]
        public void GetSkipQueryValidator_Returns_Validator()
        {
            // Arrange & Act & Assert
            Assert.NotNull(SkipQueryValidator.GetSkipQueryValidator(null));

            // Arrange & Act & Assert
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            Assert.NotNull(SkipQueryValidator.GetSkipQueryValidator(context));

            // Arrange & Act & Assert
            IServiceProvider services = new ServiceCollection()
                .AddSingleton<SkipQueryValidator>().BuildServiceProvider();
            context.RequestContainer = services;
            Assert.NotNull(SkipQueryValidator.GetSkipQueryValidator(context));
        }
    }
}