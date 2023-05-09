//-----------------------------------------------------------------------------
// <copyright file="IODataUntypedValueConverter.cs" company=".NET Foundation">
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{

    public interface IUntypedResourceMapper
    {
        IDictionary<string, object> Map(object resource, ODataSerializerContext context);
    }

    public class DefaultUntypedResourceMapper : IUntypedResourceMapper
    {
        /// <summary>
        /// Gets the instance
        /// </summary>
        public static IUntypedResourceMapper Instance = new DefaultUntypedResourceMapper();

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

    /// <summary>
    /// A context for untyped value converter.
    /// </summary>
    public class UntypedValueConverterContext
    {
        /// <summary>
        /// Gets/sets the Edm model.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets/sets the Edm property.
        /// </summary>
        public IEdmStructuralProperty Property { get; set; }

        /// <summary>
        /// Gets/sets the resource.
        /// </summary>
        public ResourceContext Resource { get; set; }
    }

    /// <summary>
    /// Base class for <see cref="IODataSerializer"/> implementations.
    /// </summary>
    /// <remarks>
    /// Each supported CLR type has a corresponding <see cref="ODataSerializer" />. A CLR type is supported if it is one of
    /// the special types or if it has a backing EDM type. Some of the special types are Uri which maps to ODataReferenceLink payload, 
    /// Uri[] which maps to ODataReferenceLinks payload, etc.
    /// </remarks>
    public interface IODataUntypedValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializer"/> class.
        /// </summary>
        /// <param name="originalValue">The original value.</param>
        /// <param name="context">The context.</param>
        /// <returns>converted value.</returns>
        object Convert(object originalValue, UntypedValueConverterContext context);
    }

    /// <summary>
    /// The default
    /// </summary>
    public class DefaultODataUntypedValueConverter : IODataUntypedValueConverter
    {
        /// <summary>
        /// Gets the instance
        /// </summary>
        public static DefaultODataUntypedValueConverter Instance = new DefaultODataUntypedValueConverter();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalValue"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual object Convert(object originalValue, UntypedValueConverterContext context)
        {
            if (originalValue == null)
            {
                return null;
            }

            Type originalType = originalValue.GetType();
            if (typeof(IEdmObject).IsAssignableFrom(originalType) ||
                typeof(DynamicTypeWrapper).IsAssignableFrom(originalType))
            {
                return originalValue;
            }

            IEdmModel model = context.Model;

            // If the value type is defined in Edm model, then we can use it and don't convert.
            IEdmTypeReference edmTypeRef = model.GetEdmTypeReference(originalType);
            if (edmTypeRef != null)
            {
                return originalValue;
            }

            originalType = TypeHelper.GetUnderlyingTypeOrSelf(originalType);
            if (originalType.IsValueType || TypeHelper.IsEnum(originalType))
            {
                // If it's unknown value type (because GetEdmTypeReference can't identify it),
                // let's simply return it as string
                return originalValue.ToString();
            }

            if (typeof(IDictionary).IsAssignableFrom(originalType))
            {
                // TODO: ?
            }

            if (TypeHelper.IsCollection(originalType))
            {
                IEnumerable enumerable = originalValue as IEnumerable;
                EdmUntypedCollection collect = new EdmUntypedCollection();
                foreach (var item in enumerable)
                {
                    collect.Add(Convert(item, context));
                }

                return collect;
            }
            else
            {
                EdmUntypedObject untypedObject = new EdmUntypedObject();

                IEnumerable<PropertyInfo> properties =
                    originalType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(prop => prop.GetIndexParameters().Length == 0 && prop.GetMethod != null);

                foreach (var propertyInfo in properties)
                {
                    object propertyValue = propertyInfo.GetValue(originalValue);

                    untypedObject[propertyInfo.Name] = Convert(propertyValue, context);
                }

                return untypedObject;
            }
        }
    }
}
