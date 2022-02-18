//-----------------------------------------------------------------------------
// <copyright file="BinderExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
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
            string acutalExpr = ExpressionStringBuilder.ToString(filterExpr);
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
}
