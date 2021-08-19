//-----------------------------------------------------------------------------
// <copyright file="CollectionExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Common
{
    /// <summary>
    /// Helper extension methods for fast use of collections.
    /// </summary>
    internal static class CollectionExtensions
    {
        public static void MergeWithReplace<TKey, TValue>(this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source)
        {
            foreach (var kvp in source)
            {
                target[kvp.Key] = kvp.Value;
            }
        }
    }
}
