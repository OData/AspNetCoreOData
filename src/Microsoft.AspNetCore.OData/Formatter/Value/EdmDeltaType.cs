﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Implementing IEdmType to identify objects which are part of DeltaResourceSet Payload.
    /// </summary>
    internal class EdmDeltaType : IEdmType
    {
        private IEdmEntityType _entityType;
        private EdmDeltaKind _deltaKind;

        internal EdmDeltaType(IEdmEntityType entityType, EdmDeltaKind deltaKind)
        {
            _entityType = entityType ?? throw Error.ArgumentNull("entityType");
            _deltaKind = deltaKind;
        }

        /// <inheritdoc />
        public EdmTypeKind TypeKind => EdmTypeKind.Entity;

        public IEdmEntityType EntityType => _entityType;

        /// <summary>
        /// Returning DeltaKind of the object within DeltaResourceSet payload
        /// </summary>
        public EdmDeltaKind DeltaKind => _deltaKind;
    }
}
