// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// Represents an <see cref="IEdmDeltaDeletedLink"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Deleted Link object in the Delta ResourceSet Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaDeletedLink : EdmDeltaLinkBase, IEdmDeltaDeletedLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedLink"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedLink.</param>
        public EdmDeltaDeletedLink(IEdmEntityType entityType)
            : this(entityType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedLink"/> class.
        /// </summary>
        /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaDeletedLink.</param>
        public EdmDeltaDeletedLink(IEdmEntityTypeReference entityTypeReference)
            : base(entityTypeReference)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedLink"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedLink.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaDeletedLink(IEdmEntityType entityType, bool isNullable)
            : base(entityType, isNullable)
        {
        }

        /// <inheritdoc />
        public override DeltaItemKind Kind => DeltaItemKind.DeltaDeletedLink;
    }
}