//-----------------------------------------------------------------------------
// <copyright file="IEdmStructuredObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmStructuredType"/>.
    /// </summary>
    public interface IEdmStructuredObject : IEdmObject
    {
        /// <summary>
        /// Gets the value of the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="value">When this method returns, contains the value of the property with the given name, if the property is found;
        /// otherwise, null. The parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the instance contains the property with the given name; otherwise, <c>false</c>.</returns>
        bool TryGetPropertyValue(string propertyName, out object value);

        /// <summary>
        /// Gets the value of the property with the given name and edm type.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="edmType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetPropertyValue(string propertyName, IEdmTypeReference edmType, out object value);
    }
}
