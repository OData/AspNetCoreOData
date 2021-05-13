// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an <see cref="IEdmDeltaDeletedLink"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Deleted Link object in the Delta ResourceSet Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaDeletedLink : EdmEntityObject, IEdmDeltaDeletedLink
    {
        // TODO: Why derived from EdmEntityObject, it doesn't make sense?
        private EdmDeltaType _edmType;

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
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "EntityDefinition checks the nullable.")]
        public EdmDeltaDeletedLink(IEdmEntityTypeReference entityTypeReference)
            : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
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
            _edmType = new EdmDeltaType(entityType, DeltaItemKind.DeltaDeletedLink);
        }

        /// <inheritdoc />
        public Uri Source { get; set; }

        /// <inheritdoc />
        public Uri Target { get; set; }

        /// <inheritdoc />
        public string Relationship { get; set; }

        /// <inheritdoc />
        public DeltaItemKind DeltaKind
        {
            get
            {
                Contract.Assert(_edmType != null);
                return _edmType.DeltaKind;
            }
        }
    }
}