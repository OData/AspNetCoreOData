//-----------------------------------------------------------------------------
// <copyright file="FilterBinderTests.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions;

public class FilterBinderTests
{
    private const string NotTesting = "";

    private static Dictionary<Type, IEdmModel> _modelCache = new Dictionary<Type, IEdmModel>();

    [Fact]
    public void BindFilter_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        FilterBinder binder = new FilterBinder();
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindFilter(null, null), "filterClause");

        // Arrange & Act & Assert
        FilterClause filterClause = new FilterClause(new Mock<SingleValueNode>().Object, new Mock<RangeVariable>().Object);
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindFilter(filterClause, null), "context");
    }

    #region Logical Operators
    [Theory]
    [InlineData(null, true, true)]
    [InlineData("", false, false)]
    [InlineData("Doritos", false, false)]
    public void LogicalOperators_EqualityOperatorWithNull(string productName, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("ProductName eq null", "$it => ($it.ProductName == null)");

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { ProductName = productName }, (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData("", true, true)]
    [InlineData("Doritos", true, true)]
    public void LogicalOperators_NotEqualityOperatorWithNull(string productName, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("ProductName ne null", "$it => ($it.ProductName != null)");

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { ProductName = productName }, (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData("", false, false)]
    [InlineData("Doritos", true, true)]
    public void LogicalOperators_EqualityOperatorWithValue(string productName, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("ProductName eq 'Doritos'", "$it => ($it.ProductName == \"Doritos\")");

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { ProductName = productName }, (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, true, true)]
    [InlineData("", true, true)]
    [InlineData("Doritos", false, false)]
    public void LogicalOperators_NotEqualOperator(string productName, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("ProductName ne 'Doritos'", "$it => ($it.ProductName != \"Doritos\")");

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { ProductName = productName }, (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData(5.01, true, true)]
    [InlineData(4.99, false, false)]
    public void LogicalOperators_GreaterThanOperator(object unitPrice, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("UnitPrice gt 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice > Convert({0:0.00}))", 5.0),
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice > Convert({0:0.00})) == True)", 5.0));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData(5.0, true, true)]
    [InlineData(4.99, false, false)]
    public void LogicalOperators_GreaterThanEqualOperator(object unitPrice, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("UnitPrice ge 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice >= Convert({0:0.00}))", 5.0),
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice >= Convert({0:0.00})) == True)", 5.0));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData(4.99, true, true)]
    [InlineData(5.01, false, false)]
    public void LogicalOperators_LessThanOperator(object unitPrice, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("UnitPrice lt 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice < Convert({0:0.00}))", 5.0),
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice < Convert({0:0.00})) == True)", 5.0));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData(5.0, true, true)]
    [InlineData(5.01, false, false)]
    public void LogicalOperators_LessThanOrEqualOperator(object unitPrice, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("UnitPrice le 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", 5.0),
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice <= Convert({0:0.00})) == True)", 5.0));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Fact]
    public void LogicalOperators_NegativeNumbers()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>("UnitPrice le -5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", -5.0),
            NotTesting);
    }

    [Theory]
    [InlineData("DateTimeOffsetProp eq DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp == $it.DateTimeOffsetProp)")]
    [InlineData("DateTimeOffsetProp ne DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp != $it.DateTimeOffsetProp)")]
    [InlineData("DateTimeOffsetProp ge DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp >= $it.DateTimeOffsetProp)")]
    [InlineData("DateTimeOffsetProp le DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp <= $it.DateTimeOffsetProp)")]
    public void LogicalOperators_WithDateTimeOffsetInEqualities(string clause, string expectedExpression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<DataTypes>(clause, expectedExpression);
    }

    [Theory]
    [InlineData("DateTimeProperty eq DateTimeProperty", "$it => ($it.DateTimeProperty == $it.DateTimeProperty)")]
    [InlineData("DateTimeProperty ne DateTimeProperty", "$it => ($it.DateTimeProperty != $it.DateTimeProperty)")]
    [InlineData("DateTimeProperty ge DateTimeProperty", "$it => ($it.DateTimeProperty >= $it.DateTimeProperty)")]
    [InlineData("DateTimeProperty le DateTimeProperty", "$it => ($it.DateTimeProperty <= $it.DateTimeProperty)")]
    public void LogicalOperators_WithDateTimeInEqualities(string clause, string expectedExpression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<DataTypes>(clause, expectedExpression);
    }

    [Fact]
    [ReplaceCulture]
    public void LogicalOperators_BooleanOperatorNullableTypes()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "UnitPrice eq 5.00m or CategoryID eq 0",
            "$it => (($it.UnitPrice == Convert(5.00)) OrElse ($it.CategoryID == 0))",
            NotTesting);
    }

    [Fact]
    public void LogicalOperators_BooleanComparisonOnNullableAndNonNullableType()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "Discontinued eq true",
            "$it => ($it.Discontinued == Convert(True))",
            "$it => (($it.Discontinued == Convert(True)) == True)");
    }

    [Fact]
    public void LogicalOperators_BooleanComparisonOnNullableType()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "Discontinued eq Discontinued",
            "$it => ($it.Discontinued == $it.Discontinued)",
            "$it => (($it.Discontinued == $it.Discontinued) == True)");
    }

    [Theory]
    [InlineData(null, null, false, false)]
    [InlineData(5.0, 0, true, true)]
    [InlineData(null, 1, false, false)]
    public void LogicalOperators_With_OrOperator(object unitPrice, object unitsInStock, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "UnitPrice eq 5.00m or UnitsInStock eq 0",
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice == Convert({0:0.00})) OrElse (Convert($it.UnitsInStock) == Convert({1})))", 5.0, 0),
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitPrice = ToNullable<decimal>(unitPrice), UnitsInStock = ToNullable<short>(unitsInStock) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, null, false, false)]
    [InlineData(5.0, 10, true, true)]
    [InlineData(null, 1, false, false)]
    public void LogicalOperators_With_AndOperator(object unitPrice, object unitsInStock, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "UnitPrice eq 5.00m and UnitsInStock eq 10.00m",
            String.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice == Convert({0:0.00})) AndAlso (Convert($it.UnitsInStock) == Convert({1:0.00})))", 5.0, 10.0),
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitPrice = ToNullable<decimal>(unitPrice), UnitsInStock = ToNullable<short>(unitsInStock) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(null, true, false)] // This is an interesting case for null propagation.
    [InlineData(5.0, false, false)]
    [InlineData(5.5, true, true)]
    public void LogicalOperators_Negation(object unitPrice, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "not (UnitPrice eq 5.00m)",
            string.Format(CultureInfo.InvariantCulture, "$it => Not(($it.UnitPrice == Convert({0:0.00})))", 5.0),
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    public void LogicalOperators_BoolNegation(bool discontinued, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "not Discontinued",
            "$it => Convert(Not($it.Discontinued))",
            "$it => (Not($it.Discontinued) == True)");

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { Discontinued = discontinued }, (falseNullPropagation, trueNullPropagation));
    }

    [Fact]
    public void LogicalOperators_NestedNegation()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "not (not(not    (Discontinued)))",
            "$it => Convert(Not(Not(Not($it.Discontinued))))",
            "$it => (Not(Not(Not($it.Discontinued))) == True)");
    }
    #endregion

    #region Arithmetic Operators
    [Theory]
    [InlineData(null, false, false)]
    [InlineData(5.0, true, true)]
    [InlineData(15.01, false, false)]
    public void ArithmeticOperators_Subtraction(object unitPrice, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "UnitPrice sub 1.00m lt 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice - Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
            string.Format(CultureInfo.InvariantCulture, "$it => ((($it.UnitPrice - Convert({0:0.00})) < Convert({1:0.00})) == True)", 1.0, 5.0));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitPrice = ToNullable<decimal>(unitPrice) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Fact]
    public void ArithmeticOperators_Addition()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "UnitPrice add 1.00m lt 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice + Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
            NotTesting);
    }

    [Fact]
    public void ArithmeticOperators_Multiplication()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "UnitPrice mul 1.00m lt 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice * Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
            NotTesting);
    }

    [Fact]
    public void ArithmeticOperators_Division()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "UnitPrice div 1.00m lt 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice / Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
            NotTesting);
    }

    [Fact]
    public void ArithmeticOperators_Modulo()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "UnitPrice mod 1.00m lt 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice % Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0),
            NotTesting);
    }
    #endregion

    #region NULL handling
    [Theory]
    [InlineData("UnitsInStock eq UnitsOnOrder", null, null, true, false)]
    [InlineData("UnitsInStock ne UnitsOnOrder", null, null, false, false)]
    [InlineData("UnitsInStock gt UnitsOnOrder", null, null, false, false)]
    [InlineData("UnitsInStock ge UnitsOnOrder", null, null, false, false)]
    [InlineData("UnitsInStock lt UnitsOnOrder", null, null, false, false)]
    [InlineData("UnitsInStock le UnitsOnOrder", null, null, false, false)]
    [InlineData("(UnitsInStock add UnitsOnOrder) eq UnitsInStock", null, null, true, false)]
    [InlineData("(UnitsInStock sub UnitsOnOrder) eq UnitsInStock", null, null, true, false)]
    [InlineData("(UnitsInStock mul UnitsOnOrder) eq UnitsInStock", null, null, true, false)]
    [InlineData("(UnitsInStock div UnitsOnOrder) eq UnitsInStock", null, null, true, false)]
    [InlineData("(UnitsInStock mod UnitsOnOrder) eq UnitsInStock", null, null, true, false)]
    [InlineData("UnitsInStock eq UnitsOnOrder", 1, null, false, false)]
    [InlineData("UnitsInStock eq UnitsOnOrder", 1, 1, true, true)]
    public void NullHandling(string filter, object unitsInStock, object unitsOnOrder, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(filter);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitsInStock = ToNullable<short>(unitsInStock), UnitsOnOrder = ToNullable<short>(unitsOnOrder) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData("UnitsInStock eq null", null, true, true)] // NULL == constant NULL is true when null propagation is enabled
    [InlineData("UnitsInStock ne null", null, false, false)]  // NULL != constant NULL is false when null propagation is enabled
    public void NullHandling_LiteralNull(string filter, object unitsInStock, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(filter);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { UnitsInStock = ToNullable<short>(unitsInStock) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Fact]
    public void NullHandling_StringFunctionWithStringParameret()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "startswith(ProductName, 'Abc')",
            NotTesting,
            "$it => (IIF((($it.ProductName == null) OrElse (\"Abc\" == null)), null, Convert($it.ProductName.StartsWith(\"Abc\"))) == True)");
    }
    #endregion

    [Theory]
    [InlineData("StringProp gt 'Middle'", "Middle", false, false)]
    [InlineData("StringProp ge 'Middle'", "Middle", true, true)]
    [InlineData("StringProp lt 'Middle'", "Middle", false, false)]
    [InlineData("StringProp le 'Middle'", "Middle", true, true)]
    [InlineData("StringProp ge StringProp", "", true, true)]
    [InlineData("StringProp gt null", "", true, true)]
    [InlineData("null gt StringProp", "", false, false)]
    [InlineData("'Middle' gt StringProp", "Middle", false, false)]
    [InlineData("'a' lt 'b'", "", true, true)]
    public void StringComparisons_Work(string filter, string value, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<DataTypes>(filter);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new DataTypes { StringProp = value }, (falseNullPropagation, trueNullPropagation));
    }

    // Issue: 477
    [Theory]
    [InlineData("indexof('hello', StringProp) gt UIntProp")]
    [InlineData("indexof('hello', StringProp) gt ULongProp")]
    [InlineData("indexof('hello', StringProp) gt UShortProp")]
    [InlineData("indexof('hello', StringProp) gt NullableUShortProp")]
    [InlineData("indexof('hello', StringProp) gt NullableUIntProp")]
    [InlineData("indexof('hello', StringProp) gt NullableULongProp")]
    public void ComparisonsInvolvingCastsAndNullableValues(string filter)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<DataTypes>(filter);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new DataTypes(), (typeof(ArgumentNullException), false));
    }

    [Theory]
    [InlineData(null, null, true, true)]
    [InlineData("not doritos", 0, true, true)]
    [InlineData("Doritos", 1, false, false)]
    public void MultipleLogicOperators_Grouping(string productName, object unitsInStock, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "((ProductName ne 'Doritos') or (UnitPrice lt 5.00m))",
            string.Format(CultureInfo.InvariantCulture, "$it => (($it.ProductName != \"Doritos\") OrElse ($it.UnitPrice < Convert({0:0.00})))", 5.0),
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { ProductName = productName, UnitsInStock = ToNullable<short>(unitsInStock) },
            (falseNullPropagation, trueNullPropagation));
    }

    [Fact]
    public void SubMemberExpressions()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "Category/CategoryName eq 'Snacks'",
            "$it => ($it.Category.CategoryName == \"Snacks\")",
            "$it => (IIF(($it.Category == null), null, $it.Category.CategoryName) == \"Snacks\")");

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product(), (typeof(NullReferenceException), false));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { Category = new Category { CategoryName = "Snacks" } },
            (true, true));
    }

    [Fact]
    public void SubMemberExpressionsRecursive()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "Category/Product/Category/CategoryName eq 'Snacks'",
            "$it => ($it.Category.Product.Category.CategoryName == \"Snacks\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product(), (typeof(NullReferenceException), false));
    }

    [Fact]
    public void ComplexPropertyNavigation()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "SupplierAddress/City eq 'Redmond'",
            "$it => ($it.SupplierAddress.City == \"Redmond\")",
            "$it => (IIF(($it.SupplierAddress == null), null, $it.SupplierAddress.City) == \"Redmond\")");

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product(), (typeof(NullReferenceException), false));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { SupplierAddress = new Address { City = "Redmond" } }, (true, true));
    }

    #region Any/All
    [Fact]
    public void AnyOperator_OnNavigationEnumerableCollections()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "Category/EnumerableProducts/any(P: P/ProductName eq 'Snacks')",
           "$it => $it.Category.EnumerableProducts.Any(P => (P.ProductName == \"Snacks\"))",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
                 new Product
                 {
                     Category = new Category
                     {
                         EnumerableProducts = new Product[]
                         {
                             new Product { ProductName = "Snacks" },
                             new Product { ProductName = "NonSnacks" }
                         }
                     }
                 },
                 (true, true));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    EnumerableProducts = new Product[]
                    {
                        new Product { ProductName = "NonSnacks" }
                    }
                }
            },
            (false, false));
    }

    [Fact]
    public void AnyOperator_OnNavigationQueryableCollections()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "Category/QueryableProducts/any(P: P/ProductName eq 'Snacks')",
           "$it => $it.Category.QueryableProducts.Any(P => (P.ProductName == \"Snacks\"))",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    QueryableProducts = new Product[]
                    {
                        new Product { ProductName = "Snacks" },
                        new Product { ProductName = "NonSnacks" }
                    }.AsQueryable()
                }
            },
            (true, true));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    QueryableProducts = new Product[]
                    {
                        new Product { ProductName = "NonSnacks" }
                    }.AsQueryable()
                }
            },
            (false, false));
    }

    [Fact]
    public void AnyOperator_WithNestedFilterWithCountNode_OnNavigationQueryableCollections()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "Category/QueryableProducts/any(p: p/AlternateAddresses/$count($filter=HouseNumber ge 8) ge 2)",
           "$it => $it.Category.QueryableProducts.Any(p => (p.AlternateAddresses.Where($it => ($it.HouseNumber >= 8)).LongCount() >= 2))",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    QueryableProducts = new Product[]
                    {
                        new Product { AlternateAddresses = new Address[] { new Address { HouseNumber = 8 } } },
                        new Product { AlternateAddresses = new Address[] { new Address { HouseNumber = 9 }, new Address { HouseNumber = 10 } } }
                    }.AsQueryable()
                }
            },
            (true, true));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    QueryableProducts = new Product[]
                    {
                        new Product { AlternateAddresses = new Address[] { new Address { HouseNumber = 8 } } },
                        new Product { AlternateAddresses = new Address[] { new Address { HouseNumber = 4 } } }  // < 8
                    }.AsQueryable()
                }
            },
            (false, false));
    }

    [Fact]
    public void AnyOperator_WithNestedAny_OnNavigationQueryableCollections()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "Category/QueryableProducts/any(p: p/AlternateAddresses/any(o:o/HouseNumber eq 8))",
           "$it => $it.Category.QueryableProducts.Any(p => p.AlternateAddresses.Any(o => (o.HouseNumber == 8)))",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    QueryableProducts = new Product[]
                    {
                        new Product { AlternateAddresses = new Address[] { new Address { HouseNumber = 8 } } },
                        new Product { AlternateAddresses = new Address[] { new Address { HouseNumber = 9 } } }
                    }.AsQueryable()
                }
            },
            (true, true));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    QueryableProducts = new Product[]
                    {
                        new Product { AlternateAddresses = new Address[] { new Address { HouseNumber = 10 } } },
                        new Product { AlternateAddresses = new Address[] { new Address { HouseNumber = 9 } } }
                    }.AsQueryable()
                }
            },
            (false, false));
    }

    [Theory]
    [InlineData("Category/QueryableProducts/any(P: P/ProductID in (1))", "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Int32].Contains(P.ProductID))", "$it => (IIF((IIF(($it.Category == null), null, $it.Category.QueryableProducts) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.QueryableProducts).Any(P => System.Collections.Generic.List`1[System.Int32].Cast().Contains(IIF((P == null), null, Convert(P.ProductID)))))) == True)")]
    [InlineData("Category/EnumerableProducts/any(P: P/ProductID in (1))", "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Int32].Contains(P.ProductID))", "$it => (IIF((IIF(($it.Category == null), null, $it.Category.EnumerableProducts) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.EnumerableProducts).Any(P => System.Collections.Generic.List`1[System.Int32].Cast().Contains(IIF((P == null), null, Convert(P.ProductID)))))) == True)")]
    [InlineData("Category/QueryableProducts/any(P: P/GuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7))", "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Guid].Contains(P.GuidProperty))", "$it => (IIF((IIF(($it.Category == null), null, $it.Category.QueryableProducts) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.QueryableProducts).Any(P => System.Collections.Generic.List`1[System.Guid].Cast().Contains(IIF((P == null), null, Convert(P.GuidProperty)))))) == True)")]
    [InlineData("Category/EnumerableProducts/any(P: P/GuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7))", "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Guid].Contains(P.GuidProperty))", "$it => (IIF((IIF(($it.Category == null), null, $it.Category.EnumerableProducts) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.EnumerableProducts).Any(P => System.Collections.Generic.List`1[System.Guid].Cast().Contains(IIF((P == null), null, Convert(P.GuidProperty)))))) == True)")]
    [InlineData("Category/QueryableProducts/any(P: P/NullableGuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7))", "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(P.NullableGuidProperty))", "$it => (IIF((IIF(($it.Category == null), null, $it.Category.QueryableProducts) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.QueryableProducts).Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(IIF((P == null), null, P.NullableGuidProperty))))) == True)")]
    [InlineData("Category/EnumerableProducts/any(P: P/NullableGuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7))", "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(P.NullableGuidProperty))", "$it => (IIF((IIF(($it.Category == null), null, $it.Category.EnumerableProducts) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.EnumerableProducts).Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(IIF((P == null), null, P.NullableGuidProperty))))) == True)")]
    [InlineData("Category/QueryableProducts/any(P: P/Discontinued in (false, null))", "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Boolean]].Contains(P.Discontinued))", "$it => (IIF((IIF(($it.Category == null), null, $it.Category.QueryableProducts) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.QueryableProducts).Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Boolean]].Contains(IIF((P == null), null, P.Discontinued))))) == True)")]
    [InlineData("Category/EnumerableProducts/any(P: P/Discontinued in (false, null))", "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Boolean]].Contains(P.Discontinued))", "$it => (IIF((IIF(($it.Category == null), null, $it.Category.EnumerableProducts) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.EnumerableProducts).Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Boolean]].Contains(IIF((P == null), null, P.Discontinued))))) == True)")]
    public void AnyOperator_WithInOperator_OnNavigation(string filter, string falseNullExpression, string trueNullExpression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, falseNullExpression, trueNullExpression);
    }

    [Theory]
    [InlineData("Category/QueryableProducts/any(P: false)", "$it => False")]
    [InlineData("Category/QueryableProducts/any(P: false and P/ProductName eq 'Snacks')", "$it => $it.Category.QueryableProducts.Any(P => (False AndAlso (P.ProductName == \"Snacks\")))")]
    [InlineData("Category/QueryableProducts/any(P: true)", "$it => $it.Category.QueryableProducts.Any()")]
    public void AnyOperator_OnNavigation_Contradiction(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression, NotTesting);
    }

    [Fact]
    public void AnyOperator_OnNavigation_NullCollection()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "Category/EnumerableProducts/any(P: P/ProductName eq 'Snacks')",
           "$it => $it.Category.EnumerableProducts.Any(P => (P.ProductName == \"Snacks\"))",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters,
                 new Product
                 {
                     Category = new Category
                     {
                     }
                 },
                 (typeof(ArgumentNullException), false));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    EnumerableProducts = new Product[]
                    {
                        new Product { ProductName = "Snacks" }
                    }
                }
            },
            (true, true));
    }

    [Fact]
    public void AllOperator_OnNavigation_NullCollection()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "Category/EnumerableProducts/all(P: P/ProductName eq 'Snacks')",
           "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters,
            new Product
            {
                Category = new Category
                {
                }
            },
            (typeof(ArgumentNullException), false));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product
            {
                Category = new Category
                {
                    EnumerableProducts = new Product[]
                    {
                        new Product { ProductName = "Snacks" }
                    }
                }
            },
            (true, true));
    }

    [Fact]
    public void MultipleAnys_WithSameRangeVariableName()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "AlternateIDs/any(n: n eq 42) and AlternateAddresses/any(n : n/City eq 'Redmond')",
           "$it => ($it.AlternateIDs.Any(n => (n == 42)) AndAlso $it.AlternateAddresses.Any(n => (n.City == \"Redmond\")))",
           NotTesting);
    }

    [Fact]
    public void MultipleAlls_WithSameRangeVariableName()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "AlternateIDs/all(n: n eq 42) and AlternateAddresses/all(n : n/City eq 'Redmond')",
           "$it => ($it.AlternateIDs.All(n => (n == 42)) AndAlso $it.AlternateAddresses.All(n => (n.City == \"Redmond\")))",
           NotTesting);
    }

    [Fact]
    public void AnyOnNavigationEnumerableCollections_EmptyFilter()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "Category/EnumerableProducts/any()",
           "$it => $it.Category.EnumerableProducts.Any()",
           NotTesting);
    }

    [Fact]
    public void AnyOnNavigationQueryableCollections_EmptyFilter()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "Category/QueryableProducts/any()",
           "$it => $it.Category.QueryableProducts.Any()",
           NotTesting);
    }

    [Fact]
    public void AllOnNavigationEnumerableCollections()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "Category/EnumerableProducts/all(P: P/ProductName eq 'Snacks')",
           "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))",
           NotTesting);
    }

    [Fact]
    public void AllOnNavigationQueryableCollections()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "Category/QueryableProducts/all(P: P/ProductName eq 'Snacks')",
           "$it => $it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\"))",
           NotTesting);
    }

    [Fact]
    public void AnyInSequenceNotNested()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "Category/QueryableProducts/any(P: P/ProductName eq 'Snacks') or Category/QueryableProducts/any(P2: P2/ProductName eq 'Snacks')",
           "$it => ($it.Category.QueryableProducts.Any(P => (P.ProductName == \"Snacks\")) OrElse $it.Category.QueryableProducts.Any(P2 => (P2.ProductName == \"Snacks\")))",
           NotTesting);
    }

    [Fact]
    public void AllInSequenceNotNested()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "Category/QueryableProducts/all(P: P/ProductName eq 'Snacks') or Category/QueryableProducts/all(P2: P2/ProductName eq 'Snacks')",
           "$it => ($it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\")) OrElse $it.Category.QueryableProducts.All(P2 => (P2.ProductName == \"Snacks\")))",
           NotTesting);
    }

    [Fact]
    public void AnyOnPrimitiveCollection()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "AlternateIDs/any(id: id eq 42)",
           "$it => $it.AlternateIDs.Any(id => (id == 42))",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { AlternateIDs = new[] { 1, 2, 42 } },
            (true, true));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { AlternateIDs = new[] { 1, 2 } },
            (false, false));
    }

    [Fact]
    public void AllOnPrimitiveCollection()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "AlternateIDs/all(id: id eq 42)",
           "$it => $it.AlternateIDs.All(id => (id == 42))",
           NotTesting);
    }

    [Fact]
    public void AnyOnComplexCollection()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "AlternateAddresses/any(address: address/City eq 'Redmond')",
           "$it => $it.AlternateAddresses.Any(address => (address.City == \"Redmond\"))",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters,
            new Product { AlternateAddresses = new[] { new Address { City = "Redmond" } } },
            (true, true));

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters,
            new Product(),
           (typeof(ArgumentNullException), false));
    }

    [Fact]
    public void AllOnComplexCollection()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "AlternateAddresses/all(address: address/City eq 'Redmond')",
           "$it => $it.AlternateAddresses.All(address => (address.City == \"Redmond\"))",
           NotTesting);
    }

    [Fact]
    public void RecursiveAllAny()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
           "Category/QueryableProducts/all(p: p/Category/EnumerableProducts/any(o: o/ProductName eq 'Snacks'))",
           "$it => $it.Category.QueryableProducts.All(p => p.Category.EnumerableProducts.Any(o => (o.ProductName == \"Snacks\")))",
           NotTesting);
    }
    #endregion

    #region String Functions
    [Theory]
    [InlineData("Abcd", -1, "Abcd", true, typeof(ArgumentOutOfRangeException))]
    [InlineData("Abcd", 0, "Abcd", true, true)]
    [InlineData("Abcd", 1, "bcd", true, true)]
    [InlineData("Abcd", 3, "d", true, true)]
    [InlineData("Abcd", 4, "", true, true)]
    [InlineData("Abcd", 5, "", true, typeof(ArgumentOutOfRangeException))]
    public void StringSubstringStart(string productName, int index, string compareString, bool trueNullPropagation, object falseNullPropagation)
    {
        // Arrange & Act & Assert
        string filter = $"substring(ProductName, {index}) eq '{compareString}'";
        var filters = BindFilterAndVerify<Product>(filter);

        // Arrange & Act & Assert
        Product product = new Product { ProductName = productName };
        if (falseNullPropagation.GetType() == typeof(bool))
        {
            bool boolValue = (bool)falseNullPropagation;
            InvokeFiltersAndVerify(filters, product, (boolValue, trueNullPropagation));
        }
        else
        {
            Type typeValue = (Type)falseNullPropagation;
            InvokeFiltersAndThrows(filters, product, (typeValue, trueNullPropagation));
        }
    }

    [Theory]
    [InlineData("Abcd", -1, 4, "Abcd", true, typeof(ArgumentOutOfRangeException))]
    [InlineData("Abcd", -1, 3, "Abc", true, typeof(ArgumentOutOfRangeException))]
    [InlineData("Abcd", 0, 1, "A", true, true)]
    [InlineData("Abcd", 0, 4, "Abcd", true, true)]
    [InlineData("Abcd", 0, 3, "Abc", true, true)]
    [InlineData("Abcd", 0, 5, "Abcd", true, typeof(ArgumentOutOfRangeException))]
    [InlineData("Abcd", 1, 3, "bcd", true, true)]
    [InlineData("Abcd", 1, 5, "bcd", true, typeof(ArgumentOutOfRangeException))]
    [InlineData("Abcd", 2, 1, "c", true, true)]
    [InlineData("Abcd", 3, 1, "d", true, true)]
    [InlineData("Abcd", 4, 1, "", true, typeof(ArgumentOutOfRangeException))]
    [InlineData("Abcd", 0, -1, "", true, typeof(ArgumentOutOfRangeException))]
    [InlineData("Abcd", 5, -1, "", true, typeof(ArgumentOutOfRangeException))]
    public void StringSubstringStartAndLength(string productName, int index, int length, string compareString, bool trueNullPropagation, object falseNullPropagation)
    {
        // Arrange & Act & Assert
        string filter = $"substring(ProductName, {index}, {length}) eq '{compareString}'";
        var filters = BindFilterAndVerify<Product>(filter);

        // Arrange & Act & Assert
        Product product = new Product { ProductName = productName };
        if (falseNullPropagation.GetType() == typeof(bool))
        {
            bool boolValue = (bool)falseNullPropagation;
            InvokeFiltersAndVerify(filters, product, (boolValue, trueNullPropagation));
        }
        else
        {
            Type typeValue = (Type)falseNullPropagation;
            InvokeFiltersAndThrows(filters, product, (typeValue, trueNullPropagation));
        }
    }

    [Fact]
    public void StringFunctions_StringContains()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "contains(ProductName, 'Abc')",
            "$it => $it.ProductName.Contains(\"Abc\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Abcd" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Abd" }, (false, false));
    }

    [Fact]
    public void StringFunctions_StringStartsWith()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "startswith(ProductName, 'Abc')",
            "$it => $it.ProductName.StartsWith(\"Abc\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Abcd" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Abd" }, (false, false));
    }

    [Fact]
    public void StringFunctions_StringEndsWith()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "endswith(ProductName, 'Abc')",
            "$it => $it.ProductName.EndsWith(\"Abc\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "AAbc" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Abcd" }, (false, false));
    }

    [Fact]
    public void StringFunctions_StringLength()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "length(ProductName) gt 0",
            "$it => ($it.ProductName.Length > 0)",
            "$it => ((IIF(($it.ProductName == null), null, Convert($it.ProductName.Length)) > Convert(0)) == True)");

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "AAbc" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "" }, (false, false));
    }

    [Fact]
    public void StringFunctions_StringIndexOf()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "indexof(ProductName, 'Abc') eq 5",
            "$it => ($it.ProductName.IndexOf(\"Abc\") == 5)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "12345Abc" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "1234Abc" }, (false, false));
    }

    [Fact]
    public void StringFunctions_StringSubstring()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "substring(ProductName, 3) eq 'uctName'",
            "$it => ($it.ProductName.Substring(3) == \"uctName\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "123uctName" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "1234Abc" }, (false, false));

        // Arrange & Act & Assert

        BindFilterAndVerify<Product>(
            "substring(ProductName, 3, 4) eq 'uctN'",
            "$it => ($it.ProductName.Substring(3, 4) == \"uctN\")",
            NotTesting);
    }

    [Fact]
    public void StringFunctions_StringToLower()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "tolower(ProductName) eq 'tasty treats'",
            "$it => ($it.ProductName.ToLower() == \"tasty treats\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Tasty Treats" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Tasty Treatss" }, (false, false));
    }

    [Fact]
    public void StringFunctions_StringToUpper()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "toupper(ProductName) eq 'TASTY TREATS'",
            "$it => ($it.ProductName.ToUpper() == \"TASTY TREATS\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Tasty Treats" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Tasty Treatss" }, (false, false));
    }

    [Fact]
    public void StringFunctions_StringTrim()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "trim(ProductName) eq 'Tasty Treats'",
            "$it => ($it.ProductName.Trim() == \"Tasty Treats\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(NullReferenceException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "   Tasty Treats   " }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "   Tasty Treatss   " }, (false, false));
    }

    [Fact]
    public void StringFunctions_StringConcat()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "concat(ProductName, 'Bar') eq 'FoodBar'",
            "$it => ($it.ProductName.Concat(\"Bar\") == \"FoodBar\")",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { ProductName = "Food" }, (true, true));
    }

    [Fact]
    public void StringFunctions_StringMatchesPattern()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "matchesPattern(ProductName, 'A\\wc')",
            "$it => $it.ProductName.IsMatch(\"A\\wc\", ECMAScript)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { ProductName = null }, (typeof(ArgumentNullException), false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Abcd" }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Abd" }, (false, false));

        InvokeFiltersAndVerify(filters, new Product { ProductName = "Aθd" }, (false, false)); // ECMAScript has strict matching of \w
    }

    [Fact]
    public void StringFunctions_RecursiveMethodCall()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "floor(floor(UnitPrice)) eq 123m",
            "$it => ($it.UnitPrice.Value.Floor().Floor() == 123)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { }, (typeof(InvalidOperationException), false));
    }
    #endregion

    #region Date Functions
    [Fact]
    public void DateFunctions_DateDay()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "day(DiscontinuedDate) eq 8",
            "$it => ($it.DiscontinuedDate.Value.Day == 8)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { }, (typeof(InvalidOperationException), false));

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { DiscontinuedDate = new DateTime(2000, 10, 8) }, (true, true));
    }

    [Fact]
    public void DateFunctions_DateDayNonNullable()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "day(NonNullableDiscontinuedDate) eq 8",
            "$it => ($it.NonNullableDiscontinuedDate.Day == 8)");
    }

    [Fact]
    public void DateFunctions_DateMonth()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "month(DiscontinuedDate) eq 8",
            "$it => ($it.DiscontinuedDate.Value.Month == 8)",
            NotTesting);
    }

    [Fact]
    public void DateFunctions_DateYear()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "year(DiscontinuedDate) eq 1974",
            "$it => ($it.DiscontinuedDate.Value.Year == 1974)",
            NotTesting);
    }

    [Fact]
    public void DateFunctions_DateHour()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>("hour(DiscontinuedDate) eq 8",
            "$it => ($it.DiscontinuedDate.Value.Hour == 8)",
            NotTesting);
    }

    [Fact]
    public void DateFunctions_DateMinute()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "minute(DiscontinuedDate) eq 12",
            "$it => ($it.DiscontinuedDate.Value.Minute == 12)",
            NotTesting);
    }

    [Fact]
    public void DateFunctions_DateSecond()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "second(DiscontinuedDate) eq 33",
            "$it => ($it.DiscontinuedDate.Value.Second == 33)",
            NotTesting);
    }

    [Theory]
    [InlineData("year(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Year == 100)")]
    [InlineData("month(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Month == 100)")]
    [InlineData("day(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Day == 100)")]
    [InlineData("hour(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Hour == 100)")]
    [InlineData("minute(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Minute == 100)")]
    [InlineData("second(DiscontinuedOffset) eq 100", "$it => ($it.DiscontinuedOffset.Second == 100)")]
    [InlineData("now() eq 2016-11-08Z", "$it => (DateTimeOffset.UtcNow == 11/08/2016 00:00:00 +00:00)")]
    public void DateFunctions_DateTimeOffsetFunctions(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression);
    }

    [Theory]
    [InlineData("year(Birthday) eq 100", "$it => {0}.Year == 100)")]
    [InlineData("month(Birthday) eq 100", "$it => {0}.Month == 100)")]
    [InlineData("day(Birthday) eq 100", "$it => {0}.Day == 100)")]
    [InlineData("hour(Birthday) eq 100", "$it => {0}.Hour == 100)")]
    [InlineData("minute(Birthday) eq 100", "$it => {0}.Minute == 100)")]
    [InlineData("second(Birthday) eq 100", "$it => {0}.Second == 100)")]
    public void DateFunctions_DateTimeFunctions(string filter, string expression)
    {
        // Arrange & Act & Assert
        string expect = string.Format(expression, "($it.Birthday");
        BindFilterAndVerify<Product>(filter, expect);
    }

    [Theory]
    [InlineData("year(NullableDateProperty) eq 2015", "$it => ($it.NullableDateProperty.Value.Year == 2015)")]
    [InlineData("month(NullableDateProperty) eq 12", "$it => ($it.NullableDateProperty.Value.Month == 12)")]
    [InlineData("day(NullableDateProperty) eq 23", "$it => ($it.NullableDateProperty.Value.Day == 23)")]
    public void DateFunctions_DateFunctions_Nullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression, NotTesting);
    }

    [Theory]
    [InlineData("year(DateProperty) eq 2015", "$it => ($it.DateProperty.Year == 2015)")]
    [InlineData("month(DateProperty) eq 12", "$it => ($it.DateProperty.Month == 12)")]
    [InlineData("day(DateProperty) eq 23", "$it => ($it.DateProperty.Day == 23)")]
    public void DateFunctions_DateFunctions_NonNullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression);
    }

    [Theory]
    [InlineData("hour(NullableTimeOfDayProperty) eq 10", "$it => ($it.NullableTimeOfDayProperty.Value.Hours == 10)")]
    [InlineData("minute(NullableTimeOfDayProperty) eq 20", "$it => ($it.NullableTimeOfDayProperty.Value.Minutes == 20)")]
    [InlineData("second(NullableTimeOfDayProperty) eq 30", "$it => ($it.NullableTimeOfDayProperty.Value.Seconds == 30)")]
    public void DateFunctions_TimeOfDayFunctions_Nullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression, NotTesting);
    }

    [Theory]
    [InlineData("hour(TimeOfDayProperty) eq 10", "$it => ($it.TimeOfDayProperty.Hours == 10)")]
    [InlineData("minute(TimeOfDayProperty) eq 20", "$it => ($it.TimeOfDayProperty.Minutes == 20)")]
    [InlineData("second(TimeOfDayProperty) eq 30", "$it => ($it.TimeOfDayProperty.Seconds == 30)")]
    public void DateFunctions_TimeOfDayFunctions_NonNullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression);
    }

    [Theory]
    [InlineData("fractionalseconds(DiscontinuedDate) eq 0.2", "$it => ((Convert($it.DiscontinuedDate.Value.Millisecond) / 1000) == 0.2)")]
    [InlineData("fractionalseconds(NullableTimeOfDayProperty) eq 0.2", "$it => ((Convert($it.NullableTimeOfDayProperty.Value.Milliseconds) / 1000) == 0.2)")]
    public void DateFunctions_FractionalsecondsFunction_Nullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression, NotTesting);
    }

    [Theory]
    [InlineData("fractionalseconds(NonNullableDiscontinuedDate) eq 0.2", "$it => ((Convert($it.NonNullableDiscontinuedDate.Millisecond) / 1000) == 0.2)")]
    [InlineData("fractionalseconds(TimeOfDayProperty) eq 0.2", "$it => ((Convert($it.TimeOfDayProperty.Milliseconds) / 1000) == 0.2)")]
    public void DateFunctions_FractionalsecondsFunction_NonNullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression);
    }

    [Theory]
    [InlineData("date(DiscontinuedDate) eq 2015-02-26",
        "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))")]
    [InlineData("date(DiscontinuedDate) lt 2016-02-26",
        "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))")]
    [InlineData("2015-02-26 ge date(DiscontinuedDate)",
        "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day))")]
    [InlineData("null ne date(DiscontinuedDate)", "$it => (null != $it.DiscontinuedDate)")]
    [InlineData("date(DiscontinuedDate) eq null", "$it => ($it.DiscontinuedDate == null)")]
    public void DateFunctions_DateFunction_Nullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression, NotTesting);
    }

    [Theory]
    [InlineData("date(NonNullableDiscontinuedDate) eq 2015-02-26",
        "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))")]
    [InlineData("date(NonNullableDiscontinuedDate) lt 2016-02-26",
        "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))")]
    [InlineData("2015-02-26 ge date(NonNullableDiscontinuedDate)",
        "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day))")]
    public void DateFunctions_DateFunction_NonNullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression);
    }

    [Theory]
    [InlineData("time(DiscontinuedDate) eq 01:02:03.0040000",
        "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) == ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
    [InlineData("time(DiscontinuedDate) ge 01:02:03.0040000",
        "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) >= ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
    [InlineData("01:02:03.0040000 le time(DiscontinuedDate)",
        "$it => (((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))) <= ((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))))")]
    [InlineData("null ne time(DiscontinuedDate)", "$it => (null != $it.DiscontinuedDate)")]
    [InlineData("time(DiscontinuedDate) eq null", "$it => ($it.DiscontinuedDate == null)")]
    public void DateFunctions_TimeFunction_Nullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression, NotTesting);
    }

    [Theory]
    [InlineData("time(NonNullableDiscontinuedDate) eq 01:02:03.0040000",
        "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) == ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
    [InlineData("time(NonNullableDiscontinuedDate) ge 01:02:03.0040000",
        "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) >= ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))")]
    [InlineData("01:02:03.0040000 le time(NonNullableDiscontinuedDate)",
        "$it => (((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))) <= ((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))))")]
    public void DateFunctions_TimeFunction_NonNullable(string filter, string expression)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression);
    }
    #endregion

    #region Math Functions
    [Fact]
    public void MathFunctions_MathRoundDecimal()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "round(UnitPrice) gt 5.00m",
            string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice.Value.Round() > {0:0.00})", 5.0),
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { UnitPrice = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { UnitPrice = 5.9m }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { UnitPrice = 5.4m }, (false, false));
    }

    [Fact]
    public void MathFunctions_MathRoundDouble()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "round(Weight) gt 5d",
            string.Format(CultureInfo.InvariantCulture, "$it => ($it.Weight.Value.Round() > {0})", 5),
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { Weight = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { Weight = 5.9d }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { Weight = 5.4d }, (false, false));
    }

    [Fact]
    public void MathFunctions_MathRoundFloat()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "round(Width) gt 5f",
            string.Format(CultureInfo.InvariantCulture, "$it => (Convert($it.Width).Value.Round() > {0})", 5),
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { Width = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { Width = 5.9f }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { Width = 5.4f }, (false, false));
    }

    [Fact]
    public void MathFunctions_MathFloorDecimal()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "floor(UnitPrice) eq 5",
            "$it => ($it.UnitPrice.Value.Floor() == 5)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { UnitPrice = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { UnitPrice = 5.4m }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { UnitPrice = 4.4m }, (false, false));
    }

    [Fact]
    public void MathFunctions_MathFloorDouble()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "floor(Weight) eq 5",
            "$it => ($it.Weight.Value.Floor() == 5)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { Weight = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { Weight = 5.4d }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { Weight = 4.4d }, (false, false));
    }

    [Fact]
    public void MathFunctions_MathFloorFloat()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "floor(Width) eq 5",
            "$it => (Convert($it.Width).Value.Floor() == 5)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { Width = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { Width = 5.4f }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { Width = 4.4f }, (false, false));
    }

    [Fact]
    public void MathFunctions_MathCeilingDecimal()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "ceiling(UnitPrice) eq 5",
            "$it => ($it.UnitPrice.Value.Ceiling() == 5)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { UnitPrice = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { UnitPrice = 4.1m }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { UnitPrice = 5.9m }, (false, false));
    }

    [Fact]
    public void MathFunctions_MathCeilingDouble()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "ceiling(Weight) eq 5",
            "$it => ($it.Weight.Value.Ceiling() == 5)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { Weight = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { Weight = 4.1d }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { Weight = 5.9d }, (false, false));
    }

    [Fact]
    public void MathFunctions_MathCeilingFloat()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "ceiling(Width) eq 5",
            "$it => (Convert($it.Width).Value.Ceiling() == 5)",
            NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndThrows(filters, new Product { Width = null }, (typeof(InvalidOperationException), false));

        InvokeFiltersAndVerify(filters, new Product { Width = 4.1f }, (true, true));

        InvokeFiltersAndVerify(filters, new Product { Width = 5.9f }, (false, false));
    }

    [Theory]
    [InlineData("floor(FloatProp) eq floor(FloatProp)")]
    [InlineData("round(FloatProp) eq round(FloatProp)")]
    [InlineData("ceiling(FloatProp) eq ceiling(FloatProp)")]
    [InlineData("floor(DoubleProp) eq floor(DoubleProp)")]
    [InlineData("round(DoubleProp) eq round(DoubleProp)")]
    [InlineData("ceiling(DoubleProp) eq ceiling(DoubleProp)")]
    [InlineData("floor(DecimalProp) eq floor(DecimalProp)")]
    [InlineData("round(DecimalProp) eq round(DecimalProp)")]
    [InlineData("ceiling(DecimalProp) eq ceiling(DecimalProp)")]
    public void MathFunctions_VariousTypes(string filter)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<DataTypes>(filter);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new DataTypes(), (true, true));
    }
    #endregion

    #region Custom Functions
    [Fact]
    public void CustomMethod_InstanceMethodOfDeclaringType()
    {
        // Arrange
        FunctionSignatureWithReturnType padrightStringEdmFunction =
            new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetInt32(false));

        MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });
        const string padrightMethodName = "padright";
        try
        {
            const string productName = "Abcd";
            const int totalWidth = 5;
            const string expectedProductName = "Abcd ";

            // Add the custom function
            // Act & Assert
            CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
            UriFunctionsBinder.BindUriFunctionName(padrightMethodName, padRightStringMethodInfo);

            string filter = string.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);
            var filters = BindFilterAndVerify<Product>(filter);

            InvokeFiltersAndVerify(filters, new Product { ProductName = productName }, (true, true));
        }
        finally
        {
            Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
        }
    }

    [Fact]
    public void CustomMethod_InstanceMethodNotOfDeclaringType()
    {
        // Arrange
        FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetString(true),
                    EdmCoreModel.Instance.GetInt32(false));

        MethodInfo padRightStringMethodInfo = typeof(FilterBinderTests).GetMethod("PadRightInstance", BindingFlags.NonPublic | BindingFlags.Instance);

        const string padrightMethodName = "padright";
        try
        {
            const int totalWidth = 5;
            const string expectedProductName = "Abcd ";

            // Add the custom function
            // Act & Assert
            CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
            UriFunctionsBinder.BindUriFunctionName(padrightMethodName, padRightStringMethodInfo);

            string filter = string.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);

            Action filterToExpression = () => BindFilterAndVerify<Product>(filter);
            ExceptionAssert.Throws(typeof(NotImplementedException),filterToExpression);
        }
        finally
        {
            Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
        }
    }

    [Fact]
    public void CustomMethod_StaticExtensionMethod()
    {
        // Arrange
        FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetInt32(false));

        MethodInfo padRightStringMethodInfo = typeof(StringExtender).GetMethod("PadRightExStatic", BindingFlags.Public | BindingFlags.Static);

        const string padrightMethodName = "padright";
        try
        {
            const string productName = "Abcd";
            const int totalWidth = 5;
            const string expectedProductName = "Abcd ";

            // Add the custom function
            // Act & Assert
            CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
            UriFunctionsBinder.BindUriFunctionName(padrightMethodName, padRightStringMethodInfo);

            string filter = String.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);
            var filters = BindFilterAndVerify<Product>(filter);

            InvokeFiltersAndVerify(filters, new Product { ProductName = productName }, (true, true));
        }
        finally
        {
            Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
        }
    }

    [Fact]
    public void CustomMethod_StaticMethodNotOfDeclaringType()
    {
        // Arrange
        FunctionSignatureWithReturnType padrightStringEdmFunction = new FunctionSignatureWithReturnType(
            EdmCoreModel.Instance.GetString(true),
            EdmCoreModel.Instance.GetString(true),
            EdmCoreModel.Instance.GetInt32(false));

        MethodInfo padRightStringMethodInfo = typeof(FilterBinderTests).GetMethod("PadRightStatic", BindingFlags.NonPublic | BindingFlags.Static);

        const string padrightMethodName = "padright";
        try
        {
            const string productName = "Abcd";
            const int totalWidth = 5;
            const string expectedProductName = "Abcd ";

            // Add the custom function
            // Act & Assert
            CustomUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction);
            UriFunctionsBinder.BindUriFunctionName(padrightMethodName, padRightStringMethodInfo);

            string filter = String.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);
            var filters = BindFilterAndVerify<Product>(filter);

            InvokeFiltersAndVerify(filters, new Product { ProductName = productName }, (true, true));
        }
        finally
        {
            Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
        }
    }

    [Fact]
    public void CustomMethod_AddSignatureAndBindFunctionWithShortcut()
    {
        // Arrange
        FunctionSignatureWithReturnType padrightStringEdmFunction =
                new FunctionSignatureWithReturnType(
                EdmCoreModel.Instance.GetString(true),
                 EdmCoreModel.Instance.GetString(true),
                EdmCoreModel.Instance.GetInt32(false));

        MethodInfo padRightStringMethodInfo = typeof(string).GetMethod("PadRight", new Type[] { typeof(int) });
        const string padrightMethodName = "padright";
        try
        {
            const string productName = "Abcd";
            const int totalWidth = 5;
            const string expectedProductName = "Abcd ";

            // Add the custom function
            // Act & Assert
            ODataUriFunctions.AddCustomUriFunction(padrightMethodName, padrightStringEdmFunction, padRightStringMethodInfo);

            string filter = String.Format("padright(ProductName, {0}) eq '{1}'", totalWidth, expectedProductName);
            var filters = BindFilterAndVerify<Product>(filter);

            InvokeFiltersAndVerify(filters, new Product { ProductName = productName }, (true, true));
        }
        finally
        {
            Assert.True(CustomUriFunctions.RemoveCustomUriFunction(padrightMethodName));
            Assert.True(UriFunctionsBinder.UnbindUriFunctionName(padrightMethodName, padRightStringMethodInfo));
        }
    }
    #endregion

    #region Data Types
    [Fact]
    public void GuidExpression()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<DataTypes>(
            "GuidProp eq 0EFDAECF-A9F0-42F3-A384-1295917AF95E",
            "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");

        // Arrange & Act & Assert - verify case insensitivity ?
        BindFilterAndVerify<DataTypes>(
            "GuidProp eq 0EFDAECF-A9F0-42F3-A384-1295917AF95E",
            "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");
    }

    [Theory]
    [InlineData("DateTimeProp eq 2000-12-12T12:00:00Z", "$it => ($it.DateTimeProp == {0})")]
    [InlineData("DateTimeProp lt 2000-12-12T12:00:00Z", "$it => ($it.DateTimeProp < {0})")]
    // TODO: [InlineData("DateTimeProp ge datetime'2000-12-12T12:00'", "$it => ($it.DateTimeProp >= {0})")] (uriparser fails on optional seconds)
    public void DateTimeExpression(string clause, string expectedExpression)
    {
        // Arrange & Act & Assert
        var dateTime = new DateTimeOffset(new DateTime(2000, 12, 12, 12, 0, 0), TimeSpan.Zero);
        BindFilterAndVerify<DataTypes>(
            clause,
            string.Format(CultureInfo.InvariantCulture, expectedExpression, dateTime));
    }

    [Theory]
    [InlineData("DateTimeOffsetProp eq datetimeoffset'2002-10-10T17:00:00Z'", "$it => ($it.DateTimeOffsetProp == {0})", 0)]
    [InlineData("DateTimeOffsetProp ge datetimeoffset'2002-10-10T17:00:00Z'", "$it => ($it.DateTimeOffsetProp >= {0})", 0)]
    [InlineData("DateTimeOffsetProp le datetimeoffset'2002-10-10T17:00:00-07:00'", "$it => ($it.DateTimeOffsetProp <= {0})", -7)]
    [InlineData("DateTimeOffsetProp eq datetimeoffset'2002-10-10T17:00:00-0600'", "$it => ($it.DateTimeOffsetProp == {0})", -6)]
    [InlineData("DateTimeOffsetProp lt datetimeoffset'2002-10-10T17:00:00-05'", "$it => ($it.DateTimeOffsetProp < {0})", -5)]
    [InlineData("DateTimeOffsetProp ne datetimeoffset'2002-10-10T17:00:00%2B09:30'", "$it => ($it.DateTimeOffsetProp != {0})", 9.5)]
    [InlineData("DateTimeOffsetProp gt datetimeoffset'2002-10-10T17:00:00%2B0545'", "$it => ($it.DateTimeOffsetProp > {0})", 5.75)]
    public void DateTimeOffsetExpression(string clause, string expectedExpression, double offsetHours)
    {
        // Arrange & Act & Assert
        var dateTimeOffset = new DateTimeOffset(2002, 10, 10, 17, 0, 0, TimeSpan.FromHours(offsetHours));

        ExceptionAssert.Throws<ODataException>(
            () => BindFilterAndVerify<DataTypes>(
                clause,
                string.Format(CultureInfo.InvariantCulture, expectedExpression, dateTimeOffset)));
    }

    [Fact]
    public void IntegerLiteralSuffix()
    {
        // Arrange & Act & Assert - long L
        BindFilterAndVerify<DataTypes>(
            "LongProp lt 987654321L and LongProp gt 123456789l",
            "$it => (($it.LongProp < 987654321) AndAlso ($it.LongProp > 123456789))");

        BindFilterAndVerify<DataTypes>(
            "LongProp lt -987654321L and LongProp gt -123456789l",
            "$it => (($it.LongProp < -987654321) AndAlso ($it.LongProp > -123456789))");
    }

    [Fact]
    public void EnumInExpression()
    {
        // Arrange & Act & Assert
        var result = BindFilterAndVerify<DataTypes>(
            "SimpleEnumProp in ('First', 'Second')",
            "$it => System.Collections.Generic.List`1[Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum].Contains($it.SimpleEnumProp)");

        Expression<Func<DataTypes, bool>> expression = result.Item2 as Expression<Func<DataTypes, bool>>;

        var memberAccess = (MemberExpression)((MethodCallExpression)expression.Body).Arguments[0];
        var values = (IList<SimpleEnum>)ExpressionBinderHelper.ExtractParameterizedConstant(memberAccess);
        Assert.Equal(new[] {SimpleEnum.First, SimpleEnum.Second}, values);
    }

    [Fact]
    public void EnumInExpression_WithNullValue_Throws()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => BindFilterAndVerify<DataTypes>("SimpleEnumProp in ('First', null)"),
            "A null value was found with the expected type 'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum[Nullable=False]'. The expected type 'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum[Nullable=False]' does not allow null values.");
    }

    [Fact]
    public void EnumInExpression_NullableEnum_WithNullable()
    {
        // Arrange & Act & Assert
        var result = BindFilterAndVerify<DataTypes>(
            "NullableSimpleEnumProp in ('First', 'Second')",
            "$it => System.Collections.Generic.List`1[System.Nullable`1[Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum]].Contains($it.NullableSimpleEnumProp)");
        Expression<Func<DataTypes, bool>> expression = result.Item2 as Expression<Func<DataTypes, bool>>;

        var memberAccess = (MemberExpression)((MethodCallExpression)expression.Body).Arguments[0];
        var values = (IList<SimpleEnum?>)ExpressionBinderHelper.ExtractParameterizedConstant(memberAccess);
        Assert.Equal(new SimpleEnum?[] {SimpleEnum.First, SimpleEnum.Second}, values);
    }
        
    [Fact]
    public void EnumInExpression_NullableEnum_WithNullValue()
    {
        // Arrange & Act & Assert
        var result = BindFilterAndVerify<DataTypes>(
            "NullableSimpleEnumProp in ('First', null)",
            "$it => System.Collections.Generic.List`1[System.Nullable`1[Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum]].Contains($it.NullableSimpleEnumProp)");
        Expression<Func<DataTypes, bool>> expression = result.Item2 as Expression<Func<DataTypes, bool>>;

        var memberAccess = (MemberExpression)((MethodCallExpression)expression.Body).Arguments[0];
        var values = (IList<SimpleEnum?>)ExpressionBinderHelper.ExtractParameterizedConstant(memberAccess);
        Assert.Equal(new SimpleEnum?[] {SimpleEnum.First, null}, values);
    }

    [Fact]
    public void RealLiteralSuffixes()
    {
        // Arrange & Act & Assert - Float F
        BindFilterAndVerify<DataTypes>(
            "FloatProp lt 4321.56F and FloatProp gt 1234.56f",
            String.Format(CultureInfo.InvariantCulture, "$it => (($it.FloatProp < {0:0.00}) AndAlso ($it.FloatProp > {1:0.00}))", 4321.56, 1234.56));

        // Arrange & Act & Assert - Decimal M
        BindFilterAndVerify<DataTypes>(
            "DecimalProp lt 4321.56M and DecimalProp gt 1234.56m",
            String.Format(CultureInfo.InvariantCulture, "$it => (($it.DecimalProp < {0:0.00}) AndAlso ($it.DecimalProp > {1:0.00}))", 4321.56, 1234.56));
    }

    [Theory]
    [InlineData("'hello,world'", "hello,world")]
    [InlineData("'''hello,world'", "'hello,world")]
    [InlineData("'hello,world'''", "hello,world'")]
    [InlineData("'hello,''wor''ld'", "hello,'wor'ld")]
    [InlineData("'hello,''''''world'", "hello,'''world")]
    [InlineData("'\"hello,world\"'", "\"hello,world\"")]
    [InlineData("'\"hello,world'", "\"hello,world")]
    [InlineData("'hello,world\"'", "hello,world\"")]
    [InlineData("'hello,\"world'", "hello,\"world")]
    [InlineData("'México D.F.'", "México D.F.")]
    [InlineData("'æææøøøååå'", "æææøøøååå")]
    [InlineData("'いくつかのテキスト'", "いくつかのテキスト")]
    public void StringLiterals(string literal, string expected)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "ProductName eq " + literal,
            string.Format("$it => ($it.ProductName == \"{0}\")", expected));
    }

    [Theory]
    [InlineData('$')]
    [InlineData('&')]
    [InlineData('+')]
    [InlineData(',')]
    [InlineData('/')]
    [InlineData(':')]
    [InlineData(';')]
    [InlineData('=')]
    [InlineData('?')]
    [InlineData('@')]
    [InlineData(' ')]
    [InlineData('<')]
    [InlineData('>')]
    [InlineData('#')]
    [InlineData('%')]
    [InlineData('{')]
    [InlineData('}')]
    [InlineData('|')]
    [InlineData('\\')]
    [InlineData('^')]
    [InlineData('~')]
    [InlineData('[')]
    [InlineData(']')]
    [InlineData('`')]
    public void SpecialCharactersInStringLiteral(char c)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "ProductName eq '" + c + "'",
            String.Format("$it => ($it.ProductName == \"{0}\")", c));

        InvokeFiltersAndVerify(filters, new Product { ProductName = c.ToString() }, (true, true));
    }
    #endregion

    #region Casts
    [Fact]
    public void NSCast_OnEnumerableEntityCollection_GeneratesExpression_WithOfTypeOnEnumerable()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "Category/EnumerableProducts/Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/any(p: p/ProductName eq 'ProductName')",
            "$it => $it.Category.EnumerableProducts.OfType().Any(p => (p.ProductName == \"ProductName\"))",
            NotTesting);

        Assert.NotNull(filters.Item1);
        Assert.NotNull(filters.Item2);
    }

    [Fact]
    public void NSCast_OnQueryableEntityCollection_GeneratesExpression_WithOfTypeOnQueryable()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "Category/QueryableProducts/Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/any(p: p/ProductName eq 'ProductName')",
            "$it => $it.Category.QueryableProducts.OfType().Any(p => (p.ProductName == \"ProductName\"))",
            NotTesting);
    }

    [Fact]
    public void NSCast_OnEntityCollection_CanAccessDerivedInstanceProperty()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
            "Category/Products/Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/any(p: p/DerivedProductName eq 'DerivedProductName')");

        InvokeFiltersAndVerify(
            filters,
            new Product { Category = new Category { Products = new Product[] { new DerivedProduct { DerivedProductName = "DerivedProductName" } } } },
            (true, true));

        InvokeFiltersAndVerify(
            filters,
            new Product { Category = new Category { Products = new Product[] { new DerivedProduct { DerivedProductName = "NotDerivedProductName" } } } },
            (false, false));
    }

    [Fact]
    public void NSCast_OnSingleEntity_GeneratesExpression_WithAsOperator()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            "Microsoft.AspNetCore.OData.Tests.Models.Product/ProductName eq 'ProductName'",
            "$it => (($it As Product).ProductName == \"ProductName\")",
            NotTesting);
    }

    [Theory]
    [InlineData("Microsoft.AspNetCore.OData.Tests.Models.Product/ProductName eq 'ProductName'")]
    [InlineData("Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/DerivedProductName eq 'DerivedProductName'")]
    [InlineData("Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/CategoryID eq 123")]
    [InlineData("Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/CategoryID eq 123")]
    public void Inheritance_WithDerivedInstance(string filter)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<DerivedProduct>(filter);

        InvokeFiltersAndVerify<DerivedProduct>(filters,
          new DerivedProduct { Category = new DerivedCategory { CategoryID = 123 }, ProductName = "ProductName", DerivedProductName = "DerivedProductName" },
          (true, true));
    }

    [Theory]
    [InlineData("Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/DerivedProductName eq 'ProductName'")]
    [InlineData("Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/CategoryID eq 123")]
    [InlineData("Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/CategoryID eq 123")]
    public void Inheritance_WithBaseInstance(string filter)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(filter);

        InvokeFiltersAndThrows(filters, new Product(), (typeof(NullReferenceException), false));
    }

    [Fact]
    public void CastToNonDerivedType_Throws()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => BindFilterAndVerify<Product>("Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/CategoryID eq 123"),
            "Encountered invalid type cast. 'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory' is not assignable from 'Microsoft.AspNetCore.OData.Tests.Models.Product'.");
    }

    [Theory]
    [InlineData("Edm.Int32 eq 123", "A binary operator with incompatible types was detected. Found operand types 'Edm.String' and 'Edm.Int32' for operator kind 'Equal'.")]
    [InlineData("ProductName/Edm.String eq 123", "A binary operator with incompatible types was detected. Found operand types 'Edm.String' and 'Edm.Int32' for operator kind 'Equal'.")]
    public void CastToNonEntityType_Throws(string filter, string error)
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => BindFilterAndVerify<Product>(filter), error);
    }

    [Theory]
    [InlineData("Edm.NonExistentType eq 123")]
    [InlineData("Category/Edm.NonExistentType eq 123")]
    [InlineData("Category/Products/Edm.NonExistentType eq 123")]
    public void CastToNonExistantType_Throws(string filter)
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => BindFilterAndVerify<Product>(filter),
            "The child type 'Edm.NonExistentType' in a cast was not an entity type. Casts can only be performed on entity types.");
    }
    #endregion

    #region cast in query option
    [Theory]
    [InlineData("cast(null,Edm.Int16) eq null", "$it => (null == null)")]
    [InlineData("cast(null,Edm.Int32) eq 123", "$it => (null == Convert(123))")]
    [InlineData("cast(null,Edm.Int64) ne 123", "$it => (null != Convert(123))")]
    [InlineData("cast(null,Edm.Single) ne 123", "$it => (null != Convert(123))")]
    [InlineData("cast(null,Edm.Double) ne 123", "$it => (null != Convert(123))")]
    [InlineData("cast(null,Edm.Decimal) ne 123", "$it => (null != Convert(123))")]
    [InlineData("cast(null,Edm.Boolean) ne true", "$it => (null != Convert(True))")]
    [InlineData("cast(null,Edm.Byte) ne 1", "$it => (null != Convert(1))")]
    [InlineData("cast(null,Edm.Guid) eq 00000000-0000-0000-0000-000000000000", "$it => (null == Convert(00000000-0000-0000-0000-000000000000))")]
    [InlineData("cast(null,Edm.String) ne '123'", "$it => (null != \"123\")")]
    [InlineData("cast(null,Edm.DateTimeOffset) eq 2001-01-01T12:00:00.000+08:00", "$it => (null == Convert(01/01/2001 12:00:00 +08:00))")]
    [InlineData("cast(null,Edm.Duration) eq duration'P8DT23H59M59.9999S'", "$it => (null == Convert(8.23:59:59.9999000))")]
    [InlineData("cast(null,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') eq null", "$it => (null == null)")]
    [InlineData("cast(null,'Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum') eq null", "$it => (null == null)")]
    [InlineData("cast(IntProp,Edm.String) eq '123'", "$it => (Convert($it.IntProp.ToString()) == \"123\")")]
    [InlineData("cast(LongProp,Edm.String) eq '123'", "$it => (Convert($it.LongProp.ToString()) == \"123\")")]
    [InlineData("cast(SingleProp,Edm.String) eq '123'", "$it => (Convert($it.SingleProp.ToString()) == \"123\")")]
    [InlineData("cast(DoubleProp,Edm.String) eq '123'", "$it => (Convert($it.DoubleProp.ToString()) == \"123\")")]
    [InlineData("cast(DecimalProp,Edm.String) eq '123'", "$it => (Convert($it.DecimalProp.ToString()) == \"123\")")]
    [InlineData("cast(BoolProp,Edm.String) eq '123'", "$it => (Convert($it.BoolProp.ToString()) == \"123\")")]
    [InlineData("cast(ByteProp,Edm.String) eq '123'", "$it => (Convert($it.ByteProp.ToString()) == \"123\")")]
    [InlineData("cast(GuidProp,Edm.String) eq '123'", "$it => (Convert($it.GuidProp.ToString()) == \"123\")")]
    [InlineData("cast(StringProp,Edm.String) eq '123'", "$it => (Convert($it.StringProp) == \"123\")")]
    [InlineData("cast(DateTimeOffsetProp,Edm.String) eq '123'", "$it => (Convert($it.DateTimeOffsetProp.ToString()) == \"123\")")]
    [InlineData("cast(TimeSpanProp,Edm.String) eq '123'", "$it => (Convert($it.TimeSpanProp.ToString()) == \"123\")")]
    [InlineData("cast(SimpleEnumProp,Edm.String) eq '123'", "$it => (Convert(Convert($it.SimpleEnumProp).ToString()) == \"123\")")]
    [InlineData("cast(FlagsEnumProp,Edm.String) eq '123'", "$it => (Convert(Convert($it.FlagsEnumProp).ToString()) == \"123\")")]
    [InlineData("cast(LongEnumProp,Edm.String) eq '123'", "$it => (Convert(Convert($it.LongEnumProp).ToString()) == \"123\")")]
    [InlineData("cast(NullableIntProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableIntProp.HasValue, $it.NullableIntProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableLongProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableLongProp.HasValue, $it.NullableLongProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableSingleProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableSingleProp.HasValue, $it.NullableSingleProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableDoubleProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableDoubleProp.HasValue, $it.NullableDoubleProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableDecimalProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableDecimalProp.HasValue, $it.NullableDecimalProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableBoolProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableBoolProp.HasValue, $it.NullableBoolProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableByteProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableByteProp.HasValue, $it.NullableByteProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableGuidProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableGuidProp.HasValue, $it.NullableGuidProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableDateTimeOffsetProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableDateTimeOffsetProp.HasValue, $it.NullableDateTimeOffsetProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableTimeSpanProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableTimeSpanProp.HasValue, $it.NullableTimeSpanProp.Value.ToString(), null)) == \"123\")")]
    [InlineData("cast(NullableSimpleEnumProp,Edm.String) eq '123'", "$it => (Convert(IIF($it.NullableSimpleEnumProp.HasValue, Convert($it.NullableSimpleEnumProp.Value).ToString(), null)) == \"123\")")]
    [InlineData("cast(IntProp,Edm.Int64) eq 123", "$it => (Convert($it.IntProp) == 123)")]
    [InlineData("cast(NullableLongProp,Edm.Double) eq 1.23", "$it => (Convert($it.NullableLongProp) == Convert(1.23))")]
    [InlineData("cast(2147483647,Edm.Int16) ne null", "$it => (Convert(Convert(2147483647)) != null)")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'1',Edm.String) eq '1'", "$it => (Convert(Convert(Second).ToString()) == \"1\")")]
    [InlineData("cast(cast(cast(IntProp,Edm.Int64),Edm.Int16),Edm.String) eq '123'", "$it => (Convert(Convert(Convert($it.IntProp)).ToString()) == \"123\")")]
    [InlineData("cast('123',Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) ne null", "$it => (Convert(123) != null)")]
    public void CastMethod_Succeeds(string filter, string expectedResult)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<DataTypes>(filter, expectedResult, NotTesting);
    }

    [Theory]
    [InlineData("cast(NoSuchProperty,Edm.Int32) ne null",
        "Could not find a property named 'NoSuchProperty' on type 'Microsoft.AspNetCore.OData.Tests.Models.DataTypes'.")]
    public void Cast_UndefinedSource_ThrowsODataException(string filter, string errorMessage)
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<DataTypes>(filter), errorMessage);
    }

    public static TheoryDataSet<string, string> CastToUnquotedUndefinedTarget
    {
        get
        {
            return new TheoryDataSet<string, string>
            {
                { "cast(Edm.DateTime) eq null", "Edm.DateTime" },
                { "cast(Edm.Unknown) eq null", "Edm.Unknown" },
                { "cast(null,Edm.DateTime) eq null", "Edm.DateTime" },
                { "cast(null,Edm.Unknown) eq null", "Edm.Unknown" },
                { "cast('2001-01-01T12:00:00.000',Edm.DateTime) eq null", "Edm.DateTime" },
                { "cast('2001-01-01T12:00:00.000',Edm.Unknown) eq null", "Edm.Unknown" },
                { "cast(DateTimeProp,Edm.DateTime) eq null", "Edm.DateTime" },
                { "cast(DateTimeProp,Edm.Unknown) eq null", "Edm.Unknown" },
            };
        }
    }

    // Exception messages here and in CastQuotedUndefinedTarget_ThrowsODataException should be consistent.
    // Worse, this message is incorrect -- casts can be performed on most types but _not_ entity types.
    [Theory]
    [MemberData(nameof(CastToUnquotedUndefinedTarget))]
    public void CastToUnquotedUndefinedTarget_ThrowsODataException(string filter, string typeName)
    {
        // Arrange
        var expectedMessage = string.Format(
            "The child type '{0}' in a cast was not an entity type. Casts can only be performed on entity types.",
            typeName);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<DataTypes>(filter), expectedMessage);
    }

    public static TheoryDataSet<string> CastToQuotedUndefinedTarget
    {
        get
        {
            return new TheoryDataSet<string>
            {
                { "cast('Edm.DateTime') eq null" },
                { "cast('Edm.Unknown') eq null" },
                { "cast(null,'Edm.DateTime') eq null" },
                { "cast(null,'Edm.Unknown') eq null" },
                { "cast('2001-01-01T12:00:00.000','Edm.DateTime') eq null" },
                { "cast('','Edm.Unknown') eq null" },
                { "cast(DateTimeProp,'Edm.DateTime') eq null" },
                { "cast(IntProp,'Edm.Unknown') eq null" },
            };
        }
    }

    [Theory]
    [MemberData(nameof(CastToQuotedUndefinedTarget))]
    public void CastToQuotedUndefinedTarget_ThrowsODataException(string filter)
    {
        // Arrange
        var expectedMessage = "Cast or IsOf Function must have a type in its arguments.";

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<DataTypes>(filter), expectedMessage);
    }

    [Theory]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) ne null")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum) ne null")]
    [InlineData("cast(0,Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) ne null")]
    [InlineData("cast(0,Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum) ne null")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'0',Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) ne null")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum'0',Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum) ne null")]
    [InlineData("cast(SimpleEnumProp,Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) ne null")]
    [InlineData("cast(FlagsEnumProp,Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum) ne null")]
    [InlineData("cast(NullableSimpleEnumProp,Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) ne null")]
    [InlineData("cast(IntProp,Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) ne null")]
    [InlineData("cast(DateTimeOffsetProp,Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) ne null")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'1',Edm.Int32) eq 1")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum'1',Edm.Int32) eq 1")]
    [InlineData("cast(SimpleEnumProp,Edm.Int32) eq 123")]
    [InlineData("cast(FlagsEnumProp,Edm.Int32) eq 123")]
    [InlineData("cast(NullableSimpleEnumProp,Edm.Guid) ne null")]

    [InlineData("cast('Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') ne null")]
    [InlineData("cast('Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum') ne null")]
    [InlineData("cast(0,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') ne null")]
    [InlineData("cast(0,'Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum') ne null")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'0','Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') ne null")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum'0','Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum') ne null")]
    [InlineData("cast(SimpleEnumProp,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') ne null")]
    [InlineData("cast(FlagsEnumProp,'Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum') ne null")]
    [InlineData("cast(NullableSimpleEnumProp,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') ne null")]
    [InlineData("cast(IntProp,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') ne null")]
    [InlineData("cast(DateTimeOffsetProp,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') ne null")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'1','Edm.Int32') eq 1")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum'1','Edm.Int32') eq 1")]
    [InlineData("cast(SimpleEnumProp,'Edm.Int32') eq 123")]
    [InlineData("cast(FlagsEnumProp,'Edm.Int32') eq 123")]
    [InlineData("cast(NullableSimpleEnumProp,'Edm.Guid') ne null")]
    public void Cast_UnsupportedSourceOrTargetForEnumCast_Throws(string filter)
    {
        // Arrange & Act & Assert
        // TODO : 1824 Should not throw exception for invalid enum cast in query option.
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<DataTypes>(filter), "Enumeration type value can only be casted to or from string.");
    }

    [Theory]
    [InlineData("cast(IntProp,Edm.DateTimeOffset) eq null")]
    [InlineData("cast(ByteProp,Edm.Guid) eq null")]
    [InlineData("cast(NullableLongProp,Edm.Duration) eq null")]
    [InlineData("cast(StringProp,Edm.Double) eq null")]
    [InlineData("cast(StringProp,Edm.Int16) eq null")]
    [InlineData("cast(DateTimeOffsetProp,Edm.Int32) eq null")]
    [InlineData("cast(NullableGuidProp,Edm.Int64) eq null")]
    [InlineData("cast(Edm.Int32) eq null")]
    [InlineData("cast($it,Edm.String) eq null")]
    [InlineData("cast(ComplexProp,Edm.Double) eq null")]
    [InlineData("cast(ComplexProp,Edm.String) eq null")]
    [InlineData("cast(StringProp,Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum) eq null")]
    [InlineData("cast(StringProp,Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum) eq null")]
    public void Cast_UnsupportedTarget_ReturnsNull(string filter)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<DataTypes>(filter, "$it => (null == null)");
    }

    // See OtherFunctions_SomeTwoParameterCasts_ThrowODataException and OtherFunctions_SomeSingleParameterCasts_ThrowODataException
    // in FilterQueryValidatorTest.  ODL's ODataQueryOptionParser and FunctionCallBinder call the code throwing these exceptions.
    [Theory]
    [InlineData("cast(null,Microsoft.AspNetCore.OData.Tests.Models.Address) ne null",
        "Encountered invalid type cast. 'Microsoft.AspNetCore.OData.Tests.Models.Address' is not assignable from '<null>'.")]
    [InlineData("cast(null,Microsoft.AspNetCore.OData.Tests.Models.DataTypes) ne null",
        "Encountered invalid type cast. 'Microsoft.AspNetCore.OData.Tests.Models.DataTypes' is not assignable from '<null>'.")]
    public void Cast_NonPrimitiveTarget_ThrowsODataException(string filter, string expectErrorMessage)
    {
        // Arrange & Act & Assert
        // TODO : 1827 Should not throw when the target type of cast is not primitive or enumeration type.
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<DataTypes>(filter), expectErrorMessage);
    }

    [Theory]
    [InlineData("cast(null,'Edm.Int32') ne null")]
    [InlineData("cast(StringProp,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') eq null")]
    [InlineData("cast(IntProp,'Edm.String') eq '123'")]
    [InlineData("cast('Microsoft.AspNetCore.OData.Tests.Models.DataTypes') eq null")]
    [InlineData("cast($it,'Microsoft.AspNetCore.OData.Tests.Models.DataTypes') eq null")]
    public void SingleQuotesOnTypeNameOfCast_WorksForNow(string filter)
    {
        // Arrange
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<DataTypes>("Customers");
        IEdmModel model = builder.GetEdmModel();
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("Customers");
        IEdmEntityType entityType = entitySet.EntityType;
        var parser = new ODataQueryOptionParser(model, entityType, entitySet,
            new Dictionary<string, string> { { "$filter", filter } });

        // Act & Assert
        // TODO : 1927 ODL parser should throw when there are single quotes on type name of cast.
        Assert.NotNull(parser.ParseFilter());
    }

    [Fact]
    public void SingleQuotesOnEnumTypeNameOfCast_WorksForNow()
    {
        // Arrange
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<DataTypes>("Customers");
        IEdmModel model = builder.GetEdmModel();
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("Customers");
        IEdmEntityType entityType = entitySet.EntityType;
        var parser = new ODataQueryOptionParser(model, entityType, entitySet,
            new Dictionary<string, string>
            {
                { "$filter", "cast(StringProp,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum') eq null" }
            });

        // Act
        // TODO : 1927 ODL parser should throw when there are single quotes on type name of cast.
        FilterClause filterClause = parser.ParseFilter();

        // Assert
        Assert.NotNull(filterClause);
        var castNode = Assert.IsType<SingleValueFunctionCallNode>(((BinaryOperatorNode)filterClause.Expression).Left);
        Assert.Equal("cast", castNode.Name);
        Assert.Equal("Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum", ((ConstantNode)castNode.Parameters.Last()).Value);
    }

    public static TheoryDataSet<string> CastToQuotedPrimitiveType
    {
        get
        {
            return new TheoryDataSet<string>
            {
                { "cast('Edm.Binary') eq null" },
                { "cast('Edm.Boolean') eq null" },
                { "cast('Edm.Byte') eq null" },
                { "cast('Edm.DateTimeOffset') eq null" },
                { "cast('Edm.Decimal') eq null" },
                { "cast('Edm.Double') eq null" },
                { "cast('Edm.Duration') eq null" },
                { "cast('Edm.Guid') eq null" },
                { "cast('Edm.Int16') eq null" },
                { "cast('Edm.Int32') eq null" },
                { "cast('Edm.Int64') eq null" },
                { "cast('Edm.SByte') eq null" },
                { "cast('Edm.Single') eq null" },
                { "cast('Edm.String') eq null" },

                { "cast(null,'Edm.Binary') eq null" },
                { "cast(null,'Edm.Boolean') eq null" },
                { "cast(null,'Edm.Byte') eq null" },
                { "cast(null,'Edm.DateTimeOffset') eq null" },
                { "cast(null,'Edm.Decimal') eq null" },
                { "cast(null,'Edm.Double') eq null" },
                { "cast(null,'Edm.Duration') eq null" },
                { "cast(null,'Edm.Guid') eq null" },
                { "cast(null,'Edm.Int16') eq null" },
                { "cast(null,'Edm.Int32') eq null" },
                { "cast(null,'Edm.Int64') eq null" },
                { "cast(null,'Edm.SByte') eq null" },
                { "cast(null,'Edm.Single') eq null" },
                { "cast(null,'Edm.String') eq null" },

                { "cast(binary'T0RhdGE=','Edm.Binary') eq binary'T0RhdGE='" },
                { "cast(false,'Edm.Boolean') eq false" },
                { "cast(23,'Edm.Byte') eq 23" },
                { "cast(2001-01-01T12:00:00.000+08:00,'Edm.DateTimeOffset') eq 2001-01-01T12:00:00.000+08:00" },
                { "cast(23,'Edm.Decimal') eq 23" },
                { "cast(23,'Edm.Double') eq 23" },
                { "cast(duration'PT12H','Edm.Duration') eq duration'PT12H'" },
                { "cast(00000000-0000-0000-0000-000000000000,'Edm.Guid') eq 00000000-0000-0000-0000-000000000000" },
                { "cast(23,'Edm.Int16') eq 23" },
                { "cast(23,'Edm.Int32') eq 23" },
                { "cast(23,'Edm.Int64') eq 23" },
                { "cast(23,'Edm.SByte') eq 23" },
                { "cast(23,'Edm.Single') eq 23" },
                { "cast('hello','Edm.String') eq 'hello'" },

                { "cast(ByteArrayProp,'Edm.Binary') eq null" },
                { "cast(BoolProp,'Edm.Boolean') eq true" },
                { "cast(DateTimeOffsetProp,'Edm.DateTimeOffset') eq 2001-01-01T12:00:00.000+08:00" },
                { "cast(DecimalProp,'Edm.Decimal') eq 23" },
                { "cast(DoubleProp,'Edm.Double') eq 23" },
                { "cast(TimeSpanProp,'Edm.Duration') eq duration'PT23H'" },
                { "cast(GuidProp,'Edm.Guid') eq 0EFDAECF-A9F0-42F3-A384-1295917AF95E" },
                { "cast(NullableShortProp,'Edm.Int16') eq 23" },
                { "cast(IntProp,'Edm.Int32') eq 23" },
                { "cast(LongProp,'Edm.Int64') eq 23" },
                { "cast(FloatProp,'Edm.Single') eq 23" },
                { "cast(StringProp,'Edm.String') eq 'hello'" },
            };
        }
    }

    [Theory]
    [MemberData(nameof(CastToQuotedPrimitiveType))]
    public void CastToQuotedPrimitiveType_Succeeds(string filter)
    {
        // Arrange
        var model = new DataTypes
        {
            BoolProp = true,
            DateTimeOffsetProp = DateTimeOffset.Parse("2001-01-01T12:00:00.000+08:00"),
            DecimalProp = 23,
            DoubleProp = 23,
            GuidProp = Guid.Parse("0EFDAECF-A9F0-42F3-A384-1295917AF95E"),
            NullableShortProp = 23,
            IntProp = 23,
            LongProp = 23,
            FloatProp = 23,
            StringProp = "hello",
            TimeSpanProp = TimeSpan.FromHours(23),
        };

        // Act & Assert
        var filters = BindFilterAndVerify<DataTypes>(filter);
        InvokeFiltersAndVerify(filters, model, (true,true));
    }

    [Theory]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.Address) eq null", "Microsoft.AspNetCore.OData.Tests.Models.Address", "Microsoft.AspNetCore.OData.Tests.Models.Product")]
    [InlineData("cast(null, Microsoft.AspNetCore.OData.Tests.Models.Address) eq null", "Microsoft.AspNetCore.OData.Tests.Models.Address", "<null>")]
    [InlineData("cast('', Microsoft.AspNetCore.OData.Tests.Models.Address) eq null", "Microsoft.AspNetCore.OData.Tests.Models.Address", "Edm.String")]
    [InlineData("cast(null, Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory)/DerivedCategoryName eq null", "Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory", "<null>")]
    public void CastToUnrelatedUnquotedTypeParameter_ThrowsEncounteredInvalidTypeCast(string filter, string propertyFullQualifiedName, string assignableFrom)
    {
        // Arrange
        var expectedMessage =
            $"Encountered invalid type cast. '{propertyFullQualifiedName}' is not assignable from '{assignableFrom}'.";

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<Product>(filter), expectedMessage);
    }

    public static TheoryDataSet<string> CastToQuotedComplexType
    {
        get
        {
            return new TheoryDataSet<string>
            {
                { "cast('Microsoft.AspNetCore.OData.Tests.Models.Address') eq null" },
                { "cast(null, 'Microsoft.AspNetCore.OData.Tests.Models.Address') eq null" },
                { "cast('', 'Microsoft.AspNetCore.OData.Tests.Models.Address') eq null" },
                { "cast(SupplierAddress, 'Microsoft.AspNetCore.OData.Tests.Models.Address') ne null" },
            };
        }
    }

    [Theory]
    [MemberData(nameof(CastToQuotedComplexType))]
    public void CastToQuotedComplexType_Succeeds(string filter)
    {
        // Arrange
        var model = new Product
        {
            SupplierAddress = new Address { City = "Redmond", },
        };

        // Act & Assert
        var filters = BindFilterAndVerify<Product>(filter);

        InvokeFiltersAndVerify(filters, model, (true, true));
    }

    [Theory]
    [InlineData("cast(SupplierAddress, Microsoft.AspNetCore.OData.Tests.Models.Address) eq null")]
    [InlineData("cast(Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct)/DerivedProductName eq null")]
    [InlineData("cast(Category, Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory)/DerivedCategoryName eq null")]
    public void CastToRelatedUnquotedEntityType_DoesNotThrowODataException(string filter)
    {
        // Arrange & Act & Assert
        var exception = Record.Exception(() => BindFilterAndVerify<Product>(filter));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("cast('Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct')/DerivedProductName eq null", "$it => (($it As DerivedProduct).DerivedProductName == null)","$it => (IIF((($it As DerivedProduct) == null), null, ($it As DerivedProduct).DerivedProductName) == null)")]
    [InlineData("cast(Category,'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory')/DerivedCategoryName eq null", "$it => (($it.Category As DerivedCategory).DerivedCategoryName == null)", "$it => (IIF((($it.Category As DerivedCategory) == null), null, ($it.Category As DerivedCategory).DerivedCategoryName) == null)")]
    public void CastToQuotedEntityOrComplexType_DerivedProductName(string filter, string expectedExpression, string expectedExpressionWithNullCheck)
    {
        // Arrange, Act & Assert
        BindFilterAndVerify<Product>(filter, expectedExpression, expectedExpressionWithNullCheck);
    }
    #endregion

    #region 'isof' in query option

    [Theory]
    [InlineData("isof(Edm.Int16)", "$it => IIF(($it Is System.Int16), True, False)")]
    [InlineData("isof('Microsoft.AspNetCore.OData.Tests.Models.Product')", "$it => IIF(($it Is Microsoft.AspNetCore.OData.Tests.Models.Product), True, False)")]
    [InlineData("isof(ProductName,Edm.String)", "$it => IIF(($it.ProductName Is System.String), True, False)")]
    [InlineData("isof(Category,'Microsoft.AspNetCore.OData.Tests.Models.Category')", "$it => IIF(($it.Category Is Microsoft.AspNetCore.OData.Tests.Models.Category), True, False)")]
    [InlineData("isof(Category,'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory')", "$it => IIF(($it.Category Is Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory), True, False)")]
    [InlineData("isof(Ranking, 'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum')", "$it => IIF(($it.Ranking Is Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum), True, False)")]
    public void IsofMethod_Succeeds(string filter, string expectedResult)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expectedResult, NotTesting);
    }

    [Theory]
    [InlineData("isof(null)")]
    [InlineData("isof(ProductName,null)")]
    public void Isof_WithNullTypeName_ThrowsArgumentNullException(string filter)
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ArgumentNullException>(() => BindFilterAndVerify<Product>(filter),
            "Value cannot be null. (Parameter 'typeName')");
    }

    [Theory]
    [InlineData("isof(NoSuchProperty,Edm.Int32)",
        "Could not find a property named 'NoSuchProperty' on type 'Microsoft.AspNetCore.OData.Tests.Models.DataTypes'.")]
    public void IsOfUndefinedSource_ThrowsODataException(string filter, string errorMessage)
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<DataTypes>(filter), errorMessage);
    }

    [Theory]
    [InlineData("isof(null,Edm.Binary)")]
    [InlineData("isof(null,Edm.Boolean)")]
    [InlineData("isof(null,Edm.Byte)")]
    [InlineData("isof(null,Edm.DateTimeOffset)")]
    [InlineData("isof(null,Edm.Decimal)")]
    [InlineData("isof(null,Edm.Double)")]
    [InlineData("isof(null,Edm.Duration)")]
    [InlineData("isof(null,Edm.Guid)")]
    [InlineData("isof(null,Edm.Int16)")]
    [InlineData("isof(null,Edm.Int32)")]
    [InlineData("isof(null,Edm.Int64)")]
    [InlineData("isof(null,Edm.SByte)")]
    [InlineData("isof(null,Edm.Single)")]
    [InlineData("isof(null,Edm.Stream)")]
    [InlineData("isof(null,Edm.String)")]
    [InlineData("isof(null,Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum)")]
    [InlineData("isof(null,Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum)")]

    [InlineData("isof(ByteArrayProp,Edm.Binary)")] // ByteArrayProp == null
    [InlineData("isof(IntProp,Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum)")]
    [InlineData("isof(NullableShortProp,'Edm.Int16')")] // NullableShortProp == null

    [InlineData("isof('Edm.Binary')")]
    [InlineData("isof('Edm.Boolean')")]
    [InlineData("isof('Edm.Byte')")]
    [InlineData("isof('Edm.DateTimeOffset')")]
    [InlineData("isof('Edm.Decimal')")]
    [InlineData("isof('Edm.Double')")]
    [InlineData("isof('Edm.Duration')")]
    [InlineData("isof('Edm.Guid')")]
    [InlineData("isof('Edm.Int16')")]
    [InlineData("isof('Edm.Int32')")]
    [InlineData("isof('Edm.Int64')")]
    [InlineData("isof('Edm.SByte')")]
    [InlineData("isof('Edm.Single')")]
    [InlineData("isof('Edm.Stream')")]
    [InlineData("isof('Edm.String')")]
    [InlineData("isof('Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum')")]
    [InlineData("isof('Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum')")]

    [InlineData("isof(23,'Edm.Byte')")]
    [InlineData("isof(23,'Edm.Decimal')")]
    [InlineData("isof(23,'Edm.Double')")]
    [InlineData("isof(23,'Edm.Int16')")]
    [InlineData("isof(23,'Edm.Int64')")]
    [InlineData("isof(23,'Edm.SByte')")]
    [InlineData("isof(23,'Edm.Single')")]
    [InlineData("isof('hello','Edm.Stream')")]
    [InlineData("isof(0,'Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum')")]
    [InlineData("isof(0,'Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum')")]

    [InlineData("isof('2001-01-01T12:00:00.000+08:00','Edm.DateTimeOffset')")] // source is string
    [InlineData("isof('00000000-0000-0000-0000-000000000000','Edm.Guid')")] // source is string
    [InlineData("isof('23','Edm.Byte')")]
    [InlineData("isof('23','Edm.Int16')")]
    [InlineData("isof('23','Edm.Int32')")]
    [InlineData("isof('false','Edm.Boolean')")]
    [InlineData("isof('OData','Edm.Binary')")]
    [InlineData("isof('PT12H','Edm.Duration')")]
    [InlineData("isof(23,'Edm.String')")]
    [InlineData("isof('0','Microsoft.AspNetCore.OData.Tests.Models.FlagsEnum')")]
    [InlineData("isof('0','Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum')")]
    public void IsOfPrimitiveType_Succeeds_WithFalse(string filter)
    {
        // Arrange
        var model = new DataTypes();

        // Act & Assert
        var filters = BindFilterAndVerify<DataTypes>(filter);
        InvokeFiltersAndVerify(filters, model, (false, false));
    }

    // Exception messages here and in IsOfQuotedUndefinedTarget_ThrowsODataException should be consistent.
    // Worse, this message is incorrect -- casts can be performed on most types but _not_ entity types and
    // isof can't be performed.
    [Theory]
    [InlineData("isof(Edm.DateTime)", "Edm.DateTime")]
    [InlineData("isof(Edm.Unknown)", "Edm.Unknown")]
    [InlineData("isof(null,Edm.DateTime)", "Edm.DateTime")]
    [InlineData("isof(null,Edm.Unknown)", "Edm.Unknown")]
    [InlineData("isof('2001-01-01T12:00:00.000',Edm.DateTime)", "Edm.DateTime")]
    [InlineData("isof('',Edm.Unknown)", "Edm.Unknown")]
    [InlineData("isof(DateTimeProp,Edm.DateTime)", "Edm.DateTime")]
    [InlineData("isof(IntProp,Edm.Unknown)", "Edm.Unknown")]
    public void IsOfUndefinedTarget_ThrowsODataException(string filter, string typeName)
    {
        // Arrange
        var expectedMessage = string.Format(
            "The child type '{0}' in a cast was not an entity type. Casts can only be performed on entity types.",
            typeName);

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<DataTypes>(filter), expectedMessage);
    }

    [Theory]
    [InlineData("isof('Edm.DateTime')")]
    [InlineData("isof('Edm.Unknown')")]
    [InlineData("isof(null,'Edm.DateTime')")]
    [InlineData("isof(null,'Edm.Unknown')")]
    [InlineData("isof('2001-01-01T12:00:00.000','Edm.DateTime')")]
    [InlineData("isof('','Edm.Unknown')")]
    [InlineData("isof(DateTimeProp,'Edm.DateTime')")]
    [InlineData("isof(IntProp,'Edm.Unknown')")]
    public void IsOfQuotedUndefinedTarget_ThrowsODataException(string filter)
    {
        // Arrange
        var expectedMessage = "Cast or IsOf Function must have a type in its arguments.";

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<DataTypes>(filter), expectedMessage);
    }

    [Theory]
    [InlineData("isof(Microsoft.AspNetCore.OData.Tests.Models.Address)", "Microsoft.AspNetCore.OData.Tests.Models.Address", "Microsoft.AspNetCore.OData.Tests.Models.Product")]
    [InlineData("isof(null,Microsoft.AspNetCore.OData.Tests.Models.Address)", "Microsoft.AspNetCore.OData.Tests.Models.Address", "<null>")]
    [InlineData("isof(null, Microsoft.AspNetCore.OData.Tests.Models.Address)", "Microsoft.AspNetCore.OData.Tests.Models.Address", "<null>")]
    [InlineData("isof(null, Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory)", "Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory", "<null>")]
    [InlineData("isof(null,    Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory)", "Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory", "<null>")]
    public void IsOfUnquotedComplexType_ThrowsODataException(string filter, string source, string assignableFrom)
    {
        // Arrange
        var expectedMessage = $"Encountered invalid type cast. '{source}' is not assignable from '{assignableFrom}'.";

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<Product>(filter), expectedMessage);
    }

    [Theory]
    [InlineData("isof(SupplierAddress,Microsoft.AspNetCore.OData.Tests.Models.Address)")]
    [InlineData("isof(SupplierAddress, Microsoft.AspNetCore.OData.Tests.Models.Address)")]
    public void IsOfUnquotedComplexType_DoesNotThrowODataException(string filter)
    {
        // Arrange
        var exception = Record.Exception(() => BindFilterAndVerify<Product>(filter));

        // Act & Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData("isof(Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct)")]
    [InlineData("isof('Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct')")]
    [InlineData("isof(Category,Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory)")]
    [InlineData("isof(Category,  Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory)")]
    [InlineData("isof(Category,  'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory')")]
    [InlineData("isof(Category, Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory)")]
    [InlineData("isof(Category, 'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory')")]
    public void IsOfUnquotedEntityType_DoesNotThrowODataException(string filter)
    {
        // Arrange & Act & Assert
        var exception = Record.Exception(() => BindFilterAndVerify<Product>(filter));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("isof('Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct')")]
    [InlineData("isof(SupplierAddress,'Microsoft.AspNetCore.OData.Tests.Models.Address')")]
    [InlineData("isof(SupplierAddress, 'Microsoft.AspNetCore.OData.Tests.Models.Address')")]
    [InlineData("isof(Category,'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory')")]
    [InlineData("isof(Category, 'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory')")]
    public void IsOfQuotedNonPrimitiveType_Succeeds(string filter)
    {
        // Arrange
        var model = new DerivedProduct
        {
            SupplierAddress = new Address { City = "Redmond", },
            Category = new DerivedCategory { DerivedCategoryName = "DerivedCategory" }
        };

        // Act & Assert
        var filters = BindFilterAndVerify<Product>(filter);
        InvokeFiltersAndVerify<Product>(filters, model, (true, true));
    }

    [Theory]
    [InlineData("isof(null,'Microsoft.AspNetCore.OData.Tests.Models.Address')")]
    [InlineData("isof(null, 'Microsoft.AspNetCore.OData.Tests.Models.Address')")]
    [InlineData("isof(null,'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory')")]
    [InlineData("isof(null, 'Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory')")]
    public void IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalse(string filter)
    {
        // Arrange
        var model = new DerivedProduct
        {
            SupplierAddress = new Address { City = "Redmond", },
            Category = new DerivedCategory { DerivedCategoryName = "DerivedCategory" }
        };

        // Act & Assert
        var filters = BindFilterAndVerify<Product>(filter);
        InvokeFiltersAndVerify<Product>(filters, model, (false, false));
    }
    #endregion

#if false
    [Fact]
    public void BindForNodeOnFilterBinder_ThrowsArgumentNull_Node()
    {
        // Arrange
        ODataQuerySettings settings = new ODataQuerySettings();
        IEdmModel model = EdmCoreModel.Instance;
        IAssemblyResolver resolver = new Mock<IAssemblyResolver>().Object;
        FilterBinder binder = new FilterBinder(settings, resolver, model);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.Bind(null), "node");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindDynamicPropertyAccessQueryNode(null), "openNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindSingleResourceFunctionCallNode(null), "node");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindSingleResourceCastNode(null), "node");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindCollectionResourceCastNode(null), "node");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindBinaryOperatorNode(null), "binaryOperatorNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindInNode(null), "inNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindRangeVariable(null), "rangeVariable");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindCollectionPropertyAccessNode(null), "propertyAccessNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindCollectionComplexNode(null), "collectionComplexNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindPropertyAccessQueryNode(null), "propertyAccessNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindSingleComplexNode(null), "singleComplexNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindUnaryOperatorNode(null), "unaryOperatorNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindAllNode(null), "allNode");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => binder.BindAnyNode(null), "anyNode");
    }

    [Fact]
    public void BindOnFilterBinder_ThrowsNotSupported_InvalidNode()
    {
        // Arrange
        ODataQuerySettings settings = new ODataQuerySettings();
        IEdmModel model = EdmCoreModel.Instance;
        IAssemblyResolver resolver = new Mock<IAssemblyResolver>().Object;
        FilterBinder binder = new FilterBinder(settings, resolver, model);

        MyNoneQueryNode node = new MyNoneQueryNode();

        // Act & Assert
        ExceptionAssert.Throws<NotSupportedException>(() => binder.Bind(node),
            "Binding OData QueryNode of kind 'None' is not supported by 'FilterBinder'.");
    }
