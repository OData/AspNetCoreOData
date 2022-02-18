//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaLink.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an <see cref="IEdmDeltaLink"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Added/Modified Link object in the Delta ResourceSet Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaLink : EdmDeltaLinkBase, IEdmDeltaLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaLink"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaLink.</param>
        public EdmDeltaLink(IEdmEntityType entityType)
            : this(entityType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaLink"/> class.
        /// </summary>
        /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaLink.</param>
        public EdmDeltaLink(IEdmEntityTypeReference entityTypeReference)
            : base(entityTypeReference)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaLink"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaLink.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaLink(IEdmEntityType entityType, bool isNullable)
            : base(entityType, isNullable)
        {
        }

        /// <inheritdoc />
        public override DeltaItemKind Kind => DeltaItemKind.DeltaLink;
    }
}
