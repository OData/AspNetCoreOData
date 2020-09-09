// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> to serialize <see cref="ODataError"/>s.
    /// </summary>
    public partial class ODataErrorSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the class <see cref="ODataSerializer"/>.
        /// </summary>
        public ODataErrorSerializer()
            : base(ODataPayloadKind.Error)
        {
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                throw Error.ArgumentNull(nameof(graph));
            }

            if (messageWriter == null)
            {
                throw Error.ArgumentNull(nameof(messageWriter));
            }

            ODataError oDataError = graph as ODataError;
            if (oDataError == null)
            {
                if (!IsHttpError(graph))
                {
                    string message = Error.Format(SRResources.ErrorTypeMustBeODataErrorOrHttpError, graph.GetType().FullName);
                    throw new SerializationException(message);
                }
                else
                {
                    oDataError = CreateODataError(graph);
                }
            }

            // TODO: Call Async version?
            bool includeDebugInformation = oDataError.InnerError != null;
            messageWriter.WriteError(oDataError, includeDebugInformation);
        }

        /// <summary>
        /// Return true if the object is an HttpError.
        /// </summary>
        /// <param name="error">The error to test.</param>
        /// <returns>true if the object is an HttpError</returns>
        internal static bool IsHttpError(object error)
        {
            return error is SerializableError;
        }

        /// <summary>
        /// Create an ODataError from an HttpError.
        /// </summary>
        /// <param name="error">The error to use.</param>
        /// <returns>an ODataError.</returns>
        internal static ODataError CreateODataError(object error)
        {
            SerializableError serializableError = error as SerializableError;
            return serializableError.CreateODataError();
        }
    }
}
