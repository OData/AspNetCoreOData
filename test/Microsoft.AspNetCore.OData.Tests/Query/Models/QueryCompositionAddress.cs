//-----------------------------------------------------------------------------
// <copyright file="QueryCompositionAddress.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