#endif

#region parameter alias for filter query option

    [Theory]
    // Parameter alias value is not null.
    [InlineData("IntProp eq @p", "1", "$it => ($it.IntProp == 1)")]
    [InlineData("BoolProp eq @p", "true", "$it => ($it.BoolProp == True)")]
    [InlineData("LongProp eq @p", "-123", "$it => ($it.LongProp == Convert(-123))")]
    [InlineData("FloatProp eq @p", "1.23", "$it => ($it.FloatProp == 1.23)")]
    [InlineData("DoubleProp eq @p", "4.56", "$it => ($it.DoubleProp == Convert(4.56))")]
    [InlineData("StringProp eq @p", "'abc'", "$it => ($it.StringProp == \"abc\")")]
    [InlineData("DateTimeOffsetProp eq @p", "2001-01-01T12:00:00.000+08:00", "$it => ($it.DateTimeOffsetProp == 01/01/2001 12:00:00 +08:00)")]
    [InlineData("TimeSpanProp eq @p", "duration'P8DT23H59M59.9999S'", "$it => ($it.TimeSpanProp == 8.23:59:59.9999000)")]
    [InlineData("GuidProp eq @p", "00000000-0000-0000-0000-000000000000", "$it => ($it.GuidProp == 00000000-0000-0000-0000-000000000000)")]
    [InlineData("SimpleEnumProp eq @p", "Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'First'", "$it => (Convert($it.SimpleEnumProp) == 0)")]
    // Parameter alias value is null.
    [InlineData("NullableIntProp eq @p", "null", "$it => ($it.NullableIntProp == null)")]
    [InlineData("NullableBoolProp eq @p", "null", "$it => ($it.NullableBoolProp == null)")]
    [InlineData("NullableLongProp eq @p", "null", "$it => ($it.NullableLongProp == null)")]
    [InlineData("NullableSingleProp eq @p", "null", "$it => ($it.NullableSingleProp == null)")]
    [InlineData("NullableDoubleProp eq @p", "null", "$it => ($it.NullableDoubleProp == null)")]
    [InlineData("StringProp eq @p", "null", "$it => ($it.StringProp == null)")]
    [InlineData("NullableDateTimeOffsetProp eq @p", "null", "$it => ($it.NullableDateTimeOffsetProp == null)")]
    [InlineData("NullableTimeSpanProp eq @p", "null", "$it => ($it.NullableTimeSpanProp == null)")]
    [InlineData("NullableGuidProp eq @p", "null", "$it => ($it.NullableGuidProp == null)")]
    [InlineData("NullableSimpleEnumProp eq @p", "null", "$it => (Convert($it.NullableSimpleEnumProp) == null)")]
    // Parameter alias value is property.
    [InlineData("@p eq 1", "IntProp", "$it => ($it.IntProp == 1)")]
    [InlineData("@p eq true", "NullableBoolProp", "$it => ($it.NullableBoolProp == Convert(True))")]
    [InlineData("@p eq -123", "LongProp", "$it => ($it.LongProp == -123)")]
    [InlineData("@p eq 1.23", "FloatProp", "$it => ($it.FloatProp == 1.23)")]
    [InlineData("@p eq 4.56", "NullableDoubleProp", "$it => ($it.NullableDoubleProp == Convert(4.56))")]
    [InlineData("@p eq 'abc'", "StringProp", "$it => ($it.StringProp == \"abc\")")]
    [InlineData("@p eq 2001-01-01T12:00:00.000+08:00", "DateTimeOffsetProp", "$it => ($it.DateTimeOffsetProp == 01/01/2001 12:00:00 +08:00)")]
    [InlineData("@p eq duration'P8DT23H59M59.9999S'", "TimeSpanProp", "$it => ($it.TimeSpanProp == 8.23:59:59.9999000)")]
    [InlineData("@p eq 00000000-0000-0000-0000-000000000000", "GuidProp", "$it => ($it.GuidProp == 00000000-0000-0000-0000-000000000000)")]
    [InlineData("@p eq Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum'First'", "SimpleEnumProp", "$it => (Convert($it.SimpleEnumProp) == 0)")]
    // Parameter alias value has built-in functions.
    [InlineData("@p eq 'abc'", "substring(StringProp,5)", "$it => ($it.StringProp.Substring(5) == \"abc\")")]
    [InlineData("2 eq @p", "IntProp add 1", "$it => (2 == ($it.IntProp + 1))")]
    [InlineData("EntityProp/AlternateAddresses/all(a: a/City ne @p)", "'abc'", "$it => $it.EntityProp.AlternateAddresses.All(a => (a.City != \"abc\"))")]
    public void ParameterAlias_Succeeds(string filter, string parameterAliasValue, string expectedResult)
    {
        // Arrange
        IEdmModel model = GetModel<DataTypes>();
        IEdmType targetEdmType = model.FindType("Microsoft.AspNetCore.OData.Tests.Models.DataTypes");
        IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("Microsoft.AspNetCore.OData.Tests.Models.Products");
        IDictionary<string, string> queryOptions = new Dictionary<string, string> { { "$filter", filter } };
        queryOptions.Add("@p", parameterAliasValue);
        ODataQueryOptionParser parser = new ODataQueryOptionParser(model, targetEdmType, targetNavigationSource, queryOptions);
        ODataQueryContext context = new ODataQueryContext(model, typeof(DataTypes));
        context.RequestContainer = new MockServiceProvider();
        FilterClause filterClause = new FilterQueryOption(filter, context, parser).FilterClause;

        // Act
        Expression actualExpression = FilterBinderTestsHelper.TestBind(
            filterClause,
            typeof(DataTypes),
            model,
            AssemblyResolverHelper.Default,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False });

        // Assert
        VerifyExpression(actualExpression, expectedResult);
    }

    [Theory]
    [InlineData("NullableIntProp eq @p", "$it => ($it.NullableIntProp == null)")]
    [InlineData("NullableBoolProp eq @p", "$it => ($it.NullableBoolProp == null)")]
    [InlineData("NullableDoubleProp eq @p", "$it => ($it.NullableDoubleProp == null)")]
    [InlineData("StringProp eq @p", "$it => ($it.StringProp == null)")]
    [InlineData("NullableDateTimeOffsetProp eq @p", "$it => ($it.NullableDateTimeOffsetProp == null)")]
    [InlineData("NullableSimpleEnumProp eq @p", "$it => (Convert($it.NullableSimpleEnumProp) == null)")]
    [InlineData("EntityProp/AlternateAddresses/any(a: a/City eq @p)", "$it => $it.EntityProp.AlternateAddresses.Any(a => (a.City == null))")]
    public void ParameterAlias_AssumedToBeNull_ValueNotFound(string filter, string expectedResult)
    {
        // Arrange
        IEdmModel model = GetModel<DataTypes>();
        IEdmType targetEdmType = model.FindType("Microsoft.AspNetCore.OData.Tests.Models.DataTypes");
        IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("Microsoft.AspNetCore.OData.Tests.Models.Products");
        IDictionary<string, string> queryOptions = new Dictionary<string, string> { { "$filter", filter } };
        ODataQueryOptionParser parser = new ODataQueryOptionParser(model, targetEdmType, targetNavigationSource, queryOptions);
        ODataQueryContext context = new ODataQueryContext(model, typeof(DataTypes));
        context.RequestContainer = new MockServiceProvider();
        FilterClause filterClause = new FilterQueryOption(filter, context, parser).FilterClause;

        // Act
        Expression actualExpression = FilterBinderTestsHelper.TestBind(
            filterClause,
            typeof(DataTypes),
            model,
            AssemblyResolverHelper.Default,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False });

        // Assert
        VerifyExpression(actualExpression, expectedResult);
    }

    [Fact]
    public void ParameterAlias_NestedCase_Succeeds()
    {
        // Arrange
        IEdmModel model = GetModel<DataTypes>();
        IEdmType targetEdmType = model.FindType("Microsoft.AspNetCore.OData.Tests.Models.DataTypes");
        IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("Microsoft.AspNetCore.OData.Tests.Models.Products");

        ODataQueryOptionParser parser = new ODataQueryOptionParser(
            model,
            targetEdmType,
            targetNavigationSource,
            new Dictionary<string, string> { { "$filter", "IntProp eq @p1" }, { "@p1", "@p2" }, { "@p2", "123" } });

        ODataQueryContext context = new ODataQueryContext(model, typeof(DataTypes));
        context.RequestContainer = new MockServiceProvider();
        FilterClause filterClause = new FilterQueryOption("IntProp eq @p1", context, parser).FilterClause;

        // Act
        Expression actualExpression = FilterBinderTestsHelper.TestBind(
            filterClause,
            typeof(DataTypes),
            model,
            AssemblyResolverHelper.Default,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False });

        // Assert
        VerifyExpression(actualExpression, "$it => ($it.IntProp == 123)");
    }

    [Fact]
    public void ParameterAlias_Throws_NotStartWithAt()
    {
        // Arrange
        IEdmModel model = GetModel<DataTypes>();
        IEdmType targetEdmType = model.FindType("Microsoft.AspNetCore.OData.Tests.Models.DataTypes");
        IEdmNavigationSource targetNavigationSource = model.FindDeclaredEntitySet("Microsoft.AspNetCore.OData.Tests.Models.Products");

        ODataQueryOptionParser parser = new ODataQueryOptionParser(
            model,
            targetEdmType,
            targetNavigationSource,
            new Dictionary<string, string> { { "$filter", "IntProp eq #p" }, { "#p", "123" } });

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => parser.ParseFilter(),
            "Syntax error: character '#' is not valid at position 11 in 'IntProp eq #p'.");
    }
