// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
    /// <summary>
    /// Represents an <see cref="IEdmDeltaDeletedResourceObject"/> with no backing CLR <see cref="Type"/>.
    /// Used to hold the Deleted Resource object in the Delta Feed Payload.
    /// </summary>
    [NonValidatingParameterBinding]
    public class EdmDeltaDeletedResourceObject : EdmEntityObject, IEdmDeltaDeletedResourceObject
    {
        private EdmDeltaType _edmType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedResourceObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedEntityObject.</param>
        public EdmDeltaDeletedResourceObject(IEdmEntityType entityType)
            : this(entityType, isNullable: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedResourceObject"/> class.
        /// </summary>
        /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaDeletedEntityObject.</param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "EntityDefinition checks the nullable.")]
        public EdmDeltaDeletedResourceObject(IEdmEntityTypeReference entityTypeReference)
            : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmDeltaDeletedResourceObject"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaDeletedEntityObject.</param>
        /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
        public EdmDeltaDeletedResourceObject(IEdmEntityType entityType, bool isNullable)
            : base(entityType, isNullable)
        {
            _edmType = new EdmDeltaType(entityType, EdmDeltaKind.DeletedResource);
        }

        /// <inheritdoc />
        public Uri Id { get; set; }

        /// <inheritdoc />
        public DeltaDeletedEntryReason? Reason { get; set; }

        /// <inheritdoc />
        public EdmDeltaKind DeltaKind
        {
            get
            {
                Contract.Assert(_edmType != null);
                return _edmType.DeltaKind;
            }
        }

        /// <summary>
        /// The navigation source of the deleted entity. If null, then the deleted entity is from the current feed.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; set; }
    }
}
