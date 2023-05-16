namespace QueryBuilder.Deltas
{
    /// <summary>
    /// The delta set item base.
    /// </summary>
    public interface IDeltaSetItem
    {
        /// <summary>
        /// Gets the delta item kind.
        /// </summary>
        DeltaItemKind Kind { get; }
    }
}
