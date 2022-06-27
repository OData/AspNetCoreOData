//-----------------------------------------------------------------------------
// <copyright file="ODataQueryValidatorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator
{
    public class ODataQueryValidatorTest
    {
        private ODataQueryValidator _validator;
        private ODataQueryContext _context;

        public ODataQueryValidatorTest()
        {
            _validator = new ODataQueryValidator();
            _context = ValidationTestHelper.CreateCustomerContext(false);
        }

        public static TheoryDataSet<AllowedQueryOptions, string, string> SupportedQueryOptions
        {
            get
            {
                return new TheoryDataSet<AllowedQueryOptions, string, string>
                {
                    { AllowedQueryOptions.Count, "$count=true", "Count" },
                    { AllowedQueryOptions.Expand, "$expand=Contacts", "Expand" },
                    { AllowedQueryOptions.Filter, "$filter=Name eq 'Name'", "Filter" },
                    { AllowedQueryOptions.Format, "$format=json", "Format" },
                    { AllowedQueryOptions.OrderBy, "$orderby=Name", "OrderBy" },
                    { AllowedQueryOptions.Select, "$select=Name", "Select" },
                    { AllowedQueryOptions.Skip, "$skip=5", "Skip" },
                    { AllowedQueryOptions.Top, "$top=10", "Top" },
                    { AllowedQueryOptions.Apply, "$apply=groupby((Name))", "Apply" },
                    { AllowedQueryOptions.Compute, "$compute=AmountSpent mul 2 as DoubleAmount", "Compute" },
                    { AllowedQueryOptions.Search, "$search=text", "Search" },
                    { AllowedQueryOptions.SkipToken, "$skiptoken=__skip__", "SkipToken" },
                };
            }
        }

        public static TheoryDataSet<AllowedQueryOptions, string, string> UnsupportedQueryOptions
        {
            get
            {
                return new TheoryDataSet<AllowedQueryOptions, string, string>
                {
                    { AllowedQueryOptions.DeltaToken, "$deltatoken=__delta__", "DeltaToken" },
                };
            }
        }

        [Fact]
        public void ValidateThrowsOnNullOption()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() =>
                _validator.Validate(null, new ODataValidationSettings()));
        }

        [Fact]
        public void ValidateThrowsOnNullSettings()
        {
            var message = RequestFactory.Create();

            ExceptionAssert.Throws<ArgumentNullException>(() =>
                _validator.Validate(new ODataQueryOptions(_context, message), null));
        }

        [Fact]
        public void QueryOptionDataSets_CoverAllValues()
        {
            // Arrange
            // Get all values in the AllowedQueryOptions enum.
            var values = new HashSet<AllowedQueryOptions>(
                Enum.GetValues(typeof(AllowedQueryOptions)).Cast<AllowedQueryOptions>());

            var groupValues = new[]
            {
                AllowedQueryOptions.All,
                AllowedQueryOptions.None,
                AllowedQueryOptions.Supported,
            };
            var dataSets = SupportedQueryOptions.Concat(UnsupportedQueryOptions);

            // Act
            // Remove the group items.
            foreach (var allowed in groupValues)
            {
                values.Remove(allowed);
            }

            // Remove the individual items.
            foreach (var allowed in dataSets.Select(item => (AllowedQueryOptions)(item[0])))
            {
                values.Remove(allowed);
            }

            // Assert
            // Should have nothing left.
            Assert.Empty(values);
        }

        [Theory]
        [MemberData(nameof(SupportedQueryOptions))]
        [MemberData(nameof(UnsupportedQueryOptions))]
        public void AllowedQueryOptions_SucceedIfAllowed(AllowedQueryOptions allow, string query, string unused)
        {
            // Arrange
            var message = RequestFactory.Create("Get", "http://localhost/?$" + query, setupAction: null);
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = allow,
            };

            // Act & Assert
            Assert.NotNull(unused);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(SupportedQueryOptions))]
        [MemberData(nameof(UnsupportedQueryOptions))]
        public void AllowedQueryOptions_ThrowIfNotAllowed(AllowedQueryOptions exclude, string query, string optionName)
        {
            // Arrange
            var message = RequestFactory.Create("Get", "http://localhost/?" + query, setupAction: null);
            var option = new ODataQueryOptions(_context, message);
            var expectedMessage = string.Format(
                "Query option '{0}' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                optionName);
            var settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.All & ~exclude,
            };

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(SupportedQueryOptions))]
        [MemberData(nameof(UnsupportedQueryOptions))]
        public void AllowedQueryOptions_ThrowIfNoneAllowed(AllowedQueryOptions unused, string query, string optionName)
        {
            // Arrange
            var message = RequestFactory.Create("Get", "http://localhost/?" + query, setupAction: null);
            var option = new ODataQueryOptions(_context, message);
            var expectedMessage = string.Format(
                "Query option '{0}' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                optionName);
            var settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.None,
            };

            // Act & Assert
            Assert.NotEqual(unused, settings.AllowedQueryOptions);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(SupportedQueryOptions))]
        public void SupportedQueryOptions_SucceedIfGroupAllowed(AllowedQueryOptions unused, string query, string unusedName)
        {
            // Arrange
            var message = RequestFactory.Create("Get", "http://localhost/?$" + query, setupAction: null);
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.Supported,
            };

            // Act & Assert
            Assert.NotEqual(unused, settings.AllowedQueryOptions);
            Assert.NotNull(unusedName);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(SupportedQueryOptions))]
        public void SupportedQueryOptions_ThrowIfGroupNotAllowed(AllowedQueryOptions unused, string query, string optionName)
        {
            // Arrange
            var message = RequestFactory.Create("Get", "http://localhost/?" + query, setupAction: null);
            var option = new ODataQueryOptions(_context, message);
            var expectedMessage = string.Format(
                "Query option '{0}' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                optionName);
            var settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Supported,
            };

            // Act & Assert
            Assert.NotEqual(unused, settings.AllowedQueryOptions);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Theory]
        [MemberData(nameof(UnsupportedQueryOptions))]
        public void UnsupportedQueryOptions_SucceedIfGroupAllowed(AllowedQueryOptions unused, string query, string unusedName)
        {
            // Arrange
            var message = RequestFactory.Create("Get", "http://localhost/?$" + query, setupAction: null);
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Supported,
            };

            // Act & Assert
            Assert.Equal(unused, settings.AllowedQueryOptions); //Equal because only Delta token is unsupported.
            Assert.NotNull(unusedName);
            ExceptionAssert.DoesNotThrow(() => _validator.Validate(option, settings));
        }

        [Theory]
        [MemberData(nameof(UnsupportedQueryOptions))]
        public void UnsupportedQueryOptions_ThrowIfGroupNotAllowed(AllowedQueryOptions unused, string query, string optionName)
        {
            // Arrange
            var message = RequestFactory.Create("Get", "http://localhost/?" + query, setupAction: null);
            var option = new ODataQueryOptions(_context, message);
            var expectedMessage = string.Format(
                "Query option '{0}' is not allowed. " +
                "To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                optionName);
            var settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.Supported,
            };

            // Act & Assert
            Assert.NotEqual(unused, settings.AllowedQueryOptions);
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }

        [Fact]
        public void Validate_ValidatesSelectExpandQueryOption_IfItIsNotNull()
        {
            // Arrange
            var message = RequestFactory.Create("Get", "http://localhost/?$expand=Contacts/Contacts", setupAction: null);
            ODataQueryOptions option = new ODataQueryOptions(_context, message);

            Mock<SelectExpandQueryValidator> selectExpandValidator = new Mock<SelectExpandQueryValidator>();
            option.SelectExpand.Validator = selectExpandValidator.Object;
            ODataValidationSettings settings = new ODataValidationSettings();

            // Act
            _validator.Validate(option, settings);

            // Assert
            selectExpandValidator.Verify(v => v.Validate(option.SelectExpand, settings), Times.Once());
        }

        [Theory]
        [InlineData("$select=")]
        [InlineData("$select=  ")]
        [InlineData("$expand=")]
        [InlineData("$expand=  ")]
        [InlineData("$select=   &$expand=  &")]
        public void Validate_ValidatesNotEmptyOrWhitespaceSelectExpandQueryOption_IfEmptyOrWhitespace(string query)
        {
            var expectedMessage = "'select' and 'expand' cannot be empty or whitespace. Omit the parameter from the query if it is not used.";
            
            // Arrange
            var message = RequestFactory.Create("Get", $"http://localhost/?{query}", setupAction: null);
            ODataQueryOptions option = new ODataQueryOptions(_context, message);
            ODataValidationSettings settings = new ODataValidationSettings();

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => _validator.Validate(option, settings), expectedMessage);
        }
    }
}
