//-----------------------------------------------------------------------------
// <copyright file="Customer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ODataRoutingSample.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Color FavoriteColor { get; set; }

        public int Amount { get; set; }

        public virtual Address HomeAddress { get; set; }

        public virtual IList<Address> FavoriteAddresses { get; set; }
    }

    public class VipCustomer : Customer
    {
        public IList<string> Emails { get; set; }
    }
}
