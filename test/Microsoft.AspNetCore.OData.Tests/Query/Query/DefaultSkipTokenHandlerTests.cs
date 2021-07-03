// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class DefaultSkipTokenHandlerTests
    {
        private static IEdmModel _model = GetEdmModel();

        [Fact]
        public void GenerateNextPageLink_ReturnsNull_NullContext()
        {
            // Arrange
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();

            // Act & Assert
            Assert.Null(handler.GenerateNextPageLink(null, 2, null, null));
        }

        [Theory]
        [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skip=10")]
        [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skip=10")]
        public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectNextLink(string baseUri, string expectedUri)
        {
            // Arrange
            ODataSerializerContext serializerContext = GetSerializerContext(_model, false);
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();

            // Act
            var uri = handler.GenerateNextPageLink(new Uri(baseUri), 10, null, serializerContext);
            var actualUri = uri.ToString();

            // Assert
            Assert.Equal(expectedUri, actualUri);
        }

        [Theory]
        [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skiptoken=Id-42")]
        [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skiptoken=Id-42")]
        public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink(string baseUri, string expectedUri)
        {
            // Arrange
            ODataSerializerContext serializerContext = GetSerializerContext(_model, true);
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            SkipCustomer instance = new SkipCustomer
            {
                Id = 42,
                Name = "ZX"
            };

            // Act
            Uri uri = handler.GenerateNextPageLink(new Uri(baseUri), 10, instance, serializerContext);
            var actualUri = uri.ToString();

            // Assert
            Assert.Equal(expectedUri, actualUri);
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_StringEmpty()
        {
            // Arrange & Act & Assert
            Assert.Equal(string.Empty, DefaultSkipTokenHandler.GenerateSkipTokenValue(null, null, null));
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue()
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer
            {
                Id = 42,
                Name = "ZX"
            };

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(lastMember, _model, null);

            // Assert
            Assert.Equal("Id-42", skipTokenValue);
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby()
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer
            {
                Id = 42,
                Name = "ZX"
            };

            IEdmEntityType entityType = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "SkipCustomer");
            IEdmProperty property = entityType.FindProperty("Name");
            IList<OrderByNode> orderByNodes = new List<OrderByNode>
            {
                new OrderByPropertyNode(property, OrderByDirection.Ascending)
            };

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(lastMember, _model, orderByNodes);

            // Assert
            Assert.Equal("Name-%27ZX%27,Id-42", skipTokenValue);
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue()
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer
            {
                Id = 42,
                Name = null
            };

            IEdmEntityType entityType = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "SkipCustomer");
            IEdmProperty property = entityType.FindProperty("Name");
            IList<OrderByNode> orderByNodes = new List<OrderByNode>
            {
                new OrderByPropertyNode(property, OrderByDirection.Ascending)
            };

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(lastMember, _model, orderByNodes);

            // Assert
            Assert.Equal("Name-null,Id-42", skipTokenValue);
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset()
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer
            {
                Id = 42,
                Birthday = new DateTime(2021, 01, 20, 3, 4, 5, DateTimeKind.Utc)
            };

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8
            IEdmEntityType entityType = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "SkipCustomer");
            IEdmProperty property = entityType.FindProperty("Birthday");
            IList<OrderByNode> orderByNodes = new List<OrderByNode>
            {
                new OrderByPropertyNode(property, OrderByDirection.Ascending)
            };

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(lastMember, _model, orderByNodes, timeZone);

            // Assert
            Assert.Equal("Birthday-2021-01-19T19%3A04%3A05-08%3A00,Id-42", skipTokenValue);
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue()
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer
            {
                Id = 42,
                Name = "ZX",
                Gender = Gender.Male
            };

            IEdmEntityType entityType = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "SkipCustomer");
            IEdmProperty property = entityType.FindProperty("Gender");
            IList<OrderByNode> orderByNodes = new List<OrderByNode>
            {
                new OrderByPropertyNode(property, OrderByDirection.Ascending)
            };

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(lastMember, _model, orderByNodes);

            // Assert
            Assert.Equal("Gender-Microsoft.AspNetCore.OData.Tests.Query.DefaultSkipTokenHandlerTests%2BGender%27Male%27,Id-42", skipTokenValue);
        }

        [Fact]
        public void ApplyToSkipTokenHandler_ThrowsArgumentNull_Query()
        {
            // Arrange
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => handler.ApplyTo(query: null, null, null, null), "query");
        }

        [Fact]
        public void ApplyToSkipTokenHandler_ThrowsArgumentNull_SkipTokenQueryOption()
        {
            // Arrange
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            IQueryable query = Array.Empty<int>().AsQueryable();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => handler.ApplyTo(query, null, null, null), "skipTokenQueryOption");
        }

        [Fact]
        public void ApplyToSkipTokenHandler_ThrowsArgumentNull_QuerySettings()
        {
            // Arrange
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            IQueryable query = Array.Empty<int>().AsQueryable();
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, EdmCoreModel.Instance.GetInt32(false).Definition);
            SkipTokenQueryOption skipTokenQueryOption = new SkipTokenQueryOption("abc", context);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => handler.ApplyTo(query, skipTokenQueryOption, null, null), "querySettings");
        }

        [Fact]
        public void ApplyToSkipTokenHandler_ThrowsNotSupported_WithoutElementType()
        {
            // Arrange
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, EdmCoreModel.Instance.GetInt32(false).Definition);
            SkipTokenQueryOption skipTokenQueryOption = new SkipTokenQueryOption("abc", context);
            IQueryable query = Array.Empty<int>().AsQueryable();

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(
                () => handler.ApplyTo(query, skipTokenQueryOption, new ODataQuerySettings(), null),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }

        [Fact]
        public void ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue()
        {
            // Arrange
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            ODataQuerySettings settings = new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.False
            };
            ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));

            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("abc", context);
            IQueryable<SkipCustomer> customers = new List<SkipCustomer>().AsQueryable();

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => handler.ApplyTo(customers, skipTokenQuery, new ODataQuerySettings(), null),
                "Unable to parse the skiptoken value 'abc'. Skiptoken value should always be server generated.");
        }

        [Fact]
        public void ApplyToOfTDefaultSkipTokenHandler_Applies_ToQuaryable()
        {
            // Arrange
            ODataQuerySettings settings = new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.False
            };
            ODataQueryContext context = new ODataQueryContext(_model, typeof(SkipCustomer));

            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption("Id-2", context);
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            IQueryable<SkipCustomer> customers = new List<SkipCustomer>
            {
                new SkipCustomer { Id = 2, Name = "Aaron" },
                new SkipCustomer { Id = 1, Name = "Andy" },
                new SkipCustomer { Id = 3, Name = "Alex" }
            }.AsQueryable();

            // Act
            SkipCustomer[] results = handler.ApplyTo(customers, skipTokenQuery, settings, null).ToArray();

            // Assert
            SkipCustomer skipTokenCustomer = Assert.Single(results);
            Assert.Equal(3, skipTokenCustomer.Id);
            Assert.Equal("Alex", skipTokenCustomer.Name);
        }

        private ODataSerializerContext GetSerializerContext(IEdmModel model, bool enableSkipToken = false)
        {
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            IEdmEntityType entityType = entitySet.EntityType();
            IEdmProperty edmProperty = entityType.FindProperty("Name");
            IEdmType edmType = entitySet.Type;
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            ODataQueryContext queryContext = new ODataQueryContext(model, edmType, path);
            queryContext.DefaultQuerySettings.EnableSkipToken = enableSkipToken;

            var request = RequestFactory.Create(opt => opt.AddRouteComponents(model));
            ResourceContext resource = new ResourceContext();
            ODataSerializerContext context = new ODataSerializerContext(resource, edmProperty, queryContext, null)
            {
                Model = model
            };

            return context;
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SkipCustomer>("Customers");
            return builder.GetEdmModel();
        }

        public class SkipCustomer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public DateTime Birthday { get; set; }

            public Gender Gender { get; set; }
        }

        public enum Gender
        {
            Male,

            Female
        }
    }
}