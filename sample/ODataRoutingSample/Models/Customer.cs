//-----------------------------------------------------------------------------
// <copyright file="Customer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

        public virtual Foo Foo { get; set; }
    }

    public class Foo
    {
        [NotMapped]
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    public class VipCustomer : Customer
    {
        public IList<string> Emails { get; set; }
    }
}
