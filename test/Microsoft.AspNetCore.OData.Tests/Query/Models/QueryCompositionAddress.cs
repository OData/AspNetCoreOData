// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Tests.Query.Models
{
    public class QueryCompositionAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }

        [NotFilterable]
        public string NotFilterableProperty { get; set; }
        [NonFilterable]
        public string NonFilterableProperty { get; set; }
    }
}
