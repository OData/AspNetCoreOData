//-----------------------------------------------------------------------------
// <copyright file="CollectionExpandedPropertyOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
            if (Collection == null)
            {
                return null;
            }

            return new TruncatedCollection<T>(Collection, PageSize, TotalCount);
        }
    }
}
