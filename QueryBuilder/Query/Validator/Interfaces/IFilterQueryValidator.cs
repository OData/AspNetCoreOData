namespace ODataQueryBuilder.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="FilterQueryOption"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface IFilterQueryValidator
    {
        /// <summary>
        /// Validates the OData query.
        /// </summary>
        /// <param name="filterQueryOption">The $filter query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings validationSettings);
    }
}
