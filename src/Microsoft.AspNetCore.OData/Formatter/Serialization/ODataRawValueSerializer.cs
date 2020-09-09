// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing the raw value of an <see cref="IEdmPrimitiveType"/>.
    /// </summary>
    public class ODataRawValueSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataRawValueSerializer"/>.
        /// </summary>
        public ODataRawValueSerializer()
            : base(ODataPayloadKind.Value)
        {
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (messageWriter == null)
            {
                throw new ArgumentNullException(nameof(messageWriter));
            }

            // TODO: Call Async version?
            // TODO: Make the enum alias working
            if (TypeHelper.IsEnum(graph.GetType()))
            {
                messageWriter.WriteValue(graph.ToString());
            }
            else
            {
                TimeZoneInfo timeZone = ODataPrimitiveSerializer.GetTimeZone(writeContext);
                messageWriter.WriteValue(ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph, timeZone));
            }
        }
    }
}
