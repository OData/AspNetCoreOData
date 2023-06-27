using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using ODataQueryBuilder.Abstracts;

namespace ODataQueryBuilder.Formatter
{
    /// <summary>
    /// ActionPayload holds the Parameter names and values provided by a client in a POST request
    /// to invoke a particular Action. The Parameter values are stored in the dictionary keyed using the Parameter name.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "ODataUntypedActionParameters is more appropriate here.")]
    // TODO: Fix attribute compilation below:
    //[NonValidatingParameterBinding]
    public class ODataUntypedActionParameters : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataUntypedActionParameters"/> class.
        /// </summary>
        /// <param name="action">The OData action of this parameters.</param>
        public ODataUntypedActionParameters(IEdmAction action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Gets the OData action of this parameters.
        /// </summary>
        public IEdmAction Action { get; }
    }
}
