//-----------------------------------------------------------------------------
// <copyright file="ODataQueryOptionsOfTEntityTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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

namespace Microsoft.AspNetCore.OData.Tests.Query;

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

    [Fact]
    public void Constructor_SetsProperties_FromDictionary()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            { "$filter", "Id eq 1" },
            { "$top", "5" }
        };

        // Act
        var options = new ODataQueryOptions<QCustomer>(queryParams, _model);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("Id eq 1", options.RawValues.Filter);
        Assert.Equal("5", options.RawValues.Top);
        Assert.IsType<ODataQueryOptions<QCustomer>>(options);
    }

    [Fact]
    public void Constructor_SetsAllSupportedODataOptions()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            { "$filter", "Name eq 'Test'" },
            { "$orderby", "Id desc" },
            { "$top", "10" },
            { "$skip", "2" },
            { "$select", "Id,Name" },
            { "$expand", "Orders" },
            { "$count", "true" },
            { "$format", "json" },
            { "$skiptoken", "abc" },
            { "$deltatoken", "def" },
            { "$apply", "aggregate(Id with sum as TotalId)" },
            { "$compute", "Amount mul 2 as DoubleAmount" },
            { "$search", "Test" }
        };

        // Act
        var options = new ODataQueryOptions<QCustomer>(queryParams, _model);

        // Assert
        Assert.Equal("Name eq 'Test'", options.RawValues.Filter);
        Assert.Equal("Id desc", options.RawValues.OrderBy);
        Assert.Equal("10", options.RawValues.Top);
        Assert.Equal("2", options.RawValues.Skip);
        Assert.Equal("Id,Name", options.RawValues.Select);
        Assert.Equal("Orders", options.RawValues.Expand);
        Assert.Equal("true", options.RawValues.Count);
        Assert.Equal("json", options.RawValues.Format);
        Assert.Equal("abc", options.RawValues.SkipToken);
        Assert.Equal("def", options.RawValues.DeltaToken);
        Assert.Equal("aggregate(Id with sum as TotalId)", options.RawValues.Apply);
        Assert.Equal("Amount mul 2 as DoubleAmount", options.RawValues.Compute);
        Assert.Equal("Test", options.RawValues.Search);
    }

    [Fact]
    public void Constructor_SetsAllSupportedODataOptions_Where_EdmModel_IsNull()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            { "$filter", "Name eq 'Test'" },
            { "$orderby", "Id desc" },
            { "$top", "10" },
            { "$skip", "2" },
            { "$select", "Id,Name" },
            { "$expand", "Orders" },
            { "$count", "true" },
            { "$format", "json" },
            { "$skiptoken", "abc" },
            { "$deltatoken", "def" },
            { "$apply", "aggregate(Id with sum as TotalId)" },
            { "$compute", "Amount mul 2 as DoubleAmount" },
            { "$search", "Test" }
        };

        // Act
        var options = new ODataQueryOptions<QCustomer>(queryParams);

        // Assert
        Assert.Equal("Name eq 'Test'", options.RawValues.Filter);
        Assert.Equal("Id desc", options.RawValues.OrderBy);
        Assert.Equal("10", options.RawValues.Top);
        Assert.Equal("2", options.RawValues.Skip);
        Assert.Equal("Id,Name", options.RawValues.Select);
        Assert.Equal("Orders", options.RawValues.Expand);
        Assert.Equal("true", options.RawValues.Count);
        Assert.Equal("json", options.RawValues.Format);
        Assert.Equal("abc", options.RawValues.SkipToken);
        Assert.Equal("def", options.RawValues.DeltaToken);
        Assert.Equal("aggregate(Id with sum as TotalId)", options.RawValues.Apply);
        Assert.Equal("Amount mul 2 as DoubleAmount", options.RawValues.Compute);
        Assert.Equal("Test", options.RawValues.Search);
    }

    [Fact]
    public void Constructor_Handles_EmptyDictionary()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>();

        // Act
        var options = new ODataQueryOptions<QCustomer>(queryParams, _model);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.RawValues.Filter);
        Assert.Null(options.RawValues.Top);
        Assert.Null(options.RawValues.OrderBy);
        Assert.Null(options.RawValues.Select);
        Assert.Null(options.RawValues.Expand);
    }

    [Fact]
    public void Constructor_Handles_UnknownKeys()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            { "unknown", "value" },
            { "$filter", "Id eq 1" }
        };

        // Act
        var options = new ODataQueryOptions<QCustomer>(queryParams, _model);

        // Assert
        Assert.Equal("Id eq 1", options.RawValues.Filter);
        // Unknown keys should not throw or affect known options
    }

    [Fact]
    public void Constructor_Supports_ODataPath()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            { "$filter", "Id eq 1" }
        };
        var entitySet = _model.FindDeclaredEntitySet("Customers");
        var path = new ODataPath(new EntitySetSegment(entitySet));

        // Act
        var options = new ODataQueryOptions<QCustomer>(queryParams, _model, path);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("Id eq 1", options.RawValues.Filter);
        Assert.Equal(typeof(QCustomer), options.Context.ElementClrType);
        Assert.Equal(path, options.Context.Path);
    }

    [Fact]
    public void JsonConverter_Deserializes_ODataQueryOptions_FromJson()
    {
        // Arrange
        var json = "{\"$filter\":\"Id eq 1\",\"$top\":\"5\"}";
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ODataQueryOptionsJsonConverter<QCustomer>(_model));

        // Act
        var odataOptions = JsonSerializer.Deserialize<ODataQueryOptions<QCustomer>>(json, options);

        // Assert
        Assert.NotNull(odataOptions);
        Assert.Equal("Id eq 1", odataOptions.RawValues.Filter);
        Assert.Equal("5", odataOptions.RawValues.Top);
    }

    [Fact]
    public void JsonConverter_Serializes_ODataQueryOptions_ToJson()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            { "$filter", "Id eq 1" },
            { "$top", "5" },
            { "$orderby", "Name ASC" },
            { "$expand", "Orders" },
            { "$select", "Id,Name" }
        };

        var odataOptions = new ODataQueryOptions<QCustomer>(queryParams, _model);

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
        };
        options.Converters.Add(new ODataQueryOptionsJsonConverter<QCustomer>(_model));

        // Act
        var json = JsonSerializer.Serialize(odataOptions, options);

        // Assert
        Assert.Contains("\"$filter\":\"Id eq 1\",\"$top\":\"5\",\"$orderby\":\"Name ASC\",\"$select\":\"Id,Name\",\"$expand\":\"Orders\"", json);
    }

    [Fact]
    public void Constructor_Throws_OnNullDictionary()
    {
        var model = GetEdmModel();
        Assert.Throws<ArgumentNullException>(() =>
            new ODataQueryOptions<QCustomer>(null, model));
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

        HttpRequest request = RequestFactory.Create(model, opt => opt.AddRouteComponents(model));

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
        HttpRequest request = RequestFactory.Create(model, opt => opt.AddRouteComponents(model));
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

    public class ODataQueryOptionsJsonConverter<TEntity> : JsonConverter<ODataQueryOptions<TEntity>>
    {
        private readonly IEdmModel _edmModel;

        public ODataQueryOptionsJsonConverter(IEdmModel edmModel)
        {
            _edmModel = edmModel;
        }

        public override ODataQueryOptions<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
            return new ODataQueryOptions<TEntity>(dict, _edmModel);
        }

        public override void Write(Utf8JsonWriter writer, ODataQueryOptions<TEntity> value, JsonSerializerOptions options)
        {
            var dict = new Dictionary<string, string>
            {
                ["$filter"] = value.RawValues.Filter,
                ["$top"] = value.RawValues.Top,
                ["$orderby"] = value.RawValues.OrderBy,
                ["$select"] = value.RawValues.Select,
                ["$expand"] = value.RawValues.Expand,
                ["$skip"] = value.RawValues.Skip,
                ["$count"] = value.RawValues.Count,
                ["$format"] = value.RawValues.Format,
                ["$skiptoken"] = value.RawValues.SkipToken,
                ["$deltatoken"] = value.RawValues.DeltaToken,
                ["$apply"] = value.RawValues.Apply,
                ["$compute"] = value.RawValues.Compute,
                ["$search"] = value.RawValues.Search
            };

            JsonSerializer.Serialize(writer, dict, options);
        }
    }
}
