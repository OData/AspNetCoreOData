// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class ExpressionHelpersTests
    {
        private static IQueryable<ECustomer> _querable = new List<ECustomer>()
        {
            new ECustomer { Id = 1, Name = "Ady", Age = 19, Phones = new List<int> { 1, 2 } },
            new ECustomer { Id = 2, Name = "Peter", Age = 29, Phones = new List<int> { 4, 5 } },
            new ECustomer { Id = 3, Name = "Sam", Age = 8, Phones = new List<int> { 7, 8 } }
        }.AsQueryable();

        [Fact]
        public void Count_Returns_CorrectCountFunc()
        {
            // Arrange & Act
            Func<long> countFunc = ExpressionHelpers.Count(_querable, typeof(ECustomer));

            // Assert
            Assert.Equal(3, countFunc());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Skip_Returns_CorrectQueryable(bool parameterize)
        {
            // Arrange & Act
            IQueryable actual = ExpressionHelpers.Skip(_querable, 2, typeof(ECustomer), parameterize);

            // Assert
            ECustomer customer = Assert.Single(actual.Cast<ECustomer>());
            Assert.Equal(3, customer.Id);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Take_Returns_CorrectQueryable(bool parameterize)
        {
            // Arrange & Act
            IQueryable actual = ExpressionHelpers.Take(_querable, 1, typeof(ECustomer), parameterize);

            // Assert
            ECustomer customer = Assert.Single(actual.Cast<ECustomer>());
            Assert.Equal(1, customer.Id);
        }

        [Fact]
        public void GroupBy_Returns_CorrectQueryable()
        {
            // Arrange
            Expression<Func<ECustomer, string>> expression = x => x.Name;

            // Act
            IQueryable actual = ExpressionHelpers.Select(_querable, expression, typeof(ECustomer));

            // Assert
            Assert.Equal(new string[] { "Ady", "Peter", "Sam" }, actual.Cast<string>());
        }

        [Fact]
        public void Select_Returns_CorrectQueryable()
        {
            // Arrange
            Expression<Func<ECustomer, string>> expression = x => x.Name;

            // Act
            IQueryable actual = ExpressionHelpers.Select(_querable, expression, typeof(ECustomer));

            // Assert
            Assert.Equal(new string[] { "Ady", "Peter", "Sam" }, actual.Cast<string>());
        }

        [Fact]
        public void Where_Returns_CorrectQueryable()
        {
            // Arrange
            Expression<Func<ECustomer, bool>> expression = x => x.Name == "Peter";

            // Act
            IQueryable actual = ExpressionHelpers.Where(_querable, expression, typeof(ECustomer));

            // Assert
            ECustomer customer = Assert.Single(actual.Cast<ECustomer>());
            Assert.Equal(2, customer.Id);
        }

        [Fact]
        public void Default_Returns_CorrectExpression()
        {
            // Arrange & Act & Assert
            Expression expression = ExpressionHelpers.Default(typeof(int));
            Assert.Equal(ExpressionType.Constant, expression.NodeType);
            ConstantExpression constantExpression = Assert.IsType<ConstantExpression>(expression);
            Assert.Equal(0, constantExpression.Value);
            Assert.Equal(typeof(int), constantExpression.Type);

            // Arrange & Act & Assert
            expression = ExpressionHelpers.Default(typeof(ExpressionHelpersTests));
            Assert.Equal(ExpressionType.Constant, expression.NodeType);
            constantExpression = Assert.IsAssignableFrom<ConstantExpression>(expression);
            Assert.Null(constantExpression.Value);
            Assert.Equal(typeof(ExpressionHelpersTests), constantExpression.Type);
        }

#if false
        [Fact]
        public void SelectMany_Returns_CorrectQueryable()
        {
            // Arrange
            Expression<Func<ECustomer, IEnumerable<int>>> expression = x => x.Phones;

            // Act
            IQueryable actual = ExpressionHelpers.SelectMany(_querable, expression, typeof(ECustomer));

            // Assert
            Assert.Equal(new[] { 1, 2, 4, 5, 7, 8 }, actual.Cast<int>());
        }
#endif

        private class ECustomer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int Age { get; set; }

            public List<int> Phones { get; set; }

            public List<string> Emails { get; set; }
        }
    }
}
