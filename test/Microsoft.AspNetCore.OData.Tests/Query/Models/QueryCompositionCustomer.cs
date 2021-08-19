//-----------------------------------------------------------------------------
// <copyright file="QueryCompositionCustomer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Tests.Query.Models
{
    public class QueryCompositionCustomerBase
    {
        public int Id { get; set; }
    }

    public class QueryCompositionCustomer : QueryCompositionCustomerBase
    {
        public string Name { get; set; }
        public QueryCompositionAddress Address { get; set; }
        public QueryCompositionCustomer RelationshipManager { get; set; }
        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<QueryCompositionCustomer> Contacts { get; set; }
        public byte[] Image { get; set; }
        public DateTimeOffset Birthday { get; set; }
        public double AmountSpent { get; set; }
        public Color FavoriteColor { get; set; }


        public QueryCompositionAddress NavigationWithNotFilterableProperty { get; set; }
        [NotFilterable]
        public QueryCompositionCustomer NotFilterableNavigationProperty { get; set; }
        [NotFilterable]
        public string NotFilterableProperty { get; set; }
        [NotSortable]
        public string NotSortableProperty { get; set; }
        [NonFilterable]
        public QueryCompositionCustomer NonFilterableNavigationProperty { get; set; }
        [NonFilterable]
        public string NonFilterableProperty { get; set; }
        [Unsortable]
        public string UnsortableProperty { get; set; }
    }
}
