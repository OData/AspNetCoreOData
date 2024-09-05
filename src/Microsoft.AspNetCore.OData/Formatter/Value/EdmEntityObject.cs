//-----------------------------------------------------------------------------
// <copyright file="EdmEntityObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;

namespace Microsoft.AspNetCore.OData.Formatter.Value;

/// <summary>
/// Represents an <see cref="IEdmEntityObject"/> with no backing CLR <see cref="Type"/>.
/// </summary>
[NonValidatingParameterBinding]
public class EdmEntityObject : EdmStructuredObject, IEdmEntityObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EdmEntityObject"/> class.
    /// </summary>
    /// <param name="edmType">The <see cref="IEdmEntityType"/> of this object.</param>
    public EdmEntityObject(IEdmEntityType edmType)
        : this(edmType, isNullable: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmEntityObject"/> class.
    /// </summary>
    /// <param name="edmType">The <see cref="IEdmEntityTypeReference"/> of this object.</param>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "EntityDefinition checks the nullable.")]
    public EdmEntityObject(IEdmEntityTypeReference edmType)
        : this(edmType.EntityDefinition(), edmType.IsNullable)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmEntityObject"/> class.
    /// </summary>
    /// <param name="edmType">The <see cref="IEdmEntityType"/> of this object.</param>
    /// <param name="isNullable">true if this object can be nullable; otherwise, false.</param>
    public EdmEntityObject(IEdmEntityType edmType, bool isNullable)
        : base(edmType, isNullable)
    {
    }
}
