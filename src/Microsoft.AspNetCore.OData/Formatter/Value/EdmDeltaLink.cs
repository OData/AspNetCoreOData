// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an <see cref="IEdmDeltaLink"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Added/Modified Link object in the Delta ResourceSet Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaLink : EdmEntityObject, IEdmDeltaLink
    {
        private EdmDeltaType _edmType;

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
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "EntityDefinition checks the nullable.")]
        public EdmDeltaLink(IEdmEntityTypeReference entityTypeReference)
            : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
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
            _edmType = new EdmDeltaType(entityType, EdmDeltaKind.DeltaLink);
        }

        /// <inheritdoc />
        public Uri Source { get; set; }

        /// <inheritdoc />
        public Uri Target { get; set; }

        /// <inheritdoc />
        public string Relationship { get; set; }

        /// <inheritdoc />
        public EdmDeltaKind DeltaKind
        {
            get
            {
                Contract.Assert(_edmType != null);
                return _edmType.DeltaKind;
            }
        }
    }
}