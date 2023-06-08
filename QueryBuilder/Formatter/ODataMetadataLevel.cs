namespace QueryBuilder.Formatter
{
    /// <summary>
    /// The amount of metadata information to serialize in an OData response.
    /// </summary>
    public enum ODataMetadataLevel
    {
        /// <summary>
        /// JSON minimal metadata.
        /// </summary>
        Minimal = 0,

        /// <summary>
        /// JSON full metadata.
        /// </summary>
        Full = 1,

        /// <summary>
        /// JSON none metadata.
        /// </summary>
        None = 2
    }
}
