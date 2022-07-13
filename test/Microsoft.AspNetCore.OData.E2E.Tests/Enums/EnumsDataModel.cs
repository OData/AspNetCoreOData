//-----------------------------------------------------------------------------
// <copyright file="EnumsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Enums
{
    public class Employee
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public List<Skill> SkillSet { get; set; }

        public Gender Gender { get; set; }

        public AccessLevel AccessLevel { get; set; }

        public FavoriteSports FavoriteSports { get; set; }
    }

    public class GdebruinEntity
    {
        public GdebruinKey ID { get; set; }

        public string Data { get; set; }
    }

    public enum GdebruinKey
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
    }

    [Flags]
    public enum AccessLevel
    {
        Read = 1,

        Write = 2,

        Execute = 4
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
}
