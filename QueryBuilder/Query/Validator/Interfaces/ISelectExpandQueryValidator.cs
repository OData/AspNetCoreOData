namespace QueryBuilder.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="SelectExpandQueryOption"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface ISelectExpandQueryValidator
    {
        /// <summary>
        /// Validates the OData query.
        /// </summary>
        /// <param name="selectExpandQueryOption">The $select and $expand query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings);
    }
}
