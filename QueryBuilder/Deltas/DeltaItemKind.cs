namespace QueryBuilder.Deltas
{
    /// <summary>
    /// The Kind of the object within the DeltaPayload used to distinguish between
    /// Resource/DeletedResource/DeltaDeletedLink/AddedLink.
    /// </summary>
    public enum DeltaItemKind
    {
        /// <summary>
        /// Corresponds to EdmEntityObject (Equivalent of ODataResource in ODL).
        /// </summary>
        Resource = 0,

        /// <summary>
        /// Corresponds to EdmDeltaDeletedResourceObject (Equivalent of ODataDeletedResource in ODL).
        /// </summary>
        DeletedResource = 1,

        /// <summary>
        /// Corresponds to EdmDeltaDeletedLink (Equivalent of ODataDeltaDeletedLink in ODL).
        /// </summary>
        DeltaDeletedLink = 2,

        /// <summary>
        /// Corresponds to EdmDeltaLink (Equivalent of ODataDeltaLink in ODL).
        /// </summary>
        DeltaLink = 3,

        /// <summary>
        /// Corresponds to any Unknown item added.
        /// </summary>
        Unknown = 4
    }
}
