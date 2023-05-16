using System;

namespace QueryBuilder.Deltas
{
    /// <summary>
    /// The typed delta.
    /// </summary>
    public interface ITypedDelta
    {
        /// <summary>
        /// Gets the actual type of the structural object for which the changes are tracked.
        /// </summary>
        Type StructuredType { get; }

        /// <summary>
        /// Gets the expected type of the entity for which the changes are tracked.
        /// </summary>
        Type ExpectedClrType { get; }
    }
}
