using System.Collections.Generic;

namespace QueryBuilder.Common
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
