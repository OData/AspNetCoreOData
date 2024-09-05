//-----------------------------------------------------------------------------
// <copyright file="ODataDeltaLinkWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper;

/// <summary>
/// Encapsulates an <see cref="ODataDeltaLink"/> added link.
/// </summary>
public sealed class ODataDeltaLinkWrapper : ODataDeltaLinkBaseWrapper
{
    /// <summary>
    /// Initializes a new instance of <see cref="ODataDeltaLinkWrapper"/>.
    /// </summary>
    /// <param name="deltaLink">The wrapped added link item.</param>
    public ODataDeltaLinkWrapper(ODataDeltaLink deltaLink)
    {
        DeltaLink = deltaLink;
    }

    /// <summary>
    /// Gets the wrapped <see cref="ODataDeltaLink"/>.
    /// </summary>
    public ODataDeltaLink DeltaLink { get; }
}
