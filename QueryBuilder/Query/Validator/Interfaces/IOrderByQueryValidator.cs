namespace ODataQueryBuilder.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="OrderByQueryOption"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface IOrderByQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="OrderByQueryOption" />.
        /// </summary>
        /// <param name="orderByOption">The $orderby query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(OrderByQueryOption orderByOption, ODataValidationSettings validationSettings);
    }
}
