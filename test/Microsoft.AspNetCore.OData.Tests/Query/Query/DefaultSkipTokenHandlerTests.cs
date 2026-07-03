//-----------------------------------------------------------------------------
// <copyright file="DefaultSkipTokenHandlerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

using Microsoft.AspNetCore.Http;
using System.Runtime.Serialization;

public class DefaultSkipTokenHandlerTests
{
    private static IEdmModel _model = GetEdmModel();

    private static IEdmModel _modelLowerCamelCased = GetEdmModelLowerCamelCased();

    private static IEdmModel _modelAliased = GetEdmModelAliased();

    private static IEdmModel _openModel = GetOpenEdmModel();

    [Fact]
    public void GenerateNextPageLink_ReturnsNull_NullContext()
    {
        // Arrange
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();

        // Act & Assert
        Assert.Null(handler.GenerateNextPageLink(null, 2, null, null));
    }

    [Theory]
    [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skip=10")]
    [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skip=10")]
    public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectNextLink(string baseUri, string expectedUri)
    {
        // Arrange
        ODataSerializerContext serializerContext = GetSerializerContext(_model, false);
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();

        // Act
        var uri = handler.GenerateNextPageLink(new Uri(baseUri), 10, null, serializerContext);
        var actualUri = uri.ToString();

        // Assert
        Assert.Equal(expectedUri, actualUri);
    }

    [Theory]
    [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skiptoken=Id-42")]
    [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skiptoken=Id-42")]
    [InlineData("http://localhost/Customers?$select=Name", "http://localhost/Customers?$select=Name&$skiptoken=Id-42")]
    public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink(string baseUri, string expectedUri)
    {
        this.GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_Implementation(baseUri,
            expectedUri, _model);
    }

    [Theory]
    [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skiptoken=id-42")]
    [InlineData("http://localhost/Customers?$expand=orders", "http://localhost/Customers?$expand=orders&$skiptoken=id-42")]
    [InlineData("http://localhost/Customers?$select=name", "http://localhost/Customers?$select=name&$skiptoken=id-42")]
    public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_WithLowerCamelCase(string baseUri, string expectedUri)
    {
        this.GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_Implementation(baseUri,
            expectedUri, _modelLowerCamelCased);
    }

    [Theory]
    [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skiptoken=SkipCustomerId-42")]
    [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skiptoken=SkipCustomerId-42")]
    [InlineData("http://localhost/Customers?$select=FirstAndLastName", "http://localhost/Customers?$select=FirstAndLastName&$skiptoken=SkipCustomerId-42")]
    public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_WithAlias(string baseUri, string expectedUri)
    {
        this.GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_Implementation(baseUri,
            expectedUri, _modelAliased);
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_StringEmpty()
    {
        // Arrange & Act & Assert
        Assert.Equal(string.Empty, DefaultSkipTokenHandler.GenerateSkipTokenValue(null, null, null));
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_Implementation(_model,
            "Id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_Implementation(_modelLowerCamelCased,
            "id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_Implementation(_modelAliased,
            "SkipCustomerId-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_Implementation(
            _model,
            "Name",
            "Name-%27ZX%27,Id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase_WithOrderby()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_Implementation(
            _modelLowerCamelCased,
            "name",
            "name-%27ZX%27,id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias_WithOrderby()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_Implementation(
            _modelAliased,
            "FirstAndLastName",
            "FirstAndLastName-%27ZX%27,SkipCustomerId-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue_Implementation(
            _model,
            "Name",
            "Name-null,Id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase_WithOrderby_IfNullValue()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue_Implementation(
            _modelLowerCamelCased,
            "name",
            "name-null,id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias_WithOrderby_IfNullValue()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue_Implementation(
            _modelAliased,
            "FirstAndLastName",
            "FirstAndLastName-null,SkipCustomerId-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset_Implementation(
            _model,
            "Birthday",
            "Birthday-2021-01-19T19%3A04%3A05-08%3A00,Id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase_WithDateTimeOffset()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset_Implementation(
            _modelLowerCamelCased,
            "birthday",
            "birthday-2021-01-19T19%3A04%3A05-08%3A00,id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias_WithDateTimeOffset()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset_Implementation(
            _modelAliased,
            "DateOfBirth",
            "DateOfBirth-2021-01-19T19%3A04%3A05-08%3A00,SkipCustomerId-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue_Implementation(
            _model,
            "Gender",
            "Gender-Microsoft.AspNetCore.OData.Tests.Query.Gender%27Male%27,Id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase_WithOrderby_WithEnumValue()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue_Implementation(
            _modelLowerCamelCased,
            "gender",
            "gender-Microsoft.AspNetCore.OData.Tests.Query.Gender%27Male%27,id-42");
    }

    [Fact]
    public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias_WithOrderby_WithEnumValue()
    {
        GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue_Implementation(
            _modelAliased,
            "MaleOrFemale",
            "MaleOrFemale-Microsoft.AspNetCore.OData.Tests.Query.Gender%27Male%27,SkipCustomerId-42");
    }

    [Fact]
    public void ApplyToSkipTokenHandler_ThrowsArgumentNull_Query()
    {
        // Arrange
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => handler.ApplyTo(query: null, null, null, null), "query");
    }

    [Fact]
    public void ApplyToSkipTokenHandler_ThrowsArgumentNull_SkipTokenQueryOption()
    {
        // Arrange
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        IQueryable query = Array.Empty<int>().AsQueryable();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => handler.ApplyTo(query, null, null, null), "skipTokenQueryOption");
    }

    [Fact]
    public void ApplyToSkipTokenHandler_ThrowsArgumentNull_QuerySettings()
    {
        // Arrange
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        IQueryable query = Array.Empty<int>().AsQueryable();
        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, EdmCoreModel.Instance.GetInt32(false).Definition);
        SkipTokenQueryOption skipTokenQueryOption = new SkipTokenQueryOption("abc", context);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => handler.ApplyTo(query, skipTokenQueryOption, null, null), "querySettings");
    }

    [Fact]
    public void ApplyToSkipTokenHandler_ThrowsNotSupported_WithoutElementType()
    {
        // Arrange
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, EdmCoreModel.Instance.GetInt32(false).Definition);
        SkipTokenQueryOption skipTokenQueryOption = new SkipTokenQueryOption("abc", context);
        IQueryable query = Array.Empty<int>().AsQueryable();

        // Act & Assert
        ExceptionAssert.Throws<NotSupportedException>(
            () => handler.ApplyTo(query, skipTokenQueryOption, new ODataQuerySettings(), null),
            "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
    }

    [Fact]
    public void ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue()
    {
        ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue_Implementation(_model);
    }

    [Fact]
    public void ApplyToSkipTokenHandler_ThrowsODataException_WithLowerCamelCase_InvalidSkipTokenValue()
    {
        ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue_Implementation(_modelLowerCamelCased);
    }
        
    [Fact]
    public void ApplyToSkipTokenHandler_ThrowsODataException_WithAlias_InvalidSkipTokenValue()
    {
        ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue_Implementation(_modelAliased);
    }

    [Fact]
    public void ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable()
    {
        ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable_Implementation(
            _model,
            "Id-2");
    }

    [Fact]
    public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithLowerCamelCase_ToQueryable()
    {
        ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable_Implementation(
            _modelLowerCamelCased,
            "id-2");
    }

    [Fact]
    public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithAlias_ToQueryable()
    {
        ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable_Implementation(
            _modelAliased,
            "SkipCustomerId-2");
    }

    [Fact]
    public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable()
    {
        ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable_Implementation(
            _model,
            "Name",
            "Name-'Alex',Id-3");
    }

    [Fact]
    public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_WithLowerCamelCase_ToQueryable()
    {
        ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable_Implementation(
            _modelLowerCamelCased,
            "name",
            "name-'Alex',Id-3");
    }

    [Fact]
    public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_WithAlias_ToQueryable()
    {
        ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable_Implementation(
            _modelAliased,
            "FirstAndLastName",
            "FirstAndLastName-'Alex',SkipCustomerId-3");
    }

    [Theory]
    [InlineData("Id-2) or (1 eq 1", 1)]    // sanitized: Id > 2 → only {Id:3}
    [InlineData("Id-42) or (1 eq 1", 0)]   // sanitized: Id > 42 → nothing
    [InlineData("Id-1) or (1 eq 1", 2)]    // sanitized: Id > 1 → {Id:2, Id:3}
    public void ApplyTo_IgnoresTrailingCharacters_WhenIntegerKeyValueContainsMalformedSuffix(
        string skipTokenRawValue, int expectedCount)
    {
        // ConvertFromUriLiteral parses only the leading integer; extra characters are discarded.
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenRawValue, context);
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Name = "Andy" },
            new SkipCustomer { Id = 2, Name = "Aaron" },
            new SkipCustomer { Id = 3, Name = "Alex" }
        }.AsQueryable();

        // Act
        SkipCustomer[] results = handler.ApplyTo(
            customers,
            skipTokenQuery,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False },
            queryOptions).ToArray();

        // Assert
        Assert.Equal(expectedCount, results.Length);
    }

