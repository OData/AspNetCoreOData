// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class ODataQueryOptionsOfTEntityTests
    {
        private static IEdmModel _model = GetEdmModel();

        [Fact]
        public void Ctor_Throws_Argument_IfContextIsofDifferentEntityType()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost");

            ODataQueryContext context = new ODataQueryContext(_model, typeof(QCustomer));

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => new ODataQueryOptions<int>(context, request),
                "context", "The entity type 'Microsoft.AspNetCore.OData.Tests.Query.ODataQueryOptionsOfTEntityTests+QCustomer' does not match the expected entity type 'System.Int32' as set on the query context.");
        }

        [Fact]
        public void Ctor_Throws_Argument_IfContextIsUnTyped()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://localhost");

            IEdmModel model = EdmCoreModel.Instance;
            IEdmType elementType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
            ODataQueryContext context = new ODataQueryContext(model, elementType);

            // At & Assert
            ExceptionAssert.ThrowsArgument(
                () => new ODataQueryOptions<int>(context, request),
                "context", "The property 'ElementClrType' of ODataQueryContext cannot be null.");
        }

        [Fact]
        public void Ctor_SuccedsIfEntityTypesMatch()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/?$top=10");

            ODataQueryContext context = new ODataQueryContext(_model, typeof(QCustomer));

            // Act
            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);

            // Assert
            Assert.Equal("10", query.Top.RawValue);
        }

        [Theory]
        [InlineData("IfMatch")]
        [InlineData("IfNoneMatch")]
        public void GetIfMatchOrNoneMatch_ReturnsETag_SetETagHeaderValue(string header)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<QCustomer> customer = builder.EntityType<QCustomer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id);
            customer.Property(c => c.Name).IsConcurrencyToken();
            builder.EntitySet<QCustomer>("Customers");
            IEdmModel model = builder.GetEdmModel();

            IEdmEntitySet customers = model.FindDeclaredEntitySet("Customers");

            HttpRequest request = RequestFactory.Create(model, opt => opt.AddModel(model));

            EntitySetSegment entitySetSegment = new EntitySetSegment(customers);
            ODataPath odataPath = new ODataPath(new[] { entitySetSegment });
            request.ODataFeature().Path = odataPath;

            Dictionary<string, object> properties = new Dictionary<string, object> { { "Name", "Foo" } };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);
            if (header.Equals("IfMatch"))
            {
                request.Headers.AddIfMatch(etagHeaderValue);
            }
            else
            {
                request.Headers.AddIfNoneMatch(etagHeaderValue);
            }

            ODataQueryContext context = new ODataQueryContext(model, typeof(QCustomer));

            // Act
            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);
            ETag result = header.Equals("IfMatch") ? query.IfMatch : query.IfNoneMatch;
            dynamic dynamicResult = result;

            // Assert
            Assert.Equal("Foo", result["Name"]);
            Assert.Equal("Foo", dynamicResult.Name);
        }

        [Theory]
        [InlineData("IfMatch")]
        [InlineData("IfNoneMatch")]
        public void GetIfMatchOrNoneMatch_ETagIsNull_IfETagHeaderValueNotSet(string header)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<QCustomer> customer = builder.EntityType<QCustomer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id);
            IEdmModel model = builder.GetEdmModel();
            HttpRequest request = RequestFactory.Create(model, opt => opt.AddModel(model));
            ODataQueryContext context = new ODataQueryContext(model, typeof(QCustomer));

            // Act
            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);
            ETag result = header.Equals("IfMatch") ? query.IfMatch : query.IfNoneMatch;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ApplyTo_ThrowsArgument_If_QueryTypeDoesnotMatch()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/?$top=10");

            ODataQueryContext context = new ODataQueryContext(_model, typeof(QCustomer));

            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => query.ApplyTo(Enumerable.Empty<int>().AsQueryable()),
                "query",
                "Cannot apply ODataQueryOptions of 'Microsoft.AspNetCore.OData.Tests.Query.ODataQueryOptionsOfTEntityTests+QCustomer' to IQueryable of 'System.Int32'. (Parameter 'query')");
        }

        [Fact]
        public void ApplyTo_Succeeds_If_QueryTypeDerivesFromOptionsType()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/?$top=10");

            ODataQueryContext context = new ODataQueryContext(_model, typeof(QCustomer));

            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);

            ExceptionAssert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<SubQCustomer>().AsQueryable()));
        }

        [Fact]
        public void ApplyTo_Succeeds_If_QueryTypeMatchesOptionsType()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/?$top=10");

            ODataQueryContext context = new ODataQueryContext(_model, typeof(QCustomer));

            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<QCustomer>().AsQueryable()));
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_ThrowsArgument_If_QueryTypeDoesnotMatch()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/?$top=10");

            ODataQueryContext context = new ODataQueryContext(_model, typeof(QCustomer));

            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => query.ApplyTo(Enumerable.Empty<int>().AsQueryable(), new ODataQuerySettings()),
                "query",
                "Cannot apply ODataQueryOptions of 'Microsoft.AspNetCore.OData.Tests.Query.ODataQueryOptionsOfTEntityTests+QCustomer' to IQueryable of 'System.Int32'. (Parameter 'query')");
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_Succeeds_If_QueryTypeDerivesFromOptionsType()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/?$top=10");

            ODataQueryContext context = new ODataQueryContext(_model, typeof(QCustomer));

            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<SubQCustomer>().AsQueryable(), new ODataQuerySettings()));
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_Succeeds_If_QueryTypeMatchesOptionsType()
        {
            // Arrange
            HttpRequest request = RequestFactory.Create(HttpMethods.Get, "http://server/?$top=10");

            ODataQueryContext context = new ODataQueryContext(_model, typeof(QCustomer));

            ODataQueryOptions<QCustomer> query = new ODataQueryOptions<QCustomer>(context, request);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<QCustomer>().AsQueryable(), new ODataQuerySettings()));
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<QCustomer>("Customers");
            return builder.GetEdmModel();
        }

        public class QCustomer
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class SubQCustomer : QCustomer
        {
        }
    }
}
