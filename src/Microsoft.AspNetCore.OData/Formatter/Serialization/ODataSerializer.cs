//-----------------------------------------------------------------------------
// <copyright file="ODataSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Base class for <see cref="IODataSerializer"/> implementations.
    /// </summary>
    /// <remarks>
    /// Each supported CLR type has a corresponding <see cref="ODataSerializer" />. A CLR type is supported if it is one of
    /// the special types or if it has a backing EDM type. Some of the special types are Uri which maps to ODataReferenceLink payload, 
    /// Uri[] which maps to ODataReferenceLinks payload, etc.
    /// </remarks>
    public abstract class ODataSerializer : IODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
        protected ODataSerializer(ODataPayloadKind payloadKind)
        {
            ODataPayloadKindHelper.Validate(payloadKind, nameof(payloadKind));

            ODataPayloadKind = payloadKind;
        }

        /// <inheritdoc/>
        public ODataPayloadKind ODataPayloadKind { get; }

        /// <inheritdoc/>
        public virtual async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            await Task.Run(() => throw Error.NotSupported(SRResources.WriteObjectNotSupported, GetType().Name)).ConfigureAwait(false);
        }
    }
}
