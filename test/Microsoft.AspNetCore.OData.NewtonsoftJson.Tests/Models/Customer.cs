//-----------------------------------------------------------------------------
// <copyright file="Customer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.NewtonsoftJson.Tests.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Address Location { get; set; }

        public Order[] Orders { get; set; }
    }
}