#endregion

    [Theory]
    [InlineData("ByteArrayProp eq binary'I6v/'", "$it => ($it.ByteArrayProp == System.Byte[])", true, true)]
    [InlineData("ByteArrayProp ne binary'I6v/'", "$it => ($it.ByteArrayProp != System.Byte[])", false, false)]
    [InlineData("binary'I6v/' eq binary'I6v/'", "$it => (System.Byte[] == System.Byte[])", true, true)]
    [InlineData("binary'I6v/' ne binary'I6v/'", "$it => (System.Byte[] != System.Byte[])", false, false)]
    [InlineData("ByteArrayPropWithNullValue ne binary'I6v/'", "$it => ($it.ByteArrayPropWithNullValue != System.Byte[])", true, true)]
    [InlineData("ByteArrayPropWithNullValue ne ByteArrayPropWithNullValue", "$it => ($it.ByteArrayPropWithNullValue != $it.ByteArrayPropWithNullValue)", false, false)]
    [InlineData("ByteArrayPropWithNullValue ne null", "$it => ($it.ByteArrayPropWithNullValue != null)", false, false)]
    [InlineData("ByteArrayPropWithNullValue eq null", "$it => ($it.ByteArrayPropWithNullValue == null)", true, true)]
    [InlineData("null ne ByteArrayPropWithNullValue", "$it => (null != $it.ByteArrayPropWithNullValue)", false, false)]
    [InlineData("null eq ByteArrayPropWithNullValue", "$it => (null == $it.ByteArrayPropWithNullValue)", true, true)]
    public void ByteArrayComparisons(string filter, string expression, bool falseNullPropagation, bool trueNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<DataTypes>(filter, expression, NotTesting);
        InvokeFiltersAndVerify(filters,
            new DataTypes
            {
                ByteArrayProp = new byte[] { 35, 171, 255 }
            },
            (falseNullPropagation, trueNullPropagation));
    }

    [Theory]
    [InlineData("binary'AP8Q' ge binary'AP8Q'", "GreaterThanOrEqual")]
    [InlineData("binary'AP8Q' le binary'AP8Q'", "LessThanOrEqual")]
    [InlineData("binary'AP8Q' lt binary'AP8Q'", "LessThan")]
    [InlineData("binary'AP8Q' gt binary'AP8Q'", "GreaterThan")]
    [InlineData("binary'AP8Q' add binary'AP8Q'", "Add")]
    [InlineData("binary'AP8Q' sub binary'AP8Q'", "Subtract")]
    [InlineData("binary'AP8Q' mul binary'AP8Q'", "Multiply")]
    [InlineData("binary'AP8Q' div binary'AP8Q'", "Divide")]
    public void DisAllowed_ByteArrayComparisons(string filter, string op)
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(
            () => BindFilterAndVerify<DataTypes>(filter),
            string.Format(CultureInfo.InvariantCulture, "A binary operator with incompatible types was detected. Found operand types 'Edm.Binary' and 'Edm.Binary' for operator kind '{0}'.", op));
    }

    [Theory]
    [InlineData("NullableUShortProp eq 12", "$it => (Convert($it.NullableUShortProp.Value) == Convert(12))")]
    [InlineData("NullableULongProp eq 12L", "$it => (Convert($it.NullableULongProp.Value) == Convert(12))")]
    [InlineData("NullableUIntProp eq 12", "$it => (Convert($it.NullableUIntProp.Value) == Convert(12))")]
    [InlineData("NullableCharProp eq 'a'", "$it => ($it.NullableCharProp.Value.ToString() == \"a\")")]
    public void Nullable_NonstandardEdmPrimitives(string filter, string expression)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<DataTypes>(filter, expression, NotTesting);

        InvokeFiltersAndThrows(filters,
            new DataTypes(),
            (typeof(InvalidOperationException), false));
    }

    [Theory]
    [InlineData("Category/Product/ProductID in (1)", "$it => System.Collections.Generic.List`1[System.Int32].Contains($it.Category.Product.ProductID)", "$it => System.Collections.Generic.List`1[System.Int32].Cast().Contains(IIF((IIF(($it.Category == null), null, $it.Category.Product) == null), null, Convert($it.Category.Product.ProductID)))")]
    [InlineData("Category/Product/GuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7)", "$it => System.Collections.Generic.List`1[System.Guid].Contains($it.Category.Product.GuidProperty)", "$it => System.Collections.Generic.List`1[System.Guid].Cast().Contains(IIF((IIF(($it.Category == null), null, $it.Category.Product) == null), null, Convert($it.Category.Product.GuidProperty)))")]
    [InlineData("Category/Product/NullableGuidProperty in (dc75698b-581d-488b-9638-3e28dd51d8f7)", "$it => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains($it.Category.Product.NullableGuidProperty)", "$it => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(IIF((IIF(($it.Category == null), null, $it.Category.Product) == null), null, $it.Category.Product.NullableGuidProperty))")]
    public void InOnNavigation(string filter, string expression, string expressionWithNullPropagation)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(filter, expression, expressionWithNullPropagation);
    }

    [Fact]
    public void MultipleConstants_Are_Parameterized()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>("ProductName eq '1' or ProductName eq '2' or ProductName eq '3' or ProductName eq '4'",
            "$it => (((($it.ProductName == \"1\") OrElse ($it.ProductName == \"2\")) OrElse ($it.ProductName == \"3\")) OrElse ($it.ProductName == \"4\"))",
            NotTesting);
    }

    [Fact]
    public void Constants_Are_Not_Parameterized_IfDisabled()
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>("ProductName eq '1'", settingsCustomizer: (settings) =>
            {
                settings.EnableConstantParameterization = false;
            });

        Assert.Equal("$it => ($it.ProductName == \"1\")", (filters.Item1 as Expression).ToString());
    }

    [Fact]
    public void CollectionConstants_Are_Parameterized()
    {
        // Arrange & Act & Assert
        var result = BindFilterAndVerify<Product>("ProductName in ('Prod1', 'Prod2')",
            "$it => System.Collections.Generic.List`1[System.String].Contains($it.ProductName)");

        Expression<Func<Product, bool>> expression = result.Item2 as Expression<Func<Product, bool>>;

        var memberAccess = (MemberExpression)((MethodCallExpression)expression.Body).Arguments[0];
        var values = (IList<string>)ExpressionBinderHelper.ExtractParameterizedConstant(memberAccess);
        Assert.Equal(new[] { "Prod1", "Prod2" }, values);
    }

    [Fact]
    public void CollectionConstants_Are_Not_Parameterized_If_Disabled()
    {
        // Arrange & Act & Assert
        var result = BindFilterAndVerify<Product>("ProductName in ('Prod1', 'Prod2')",
            "$it => System.Collections.Generic.List`1[System.String].Contains($it.ProductName)",
            settingsCustomizer: (settings) =>
            {
                settings.EnableConstantParameterization = false;
            });

        Expression<Func<Product, bool>> expression = result.Item2 as Expression<Func<Product, bool>>;
        var values = (IList<string>)((ConstantExpression)((MethodCallExpression)expression.Body).Arguments[0]).Value;
        Assert.Equal(new[] { "Prod1", "Prod2" }, values);
    }

    [Fact]
    public void CollectionConstants_OfEnums_Are_Not_Parameterized_If_Disabled()
    {
        // Arrange & Act & Assert
        var result = BindFilterAndVerify<DataTypes>(
            "SimpleEnumProp in ('First', 'Second')",
            "$it => System.Collections.Generic.List`1[Microsoft.AspNetCore.OData.Tests.Models.SimpleEnum].Contains($it.SimpleEnumProp)",
            settingsCustomizer: (settings) =>
            {
                settings.EnableConstantParameterization = false;
            });

        Expression<Func<DataTypes, bool>> expression = result.Item2 as Expression<Func<DataTypes, bool>>;
        var values = (IList<SimpleEnum>)((ConstantExpression)((MethodCallExpression)expression.Body).Arguments[0]).Value;
        Assert.Equal(new[] { SimpleEnum.First, SimpleEnum.Second }, values);
    }

    [Fact]
    public void FilterByDynamicProperty()
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<DynamicProduct>("Token eq '1'",
            "$it => (Convert(IIF($it.ProductProperties.ContainsKey(Token), $it.ProductPropertiesToken, null)) == \"1\")",
            "$it => (Convert(IIF((($it.ProductProperties != null) AndAlso $it.ProductProperties.ContainsKey(Token)), $it.ProductPropertiesToken, null)) == \"1\")");
    }

    [Theory]
    [InlineData(new[] { 1, 2, 42 }, true)]
    [InlineData(new[] { 1, 2 }, false)]
    public void InOnPrimitiveCollectionPropertyOnRHS(int[] alternateIds, bool withNullPropagation)
    {
        // Arrange & Act & Assert
        var filters = BindFilterAndVerify<Product>(
           "42 in AlternateIDs",
           "$it => $it.AlternateIDs.Contains(42)",
           NotTesting);

        // Arrange & Act & Assert
        InvokeFiltersAndVerify(filters, new Product { AlternateIDs = alternateIds }, (withNullPropagation, withNullPropagation));
    }

    [Theory]
    [InlineData("AlternateAddresses/$count gt 2", "$it => ($it.AlternateAddresses.LongCount() > 2)", "$it => ((IIF(($it.AlternateAddresses == null), null, Convert($it.AlternateAddresses.LongCount())) > Convert(2)) == True)")] // Products?$filter=AlternateAddresses/$count gt 2
    [InlineData("Category/Products/$count($filter=ProductID gt 2) gt 2", "$it => ($it.Category.Products.Where($it => ($it.ProductID > 2)).LongCount() > 2)", "$it => ((IIF((IIF(($it.Category == null), null, $it.Category.Products).Where($it => ($it.ProductID > 2)) == null), null, Convert(IIF(($it.Category == null), null, $it.Category.Products).Where($it => ($it.ProductID > 2)).LongCount())) > Convert(2)) == True)")] // Products?$filter=Category/Products/$count($filter=ProductID gt 2) gt 2
    public void CountExpression(string clause, string expectedExpression, string expectedExpressionWithNullPropagation)
    {
        // Arrange & Act & Assert
        BindFilterAndVerify<Product>(
            clause,
            expectedExpression,
            expectedExpressionWithNullPropagation);
    }

