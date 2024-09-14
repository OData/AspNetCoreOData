//-----------------------------------------------------------------------------
// <copyright file="SearchQueryOptionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class SearchQueryOptionTests
{
    private static IEdmModel _model = GetModel();

    [Fact]
    public void CtorSearchQueryOption_ThrowsArgumentNull_ForInputParameter()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNullOrEmpty(() => new SearchQueryOption(null, null, null), "rawValue");
        ExceptionAssert.ThrowsArgumentNullOrEmpty(() => new SearchQueryOption(string.Empty, null, null), "rawValue");

        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new SearchQueryOption("any", null, null), "context");

        // Arrange & Act & Assert
        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        ExceptionAssert.ThrowsArgumentNull(() => new SearchQueryOption("any", context, null), "queryOptionParser");
    }

    [Fact]
    public void CtorSearchQueryOption_CanConstructValidSearchQuery()
    {
        // Arrange
        ODataQueryContext context = new ODataQueryContext(_model, typeof(int));

        // Act
        SearchQueryOption search = new SearchQueryOption("any", context);

        // Assert
        Assert.Same(context, search.Context);
        Assert.Equal("any", search.RawValue);
    }

    [Fact]
    public void CtorSearchQueryOption_GetQueryNodeParsesQuery()
    {
        // Arrange
        ODataQueryContext context = new ODataQueryContext(_model, typeof(int)) { RequestContainer = new MockServiceProvider() };

        // Act
        SearchQueryOption search = new SearchQueryOption("any", context);
        SearchClause searchClause = search.SearchClause;

        // Assert
        Assert.NotNull(searchClause);

        SearchTermNode searchTermNode = Assert.IsType<SearchTermNode>(searchClause.Expression);
        Assert.Equal(QueryNodeKind.SearchTerm, searchTermNode.Kind);
        Assert.Equal("any", searchTermNode.Text);
    }

    [Fact]
    public void SearchQueryOption_ApplyTo_Throws_Null_Query()
    {
        // Arrange
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SearchProduct)) { RequestContainer = new MockServiceProvider() };
        SearchQueryOption search = new SearchQueryOption("any", context);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => search.ApplyTo(null, new ODataQuerySettings()), "query");
    }

    [Fact]
    public void SearchQueryOption_ApplyTo_Throws_Null_QuerySettings()
    {
        // Arrange
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SearchProduct)) { RequestContainer = new MockServiceProvider() };
        SearchQueryOption search = new SearchQueryOption("any", context);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => search.ApplyTo(new SearchProduct[0].AsQueryable(), null), "querySettings");
    }

    [Fact]
    public void SearchQueryOption_ApplyTo_ReturnsOrigialQuery_WithoutISearchBinder()
    {
        // Arrange
        ODataQueryContext context = new ODataQueryContext(_model, typeof(SearchProduct)) { RequestContainer = new MockServiceProvider() };

        SearchQueryOption search = new SearchQueryOption("food", context);

        var searchProducts = (new List<SearchProduct>
        {
            new SearchProduct { Id = 1, Category = "food" },
            new SearchProduct { Id = 2, Category = "non-food" },
            new SearchProduct { Id = 3, Category = "food" }
        }).AsQueryable();

        // Act
        var results = search.ApplyTo(searchProducts, new ODataQuerySettings()).Cast<SearchProduct>();

        // Arrange
        Assert.Equal(3, results.Count());
    }

    [Fact]
    public void SearchQueryOption_ApplyTo_ReturnsOrigialQuery_WithISearchBinder()
    {
        // Arrange
        IServiceProvider serviceProvider = new MockServiceProvider(
            a => a.AddSingleton<ISearchBinder, TestSearchBinder>());

        ODataQueryContext context = new ODataQueryContext(_model, typeof(SearchProduct)) { RequestContainer = serviceProvider };

        SearchQueryOption search = new SearchQueryOption("food", context);

        var searchProducts = (new List<SearchProduct>
        {
            new SearchProduct { Id = 1, Category = "food" },
            new SearchProduct { Id = 2, Category = "non-food" },
            new SearchProduct { Id = 3, Category = "food" }
        }).AsQueryable();

        // Act
        var results = search.ApplyTo(searchProducts, new ODataQuerySettings()).Cast<SearchProduct>();

        // Arrange
        Assert.Equal(2, results.Count());

        Assert.Collection(results,
            e =>
            {
                Assert.Equal(1, e.Id);
                Assert.Equal("food", e.Category);
            },
            e =>
            {
                Assert.Equal(3, e.Id);
                Assert.Equal("food", e.Category);
            });
    }

    private static IEdmModel GetModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<SearchProduct>("Products");
        return builder.GetEdmModel();
    }
}

public class SearchProduct
{
    public int Id { get; set; }

    public string Category { get; set; }
}

public class TestSearchBinder : ISearchBinder
{
    public Expression BindSearch(SearchClause searchClause, QueryBinderContext context)
    {
        SearchTermNode node = searchClause.Expression as SearchTermNode;
        if (node != null)
        {
            Expression<Func<SearchProduct, bool>> exp = p => p.Category == node.Text;
            return exp;
        }

        throw new NotImplementedException();
    }
}
