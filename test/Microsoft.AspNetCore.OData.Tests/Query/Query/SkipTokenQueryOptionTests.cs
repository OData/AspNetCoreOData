//-----------------------------------------------------------------------------
// <copyright file="SkipTokenQueryOptionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class SkipTokenQueryOptionTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CtorSkipTokenQueryOption_ThrowsArgumentNullOrEmpty_RawValue(string rawValue)
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => new SkipTokenQueryOption(rawValue, null), "rawValue");
        }

        [Fact]
        public void CtorSkipTokenQueryOption_ThrowsArgumentNull_Context()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new SkipTokenQueryOption("abc", null), "context");
        }

        [Fact]
        public void CtorSkipTokenQueryOption_SetsProperties()
        {
            // Arrange
            Mock<SkipTokenHandler> handler = new Mock<SkipTokenHandler>();
            SkipTokenQueryValidator validator = new SkipTokenQueryValidator();
            IServiceProvider sp = new ServiceCollection()
                .AddSingleton<SkipTokenHandler>(handler.Object)
                .AddSingleton<ISkipTokenQueryValidator>(validator)
                .BuildServiceProvider();

            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int))
            {
                RequestContainer = sp
            };

            // Act
            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("abc", context);

            // Assert
            Assert.Equal("abc", skipTokenQuery.RawValue);
            Assert.Same(context, skipTokenQuery.Context);
            Assert.Same(handler.Object, skipTokenQuery.Handler);
            Assert.Same(validator, skipTokenQuery.Validator);
        }

        [Fact]
        public void ApplyToOfTSkipTokenQueryOption_Calls_ApplyToOfTOnSkipTokenHandler()
        {
            // Arrange
            Mock<SkipTokenHandler> handler = new Mock<SkipTokenHandler>();
            ODataQuerySettings settings = new ODataQuerySettings();
            SkipTokenQueryValidator validator = new SkipTokenQueryValidator();
            IServiceProvider sp = new ServiceCollection()
                .AddSingleton<SkipTokenHandler>(handler.Object)
                .AddSingleton<ISkipTokenQueryValidator>(validator)
                .BuildServiceProvider();

            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int))
            {
                RequestContainer = sp
            };

            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("abc", context);

            IQueryable<SkipTokenCustomer> query = Array.Empty<SkipTokenCustomer>().AsQueryable();
            handler.Setup(h => h.ApplyTo<SkipTokenCustomer>(query, skipTokenQuery, settings, null)).Returns(query).Verifiable();

            // Act
            skipTokenQuery.ApplyTo<SkipTokenCustomer>(query, settings, null);

            // Assert
            handler.Verify();
        }

        [Fact]
        public void ApplyToOfTSkipTokenQueryOption_Applies_ToQuaryable()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            ODataQuerySettings settings = new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.False
            };
            ODataQueryContext context = new ODataQueryContext(model, typeof(SkipTokenCustomer));
            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("Id-2", context);

            IQueryable<SkipTokenCustomer> customers = new List<SkipTokenCustomer>
            {
                new SkipTokenCustomer { Id = 2, Name = "Aaron" },
                new SkipTokenCustomer { Id = 1, Name = "Andy" },
                new SkipTokenCustomer { Id = 3, Name = "Alex" }
            }.AsQueryable();

            // Act
            SkipTokenCustomer[] results = skipTokenQuery.ApplyTo(customers, settings, null).ToArray();

            // Assert
            SkipTokenCustomer skipTokenCustomer = Assert.Single(results);
            Assert.Equal(3, skipTokenCustomer.Id);
            Assert.Equal("Alex", skipTokenCustomer.Name);
        }

        [Fact]
        public void ApplyToOfTSkipTokenQueryOption_Applies_ToQuaryable_WithOrderby()
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            ODataQuerySettings settings = new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.False
            };
            ODataQueryContext context = new ODataQueryContext(model, typeof(SkipTokenCustomer));

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/Customers/?$orderby=Name&$skiptoken=Name-'Alex',Id-3");

            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);
            SkipTokenQueryOption skipTokenQuery = queryOptions.SkipToken;

            IQueryable<SkipTokenCustomer> customers = new List<SkipTokenCustomer>
            {
                new SkipTokenCustomer { Id = 2, Name = "Caron" },
                new SkipTokenCustomer { Id = 1, Name = "Bndy" },
                new SkipTokenCustomer { Id = 3, Name = "Alex" },
                new SkipTokenCustomer { Id = 4, Name = "Aab" }
            }.AsQueryable();

            // Act
            SkipTokenCustomer[] results = skipTokenQuery.ApplyTo(customers, settings, queryOptions).ToArray();

            // Assert
            Assert.Equal(2, results.Length);
            Assert.Equal(2, results[0].Id);
            Assert.Equal(1, results[1].Id);
        }

        [Fact]
        public void ApplyToSkipTokenQueryOption_Calls_ApplyToOnSkipTokenHandler()
        {
            // Arrange
            Mock<SkipTokenHandler> handler = new Mock<SkipTokenHandler>();
            ODataQuerySettings settings = new ODataQuerySettings();
            SkipTokenQueryValidator validator = new SkipTokenQueryValidator();
            IServiceProvider sp = new ServiceCollection()
                .AddSingleton<SkipTokenHandler>(handler.Object)
                .AddSingleton<ISkipTokenQueryValidator>(validator)
                .BuildServiceProvider();

            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int))
            {
                RequestContainer = sp
            };

            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("abc", context);

            IQueryable queryable = new Mock<IQueryable>().Object;
            handler.Setup(h => h.ApplyTo(queryable, skipTokenQuery, settings, null)).Returns(queryable).Verifiable();

            // Act
            skipTokenQuery.ApplyTo(queryable, settings, null);

            // Assert
            handler.Verify();
        }

        [Fact]
        public void ValidateSkipTokenQueryOption_ThrowsArgumentNull_ValidationSettings()
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            SkipTokenQueryOption skipToken = new SkipTokenQueryOption("abc", context);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => skipToken.Validate(null), "validationSettings");
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SkipTokenCustomer>("Customers");
            return builder.GetEdmModel();
        }

        private class SkipTokenCustomer
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
