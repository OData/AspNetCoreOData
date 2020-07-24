// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Query.Container
{
    internal class CollectionExpandedProperty<T> : NamedProperty<T>
    {
        public int PageSize { get; set; }

        public long? TotalCount { get; set; }

        public IEnumerable<T> Collection { get; set; }

        public override object GetValue()
        {
            if (TotalCount == null)
            {
                return new TruncatedCollection<T>(Collection, PageSize);
            }
            else
            {
                return new TruncatedCollection<T>(Collection, PageSize, TotalCount);
            }
        }
    }
}
