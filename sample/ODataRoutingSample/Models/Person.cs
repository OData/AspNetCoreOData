//-----------------------------------------------------------------------------
// <copyright file="Person.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ODataRoutingSample.Models;

public class Person
{
    // [Column(Order = 1)] // This attribute can be used with [Key] convention model building
                           // It is ignored if the property is added explicitly.
    public string FirstName { get; set; }

    // [Column(Order = 2)]
    public string LastName { get; set; }

    public object Data { get; set; } // Edm.Untyped

    public object Other { get; set; }

    public IList<object> Infos { get; set; } // Collection(Edm.Untyped)

    public IList<object> Sources { get; set; } // Collection(Edm.Untyped)

    public PersonExtraInfo CustomProperties { get; set; }
}

public class PersonExtraInfo
{
    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
}

public enum AnyEnum
{
    E1
}
