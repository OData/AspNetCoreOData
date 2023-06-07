namespace QueryBuilder.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="ODataQueryOptions"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface IODataQueryValidator
    {
        /// <summary>
        /// Validates the OData query.
        /// </summary>
        /// <param name="options">The OData query options to validate.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(ODataQueryOptions options, ODataValidationSettings validationSettings);
    }
}
