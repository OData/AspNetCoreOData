using Microsoft.OData;
using ODataQueryBuilder.Deltas;
using System;

namespace ODataQueryBuilder.Deltas
{
    /// <summary>
    /// <see cref="IDeltaDeletedResource" /> allows and tracks changes to a deleted resource.
    /// </summary>
    public interface IDeltaDeletedResource : IDelta
    {
        /// <inheritdoc />
        Uri Id { get; set; }

        /// <inheritdoc />
        DeltaDeletedEntryReason? Reason { get; set; }
    }
}
