//-----------------------------------------------------------------------------
// <copyright file="EdmUntypedCollection.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value;

/// <summary>
/// Represents an <see cref="IEdmObject"/> that is a collection of untyped values.
/// </summary>
[NonValidatingParameterBinding]
public sealed class EdmUntypedCollection : List<object>, IEdmObject
{
    /// <inheritdoc/>
    public IEdmTypeReference GetEdmType() => EdmUntypedHelpers.NullableUntypedCollectionReference;
}
