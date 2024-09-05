//-----------------------------------------------------------------------------
// <copyright file="IEdmObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value;

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
