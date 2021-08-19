//-----------------------------------------------------------------------------
// <copyright file="SerializeUtils.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    /// <summary>
    /// Serialize Utils
    /// </summary>
    public static class SerializeUtils
    {
        public static string SerializeAsJson(Action<Utf8JsonWriter> action)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                JsonWriterOptions options = new JsonWriterOptions
                {
                    Indented = true
                };

                using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(ms))
                {
                    action(jsonWriter);
                    jsonWriter.Flush();
                }

                ms.Seek(0, SeekOrigin.Begin);
                return new StreamReader(ms).ReadToEnd();
            }
        }

        public static async Task<string> SerializeAsJsonAsync(Action<Utf8JsonWriter> action)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                JsonWriterOptions options = new JsonWriterOptions
                {
                    Indented = true
                };

                using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(ms))
                {
                    action(jsonWriter);
                    await jsonWriter.FlushAsync();
                }

                ms.Seek(0, SeekOrigin.Begin);
                return await new StreamReader(ms).ReadToEndAsync();
            }
        }
    }
}
