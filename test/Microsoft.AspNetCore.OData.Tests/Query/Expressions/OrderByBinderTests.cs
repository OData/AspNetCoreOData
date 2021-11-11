//-----------------------------------------------------------------------------
// <copyright file="OrderByBinderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionTreeToString;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    public class OrderByBinderTests
    {
        private static readonly MethodInfo _orderbyGenericMethod
            = typeof(Enumerable).GetMethods().First(x => x.Name == "OrderBy" && x.GetParameters().Length == 2);

        private static readonly MethodInfo _orderByDescendingGenericMethod
           = typeof(Enumerable).GetMethods().First(x => x.Name == "OrderByDescending" && x.GetParameters().Length == 2);

        private static readonly MethodInfo _thenByGenericMethod
           = typeof(Enumerable).GetMethods().First(x => x.Name == "ThenBy" && x.GetParameters().Length == 2);

        private static readonly MethodInfo _thenByDescendingGenericMethod
           = typeof(Enumerable).GetMethods().First(x => x.Name == "ThenByDescending" && x.GetParameters().Length == 2);

        private static readonly ODataQuerySettings _defaultSettings = new ODataQuerySettings
        {
            HandleNullPropagation = HandleNullPropagationOption.False
        };

        private static IEdmModel _model = GetEdmModel();

        [Fact]
        public void OrderByBinder_Binds_Throws_InputParameters()
        {
            // Arrange
            IOrderByBinder binder = new OrderByBinder();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => binder.BindOrderBy(null, null), "orderByClause");

            // Act & Assert
            SingleValueNode expression = new Mock<SingleValueNode>().Object;
            RangeVariable range = new Mock<RangeVariable>().Object;
            OrderByClause orderByClause = new OrderByClause(null, expression, OrderByDirection.Descending, range);
            ExceptionAssert.ThrowsArgumentNull(() => binder.BindOrderBy(orderByClause, null), "context");
        }

        [Fact]
        public void OrderByBinder_Binds_DirectStringProperty_WorksAsExpected()
        {
            // Arrange
            IEnumerable<Product> products = new[]
            {
                new Product { ProductID = 1, ProductName = "a2" },
                new Product { ProductID = 2, ProductName = "a1" },
                new Product { ProductID = 3, ProductName = "a3" },
            };

            string orderBy = "ProductName";

            string expectedExpr = "(Product $it) => $it.ProductName";

            // Act & Assert -- Ascending
            RunOrderByTestAndVerify(products, orderBy, new[] { 2, 1, 3 }, expectedExpr);

            // Act & Assert -- Descending
            RunOrderByTestAndVerify(products, $"{orderBy} desc", new[] { 3, 1, 2 }, expectedExpr);
        }

        [Fact]
        public void OrderByBinder_Binds_DirectIntProperty_WorksAsExpected()
        {
            // Arrange
            IEnumerable<Product> products = new[]
            {
                new Product { ProductID = 1, CategoryID = 4 },
                new Product { ProductID = 2, CategoryID = 2 },
                new Product { ProductID = 3, CategoryID = 7 },
            };

            string orderBy = "CategoryID";

            string expectedExpr = "(Product $it) => $it.CategoryID";

            // Act & Assert -- Ascending
            RunOrderByTestAndVerify(products, orderBy, new[] { 2, 1, 3 }, expectedExpr);

            // Act & Assert -- Descending
            RunOrderByTestAndVerify(products, $"{orderBy} desc", new[] { 3, 1, 2 }, expectedExpr);
        }

        [Fact]
        public void OrderByBinder_Binds_DerivedStringFacterty_WorksAsExpected()
        {
            // Arrange
            IEnumerable<Product> products = new[]
            {
                new Product { ProductID = 1, SupplierAddress = new Address { State = "WA" } },
                new Product { ProductID = 2, SupplierAddress = new Address { State = "ZA" } },
                new Product { ProductID = 3, SupplierAddress = new Address { State = "EA" } },
            };

            string orderBy = "SupplierAddress/State";

            string expectedExpr = "(Product $it) => $it.SupplierAddress.State";

            // Act & Assert -- Ascending
            RunOrderByTestAndVerify(products, orderBy, new[] { 3, 1, 2 }, expectedExpr);

            // Act & Assert -- Descending
            RunOrderByTestAndVerify(products, $"{orderBy} desc", new[] { 2, 1, 3 }, expectedExpr);
        }

        [Fact]
        public void OrderByBinder_Binds_UsingDollarCountOnCollectionProperty_WorksAsExpected()
        {
            // Arrange
            IEnumerable<Product> products = new[]
            {
                new Product { ProductID = 1, AlternateIDs = new[] { 1, 2, 3 }}, // 3 items
                new Product { ProductID = 2, AlternateIDs = new[] { 1, 2, 3, 5, 6}}, // 5 items
                new Product { ProductID = 3, AlternateIDs = new[] { 1, 2 }} // 2 items
            };

            string orderBy = "AlternateIDs/$count";

            string expectedExpr = "(Product $it) => $it.AlternateIDs.LongCount()";

            // Act & Assert -- Ascending
            RunOrderByTestAndVerify(products, orderBy, new[] { 3, 1, 2 }, expectedExpr);

            // Act & Assert -- Descending
            RunOrderByTestAndVerify(products, $"{orderBy} desc", new[] { 2, 1, 3 }, expectedExpr);
        }

        [Fact]
        public void OrderByBinder_Binds_UsingDollarCountWithFilterOnCollectionProperty_WorksAsExpected()
        {
            // Arrange
            IEnumerable<Product> products = new[]
            {
                new Product { ProductID = 1, AlternateAddresses = new[] { new Address { HouseNumber = 9 }, new Address { HouseNumber = 10 } }}, // 2 addresses matched
                new Product { ProductID = 2, AlternateAddresses = new[] { new Address { HouseNumber = 2 }, new Address { HouseNumber = 3 } }}, // 0 matched
                new Product { ProductID = 3, AlternateAddresses = new[] { new Address { HouseNumber = 1 }, new Address { HouseNumber = 11 } }} // 1 matched
            };

            string orderBy = "AlternateAddresses/$count($filter=HouseNumber gt 8)";

            string expectedExpr = "(Product $it) => $it.AlternateAddresses.Where((Address $it) => $it.HouseNumber > #TypedLinqParameterContainer<int>.TypedProperty).LongCount()";

            // Act & Assert -- Ascending
            RunOrderByTestAndVerify(products, orderBy, new[] { 2, 3, 1 }, expectedExpr);

            // Act & Assert -- Descending
            RunOrderByTestAndVerify(products, $"{orderBy} desc", new[] { 1, 3, 2 }, expectedExpr);
        }

        [Fact]
        public void OrderByBinder_Binds_PropertyOnDerivedType_WorksAsExpected()
        {
            // Arrange
            IEnumerable<Product> products = new[]
            {
                new DerivedProduct { ProductID = 1, DerivedProductName = "N3" },
                new DerivedProduct { ProductID = 2, DerivedProductName = "N1" },
                new DerivedProduct { ProductID = 3, DerivedProductName = "N2" }
            };

            string orderBy = "NS.DerivedProduct/DerivedProductName";

            string expectedExpr = "(Product $it) => ($it as DerivedProduct).DerivedProductName";

            // Act & Assert -- Ascending
            RunOrderByTestAndVerify(products, orderBy, new[] { 2, 3, 1 }, expectedExpr);

            // Act & Assert -- Descending
            RunOrderByTestAndVerify(products, $"{orderBy} desc", new[] { 1, 3, 2 }, expectedExpr);
        }

        [Fact]
        public void OrderByBinder_Binds_MultipleProperties_WorksAsExpected()
        {
            // Arrange
            IEnumerable<Product> products = new[]
            {
                new Product { ProductID = 1, ProductName = "grape" },
                new Product { ProductID = 2, ProductName = "passionfruit" },
                new Product { ProductID = 3, ProductName = "banana" },
                new Product { ProductID = 4, ProductName = "mango" },
                new Product { ProductID = 5, ProductName = "orange" },
                new Product { ProductID = 6, ProductName = "raspberry" },
                new Product { ProductID = 7, ProductName = "apple" },
                new Product { ProductID = 8, ProductName = "blueberry" },
            };

            string orderBy = "length(ProductName),ProductName";

            string expectedExpr1 = "(Product $it) => $it.ProductName.Length";
            string expectedExpr2 = "(Product $it) => $it.ProductName";

            // Act & Assert -- Ascending, and have the following sorting:
            /* 
            apple
            grape
            mango
            banana
            orange
            blueberry
            raspberry
            passionfruit
            */
            RunOrderByTestAndVerify(products, orderBy, new[] { 7, 1, 4, 3, 5, 8, 6, 2 }, expectedExpr1, expectedExpr2);

            // Act & Assert -- Descending
            // Be noted, only desc the second one and have the following sorting:
            /* 
            mango
            grape
            apple
            orange
            banana
            raspberry
            blueberry
            passionfruit
            */
            RunOrderByTestAndVerify(products, $"{orderBy} desc", new[] { 4, 1, 7, 5, 3, 6, 8, 2 }, expectedExpr1, expectedExpr2);
        }

        #region Helpers
        //private void RunOrderByTestAndVerify(IEnumerable<Product> products, string orderBy, string expectedExpr, int[] expectedOrderedId)
        //{
        //    // Act - Bind string to Linq.Expression
        //    (Expression orderByExpression, OrderByDirection direction) = BindOrderBy<Product>(orderBy, _model);

        //    // Assert
        //    Assert.NotNull(orderByExpression);

        //    string orderByExpressionString = orderByExpression.ToString("C#");
        //    Assert.Equal(expectedExpr, orderByExpressionString);

        //    // Act - Run Orderby
        //    IOrderedEnumerable<Product> orderedProducts = InvokeOrderBy(products, orderByExpression, direction);

        //    // Assert
        //    Assert.True(expectedOrderedId.SequenceEqual(orderedProducts.Select(a => a.ProductID))); // ordered id
        //}

        private void RunOrderByTestAndVerify(IEnumerable<Product> products, string orderBy, int[] expectedOrderedId, params string[] expectedExprs)
        {
            // Act - Bind string to Linq.Expression
            OrderByBinderResult binderResult = BindOrderBy<Product>(orderBy, _model);

            // Assert
            Assert.NotNull(binderResult);

            bool alreadyOrdered = false;
            IList<string> exprs = new List<string>();
            OrderByBinderResult result = binderResult;
            IEnumerable<Product> collections = products;
            IOrderedEnumerable<Product> orderedProducts;
            do
            {
                exprs.Add(result.OrderByExpression.ToString("C#"));

                // Act - Invoke Orderby
                orderedProducts = InvokeOrderBy(collections, result.OrderByExpression, result.Direction, alreadyOrdered);

                alreadyOrdered = true;

                result = result.ThenBy;

                collections = orderedProducts;
            }
            while (result != null);

            // Assert
            Assert.True(expectedExprs.SequenceEqual(exprs));
            Assert.True(expectedOrderedId.SequenceEqual(orderedProducts.Select(a => a.ProductID))); // ordered id
        }

        public static IOrderedEnumerable<T> InvokeOrderBy<T>(IEnumerable<T> collection, Expression orderByExpr, OrderByDirection direction, bool alreadyOrdered = false)
        {
            LambdaExpression orderByLambda = orderByExpr as LambdaExpression;
            Assert.NotNull(orderByLambda);

            Type returnType = orderByLambda.Body.Type;
            Delegate function = orderByLambda.Compile();
            Type type = typeof(T);
            MethodInfo orderByMethod;

            if (alreadyOrdered)
            {
                if (direction == OrderByDirection.Ascending)
                {
                    orderByMethod = _thenByGenericMethod.MakeGenericMethod(type, returnType);
                }
                else
                {
                    orderByMethod = _thenByDescendingGenericMethod.MakeGenericMethod(type, returnType);
                }

                return orderByMethod.Invoke(null, new object[] { collection, function }) as IOrderedEnumerable<T>;
            }
            else
            {
                if (direction == OrderByDirection.Ascending)
                {
                    orderByMethod = _orderbyGenericMethod.MakeGenericMethod(type, returnType);
                }
                else
                {
                    orderByMethod = _orderByDescendingGenericMethod.MakeGenericMethod(type, returnType);
                }

                return orderByMethod.Invoke(null, new object[] { collection, function }) as IOrderedEnumerable<T>;
            }
        }

        //private static (Expression, OrderByDirection) BindOrderBy<T>(string orderBy, IEdmModel model, ODataQuerySettings querySettings = null, IAssemblyResolver assembliesResolver = null)
        //{
        //    Type elementType = typeof(T);
        //    OrderByClause orderByClause = CreateOrderByNode(orderBy, model, elementType);
        //    Assert.NotNull(orderByClause);

        //    querySettings = querySettings ?? _defaultSettings;
        //    QueryBinderContext context = new QueryBinderContext(model, querySettings, elementType)
        //    {
        //        AssembliesResolver = assembliesResolver
        //    };

        //    IOrderByBinder orderByBinder = new OrderByBinder();
        //    Expression orderByExpr = orderByBinder.BindOrderBy(orderByClause, context);
        //    return (orderByExpr, orderByClause.Direction);
        //}

        private static OrderByBinderResult BindOrderBy<T>(string orderBy, IEdmModel model, ODataQuerySettings querySettings = null, IAssemblyResolver assembliesResolver = null)
        {
            Type elementType = typeof(T);
            OrderByClause orderByClause = CreateOrderByNode(orderBy, model, elementType);
            Assert.NotNull(orderByClause);

            querySettings = querySettings ?? _defaultSettings;
            QueryBinderContext context = new QueryBinderContext(model, querySettings, elementType)
            {
                AssembliesResolver = assembliesResolver
            };

            IOrderByBinder orderByBinder = new OrderByBinder();
            return orderByBinder.BindOrderBy(orderByClause, context);
        }

        private static OrderByClause CreateOrderByNode(string orderBy, IEdmModel model, Type entityType)
        {
            IEdmEntityType productType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == entityType.Name);
            Assert.NotNull(productType); // Guard

            IEdmEntitySet products = model.EntityContainer.FindEntitySet("Products");
            Assert.NotNull(products); // Guard

            ODataQueryOptionParser parser = new ODataQueryOptionParser(model, productType, products,
                new Dictionary<string, string> { { "$orderby", orderBy } });

            return parser.ParseOrderBy();
        }
        #endregion

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "NS";
            builder.EntitySet<Product>("Products");
            builder.EntityType<DerivedProduct>().DerivesFrom<Product>();
            builder.EntityType<DerivedCategory>().DerivesFrom<Category>();
            return builder.GetEdmModel();
        }
    }
}
