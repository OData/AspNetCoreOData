//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryOptionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class ApplyQueryOptionTest
{
    private static IEdmModel _model = GetEdmModel();

    [Fact]
    public void CtorApplyQueryOption_ThrowsArgumentNull_ForInputParameter()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNullOrEmpty(() => new ApplyQueryOption(null, null, null), "rawValue");
        ExceptionAssert.ThrowsArgumentNullOrEmpty(() => new ApplyQueryOption(string.Empty, null, null), "rawValue");

        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new ApplyQueryOption("groupby", null, null), "context");

        // Arrange & Act & Assert
        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        ExceptionAssert.ThrowsArgumentNull(() => new ApplyQueryOption("groupby", context, null), "queryOptionParser");
    }

    [Fact]
    public void ApplyToApplyQueryOption_ThrowsArgumentNull_ForInputParameter()
    {
        // Arrange
        IEdmType type = EdmCoreModel.Instance.GetString(false).Definition;
        ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, type);
        ApplyQueryOption apply = new ApplyQueryOption("groupby", context);

        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => apply.ApplyTo(null, null), "query");

        // Arrange & Act & Assert
        Mock<IQueryable> queryable = new Mock<IQueryable>();
        ExceptionAssert.ThrowsArgumentNull(() => apply.ApplyTo(queryable.Object, null), "querySettings");

        // Arrange & Act & Assert
        ExceptionAssert.Throws<NotSupportedException>(() => apply.ApplyTo(queryable.Object, new ODataQuerySettings()),
            "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");

        // Arrange & Act & Assert
        context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
        apply = new ApplyQueryOption("groupby", context);
        queryable.Setup(q => q.Provider).Returns(new System.Data.Linq.MyQueryProvider());
        ExceptionAssert.Throws<NotSupportedException>(() => apply.ApplyTo(queryable.Object, new ODataQuerySettings()),
            "$apply query options not supported for LINQ to SQL providers.");
    }

    // Legal apply queries usable against CustomerApplyTestData.
    // Tuple is: apply, expected number
    public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestApplies
    {
        get
        {
            return new TheoryDataSet<string, List<Dictionary<string, object>>>
            {
                {
                    "aggregate($count as Count)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Count", 5L} }
                    }
                },
                {
                    "aggregate(Id with sum as Id)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Id", 15} }
                    }
                },
                {
                    "aggregate(cast(Id, Edm.Int64) with sum as Id)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Id", 15L} }
                    }
                },
                {
                    "aggregate(SharePrice with sum as SharePrice)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "SharePrice", 22.5M} }
                    }
                },
                {
                    "aggregate(SharePrice with min as SharePrice)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "SharePrice", 2.5M} }
                    }
                },
                 {
                    "aggregate(SharePrice with max as SharePrice)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "SharePrice", 10M} }
                    }
                },
                {
                    "aggregate(SharePrice with average as SharePrice)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "SharePrice", 7.5M} }
                    }
                },
                {
                    "aggregate(Id with sum as Total, SharePrice with countdistinct as SharePriceDistinctCount)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "SharePriceDistinctCount", 3L}, { "Total", 15} }
                    }
                },
                {
                    "groupby((Name))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest"} },
                        new Dictionary<string, object> { { "Name", "Highest"} },
                        new Dictionary<string, object> { { "Name", "Middle"} }
                    }
                },
                {
                    "groupby((Name), aggregate(Id with sum as Total))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest"}, { "Total", 10} },
                        new Dictionary<string, object> { { "Name", "Highest"}, { "Total", 2} },
                        new Dictionary<string, object> { { "Name", "Middle"}, { "Total", 3 } }
                    }
                },
                {
                    "groupby((Name), aggregate($count as Count))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest"}, { "Count", 3L} },
                        new Dictionary<string, object> { { "Name", "Highest"}, { "Count", 1L} },
                        new Dictionary<string, object> { { "Name", "Middle"}, { "Count", 1L} }
                    }
                },
                {
                    "filter(Name eq 'Lowest')/groupby((Name))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest"} }
                    }
                },
                {
                    "groupby((Name), aggregate(Id with sum as Total))/filter(Total eq 3)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Middle"}, { "Total", 3 } }
                    }
                },
                {
                    "groupby((Name))/filter(Name eq 'Lowest')",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest"} }
                    }
                },
                {
                    "groupby((Address/City))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Address/City", "redmond"} },
                        new Dictionary<string, object> { { "Address/City", "seattle"} },
                        new Dictionary<string, object> { { "Address/City", "hobart"} },
                        new Dictionary<string, object> { { "Address/City", null} },
                    }
                },
                {
                    "groupby((Address/City, Address/State))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Address/City", "redmond"}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "Address/City", "seattle"}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "Address/City", "hobart"}, { "Address/State", null} },
                        new Dictionary<string, object> { { "Address/City", null}, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/City, Address/State))/groupby((Address/State), aggregate(Address/City with max as MaxCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(Address/City with max as MaxCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(startswith(Address/City, 's') with max as MaxCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", false}, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(endswith(Address/City, 't') with max as MaxCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", false}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(contains(Address/City, 'o') with max as MaxCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(length(Address/City) with max as MaxCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", 7}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", 6}, { "Address/State", null} },
                    }
                },
                {
                    "aggregate(year(StartDate) with max as MaxYear, year(StartDate) with min as MinYear)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxYear", 2018}, { "MinYear", 2016} },
                    }
                },
                {
                    "aggregate(month(StartDate) with max as MaxMonth, month(StartDate) with min as MinMonth)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxMonth", 5}, { "MinMonth", 1} },
                    }
                },
                {
                    "aggregate(day(StartDate) with max as MaxDay, day(StartDate) with min as MinDay)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxDay", 7}, { "MinDay", 1} },
                    }
                },
                {
                    "aggregate(hour(StartDate) with max as MaxHour, hour(StartDate) with min as MinHour)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxHour", 5 }, { "MinHour", 1} },
                    }
                },
                {
                    "aggregate(minute(StartDate) with max as MaxMinute, minute(StartDate) with min as MinMinute)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxMinute", 6}, { "MinMinute", 2} },
                    }
                },
                {
                    "aggregate(second(StartDate) with max as MaxSecond, second(StartDate) with min as MinSecond)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxSecond", 7}, { "MinSecond", 3} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(concat(Address/City,Address/State) with max as MaxCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", "seattleWA"}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", null}, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(Address/City with max as MaxCity, Address/City with min as MinCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", "seattle"}, { "MinCity", "redmond"}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", "hobart"}, { "MinCity", "hobart" }, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(Address/City with max as MaxCity, Id mul Id with sum as Id))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Id", 30}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Id", 25 }, { "Address/State", null} },
                    }
                },
                {
                    "groupby((Address/State), aggregate(Id mul Id with sum as Id))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Id", 30}, { "Address/State", "WA"} },
                        new Dictionary<string, object> { { "Id", 25}, { "Address/State", null} },
                    }
                },
                {
                    "filter(Company/CEO/EmployeeName eq 'john')/groupby((Company/CEO/EmployeeName))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"} }
                    }
                },
                {
                    "groupby((Company/CEO/EmployeeName))/filter(Company/CEO/EmployeeName eq 'john')",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"} }
                    }
                },
                {
                    "groupby((Name, Company/CEO/EmployeeName))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest"}, { "Company/CEO/EmployeeName", "john" } },
                        new Dictionary<string, object> { { "Name", "Highest"}, { "Company/CEO/EmployeeName", "tom" } },
                        new Dictionary<string, object> { { "Name", "Middle"}, { "Company/CEO/EmployeeName", "john" } },
                        new Dictionary<string, object> { { "Name", "Lowest"}, { "Company/CEO/EmployeeName", "alex" } },
                        new Dictionary<string, object> { { "Name", "Lowest"}, { "Company/CEO/EmployeeName", null } }
                    }
                },
                {
                    "groupby((Address/City, Company/CEO/EmployeeName))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Address/City", "redmond"}, { "Company/CEO/EmployeeName", "john" } },
                        new Dictionary<string, object> { { "Address/City", "seattle"}, { "Company/CEO/EmployeeName", "tom" } },
                        new Dictionary<string, object> { { "Address/City", "hobart"}, { "Company/CEO/EmployeeName", "john" } },
                        new Dictionary<string, object> { { "Address/City", null}, { "Company/CEO/EmployeeName", "alex" } },
                        new Dictionary<string, object> { { "Address/City", "redmond"}, { "Company/CEO/EmployeeName", null } }
                    }
                },
                {
                    "groupby((Company/CEO/HomeAddress/City))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "redmond"} },
                        new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "seattle"} },
                        new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "hobart"} },
                        new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", null} },
                    }
                },
                {
                    "groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "redmond"}, { "Company/CEO/HomeAddress/State", "WA"} },
                        new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "seattle"}, { "Company/CEO/HomeAddress/State", "WA"} },
                        new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", "hobart"}, { "Company/CEO/HomeAddress/State", null} },
                        new Dictionary<string, object> { { "Company/CEO/HomeAddress/City", null}, { "Company/CEO/HomeAddress/State", null} },
                    }
                },
                {
                    "groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))/groupby((Company/CEO/HomeAddress/State), aggregate(Company/CEO/HomeAddress/City with max as MaxCity))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Company/CEO/HomeAddress/State", "WA"} },
                        new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Company/CEO/HomeAddress/State", null} },
                    }
                },
                {
                    "groupby((Company/CEO/EmployeeName))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null} }
                    }
                },
                {
                    "groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"}, { "Company/CEO/BaseSalary", 20M} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"}, { "Company/CEO/BaseSalary", 0M} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null}, { "Company/CEO/BaseSalary", null} }
                    }
                },
                {
                    "groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))/groupby((Company/CEO/BaseSalary), aggregate(Company/CEO/EmployeeName with max as MaxEmployeeName))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "MaxEmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} },
                        new Dictionary<string, object> {{ "MaxEmployeeName", "alex"}, { "Company/CEO/BaseSalary", 0M} },
                        new Dictionary<string, object> {{ "MaxEmployeeName", null}, { "Company/CEO/BaseSalary", null} }
                    }
                },
                {
                    "groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))/groupby((Company/CEO/EmployeeName), aggregate(Company/CEO/BaseSalary with average as AverageBaseSalary))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "AverageBaseSalary", 20M }, { "Company/CEO/EmployeeName", "john"} },
                        new Dictionary<string, object> {{ "AverageBaseSalary", 20M }, { "Company/CEO/EmployeeName", "tom"} },
                        new Dictionary<string, object> {{ "AverageBaseSalary", 0M }, { "Company/CEO/EmployeeName", "alex"} },
                        new Dictionary<string, object> {{ "AverageBaseSalary", null }, { "Company/CEO/EmployeeName", null} }
                    }
                },
                {
                    "aggregate(Id mul Id with sum as Id)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Id", 55} }
                    }
                },
                {
                    // Note SharePrice and Id have different type
                    "aggregate(SharePrice mul Id with sum as Result)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Result", 65.0M} }
                    }
                },
                {
                    "groupby((Website))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Website", null} },
                    }
                },
                {
                    "aggregate(IntProp with max as MaxIntProp)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxIntProp", 2} }
                    }
                },
                {
                    "aggregate(IntProp with min as MinIntProp)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MinIntProp", 1} }
                    }
                },
                {
                    "aggregate(IntProp with countdistinct as DistinctIntProp)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "DistinctIntProp", 3L} }
                    }
                },
                {
                    "aggregate(IntProp with sum as TotalIntProp)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "TotalIntProp", 3M} }
                    }
                },
                {
                    "aggregate(IntProp with average as TotalIntProp)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "TotalIntProp", 1.5M} }
                    }
                },
                {
                    "aggregate(MixedProp with sum as TotalMixedProp)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "TotalMixedProp", 1M} }
                    }
                },
                {
                    "groupby((StringProp), aggregate(IntProp with min as MinIntProp))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "StringProp", "Test1" }, { "MinIntProp", 1} },
                        new Dictionary<string, object> { { "StringProp", "Test2" }, { "MinIntProp", 2} },
                        new Dictionary<string, object> { { "StringProp", "Test3" }, { "MinIntProp", null} },
                        new Dictionary<string, object> { { "StringProp", null }, { "MinIntProp", null} },
                    }
                },
                {
                    "groupby((StringProp), aggregate(IntProp with min as MinIntProp))/groupby((StringProp))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "StringProp", "Test1" } },
                        new Dictionary<string, object> { { "StringProp", "Test2" } },
                        new Dictionary<string, object> { { "StringProp", "Test3" } },
                        new Dictionary<string, object> { { "StringProp", null } },
                    }
                },
                {
                    "aggregate($count as Count)/compute(Count add Count as DoubleCount)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Count", 5L}, { "DoubleCount", 10L } }
                    }
                },
                {
                    "groupby((Name), aggregate(Id with sum as Total))/compute(Total add Total as DoubleTotal, length(Name) as NameLen)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest"},  { "Total", 10}, { "DoubleTotal", 20}, { "NameLen", 6},},
                        new Dictionary<string, object> { { "Name", "Highest"}, { "Total", 2} , { "DoubleTotal", 4} , { "NameLen", 7} ,},
                        new Dictionary<string, object> { { "Name", "Middle"},  { "Total", 3 }, { "DoubleTotal", 6 }, { "NameLen", 6 }, }
                    }
                },
                {
                    "compute(length(Name) as NameLen)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", 6}, { "Id", 1},},
                        new Dictionary<string, object> { { "Name", "Highest"},  { "NameLen", 7}, { "Id", 2},},
                        new Dictionary<string, object> { { "Name", "Middle" },  { "NameLen", 6}, { "Id", 3},},
                        new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", 6}, { "Id", 4},},
                        new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", 6}, { "Id", 5},},
                    }
                },
                {
                    "compute(length(ShareSymbol) as NameLen)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", null}, { "Id", 1},},
                        new Dictionary<string, object> { { "Name", "Highest"},  { "NameLen", null}, { "Id", 2},},
                        new Dictionary<string, object> { { "Name", "Middle" },  { "NameLen", null}, { "Id", 3},},
                        new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", null}, { "Id", 4},},
                        new Dictionary<string, object> { { "Name", "Lowest" },  { "NameLen", null}, { "Id", 5},},
                    }
                },
                {
                    "compute(length(Name) as NameLen)/aggregate(NameLen with sum as TotalLen)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "TotalLen", 31} }
                    }
                },
                {
                    "compute(length(Name) as NameLen)/aggregate(NameLen add Id with sum as TotalLen)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "TotalLen", 46} }
                    }
                },
                {
                    "compute(length(Name) as NameLen)/groupby((Name),aggregate(Id with sum as Total, NameLen with max as MaxNameLen))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Name", "Lowest"},  { "Total", 10},  { "MaxNameLen", 6},},
                        new Dictionary<string, object> { { "Name", "Highest"}, { "Total", 2} ,  { "MaxNameLen", 7} ,},
                        new Dictionary<string, object> { { "Name", "Middle"},  { "Total", 3 },  { "MaxNameLen", 6 }, }
                    }
                },
                {
                    "compute(length(Name) as NameLen)/groupby((NameLen),aggregate(Id with sum as Total))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Total", 13},  { "NameLen", 6},},
                        new Dictionary<string, object> { { "Total", 2} ,  { "NameLen", 7} ,},
                    }
                },
                {
                    "groupby((Address/State), aggregate(Address/City with max as MaxCity, Address/City with min as MinCity))/compute(length(MaxCity) as MaxCityLen)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", "seattle"}, { "MinCity", "redmond"}, { "Address/State", "WA"}, {"MaxCityLen", 7 } },
                        new Dictionary<string, object> { { "MaxCity", "hobart"}, { "MinCity", "hobart" }, { "Address/State", null}, {"MaxCityLen", 6 } },
                    }
                },
                {
                    "compute(length(Address/City) as CityLength)/groupby((Address/State), aggregate(Address/City with max as MaxCity, Address/City with min as MinCity, CityLength with max as MaxCityLen))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "MaxCity", "seattle"}, { "MinCity", "redmond"}, { "Address/State", "WA"}, {"MaxCityLen", 7 } },
                        new Dictionary<string, object> { { "MaxCity", "hobart"}, { "MinCity", "hobart" }, { "Address/State", null}, {"MaxCityLen", 6 } },
                    }
                },
            };
        }
    }

    public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestAppliesMixedWithOthers
    {
        get
        {
            return new TheoryDataSet<string, List<Dictionary<string, object>>>
            {
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total))&$filter=Total eq 3",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}}
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total))&$orderby=Name",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                        new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                    }
                },
                {
                    "$apply=groupby((Name))&$orderby=Name",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}},
                        new Dictionary<string, object> {{"Name", "Lowest"}},
                        new Dictionary<string, object> {{"Name", "Middle"}},
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total, Id with sum as Total2))&$orderby=Total",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                        new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total, Id with sum as Total2))&$orderby=Total, Total2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                        new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total))&$orderby=Name, Total",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                        new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                    }
                },
                {
                    "$apply=groupby((Address/City))&$orderby=Address/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", null}},
                        new Dictionary<string, object> {{"Address/City", "hobart"}},
                        new Dictionary<string, object> {{"Address/City", "redmond"}},
                        new Dictionary<string, object> {{"Address/City", "seattle"}},
                    }
                },
                {
                    "$apply=groupby((Address/City))&$filter=Address/City eq 'redmond'&$orderby=Address/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", "redmond"}},
                    }
                },
                {
                    "$apply=groupby((Address/City, Address/State))&$filter=Address/State eq 'WA'&$orderby=Address/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                        new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                    }
                },
                {
                    "$apply=groupby((Address/City, Address/State))&$orderby=Address/State desc, Address/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                        new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                        new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                        new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                    }
                },
                {
                    "$apply=groupby((Address/City))&$filter=Address/City eq 'redmond'",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", "redmond"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City))&$orderby=Company/CEO/HomeAddress/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", null}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "hobart"}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City))&$filter=Company/CEO/HomeAddress/City eq 'redmond'&$orderby=Company/CEO/HomeAddress/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))&$filter=Company/CEO/HomeAddress/State eq 'WA'&$orderby=Company/CEO/HomeAddress/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}, {"Company/CEO/HomeAddress/State", "WA"}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}, {"Company/CEO/HomeAddress/State", "WA"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))&$orderby=Company/CEO/HomeAddress/State desc, Company/CEO/HomeAddress/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}, {"Company/CEO/HomeAddress/State", "WA"}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}, {"Company/CEO/HomeAddress/State", "WA"}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", null}, {"Company/CEO/HomeAddress/State", null}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "hobart"}, {"Company/CEO/HomeAddress/State", null}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City))&$filter=Company/CEO/HomeAddress/City eq 'redmond'",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/EmployeeName))&$orderby=Company/CEO/EmployeeName",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"} }
                    }
                },
                {
                    "$apply=groupby((Company/CEO/EmployeeName))&$filter=Company/CEO/EmployeeName eq 'alex'&$orderby=Company/CEO/EmployeeName",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/EmployeeName", "alex"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))&$filter= Company/CEO/BaseSalary eq 20&$orderby=Company/CEO/EmployeeName",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"}, { "Company/CEO/BaseSalary", 20M} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} }
                    }
                },
                {
                    "$apply=groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))&$orderby=Company/CEO/BaseSalary desc, Company/CEO/EmployeeName desc",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"}, { "Company/CEO/BaseSalary", 20M} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"}, { "Company/CEO/BaseSalary", 0M} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null}, { "Company/CEO/BaseSalary", null} }
                    }
                },
                {
                    "$apply=groupby((Company/CEO/EmployeeName))&$filter=Company/CEO/EmployeeName eq 'john'",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/EmployeeName", "john"}},
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total))/compute(Total mul 2 as NewTotal)&$orderby=Name, NewTotal",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2},  {"NewTotal", 4}, },
                        new Dictionary<string, object> {{"Name", "Lowest"},  {"Total", 10}, {"NewTotal", 20},},
                        new Dictionary<string, object> {{"Name", "Middle"},  {"Total", 3},  {"NewTotal", 6}, },
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total))/compute(Total mul 2 as NewTotal)/filter(NewTotal gt 6)&$orderby=Name, NewTotal",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Lowest"},  {"Total", 10}, {"NewTotal", 20},},
                    }
                },
                //{
                //    "$apply=groupby((Name))&$top=1",
                //    new List<Dictionary<string, object>>
                //    {
                //        new Dictionary<string, object> {{"Name", "Highest"}},
                //    }
                //},
                //{
                //    "$apply=groupby((Name))&$skip=1",
                //    new List<Dictionary<string, object>>
                //    {
                //        new Dictionary<string, object> {{"Name", "Lowest"}},
                //        new Dictionary<string, object> {{"Name", "Middle"}},
                //    }
                //},
            };
        }
    }

    public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestAppliesForPaging
    {
        get
        {
            return new TheoryDataSet<string, List<Dictionary<string, object>>>
            {
                {
                    "$apply=aggregate(Id with sum as Id)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Id", 15} }
                    }
                },
                {
                    "$apply=aggregate(Id with sum as Total)",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "Total", 15} }
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total, Id with sum as Total2))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                        new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total, Id with sum as Total2))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                    }
                },
                {
                    "$apply=groupby((Name))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}},
                        new Dictionary<string, object> {{"Name", "Lowest"}},
                    }
                },
                {
                    "$apply=groupby((Name))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Middle"}},
                    }
                },
                {
                    "$apply=groupby((Id, Name))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Lowest"}, { "Id", 1}},
                        new Dictionary<string, object> {{"Name", "Highest"}, { "Id", 2}},
                    }
                },
                {
                    "$apply=groupby((Name, Id))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest" }, { "Id", 2}},
                        new Dictionary<string, object> {{"Name", "Lowest" }, { "Id", 1}},
                    }
                },
                {
                    "$apply=groupby((Name, Id))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Lowest" }, { "Id", 4}},
                        new Dictionary<string, object> {{"Name", "Lowest" }, { "Id", 5}},
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                        new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                    }
                },
                {
                    "$apply=groupby((Name), aggregate(Id with sum as Total))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                    }
                },
                {
                    "$apply=groupby((Address/City))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", null}},
                        new Dictionary<string, object> {{"Address/City", "hobart"}},
                    }
                },
                {
                    "$apply=groupby((Address/City))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", "redmond"}},
                        new Dictionary<string, object> {{"Address/City", "seattle"}},
                    }
                },
                {
                    "$apply=groupby((Address/City, Address/State))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                        new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                    }
                },
                {
                    "$apply=groupby((Address/City, Address/State))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                        new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", null}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "hobart"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", null}, {"Company/CEO/HomeAddress/State", null}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "hobart"}, {"Company/CEO/HomeAddress/State", null}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/HomeAddress/City, Company/CEO/HomeAddress/State))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "redmond"}, {"Company/CEO/HomeAddress/State", "WA"}},
                        new Dictionary<string, object> {{"Company/CEO/HomeAddress/City", "seattle"}, {"Company/CEO/HomeAddress/State", "WA"}},
                    }
                },
                {
                    "$apply=groupby((Company/CEO/EmployeeName))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"} }
                    }
                },
                {
                    "$apply=groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", null}, { "Company/CEO/BaseSalary", null} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "alex"}, { "Company/CEO/BaseSalary", 0M} }
                    }
                },
                {
                    "$apply=groupby((Company/CEO/EmployeeName, Company/CEO/BaseSalary))&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "john"}, { "Company/CEO/BaseSalary", 20M} },
                        new Dictionary<string, object> {{ "Company/CEO/EmployeeName", "tom"}, { "Company/CEO/BaseSalary", 20M} }
                    }
                },
                {
                    "$apply=groupby((Name))&$orderby=Name",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}},
                        new Dictionary<string, object> {{"Name", "Lowest"}},
                    }
                },
                {
                    "$apply=groupby((Name))&$skip=2&$orderby=Name",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Middle"}},
                    }
                },
                {
                    "$apply=groupby((Name))&$orderby=Name desc",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Middle"}},
                        new Dictionary<string, object> {{"Name", "Lowest"}},
                    }
                },
                {
                    "$apply=groupby((Name))&$skip=2&$orderby=Name desc",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}},
                    }
                },
                {
                    "$apply=groupby((Id, Name))&$orderby=Id",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Lowest"}, { "Id", 1}},
                        new Dictionary<string, object> {{"Name", "Highest"}, { "Id", 2}},
                    }
                },
                {
                    "$apply=groupby((Id, Name))&$orderby=Name",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Highest"}, { "Id", 2}},
                        new Dictionary<string, object> {{"Name", "Lowest"}, { "Id", 1}},
                    }
                },
                {
                    "$apply=groupby((Id, Name))&$orderby=Name&$skip=2",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Name", "Lowest"}, { "Id", 4}},
                        new Dictionary<string, object> {{"Name", "Lowest"}, { "Id", 5}},
                    }
                },
                {
                    "$apply=groupby((Address/City))&$orderby=Address/City",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", null}},
                        new Dictionary<string, object> {{"Address/City", "hobart"}},
                    }
                },
                {
                    "$apply=groupby((Address/City, Address/State))&$orderby=Address/State",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                        new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                    }
                },
                {
                    "$apply=compute(0 as ComputeProperty)/groupby((ComputeProperty))&$orderby=ComputeProperty desc",
                    new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> {{ "ComputeProperty", 0}},
                    }
                },
            };
        }
    }


    // Legal filter queries usable against CustomerFilterTestData.
    // Tuple is: filter, expected list of customer ID's
    public static TheoryDataSet<string, int[]> CustomerTestFilters
    {
        get
        {
            return new TheoryDataSet<string, int[]>
            {
                // Primitive properties
                { "Name eq 'Highest'", new int[] { 2 } },
                { "endswith(Name, 'est')", new int[] { 1, 2, 4, 5 } },

                // Complex properties
                { "Address/City eq 'redmond'", new int[] { 1, 5 } },
                { "contains(Address/City, 'e')", new int[] { 1, 2, 5 } },
                { "Company/CEO/HomeAddress/City eq 'redmond'", new int[] { 1, 4 } },
                { "contains(Company/CEO/HomeAddress/City, 'e')", new int[] { 1, 2, 4 } },

                // Primitive property collections
                { "Aliases/any(alias: alias eq 'alias34')", new int[] { 3, 4 } },
                { "Aliases/any(alias: alias eq 'alias4')", new int[] { 4 } },
                { "Aliases/all(alias: alias eq 'alias2')", new int[] { 2 } },

                // Navigational properties
                { "Orders/any(order: order/OrderId eq 12)", new int[] { 1 } },
                { "startswith(Company/CompanyName, 'company')", new int[] { 1, 2, 3, 4 } },
                { "Company/CompanyName eq 'company1'", new int[] { 1, 2 } },
                { "Company/CompanyName eq 'company2'", new int[] { 3 } },
                { "Company/CompanyName eq 'company3'", new int[] { 4 } },
                { "Company/CEO/EmployeeName eq 'john'", new int[] { 1, 3 } },
                { "Company/CEO/EmployeeName eq 'tom'", new int[] { 2 } },
                { "Company/CEO/EmployeeName eq 'alex'", new int[] { 4 } },
                { "Company/CEO/BaseSalary eq 0", new int[] { 4 } },
                { "Company/CEO/BaseSalary eq 20", new int[] { 1, 2, 3 } },
            };
        }
    }

    // Test data used by CustomerTestApplies TheoryDataSet
    public static List<Customer> CustomerApplyTestData
    {
        get
        {
            List<Customer> customerList = new List<Customer>();

            Customer c = new Customer
            {
                Id = 1,
                Name = "Lowest",
                SharePrice = 10,
                Address = new Address { City = "redmond", State = "WA" },
                DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test1" }, { "IntProp", 1 }, { "MixedProp", 1 } },
                StartDate = new DateTimeOffset(new DateTime(2018, 02, 07, 1, 2, 3)),
            };
            c.Company = new Company()
            {
                CompanyName = "company1",
                CEO = new Employee()
                {
                    EmployeeName = "john",
                    BaseSalary = 20,
                    HomeAddress = new Address { City = "redmond", State = "WA" }
                }
            };
            c.Orders = new List<Order>
            {
                new Order { OrderId = 11, Customer = c },
                new Order { OrderId = 12, Customer = c },
            };
            customerList.Add(c);

            c = new Customer
            {
                Id = 2,
                Name = "Highest",
                SharePrice = 2.5M,
                Address = new Address { City = "seattle", State = "WA" },
                Aliases = new List<string> { "alias2", "alias2" },
                DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test2" }, { "IntProp", 2 }, { "MixedProp", "String" } },
                StartDate = new DateTimeOffset(new DateTime(2017, 03, 07, 5, 6, 7))
            };
            c.Company = new Company()
            {
                CompanyName = "company1",
                CEO = new Employee()
                {
                    EmployeeName = "tom",
                    BaseSalary = 20,
                    HomeAddress = new Address { City = "seattle", State = "WA" }
                }
            };
            customerList.Add(c);

            c = new Customer
            {
                Id = 3,
                Name = "Middle",
                Address = new Address { City = "hobart" },
                Aliases = new List<string> { "alias2", "alias34", "alias31" },
                DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test3" } },
                StartDate = new DateTimeOffset(new DateTime(2018, 01, 01, 2, 3, 4)),
            };
            c.Company = new Company()
            {
                CompanyName = "company2",
                CEO = new Employee()
                {
                    EmployeeName = "john",
                    BaseSalary = 20,
                    HomeAddress = new Address { City = "hobart" }
                }
            };
            customerList.Add(c);

            c = new Customer
            {
                Id = 4,
                Name = "Lowest",
                Aliases = new List<string> { "alias34", "alias4" },
                StartDate = new DateTimeOffset(new DateTime(2016, 05, 07, 2, 3, 4)),
            };
            c.Company = new Company()
            {
                CompanyName = "company3",
                CEO = new Employee()
                {
                    EmployeeName = "alex",
                    HomeAddress = new Address { City = "redmond", State = "WA" }
                }
            };
            customerList.Add(c);

            c = new Customer
            {
                Id = 5,
                Name = "Lowest",
                SharePrice = 10,
                Address = new Address { City = "redmond", State = "WA" },
            };
            customerList.Add(c);

            return customerList;
        }
    }

    public static TheoryDataSet<string> AppliesWithReferencesOnGroupedOut
    {
        get
        {
            return new TheoryDataSet<string>
            {
                "$apply=groupby((Name))&$filter=Id eq 1",
                "$apply=groupby((Name))/filter(Id eq 1)",
                "$apply=groupby((Name))/filter(Address/City eq 1)",
                "$apply=groupby((Name))/groupby((Id))",
                "$apply=groupby((Company/CEO/EmployeeName))&$filter=Id eq 1",
                "$apply=groupby((Company/CEO/EmployeeName))/filter(Id eq 1)",
                "$apply=groupby((Company/CEO/EmployeeName))/filter(Address/City eq 1)",
                "$apply=groupby((Company/CEO/EmployeeName))/groupby((Id))",
                "$apply=groupby((Company/CEO/EmployeeName))/groupby((Company/CEO/BaseSalary))",
                "$apply=groupby((Company/CEO/EmployeeName))/filter(Company/CEO/BaseSalary eq 20)",
                "$apply=groupby((Company/CEO/EmployeeName))&$filter=Company/CEO/BaseSalary eq 20",
                "$apply=groupby((Company/CEO/EmployeeName))/groupby((Company/CEO/BaseSalary))"
            };
        }
    }

    [Theory]
    [MemberData(nameof(CustomerTestApplies))]
    public void ApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
    {
        // Arrange
        var model = new ODataModelBuilder()
                        .Add_Order_EntityType()
                        .Add_Customer_EntityType_With_Address()
                        .Add_CustomerOrders_Relationship()
                        .Add_Customer_EntityType_With_CollectionProperties()
                        .Add_Company_EntityType()
                        .Add_CustomerCompany_Relationship()
                        .Add_Employee_EntityType_With_HomeAddress()
                        .Add_CompanyEmployees_Relationship()
                        .Add_Customers_EntitySet()
                        .GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var queryOptionParser = new ODataQueryOptionParser(
            context.Model,
            context.ElementType,
            context.NavigationSource,
            new Dictionary<string, string> { { "$apply", filter } });
        var applyOption = new ApplyQueryOption(filter, context, queryOptionParser);
        IEnumerable<Customer> customers = CustomerApplyTestData;

        // Act
        IQueryable queryable = applyOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

        // Assert
        Assert.NotNull(queryable);
        var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

        Assert.Equal(aggregation.Count(), actualCustomers.Count());

        var aggEnum = actualCustomers.GetEnumerator();

        foreach (var expected in aggregation)
        {
            aggEnum.MoveNext();
            var agg = aggEnum.Current;
            foreach (var key in expected.Keys)
            {
                object value = GetValue(agg, key);
                Assert.Equal(expected[key], value);
            }
        }
    }
    /*
    [Theory]
    [MemberData(nameof(CustomerTestAppliesMixedWithOthers))]
    public void ClausesAfterApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
    {
        // Arrange
        var model = new ODataModelBuilder()
                        .Add_Order_EntityType()
                        .Add_Customer_EntityType_With_Address()
                        .Add_CustomerOrders_Relationship()
                        .Add_Customer_EntityType_With_CollectionProperties()
                        .Add_Company_EntityType()
                        .Add_CustomerCompany_Relationship()
                        .Add_Employee_EntityType_With_HomeAddress()
                        .Add_CompanyEmployees_Relationship()
                        .Add_Customers_EntitySet()
                        .GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer));

        var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
        var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + filter, configuration, "OData");

        var options = new ODataQueryOptions(context, request);

        IEnumerable<Customer> customers = CustomerApplyTestData;
        // Act
        IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });


        // Assert
        Assert.NotNull(queryable);
        var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

        Assert.Equal(aggregation.Count(), actualCustomers.Count());

        var aggEnum = actualCustomers.GetEnumerator();

        foreach (var expected in aggregation)
        {
            aggEnum.MoveNext();
            var agg = aggEnum.Current;
            foreach (var key in expected.Keys)
            {
                object value = GetValue(agg, key);
                Assert.Equal(expected[key], value);
            }
        }
    }

    [Theory]
    [MemberData(nameof(AppliesWithReferencesOnGroupedOut))]
    public void ClausesWithGroupedOutReferences_Throw_ODataException(string clause)
    {
        // Arrange
        var model = new ODataModelBuilder()
                        .Add_Order_EntityType()
                        .Add_Customer_EntityType_With_Address()
                        .Add_CustomerOrders_Relationship()
                        .Add_Customer_EntityType_With_CollectionProperties()
                        .Add_Customers_EntitySet()
                        .GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer));

        var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
        var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + clause, configuration, "OData");

        var options = new ODataQueryOptions(context, request);

        IEnumerable<Customer> customers = CustomerApplyTestData;

        // Act & Assert
        ExceptionAssert.Throws<ODataException>(() =>
        {
            IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });
        });
    }

    [Theory]
    [MemberData(nameof(CustomerTestAppliesForPaging))]
    public void StableSortingAndPagingApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
    {
        // Arrange
        var model = new ODataModelBuilder()
                        .Add_Order_EntityType()
                        .Add_Customer_EntityType_With_Address()
                        .Add_CustomerOrders_Relationship()
                        .Add_Customer_EntityType_With_CollectionProperties()
                        .Add_Company_EntityType()
                        .Add_CustomerCompany_Relationship()
                        .Add_Employee_EntityType_With_HomeAddress()
                        .Add_CompanyEmployees_Relationship()
                        .Add_Customers_EntitySet()
                        .GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer));

        var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
        var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/?" + filter, configuration, "OData");

        var options = new ODataQueryOptions(context, request);

        IEnumerable<Customer> customers = CustomerApplyTestData;
        // Act
        IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True, PageSize = 2 });
        
        // Assert
        Assert.NotNull(queryable);
        var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

        Assert.Equal(aggregation.Count(), actualCustomers.Count());

        var aggEnum = actualCustomers.GetEnumerator();

        foreach (var expected in aggregation)
        {
            aggEnum.MoveNext();
            var agg = aggEnum.Current;
            foreach (var key in expected.Keys)
            {
                object value = GetValue(agg, key);
                Assert.Equal(expected[key], value);
            }
        }
    }
    */

    [Theory]
    [MemberData(nameof(CustomerTestFilters))]
    public void ApplyTo_Returns_Correct_Queryable_ForFilter(string filter, int[] customerIds)
    {
        // Arrange
        var model = new ODataModelBuilder()
                        .Add_Order_EntityType()
                        .Add_Customer_EntityType_With_Address()
                        .Add_CustomerOrders_Relationship()
                        .Add_Customer_EntityType_With_CollectionProperties()
                        .Add_Company_EntityType()
                        .Add_CustomerCompany_Relationship()
                        .Add_Employee_EntityType_With_HomeAddress()
                        .Add_CompanyEmployees_Relationship()
                        .Add_Customers_EntitySet()
                        .GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockServiceProvider() };
        var queryOptionParser = new ODataQueryOptionParser(
            context.Model,
            context.ElementType,
            context.NavigationSource,
            new Dictionary<string, string> { { "$apply", string.Format("filter({0})", filter) } });
        var filterOption = new ApplyQueryOption(string.Format("filter({0})", filter), context, queryOptionParser);
        IEnumerable<Customer> customers = CustomerApplyTestData;

        // Act
        IQueryable queryable = filterOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

        // Assert
        Assert.NotNull(queryable);
        IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
        Assert.Equal(
            customerIds,
            actualCustomers.Select(customer => customer.Id));
    }

    //[Fact]
    //public async Task ApplyToSerializationWorks()
    //{
    //    // Arrange
    //    var model = new ODataModelBuilder()
    //                    .Add_Order_EntityType()
    //                    .Add_Customer_EntityType_With_Address()
    //                    .Add_CustomerOrders_Relationship()
    //                    .Add_Customer_EntityType_With_CollectionProperties()
    //                    .Add_Customers_EntitySet()
    //                    .GetEdmModel();

    //    var controllers = new[] { typeof(MetadataController), typeof(CustomersController) };
    //    var server = TestServerFactory.Create(controllers, (config) =>
    //    {
    //        config.MapODataServiceRoute("odata", "odata", model);
    //    });

    //    HttpClient client = TestServerFactory.CreateClient(server);

    //    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
    //        "http://localhost/odata/Customers?$apply=groupby((Name), aggregate(Id with sum as TotalId))");

    //    // Act
    //    HttpResponseMessage response = await client.SendAsync(request);

    //    // Assert
    //    Assert.True(response.IsSuccessStatusCode);
    //    Assert.NotNull(response);
    //    var result = await response.Content.ReadAsObject<JObject>();
    //    var results = result["value"] as JArray;
    //    Assert.Equal(3, results.Count);
    //    Assert.Equal("10", results[0]["TotalId"].ToString());
    //    Assert.Equal("Lowest", results[0]["Name"].ToString());
    //    Assert.Equal("2", results[1]["TotalId"].ToString());
    //    Assert.Equal("Highest", results[1]["Name"].ToString());
    //    Assert.Equal("3", results[2]["TotalId"].ToString());
    //    Assert.Equal("Middle", results[2]["Name"].ToString());
    //}

    //[Fact]
    //public async Task ApplyToSerializationWorksForCompelxTypes()
    //{
    //    // Arrange
    //    var model = new ODataModelBuilder()
    //                    .Add_Order_EntityType()
    //                    .Add_Customer_EntityType_With_Address()
    //                    .Add_CustomerOrders_Relationship()
    //                    .Add_Customer_EntityType_With_CollectionProperties()
    //                    .Add_Customers_EntitySet()
    //                    .GetEdmModel();

    //    var controllers = new[] { typeof(MetadataController), typeof(CustomersController) };
    //    var server = TestServerFactory.Create(controllers, (config) =>
    //    {
    //        config.MapODataServiceRoute("odata", "odata", model);
    //    });

    //    HttpClient client = TestServerFactory.CreateClient(server);

    //    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
    //        "http://localhost/odata/Customers?$apply=groupby((Address/City), aggregate(Id with sum as TotalId))");

    //    // Act
    //    HttpResponseMessage response = await client.SendAsync(request);

    //    // Assert
    //    Assert.True(response.IsSuccessStatusCode);
    //    Assert.NotNull(response);
    //    var result = await response.Content.ReadAsObject<JObject>();
    //    var results = result["value"] as JArray;
    //    Assert.Equal(4, results.Count);
    //    Assert.Equal("6", results[0]["TotalId"].ToString());
    //    var address0 = results[0]["Address"] as JObject;
    //    Assert.Equal("redmond", address0["City"].ToString());
    //}

    [Fact]
    public void SortOnNestedDynamicPropertyWorks()
    {
        // Arrange
        var modelBuilder = new ODataModelBuilder();

        var addressType = modelBuilder.ComplexType<Address>();
        addressType.HasDynamicProperties(a => a.DynamicProperties);

        var customerType = modelBuilder.EntityType<Customer>();
        customerType.ComplexProperty(c => c.Address);

        var model = modelBuilder.GetEdmModel();

        var context = new ODataQueryContext(model, typeof(Customer));

        var request = RequestFactory.Create("Get", "http://localhost/?$apply=groupby((Address/DynamicCity))&$orderby=Address/DynamicCity");

        var options = new ODataQueryOptions(context, request);

        Customer[] customers =
        {
                new Customer
                {
                    Address = new Address { DynamicProperties = new Dictionary<string, object> { { "DynamicCity", "City 2" } } },
                },
                new Customer
                {
                    Address = new Address { DynamicProperties = new Dictionary<string, object> { { "DynamicCity", "City 1" } } },
                },
                new Customer
                {
                    Address = new Address { DynamicProperties = new Dictionary<string, object> { { "DynamicCity", "City 2" } } },
                },
                new Customer
                {
                    Address = new Address { DynamicProperties = new Dictionary<string, object> { { "DynamicCity", "City 1" } } },
                }
            };

        // Act
        IQueryable queryable = options.ApplyTo(customers.AsQueryable());

        // Assert
        Dictionary<string, object>[] expectedGroups =
        {
                new Dictionary<string, object> { { "Address/DynamicCity", "City 1" } },
                new Dictionary<string, object> { { "Address/DynamicCity", "City 2" } }
            };
        var actualGroups = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();
        Assert.Equal(expectedGroups.Length, actualGroups.Count);

        var aggEnum = actualGroups.GetEnumerator();
        foreach (var expected in expectedGroups)
        {
            aggEnum.MoveNext();
            var agg = aggEnum.Current;
            foreach (var key in expected.Keys)
            {
                object value = GetValue(agg, key);
                Assert.Equal(expected[key], value);
            }
        }
    }

    public static List<Product> ProductApplyForTypeCastTestData
    {
        get
        {
            List<Product> productList = new List<Product>();

            Product p1 = new DerivedProduct
            {
                ProductID = 1,
                ProductName = "Product 1",
                DerivedProductName = "Product A",
                Category = new DerivedCategory
                {
                    CategoryID = 1,
                    CategoryName = "Category 1",
                    DerivedCategoryName = "Category A",
                },
            };
            productList.Add(p1);
            Product p2 = new DerivedProduct
            {
                ProductID = 2,
                ProductName = "Product 1",
                DerivedProductName = "Product A",
                Category = new DerivedCategory
                {
                    CategoryID = 1,
                    CategoryName = "Category 1",
                    DerivedCategoryName = "Category A",
                },
            };
            productList.Add(p2);
            Product p3 = new DerivedProduct
            {
                ProductID = 3,
                ProductName = "Product 2",
                DerivedProductName = "Product B",
                Category = new DerivedCategory
                {
                    CategoryID = 2,
                    CategoryName = "Category 2",
                    DerivedCategoryName = "Category B",
                },
            };
            productList.Add(p3);

            return productList;
        }
    }

    public static TheoryDataSet<string, List<Dictionary<string, object>>> ProductTestAppliesForTypeCast
    {
        get
        {
            return new TheoryDataSet<string, List<Dictionary<string, object>>>
                {
                    {
                        "groupby((Microsoft.AspNetCore.OData.Tests.Models.Product/ProductName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { {"ProductName", "Product 1"} },
                            new Dictionary<string, object> { {"ProductName", "Product 2"} }
                        }
                    },
                    {
                        "groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/DerivedProductName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "DerivedProductName", "Product A"} },
                            new Dictionary<string, object> { { "DerivedProductName", "Product B"} }
                        }
                    },
                    {
                        "groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/CategoryName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Category/CategoryName", "Category 1" } },
                            new Dictionary<string, object> { { "Category/CategoryName", "Category 2" } }
                        }
                    },
                    {
                        "groupby((Category/Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/DerivedCategoryName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Category/DerivedCategoryName", "Category A" } },
                            new Dictionary<string, object> { { "Category/DerivedCategoryName", "Category B" } }
                        }
                    },
                    {
                        "groupby((Microsoft.AspNetCore.OData.Tests.Models.DerivedProduct/Category/Microsoft.AspNetCore.OData.Tests.Models.DerivedCategory/DerivedCategoryName))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Category/DerivedCategoryName", "Category A" } },
                            new Dictionary<string, object> { { "Category/DerivedCategoryName", "Category B" } }
                        }
                    }
                };
        }
    }

    [Theory]
    [MemberData(nameof(ProductTestAppliesForTypeCast))]
    public void ApplyTo_Returns_Correct_Queryable_ForTypeCast(string apply, List<Dictionary<string, object>> aggregation)
    {
        // Arrange
        var model = new ODataModelBuilder()
                        .Add_Products_EntityType()
                        .Add_DerivedProducts_EntityType()
                        .Add_Categories_EntityType()
                        .Add_DerivedCategories_EntityType()
                        .Add_Products_EntitySet()
                        .GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Product)) { RequestContainer = new MockServiceProvider() };
        var queryOptionParser = new ODataQueryOptionParser(
            context.Model,
            context.ElementType,
            context.NavigationSource,
            new Dictionary<string, string> { { "$apply", apply } });
        var applyOption = new ApplyQueryOption(apply, context, queryOptionParser);
        IEnumerable<Product> products = ProductApplyForTypeCastTestData;

        // Act
        IQueryable queryable = applyOption.ApplyTo(products.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

        // Assert
        Assert.NotNull(queryable);
        var actualProducts = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

        Assert.Equal(aggregation.Count(), actualProducts.Count());

        var aggEnum = actualProducts.GetEnumerator();

        foreach (var expected in aggregation)
        {
            aggEnum.MoveNext();
            var agg = aggEnum.Current;
            foreach (var key in expected.Keys)
            {
                object value = GetValue(agg, key);
                Assert.Equal(expected[key], value);
            }
        }
    }

    [Fact]
    public void DynamicTypeWrapperBindings_Must_Have_Correct_ReflectedType()
    {
        var filter = "groupby((Name), aggregate(CustomerId with sum as Total))";
        // Arrange
        var model = new ODataModelBuilder().Add_Customer_EntityType_With_Address().GetEdmModel();
        var context = new ODataQueryContext(model, typeof(Customer)) { };
        var queryOptionParser = new ODataQueryOptionParser(
            context.Model,
            context.ElementType,
            context.NavigationSource,
            new Dictionary<string, string> { { "$apply", filter } });
        var applyOption = new ApplyQueryOption(filter, context, queryOptionParser);
        IEnumerable<Customer> customers = CustomerApplyTestData;

        // Act
        IQueryable queryable = applyOption.ApplyTo(customers.AsQueryable(), new OData.Query.ODataQuerySettings());
        new DynamicTypeWrapperExpressionVisitor().Visit(queryable.Expression);
    }

    private class DynamicTypeWrapperExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (typeof(DynamicTypeWrapper).IsAssignableFrom(node.Type))
            {
                foreach (var binding in node.Bindings)
                {
                    Assert.Equal(node.Type, binding.Member.ReflectedType);
                }
            }

            return base.VisitMemberInit(node);
        }
    }

    private object GetValue(DynamicTypeWrapper wrapper, string path)
    {
        var parts = path.Split('/');
        foreach (var part in parts)
        {
            object value;
            wrapper.TryGetPropertyValue(part, out value);
            wrapper = value as DynamicTypeWrapper;
            if (wrapper == null)
            {
                return value;
            }
        }

        Assert.False(true, "Property " + path + " not found");
        return null;
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<ApplyCustomer>("Customers");
        return builder.GetEdmModel();
    }

    private class ApplyCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
    }
}

public class CustomersController : ODataController
{
    private List<Customer> _customers;

    public CustomersController()
    {
        _customers = ApplyQueryOptionTest.CustomerApplyTestData;
    }

    [EnableQuery]
    public IQueryable<Customer> Get()
    {
        return _customers.AsQueryable();
    }
}