// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon.Values;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    /// <summary>
    /// Extension methods for <see cref="HttpContent"/>
    /// </summary>
    public static class HttpContentExtensions
    {
        /// <summary>
        /// Read the <see cref="HttpContent"/> as OData resource set.
        /// </summary>
        /// <param name="content">The http content.</param>
        /// <returns>The OData array.</returns>
        public static async Task<ODataArray> ReadAsODataArrayAsync(this HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Stream stream = await content.ReadAsStreamAsync();
            using (JsonDocument doc = await JsonDocument.ParseAsync(stream))
            {
                JsonElement root = doc.RootElement;

                // OData array is an object as:
                // {
                //     "@odata.context":...
                //     "value" [ ... ]
                // }
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                if (!root.TryGetProperty("value", out JsonElement value) &&
                    value.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                ODataArray odataArray = new ODataArray();

                // value array
                foreach (var item in value.EnumerateArray())
                {
                    IODataValue itemValue = item.ParseAsODataValue();
                    odataArray.Add(itemValue);
                }

                // context uri
                odataArray.ContextUri = ReadStringPropertyValue(root, "@odata.context", "@context");

                // next link
                odataArray.NextLink = ReadStringPropertyValue(root, "@odata.nextlink", "@nextlink");

                // odata.count
                odataArray.TotalCount = ReadIntPropertyValue(root, "@odata.count", "@count");

                return odataArray;
            }
        }

        /// <summary>
        /// Read the <see cref="HttpContent"/> as OData resource.
        /// </summary>
        /// <param name="content">The http content.</param>
        /// <returns>The OData object.</returns>
        public static async Task<ODataObject> ReadAsODataObjectAsync(this HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Stream stream = await content.ReadAsStreamAsync();
            using (JsonDocument doc = await JsonDocument.ParseAsync(stream))
            {
                JsonElement root = doc.RootElement;

                // OData object is an object as:
                // {
                //     "@odata.context":...
                //     "Id": 2,
                //     "Name: "Sam"
                // }
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                ODataObject odataObject = new ODataObject();

                // value array
                foreach (JsonProperty property in root.EnumerateObject())
                {
                    if (property.Name == "@odata.context" || property.Name == "@context")
                    {
                        // context uri
                        odataObject.ContextUri = property.Value.GetString();
                    }
                    else
                    {
                        IODataValue itemValue = property.Value.ParseAsODataValue();
                        odataObject[property.Name] = itemValue;
                    }
                }

                return odataObject;
            }
        }

        /// <summary>
        /// Read the <see cref="HttpContent"/> as OData value.
        /// </summary>
        /// <param name="content">The http content.</param>
        /// <returns>The OData value.</returns>
        public static IODataValue ReadAsOData(this HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Stream stream = content.ReadAsStreamAsync().Result;
            using (JsonDocument doc = JsonDocument.Parse(stream))
            {
                JsonElement root = doc.RootElement;
                return ParseAsODataValue(root);
            }
        }

        /// <summary>
        /// Read the <see cref="HttpContent"/> as OData value.
        /// </summary>
        /// <param name="content">The http content.</param>
        /// <returns>The OData value.</returns>
        public static async Task<IODataValue> ReadAsODataAsync(this HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Stream stream = await content.ReadAsStreamAsync();
            using (JsonDocument doc = await JsonDocument.ParseAsync(stream))
            {
                JsonElement root = doc.RootElement;
                return ParseAsODataValue(root);
            }
        }

        /// <summary>
        /// Read <see cref="JsonElement"/> as OData value.
        /// </summary>
        /// <param name="node">The JSON element.</param>
        /// <returns>The OData value.</returns>
        public static IODataValue ParseAsODataValue(this JsonElement node)
        {
            switch (node.ValueKind)
            {
                case JsonValueKind.Object: // A JSON object.
                    ODataObject odataObject = new ODataObject();
                    foreach (JsonProperty property in node.EnumerateObject())
                    {
                        odataObject[property.Name] = ParseAsODataValue(property.Value);
                    }

                    return odataObject;

                case JsonValueKind.Array: // A JSON array.
                    ODataArray odataArray = new ODataArray();
                    foreach (JsonElement element in node.EnumerateArray())
                    {
                        odataArray.Add(ParseAsODataValue(element));
                    }

                    return odataArray;

                case JsonValueKind.String: // A JSON string.
                    return new ODataString(node.GetString());

                case JsonValueKind.Number: // A JSON number.
                    return ReadNumber(node);

                case JsonValueKind.True: // A JSON true.
                    return ODataBoolean.True;

                case JsonValueKind.False: // A JSON false.
                    return ODataBoolean.False;

                case JsonValueKind.Null: // A JSON null.
                    return ODataNull.Null;

                case JsonValueKind.Undefined:
                default:
                    throw new ODataException($"Found an Undefined JSON element '{node.GetRawText()}'!");
            }
        }

        /// <summary>
        /// Read the JSON node as a number
        /// </summary>
        /// <param name="node">The JSON element.</param>
        /// <returns>OData number.</returns>
        private static ODataNumber ReadNumber(JsonElement node)
        {
            Contract.Assert(JsonValueKind.Number == node.ValueKind);

            if (node.TryGetInt32(out int int32Value))
            {
                return new ODataInt(int32Value);
            }

            if (node.TryGetInt64(out long int64Value))
            {
                return new ODataLong(int64Value);
            }

            if (node.TryGetDecimal(out decimal decimalValue))
            {
                return new ODataDecimal(decimalValue);
            }

            if (node.TryGetDouble(out double doubleValue))
            {
                return new ODataDouble(doubleValue);
            }

            throw new ODataException($"Can not read a JSON element '{node.GetRawText()}' as Number!");
        }

        private static string ReadStringPropertyValue(JsonElement node, params string[] propertyNames)
        {
            Contract.Assert(node.ValueKind == JsonValueKind.Object);

            foreach (string propertyName in propertyNames)
            {
                if (node.TryGetProperty(propertyName, out JsonElement propertyValue))
                {
                    if (propertyValue.ValueKind != JsonValueKind.String)
                    {
                        throw new ODataException($"Found a non-string JSON element '{node.GetRawText()}'!");
                    }

                    return propertyValue.GetString();
                }
            }

            return null;
        }

        private static int? ReadIntPropertyValue(JsonElement node, params string[] propertyNames)
        {
            Contract.Assert(node.ValueKind == JsonValueKind.Object);

            foreach (string propertyName in propertyNames)
            {
                if (node.TryGetProperty(propertyName, out JsonElement propertyValue))
                {
                    if (propertyValue.ValueKind != JsonValueKind.Number)
                    {
                        throw new ODataException($"Found a non-number JSON element '{node.GetRawText()}'!");
                    }

                    return propertyValue.GetInt32();
                }
            }

            return null;
        }
    }
}
