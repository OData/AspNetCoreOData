//-----------------------------------------------------------------------------
// <copyright file="Account.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace ODataRoutingSample.Models
{
    public class Account
    {
        public Guid AccountId { get; set; }

        public string Name { get; set; }

        public Address HomeAddress { get; set; }

        public AccountInfo AccountInfo { get; set; }
    }

    public class AccountInfo
    {
        public int Id { get; set; }

        public double Balance { get; set; }
    }
}
