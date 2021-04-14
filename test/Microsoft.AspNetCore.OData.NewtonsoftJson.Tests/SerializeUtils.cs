// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson.Tests
{
    /// <summary>
    /// Serialize Utils
    /// </summary>
    public static class SerializeUtils
    {
        public static string WriteJson(JsonConverter converter, object value, bool indented = false)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Formatting = indented ? Formatting.Indented : Formatting.None;
                settings.Converters.Add(converter);

                JsonSerializer serializer = JsonSerializer.Create(settings);

                converter.WriteJson(writer, value, serializer);

                writer.Flush();

                return sb.ToString();
            }
        }
    }
}
