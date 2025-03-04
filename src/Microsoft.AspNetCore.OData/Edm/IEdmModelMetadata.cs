//-----------------------------------------------------------------------------
// <copyright file="IEdmModelMetadata.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm;

/// <summary>
/// Represents the Edm model metadata used during OData query.
/// </summary>
public interface IEdmModelMetadata
{
    /// <summary>
    /// Gets the <see cref="IEdmModel"/>.
    /// </summary>
    IEdmModel Model { get; }
}
