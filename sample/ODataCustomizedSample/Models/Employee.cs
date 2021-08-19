//-----------------------------------------------------------------------------
// <copyright file="Employee.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ODataCustomizedSample.Models
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
}
