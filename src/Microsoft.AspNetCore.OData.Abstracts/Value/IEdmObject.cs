// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatting.Value
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmType"/>.
    /// </summary>
    public interface IEdmObject
    {
        /// <summary>
        /// Gets the <see cref="IEdmTypeReference"/> of this instance.
        /// </summary>
        /// <returns>The <see cref="IEdmTypeReference"/> of this instance.</returns>
        IEdmTypeReference GetEdmType();
    }
}
