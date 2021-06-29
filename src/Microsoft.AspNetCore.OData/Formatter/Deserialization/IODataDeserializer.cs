// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// An <see cref="IODataDeserializer"/> is used to read an ODataMessage into a CLR object.
    /// </summary>
    /// <remarks>
    /// Each supported CLR type has a corresponding <see cref="IODataDeserializer" />. A CLR type is supported if it is one of
    /// the special types or if it has a backing EDM type. Some of the special types are Uri which maps to ODataReferenceLink payload, 
    /// Uri[] which maps to ODataReferenceLinks payload, ODataWorkspace which maps to ODataServiceDocument payload.
    /// </remarks>
    public interface IODataDeserializer
    {
        /// <summary>
        /// The kind of ODataPayload this deserializer handles.
        /// </summary>
        ODataPayloadKind ODataPayloadKind { get; }

        /// <summary>
        /// Reads an <see cref="IODataRequestMessage"/> using messageReader.
        /// </summary>
        /// <param name="messageReader">The messageReader to use.</param>
        /// <param name="type">The type of the object to read into.</param>
        /// <param name="readContext">The read context.</param>
        /// <returns>The deserialized object.</returns>
        Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext);
    }
}