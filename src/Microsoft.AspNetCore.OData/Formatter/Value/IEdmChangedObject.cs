//-----------------------------------------------------------------------------
// <copyright file="IEdmChangedObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmChangedObject"/>.
    /// Base interface to be implemented by any Delta object required to be part of the DeltaResourceSet Payload.
    /// </summary>
    public interface IEdmChangedObject : IEdmObject
    {
        /// <summary>
        /// DeltaKind for the objects part of the DeltaResourceSet Payload.
        /// Used to determine which Delta object to create during serialization.
        /// </summary>
        DeltaItemKind Kind { get; }
    }
}
