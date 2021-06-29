﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// An <see cref="IODataSerializer"/> is used to write a CLR object to an ODataMessage.
    /// </summary>
    /// <remarks>
    /// Each supported CLR type has a corresponding <see cref="IODataSerializer" />. A CLR type is supported if it is one of
    /// the special types or if it has a backing EDM type. Some of the special types are Uri which maps to ODataReferenceLink payload, 
    /// Uri[] which maps to ODataReferenceLinks payload, etc.
    /// </remarks>
    public interface IODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IODataSerializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
        ODataPayloadKind ODataPayloadKind { get; }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a whole using the given messageWriter and writeContext.
        /// </summary>
        /// <param name="graph">The object to be written</param>
        /// <param name="type">The type of the object to be written.</param>
        /// <param name="messageWriter">The <see cref="ODataMessageWriter"/> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext);
    }
}