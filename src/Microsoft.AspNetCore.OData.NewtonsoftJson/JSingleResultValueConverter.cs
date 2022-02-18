//-----------------------------------------------------------------------------
// <copyright file="JSingleResultValueConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Results;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson
{
    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SingleResult"/> instances to JSON.
    /// </summary>
    internal class JSingleResultValueConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified <see cref="SingleResult"/> type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>true if this instance can convert the specified object type; otherwise, false.</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType is null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            return typeof(SingleResult).IsAssignableFrom(objectType);
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
            throw new NotImplementedException(SRResources.ReadSingleResultNotImplemented);
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

            SingleResult singleResult = value as SingleResult;
            if (singleResult != null)
            {
                // TODO: make sure the implementation (to get the first object) is correct?
                var singleObject = singleResult.Queryable.Cast<object>().FirstOrDefault();
                if (singleObject is not null)
                {
                    serializer.Serialize(writer, singleObject);
                }
            }
        }
    }
}
