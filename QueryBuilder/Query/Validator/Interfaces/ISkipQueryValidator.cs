namespace ODataQueryBuilder.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="SkipQueryOption"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface ISkipQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="SkipQueryOption" />.
        /// </summary>
        /// <param name="skipQueryOption">The $skip query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(SkipQueryOption skipQueryOption, ODataValidationSettings validationSettings);
    }
}
