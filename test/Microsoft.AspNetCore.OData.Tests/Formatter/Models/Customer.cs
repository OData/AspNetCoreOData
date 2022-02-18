//-----------------------------------------------------------------------------
// <copyright file="Customer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Models
{
    public class Customer
    {
        public Customer()
        {
            this.Orders = new List<Order>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public IList<Order> Orders { get; set; }
        public SimpleEnum SimpleEnum { get; set; }
        public Address HomeAddress { get; set; }
    }

    public class SpecialCustomer : Customer
    {
        public int Level { get; set; }
        public DateTimeOffset Birthday { get; set; }
        public decimal Bonus { get; set; }
    }
}
