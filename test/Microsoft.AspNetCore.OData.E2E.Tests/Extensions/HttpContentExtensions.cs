//-----------------------------------------------------------------------------
// <copyright file="HttpContentExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Extensions
{
    /// <summary>
    /// Extensions for HttpContent.
    /// </summary>
    public static class HttpContentExtensions
    {
        /// <summary>
        /// Get the content as the value of ObjectContent.
        /// </summary>
        /// <returns>The content value.</returns>
        public static string AsObjectContentValue(this HttpContent content)
        {
            string json = content.ReadAsStringAsync().Result;
            try
            {
                using JsonDocument document = JsonDocument.Parse(json);
               
                JsonElement root = document.RootElement;

                return root.GetProperty("value").GetString();
            }
            catch (System.Text.Json.JsonException)
            {
            }

            return json;
        }

        /// <summary>
        /// A custom extension for AspNetCore to deserialize JSON content as an object.
        /// AspNet provides this in  System.Net.Http.Formatting.
        /// </summary>
        /// <returns>The content value.</returns>
        public static async Task<T> ReadAsObject<T>(this HttpContent content)
        {
            string json = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Get the content as the value of JsonElement.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The content as Json element.</returns>
        public static async Task<JsonElement> ReadAsElement(this HttpContent content)
        {
            string json = await content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(json);

            return  document.RootElement;
        }
    }
}
