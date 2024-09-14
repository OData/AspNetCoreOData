//-----------------------------------------------------------------------------
// <copyright file="DeltaDeletedLinkOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNetCore.OData.Deltas;

/// <summary>
/// <see cref="DeltaDeletedLink{T}" /> allows and tracks changes to delta deleted link.
/// </summary>
internal sealed class DeltaDeletedLink<T> : DeltaLinkBase<T>, IDeltaDeletedLink where T: class
{
    /// <summary>
    /// Initializes a new instance of <see cref="DeltaDeletedLink{T}"/>.
    /// </summary>
    public DeltaDeletedLink()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DeltaDeletedLink{T}"/>.
    /// </summary>
    /// <param name="structuralType">The actual structural type.</param>
    public DeltaDeletedLink(Type structuralType)
        : base(structuralType)
    {
    }

    /// <inheritdoc />
    public override DeltaItemKind Kind => DeltaItemKind.DeltaDeletedLink;
}
