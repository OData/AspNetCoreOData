// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class TopQueryOptionTests
    {
        [Fact]
        public void ConstructorNullContextThrows()
        {
            ExceptionAssert.Throws<ArgumentNullException>(() => new TopQueryOption("1", null));
        }

        [Fact]
        public void ConstructorNullRawValueThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() =>
                new TopQueryOption(null, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorEmptyRawValueThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(() =>
                new TopQueryOption(string.Empty, new ODataQueryContext(model, typeof(Customer))));
        }

        [Fact]
        public void ConstructorNullQueryOptionParserThrows()
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() =>
                new TopQueryOption("7", new ODataQueryContext(model, typeof(Customer)), queryOptionParser: null),
                "queryOptionParser");
        }

        [Theory]
        [InlineData("2")]
        [InlineData("100")]
        [InlineData("0")]
        public void CanConstructValidFilterQuery(string topValue)
        {
            // Arrange
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));

            // Act
            var top = new TopQueryOption(topValue, context);

            // Assert
            Assert.Same(context, top.Context);
            Assert.Equal(topValue, top.RawValue);
        }

        //[Theory]
        //[InlineData("NotANumber")]
        //[InlineData("''")]
        //[InlineData(" ")]
        //[InlineData("-1")]
        //[InlineData("6926906880")]
        //public void ApplyInvalidTopQueryThrows(string topValue)
        //{
        //    var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
        //    var context = new ODataQueryContext(model, typeof(Customer));
        //    var top = new TopQueryOption(topValue, context);

        //    ExceptionAssert.Throws<ODataException>(() =>
        //        top.ApplyTo(ODataQueryOptionTest.Customers, new ODataQuerySettings()));
        //}

        [Theory]
        [InlineData("0", 0)]
        [InlineData("100", 100)]
        public void Value_Returns_ParsedTopValue(string topValue, int expectedValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var top = new TopQueryOption(topValue, context);

            Assert.Equal(expectedValue, top.Value);
        }

        [Theory]
        [InlineData("NotANumber")]
        [InlineData("''")]
        [InlineData(" ")]
        [InlineData("-1")]
        public void Value_ThrowsODataException_ForInvalidValues(string skipValue)
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            var top = new TopQueryOption(skipValue, context);

            ExceptionAssert.Throws<ODataException>(() => top.Value);
        }

        [Fact]
        public void CanApplyTop()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var topOption = new TopQueryOption("1", new ODataQueryContext(model, typeof(Customer)));

            var customers = (new List<Customer>{
                new Customer { Id = 1, Name = "Andy" },
                new Customer { Id = 2, Name = "Aaron" },
                new Customer { Id = 3, Name = "Alex" }
            }).AsQueryable();

            var results = topOption.ApplyTo(customers, new ODataQuerySettings()).ToArray();
            Assert.Single(results);
            Assert.Equal(1, results[0].Id);
        }

        [Fact]
        public void CanApplySkipTopOrderby()
        {
            var model = new ODataModelBuilder().Add_Customer_EntityType().Add_Customers_EntitySet().GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
            var orderbyOption = new OrderByQueryOption("Name", context);
            var skipOption = new SkipQueryOption("2", context);
            var topOption = new TopQueryOption("2", context);

            var customers = (new List<Customer>{
                new Customer { Id = 1, Name = "Andy" },
                new Customer { Id = 2, Name = "Aaron" },
                new Customer { Id = 3, Name = "Alex" },
                new Customer { Id = 4, Name = "Ace" },
                new Customer { Id = 5, Name = "Abner" }
            }).AsQueryable();

            IQueryable queryable = orderbyOption.ApplyTo(customers);
            queryable = skipOption.ApplyTo(queryable, new ODataQuerySettings());
            queryable = topOption.ApplyTo(queryable, new ODataQuerySettings());
            var results = ((IQueryable<Customer>)queryable).ToArray();
            Assert.Equal(2, results.Length);
            Assert.Equal(4, results[0].Id);
            Assert.Equal(3, results[1].Id);
        }

        //[Fact]
        //public void CanTurnOffValidationForTop()
        //{
        //    // Arrange
        //    ODataValidationSettings settings = new ODataValidationSettings()
        //    {
        //        MaxTop = 10
        //    };
        //    TopQueryOption option = new TopQueryOption("11", ValidationTestHelper.CreateCustomerContext());

        //    // Act and Assert
        //    ExceptionAssert.Throws<ODataException>(() =>
        //        option.Validate(settings),
        //        "The limit of '10' for Top query has been exceeded. The value from the incoming request is '11'.");
        //    option.Validator = null;
        //    ExceptionAssert.DoesNotThrow(() => option.Validate(settings));
        //}

        [Fact]
        public void Property_Value_WorksWithUnTypedContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            TopQueryOption top = new TopQueryOption("42", context);

            // Act & Assert
            Assert.Equal(42, top.Value);
        }

        [Fact]
        public void ApplyTo_WithUnTypedContext_Throws_InvalidOperation()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            TopQueryOption top = new TopQueryOption("42", context);
            IQueryable queryable = new Mock<IQueryable>().Object;

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => top.ApplyTo(queryable, new ODataQuerySettings()),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }
    }
}
