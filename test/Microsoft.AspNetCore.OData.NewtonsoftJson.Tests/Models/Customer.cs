// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
