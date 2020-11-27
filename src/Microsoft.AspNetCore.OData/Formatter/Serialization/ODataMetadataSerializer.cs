// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing $metadata. 
    /// </summary>
    public class ODataMetadataSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataMetadataSerializer"/>.
        /// </summary>
        public ODataMetadataSerializer()
            : base(ODataPayloadKind.MetadataDocument)
        {
        }

        /// <inheritdoc/>
        /// <remarks>The metadata written is from the model set on the <paramref name="messageWriter"/>. The <paramref name="graph" />
        /// is not used.</remarks>
        public override Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                return Task.FromException(Error.ArgumentNull(nameof(messageWriter)));
            }

            // NOTE: ODataMessageWriter doesn't have a way to set the IEdmModel. So, there is an underlying assumption here that
            // the model received by this method and the model passed(from configuration) while building ODataMessageWriter is the same (clr object).
            return Task.Run(() => messageWriter.WriteMetadataDocument());

            // TODO: add the async version in the ODL
            // messageWriter.WriteMetadataDocumentAsync
        }
    }
}
