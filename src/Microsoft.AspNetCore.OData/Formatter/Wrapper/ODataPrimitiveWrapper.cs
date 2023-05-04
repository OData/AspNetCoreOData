//-----------------------------------------------------------------------------
// <copyright file="ODataPrimitiveWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates <see cref="ODataPrimitiveValue"/> in an Untyped (declared or undeclared) collection.
    /// </summary>
    public class ODataPrimitiveWrapper : ODataItemWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataPrimitiveWrapper"/>.
        /// </summary>
        /// <param name="value">The wrapped primitive value.</param>
        public ODataPrimitiveWrapper(ODataPrimitiveValue value)
        {
            Value = value ?? throw Error.ArgumentNull(nameof(value));
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataPrimitiveValue"/>.
        /// </summary>
        public ODataPrimitiveValue Value { get; }
    }
}
