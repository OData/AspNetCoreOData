//-----------------------------------------------------------------------------
// <copyright file="SearchQueryValidatorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class SearchQueryValidatorTest
{
    [Fact]
    public void ValidateSearchQuery_CallsTheRegisteredValidator()
    {
        // Arrange
        int count = 0;
        ODataQueryContext context = ValidationTestHelper.CreateCustomerContext(s => s.AddSingleton<ISearchQueryValidator>(new MySearchValidator(() => count++)));
        ODataValidationSettings settings = new ODataValidationSettings();

        // Act & Assert
        Assert.Equal(0, count);
        SearchQueryOption search = new SearchQueryOption("any", context);

        search.Validate(settings);
        Assert.Equal(1, count);

        search.Validate(settings);
        Assert.Equal(2, count);
    }

    [Fact]
    public void GetSearchQueryValidator_Returns_Validator()
    {
        // Arrange & Act & Assert
        ODataQueryContext context = null;
        Assert.Null(context.GetSearchQueryValidator());

        // Arrange & Act & Assert
        context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        Assert.Null(context.GetSearchQueryValidator());

        // Arrange & Act & Assert
        IServiceProvider services = new ServiceCollection()
            .AddSingleton<ISearchQueryValidator>(new MySearchValidator()).BuildServiceProvider();
        context.RequestContainer = services;
        Assert.NotNull(context.GetSearchQueryValidator());
    }

    [Fact]
    public void TryValidate_ReturnsTrueAndNoErrors_WhenRegisteredValidatorDoesNotThrow()
    {
        // Arrange
        int count = 0;
        var context = ValidationTestHelper.CreateCustomerContext(s => s.AddSingleton<ISearchQueryValidator>(new MySearchValidator(() => count++)));
        var settings = new ODataValidationSettings();
        var search = new SearchQueryOption("any", context);

        // Act
        var result = search.TryValidate(settings, out var errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
        Assert.Equal(1, count);
    }

    [Fact]
    public void TryValidate_ReturnsFalseAndError_WithoutThrowing_WhenRegisteredValidatorThrows()
    {
        // Arrange
        var context = ValidationTestHelper.CreateCustomerContext(
            s => s.AddSingleton<ISearchQueryValidator>(new MySearchValidator(() => throw new ODataException("Search failed."))));
        var settings = new ODataValidationSettings();
        var search = new SearchQueryOption("any", context);

        // Act
        var result = search.TryValidate(settings, out var errors);

        // Assert
        Assert.False(result);
        var error = Assert.Single(errors);
        Assert.Equal("Search failed.", error);
    }

    private class MySearchValidator : ISearchQueryValidator
    {
        public MySearchValidator(Action verify = null)
        {
            Verify = verify;
        }

        public Action Verify { get; }

        public void Validate(SearchQueryOption searchQueryOption, ODataValidationSettings validationSettings)
        {
            Verify?.Invoke();
        }

        public bool TryValidate(SearchQueryOption searchQueryOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
        {
            Verify?.Invoke();
            validationErrors = null;
            return true;
        }
    }
}
