//-----------------------------------------------------------------------------
// <copyright file="AggregationBinderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions;

public class AggregationBinderTests
{
    private static readonly Uri _serviceBaseUri = new Uri("http://server/service/");

    private static Dictionary<Type, IEdmModel> _modelCache = new Dictionary<Type, IEdmModel>();

    [Fact]
    public void SingleGroupBy()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((ProductName))",
            ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = $it.ProductName, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Fact]
    public void MultipleGroupBy()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((ProductName, SupplierID))",
            ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new AggregationPropertyContainer() {Name = SupplierID, Value = Convert($it.SupplierID), Next = new LastInChain() {Name = ProductName, Value = $it.ProductName, }, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Fact]
    public void NavigationGroupBy()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((Category/CategoryName))",
            ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = CategoryName, Value = $it.Category.CategoryName, }, }, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Fact]
    public void NestedNavigationGroupBy()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((Category/Product/ProductName))",
            ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Product, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = $it.Category.Product.ProductName, }, }, }, }, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Fact]
    public void NavigationMultipleGroupBy()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((Category/CategoryName, SupplierAddress/State))",
            ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedProperty() {Name = SupplierAddress, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = State, Value = $it.SupplierAddress.State, }, }, Next = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = CategoryName, Value = $it.Category.CategoryName, }, }, }, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Fact]
    public void NestedNavigationMultipleGroupBy()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((Category/Product/ProductName, Category/Product/UnitPrice))",
            ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Product, NestedValue = new GroupByWrapper() {GroupByContainer = new AggregationPropertyContainer() {Name = UnitPrice, Value = Convert($it.Category.Product.UnitPrice), Next = new LastInChain() {Name = ProductName, Value = $it.Category.Product.ProductName, }, }, }, }, }, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Fact]
    public void SingleDynamicGroupBy()
    {
        var filters = VerifyQueryDeserialization<DynamicProduct>(
            "groupby((ProductProperty))",
            ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductProperty, Value = IIF($it.ProductProperties.ContainsKey(ProductProperty), $it.ProductPropertiesProductProperty, null), }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }


    [Fact]
    public void SingleSum()
    {
        var filters = VerifyQueryDeserialization(
            "aggregate(SupplierID with sum as SupplierID)",
            ".GroupBy($it => new NoGroupByWrapper())"
            + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = SupplierID, Value = Convert(Convert($it).Sum($it => $it.SupplierID)), }, })");
    }

    [Fact]
    public void SingleDynamicSum()
    {
        var filters = VerifyQueryDeserialization<DynamicProduct>(
            "aggregate(ProductProperty with sum as ProductProperty)",
            ".GroupBy($it => new NoGroupByWrapper())"
            + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = ProductProperty, Value = Convert(Convert($it).Sum($it => IIF($it.ProductProperties.ContainsKey(ProductProperty), $it.ProductPropertiesProductProperty, null).SafeConvertToDecimal())), }, })");
    }

    [Fact]
    public void SingleMin()
    {
        var filters = VerifyQueryDeserialization(
            "aggregate(SupplierID with min as SupplierID)",
            ".GroupBy($it => new NoGroupByWrapper())"
            + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = SupplierID, Value = Convert(Convert($it).Min($it => $it.SupplierID)), }, })");
    }

    [Fact]
    public void SingleDynamicMin()
    {
        var filters = VerifyQueryDeserialization<DynamicProduct>(
            "aggregate(ProductProperty with min as MinProductProperty)",
            ".GroupBy($it => new NoGroupByWrapper())"
            + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = MinProductProperty, Value = Convert($it).Min($it => IIF($it.ProductProperties.ContainsKey(ProductProperty), $it.ProductPropertiesProductProperty, null)), }, })");
    }

    [Fact]
    public void SingleMax()
    {
        var filters = VerifyQueryDeserialization(
            "aggregate(SupplierID with max as SupplierID)",
            ".GroupBy($it => new NoGroupByWrapper())"
            + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = SupplierID, Value = Convert(Convert($it).Max($it => $it.SupplierID)), }, })");
    }

    [Fact]
    public void SingleAverage()
    {
        var filters = VerifyQueryDeserialization(
            "aggregate(UnitPrice with average as AvgUnitPrice)",
            ".GroupBy($it => new NoGroupByWrapper())"
            + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = AvgUnitPrice, Value = Convert(Convert($it).Average($it => $it.UnitPrice)), }, })");
    }

    [Fact]
    public void SingleCountDistinct()
    {
        var filters = VerifyQueryDeserialization(
            "aggregate(SupplierID with countdistinct as Count)",
            ".GroupBy($it => new NoGroupByWrapper())"
            + ".Select($it => new NoGroupByAggregationWrapper() {Container = new LastInChain() {Name = Count, Value = Convert(Convert($it).Select($it => $it.SupplierID).Distinct().LongCount()), }, })");
    }

    [Fact]
    public void MultipleAggregate()
    {
        var filters = VerifyQueryDeserialization(
            "aggregate(SupplierID with sum as SupplierID, CategoryID with sum as CategoryID)",
            ".GroupBy($it => new NoGroupByWrapper())"
            + ".Select($it => new NoGroupByAggregationWrapper() {Container = new AggregationPropertyContainer() {Name = CategoryID, Value = Convert(Convert($it).Sum($it => $it.CategoryID)), Next = new LastInChain() {Name = SupplierID, Value = Convert(Convert($it).Sum($it => $it.SupplierID)), }, }, })");
    }

    [Fact]
    public void GroupByAndAggregate()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((ProductName), aggregate(SupplierID with sum as SupplierID))",
            ".Select($it => new FlatteningWrapper`1() {Source = $it, GroupByContainer = new LastInChain() {Name = Property0, Value = Convert($it.SupplierID), }, })"
            + ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = $it.Source.ProductName, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, Container = new LastInChain() {Name = SupplierID, Value = Convert(Convert($it).Sum($it => Convert(Convert($it.GroupByContainer).Value))), }, })");
    }

    [Fact]
    public void GroupByAndMultipleAggregations()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((ProductName), aggregate(SupplierID with sum as SupplierID, CategoryID with sum as CategoryID))",
            ".Select($it => new FlatteningWrapper`1() {Source = $it, GroupByContainer = new AggregationPropertyContainer() {Name = Property1, Value = Convert($it.SupplierID), Next = new LastInChain() {Name = Property0, Value = Convert($it.CategoryID), }, }, })"
            + ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = $it.Source.ProductName, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, Container = new AggregationPropertyContainer() {Name = CategoryID, Value = Convert(Convert($it).Sum($it => Convert(Convert($it.GroupByContainer.Next).Value))), Next = new LastInChain() {Name = SupplierID, Value = Convert(Convert($it).Sum($it => Convert(Convert($it.GroupByContainer).Value))), }, }, })");
    }

    [Fact]
    public void NSCast_OnSingleEntity_GeneratesExpression_WithAsOperator()
    {
        var filters = VerifyQueryDeserialization(
            "groupby((Microsoft.AspNetCore.OData.Tests.Models.Product/ProductName))",
            ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = ($it As Product).ProductName, }, })"
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Theory]
    [InlineData("groupby((Microsoft.AspNetCore.OData.Tests.Models.Product/ProductName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = ($it As Product).ProductName, }, })")]
    [InlineData("groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/DerivedProductName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = DerivedProductName, Value = ($it As DerivedProduct).DerivedProductName, }, })")]
    [InlineData("groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/CategoryName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = CategoryName, Value = ($it As DerivedProduct).Category.CategoryName, }, }, }, })")]
    [InlineData("groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/DerivedCategoryName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = DerivedCategoryName, Value = (($it As DerivedProduct).Category As DerivedCategory).DerivedCategoryName, }, }, }, })")]
    public void Inheritance_WithDerivedInstance(string filter, string expectedResult)
    {
        var filters = VerifyQueryDeserialization<DerivedProduct>(filter, expectedResult
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Theory]
    [InlineData("groupby((ProductName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = ProductName, Value = $it.ProductName, }, })")]
    [InlineData("groupby((Category/Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/DerivedCategoryName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = DerivedCategoryName, Value = ($it.Category As DerivedCategory).DerivedCategoryName, }, }, }, })")]
    [InlineData("groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/DerivedProductName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = DerivedProductName, Value = ($it As DerivedProduct).DerivedProductName, }, })")]
    [InlineData("groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/CategoryName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = CategoryName, Value = ($it As DerivedProduct).Category.CategoryName, }, }, }, })")]
    [InlineData("groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/DerivedCategoryName))", ".GroupBy($it => new GroupByWrapper() {GroupByContainer = new NestedPropertyLastInChain() {Name = Category, NestedValue = new GroupByWrapper() {GroupByContainer = new LastInChain() {Name = DerivedCategoryName, Value = (($it As DerivedProduct).Category As DerivedCategory).DerivedCategoryName, }, }, }, })")]
    public void Inheritance_WithBaseInstance(string filter, string expectedResult)
    {
        var filters = VerifyQueryDeserialization<Product>(filter, expectedResult
            + ".Select($it => new AggregationWrapper() {GroupByContainer = $it.Key.GroupByContainer, })");
    }

    [Fact]
    public void CastToNonDerivedType_Throws()
    {
        ExceptionAssert.Throws<ODataException>(
            () => VerifyQueryDeserialization<Product>("groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/CategoryName))"),
            "Encountered invalid type cast. 'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory' is not assignable from 'Microsoft.AspNetCore.OData.Tests.Models.Product'.");
    }

    private Expression VerifyQueryDeserialization(string filter, string expectedResult = null, Action<ODataQuerySettings> settingsCustomizer = null)
    {
        return VerifyQueryDeserialization<Product>(filter, expectedResult, settingsCustomizer);
    }

    private Expression VerifyQueryDeserialization<T>(string clauseString, string expectedResult = null, Action<ODataQuerySettings> settingsCustomizer = null) where T : class
    {
        IEdmModel model = GetModel<T>();
        ApplyClause clause = CreateApplyNode(clauseString, model, typeof(T));
        IAssemblyResolver assembliesResolver = AssemblyResolverHelper.Default;

        Func<ODataQuerySettings, ODataQuerySettings> customizeSettings = (settings) =>
        {
            if (settingsCustomizer != null)
            {
                settingsCustomizer.Invoke(settings);
            }

            return settings;
        };

        QueryBinderContext queryBinderContext = new QueryBinderContext(
            model,
            customizeSettings(new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }),
            typeof(T));

        var binder = new AggregationBinder();

        var query = Enumerable.Empty<T>().AsQueryable();

        var queryResult = binder.ApplyBind(query, clause.Transformations.First(), queryBinderContext, out Type resultClrType);

        var applyExpr = queryResult.Expression;

        VerifyExpression<T>(applyExpr, expectedResult);

        return applyExpr;
    }

    private void VerifyExpression<T>(Expression clause, string expectedExpression)
    {
        // strip off the beginning part of the expression to get to the first
        // actual query operator
        string resultExpression = ExpressionStringBuilder.ToString(clause);

        var replace = "System.Linq.EmptyPartition`1[" + typeof(T).FullName + "]";

        resultExpression = resultExpression.Replace(replace, string.Empty);
        Assert.True(resultExpression == expectedExpression,
            String.Format("Expected expression '{0}' but the deserializer produced '{1}'", expectedExpression, resultExpression));
    }

    private ApplyClause CreateApplyNode(string clause, IEdmModel model, Type entityType)
    {
        IEdmEntityType productType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == entityType.Name);
        Assert.NotNull(productType); // Guard

        IEdmEntitySet products = model.EntityContainer.FindEntitySet("Products");
        Assert.NotNull(products); // Guard

        ODataQueryOptionParser parser = new ODataQueryOptionParser(model, productType, products,
            new Dictionary<string, string> { { "$apply", clause } });

        return parser.ParseApply();
    }

    private IEdmModel GetModel<T>() where T : class
    {
        Type key = typeof(T);
        IEdmModel value;

        if (!_modelCache.TryGetValue(key, out value))
        {
            ODataModelBuilder model = new ODataConventionModelBuilder();
            model.EntitySet<T>("Products");
            if (key == typeof(Product))
            {
                model.EntityType<DerivedProduct>().DerivesFrom<Product>();
                model.EntityType<DerivedCategory>().DerivesFrom<Category>();
            }

            value = _modelCache[key] = model.GetEdmModel();
        }
        return value;
    }
}