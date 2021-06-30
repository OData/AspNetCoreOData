// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="IODataSerializer"/> that serializes instances of objects backed by an <see cref="IEdmType"/>.
    /// </summary>
    public abstract class ODataEdmTypeSerializer : ODataSerializer, IODataEdmTypeSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeSerializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
        protected ODataEdmTypeSerializer(ODataPayloadKind payloadKind)
            : base(payloadKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeSerializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
        /// <param name="serializerProvider">The <see cref="IODataSerializerProvider"/> to use to write inner objects.</param>
        protected ODataEdmTypeSerializer(ODataPayloadKind payloadKind, IODataSerializerProvider serializerProvider)
            : this(payloadKind)
        {
            SerializerProvider = serializerProvider ?? throw Error.ArgumentNull(nameof(serializerProvider));
        }

        /// <summary>
        /// Gets the <see cref="IODataSerializerProvider"/> that can be used to write inner objects.
        /// </summary>
        public IODataSerializerProvider SerializerProvider { get; }

        /// <inheritdoc/>
        public virtual Task WriteObjectInlineAsync(object graph, IEdmTypeReference expectedType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.WriteObjectInlineNotSupported, GetType().Name);
        }

        /// <inheritdoc/>
        public virtual ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.CreateODataValueNotSupported, GetType().Name);
        }
    }
}
