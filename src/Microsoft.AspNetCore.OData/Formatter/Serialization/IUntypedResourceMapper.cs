//-----------------------------------------------------------------------------
// <copyright file="IUntypedResourceMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// The mapper interface to map a resource to a dictionary of properties.
    /// </summary>
    public interface IUntypedResourceMapper
    {
        /// <summary>
        /// Map the given object to a dictionary.
        /// </summary>
        /// <param name="resource">The given resource.</param>
        /// <param name="context">The serializer context.</param>
        /// <returns>The mapped dictionary.</returns>
        IDictionary<string, object> Map(object resource, ODataSerializerContext context);
    }

    /// <summary>
    /// Default implementation of <see cref="IUntypedResourceMapper"/>.
    /// </summary>
    public class DefaultUntypedResourceMapper : IUntypedResourceMapper
    {
        /// <summary>
        /// Gets the instance
        /// </summary>
        public static IUntypedResourceMapper Instance = new DefaultUntypedResourceMapper();

        /// <summary>
        /// Map the given object to a dictionary.
        /// </summary>
        /// <param name="resource">The given resource.</param>
        /// <param name="context">The serializer context.</param>
        /// <returns>The mapped dictionary.</returns>
        public virtual IDictionary<string, object> Map(object resource, ODataSerializerContext context)
        {
            IDictionary<string, object> mapped = new Dictionary<string, object>();
            if (resource == null)
            {
                return mapped;
            }

            Type originalType = resource.GetType();
            // Let's consider a dictionary is a resource (key and value pairs)
            if (resource is IDictionary dict)
            {
                foreach (var item in dict.Keys)
                {
                    // Now matter what type of the key, let's covert it to string
                    // If duplicated, the last wins.
                    mapped[item.ToString()] = dict[item];
                }

                return mapped;
            }

            IEnumerable<PropertyInfo> propeInfos =
                    originalType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(prop => prop.GetIndexParameters().Length == 0 && prop.GetMethod != null);

            foreach (PropertyInfo propInfo in propeInfos)
            {
                JsonIgnoreAttribute jsonIgnore = GetJsonIgnore(propInfo);
                if (jsonIgnore != null)
                {
                    return null;
                }

                string propertyName = propInfo.Name;
                JsonPropertyNameAttribute jsonProperty = GetJsonProperty(propInfo);
                if (jsonProperty != null && !string.IsNullOrWhiteSpace(jsonProperty.Name))
                {
                    propertyName = jsonProperty.Name;
                }

                mapped[propertyName] = propInfo.GetValue(resource);
            }

            return mapped;
        }

        private static JsonPropertyNameAttribute GetJsonProperty(PropertyInfo property) =>
            property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), inherit: false)
            .OfType<JsonPropertyNameAttribute>().SingleOrDefault();

        private static JsonIgnoreAttribute GetJsonIgnore(PropertyInfo property) =>
            property.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: false)
            .OfType<JsonIgnoreAttribute>().SingleOrDefault();
    }
}
