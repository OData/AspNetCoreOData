// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace ODataRoutingSample.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Color FavoriteColor { get; set; }

        public virtual Address HomeAddress { get; set; }

        public virtual IList<Address> FavoriteAddresses { get; set; }
    }

    public class VipCustomer : Customer
    {
        public IList<string> Emails { get; set; }
    }

    public class Organization
    {
        public int OrganizationId { get; set; }
    }
}
