// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.Edm;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="IODataSerializer"/> that serializes instances of objects backed by an <see cref="IEdmType"/>.
    /// </summary>
    public interface IODataEdmTypeSerializer: IODataSerializer
    {
        /// <summary>
        /// Creates an <see cref="ODataValue"/> for the object represented by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The value of the <see cref="ODataValue"/> to be created.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        /// <returns>The <see cref="ODataValue"/> created.</returns>
        ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext);

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writer">The <see cref="ODataWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        Task WriteObjectInlineAsync(object graph, IEdmTypeReference expectedType, ODataWriter writer, ODataSerializerContext writeContext);
    }
}