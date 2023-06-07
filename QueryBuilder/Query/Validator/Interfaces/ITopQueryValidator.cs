namespace QueryBuilder.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="TopQueryOption"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface ITopQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="TopQueryOption" />.
        /// </summary>
        /// <param name="topQueryOption">The $top query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(TopQueryOption topQueryOption, ODataValidationSettings validationSettings);
    }
}
