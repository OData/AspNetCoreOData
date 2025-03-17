//-----------------------------------------------------------------------------
// <copyright file="EnumsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Enums;

public class Employee
{
    public int ID { get; set; }

    public string Name { get; set; }

    public List<Skill> SkillSet { get; set; }

    public Gender Gender { get; set; }

    public AccessLevel AccessLevel { get; set; }

    public EmployeeType EmployeeType { get; set; }

    public FavoriteSports FavoriteSports { get; set; }
}

[Flags]
public enum AccessLevel
{
    None = 0,

    Read = 1,

    Write = 2,

    Execute = 4,

    Admin = 7 // Read | Write | Execute
}

[Flags]
[DataContract(Name = "employeeType")]
public enum EmployeeType
{
    [EnumMember(Value = "full time")]
    FullTime = 1,

    [EnumMember(Value = "Part Time")]
    PartTime = 2,

    [EnumMember(Value = "contract")]
    Contract = 4,

    [EnumMember(Value = "intern")]
    Intern = 8
}

public enum Gender
{
    Male = 1,

    Female = 2
}

public enum Skill
{
    CSharp,

    Sql,

    Web,
}

public enum Sport
{
    Pingpong,

    Basketball
}

public class FavoriteSports
{
    public Sport LikeMost { get; set; }
    public List<Sport> Like { get; set; }
}

[DataContract]
public enum Status
{
    [EnumMember(Value = "Sold out")]
    SoldOut,

    [EnumMember(Value = "In store")]
    InStore
}

public class WeatherForecast
{
    public int Id { get; set; }

    public Status Status { get; set; }

    public Skill Skill { get; set; }
}
