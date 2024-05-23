//-----------------------------------------------------------------------------
// <copyright file="IDeltaSetItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// The delta set item base.
    /// </summary>
    public interface IDeltaSetItem
    {
        /// <summary>
        /// Gets the delta item kind.
        /// </summary>
        DeltaItemKind Kind { get; }

        /// <summary>
        /// Gets or sets the annotation container to hold transient instance annotations.
        /// </summary>
        IODataInstanceAnnotationContainer TransientInstanceAnnotationContainer { get; set; }
    }
}
