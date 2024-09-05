//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinkWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper;

/// <summary>
/// Encapsulates an <see cref="ODataEntityReferenceLink"/>.
/// </summary>
public class ODataEntityReferenceLinkWrapper : ODataItemWrapper
{
    /// <summary>
    /// Initializes a new instance of <see cref="ODataEntityReferenceLinkWrapper"/>.
    /// </summary>
    /// <param name="link">The wrapped entity reference item.</param>
    public ODataEntityReferenceLinkWrapper(ODataEntityReferenceLink link)
    {
        EntityReferenceLink = link;
    }

    /// <summary>
    /// Gets the wrapped <see cref="ODataEntityReferenceLink"/>.
    /// </summary>
    public ODataEntityReferenceLink EntityReferenceLink { get; }
}
