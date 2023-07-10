namespace ODataQueryBuilder.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="OrderByQueryOptionFundamentals"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface IOrderByQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="OrderByQueryOptionFundamentals" />.
        /// </summary>
        /// <param name="orderByOption">The $orderby query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(OrderByQueryOptionFundamentals orderByOption, ODataValidationSettings validationSettings);
    }
}