#region Negative Tests

    [Fact]
    public void TypeMismatchInComparison()
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ODataException>(() => BindFilterAndVerify<Product>("length(123) eq 12"));
    }
#endregion

#region Helpers
    internal static void InvokeFiltersAndThrows<T>((Expression, Expression) filters, T instance, (Type, bool) expectedValue)
    {
        ExceptionAssert.Throws(expectedValue.Item1, () => InvokeFilter(instance, filters.Item1));

        bool expected = InvokeFilter(instance, filters.Item2);
        Assert.Equal(expectedValue.Item2, expected);
    }

    internal static void InvokeFiltersAndVerify<T>((Expression, Expression) filters, T instance, (bool, bool) expectedValue)
    {
        bool expected = InvokeFilter(instance, filters.Item1);
        Assert.Equal(expectedValue.Item1, expected);

        expected = InvokeFilter(instance, filters.Item2);
        Assert.Equal(expectedValue.Item2, expected);
    }

    public static bool InvokeFilter<T>(T instance, Expression filter)
    {
        Expression<Func<T, bool>> filterExpression = filter as Expression<Func<T, bool>>;
        Assert.NotNull(filterExpression);

        return filterExpression.Compile().Invoke(instance);
    }

    internal static Expression BindFilter(IEdmModel model, FilterClause filterClause, Type elementType, ODataQuerySettings querySettings, IAssemblyResolver resolver = null)
    {
        IFilterBinder binder = new FilterBinder();
        QueryBinderContext context = new QueryBinderContext(model, querySettings, elementType)
        {
            AssembliesResolver = resolver ?? AssemblyResolverHelper.Default,
        };

        return binder.BindFilter(filterClause, context);
    }

    /// <summary>
    /// Returns (FalseNullProp Expression,  TrueNullProp Expression)
    /// If expectedTrueNullPropagation is same as expectedFalseNullPropagation, don't need provide its value.
    /// </summary>
    internal static (Expression, Expression) BindFilterAndVerify<T>(string filter,
        string expectedFalseNullPropagation = null,
        string expectedTrueNullPropagation = null,
        Action<ODataQuerySettings> settingsCustomizer = null,
        IAssemblyResolver assembliesResolver = null) where T : class
    {
        Type elementType = typeof(T);
        IEdmModel model = GetModel<T>();
        FilterClause filterClause = CreateFilterClause(filter, model, elementType);
        Assert.NotNull(filterClause);

        // HandleNullPropagation == false
        ODataQuerySettings querySettings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False };
        settingsCustomizer?.Invoke(querySettings);

        Expression filterExprFalseNull = BindFilter(model, filterClause, elementType, querySettings, assembliesResolver);
        if (expectedFalseNullPropagation != null && expectedFalseNullPropagation != NotTesting)
        {
            VerifyExpression(filterExprFalseNull, expectedFalseNullPropagation);
        }

        // HandleNullPropagation == true
        querySettings = new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True };
        settingsCustomizer?.Invoke(querySettings);
        Expression filterExprTrueNull = BindFilter(model, filterClause, elementType, querySettings, assembliesResolver);

        if (expectedTrueNullPropagation != NotTesting)
        {
            string nullPropagation = expectedTrueNullPropagation ?? expectedFalseNullPropagation; // Same expected
            if (nullPropagation != null)
            {
                VerifyExpression(filterExprTrueNull, nullPropagation);
            }
        }

        return (filterExprFalseNull, filterExprTrueNull);
    }

    private static void VerifyExpression(Expression filter, string expectedExpression)
    {
        // strip off the beginning part of the expression to get to the first
        // actual query operator
        string resultExpression = ExpressionStringBuilder.ToString(filter);
        Assert.True(resultExpression == expectedExpression,
            string.Format("Expected expression '{0}' but the deserializer produced '{1}'", expectedExpression, resultExpression));
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

    private static IEdmModel GetModel<T>() where T : class
    {
        Type key = typeof(T);
        IEdmModel value;

        if (!_modelCache.TryGetValue(key, out value))
        {
            ODataModelBuilder model = new ODataConventionModelBuilder();
            model.EntitySet<T>("Entities");
            if (key == typeof(Product))
            {
                model.EntityType<DerivedProduct>().DerivesFrom<Product>();
                model.EntityType<DerivedCategory>().DerivesFrom<Category>();
            }

            value = _modelCache[key] = model.GetEdmModel();
        }

        return value;
    }

    private T? ToNullable<T>(object value) where T : struct
    {
        return value == null ? null : (T?)Convert.ChangeType(value, typeof(T));
    }

    // Used by Custom Method binder tests - by reflection
    private string PadRightInstance(string str, int number)
    {
        return str.PadRight(number);
    }

    // Used by Custom Method binder tests - by reflection
    private static string PadRightStatic(string str, int number)
    {
        return str.PadRight(number);
    }

    #endregion
}

// Used by Custom Method binder tests - by reflection
public static class StringExtender
{
    public static string PadRightExStatic(this string str, int width)
    {
        return str.PadRight(width);
    }
}
