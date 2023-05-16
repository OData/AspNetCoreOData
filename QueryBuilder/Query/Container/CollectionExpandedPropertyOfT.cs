using System.Collections.Generic;

namespace QueryBuilder.Query.Container
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
