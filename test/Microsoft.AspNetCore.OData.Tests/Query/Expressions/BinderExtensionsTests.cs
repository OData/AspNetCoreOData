//-----------------------------------------------------------------------------
// <copyright file="BinderExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionTreeToString;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    public class MyQueryBinder : QueryBinder
    {
        public override Expression BindCountNode(CountNode node, QueryBinderContext context)
        {
            throw new NotImplementedException();
        }

        public Expression DoBind(QueryNode node, QueryBinderContext context)
        {
            return Bind(node, context);
        }
    }

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

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            builder.EntityType<DerivedProduct>().DerivesFrom<Product>();
            builder.EntityType<DerivedCategory>().DerivesFrom<Category>();
            return builder.GetEdmModel();
        }



        [Theory]
        [InlineData(null, true, true)]
        [InlineData("", false, false)]
        [InlineData("Doritos", false, false)]
        public void EqualityOperatorWithNull2(string productName, bool withNullPropagation, bool withoutNullPropagation)
        {
            IEdmModel model = GetEdmModel();
            ODataQuerySettings querySettings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True };

            QueryBinderContext context = new QueryBinderContext(model, querySettings, typeof(Product));

            FilterClause filter = CreateFilterClause("ProductName eq null", model, typeof(Product));
            ParameterExpression filterParameter = Expression.Parameter(typeof(Product), filter.RangeVariable.Name);

            context.AddlambdaParameters(filter.RangeVariable.Name, filterParameter);

            MyQueryBinder binder = new MyQueryBinder();
            Expression expr = binder.DoBind(filter.Expression, context);

            string exprStr = expr.ToString("C#");

            //var filters = VerifyQueryDeserialization(
            //    "ProductName eq null",
            //    "$it => ($it.ProductName == null)");

            //RunFilters(filters,
            //    new Product { ProductName = productName },
            //    new { WithNullPropagation = withNullPropagation, WithoutNullPropagation = withoutNullPropagation });
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
            string expectedExpr = "(Product $it) => $it.AlternateAddresses.LongCount() == #TypedLinqParameterContainer<long>.TypedProperty";
            RunFilterTestAndVerify(products, filter, new[] { 1 }, expectedExpr);

            // Act & Assert
            filter = "AlternateAddresses/$count in [3,4]";
            expectedExpr = "(Product $it) => #TypedLinqParameterContainer<List<long>>.TypedProperty.Contains($it.AlternateAddresses.LongCount())";
            RunFilterTestAndVerify(products, filter, new[] { 2, 3 }, expectedExpr);

            // Act & Assert
            filter = "AlternateAddresses/$count($filter=HouseNumber gt 8) gt 2";
            expectedExpr = "(Product $it) => $it.AlternateAddresses.Where((Address $it) => $it.HouseNumber > #TypedLinqParameterContainer<int>.TypedProperty).LongCount() > #TypedLinqParameterContainer<long>.TypedProperty";
            RunFilterTestAndVerify(products, filter, new[] { 3 }, expectedExpr);
        }

        private void RunFilterTestAndVerify(IEnumerable<Product> products, string filter, int[] expectedIds, string expectedExpr, ODataQuerySettings querySettings = null, IAssemblyResolver assembliesResolver = null)
        {
            // Act - Bind string to Linq.Expression
            Expression filterExpr = BindFilter<Product>(filter, _model, querySettings, assembliesResolver);

            // Assert
            string acutalExpr = filterExpr.ToString("C#");
            Assert.Equal(expectedExpr, acutalExpr);

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

        private static Expression BindFilter<T>(string filter, IEdmModel model, ODataQuerySettings querySettings = null, IAssemblyResolver assembliesResolver = null)
        {
            Type elementType = typeof(T);
            FilterClause orderByClause = CreateFilterClause(filter, model, elementType);
            Assert.NotNull(orderByClause);

            querySettings = querySettings ?? _defaultSettings;
            QueryBinderContext context = new QueryBinderContext(model, querySettings, elementType)
            {
                AssembliesResolver = assembliesResolver,
                GetNestedFilterBinder = () => new FilterBinder2()
            };

            IFilterBinder filterBinder = new FilterBinder2();
            return filterBinder.BindFilter(orderByClause, context);
        }

        internal static Expression BindFilter<T>(IEdmModel model, string filter,
     ODataQuerySettings querySettings, string expectedExpr = null, IAssemblyResolver assembliesResolver = null) where T : class
        {
            Type elementType = typeof(T);
            FilterClause filterClause = CreateFilterClause(filter, model, elementType);
            Assert.NotNull(filterClause);

            IFilterBinder binder = new FilterBinder2();
            QueryBinderContext context = new QueryBinderContext(model, querySettings, elementType)
            {
                AssembliesResolver = assembliesResolver ?? AssemblyResolverHelper.Default,
                GetNestedFilterBinder = () => binder
            };

            Expression exp = binder.BindFilter(filterClause, context);
            if (expectedExpr != null)
            {
                string acutalExpr = exp.ToString("C#");
                Assert.Equal(expectedExpr, acutalExpr);
            }

            return exp;
        }


        private void RunFilterTestAndVerify<T>(T instance, string filter, bool expect, string expectedExpr, ODataQuerySettings querySettings = null, IAssemblyResolver assembliesResolver = null)
        {
            // Act - Bind string to Linq.Expression
            Expression filterExpr = BindFilter<T>(filter, _model, querySettings, assembliesResolver);

            // Assert
            string acutalExpr = filterExpr.ToString("C#");
            Assert.Equal(expectedExpr, acutalExpr);

            // Act
            bool actual = InvokeFilter<T>(instance, filterExpr);

            // Assert
            Assert.Equal(expect, actual);
        }

        public static bool InvokeFilter<T>(T instance, Expression filter)
        {
            Expression<Func<T, bool>> filterExpression = filter as Expression<Func<T, bool>>;
            Assert.NotNull(filterExpression);

            return filterExpression.Compile().Invoke(instance);
        }

        private static FilterClause CreateFilterClause(string filter, IEdmModel model, Type type)
        {
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == type.Name);
            Assert.NotNull(entityType); // Guard

            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Entities");
            Assert.NotNull(entitySet); // Guard

            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, entityType, entitySet,
                new Dictionary<string, string> { { "$filter", filter } });

            return parser.ParseFilter();
        }
    }
}
