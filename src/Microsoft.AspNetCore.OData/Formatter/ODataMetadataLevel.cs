//-----------------------------------------------------------------------------
// <copyright file="ODataMetadataLevel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Formatter;

/// <summary>
/// The amount of metadata information to serialize in an OData response.
/// </summary>
public enum ODataMetadataLevel
{
    /// <summary>
    /// JSON minimal metadata.
    /// </summary>
    Minimal = 0,

    /// <summary>
    /// JSON full metadata.
    /// </summary>
    Full = 1,

    /// <summary>
    /// JSON none metadata.
    /// </summary>
    None = 2
}
