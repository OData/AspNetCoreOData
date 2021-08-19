//-----------------------------------------------------------------------------
// <copyright file="IDeltaLinkBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// <see cref="IDelta" /> allows and tracks changes to delta link.
    /// </summary>
    internal interface IDeltaLinkBase : IDeltaSetItem
    {
        /// <summary>
        /// The Uri of the entity from which the relationship is defined, which may be absolute or relative.
        /// </summary>
        Uri Source { get; set; }

        /// <summary>
        /// The Uri of the related entity, which may be absolute or relative.
        /// </summary>
        Uri Target { get; set; }

        /// <summary>
        /// The name of the relationship property on the parent object.
        /// </summary>
        string Relationship { get; set; }
    }
}
