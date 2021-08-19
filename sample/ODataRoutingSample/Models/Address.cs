//-----------------------------------------------------------------------------
// <copyright file="Address.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataRoutingSample.Models
{
    public class Address
    {
        public string City { get; set; }

        public string Street { get; set; }
    }

    public class CnAddress : Address
    {
        public string Postcode { get; set; }
    }

    public class UsAddress : Address
    {
        public string Zipcode { get; set; }
    }
}
