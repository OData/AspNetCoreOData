// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Base class for all <see cref="ODataDeserializer" />s that deserialize into an object backed by <see cref="IEdmType"/>.
    /// </summary>
    public abstract class ODataEdmTypeDeserializer : ODataDeserializer, IODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeDeserializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this deserializer reads.</param>
        protected ODataEdmTypeDeserializer(ODataPayloadKind payloadKind)
            : base(payloadKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeDeserializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload this deserializer handles.</param>
        /// <param name="deserializerProvider">The <see cref="IODataDeserializerProvider"/>.</param>
        protected ODataEdmTypeDeserializer(ODataPayloadKind payloadKind, IODataDeserializerProvider deserializerProvider)
            : this(payloadKind)
        {
            if (deserializerProvider == null)
            {
                throw Error.ArgumentNull("deserializerProvider");
            }

            DeserializerProvider = deserializerProvider;
        }

        /// <summary>
        /// The <see cref="IODataDeserializerProvider"/> to use for deserializing inner items.
        /// </summary>
        public IODataDeserializerProvider DeserializerProvider { get; private set; }

        /// <inheritdoc/>
        public virtual object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            throw Error.NotSupported(SRResources.DoesNotSupportReadInLine, GetType().Name);
        }
    }
}

