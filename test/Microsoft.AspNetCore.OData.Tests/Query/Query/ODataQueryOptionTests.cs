//-----------------------------------------------------------------------------
// <copyright file="ODataQueryOptionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class ODataQueryOptionTests
    {
        internal static IQueryable Customers = new List<Customer>().AsQueryable();

        [Fact]
        public void CtorODataQueryOption_ThrowsArgumentNull_Context()
        {
            // Arrange
            Mock<HttpRequest> request = new Mock<HttpRequest>();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataQueryOptions(null, request.Object), "context");
        }

        [Fact]
        public void CtorODataQueryOption_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataQueryOptions(new ODataQueryContext(EdmCoreModel.Instance, typeof(bool)), null), "request");
        }

        [Fact]
        public void ValidateODataQueryOption_ThrowsArgumentNull_ValidationSettingst()
        {
            // Arrange & Act & Assert
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://any");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            ExceptionAssert.ThrowsArgumentNull(() => queryOptions.Validate(null), "validationSettings");
        }

        [Theory]
        [InlineData("$filter")]
        [InlineData("$count")]
        [InlineData("$orderby")]
        [InlineData("$skip")]
        [InlineData("$top")]
        public void CtorODataQueryOption_ThrowsIfEmptyQueryOptionValue(string queryName)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();
            IEdmModel model = builder.GetEdmModel();
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers/?" + queryName + "=", setupAction: null);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request),
                "The value for OData query '" + queryName + "' cannot be empty.");
        }

        [Fact]
        public void CtorODataQueryOption_CanExtractQueryOptionsCorrectly()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();
            IEdmModel model = builder.GetEdmModel();

            HttpRequest request = RequestFactory.Create(
                HttpMethods.Get,
                "http://server/service/Customers/?$filter=Filter&$select=Select&$orderby=OrderBy&$expand=Expand&$top=10&$skip=20&$count=true&$skiptoken=SkipToken&$deltatoken=DeltaToken");

            // Act
            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Assert
            Assert.Equal("Filter", queryOptions.RawValues.Filter);
            Assert.NotNull(queryOptions.Filter);
            Assert.Equal("OrderBy", queryOptions.RawValues.OrderBy);
            Assert.NotNull(queryOptions.OrderBy);
            Assert.Equal("10", queryOptions.RawValues.Top);
            Assert.NotNull(queryOptions.Top);
            Assert.Equal("20", queryOptions.RawValues.Skip);
            Assert.NotNull(queryOptions.Skip);
            Assert.Equal("Expand", queryOptions.RawValues.Expand);
            Assert.Equal("Select", queryOptions.RawValues.Select);
            Assert.NotNull(queryOptions.SelectExpand);
            Assert.Equal("true", queryOptions.RawValues.Count);
            Assert.Equal("SkipToken", queryOptions.RawValues.SkipToken);
            Assert.Equal("DeltaToken", queryOptions.RawValues.DeltaToken);
        }

        [Theory]
        [InlineData(" $filter=Filter& $select=Select& $orderby=OrderBy& $expand=Expand& $top=10& $skip=20& $count=true& $skiptoken=SkipToken& $deltatoken=DeltaToken")]
        [InlineData("%20$filter=Filter&%20$select=Select&%20$orderby=OrderBy&%20$expand=Expand&%20$top=10&%20$skip=20&%20$count=true&%20$skiptoken=SkipToken&%20$deltatoken=DeltaToken")]
        [InlineData("$filter =Filter&$select =Select&$orderby =OrderBy&$expand =Expand&$top =10&$skip =20&$count =true&$skiptoken =SkipToken&$deltatoken =DeltaToken")]
        [InlineData("$filter%20=Filter&$select%20=Select&$orderby%20=OrderBy&$expand%20=Expand&$top%20=10&$skip%20=20&$count%20=true&$skiptoken%20=SkipToken&$deltatoken%20=DeltaToken")]
        [InlineData(" $filter =Filter& $select =Select& $orderby =OrderBy& $expand =Expand& $top =10& $skip =20& $count =true& $skiptoken =SkipToken& $deltatoken =DeltaToken")]
        public void CtorODataQueryOption_CanExtractQueryOptionsWithExtraSpacesCorrectly(string clause)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();
            IEdmModel model = builder.GetEdmModel();

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers/?"+ clause);

            // Act
            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Assert
            Assert.Equal("Filter", queryOptions.RawValues.Filter);
            Assert.NotNull(queryOptions.Filter);
            Assert.Equal("OrderBy", queryOptions.RawValues.OrderBy);
            Assert.NotNull(queryOptions.OrderBy);
            Assert.Equal("10", queryOptions.RawValues.Top);
            Assert.NotNull(queryOptions.Top);
            Assert.Equal("20", queryOptions.RawValues.Skip);
            Assert.NotNull(queryOptions.Skip);
            Assert.Equal("Expand", queryOptions.RawValues.Expand);
            Assert.Equal("Select", queryOptions.RawValues.Select);
            Assert.NotNull(queryOptions.SelectExpand);
            Assert.Equal("true", queryOptions.RawValues.Count);
            Assert.Equal("SkipToken", queryOptions.RawValues.SkipToken);
            Assert.Equal("DeltaToken", queryOptions.RawValues.DeltaToken);
        }

        [Fact]
        public void ApplyToODataQueryOption_Throws_With_Null_Queryable()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://any");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null), "query");
        }

        [Fact]
        public void ApplyToODataQueryOption_With_QuerySettings_Throws_With_Null_Queryable()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://any");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => queryOptions.ApplyTo(null, new ODataQuerySettings()), "query");
        }

        [Fact]
        public void ApplyToODataQueryOption_Throws_With_Null_QuerySettings()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://any");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => queryOptions.ApplyTo(new Customer[0].AsQueryable(), null), "querySettings");
        }

        // Used to test modifications to $orderby when $skip or $top are present
        // and the entity type has 2 keys -- CustomerId and Name.
        // Tuple is: query expression, ensureStableOrdering, expected expression
        public static TheoryDataSet<string, bool, string> SkipTopOrderByUsingKeysTestData
        {
            get
            {
                return new TheoryDataSet<string, bool, string>
                {
                    // First key present with $skip, adds 2nd key
                    { "$orderby=CustomerId&$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1)" },

                    // First key present with $top, adds 2nd key
                    { "$orderby=CustomerId&$top=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Take(1)" },

                    // First key present with $skip and $top, adds 2nd key
                    { "$orderby=CustomerId&$skip=1&$top=2", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1).Take(2)" },

                    // First key present, no $skip or $top, no modification
                    { "$orderby=CustomerId", false, "OrderBy($it => $it.CustomerId)" },

                    // First key present, 'ensureStableOrdering' is false, no modification
                    { "$orderby=CustomerId&$skip=1", false, "OrderBy($it => $it.CustomerId).Skip(1)" },

                    // Second key present, adds 1st key after 2nd
                    { "$orderby=Name&$skip=1", true, "OrderBy($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)" },

                    // Second key plus 'asc' suffix, adds 1st key and preserves suffix
                    { "$orderby=Name asc&$skip=1", true, "OrderBy($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)" },

                    // Second key plus 'desc' suffix, adds 1st key and preserves suffix
                    { "$orderby=Name desc&$skip=1", true, "OrderByDescending($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)" },

                    // All keys present, no modification
                    { "$orderby=CustomerId,Name&$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1)" },

                    // All keys present but in reverse order, no modification
                    { "$orderby=Name,CustomerId&$skip=1", true,  "OrderBy($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)" },

                    // First key present but with extraneous whitespace, adds 2nd key
                    { "$orderby= CustomerId &$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1)" },

                    // All keys present with extraneous whitespace, no modification
                    { "$orderby= \t CustomerId \t , Name \t desc \t &$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenByDescending($it => $it.Name).Skip(1)" },

                    // Ordering on non-key property, adds all keys
                    { "$orderby=Website&$skip=1", true,  "OrderBy($it => $it.Website).ThenBy($it => $it.CustomerId).ThenBy($it => $it.Name).Skip(1)" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(SkipTopOrderByUsingKeysTestData))]
        public void ApplyToODataQueryOption_Adds_Missing_Keys_To_OrderBy(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => new { c.CustomerId, c.Name });

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?" + oDataQuery);

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = ensureStableOrdering,
            };

            // Act
            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        // Used to test modifications to $orderby when $skip or $top are present
        // and the entity type has a no key properties.
        // Tuple is: query expression, ensureStableOrdering, expected expression
        public static TheoryDataSet<string, bool, string> SkipTopOrderByWithNoKeysTestData
        {
            get
            {
                return new TheoryDataSet<string, bool, string>
                {
                    // Single property present with $skip, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$skip=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.ShareSymbol).ThenBy($it => $it.Website).Skip(1)" },

                    // Single property present with $top, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$top=1", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.ShareSymbol).ThenBy($it => $it.Website).Take(1)" },

                    // Single property present with $skip and $top, adds all remaining in alphabetic order
                    { "$orderby=CustomerId&$skip=1&$top=2", true,  "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.ShareSymbol).ThenBy($it => $it.Website).Skip(1).Take(2)" },

                    // Single property present, no $skip or $top, no modification
                    { "$orderby=SharePrice", false,  "OrderBy($it => $it.SharePrice)" },

                    // Single property present, ensureStableOrdering is false, no modification
                    { "$orderby=SharePrice&$skip=1", false,  "OrderBy($it => $it.SharePrice).Skip(1)" },

                    // All properties present, non-alphabetic order, no modification
                    { "$orderby=Name,SharePrice,CustomerId,Website,ShareSymbol&$skip=1", true,  "OrderBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.CustomerId).ThenBy($it => $it.Website).ThenBy($it => $it.ShareSymbol).Skip(1)" },

                    // All properties present, extraneous whitespace, non-alphabetic order, no modification
                    { "$orderby= \t Name \t , \t SharePrice \t , \t CustomerId \t , \t Website \t , \t ShareSymbol \t &$skip=1", true,  "OrderBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.CustomerId).ThenBy($it => $it.Website).ThenBy($it => $it.ShareSymbol).Skip(1)" },

                };
            }
        }

        [Theory]
        [MemberData(nameof(SkipTopOrderByWithNoKeysTestData))]
        public void ApplyToODataQueryOption_Adds_Missing_NonKey_Properties_To_OrderBy(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            // Arrange
            IEdmModel model = GetEdmModelWithoutKey();

            var request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?" + oDataQuery);

            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = ensureStableOrdering
            };

            // Act
            IQueryable finalQuery = queryOptions.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Fact]
        public void ApplyToODataQueryOption_Does_Not_Replace_Original_OrderBy_With_Missing_Keys()
        {
            // Arrange
            IEdmModel model = GetEdmModelWithoutKey();

            var request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?$orderby=Name");

            // Act
            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            OrderByQueryOption originalOption = queryOptions.OrderBy;
            ODataQuerySettings querySettings = new ODataQuerySettings();

            queryOptions.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            Assert.Same(originalOption, queryOptions.OrderBy);
        }

        [Fact]
        public void ApplyToODataQueryOption_SetsRequestSelectExpandClause_IfSelectExpandIsNotNull()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?$select=Name");
            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Act
            queryOptions.ApplyTo(Enumerable.Empty<Customer>().AsQueryable());

            // Assert
            Assert.NotNull(request.ODataFeature().SelectExpandClause);
        }

        [Fact]
        [Trait("ODataQueryOption", "Can bind a typed ODataQueryOption to the request uri without any query")]
        public void CtorODataQueryOption_ContextPropertyGetter()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers");

            // Act
            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Assert
            Type entityType = queryOptions.Context.ElementClrType;
            Assert.NotNull(entityType);
            Assert.Equal("Microsoft.AspNetCore.OData.Tests.Query.Customer", entityType.Namespace + "." + entityType.Name);
        }

        public static TheoryDataSet<string, string> QueryTestData
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                     { "$filter", null },
                     { "$filter", "''" },
                     { "$filter", "" },
                     { "$filter", " " },
                     { "$filter", "Name eq 'MSFT'" },

                     { "$orderby", null },
                     { "$orderby", "''" },
                     { "$orderby", "" },
                     { "$orderby", " " },
                     { "$orderby", "Name" },

                     { "$top", null },
                     { "$top", "''" },
                     { "$top", "" },
                     { "$top", " " },
                     { "$top", "12" },

                     { "$skip", null },
                     { "$skip", "''" },
                     { "$skip", "" },
                     { "$skip", " " },
                     { "$skip", "12" },

                     { "$apply", null },
                     { "$apply", "" },
                     { "$apply", " " },
                     { "$apply", "aggregate(SharePrice mul CustomerId with sum as Name)" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(QueryTestData))]
        public void CtorODataQueryOptions_QueryTest(string queryName, string queryValue)
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);

            string uri;
            if (queryValue == null)
            {
                // same as not passing the query - - this would work
                uri = string.Format("http://server/service/Customers?{0}=", queryName);
            }
            else
            {
                // if queryValue is invalid, such as whitespace or not a number for top and skip
                uri = string.Format("http://server/service/Customers?{0}={1}", queryName, queryValue);
            }

            var request = RequestFactory.Create(HttpMethods.Get, uri);

            // Act && Assert
            if (string.IsNullOrWhiteSpace(queryValue))
            {
                ExceptionAssert.Throws<ODataException>(() =>
                    new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request));
            }
            else
            {
                var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

                if (queryName == "$filter")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.Filter);
                }
                else if (queryName == "$orderby")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.OrderBy);
                }
                else if (queryName == "$top")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.Top);
                }
                else if (queryName == "$skip")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.Skip);
                }
                else if (queryName == "$apply")
                {
                    Assert.Equal(queryValue, queryOptions.RawValues.Apply);
                }
            }
        }

        [Fact]
        public void CtorODataQueryOptions_MissingQueryReturnsOriginalList()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);

            // the query is completely missing - this would work
            string uri = "http://server/service/Customers";
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, uri);

            // Act
            ODataQueryOptions queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Assert: everything is null
            Assert.Null(queryOptions.RawValues.OrderBy);
            Assert.Null(queryOptions.RawValues.Filter);
            Assert.Null(queryOptions.RawValues.Skip);
            Assert.Null(queryOptions.RawValues.Top);
        }

        [Fact]
        public void ApplyToODataQueryOptions_OrderbyWithUnknownPropertyThrows()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?$orderby=UnknownProperty");

            // Act & Assert
            ODataQueryOptions option = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ExceptionAssert.Throws<ODataException>(() => option.ApplyTo(new List<Customer>().AsQueryable()));
        }

        [Fact]
        public void ApplyToODataQueryOptions_CannotConvertBadTopQueryThrows()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?$top=NotANumber");

            // Act & Assert
            ODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ExceptionAssert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Invalid value 'NotANumber' for $top query option found. " +
                 "The $top query option requires a non-negative integer value.");

            // Act & Assert
            request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?$top=''");

            options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ExceptionAssert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Invalid value '''' for $top query option found. " +
                 "The $top query option requires a non-negative integer value.");
        }

        [Fact]
        public void ApplyToODataQueryOptions_CannotConvertBadSkipQueryThrows()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);

            // Act & Assert
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?$skip=NotANumber");
            ODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ExceptionAssert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Invalid value 'NotANumber' for $skip query option found. " +
                 "The $skip query option requires a non-negative integer value.");

            // Act & Assert
            request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?$skip=''");
            options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ExceptionAssert.Throws<ODataException>(() =>
                 options.ApplyTo(Customers),
                 "Invalid value '''' for $skip query option found. " +
                 "The $skip query option requires a non-negative integer value.");
        }

        [Theory]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy($it => $it.A).ThenBy($it => $it.B).Skip(1)")]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy($it => $it.ID).Skip(1)")]
        [InlineData("$skip=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy($it => $it.ID1).ThenBy($it => $it.ID2).Skip(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy($it => $it.A).ThenBy($it => $it.B).Take(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy($it => $it.ID).Take(1)")]
        [InlineData("$top=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy($it => $it.ID1).ThenBy($it => $it.ID2).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_ComplexModel), "OrderBy($it => $it.A).ThenBy($it => $it.B).Skip(1).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_EntityModel), "OrderBy($it => $it.ID).Skip(1).Take(1)")]
        [InlineData("$skip=1&$top=1", typeof(ODataQueryOptionTest_EntityModelMultipleKeys), "OrderBy($it => $it.ID1).ThenBy($it => $it.ID2).Skip(1).Take(1)")]
        public void ApplyToODataQueryOptions_Picks_DefaultOrder(string oDataQuery, Type elementType, string expectedExpression)
        {
            // Arrange
            IQueryable query = Array.CreateInstance(elementType, 0).AsQueryable();
            ODataConventionModelBuilder modelBuilder = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>();
            modelBuilder.AddEntitySet("entityset", modelBuilder.AddEntityType(elementType));
            IEdmModel model = modelBuilder.GetEdmModel();

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/entityset?" + oDataQuery);

            // Act
            ODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, elementType), request);
            IQueryable finalQuery = options.ApplyTo(query);

            // Assert
            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("OrderBy"));

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [InlineData("$filter=1 eq 1")]
        [InlineData("")]
        public void ApplyToODataQueryOptions_DoesnotPickDefaultOrder_IfSkipAndTopAreNotPresent(string oDataQuery)
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);

            // Act
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?" + oDataQuery);

            ODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            IQueryable finalQuery = options.ApplyTo(Customers);

            // Assert
            string queryExpression = finalQuery.Expression.ToString();
            Assert.DoesNotContain("OrderBy", queryExpression);
        }

        [Theory]
        [InlineData("$orderby=Name", "OrderBy($it => $it.Name)")]
        [InlineData("$orderby=Website", "OrderBy($it => $it.Website)")]
        [InlineData("$orderby=Name&$skip=1", "OrderBy($it => $it.Name).ThenBy($it => $it.CustomerId).Skip(1)")]
        [InlineData("$orderby=Website&$top=1&$skip=1", "OrderBy($it => $it.Website).ThenBy($it => $it.CustomerId).Skip(1).Take(1)")]
        public void ApplyToODataQueryOptions__DoesnotPickDefaultOrder_IfOrderByIsPresent(string oDataQuery, string expectedExpression)
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?" + oDataQuery);

            // Act
            ODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            IQueryable finalQuery = options.ApplyTo(Customers);

            // Assert
            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("OrderBy"));

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [InlineData("$skip=1", true, "OrderBy($it => $it.CustomerId).Skip(1)")]
        [InlineData("$skip=1", false, "Skip(1)")]
        [InlineData("$filter=1 eq 1", true, "Where($it => (1 == 1))")]
        [InlineData("$filter=1 eq 1", false, "Where($it => (1 == 1))")]
        public void ApplyToODataQueryOptions_Builds_Default_OrderBy_With_Keys(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?" + oDataQuery);

            ODataQueryOptions options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = ensureStableOrdering
            };

            // Act
            IQueryable finalQuery = options.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(queryExpression, expectedExpression);
        }

        [Theory]
        [InlineData("$skip=1", true, "OrderBy($it => $it.CustomerId).ThenBy($it => $it.Name).ThenBy($it => $it.SharePrice).ThenBy($it => $it.ShareSymbol).ThenBy($it => $it.Website).Skip(1)")]
        [InlineData("$skip=1", false, "Skip(1)")]
        [InlineData("$filter=1 eq 1", true, "Where($it => (1 == 1))")]
        [InlineData("$filter=1 eq 1", false, "Where($it => (1 == 1))")]
        public void ApplyToODataQueryOptions_Builds_Default_OrderBy_No_Keys(string oDataQuery, bool ensureStableOrdering, string expectedExpression)
        {
            // Arrange
            IEdmModel model = GetEdmModelWithoutKey();

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?" + oDataQuery);

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = ensureStableOrdering
            };

            // Act
            IQueryable finalQuery = options.ApplyTo(new Customer[0].AsQueryable(), querySettings);

            // Assert
            string queryExpression = ExpressionStringBuilder.ToString(finalQuery.Expression);
            queryExpression = queryExpression.Substring(queryExpression.IndexOf("]") + 2);

            Assert.Equal(expectedExpression, queryExpression);
        }

        [Fact]
        public void Validate_ThrowsValidationErrors_ForOrderBy()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/service/Customers?$orderby=CustomerId,Name");

            var options = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);
            ODataValidationSettings validationSettings = new ODataValidationSettings { MaxOrderByNodeCount = 1 };

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => options.Validate(validationSettings),
                "The number of clauses in $orderby query option exceeded the maximum number allowed. The maximum number of $orderby clauses allowed is 1.");
        }

        [Theory]
        [InlineData("$orderby")]
        [InlineData("$filter")]
        [InlineData("$top")]
        [InlineData("$skip")]
        [InlineData("$count")]
        [InlineData("$expand")]
        [InlineData("$select")]
        [InlineData("$format")]
        [InlineData("$skiptoken")]
        [InlineData("$deltatoken")]
        public void IsSystemQueryOption_Returns_True_For_All_Supported_Query_Names(string queryName)
        {
            // Arrange & Act & Assert
            Assert.True(ODataQueryOptions.IsSystemQueryOption(queryName));

            string newQueryName = queryName.Substring(1); // remove "$"
            Assert.False(ODataQueryOptions.IsSystemQueryOption(newQueryName, false));
            Assert.True(ODataQueryOptions.IsSystemQueryOption(newQueryName, true));
        }

        [Fact]
        public void IsSystemQueryOption_Returns_False_For_Unrecognized_Query_Name()
        {
            // Arrange & Act & Assert
            Assert.False(ODataQueryOptions.IsSystemQueryOption("$invalidqueryname"));
        }

        [Fact]
        public void IsSystemQueryOption_ThrowsArgumentNull_QueryOptionName()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => ODataQueryOptions.IsSystemQueryOption(null), "queryOptionName");
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => ODataQueryOptions.IsSystemQueryOption(string.Empty), "queryOptionName");
        }

        [Fact]
        public void GenerateStableOrder_Works_WithGroupbyApplyClause()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost/Customers?$apply=groupby((CustomerId, Name))&$orderby=Name");

            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));
            ODataQueryOptions option = new ODataQueryOptions(context, request);

            // Act
            OrderByQueryOption orderByQuery = option.GenerateStableOrder();

            // Assert
            Assert.NotNull(orderByQuery);
            Assert.Equal(2, orderByQuery.OrderByNodes.Count);
            Assert.Collection(orderByQuery.OrderByNodes,
                e =>
                {
                    OrderByPropertyNode node = Assert.IsType<OrderByPropertyNode>(e);
                    Assert.Equal("Name", node.Property.Name);
                },
                e =>
                {
                    OrderByPropertyNode node = Assert.IsType<OrderByPropertyNode>(e);
                    Assert.Equal("CustomerId", node.Property.Name);
                });
        }

        [Fact]
        public void GenerateStableOrder_Works_WithAggregateApplyClause()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost/Customers?$apply=aggregate(CustomerId with sum as Total)&$orderby=Total");

            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));
            ODataQueryOptions option = new ODataQueryOptions(context, request);

            // Act
            OrderByQueryOption orderByQuery = option.GenerateStableOrder();

            // Assert
            Assert.NotNull(orderByQuery);
            OrderByNode orderbyNode = Assert.Single(orderByQuery.OrderByNodes);
            OrderByOpenPropertyNode node = Assert.IsType<OrderByOpenPropertyNode>(orderbyNode);
            Assert.Equal("Total", node.PropertyName);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(4, false)]
        [InlineData(8, false)]
        public void LimitResults_LimitsResults(int limit, bool resultsLimitedExpected)
        {
            // Arrange
            IQueryable queryable = new List<Customer>() {
                new Customer() { CustomerId = 0 },
                new Customer() { CustomerId = 1 },
                new Customer() { CustomerId = 2 },
                new Customer() { CustomerId = 3 }
            }.AsQueryable();
            IEdmModel model = GetEdmModel(c => c.CustomerId);
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));

            // Act
            bool resultsLimited;
            IQueryable<Customer> result = ODataQueryOptions.LimitResults(queryable, limit, false, out resultsLimited) as IQueryable<Customer>;

            // Assert
            Assert.Equal(Math.Min(limit, 4), result.Count());
            Assert.Equal(resultsLimitedExpected, resultsLimited);
        }

        [Fact]
        public void CanTurnOffAllValidation()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost/?$filter=Name eq 'abc'");

            IEdmModel model = GetEdmModel(c => c.CustomerId);
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));
            ODataQueryOptions option = new ODataQueryOptions(context, request);
            ODataValidationSettings settings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.OrderBy
            };

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => option.Validate(settings),
                "Query option 'Filter' is not allowed. To allow it, set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.");

            option.Validator = null;
            ExceptionAssert.DoesNotThrow(() => option.Validate(settings));
        }

        public static TheoryDataSet<IQueryable, string, object> Querying_Primitive_Collections_Data
        {
            get
            {
                IQueryable<int> e = Enumerable.Range(1, 9).AsQueryable();
                return new TheoryDataSet<IQueryable, string, object>
                {
                    { e.Select(i => (short)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (short)6 },
                    { e.Select(i => i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", 6 },
                    { e.Select(i => (long)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (long)6 },
                    { e.Select(i => (ushort)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (ushort)6 },
                    { e.Select(i => (uint)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (uint)6 },
                    { e.Select(i => (ulong)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (ulong)6 },
                    { e.Select(i => (float)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (float)6 },
                    { e.Select(i => (double)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (double)6 },
                    { e.Select(i => (decimal)i), "$filter=$it ge 5&$orderby=$it desc&$skip=3&$top=1", (decimal)6 },
                    { e.Select(i => new DateTimeOffset(new DateTime(i, 1, 1), TimeSpan.Zero)), "$filter=year($it) ge 5&$orderby=$it desc&$skip=3&$top=1", new DateTimeOffset(new DateTime(year: 6, month: 1, day: 1), TimeSpan.Zero) },
                    { e.Select(i => i.ToString()), "$filter=$it ge '5'&$orderby=$it desc&$skip=3&$top=1", "6" },

                    { e.Select(i => (i % 2 != 0 ? null : (short?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (short?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (int?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (int?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (long?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (long?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (ushort?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (ushort?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (uint?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (uint?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (ulong?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (ulong?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (float?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (float?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (double?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (double?)6 },
                    { e.Select(i => (i % 2 != 0 ? null : (decimal?)i)), "$filter=$it ge 5&$orderby=$it desc&$skip=1&$top=1", (decimal?)6 },
                };
            }
        }

        [Theory]
        [MemberData(nameof(Querying_Primitive_Collections_Data))]
        public void Querying_Primitive_Collections(IQueryable queryable, string query, object result)
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost/?" + query);
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, queryable.ElementType);
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            // Act
            queryable = options.ApplyTo(queryable);

            // Assert
            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(result, enumerator.Current);
        }

        private enum SimpleEnum
        {
            First,
            Second,
            Third,
            Fourth
        }

        public static TheoryDataSet<IQueryable, string, object> Querying_Enum_Collections_Data
        {
            get
            {
                IQueryable<int> e = Enumerable.Range(1, 9).AsQueryable();
                return new TheoryDataSet<IQueryable, string, object>
                {
                    { e.Select(i => (SimpleEnum)(i%3)), "$filter=$it eq Microsoft.AspNetCore.OData.Tests.Query.SimpleEnum'First'&$orderby=$it desc&$skip=1&$top=1", SimpleEnum.First },
                    { e.Select(i => (SimpleEnum?)null), "$filter=$it eq null&$orderby=$it desc&$skip=1&$top=1", null },
                };
            }
        }

        [Theory]
        [MemberData(nameof(Querying_Enum_Collections_Data))]
        public void Querying_Enum_Collections(IQueryable queryable, string query, object result)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EnumTypeConfiguration<SimpleEnum> simpleEnum = builder.EnumType<SimpleEnum>();
            simpleEnum.Member(SimpleEnum.First);
            simpleEnum.Member(SimpleEnum.Second);
            simpleEnum.Member(SimpleEnum.Third);
            IEdmModel model = builder.GetEdmModel();

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost/?" + query);
            ODataQueryContext context = new ODataQueryContext(model, queryable.ElementType);
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            // Act
            queryable = options.ApplyTo(queryable);

            // Assert
            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(result, enumerator.Current);
        }

        [Fact]
        public void ODataQueryOptions_IgnoresUnknownOperatorStartingWithDollar()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost/?$filter=$it eq 6&$unknown=value");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            // Act
            var queryable = options.ApplyTo(Enumerable.Range(0, 10).AsQueryable());

            // Assert
            IEnumerator enumerator = queryable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(6, enumerator.Current);
        }

        [Fact]
        public void ApplyTo_Entity_ThrowsArgumentNull_Entity()
        {
            // Arrange & Act
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://any");

            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => queryOptions.ApplyTo(entity: null, querySettings: new ODataQuerySettings()),
                "entity");
        }

        [Fact]
        public void ApplyTo_IgnoresCount_IfRequestAlreadyHasCount()
        {
            // Arrange
            long count = 42;
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost/?$count=true");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions options = new ODataQueryOptions(context, request);
            request.ODataFeature().TotalCount = count;

            // Act
            options.ApplyTo(Enumerable.Empty<int>().AsQueryable());

            // Assert
            Assert.Equal(count, request.ODataFeature().TotalCount);
        }

        [Fact]
        public void ApplyTo_Entity_ThrowsArgumentNull_QuerySettings()
        {
            // Arrange & Act
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://any");

            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Arrange
            ExceptionAssert.ThrowsArgumentNull(
                () => queryOptions.ApplyTo(entity: 42, querySettings: null),
                "querySettings");
        }

        [Theory]
        [InlineData("$filter=CustomerId eq 1")]
        [InlineData("$orderby=CustomerId")]
        [InlineData("$count=true")]
        [InlineData("$skip=1")]
        [InlineData("$top=0")]
        public void ApplyTo_Entity_ThrowsInvalidOperation_IfNonSelectExpand(string parameter)
        {
            // Arrange & Act
            IEdmModel model = GetEdmModel(c => c.CustomerId);

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost?" + parameter);
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => queryOptions.ApplyTo(42, new ODataQuerySettings()),
                "The requested resource is not a collection. Query options $filter, $orderby, $count, $skip, and $top can be applied only on collections.");
        }

        [Theory]
        [InlineData("?$select=Orders/OrderId", AllowedQueryOptions.Select)]
        [InlineData("?$expand=Orders", AllowedQueryOptions.Expand)]
        public void ApplyTo_Entity_DoesnotApply_IfSetApplied(string queryOption, AllowedQueryOptions appliedQueryOptions)
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));
            Customer customer = new Customer
            {
                CustomerId = 1,
                Orders = new List<Order>
                {
                    new Order {OrderId = 1}
                }
            };

            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost" + queryOption);
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            // Act
            object result = options.ApplyTo(customer, new ODataQuerySettings(), appliedQueryOptions);

            // Assert
            Assert.Equal(customer, (result as Customer));
        }

        [Theory]
        [InlineData("?$filter=CustomerId eq 1", AllowedQueryOptions.Filter)]
        [InlineData("?$orderby=CustomerId", AllowedQueryOptions.OrderBy)]
        [InlineData("?$count=true", AllowedQueryOptions.Count)]
        [InlineData("?$skip=1", AllowedQueryOptions.Skip)]
        [InlineData("?$top=1", AllowedQueryOptions.Top)]
        [InlineData("?$select=CustomerId", AllowedQueryOptions.Select)]
        [InlineData("?$expand=Orders", AllowedQueryOptions.Expand)]
        public void ApplyTo_DoesnotApply_IfSetApplied(string queryOption, AllowedQueryOptions appliedQueryOptions)
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost" + queryOption);
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));
            ODataQueryOptions options = new ODataQueryOptions(context, request);
            IQueryable<Customer> customers =
                Enumerable.Range(1, 10).Select(
                    i => new Customer
                    {
                        CustomerId = i,
                        Orders = new List<Order>
                        {
                            new Order {OrderId = i}
                        }
                    })
                .AsQueryable();

            // Act
            IQueryable result = options.ApplyTo(customers, new ODataQuerySettings(), appliedQueryOptions);

            // Assert
            Assert.Equal(10, (result as IQueryable<Customer>).Count());
        }

        [Fact]
        public void ApplyTo_DoesnotCalculateNextPageLink_IfRequestAlreadyHasNextPageLink()
        {
            // Arrange
            Uri nextPageLink = new Uri("http://localhost/nextpagelink");
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost/");
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ODataQueryOptions options = new ODataQueryOptions(context, request);
            request.ODataFeature().NextLink = nextPageLink;

            // Act
            IQueryable result = options.ApplyTo(Enumerable.Range(0, 100).AsQueryable(), new ODataQuerySettings { PageSize = 1 });

            // Assert
            Assert.Equal(nextPageLink, request.ODataFeature().NextLink);
            Assert.Single((result as IQueryable<int>));
        }

        [Fact]
        public void ODataQueryOptions_WithUnTypedContext_CanBeBuilt()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            model.AddElement(customer);

            ODataQueryContext context = new ODataQueryContext(model, customer);
            HttpRequest request = RequestFactory.Create(HttpMethods.Get,
                "http://localhost/?$filter=Id eq 42&$orderby=Id&$skip=42&$top=42&$count=true&$select=Id&$expand=Orders");

            // Act
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            // Assert
            Assert.NotNull(queryOptions.Filter);
            Assert.NotNull(queryOptions.OrderBy);
            Assert.NotNull(queryOptions.Skip);
            Assert.NotNull(queryOptions.Top);
            Assert.NotNull(queryOptions.SelectExpand);
            Assert.NotNull(queryOptions.Count);
        }

        [Fact]
        public async Task ODataQueryOptions_SetToApplied()
        {
            // Arrange
            string url = "http://localhost/odata/EntityModels?$filter=ID eq 1&$skip=1&$select=A&$expand=ExpandProp";
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Property", responseString);
            Assert.DoesNotContain("1", responseString);
            Assert.DoesNotContain("ExpandProperty", responseString);
        }

        [Theory]
        [InlineData("ExpandProp1")]
        [InlineData("ExpandProp2")]
        public async Task ODataQueryOptions_ApplyOrderByInExpandResult_WhenSetPageSize(string propName)
        {
            // Arrange
            string url = "http://localhost/odata/Products?$expand=" + propName;
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            var expandProp = result[0][propName] as JArray;
            Assert.Equal(2, expandProp.Count);
            Assert.Equal(1, expandProp[0]["ID"]);
            Assert.Equal(2, expandProp[1]["ID"]);
        }

        [Theory]
        [InlineData("ExpandProp3")]
        [InlineData("ExpandProp4")]
        public async Task ODataQueryOptions_ApplyOrderByInExpandResult_WhenSetPageSize_MultiplyKeys(string propName)
        {
            // Arrange
            string url = "http://localhost/odata/Products?$expand=" + propName;
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);
            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var result = responseObject["value"] as JArray;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            var expandProp = result[0][propName] as JArray;
            Assert.Equal(2, expandProp.Count);
            Assert.Equal(1, expandProp[0]["ID1"]);
            Assert.Equal(1, expandProp[0]["ID2"]);
            Assert.Equal(2, expandProp[1]["ID1"]);
            Assert.Equal(1, expandProp[1]["ID2"]);
        }

        [Fact]
        public void DuplicateUnsupportedQueryParametersIgnored()
        {
            // Arrange
            IEdmModel model = GetEdmModel(c => c.CustomerId);

            // a simple query with duplicate ignored parameters (key=test)
            string uri = "http://server/service/Customers?$top=10&test=1&test=2";
            var request = RequestFactory.Create(HttpMethods.Get, uri);

            // Act
            var queryOptions = new ODataQueryOptions(new ODataQueryContext(model, typeof(Customer)), request);

            // Assert
            Assert.Equal("10", queryOptions.RawValues.Top);
        }

        [Fact]
        public async Task DuplicateUnsupportedQueryParametersIgnoredWithNoException()
        {
            // Arrange
            string url = "http://localhost/odata/Products?$top=1&test=1&test=2";
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static HttpClient CreateClient()
        {
            var controllers = new[] { typeof(EntityModelsController), typeof(ProductsController) };

            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ODataQueryOptionTest_EntityModel>("EntityModels");
            builder.EntitySet<MyProduct>("Products");
            builder.EntitySet<ODataQueryOptionTest_EntityModelMultipleKeys>("ODataQueryOptionTest_EntityModelMultipleKeys");
            IEdmModel model = builder.GetEdmModel();

            var server = TestServerUtils.Create(
                opt =>
                {
                    opt.Count().OrderBy().Filter().Expand().SetMaxTop(null);
                    opt.AddRouteComponents("odata", model);
                },
                controllers);

            return server.CreateClient();
        }

        private static IEdmModel GetEdmModelWithoutKey()
        {
            return GetEdmModel<int>(null);
        }

        private static IEdmModel GetEdmModel<TKey>(Expression<Func<Customer, TKey>> keyDefinitionExpression)
        {
            Mock<ODataModelBuilder> mock = new Mock<ODataModelBuilder>();
            mock.Setup(b => b.ValidateModel(It.IsAny<IEdmModel>())).Callback(() => { });
            mock.CallBase = true;
            ODataModelBuilder builder = mock.Object;
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            if (keyDefinitionExpression != null)
            {
                customer.HasKey(keyDefinitionExpression);
            }

            customer.Property(c => c.CustomerId);
            customer.Property(c => c.Name);
            customer.Property(c => c.Website);
            customer.Property(c => c.SharePrice);
            customer.Property(c => c.ShareSymbol);
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }

        private class Customer
        {
            public int CustomerId { get; set; }
            public string Name { get; set; }
            public string Website { get; set; }
            public string ShareSymbol { get; set; }
            public decimal? SharePrice { get; set; }
            public List<Order> Orders { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
        }
    }

    public class EntityModelsController : ODataController
    {
        private static readonly IQueryable<ODataQueryOptionTest_EntityModel> _entityModels;

        public IActionResult Get(ODataQueryOptions<ODataQueryOptionTest_EntityModel> queryOptions)
        {
            // Don't apply Filter and Expand, but apply Select.
            var appliedQueryOptions = AllowedQueryOptions.Skip | AllowedQueryOptions.Filter | AllowedQueryOptions.Expand;
            var res = queryOptions.ApplyTo(_entityModels, appliedQueryOptions);
            return Ok(res.AsQueryable());
        }

        private static IEnumerable<ODataQueryOptionTest_EntityModel> CreateODataQueryOptionTest_EntityModel()
        {
            var entityModel = new ODataQueryOptionTest_EntityModel
            {
                ID = 1,
                A = "Property",
                ExpandProp = new ODataQueryOptionTest_EntityModelMultipleKeys
                {
                    ID1 = 2,
                    ID2 = 3,
                    A = "ExpandProperty"
                }
            };
            yield return entityModel;
        }

        static EntityModelsController()
        {
            _entityModels = CreateODataQueryOptionTest_EntityModel().AsQueryable();
        }
    }

    public class ProductsController : ODataController
    {
        private static readonly IQueryable<MyProduct> _products;

        [EnableQuery(PageSize = 2)]
        public IActionResult Get()
        {
            return Ok(_products);
        }

        private static IEnumerable<MyProduct> CreateProducts()
        {
            var prop1 = new List<ODataQueryOptionTest_EntityModel>
            {
                new ODataQueryOptionTest_EntityModel
                {
                    ID = 2,
                    A = "",
                    ExpandProp = null
                },
                new ODataQueryOptionTest_EntityModel
                {
                    ID = 1,
                    A = "",
                    ExpandProp = null
                },
                new ODataQueryOptionTest_EntityModel
                {
                    ID = 3,
                    A = "",
                    ExpandProp = null
                }
            };
            var prop2 = new List<ODataQueryOptionTest_EntityModelMultipleKeys>
            {
                new ODataQueryOptionTest_EntityModelMultipleKeys
                {
                    ID1 = 2,
                    ID2 = 3,
                    A = ""
                },
                new ODataQueryOptionTest_EntityModelMultipleKeys
                {
                    ID1 = 1,
                    ID2 = 1,
                    A = ""
                },
                new ODataQueryOptionTest_EntityModelMultipleKeys
                {
                    ID1 = 2,
                    ID2 = 1,
                    A = ""
                }
            };
            var product = new MyProduct
            {
                Id = 1,
                ExpandProp1 = prop1,
                ExpandProp2 = prop1.AsQueryable(),
                ExpandProp3 = prop2,
                ExpandProp4 = prop2.AsQueryable()
            };
            yield return product;
        }

        static ProductsController()
        {
            _products = CreateProducts().AsQueryable();
        }
    }

    public class MyProduct
    {
        public int Id { get; set; }

        public List<ODataQueryOptionTest_EntityModel> ExpandProp1 { get; set; }

        public IQueryable<ODataQueryOptionTest_EntityModel> ExpandProp2 { get; set; }

        public List<ODataQueryOptionTest_EntityModelMultipleKeys> ExpandProp3 { get; set; }

        public IQueryable<ODataQueryOptionTest_EntityModelMultipleKeys> ExpandProp4 { get; set; }
    }

    public class ODataQueryOptionTest_ComplexModel
    {
        public int A { get; set; }

        public string B { get; set; }
    }

    public class ODataQueryOptionTest_EntityModel
    {
        public int ID { get; set; }

        public string A { get; set; }

        public ODataQueryOptionTest_EntityModelMultipleKeys ExpandProp { get; set; }
    }

    public class ODataQueryOptionTest_EntityModelMultipleKeys
    {
        [Key]
        public int ID1 { get; set; }

        [Key]
        public int ID2 { get; set; }

        public string A { get; set; }
    }
}
