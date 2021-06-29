// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
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
}