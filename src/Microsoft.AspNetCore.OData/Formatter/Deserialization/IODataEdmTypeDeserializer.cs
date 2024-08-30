//-----------------------------------------------------------------------------
// <copyright file="IODataEdmTypeDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization;

/// <summary>
/// Interface for all <see cref="ODataDeserializer" />s that deserialize into an object backed by <see cref="IEdmType"/>.
/// </summary>
public interface IODataEdmTypeDeserializer: IODataDeserializer
{
    /// <summary>
    /// Deserializes the item into a new object of type corresponding to <paramref name="edmType"/>.
    /// </summary>
    /// <param name="item">The item to deserialize.</param>
    /// <param name="edmType">The EDM type of the object to read into.</param>
    /// <param name="readContext">The <see cref="ODataDeserializerContext"/>.</param>
    /// <returns>The deserialized object.</returns>
    object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext);
}
