using System.Collections;

namespace ODataQueryBuilder.Query
{
    /// <summary>
    /// Represents a collection that has total count.
    /// </summary>
    internal interface ICountOptionCollection : IEnumerable
    {
        /// <summary>
        /// Gets a value representing the total count of the collection.
        /// </summary>
        long? TotalCount { get; }
    }
}
