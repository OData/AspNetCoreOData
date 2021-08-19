//-----------------------------------------------------------------------------
// <copyright file="SkipQueryOptionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class SkipQueryOptionTests
    {
        [Fact]
        public void ConstructorNullContextThrows()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() => new SkipQueryOption("1", null));
        }

        [Fact]
        public void ConstructorNullRawValueThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() =>
                new SkipQueryOption(null, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() =>
                new SkipQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorNullQueryOptionParserThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() =>
                new SkipQueryOption("5", new ODataQueryContext(model, typeof(Customer)), queryOptionParser: null),
                "queryOptionParser");
        }

        [Theory]
        [InlineData("2")]
        [InlineData("100")]
        [InlineData("0")]
        [InlineData("-1")]
        public void CanConstructValidFilterQuery(string skipValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var skip = new SkipQueryOption(skipValue, context);

            Assert.Same(context, skip.Context);
            Assert.Equal(skipValue, skip.RawValue);
        }

        //[Theory]
        //[InlineData("NotANumber")]
        //[InlineData("''")]
        //[InlineData(" ")]
        //public void ApplyInValidSkipQueryThrows(string skipValue)
        //{
        //    var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        //    var context = new ODataQueryContext(model, typeof(Customer));
        //    var skip = new SkipQueryOption(skipValue, context);

        //    ExceptionAssert.Throws<ODataException>(() =>
        //        skip.ApplyTo(ODataQueryOptionTest.Customers, new ODataQuerySettings()));
        //}

        [Theory]
        [InlineData("0", 0)]
        [InlineData("100", 100)]
        public void Value_Returns_ParsedSkipValue(string skipValue, int expectedValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var skip = new SkipQueryOption(skipValue, context);

            Assert.Equal(expectedValue, skip.Value);
        }

        [Theory]
        [InlineData("NotANumber")]
        [InlineData("''")]
        [InlineData(" ")]
        [InlineData("-1")]
        [InlineData("6926906880")]
        public void Value_ThrowsODataException_ForInvalidValues(string skipValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var skip = new SkipQueryOption(skipValue, context);

            ExceptionAssert.Throws<ODataException>(() => skip.Value);
        }

        [Fact]
        public void CanApplySkip()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var skipOption = new SkipQueryOption("1", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { Id = 1, Name = "Andy" },
                new Customer { Id = 2, Name = "Aaron" },
                new Customer { Id = 3, Name = "Alex" }
            }).AsQueryable();

            var results = skipOption.ApplyTo(customers, new ODataQuerySettings()).ToArray();
            Assert.Equal(2, results.Length);
            Assert.Equal(2, results[0].Id);
            Assert.Equal(3, results[1].Id);
        }

        [Fact]
        public void CanApplySkipOrderby()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
            var orderbyOption = new OrderByQueryOption("Name", context);
            var skipOption = new SkipQueryOption("1", context);

            var customers = (new List<Customer>{
                new Customer { Id = 1, Name = "Andy" },
                new Customer { Id = 2, Name = "Aaron" },
                new Customer { Id = 3, Name = "Alex" }
            }).AsQueryable();

            IQueryable queryable = orderbyOption.ApplyTo(customers);
            queryable = skipOption.ApplyTo(queryable, new ODataQuerySettings());
            var results = ((IQueryable<Customer>)queryable).ToArray();
            Assert.Equal(2, results.Length);
            Assert.Equal(3, results[0].Id);
            Assert.Equal(1, results[1].Id);
        }

        //[Fact]
        //public void CanTurnOffValidationForSkip()
        //{
        //    // Arrange
        //    ODataValidationSettings settings = new ODataValidationSettings()
        //    {
        //        MaxSkip = 10
        //    };
        //    SkipQueryOption option = new SkipQueryOption("11", ValidationTestHelper.CreateCustomerContext());

        //    // Act and Assert
        //    ExceptionAssert.Throws<ODataException>(() =>
        //        option.Validate(settings),
        //        "The limit of '10' for Skip query has been exceeded. The value from the incoming request is '11'.");
        //    option.Validator = null;
        //    ExceptionAssert.DoesNotThrow(() => option.Validate(settings));
        //}

        [Fact]
        public void Property_Value_WorksWithUnTypedContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            SkipQueryOption skip = new SkipQueryOption("42", context);

            // Act & Assert
            Assert.Equal(42, skip.Value);
        }

        [Fact]
        public void ApplyTo_WithUnTypedContext_Throws_InvalidOperation()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            SkipQueryOption skip = new SkipQueryOption("42", context);
            IQueryable queryable = new Mock<IQueryable>().Object;

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => skip.ApplyTo(queryable, new ODataQuerySettings()),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }
    }
}
