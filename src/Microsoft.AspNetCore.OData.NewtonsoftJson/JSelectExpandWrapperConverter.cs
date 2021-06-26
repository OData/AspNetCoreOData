// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.Edm;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson
{
    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="ISelectExpandWrapper"/> instances to JSON.
    /// </summary>
    internal class JSelectExpandWrapperConverter : JsonConverter
    {
        private Func<IEdmModel, IEdmStructuredType, IPropertyMapper> _mapperProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpandWrapperConverter"/> class.
        /// </summary>
        public JSelectExpandWrapperConverter()
            : this ((IEdmModel model, IEdmStructuredType type) => new JsonPropertyNameMapper(model, type))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpandWrapperConverter"/> class.
        /// </summary>
        /// <param name="mapperProvider">The mapper provider.</param>
        public JSelectExpandWrapperConverter(Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider)
        {
            _mapperProvider = mapperProvider ?? throw new ArgumentNullException(nameof(mapperProvider));
        }

        /// <summary>
        /// Determines whether this instance can convert the specified <see cref="ISelectExpandWrapper"/> type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>true if this instance can convert the specified object type; otherwise, false.</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType is null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            return typeof(ISelectExpandWrapper).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException(SRResources.ReadSelectExpandWrapperNotImplemented);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (serializer is null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            ISelectExpandWrapper selectExpandWrapper = value as ISelectExpandWrapper;
            if (selectExpandWrapper != null)
            {
                serializer.Serialize(writer, selectExpandWrapper.ToDictionary(_mapperProvider));
            }
        }
    }
}
