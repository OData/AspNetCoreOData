// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Implementing IEdmType to identify objects which are part of DeltaResourceSet Payload.
    /// </summary>
    internal class EdmDeltaType : IEdmType
    {
        internal EdmDeltaType(IEdmEntityType entityType, DeltaItemKind deltaKind)
        {
            EntityType = entityType ?? throw Error.ArgumentNull(nameof(entityType));
            DeltaKind = deltaKind;
        }

        /// <inheritdoc />
        public EdmTypeKind TypeKind => EdmTypeKind.Entity;

        public IEdmEntityType EntityType { get; }

        /// <summary>
        /// Returning DeltaKind of the object within DeltaResourceSet payload
        /// </summary>
        public DeltaItemKind DeltaKind { get; }
    }
}
