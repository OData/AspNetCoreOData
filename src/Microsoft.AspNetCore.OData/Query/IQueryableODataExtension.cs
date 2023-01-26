//-----------------------------------------------------------------------------
// <copyright file="IQueryableODataExtension.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// The cast options.
    /// </summary>
    public class ODataCastOptions
    {
        /// <summary>
        /// Gets/sets the map provider.
        /// </summary>
        public Func<IEdmModel, IEdmStructuredType, IPropertyMapper> MapProvider { get; set; }
    }

    /// <summary>
    /// Provides a set of static methods for querying data structures that implement <see cref="IQueryable"/>
    /// </summary>
    public static class IQueryableODataExtension
    {
        /// <summary>
        /// Converts the source to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to convert the source to.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="options">The cast options.</param>
        /// <returns>The converted object if it's OData object. Otherwise, return same source or the default.</returns>
        [SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>")]
        public static TResult OCast<TResult>(this object source, ODataCastOptions options = null)
        {
            if (source is null)
            {
                return default;
            }

            Type sourceType = source.GetType();
            if (source is SelectExpandWrapper wrapper)
            {
                return (TResult)CreateInstance(wrapper, sourceType, options);
            }

            if (typeof(TResult) == sourceType)
            {
                return (TResult)source;
            }

            if (typeof(TResult).IsAssignableFrom(sourceType))
            {
                return (TResult)Convert.ChangeType(source, typeof(TResult), CultureInfo.InvariantCulture);
            }

            return default;
        }

        /// <summary>
        /// Converts the elements of an <see cref="IQueryable"/> to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to convert the elements of source to.</typeparam>
        /// <param name="source">The <see cref="IQueryable"/> that contains the elements to be converted.</param>
        /// <param name="options">The cast options.</param>
        /// <returns>Contains each element of the source sequence converted to the specified type.</returns>
        [SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>")]
        public static IEnumerable<TResult> OCast<TResult>(this IQueryable source, ODataCastOptions options = null)
        {
            if (source is null)
            {
                throw Error.ArgumentNull(nameof(source));
            }

            foreach (var item in source)
            {
                yield return OCast<TResult>(item, options);
            }
        }

        /// <summary>
        /// Create the object based on <see cref="SelectExpandWrapper"/>
        /// </summary>
        /// <param name="wrapper">The select expand wrapper.</param>
        /// <param name="resultType">The created type.</param>
        /// <param name="options">The options.</param>
        /// <returns>The created object.</returns>
        private static object CreateInstance(SelectExpandWrapper wrapper, Type resultType, ODataCastOptions options)
        {
            object instance;
            Type instanceType = resultType;
            IEdmModel model = wrapper.Model;
            if (wrapper.UseInstanceForProperties)
            {
                instance = wrapper.InstanceValue;
                instanceType = instance.GetType();
            }
            else
            {
                if (wrapper.InstanceType != null && wrapper.InstanceType != resultType.FullName)
                {
                    // inheritance
                    IEdmTypeReference typeReference = wrapper.GetEdmType();
                    instanceType = model.GetClrType(typeReference); // inheritance type
                }

                instance = Activator.CreateInstance(instanceType);
            }

            IDictionary<string, object> properties;
            if (options != null && options.MapProvider != null)
            {
                properties = wrapper.ToDictionary(options.MapProvider);
            }
            else
            {
                properties = wrapper.ToDictionary();
            }

            foreach (var property in properties)
            {
                string propertyName = property.Key;
                object propertyValue = property.Value;
                if (propertyValue == null)
                {
                    // If it's null, we don't need to do anything.
                    continue;
                }

                PropertyInfo propertyInfo = GetPropertyInfo(instanceType, propertyName);
                if (propertyInfo == null)
                {
                    throw new ODataException(Error.Format(SRResources.PropertyNotFound, instanceType.FullName, propertyName));
                }

                bool isCollection = TypeHelper.IsCollection(propertyInfo.PropertyType, out Type elementType);

                if (isCollection)
                {
                    IList<object> collection = new List<object>();
                    IEnumerable collectionPropertyValue = propertyValue as IEnumerable;
                    foreach (var item in collectionPropertyValue)
                    {
                        object itemValue = CreateValue(item, elementType, options);
                        collection.Add(itemValue);
                    }

                    IEdmTypeReference typeRef = model.GetEdmTypeReference(propertyInfo.PropertyType);
                    IEdmCollectionTypeReference collectionTypeRef = typeRef.AsCollection();

                    DeserializationHelpers.SetCollectionProperty(instance, propertyName, collectionTypeRef, collection, true, null);
                }
                else
                {
                    object itemValue = CreateValue(propertyValue, elementType, options);
                    propertyInfo.SetValue(instance, itemValue);
                }
            }

            return instance;
        }

        private static object CreateValue(object value, Type elementType, ODataCastOptions options)
        {
            Type valueType = value.GetType();

            if (typeof(ISelectExpandWrapper).IsAssignableFrom(valueType))
            {
                SelectExpandWrapper subWrapper = value as SelectExpandWrapper;
                return CreateInstance(subWrapper, elementType, options);
            }
            else
            {
                return value;
            }
        }

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propertyInfo = null;
            var properties = type.GetProperties().Where(p => p.Name == propertyName).ToArray();
            if (properties.Length <= 0)
            {
                propertyInfo = null;
            }
            else if (properties.Length == 1)
            {
                propertyInfo = properties[0];
            }
            else
            {
                // resolve 'new' modifier
                propertyInfo = properties.FirstOrDefault(p => p.DeclaringType == type);
                if (propertyInfo == null)
                {
                    throw new ODataException(Error.Format(SRResources.AmbiguousPropertyNameFound, propertyName));
                }
            }

            return propertyInfo;
        }
    }
}
