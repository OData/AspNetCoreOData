using Microsoft.OData.Edm;

namespace QueryBuilder.Query.Validator
{
    /// <summary>
    /// The base for validator context.
    /// </summary>
    public abstract class QueryValidatorContext
    {
        /// <summary>
        /// The Query context.
        /// </summary>
        public ODataQueryContext2 Context { get; set; }

        /// <summary>
        /// The Query validation settings.
        /// </summary>
        public ODataValidationSettings ValidationSettings { get; set; }

        /// <summary>
        /// The applied property, It could be null.
        /// </summary>
        public IEdmProperty Property { get; set; }

        /// <summary>
        /// The applied strutured type.
        /// </summary>
        public IEdmStructuredType StructuredType { get; set; }

        /// <summary>
        /// The current depth.
        /// </summary>
        public int CurrentDepth { get; set; }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model => Context.Model;
    }
}
