//-----------------------------------------------------------------------------
// <copyright file="NullEdmComplexObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an <see cref="IEdmComplexObject"/> that is null.
    /// </summary>
    public class NullEdmComplexObject : IEdmComplexObject
    {
        private IEdmComplexTypeReference _edmType;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullEdmComplexObject"/> class.
        /// </summary>
        /// <param name="edmType">The EDM type of this object.</param>
        public NullEdmComplexObject(IEdmComplexTypeReference edmType)
        {
            _edmType = edmType ?? throw Error.ArgumentNull(nameof(edmType));
        }

        /// <inheritdoc/>
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            throw Error.InvalidOperation(SRResources.EdmComplexObjectNullRef, propertyName, _edmType.ToTraceString());
        }

        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
        {
            return _edmType;
        }

        public bool TryGetPropertyValue(string propertyName, IEdmTypeReference edmType, out object value)
        {
            throw new System.NotImplementedException();
        }
    }
}
