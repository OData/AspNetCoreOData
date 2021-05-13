// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmChangedObject"/>.
    /// Base interface to be implemented by any Delta object required to be part of the DeltaResourceSet Payload.
    /// </summary>
    public interface IEdmChangedObject : IEdmStructuredObject
    {
        /// <summary>
        /// DeltaKind for the objects part of the DeltaResourceSet Payload.
        /// Used to determine which Delta object to create during serialization.
        /// </summary>
        DeltaKind DeltaKind { get; }
    }
}