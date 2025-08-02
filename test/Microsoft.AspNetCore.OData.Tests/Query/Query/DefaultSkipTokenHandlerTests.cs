//-----------------------------------------------------------------------------
// <copyright file="DefaultSkipTokenHandlerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
    using Microsoft.AspNetCore.Http;
    using System.Runtime.Serialization;

    public class DefaultSkipTokenHandlerTests
    {
        private static IEdmModel _model = GetEdmModel();

        private static IEdmModel _modelLowerCamelCased = GetEdmModelLowerCamelCased();

        private static IEdmModel _modelAliased = GetEdmModelAliased();

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
        [InlineData("http://localhost/Customers?$select=Name", "http://localhost/Customers?$select=Name&$skiptoken=Id-42")]
        public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink(string baseUri, string expectedUri)
        {
            this.GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_Implementation(baseUri,
                expectedUri, _model);
        }

        [Theory]
        [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skiptoken=id-42")]
        [InlineData("http://localhost/Customers?$expand=orders", "http://localhost/Customers?$expand=orders&$skiptoken=id-42")]
        [InlineData("http://localhost/Customers?$select=name", "http://localhost/Customers?$select=name&$skiptoken=id-42")]
        public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_WithLowerCamelCase(string baseUri, string expectedUri)
        {
            this.GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_Implementation(baseUri,
                expectedUri, _modelLowerCamelCased);
        }

        [Theory]
        [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skiptoken=SkipCustomerId-42")]
        [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skiptoken=SkipCustomerId-42")]
        [InlineData("http://localhost/Customers?$select=FirstAndLastName", "http://localhost/Customers?$select=FirstAndLastName&$skiptoken=SkipCustomerId-42")]
        public void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_WithAlias(string baseUri, string expectedUri)
        {
            this.GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_Implementation(baseUri,
                expectedUri, _modelAliased);
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
            GenerateSkipTokenValue_Returns_SkipTokenValue_Implementation(_model,
                "Id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_Implementation(_modelLowerCamelCased,
                "id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_Implementation(_modelAliased,
                "SkipCustomerId-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_Implementation(
                _model,
                "Name",
                "Name-%27ZX%27,Id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase_WithOrderby()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_Implementation(
                _modelLowerCamelCased,
                "name",
                "name-%27ZX%27,id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias_WithOrderby()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_Implementation(
                _modelAliased,
                "FirstAndLastName",
                "FirstAndLastName-%27ZX%27,SkipCustomerId-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue_Implementation(
                _model,
                "Name",
                "Name-null,Id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase_WithOrderby_IfNullValue()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue_Implementation(
                _modelLowerCamelCased,
                "name",
                "name-null,id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias_WithOrderby_IfNullValue()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue_Implementation(
                _modelAliased,
                "FirstAndLastName",
                "FirstAndLastName-null,SkipCustomerId-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset_Implementation(
                _model,
                "Birthday",
                "Birthday-2021-01-19T19%3A04%3A05-08%3A00,Id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase_WithDateTimeOffset()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset_Implementation(
                _modelLowerCamelCased,
                "birthday",
                "birthday-2021-01-19T19%3A04%3A05-08%3A00,id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias_WithDateTimeOffset()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset_Implementation(
                _modelAliased,
                "DateOfBirth",
                "DateOfBirth-2021-01-19T19%3A04%3A05-08%3A00,SkipCustomerId-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue_Implementation(
                _model,
                "Gender",
                "Gender-Microsoft.AspNetCore.OData.Tests.Query.DefaultSkipTokenHandlerTests%2BGender%27Male%27,Id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithLowerCamelCase_WithOrderby_WithEnumValue()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue_Implementation(
                _modelLowerCamelCased,
                "gender",
                "gender-Microsoft.AspNetCore.OData.Tests.Query.DefaultSkipTokenHandlerTests%2BGender%27Male%27,id-42");
        }

        [Fact]
        public void GenerateSkipTokenValue_Returns_SkipTokenValue_WithAlias_WithOrderby_WithEnumValue()
        {
            GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue_Implementation(
                _modelAliased,
                "MaleOrFemale",
                "MaleOrFemale-Microsoft.AspNetCore.OData.Tests.Query.DefaultSkipTokenHandlerTests%2BGender%27Male%27,SkipCustomerId-42");
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
            ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue_Implementation(_model);
        }

        [Fact]
        public void ApplyToSkipTokenHandler_ThrowsODataException_WithLowerCamelCase_InvalidSkipTokenValue()
        {
            ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue_Implementation(_modelLowerCamelCased);
        }
        
        [Fact]
        public void ApplyToSkipTokenHandler_ThrowsODataException_WithAlias_InvalidSkipTokenValue()
        {
            ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue_Implementation(_modelAliased);
        }

        [Fact]
        public void ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable()
        {
            ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable_Implementation(
                _model,
                "Id-2");
        }

        [Fact]
        public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithLowerCamelCase_ToQueryable()
        {
            ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable_Implementation(
                _modelLowerCamelCased,
                "id-2");
        }

        [Fact]
        public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithAlias_ToQueryable()
        {
            ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable_Implementation(
                _modelAliased,
                "SkipCustomerId-2");
        }

        [Fact]
        public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable()
        {
            ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable_Implementation(
                _model,
                "Name",
                "Name-'Alex',Id-3");
        }

        [Fact]
        public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_WithLowerCamelCase_ToQueryable()
        {
            ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable_Implementation(
                _modelLowerCamelCased,
                "name",
                "name-'Alex',Id-3");
        }

        [Fact]
        public void ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_WithAlias_ToQueryable()
        {
            ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable_Implementation(
                _modelAliased,
                "FirstAndLastName",
                "FirstAndLastName-'Alex',SkipCustomerId-3");
        }

        private ODataSerializerContext GetSerializerContext(IEdmModel model, bool enableSkipToken = false)
        {
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            IEdmEntityType entityType = entitySet.EntityType();
            IEdmProperty edmProperty = entityType.FindProperty("Name");
            IEdmType edmType = entitySet.Type;
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            ODataQueryContext queryContext = new ODataQueryContext(model, edmType, path);
            queryContext.DefaultQueryConfigurations.EnableSkipToken = enableSkipToken;

            var request = RequestFactory.Create(opt => opt.AddRouteComponents(model));
            ResourceContext resource = new ResourceContext();
            ODataSerializerContext context = new ODataSerializerContext(resource, edmProperty, queryContext, null)
            {
                Model = model,
                Request = request
            };

            return context;
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder {ModelAliasingEnabled = false};
            builder.EntitySet<SkipCustomer>("Customers");
            return builder.GetEdmModel();
        }

        private static IEdmModel GetEdmModelLowerCamelCased()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder {ModelAliasingEnabled = false};
            builder.EntitySet<SkipCustomer>("Customers");
            builder.EnableLowerCamelCase();
            return builder.GetEdmModel();
        }

        private static IEdmModel GetEdmModelAliased()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder {ModelAliasingEnabled = true};
            var entitySetConfiguration = builder.EntitySet<SkipCustomer>("Customers");
            return builder.GetEdmModel();
        }
        
        private void GetNextPageLinkDefaultSkipTokenHandler_Returns_CorrectSkipTokenLink_Implementation(
            string baseUri,
            string expectedUri,
            IEdmModel edmModel)
        {
            // Arrange
            ODataSerializerContext serializerContext = this.GetSerializerContext(
                edmModel,
                true);
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            SkipCustomer instance = new SkipCustomer {Id = 42, Name = "ZX"};

            // Act
            Uri uri = handler.GenerateNextPageLink(
                new Uri(baseUri),
                10,
                instance,
                serializerContext);
            var actualUri = uri.ToString();

            // Assert
            Assert.Equal(
                expectedUri,
                actualUri);
        }
        
        private static void GenerateSkipTokenValue_Returns_SkipTokenValue_Implementation(IEdmModel edmModel, string expectedSkipToken)
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer {Id = 42, Name = "ZX"};

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
                lastMember,
                edmModel,
                null
                );

            // Assert
            Assert.Equal(
                expectedSkipToken,
                skipTokenValue);
        }

        private static void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_Implementation(
            IEdmModel edmModel,
            string propertyName,
            string expectedSkipToken)
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer {Id = 42, Name = "ZX"};

            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
                .First(c => c.Name == "SkipCustomer");
            IEdmProperty property = entityType.FindProperty(propertyName);

            OrderByClause clause = BuildOrderByClause(edmModel, propertyName);

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
                lastMember,
                edmModel,
                clause);

            // Assert
            Assert.Equal(
                expectedSkipToken,
                skipTokenValue);
        }

        private static OrderByClause BuildOrderByClause(IEdmModel edmModel, string propertyName)
        {
            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
                .First(c => c.Name == "SkipCustomer");
            IEdmProperty property = entityType.FindProperty(propertyName);

            IEdmNavigationSource entitySet = edmModel.FindDeclaredEntitySet("Customers");
            ResourceRangeVariable rangeVariable = new ResourceRangeVariable("$it", new EdmEntityTypeReference(entityType, true), entitySet);

            ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);
            SingleValuePropertyAccessNode node = new SingleValuePropertyAccessNode(source, property);
            return new OrderByClause(null, node, OrderByDirection.Ascending, rangeVariable);
        }

        private static void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_IfNullValue_Implementation(
            IEdmModel edmModel,
            string propertyName,
            string expectedSkipToken)
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer {Id = 42, Name = null};

            OrderByClause clause = BuildOrderByClause(edmModel, propertyName);

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
                lastMember,
                edmModel,
                clause);

            // Assert
            Assert.Equal(
                expectedSkipToken,
                skipTokenValue);
        }

        private static void GenerateSkipTokenValue_Returns_SkipTokenValue_WithDateTimeOffset_Implementation(
            IEdmModel edmModel,
            string propertyName,
            string expectedSkipToken)
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer
                {
                    Id = 42,
                    Birthday = new DateTime(
                        2021,
                        01,
                        20,
                        3,
                        4,
                        5,
                        DateTimeKind.Utc)
                };

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8
            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>()
                .First(c => c.Name == "SkipCustomer");

            OrderByClause clause = BuildOrderByClause(edmModel, propertyName);
            ODataSerializerContext context = new ODataSerializerContext()
            {
                TimeZone = timeZone
            };

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
                lastMember,
                edmModel,
                clause,
                context);

            // Assert
            Assert.Equal(
                expectedSkipToken,
                skipTokenValue);
        }
        
        private static void GenerateSkipTokenValue_Returns_SkipTokenValue_WithOrderby_WithEnumValue_Implementation(
            IEdmModel edmModel,
            string propertyName,
            string expectedSkipToken)
        {
            // Arrange
            SkipCustomer lastMember = new SkipCustomer {Id = 42, Name = "ZX", Gender = Gender.Male};

            OrderByClause clause = BuildOrderByClause(edmModel, propertyName);

            // Act
            string skipTokenValue = DefaultSkipTokenHandler.GenerateSkipTokenValue(
                lastMember,
                edmModel,
                clause);

            // Assert
            Assert.Equal(
                expectedSkipToken,
                skipTokenValue);
        }

        private static void ApplyToSkipTokenHandler_ThrowsODataException_InvalidSkipTokenValue_Implementation(
            IEdmModel edmModel)
        {
            // Arrange
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            ODataQuerySettings settings = new ODataQuerySettings {HandleNullPropagation = HandleNullPropagationOption.False};
            ODataQueryContext context = new ODataQueryContext(
                edmModel,
                typeof(SkipCustomer));
            HttpRequest request = RequestFactory.Create("Get", "http://localhost/");
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(
                "abc",
                context);
            IQueryable<SkipCustomer> customers = new List<SkipCustomer>().AsQueryable();

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(
                () => handler.ApplyTo(
                    customers,
                    skipTokenQuery,
                    new ODataQuerySettings(),
                    queryOptions),
                "Could not find a property named 'abc' on type 'Microsoft.AspNetCore.OData.Tests.Query.SkipCustomer'.");
        }
        
        private static void ApplyToOfTDefaultSkipTokenHandler_Applies_ToQueryable_Implementation(
            IEdmModel edmModel,
            string skipTokenQueryOptionRawValue)
        {
            // Arrange
            ODataQuerySettings settings = new ODataQuerySettings {HandleNullPropagation = HandleNullPropagationOption.False};
            ODataQueryContext context = new ODataQueryContext(
                edmModel,
                typeof(SkipCustomer));
            HttpRequest request = RequestFactory.Create("Get", "http://localhost/");
            ODataQueryOptions queryOptions = new ODataQueryOptions(context, request);

            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(
                skipTokenQueryOptionRawValue,
                context);
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            IQueryable<SkipCustomer> customers = new List<SkipCustomer>
                {
                    new SkipCustomer {Id = 2, Name = "Aaron"},
                    new SkipCustomer {Id = 1, Name = "Andy"},
                    new SkipCustomer {Id = 3, Name = "Alex"}
                }.AsQueryable();

            // Act
            SkipCustomer[] results = handler.ApplyTo(
                    customers,
                    skipTokenQuery,
                    settings,
                    queryOptions)
                .ToArray();

            // Assert
            SkipCustomer skipTokenCustomer = Assert.Single(results);
            Assert.Equal(
                3,
                skipTokenCustomer.Id);
            Assert.Equal(
                "Alex",
                skipTokenCustomer.Name);
        }

        private static void ApplyToOfTDefaultSkipTokenHandler_Applies_WithOrderByDesc_ToQueryable_Implementation(
            IEdmModel edmModel,
            string orderByPropertyName,
            string skipTokenQueryOptionRawValue)
        {
            // Arrange
            ODataQuerySettings settings = new ODataQuerySettings {HandleNullPropagation = HandleNullPropagationOption.False};
            ODataQueryContext context = new ODataQueryContext(
                edmModel,
                typeof(SkipCustomer));

            HttpRequest request = RequestFactory.Create(
                HttpMethods.Get,
                $"http://server/service/Customers/?$orderby={orderByPropertyName} desc&$skiptoken={skipTokenQueryOptionRawValue}");

            // Act
            ODataQueryOptions oDataQueryOptions = new ODataQueryOptions(context, request);

            SkipTokenQueryOption skipTokenQuery = new SkipTokenQueryOption(
                skipTokenQueryOptionRawValue,
                context);
            DefaultSkipTokenHandler handler = new DefaultSkipTokenHandler();
            IQueryable<SkipCustomer> customers = new List<SkipCustomer>
                {
                    new SkipCustomer {Id = 2, Name = "Aaron"},
                    new SkipCustomer {Id = 1, Name = "Andy"},
                    new SkipCustomer {Id = 3, Name = "Alex"}
                }.AsQueryable();

            // Act
            SkipCustomer[] results = handler.ApplyTo(
                    customers,
                    skipTokenQuery,
                    settings,
                    oDataQueryOptions)
                .ToArray();

            // Assert
            SkipCustomer skipTokenCustomer = Assert.Single(results);
            Assert.Equal(
                2,
                skipTokenCustomer.Id);
            Assert.Equal(
                "Aaron",
                skipTokenCustomer.Name);
        }

        [DataContract]
        public class SkipCustomer
        {
            [DataMember(Name = "SkipCustomerId")]
            public int Id { get; set; }

            [DataMember(Name = "FirstAndLastName")]
            public string Name { get; set; }

            [DataMember(Name = "DateOfBirth")]
            public DateTime Birthday { get; set; }

            [DataMember(Name = "MaleOrFemale")]
            public Gender Gender { get; set; }
        }

        public enum Gender
        {
            Male,

            Female
        }
    }
}
