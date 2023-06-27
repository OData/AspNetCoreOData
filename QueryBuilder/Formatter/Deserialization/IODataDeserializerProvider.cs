using System;
//using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace ODataQueryBuilder.Formatter.Deserialization
{
    /// <summary>
    /// Represents a factory that creates an <see cref="IODataDeserializer"/>.
    /// </summary>
    public interface IODataDeserializerProvider

    {
        /// <summary>
        /// Gets an <see cref="ODataDeserializer"/> for the given type.
        /// </summary>
        /// <param name="type">The CLR type.</param>
        /// <param name="requestUri">The request being deserialized.</param>
        /// <returns>An <see cref="ODataDeserializer"/> that can deserialize the given type.</returns>
        public IODataDeserializer GetODataDeserializer(Type type, Uri requestUri);

        /// <summary>
        /// Gets the <see cref="ODataEdmTypeDeserializer"/> for the given EDM type.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="isDelta">Is delta</param>
        /// <returns>An <see cref="ODataEdmTypeDeserializer"/> that can deserialize the given EDM type.</returns>
        public IODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType, bool isDelta = false);
    }
}
