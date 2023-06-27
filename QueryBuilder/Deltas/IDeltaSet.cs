using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ODataQueryBuilder.Deltas
{
    /// <summary>
    /// The interface for a delta resource set.
    /// </summary>
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix.", Justification = "The set suffix is correct.")]
    public interface IDeltaSet : ICollection<IDeltaSetItem>
    {
    }
}
