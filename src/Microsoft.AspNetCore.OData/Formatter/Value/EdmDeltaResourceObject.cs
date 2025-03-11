//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaResourceObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter.Value;

/// <summary>
/// Represents an <see cref="IEdmChangedObject"/> with no backing CLR <see cref="Type"/>.
/// Used to hold the Entry object in the Delta Feed Payload.
/// </summary>
[Obsolete("EdmDeltaResourceObject is obsolete and will be dropped in the 10.x release. Please use EdmEntityObject instead.")]
[NonValidatingParameterBinding]
public class EdmDeltaResourceObject : EdmEntityObject, IEdmChangedObject
{
    // TODO: this class should remove, use the EdmEntityObject.
    private EdmDeltaType _edmType;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmDeltaResourceObject"/> class.
    /// </summary>
    /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaEntityObject.</param>
    public EdmDeltaResourceObject(IEdmEntityType entityType)
        : this(entityType, isNullable: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmDeltaResourceObject"/> class.
    /// </summary>
    /// <param name="entityTypeReference">The <see cref="IEdmEntityTypeReference"/> of this DeltaEntityObject.</param>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "EntityDefinition checks the nullable.")]
    public EdmDeltaResourceObject(IEdmEntityTypeReference entityTypeReference)
        : this(entityTypeReference.EntityDefinition(), entityTypeReference.IsNullable)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmDeltaResourceObject"/> class.
    /// </summary>
    /// <param name="entityType">The <see cref="IEdmEntityType"/> of this DeltaEntityObject.</param>
    /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
    public EdmDeltaResourceObject(IEdmEntityType entityType, bool isNullable)
        : base(entityType, isNullable)
    {
        _edmType = new EdmDeltaType(entityType, DeltaItemKind.Resource);
    }

    /// <inheritdoc />
    public DeltaItemKind DeltaKind
    {
        get
        {
            Contract.Assert(_edmType != null);
            return _edmType.DeltaKind;
        }
    }

    /// <summary>
    /// The navigation source of the entity. If null, then the entity is from the current feed.
    /// </summary>
    public IEdmNavigationSource NavigationSource { get; set; }
}
