//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaType.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
