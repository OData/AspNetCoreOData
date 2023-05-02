//-----------------------------------------------------------------------------
// <copyright file="EdmUntypedObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an <see cref="IEdmUntypedObject"/> with no backing Edm type, or its Edm.Untyped.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmUntypedObject : Dictionary<string, object>, IEdmUntypedObject
    {
        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
            => EdmUntypedStructuredTypeReference.NullableTypeReference;

        /// <inheritdoc/>
        public bool TryGetPropertyValue(string propertyName, out object value)
            => TryGetValue(propertyName, out value);
    }
}
