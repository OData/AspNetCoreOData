//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaLinkBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter.Value;

/// <summary>
/// The base class for delta link.
/// </summary>
public abstract class EdmDeltaLinkBase : IEdmDeltaLinkBase
{
    private IEdmEntityTypeReference _edmTypeReference;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmDeltaLinkBase"/> class.
    /// </summary>
    /// <param name="typeReference">The given entity type reference.</param>
    protected EdmDeltaLinkBase(IEdmEntityTypeReference typeReference)
    {
        _edmTypeReference = typeReference ?? throw Error.ArgumentNull(nameof(typeReference));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmDeltaLinkBase"/> class.
    /// </summary>
    /// <param name="entityType">The given entity type.</param>
    /// <param name="isNullable">Nullable or not.</param>
    protected EdmDeltaLinkBase(IEdmEntityType entityType, bool isNullable)
    {
        if (entityType == null)
        {
            throw Error.ArgumentNull(nameof(entityType));
        }

        _edmTypeReference = new EdmEntityTypeReference(entityType, isNullable);
    }

    /// <summary>
    /// Gets the entity type.
    /// </summary>
    public IEdmEntityType EntityType => _edmTypeReference.EntityDefinition();

    /// <summary>
    /// Gets the nullable value.
    /// </summary>
    public bool IsNullable => _edmTypeReference.IsNullable;

    /// <summary>
    /// The Uri of the entity from which the relationship is defined, which may be absolute or relative.
    /// </summary>
    public Uri Source { get; set; }

    /// <summary>
    /// The Uri of the related entity, which may be absolute or relative.
    /// </summary>
    public Uri Target { get; set; }

    /// <summary>
    /// The name of the relationship property on the parent object.
    /// </summary>
    public string Relationship { get; set; }

    /// <inheritdoc />
    public abstract DeltaItemKind Kind { get; }

    /// <inheritdoc />
    public IEdmTypeReference GetEdmType() => _edmTypeReference;
}
