//-----------------------------------------------------------------------------
// <copyright file="ODataRawValueSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
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
        public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
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
                await messageWriter.WriteValueAsync(graph.ToString()).ConfigureAwait(false);
            }
            else
            {
                await messageWriter.WriteValueAsync(ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph, writeContext?.TimeZone)).ConfigureAwait(false);
            }
        }
    }
}
