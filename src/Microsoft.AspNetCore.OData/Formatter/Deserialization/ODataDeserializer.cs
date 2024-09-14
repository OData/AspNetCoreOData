//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization;

/// <summary>
/// An <see cref="ODataDeserializer"/> is used to read an ODataMessage into a CLR object.
/// </summary>
/// <remarks>
/// Each supported CLR type has a corresponding <see cref="ODataDeserializer" />. A CLR type is supported if it is one of
/// the special types or if it has a backing EDM type. Some of the special types are Uri which maps to ODataReferenceLink payload, 
/// Uri[] which maps to ODataReferenceLinks payload, ODataWorkspace which maps to ODataServiceDocument payload.
/// </remarks>
public abstract class ODataDeserializer : IODataDeserializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataDeserializer"/> class.
    /// </summary>
    /// <param name="payloadKind">The kind of payload this deserializer handles.</param>
    protected ODataDeserializer(ODataPayloadKind payloadKind)
    {
        ODataPayloadKind = payloadKind;
    }

    /// <inheritdoc />
    public ODataPayloadKind ODataPayloadKind { get; private set; }

    /// <inheritdoc/>
    public virtual Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
    {
        throw Error.NotSupported(SRResources.DeserializerDoesNotSupportRead, GetType().Name);
    }
}
