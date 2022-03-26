//-----------------------------------------------------------------------------
// <copyright file="Person.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataAlternateKeySample.Models
{
    /// <summary>
    /// Entity type with composed alternate keys
    /// </summary>
    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CountryOrRegion { get; set; }

        public string Passport { get; set; }
    }
}
