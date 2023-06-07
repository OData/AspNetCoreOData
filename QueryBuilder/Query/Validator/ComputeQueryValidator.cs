namespace QueryBuilder.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="ComputeQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class ComputeQueryValidator : IComputeQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="ComputeQueryOption" />.
        /// </summary>
        /// <param name="computeQueryOption">The $compute query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings)
        {
            if (computeQueryOption == null)
            {
                throw Error.ArgumentNull(nameof(computeQueryOption));
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull(nameof(validationSettings));
            }

            // so far, we don't have validation rules here for $compute
            // because 'DefaultQuerySetting' doesn't have configuration for $compute
            // we can only let ODL to parse and verify the compute clause,
            // however, developer can override this method add his own rules
            _ = computeQueryOption.ComputeClause;
        }
    }
}
