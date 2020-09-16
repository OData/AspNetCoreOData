// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Tests.Query.Models
{
    public class QueryCompositionCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QueryCompositionAddress[] Locations { get; set; }
        public int[] AlternateIds { get; set; }
    }
}
