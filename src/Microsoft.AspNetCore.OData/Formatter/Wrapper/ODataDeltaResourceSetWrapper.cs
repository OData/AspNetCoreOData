//-----------------------------------------------------------------------------
// <copyright file="ODataDeltaResourceSetWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper;

/// <summary>
/// Encapsulates an <see cref="ODataDeltaResourceSet"/>.
/// </summary>
/// <remarks>
/// The Delta resource set could have normal resource or the deleted resource.
/// The Delta resource set could have delta link.
/// </remarks>
public sealed class ODataDeltaResourceSetWrapper : ODataResourceSetBaseWrapper
{
    /// <summary>
    /// Initializes a new instance of <see cref="ODataDeltaResourceSetWrapper"/>.
    /// </summary>
    /// <param name="deltaResourceSet">The wrapped delta resource set item.</param>
    public ODataDeltaResourceSetWrapper(ODataDeltaResourceSet deltaResourceSet)
    {
        DeltaResourceSet = deltaResourceSet;
        DeltaItems = new List<ODataItemWrapper>();
    }

    /// <summary>
    /// Gets the wrapped <see cref="ODataDeltaResourceSet"/>.
    /// </summary>
    public ODataDeltaResourceSet DeltaResourceSet { get; }

    /// <summary>
    /// Gets the nested delta items (resource, or deleted resource, or deleted link, or added link).
    /// Be noted: the order of the delta items matters.
    /// </summary>
    public IList<ODataItemWrapper> DeltaItems { get; }
}
