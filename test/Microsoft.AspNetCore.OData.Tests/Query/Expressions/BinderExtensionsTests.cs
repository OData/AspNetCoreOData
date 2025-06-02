//-----------------------------------------------------------------------------
// <copyright file="BinderExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions;

public class BinderExtensionsTests
{
    private static IEdmModel _model;
    private static IEdmEntitySet _products;
    private static IEdmStructuredTypeReference _productTypeReference;

    static BinderExtensionsTests()
    {
        _model = GetEdmModel();

        IEdmEntityType productType = _model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == "Product");
        Assert.NotNull(productType); // Guard

        _products = _model.EntityContainer.FindEntitySet("Products");
        Assert.NotNull(_products); // Guard

        _productTypeReference = new EdmEntityTypeReference(productType, true);
    }


    private static readonly ODataQuerySettings _defaultSettings = new ODataQuerySettings
    {
        HandleNullPropagation = HandleNullPropagationOption.False
    };

    private static readonly ODataQuerySettings _defaultSettingsTrue = new ODataQuerySettings
    {
        HandleNullPropagation = HandleNullPropagationOption.True
    };

    [Fact]
    public void ApplyBind_OnIFilterBinder_WithEnumerable_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        IFilterBinder binder = null;
        IEnumerable query = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "binder");

        // Arrange & Act & Assert
        binder = new Mock<IFilterBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "query");

        // Arrange & Act & Assert
        query = new Mock<IEnumerable>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "filterClause");

        // Arrange & Act & Assert
        FilterClause filterClause = new FilterClause(new Mock<SingleValueNode>().Object, new Mock<RangeVariable>().Object);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, filterClause, null), "context");
    }

    [Fact]
    public void ApplyBind_OnIFilterBinder_WithQueryable_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        IFilterBinder binder = null;
        IQueryable query = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "binder");

        // Arrange & Act & Assert
        binder = new Mock<IFilterBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "query");

        // Arrange & Act & Assert
        query = new Mock<IQueryable>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "filterClause");

        // Arrange & Act & Assert
        FilterClause filterClause = new FilterClause(new Mock<SingleValueNode>().Object, new Mock<RangeVariable>().Object);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, filterClause, null), "context");
    }

    [Fact]
    public void ApplyBind_OnIFilterBinder_WithExpression_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        IFilterBinder binder = null;
        Expression source = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "binder");

        // Arrange & Act & Assert
        binder = new Mock<IFilterBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "source");

        // Arrange & Act & Assert
        source = new Mock<Expression>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "filterClause");

        // Arrange & Act & Assert
        FilterClause filterClause = new FilterClause(new Mock<SingleValueNode>().Object, new Mock<RangeVariable>().Object);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, filterClause, null), "context");
    }

    [Fact]
    public void ApplyBind_OnIOrderByBinder_WithQueryable_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        IOrderByBinder binder = null;
        IQueryable query = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null, true), "binder");

        // Arrange & Act & Assert
        binder = new Mock<IOrderByBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null, true), "query");

        // Arrange & Act & Assert
        query = new Mock<IQueryable>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null, true), "orderByClause");

        // Arrange & Act & Assert
        OrderByClause orderByClause = new OrderByClause(null, new Mock<SingleValueNode>().Object, OrderByDirection.Descending, new Mock<RangeVariable>().Object);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, orderByClause, null, true), "context");
    }

    [Fact]
    public void ApplyBind_OnIOrderByBinder_WithExpression_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        IOrderByBinder binder = null;
        Expression source = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null, true), "binder");

        // Arrange & Act & Assert
        binder = new Mock<IOrderByBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null, true), "source");

        // Arrange & Act & Assert
        source = new Mock<Expression>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null, true), "orderByClause");

        // Arrange & Act & Assert
        OrderByClause orderByClause = new OrderByClause(null, new Mock<SingleValueNode>().Object, OrderByDirection.Descending, new Mock<RangeVariable>().Object);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, orderByClause, null, true), "context");
    }

    [Fact]
    public void ApplyBind_OnISelectExpandBinder_WithQueryable_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        ISelectExpandBinder binder = null;
        IQueryable source = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "binder");

        // Arrange & Act & Assert
        binder = new Mock<ISelectExpandBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "source");

        // Arrange & Act & Assert
        source = new Mock<IQueryable>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "selectExpandClause");

        // Arrange & Act & Assert
        SelectExpandClause selectExpandClause = new SelectExpandClause(null, true);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, selectExpandClause, null), "context");
    }

    [Fact]
    public void ApplyBind_OnISelectExpandBinder_WithObject_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        ISelectExpandBinder binder = null;
        object source = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "binder");

        // Arrange & Act & Assert
        binder = new Mock<ISelectExpandBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "source");

        // Arrange & Act & Assert
        source = new object();
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "selectExpandClause");

        // Arrange & Act & Assert
        SelectExpandClause selectExpandClause = new SelectExpandClause(null, true);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, selectExpandClause, null), "context");
    }

    [Fact]
    public void ApplyBind_OnISearchBinder_WithExpression_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        ISearchBinder binder = null;
        Expression source = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "binder");

        // Arrange & Act & Assert
        binder = new Mock<ISearchBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "source");

        // Arrange & Act & Assert
        source = new Mock<Expression>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, null, null), "searchClause");

        // Arrange & Act & Assert
        SearchClause searchClause = new SearchClause(new Mock<SingleValueNode>().Object);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(source, searchClause, null), "context");
    }

    [Fact]
    public void ApplyBind_OnISearchBinder_WithQueryable_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        ISearchBinder binder = null;
        IQueryable query = null;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "binder");

        // Arrange & Act & Assert
        binder = new Mock<ISearchBinder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "source");

        // Arrange & Act & Assert
        query = new Mock<IQueryable>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, null, null), "searchClause");

        // Arrange & Act & Assert
        SearchClause searchClause = new SearchClause(new Mock<SingleValueNode>().Object);
        ExceptionAssert.ThrowsArgumentNull(() => binder.ApplyBind(query, searchClause, null), "context");
    }

    [Fact]
    public void SearchBinder_ApplyBind_WorksForQueryable()
    {
        // Arrange
        IQueryable<Product> products = new List<Product>().AsQueryable();
        QueryBinderContext context = new QueryBinderContext(_model, _defaultSettings, typeof(Product));

        // Act & Assert
        Mock<ISearchBinder> binder = new Mock<ISearchBinder>();
        SearchClause searchClause = new SearchClause(new Mock<SingleValueNode>().Object);
        Expression body = Expression.Constant(true);
        ParameterExpression searchParameter = context.CurrentParameter;
        LambdaExpression searchExpr = Expression.Lambda(body, searchParameter);

        binder.Setup(b => b.BindSearch(searchClause, context)).Returns(searchExpr);

        // Act
        Expression result = binder.Object.ApplyBind(Expression.Constant(products), searchClause, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("System.Collections.Generic.List`1[Microsoft.AspNetCore.OData.Tests.Models.Product].Where($it => True)", result.ToString());
    }

    [Fact]
    public void FilterBinder_ApplyBind_WorksForQueryable()
    {
        // Arrange
        IQueryable<Product> products = new List<Product>().AsQueryable();
        QueryBinderContext context = new QueryBinderContext(_model, _defaultSettings, typeof(Product));

        // Act & Assert
        Mock<IFilterBinder> binder = new Mock<IFilterBinder>();
        FilterClause filterClause = new FilterClause(new Mock<SingleValueNode>().Object, new Mock<RangeVariable>().Object);
        Expression body = Expression.Constant(true);
        ParameterExpression filterParameter = context.CurrentParameter;
        LambdaExpression filterExpr = Expression.Lambda(body, filterParameter);

        binder.Setup(b => b.BindFilter(filterClause, context)).Returns(filterExpr);

        // Act
        Expression result = binder.Object.ApplyBind(Expression.Constant(products), filterClause, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("System.Collections.Generic.List`1[Microsoft.AspNetCore.OData.Tests.Models.Product].Where($it => True)", result.ToString());
    }

    [Fact]
    public void FilterBinder_ApplyBind_WorksForEnumerable()
    {
        // Arrange
        IEnumerable<Product> products = new[]
        {
            new Product { ProductID = 1, ProductName = "abc" },
            new Product { ProductID = 2, ProductName = null },
            new Product { ProductID = 3, ProductName = "xyz" },
        };

        // Act & Assert
        RunFilterTestAndVerify(products, "ProductName eq null", new[] { 2 }, "$it => ($it.ProductName == null)");

        // Act & Assert
        RunFilterTestAndVerify(products, "ProductName ne null", new[] { 1, 3 }, "$it => ($it.ProductName != null)");
    }

    [Fact]
    public void BindCountNode_Works()
    {
        // Arrange
        IEnumerable<Product> products = new[]
        {
            new Product
            {
                ProductID = 1,
                AlternateAddresses = new Address[] // has 2 address items whose HourseNumber > 8
                {
                    new Address { HouseNumber = 6 },
                    new Address { HouseNumber = 9 }, //  > 8
                    new Address { HouseNumber = 10 }, // > 8
                    new Address { HouseNumber = 3 },
                    new Address { HouseNumber = 1 },
                }
            },
            new Product
            {
                ProductID = 2,
                AlternateAddresses = new Address[] // only 1 address item whose HourseNumber > 8
                {
                    new Address { HouseNumber = 5 },
                    new Address { HouseNumber = 6 },
                    new Address { HouseNumber = 9 }, // > 8
                    new Address { HouseNumber = 3 },
                }
            },
            new Product
            {
                ProductID = 3,
                AlternateAddresses = new Address[] // has 3 address items whose HourseNumber > 8
                {
                    new Address { HouseNumber = 10 },
                    new Address { HouseNumber = 11 },
                    new Address { HouseNumber = 9 },
                }
            },
        };

        // Act & Assert
        string filter = "AlternateAddresses/$count eq 5";
        string expectedExpr = "$it => ($it.AlternateAddresses.LongCount() == 5)";
        RunFilterTestAndVerify(products, filter, new[] { 1 }, expectedExpr);

        // Act & Assert
        filter = "AlternateAddresses/$count in [3,4]";
        expectedExpr = "$it => System.Collections.Generic.List`1[System.Int64].Contains($it.AlternateAddresses.LongCount())";
        RunFilterTestAndVerify(products, filter, new[] { 2, 3 }, expectedExpr);

        // Act & Assert
        filter = "AlternateAddresses/$count($filter=HouseNumber gt 8) gt 2";
        expectedExpr = "$it => ($it.AlternateAddresses.Where($it => ($it.HouseNumber > 8)).LongCount() > 2)";
        RunFilterTestAndVerify(products, filter, new[] { 3 }, expectedExpr);
    }

    private static Expression BindFilter<T>(string filter, IEdmModel model, ODataQuerySettings querySettings = null, IAssemblyResolver assembliesResolver = null)
    {
        Type elementType = typeof(T);
        FilterClause orderByClause = CreateFilterClause(filter, model, elementType);
        Assert.NotNull(orderByClause);

        querySettings = querySettings ?? _defaultSettings;
        QueryBinderContext context = new QueryBinderContext(model, querySettings, elementType)
        {
            AssembliesResolver = assembliesResolver,
        };

        IFilterBinder filterBinder = new FilterBinder();
        return filterBinder.BindFilter(orderByClause, context);
    }

    private void RunFilterTestAndVerify(IEnumerable<Product> products, string filter, int[] expectedIds, string expectedExpr, ODataQuerySettings querySettings = null, IAssemblyResolver assembliesResolver = null)
    {
        // Act - Bind string to Linq.Expression
        Expression filterExpr = BindFilter<Product>(filter, _model, querySettings, assembliesResolver);

        // Assert
        string actualExpr = ExpressionStringBuilder.ToString(filterExpr);
        Assert.Equal(expectedExpr, actualExpr);

        // Act
        IEnumerable<Product> results = InvokeFilter(products, filterExpr);

        // Assert
        Assert.True(expectedIds.SequenceEqual(results.Select(a => a.ProductID))); // ordered id
    }

    public static IEnumerable<T> InvokeFilter<T>(IEnumerable<T> collection, Expression filterExpr)
    {
        LambdaExpression filterLambda = filterExpr as LambdaExpression;
        Assert.NotNull(filterLambda);

        Type type = typeof(T);

        Delegate function = filterLambda.Compile();

        MethodInfo whereMethod = ExpressionHelperMethods.EnumerableWhereGeneric.MakeGenericMethod(type);
        return whereMethod.Invoke(null, new object[] { collection, function }) as IEnumerable<T>;
    }

    private static FilterClause CreateFilterClause(string filter, IEdmModel model, Type type)
    {
        IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == type.Name);
        Assert.NotNull(entityType); // Guard

        IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Products");
        Assert.NotNull(entitySet); // Guard

        ODataQueryOptionParser parser = new ODataQueryOptionParser(model, entityType, entitySet,
            new Dictionary<string, string> { { "$filter", filter } });

        return parser.ParseFilter();
    }

    private static IEdmModel GetEdmModel()
    {
        ODataModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<Product>("Products");
        builder.EntityType<DerivedProduct>().DerivesFrom<Product>();
        builder.EntityType<DerivedCategory>().DerivesFrom<Category>();
        return builder.GetEdmModel();
    }
}
