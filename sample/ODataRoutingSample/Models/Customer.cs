//-----------------------------------------------------------------------------
// <copyright file="Customer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.ModelBuilder;
using System.Collections.Generic;

namespace ODataRoutingSample.Models
{
    public class TestPolicy
    {
        [Contained]
        public virtual IList<TestAuthMethodConfiguration> AuthMethodConfigurations { get; set; } = new List<TestAuthMethodConfiguration>();
    }

    public class TestAuthMethodConfiguration
    {
        public string Id { get; set; }

        [Contained]
        public virtual IList<PassKeyProfile> PassKeyProfiles { get; set; } //// /policies/authmethodconfigurations/{id1}/passkeyprofiles/{id2}
    }

    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Color FavoriteColor { get; set; }

        public int Amount { get; set; }

        public virtual Address HomeAddress { get; set; }

        public virtual IList<Address> FavoriteAddresses { get; set; }

        public virtual IList<PassKeyProfile> PassKeyProfiles { get; set; } = new List<PassKeyProfile>();
    }

    public class PassKeyProfile
    {
        public int Id { get; set; }

        public string Value { get; set; }
    }

    public class VipCustomer : Customer
    {
        public IList<string> Emails { get; set; }
    }
}
