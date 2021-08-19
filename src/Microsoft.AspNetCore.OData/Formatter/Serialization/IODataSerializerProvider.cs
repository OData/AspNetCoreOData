//-----------------------------------------------------------------------------
// <copyright file="IODataSerializerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// An <see cref="IODataSerializerProvider"/> is a factory for creating <see cref="IODataSerializer"/>s.
    /// </summary>
    public interface IODataSerializerProvider
    {
        /// <summary>
        /// Gets an <see cref="ODataEdmTypeSerializer"/> for the given edmType.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmTypeReference"/>.</param>
        /// <returns>The <see cref="ODataSerializer"/>.</returns>
        public IODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType);

        /// <summary>
        /// Gets an <see cref="ODataSerializer"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which the serializer is being requested.</param>
        /// <param name="request">The request for which the response is being serialized.</param>
        /// <returns>The <see cref="IODataSerializer"/> for the given type.</returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public IODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request);
    }
}
