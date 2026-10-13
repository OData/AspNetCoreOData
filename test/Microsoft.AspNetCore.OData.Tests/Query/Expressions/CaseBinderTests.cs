//-----------------------------------------------------------------------------
// <copyright file="CaseBinderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions;

public class CaseBinderTests
{
    [Theory]
    [InlineData("case(true:'first',true:'second')", "first")]
    [InlineData("case(false:'never')", null)]
    [InlineData("case(true:null,true:'ignored')", null)]
    [InlineData("case(false:null,true:Name)", "name")]
    [InlineData("case(true:case(false:'no',true:concat(Name,'!')))", "name!")]
    public void Case_ReturnsFirstMatchingResult(string expression, string expected)
    {
        Assert.Equal(expected, Evaluate<string>(expression, new() { Name = "name" }));
    }

    [Theory]
    [InlineData(true, "enabled")]
    [InlineData(false, "fallback")]
    [InlineData(null, "fallback")]
    public void Case_OnlyTrueConditionsMatch(bool? enabled, string expected)
    {
        Assert.Equal(expected, Evaluate<string>("case(Enabled:'enabled',true:'fallback')", new() { Enabled = enabled }));
    }

    [Theory]
    [InlineData(HandleNullPropagationOption.True, true)]
    [InlineData(HandleNullPropagationOption.True, false)]
    [InlineData(HandleNullPropagationOption.False, true)]
    [InlineData(HandleNullPropagationOption.False, false)]
    public void Case_DoesNotEvaluateUnselectedResults(HandleNullPropagationOption nullPropagation, bool parameterize)
    {
        ODataQuerySettings settings = new() { HandleNullPropagation = nullPropagation, EnableConstantParameterization = parameterize };
        Assert.Equal(7, Evaluate<int?>("case(true:7,true:1 div Number)", new() { Number = 0 }, settings));
        Assert.Equal(7, Evaluate<int?>("case(true:7,1 div Number eq -1:9)", new() { Number = 0 }, settings));
    }

    [Theory]
    [InlineData(true, 42)]
    [InlineData(false, null)]
    [InlineData(null, null)]
    public void Case_AllowsNoMatchForValueTypes(bool? enabled, int? expected)
    {
        Assert.Equal(expected, Evaluate<int?>("case(Enabled:Number)", new() { Enabled = enabled, Number = 42 }));
    }

    [Theory]
    [InlineData(null, "fallback")]
    [InlineData("name", "name")]
    public void Case_UnifiesNullableAndRequiredStrings(string name, string expected)
    {
        Assert.Equal(expected, Evaluate<string>("case(Name ne null:Name,true:RequiredName)", new() { Name = name, RequiredName = "fallback" }));
    }

    [Theory]
    [InlineData(false, null, "fallback")]
    [InlineData(true, null, "fallback")]
    [InlineData(true, "city", "city")]
    public void Case_HandlesNullNavigation(bool hasAddress, string city, string expected)
    {
        CaseItem input = new() { Address = hasAddress ? new() { City = city } : null, RequiredName = "fallback" };
        Assert.Equal(expected, Evaluate<string>("case(Address/City ne null:Address/City,true:RequiredName)", input));
    }

    [Fact]
    public void Case_UnifiesNullableAndRequiredNumbers()
    {
        Assert.Equal(5, Evaluate<int?>("case(true:OptionalNumber,true:Number)", new() { OptionalNumber = 5, Number = 7 }));
        Assert.Null(Evaluate<int?>("case(true:OptionalNumber,true:Number)", new() { OptionalNumber = null, Number = 7 }));
    }

    [Fact]
    public void Case_PromotesNumericResults()
    {
        CaseItem input = new() { Enabled = true, Number = 4 };
        Assert.Equal(4L, Evaluate<long?>("case(Enabled:Number,true:cast(1,'Edm.Int64'))", input));
        Assert.Equal(4m, Evaluate<decimal?>("case(Enabled:Number,true:1.5m)", input));
        Assert.Equal(4f, Evaluate<float?>("case(Enabled:Number,true:1.5f)", input));
        Assert.Equal(4d, Evaluate<double?>("case(Enabled:Number,true:1.5d)", input));
        Assert.Equal(1.5m, Evaluate<decimal?>("case(false:Number,true:1.5m)", input));
    }

    [Fact]
    public void Case_PreservesNonNumericScalars()
    {
        CaseItem input = new() { Enabled = true, Status = CaseStatus.Second, Token = Guid.NewGuid(), Date = new(2026, 10, 13) };
        Assert.True(Evaluate<bool?>("case(true:Enabled)", input));
        Assert.Equal(CaseStatus.Second, Evaluate<CaseStatus?>("case(true:Status)", input));
        Assert.Equal(CaseStatus.First, Evaluate<CaseStatus?>("case(false:Status,true:Microsoft.AspNetCore.OData.Tests.Query.Expressions.CaseStatus'First')", input));
        Assert.Equal(input.Token, Evaluate<Guid?>("case(true:Token)", input));
        Assert.Equal(input.Date, Evaluate<DateOnly?>("case(true:Date)", input));
    }

    [Fact]
    public void Case_AllowsOnlyNullResults()
    {
        Assert.Null(Evaluate<object>("case(true:null)", new()));
    }

    [Theory]
    [InlineData("case(true:'text',false:1)")]
    [InlineData("case(true:Address,true:'text')")]
    public void Case_RejectsUnsupportedResults(string expression)
    {
        Assert.Throws<ODataException>(() => Evaluate<object>(expression, new()));
    }

    [Fact]
    public void Case_RespectsFunctionCallDepth()
    {
        ODataQuerySettings settings = new() { MaxFunctionCallDepth = 1 };
        Assert.Throws<ODataException>(() => Evaluate<string>("case(true:concat(Name,'!'))", new(), settings));
    }

    private static T Evaluate<T>(string expression, CaseItem input, ODataQuerySettings settings = null)
    {
        ODataConventionModelBuilder modelBuilder = new();
        modelBuilder.EntitySet<CaseItem>("Items").EntityType.Property(item => item.RequiredName).IsRequired();
        IEdmModel model = modelBuilder.GetEdmModel();
        IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Items");
        ODataQueryOptionParser parser = new(model, entitySet.EntityType, entitySet,
            new Dictionary<string, string> { ["$compute"] = expression + " as result" });
        SingleValueNode node = parser.ParseCompute().ComputedItems.Single().Expression;
        QueryBinderContext context = new(model, settings ?? new() { HandleNullPropagation = HandleNullPropagationOption.True }, typeof(CaseItem));
        FilterBinder binder = new();
        Expression body = binder.Bind(node, context);
        Func<CaseItem, T> evaluate = Expression.Lambda<Func<CaseItem, T>>(body, context.CurrentParameter).Compile();
        return evaluate(input);
    }
}

public class CaseItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string RequiredName { get; set; } = "";
    public bool? Enabled { get; set; }
    public int Number { get; set; }
    public int? OptionalNumber { get; set; }
    public CaseStatus Status { get; set; }
    public Guid Token { get; set; }
    public DateOnly Date { get; set; }
    public CaseAddress Address { get; set; }
}

public enum CaseStatus { First, Second }

public class CaseAddress
{
    public string City { get; set; }
}
