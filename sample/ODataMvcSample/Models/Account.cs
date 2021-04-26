// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace ODataMvcSample.Models
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
