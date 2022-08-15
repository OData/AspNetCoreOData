//-----------------------------------------------------------------------------
// <copyright file="Person.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations.Schema;

namespace ODataRoutingSample.Models
{
    public class Person
    {
        // [Column(Order = 1)] // This attribute can be used with [Key] convention model building
                               // It is ignored if the property is added explicitly.
        public string FirstName { get; set; }

        // [Column(Order = 2)]
        public string LastName { get; set; }
    }
}
