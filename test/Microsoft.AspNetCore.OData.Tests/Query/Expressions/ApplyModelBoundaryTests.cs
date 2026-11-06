//-----------------------------------------------------------------------------
// <copyright file="ApplyModelBoundaryTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions;

public class ApplyModelBoundaryTests
{
    private static readonly IEdmModel Model = GetEdmModel();

    [Theory]
    [InlineData("compute(Instance as Result)")]
    [InlineData("groupby((Instance))")]
    [InlineData("groupby((Instance/Secret))")]
    [InlineData("compute(Instance/Secret as Result)")]
    [InlineData("compute(substring(Instance/Secret,0,6) as Result)")]
    [InlineData("compute(length(Instance/Config/Value) as Result)")]
    [InlineData("filter(Instance/Secret eq 'hidden')")]
    [InlineData("filter(Instance/HiddenTags/any(t:t eq 'hidden'))")]
    [InlineData("compute(Container as Result)")]
    [InlineData("compute(GroupByContainer as Result)")]
    [InlineData("compute(Values as Result)")]
    [InlineData("compute(Source as Result)")]
    [InlineData("filter(Values/any())")]
    public void Apply_RejectsWrapperImplementationMembers(string transformation)
    {
        // Arrange
        var context = new ODataQueryContext(Model, typeof(BoundaryRow));
        string query = "compute(Id as Key)/" + transformation;
        var apply = new ApplyQueryOption(query, context, CreateParser(new Dictionary<string, string> { { "$apply", query } }));

        // Act & Assert
        Assert.Throws<ODataException>(() => apply.ApplyTo(CreateRows(), Settings()).Cast<object>().ToList());
    }

    [Theory]
    [InlineData("Instance/Secret eq 'hidden'")]
    [InlineData("length(Instance/Secret) eq 6")]
    [InlineData("Values/any()")]
    public void FilterAfterApply_RejectsWrapperImplementationMembers(string filter)
    {
        // Arrange
        var query = Apply("compute(Id as Key)");
        var parser = CreateParser(new Dictionary<string, string> { { "$filter", filter } });
        var context = new QueryBinderContext(Model, Settings(), query.ElementType);

        // Act & Assert
        Assert.Throws<ODataException>(() => new FilterBinder().ApplyBind(query, parser.ParseFilter(), context).Cast<object>().ToList());
    }

    [Theory]
    [InlineData("Instance/Secret")]
    [InlineData("length(Instance/Secret)")]
    public void OrderByAfterApply_RejectsWrapperImplementationMembers(string orderBy)
    {
        // Arrange
        var query = Apply("compute(Id as Key)");
        var parser = CreateParser(new Dictionary<string, string> { { "$orderby", orderBy } });
        var context = new QueryBinderContext(Model, Settings(), query.ElementType);

        // Act & Assert
        Assert.Throws<ODataException>(() => new OrderByBinder().ApplyBind(query, parser.ParseOrderBy(), context, false).Cast<object>().ToList());
    }

    [Theory]
    [InlineData("Instance")]
    [InlineData("Source")]
    [InlineData("Container")]
    [InlineData("GroupByContainer")]
    [InlineData("Values")]
    public void Apply_ResolvesAliasesEvenWhenNamesMatchWrapperMembers(string alias)
    {
        // Act
        var result = Apply($"compute(Id as {alias})/groupby(({alias}))").Cast<DynamicTypeWrapper>().Single();

        // Assert
        Assert.Equal(1, result.Values[alias]);
    }

    [Fact]
    public void Apply_PreservesDeclaredRenamedPropertiesAndChainedAliases()
    {
        // Act
        var result = Apply("compute(Id as Key)/compute(length(DisplayName) as Size)/compute(Key add Size as Total)")
            .Cast<DynamicTypeWrapper>().Single();

        // Assert
        Assert.Equal(8, result.Values["Total"]);
    }

    [Fact]
    public void Apply_PreservesDynamicPropertiesAndGroupedAliases()
    {
        // Act
        var result = Apply("groupby((DynamicName))/compute(length(DynamicName) as Size)")
            .Cast<DynamicTypeWrapper>().Single();

        // Assert
        Assert.Equal(6, result.Values["Size"]);
    }

    [Theory]
    [InlineData("Unmodeled/Value eq 'hidden'")]
    [InlineData("Unmodeled/Tags/any(t:t eq 'hidden')")]
    [InlineData("Wrapped/Instance/Secret eq 'hidden'")]
    public void NestedDynamicAccess_RejectsClrMembersOfUnmodeledTypes(string filter)
    {
        // Act & Assert
        Assert.Throws<ODataException>(() => Apply("filter(" + filter + ")").Cast<object>().ToList());
    }

    private static IQueryable Apply(string transformation)
    {
        var context = new ODataQueryContext(Model, typeof(BoundaryRow));
        var parser = CreateParser(new Dictionary<string, string> { { "$apply", transformation } });
        return new ApplyQueryOption(transformation, context, parser).ApplyTo(CreateRows(), Settings());
    }

    private static ODataQueryOptionParser CreateParser(Dictionary<string, string> options)
    {
        var entitySet = Model.EntityContainer.FindEntitySet("Rows");
        return new ODataQueryOptionParser(Model, entitySet.EntityType(), entitySet, options);
    }

    private static ODataQuerySettings Settings()
    {
        return new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True };
    }

    private static IQueryable<BoundaryRow> CreateRows()
    {
        var row = new BoundaryRow
        {
            Id = 1,
            Name = "visible",
            Secret = "hidden",
            Config = new UnmodeledConfig { Value = "hidden", Tags = new[] { "hidden" } },
            HiddenTags = new[] { "hidden" },
            DynamicProperties = new Dictionary<string, object> { { "DynamicName", "public" } }
        };
        row.DynamicProperties["Unmodeled"] = row.Config;
        row.DynamicProperties["Wrapped"] = new ComputeWrapper<BoundaryRow> { Instance = row };
        return new[] { row }.AsQueryable();
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        var row = builder.EntitySet<BoundaryRow>("Rows").EntityType;
        row.Ignore(r => r.Secret);
        row.Ignore(r => r.Config);
        row.Ignore(r => r.HiddenTags);
        row.Property(r => r.Name).Name = "DisplayName";
        return builder.GetEdmModel();
    }

    public class BoundaryRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Secret { get; set; }
        public UnmodeledConfig Config { get; set; }
        public IList<string> HiddenTags { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    public class UnmodeledConfig
    {
        public string Value { get; set; }
        public IList<string> Tags { get; set; }
    }
}
