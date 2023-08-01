//-----------------------------------------------------------------------------
// <copyright file="WeatherForecast.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Net.NetworkInformation;

namespace ODataNewtonsoftJsonSample
{
    /// <summary>
    /// Mac converter.
    /// </summary>
    public class MACConverter : JsonConverter<PhysicalAddress>
    {
        /// <summary>
        /// Read JSON.
        /// </summary>
        /// <param name="reader">Json reader.</param>
        /// <param name="objectType">Object type.</param>
        /// <param name="existingValue">existing value to read.</param>
        /// <param name="hasExistingValue">If has existing value to read.</param>
        /// <param name="serializer">Json serializer.</param>
        /// <returns>Element readed.</returns>
        public override PhysicalAddress ReadJson(JsonReader reader, Type objectType, PhysicalAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value;

            if (value is null)
            {
                return null;
            }

            try
            {
                // If value is not a string, an exception will be thrown.
                string macValue = (string)value;

                return CreatePhysicalAddressFromMAC(macValue);
            }
            catch (Exception exception)
            {
                throw new FormatException(exception.Message);
            }
        }

        /// <summary>
        /// Write JSON.
        /// </summary>
        /// <param name="writer">Json writer.</param>
        /// <param name="value">value to convert.</param>
        /// <param name="serializer">Json serializer.</param>
        public override void WriteJson(JsonWriter writer, PhysicalAddress value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public static PhysicalAddress CreatePhysicalAddressFromMAC(string mac)
        {
            if (mac is null)
            {
                return null;
            }

            try
            {
                // If value is empty, an exception will be thrown.
                if (string.IsNullOrEmpty(mac))
                {
                    throw new FormatException("An invalid physical address was specified: ''");
                }

                return PhysicalAddress.Parse(mac);
            }
            catch (Exception exception)
            {
                throw new FormatException(exception.Message);
            }
        }
    }
}
