//-----------------------------------------------------------------------------
// <copyright file="Customer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataAlternateKeySample.Models
{
    /// <summary>
    /// Entity type with one alternate key
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string SSN { get; set; }
    }
}