    [Theory]
    [InlineData("Name-'ZX') or (1 eq 1,Id-42")]  // extra expression after closing quote
    [InlineData("Name-'ZX' or 1 eq 1,Id-42")]     // extra expression after closing quote (no paren)
    public void ApplyTo_IgnoresTrailingCharacters_WhenStringValueContainsMalformedSuffix(string skipTokenRawValue)
    {
        // ConvertFromUriLiteral reads only the string literal up to the closing quote; extra characters are discarded.
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            "http://server/Customers?$orderby=Name asc&$skiptoken=" + skipTokenRawValue);
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenRawValue, context);
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Name = "Andy" },
            new SkipCustomer { Id = 2, Name = "Aaron" },
            new SkipCustomer { Id = 3, Name = "Alex" }
        }.AsQueryable();

        // Act
        SkipCustomer[] results = handler.ApplyTo(
            customers,
            skipTokenQuery,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False },
            queryOptions).ToArray();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ApplyTo_ThrowsInvalidOperationException_WhenSegmentCountMismatchesOrderByCount()
    {
        // Stable order yields 1 clause; a token with 2 segments must be rejected.
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

        // 2-segment token for an entity whose stable order yields 1 clause
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("Id-1,Name-'Alex'", context);
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>().AsQueryable();

        // Act & Assert
        ExceptionAssert.Throws<InvalidOperationException>(
            () => handler.ApplyTo(customers, skipTokenQuery, new ODataQuerySettings(), queryOptions),
            "Unable to get property values from the skiptoken value, or the token value can not match the orderby clause.");
    }

    [Fact]
    public void ApplyTo_StillWorks_WithLegitimateIntegerSkipToken()
    {
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/?$skiptoken=Id-2");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("Id-2", context);
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Name = "Andy" },
            new SkipCustomer { Id = 2, Name = "Aaron" },
            new SkipCustomer { Id = 3, Name = "Alex" }
        }.AsQueryable();

        // Act
        SkipCustomer[] results = handler.ApplyTo(
            customers,
            skipTokenQuery,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False },
            queryOptions).ToArray();

        // Assert — only Id=3 (Alex) is after the token value of 2
        SkipCustomer result = Assert.Single(results);
        Assert.Equal(3, result.Id);
    }

    [Fact]
    public void ApplyTo_StillWorks_WithLegitimateStringOrderBySkipToken()
    {
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            "http://server/Customers?$orderby=Name asc&$skiptoken=Name-'Aaron',Id-2");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("Name-'Aaron',Id-2", context);
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Name = "Andy" },
            new SkipCustomer { Id = 2, Name = "Aaron" },
            new SkipCustomer { Id = 3, Name = "Alex" }
        }.AsQueryable();

        // Act
        SkipCustomer[] results = handler.ApplyTo(
            customers,
            skipTokenQuery,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False },
            queryOptions).ToArray();

        // Assert — "Alex" (Id=3) and "Andy" (Id=1) come after "Aaron" alphabetically
        Assert.Equal(2, results.Length);
        Assert.Contains(results, c => c.Name == "Alex");
        Assert.Contains(results, c => c.Name == "Andy");
    }

    [Fact]
    public void ApplyTo_DoesNotValidateFilterSettings_ForSyntheticSkipTokenFilter()
    {
        // The synthetic skip-token filter must not be validated against ODataValidationSettings.
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            "http://server/Customers?$orderby=Name asc&$skiptoken=Name-'Aaron',Id-2");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

        context.ValidationSettings = new ODataValidationSettings
        {
            AllowedLogicalOperators = AllowedLogicalOperators.None
        };

        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("Name-'Aaron',Id-2", context);
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 3, Name = "Bob" },
            new SkipCustomer { Id = 4, Name = "Carol" },
        }.AsQueryable();

        // Act — must NOT throw even though AllowedLogicalOperators.None is set
        IQueryable result = handler.ApplyTo(customers, skipTokenQuery, new ODataQuerySettings(), queryOptions);

        Assert.NotNull(result);
    }

    [Fact]
    public void ApplyTo_ThrowsNotSupportedException_ForTypelessODataQueryContext()
    {
        // ODataQueryContext built from an IEdmType leaves ElementClrType null.
        IEdmEntitySet entitySet = _model.EntityContainer.FindEntitySet("Customers");
        IEdmEntityType edmEntityType = entitySet.EntityType;

        ODataQueryContext typelessContext = new ODataQueryContext(_model, (IEdmType)edmEntityType, null);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("Id-1", typelessContext);
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();

        // Act & Assert
        ExceptionAssert.Throws<NotSupportedException>(
            () => handler.ApplyTo(
                new List<object>().AsQueryable(),
                skipTokenQuery,
                new ODataQuerySettings(),
                null));
    }

    [Fact]
    public void ApplyTo_Succeeds_ForOpenEntityType_StringDynamicPropertyComparison()
    {
        // Dynamic properties have TypeReference=null; the binder coerces the object-typed
        // expression to the constant's CLR type so comparison works correctly at runtime.
        ODataQueryContext context = new ODataQueryContext(_openModel, typeof(OpenSkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            "http://server/OpenCustomers?$orderby=DynamicProp asc&$skiptoken=DynamicProp-'abc',Id-1");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("DynamicProp-'abc',Id-1", context);
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();

        IList<OpenSkipCustomer> customers = new List<OpenSkipCustomer>
        {
            new OpenSkipCustomer { Id = 1, DynamicProperties = new Dictionary<string, object> { { "DynamicProp", "abc" } } },
            new OpenSkipCustomer { Id = 2, DynamicProperties = new Dictionary<string, object> { { "DynamicProp", "def" } } },
            new OpenSkipCustomer { Id = 3, DynamicProperties = new Dictionary<string, object> { { "DynamicProp", "xyz" } } },
        };

        // Act — should succeed now that the binder coerces object-typed dynamic property expressions.
        IQueryable result = handler.ApplyTo(
            customers.AsQueryable(),
            skipTokenQuery,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True },
            queryOptions);

        List<OpenSkipCustomer> resultList = result.Cast<OpenSkipCustomer>().ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, c => c.Id == 2);
        Assert.Contains(resultList, c => c.Id == 3);
    }

    [Fact]
    public void ApplyTo_Works_ForVanillaQueryContext_WithoutODataPath()
    {
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));  // no path
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/?$skiptoken=Id-2");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("Id-2", context);
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Name = "Andy" },
            new SkipCustomer { Id = 2, Name = "Aaron" },
            new SkipCustomer { Id = 3, Name = "Alex" }
        }.AsQueryable();

        // Act
        SkipCustomer[] results = handler.ApplyTo(
            customers,
            skipTokenQuery,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False },
            queryOptions).ToArray();

        // Assert — skip token "Id-2" returns only Id=3 (Alex)
        SkipCustomer result = Assert.Single(results);
        Assert.Equal(3, result.Id);
    }

    [Theory]
    [InlineData("IsVerified-false,Id-2", HandleNullPropagationOption.False, new[] { 3, 4 })]
    [InlineData("IsVerified-false,Id-2", HandleNullPropagationOption.True,  new[] { 3, 4 })]
    [InlineData("IsVerified-true,Id-4",  HandleNullPropagationOption.False, new int[0])]
    [InlineData("IsVerified-true,Id-4",  HandleNullPropagationOption.True,  new int[0])]
    public void ApplyTo_Works_WithNullableBoolOrderByAscending(
        string skipTokenValue,
        HandleNullPropagationOption nullPropagation,
        int[] expectedIds)
    {
        ODataQueryContext context = new ODataQueryContext(_openModel, typeof(OpenSkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            $"http://server/OpenCustomers?$orderby=IsVerified asc,Id asc&$skiptoken={skipTokenValue}");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<OpenSkipCustomer> customers = new List<OpenSkipCustomer>
        {
            new OpenSkipCustomer { Id = 1, IsVerified = null },
            new OpenSkipCustomer { Id = 2, IsVerified = false },
            new OpenSkipCustomer { Id = 3, IsVerified = false },
            new OpenSkipCustomer { Id = 4, IsVerified = true },
        }.AsQueryable();

        // Act
        OpenSkipCustomer[] results = new DefaultSkipTokenHandler()
            .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
            .Cast<OpenSkipCustomer>().ToArray();

        // Assert
        Assert.Equal(expectedIds.Length, results.Length);
        foreach (int expectedId in expectedIds)
        {
            Assert.Contains(results, c => c.Id == expectedId);
        }
    }

    [Theory]
    // Ordered desc: [4:true, 3:false, 2:false, 1:null]
    // After (true,4)  → {3:false, 2:false, 1:null}  = {1,2,3}
    // After (false,2) → {1:null}                     = {1}
    [InlineData("IsVerified-true,Id-4",  HandleNullPropagationOption.False, new[] { 1, 2, 3 })]
    [InlineData("IsVerified-true,Id-4",  HandleNullPropagationOption.True,  new[] { 1, 2, 3 })]
    [InlineData("IsVerified-false,Id-2", HandleNullPropagationOption.False, new[] { 1 })]
    [InlineData("IsVerified-false,Id-2", HandleNullPropagationOption.True,  new[] { 1 })]
    public void ApplyTo_Works_WithNullableBoolOrderByDescending(
        string skipTokenValue,
        HandleNullPropagationOption nullPropagation,
        int[] expectedIds)
    {
        ODataQueryContext context = new ODataQueryContext(_openModel, typeof(OpenSkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            $"http://server/OpenCustomers?$orderby=IsVerified desc,Id desc&$skiptoken={skipTokenValue}");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<OpenSkipCustomer> customers = new List<OpenSkipCustomer>
        {
            new OpenSkipCustomer { Id = 1, IsVerified = null },
            new OpenSkipCustomer { Id = 2, IsVerified = false },
            new OpenSkipCustomer { Id = 3, IsVerified = false },
            new OpenSkipCustomer { Id = 4, IsVerified = true },
        }.AsQueryable();

        // Act
        OpenSkipCustomer[] results = new DefaultSkipTokenHandler()
            .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
            .Cast<OpenSkipCustomer>().ToArray();

        // Assert
        Assert.Equal(expectedIds.Length, results.Length);
        foreach (int expectedId in expectedIds)
        {
            Assert.Contains(results, c => c.Id == expectedId);
        }
    }

    [Theory]
    // Non-nullable bool descending: true > false.
    // Ordered desc by IsActive, then asc by Id: [3:true, 4:true, 1:false, 2:false]
    // After (true,4)  → {1:false, 2:false} = {1,2}
    // After (false,1) → {2:false}           = {2}
    [InlineData("IsActive-true,Id-4",  HandleNullPropagationOption.False, new[] { 1, 2 })]
    [InlineData("IsActive-true,Id-4",  HandleNullPropagationOption.True,  new[] { 1, 2 })]
    [InlineData("IsActive-false,Id-1", HandleNullPropagationOption.False, new[] { 2 })]
    [InlineData("IsActive-false,Id-1", HandleNullPropagationOption.True,  new[] { 2 })]
    public void ApplyTo_Works_WithNonNullableBoolOrderByDescending(
        string skipTokenValue,
        HandleNullPropagationOption nullPropagation,
        int[] expectedIds)
    {
        ODataQueryContext context = new ODataQueryContext(_openModel, typeof(OpenSkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            $"http://server/OpenCustomers?$orderby=IsActive desc,Id asc&$skiptoken={skipTokenValue}");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<OpenSkipCustomer> customers = new List<OpenSkipCustomer>
        {
            new OpenSkipCustomer { Id = 1, IsActive = false },
            new OpenSkipCustomer { Id = 2, IsActive = false },
            new OpenSkipCustomer { Id = 3, IsActive = true },
            new OpenSkipCustomer { Id = 4, IsActive = true },
        }.AsQueryable();

        // Act
        OpenSkipCustomer[] results = new DefaultSkipTokenHandler()
            .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
            .Cast<OpenSkipCustomer>().ToArray();

        // Assert
        Assert.Equal(expectedIds.Length, results.Length);
        foreach (int expectedId in expectedIds)
        {
            Assert.Contains(results, c => c.Id == expectedId);
        }
    }

    [Theory]
    [InlineData(HandleNullPropagationOption.False)]
    [InlineData(HandleNullPropagationOption.True)]
    public void ApplyTo_Works_WithDeclaredEnumPropertyOrderByAscending(HandleNullPropagationOption nullPropagation)
    {
        // Gender enum: Male=0, Female=1. Ascending: [2:Male, 4:Male, 1:Female, 3:Female]
        string skipTokenValue = $"Gender-{typeof(Gender).FullName}'Male',Id-2";
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            "http://server/Customers?$orderby=Gender asc,Id asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Gender = Gender.Female },
            new SkipCustomer { Id = 2, Gender = Gender.Male },
            new SkipCustomer { Id = 3, Gender = Gender.Female },
            new SkipCustomer { Id = 4, Gender = Gender.Male },
        }.AsQueryable();

        // Act
        SkipCustomer[] results = new DefaultSkipTokenHandler()
            .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
            .Cast<SkipCustomer>().ToArray();

        // Assert: items after (Male, 2): {4:Male, 1:Female, 3:Female}
        Assert.Equal(3, results.Length);
        Assert.Contains(results, c => c.Id == 1);
        Assert.Contains(results, c => c.Id == 3);
        Assert.Contains(results, c => c.Id == 4);
    }

    [Theory]
    [InlineData(HandleNullPropagationOption.False)]
    [InlineData(HandleNullPropagationOption.True)]
    public void ApplyTo_Works_WithDeclaredEnumPropertyOrderByDescending(HandleNullPropagationOption nullPropagation)
    {
        // Gender enum: Male=0, Female=1. Descending: [1:Female, 3:Female, 2:Male, 4:Male]
        // "(Gender eq null)" on a non-nullable enum must not throw during expression tree construction.
        string skipTokenValue = $"Gender-{typeof(Gender).FullName}'Female',Id-1";
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            "http://server/Customers?$orderby=Gender desc,Id asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Gender = Gender.Female },
            new SkipCustomer { Id = 2, Gender = Gender.Male },
            new SkipCustomer { Id = 3, Gender = Gender.Female },
            new SkipCustomer { Id = 4, Gender = Gender.Male },
        }.AsQueryable();

        // Act
        SkipCustomer[] results = new DefaultSkipTokenHandler()
            .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
            .Cast<SkipCustomer>().ToArray();

        // Assert: items after (Female, 1): {3:Female, 2:Male, 4:Male}
        Assert.Equal(3, results.Length);
        Assert.Contains(results, c => c.Id == 3);
        Assert.Contains(results, c => c.Id == 2);
        Assert.Contains(results, c => c.Id == 4);
    }

    [Theory]
    [InlineData(HandleNullPropagationOption.False)]
    [InlineData(HandleNullPropagationOption.True)]
    public void ApplyTo_Throws_WhenSkipTokenContainsUnknownEnumMember_Quoted(HandleNullPropagationOption nullPropagation)
    {
        string skipTokenValue = $"Gender-{typeof(Gender).FullName}'Bogus',Id-1";
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/Customers?$orderby=Gender asc,Id asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Gender = Gender.Female },
        }.AsQueryable();

        Assert.Throws<ODataException>(() =>
            new DefaultSkipTokenHandler()
                .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
                .Cast<SkipCustomer>().ToArray());
    }

    [Theory]
    [InlineData(HandleNullPropagationOption.False)]
    [InlineData(HandleNullPropagationOption.True)]
    public void ApplyTo_Throws_WhenSkipTokenContainsUnknownEnumMember_Unquoted(HandleNullPropagationOption nullPropagation)
    {
        // Raw value has no type-name prefix and no single quotes — treated as a plain member name.
        string skipTokenValue = "Gender-Bogus,Id-1";
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/Customers?$orderby=Gender asc,Id asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Gender = Gender.Female },
        }.AsQueryable();

        Assert.Throws<ODataException>(() =>
            new DefaultSkipTokenHandler()
                .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
                .Cast<SkipCustomer>().ToArray());
    }

    [Fact]
    public void ApplyTo_Throws_WhenSkipTokenContainsEmptyEnumMember()
    {
        string skipTokenValue = $"Gender-{typeof(Gender).FullName}'',Id-1";
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/Customers?$orderby=Gender asc,Id asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Gender = Gender.Female },
        }.AsQueryable();

        Assert.Throws<ODataException>(() =>
            new DefaultSkipTokenHandler()
                .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings(), queryOptions)
                .Cast<SkipCustomer>().ToArray());
    }

    [Fact]
    public void ApplyTo_Throws_WhenSkipTokenContainsUnterminatedQuoteInEnumValue()
    {
        // Gender segment has an opening quote with no closing quote.
        // ParseValue consumes the rest of the string as quoted content, leaving
        // BuildTypedConstantNode to detect the missing closing quote.
        string skipTokenValue = $"Id-1,Gender-{typeof(Gender).FullName}'Male";
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/Customers?$orderby=Id asc,Gender asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Gender = Gender.Female },
        }.AsQueryable();

        Assert.Throws<ODataException>(() =>
            new DefaultSkipTokenHandler()
                .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings(), queryOptions)
                .Cast<SkipCustomer>().ToArray());
    }

    [Theory]
    [InlineData(HandleNullPropagationOption.False)]
    [InlineData(HandleNullPropagationOption.True)]
    public void ApplyTo_Works_WithNumericEnumValue(HandleNullPropagationOption nullPropagation)
    {
        // "1" is the numeric underlying value of Gender.Female. OData allows enum values
        // to be specified by their underlying integer form.
        string skipTokenValue = $"Gender-{typeof(Gender).FullName}'1',Id-1";
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/Customers?$orderby=Gender asc,Id asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Gender = Gender.Female },
            new SkipCustomer { Id = 2, Gender = Gender.Male },
            new SkipCustomer { Id = 3, Gender = Gender.Female },
        }.AsQueryable();

        // Gender asc: Male(0) < Female(1). After (Female=1, Id=1): Id=3 (Female, Id>1)
        SkipCustomer[] results = new DefaultSkipTokenHandler()
            .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
            .Cast<SkipCustomer>().ToArray();

        Assert.Single(results);
        Assert.Equal(3, results[0].Id);
    }

    [Theory]
    [InlineData(HandleNullPropagationOption.False)]
    [InlineData(HandleNullPropagationOption.True)]
    public void ApplyTo_DoesNotThrow_WhenOpenTypeDynamicPropertyAbsentFromSomeEntities(HandleNullPropagationOption nullPropagation)
    {
        // Score is absent for customer 2 — its DynamicProperties dict has no "Score" key.
        // Without nullable coercion in CreateBinaryExpression, the object→int conversion
        // would throw NullReferenceException at query execution time.
        string skipTokenValue = "Score-5,Id-1";
        IEdmModel model = GetOpenEdmModel();
        ODataQueryContext context = new ODataQueryContext(model, typeof(OpenSkipCustomer));
        HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/OpenCustomers?$orderby=Score asc,Id asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        var customers = new List<OpenSkipCustomer>
        {
            new OpenSkipCustomer { Id = 1, DynamicProperties = new Dictionary<string, object> { ["Score"] = 5 } },
            new OpenSkipCustomer { Id = 2, DynamicProperties = new Dictionary<string, object>() },
            new OpenSkipCustomer { Id = 3, DynamicProperties = new Dictionary<string, object> { ["Score"] = 10 } },
        }.AsQueryable();

        // After (Score=5, Id=1): only customer 3 (Score=10) qualifies; customer 2 (null) does not
        OpenSkipCustomer[] results = new DefaultSkipTokenHandler()
            .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
            .Cast<OpenSkipCustomer>().ToArray();

        Assert.Single(results);
        Assert.Equal(3, results[0].Id);
    }

    [Fact]
    public void GenerateSkipTokenValue_UsesEdmMemberName_ForAliasedEnum()
    {
        // Tier.Gold has [EnumMember(Value="Oro")], so the EDM member is named "Oro".
        // GenerateSkipTokenValue must use the EDM name ("Oro"), not the CLR name ("Gold").
        OrderByClause clause = BuildOrderByClause(_modelAliased, typeof(SkipCustomer), "Tier asc,SkipCustomerId asc");

        string token = DefaultSkipTokenHandler.GenerateSkipTokenValue(
            new SkipCustomer { Id = 1, Tier = Tier.Gold }, _modelAliased, clause);

        // The token must contain the EDM alias "Oro", not the CLR name "Gold".
        Assert.Contains("Oro", token);
        Assert.DoesNotContain("Gold", token);
    }

    [Theory]
    [InlineData(HandleNullPropagationOption.False)]
    [InlineData(HandleNullPropagationOption.True)]
    public void ApplyTo_Works_WithAliasedEnumPropertyOrderByAscending(HandleNullPropagationOption nullPropagation)
    {
        // Tier: Gold(1) < Platinum(2). EDM aliases: Oro, Platino.
        // Token uses EDM alias "Oro" for Tier.Gold.
        string skipTokenValue = $"Tier-{GetTierEdmTypeName(_modelAliased)}'Oro',SkipCustomerId-1";
        ODataQueryContext context = new ODataQueryContext(_modelAliased, typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create(HttpMethods.Get,
            "http://server/Customers?$orderby=Tier asc,SkipCustomerId asc");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(skipTokenValue, context);

        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
        {
            new SkipCustomer { Id = 1, Tier = Tier.Gold },
            new SkipCustomer { Id = 2, Tier = Tier.Gold },
            new SkipCustomer { Id = 3, Tier = Tier.Platinum },
            new SkipCustomer { Id = 4, Tier = Tier.Platinum },
        }.AsQueryable();

        // After (Tier=Gold=Oro, Id=1): {2:Gold, 3:Platinum, 4:Platinum}
        SkipCustomer[] results = new DefaultSkipTokenHandler()
            .ApplyTo(customers, skipTokenQuery, new ODataQuerySettings { HandleNullPropagation = nullPropagation }, queryOptions)
            .Cast<SkipCustomer>().ToArray();

        Assert.Equal(3, results.Length);
        Assert.Contains(results, c => c.Id == 2);
        Assert.Contains(results, c => c.Id == 3);
        Assert.Contains(results, c => c.Id == 4);
    }

    private static string GetTierEdmTypeName(IEdmModel model)
    {
        IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
        IEdmStructuralProperty tierProp = entitySet.EntityType
            .StructuralProperties().First(p => p.Name == "Tier");
        return tierProp.Type.AsEnum().EnumDefinition().FullName();
    }

    private static OrderByClause BuildOrderByClause(IEdmModel model, Type clrType, string orderByRaw)
    {
        ODataQueryContext context = new ODataQueryContext(model, clrType);
        return new OrderByQueryOption(orderByRaw, context).OrderByClause;
    }

    private ODataSerializerContext GetSerializerContext(IEdmModel model, bool enableSkipToken = false)
    {
        IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
        IEdmEntityType entityType = entitySet.EntityType;
        IEdmProperty edmProperty = entityType.FindProperty("Name");
        IEdmType edmType = entitySet.Type;
        ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
        ODataQueryContext queryContext = new ODataQueryContext(model, edmType, path);
        queryContext.DefaultQueryConfigurations.EnableSkipToken = enableSkipToken;

        var request = RequestFactory.Create(opt => opt.AddRouteComponents(model));
        ResourceContext resource = new ResourceContext();
        ODataSerializerContext context = new ODataSerializerContext(resource, edmProperty, queryContext, null)
        {
            Model = model,
            Request = request
        };

        return context;
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder {ModelAliasingEnabled = false};
        builder.EntitySet<SkipCustomer>("Customers");
        return builder.GetEdmModel();
    }

    private static IEdmModel GetEdmModelLowerCamelCased()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder {ModelAliasingEnabled = false};
        builder.EntitySet<SkipCustomer>("Customers");
        builder.EnableLowerCamelCase();
        return builder.GetEdmModel();
    }

    private static IEdmModel GetEdmModelAliased()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder {ModelAliasingEnabled = true};
        var entitySetConfiguration = builder.EntitySet<SkipCustomer>("Customers");
        return builder.GetEdmModel();
    }

    private static IEdmModel GetOpenEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<OpenSkipCustomer>("OpenCustomers");
        return builder.GetEdmModel();
    }

    private void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_Implementation(
        string baseUri,
        string expectedUri,
        IEdmModel edmModel)
    {
        // Arrange
        ODataSerializerContext serializerContext = this.GetSerializerContext(
            edmModel,
            true);
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        SkipCustomer instance = new SkipCustomer {Id = 42, Name = "ZX"};

        // Act
        Uri uri = handler.GenerateNextPageLink(
            new Uri(baseUri),
            10,
            instance,
            serializerContext);
        var actualUri = uri.ToString();

        // Assert
        Assert.Equal(
            expectedUri,
            actualUri);
    }
        
    private static void GenerateSkipTokenValue_Returns_SkipTokenValue_Implementation(IEdmModel edmModel, string expectedSkipToken)
    {
        // Arrange
        SkipCustomer lastMember = new SkipCustomer {Id = 42, Name = "ZX"};

        // Act
        string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
            lastMember,
            edmModel,
            null
            );

        // Assert
        Assert.Equal(
            expectedSkipToken,
            skipTokenValue);
    }

    private static void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_Implementation(
        IEdmModel edmModel,
        string propertyName,
        string expectedSkipToken)
    {
        // Arrange
        SkipCustomer lastMember = new SkipCustomer {Id = 42, Name = "ZX"};

        IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
            .First(c => c.Name == "SkipCustomer");
        IEdmProperty property = entityType.FindProperty(propertyName);

        OrderByClause clause = BuildOrderByClause(edmModel, propertyName);

        // Act
        string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
            lastMember,
            edmModel,
            clause);

        // Assert
        Assert.Equal(
            expectedSkipToken,
            skipTokenValue);
    }

    private static OrderByClause BuildOrderByClause(IEdmModel edmModel, string propertyName)
    {
        IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
            .First(c => c.Name == "SkipCustomer");
        IEdmProperty property = entityType.FindProperty(propertyName);

        IEdmNavigationSource entitySet = edmModel.FindDeclaredEntitySet("Customers");
        ResourceRangeVariable rangeVariable = new ResourceRangeVariable("$it", new EdmEntityTypeReference(entityType, true), entitySet);

        ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);
        SingleValuePropertyAccessNode node = new SingleValuePropertyAccessNode(source, property);
        return new OrderByClause(null, node, OrderByDirection.Ascending, rangeVariable);
    }

    private static void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue_Implementation(
        IEdmModel edmModel,
        string propertyName,
        string expectedSkipToken)
    {
        // Arrange
        SkipCustomer lastMember = new SkipCustomer {Id = 42, Name = null};

        OrderByClause clause = BuildOrderByClause(edmModel, propertyName);

        // Act
        string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
            lastMember,
            edmModel,
            clause);

        // Assert
        Assert.Equal(
            expectedSkipToken,
            skipTokenValue);
    }

    private static void GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset_Implementation(
        IEdmModel edmModel,
        string propertyName,
        string expectedSkipToken)
    {
        // Arrange
        SkipCustomer lastMember = new SkipCustomer
            {
                Id = 42,
                Birthday = new DateTime(
                    2021,
                    01,
                    20,
                    3,
                    4,
                    5,
                    DateTimeKind.Utc)
            };

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8
        IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
            .First(c => c.Name == "SkipCustomer");

        OrderByClause clause = BuildOrderByClause(edmModel, propertyName);
        ODataSerializerContext context = new ODataSerializerContext()
        {
            TimeZone = timeZone
        };

        // Act
        string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
            lastMember,
            edmModel,
            clause,
            context);

        // Assert
        Assert.Equal(
            expectedSkipToken,
            skipTokenValue);
    }
        
    private static void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue_Implementation(
        IEdmModel edmModel,
        string propertyName,
        string expectedSkipToken)
    {
        // Arrange
        SkipCustomer lastMember = new SkipCustomer {Id = 42, Name = "ZX", Gender = Gender.Male};

        OrderByClause clause = BuildOrderByClause(edmModel, propertyName);

        // Act
        string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
            lastMember,
            edmModel,
            clause);

        // Assert
        Assert.Equal(
            expectedSkipToken,
            skipTokenValue);
    }

    private static void ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue_Implementation(
        IEdmModel edmModel)
    {
        // Arrange
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        ODataQuerySettings settings = new ODataQuerySettings {HandleNullPropagation = HandleNullPropagationOption.False};
        ODataQueryContext context = new ODataQueryContext(
            edmModel,
            typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(
            "abc",
            context);
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>().AsQueryable();

        // Act & Assert
        // The value "abc" is not a valid typed literal for the key property (int32), so type
        // validation now rejects it before the filter is even built.
        ExceptionAssert.Throws<ODataException>(
            () => handler.ApplyTo(
                customers,
                skipTokenQuery,
                new ODataQuerySettings(),
                queryOptions),
            "Unable to parse the skiptoken value 'abc'. Skiptoken value should always be server generated.");
    }
        
    private static void ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable_Implementation(
        IEdmModel edmModel,
        string skipTokenQueryOptionRawValue)
    {
        // Arrange
        ODataQuerySettings settings = new ODataQuerySettings {HandleNullPropagation = HandleNullPropagationOption.False};
        ODataQueryContext context = new ODataQueryContext(
            edmModel,
            typeof(SkipCustomer));
        HttpRequest request = RequestFactory.Create("Get", "http://localhost/");
        ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(
            skipTokenQueryOptionRawValue,
            context);
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
            {
                new SkipCustomer {Id = 2, Name = "Aaron"},
                new SkipCustomer {Id = 1, Name = "Andy"},
                new SkipCustomer {Id = 3, Name = "Alex"}
            }.AsQueryable();

        // Act
        SkipCustomer[] results = handler.ApplyTo(
                customers,
                skipTokenQuery,
                settings,
                queryOptions)
            .ToArray();

        // Assert
        SkipCustomer skipTokenCustomer = Assert.Single(results);
        Assert.Equal(
            3,
            skipTokenCustomer.Id);
        Assert.Equal(
            "Alex",
            skipTokenCustomer.Name);
    }

    private static void ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable_Implementation(
        IEdmModel edmModel,
        string orderByPropertyName,
        string skipTokenQueryOptionRawValue)
    {
        // Arrange
        ODataQuerySettings settings = new ODataQuerySettings {HandleNullPropagation = HandleNullPropagationOption.False};
        ODataQueryContext context = new ODataQueryContext(
            edmModel,
            typeof(SkipCustomer));

        HttpRequest request = RequestFactory.Create(
            HttpMethods.Get,
            $"http://server/service/Customers/?$orderby={orderByPropertyName} desc&$skiptoken={skipTokenQueryOptionRawValue}");

        // Act
        ODataQueryOptions oDataQueryOptions = new ODataQueryOptions(context, request);

        SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(
            skipTokenQueryOptionRawValue,
            context);
        DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
        IQueryable<SkipCustomer> customers = new List<SkipCustomer>
            {
                new SkipCustomer {Id = 2, Name = "Aaron"},
                new SkipCustomer {Id = 1, Name = "Andy"},
                new SkipCustomer {Id = 3, Name = "Alex"}
            }.AsQueryable();

        // Act
        SkipCustomer[] results = handler.ApplyTo(
                customers,
                skipTokenQuery,
                settings,
                oDataQueryOptions)
            .ToArray();

        // Assert
        SkipCustomer skipTokenCustomer = Assert.Single(results);
        Assert.Equal(
            2,
            skipTokenCustomer.Id);
        Assert.Equal(
            "Aaron",
            skipTokenCustomer.Name);
    }

    // ---- Tests for properties excluded from the EDM model ----

    private static IEdmModel _openModelWithIgnoredProp = GetOpenEdmModelWithIgnoredProp();

    [Fact]
    public void GenerateSkipTokenValue_DoesNotInclude_IgnoredCLRProperty_WhenOrderedByUndeclaredOpenProperty()
    {
        // Arrange: open entity type where ExcludedToken is omitted from the EDM model via .Ignore().
        // Ordering by an undeclared open property name must not include its CLR value in the skip token.
        IgnoredPropertyCustomer lastMember = new IgnoredPropertyCustomer
        {
            Id = 42,
            Name = "Alice",
            ExcludedToken = "clr-only-value",
            DynamicProperties = new System.Collections.Generic.Dictionary<string, object>()
        };

        OrderByClause clause = BuildOpenPropertyOrderByClause(_openModelWithIgnoredProp, "ExcludedToken", "Customers");

        // Act
        string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(lastMember, _openModelWithIgnoredProp, clause);

        // Assert: the CLR value of the excluded property must not appear in the skip token
        Assert.NotNull(skipTokenValue);
        Assert.DoesNotContain("clr-only-value", skipTokenValue, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateSkipTokenValue_StillWorks_ForDeclaredProperty_WhenUndeclaredPropertyIsIgnored()
    {
        // Regression guard: the fix must not break skip token generation for declared EDM properties.
        IgnoredPropertyCustomer lastMember = new IgnoredPropertyCustomer { Id = 7, Name = "Bob" };

        OrderByClause clause = BuildDeclaredPropertyOrderByClause(_openModelWithIgnoredProp, "Name", "Customers");

        // Act
        string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(lastMember, _openModelWithIgnoredProp, clause);

        // Assert: skip token contains the declared Name value followed by the key
        Assert.Equal("Name-%27Bob%27,Id-7", skipTokenValue);
    }

    private static OrderByClause BuildOpenPropertyOrderByClause(IEdmModel edmModel, string openPropertyName, string entitySetName)
    {
        IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
            .First(c => c.Name == "IgnoredPropertyCustomer");
        IEdmNavigationSource entitySet = edmModel.FindDeclaredEntitySet(entitySetName);
        ResourceRangeVariable rangeVariable = new ResourceRangeVariable(
            "$it", new EdmEntityTypeReference(entityType, true), entitySet);
        ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);

        // This is the node the OData URI parser emits for an undeclared property name on an open type
        SingleValueOpenPropertyAccessNode openNode = new SingleValueOpenPropertyAccessNode(source, openPropertyName);
        return new OrderByClause(null, openNode, OrderByDirection.Ascending, rangeVariable);
    }

    private static OrderByClause BuildDeclaredPropertyOrderByClause(IEdmModel edmModel, string propertyName, string entitySetName)
    {
        IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
            .First(c => c.Name == "IgnoredPropertyCustomer");
        IEdmProperty property = entityType.FindProperty(propertyName);
        IEdmNavigationSource entitySet = edmModel.FindDeclaredEntitySet(entitySetName);
        ResourceRangeVariable rangeVariable = new ResourceRangeVariable(
            "$it", new EdmEntityTypeReference(entityType, true), entitySet);
        ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);
        SingleValuePropertyAccessNode node = new SingleValuePropertyAccessNode(source, property);
        return new OrderByClause(null, node, OrderByDirection.Ascending, rangeVariable);
    }

    private static IEdmModel GetOpenEdmModelWithIgnoredProp()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder { ModelAliasingEnabled = false };
        var entitySet = builder.EntitySet<IgnoredPropertyCustomer>("Customers");
        entitySet.EntityType.Ignore(c => c.ExcludedToken); // omitted from the EDM model
        return builder.GetEdmModel();
    }

    public class IgnoredPropertyCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
        // Omitted from the EDM model via .Ignore()
        public string ExcludedToken { get; set; }
    }

    [DataContract]
    public class SkipCustomer
    {
        [DataMember(Name = "SkipCustomerId")]
        public int Id { get; set; }

        [DataMember(Name = "FirstAndLastName")]
        public string Name { get; set; }

        [DataMember(Name = "DateOfBirth")]
        public DateTime Birthday { get; set; }

        [DataMember(Name = "MaleOrFemale")]
        public Gender Gender { get; set; }

        [DataMember(Name = "Tier")]
        public Tier Tier { get; set; }
    }

    public enum Gender
    {
        Male,

        Female
    }

    // Enum whose member names differ from their [EnumMember] aliases.
    [DataContract]
    public enum Tier
    {
        [EnumMember(Value = "Oro")]
        Gold = 1,

        [EnumMember(Value = "Platino")]
        Platinum = 2,
    }

    public class OpenSkipCustomer
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool? IsVerified { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }
}
