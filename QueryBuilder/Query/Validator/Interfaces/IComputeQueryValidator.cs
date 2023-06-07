namespace QueryBuilder.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="ComputeQueryOption"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface IComputeQueryValidator
    {
        /// <summary>
        /// Validates the OData query.
        /// </summary>
        /// <param name="computeQueryOption">The $compute query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings);
    }
}
