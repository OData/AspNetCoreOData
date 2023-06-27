using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ODataQueryBuilder.Query.Container
{
    /// <summary>
    /// Represents a collection that is truncated to a given page size.
    /// </summary>
    [SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "<Pending>")]
    public interface ITruncatedCollection : IEnumerable
    {
        /// <summary>
        /// Gets the page size the collection is truncated to.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Gets a value representing if the collection is truncated or not.
        /// </summary>
        bool IsTruncated { get; }
    }
}
