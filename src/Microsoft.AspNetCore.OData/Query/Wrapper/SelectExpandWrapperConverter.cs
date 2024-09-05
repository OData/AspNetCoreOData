//-----------------------------------------------------------------------------
// <copyright file="SelectExpandWrapperConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query.Wrapper;

/// <summary>
/// Supports converting <see cref="SelectExpandWrapper{T}"/> types by using a factory pattern.
/// </summary>
internal class SelectExpandWrapperConverter : JsonConverterFactory
{
    public static readonly Func<IEdmModel, IEdmStructuredType, IPropertyMapper> MapperProvider =
        (IEdmModel model, IEdmStructuredType type) => new JsonPropertyNameMapper(model, type);

    /// <summary>
    /// determines whether the converter instance can convert the specified object type.
    /// </summary>
    /// <param name="typeToConvert">The type of the object to check whether it can be converted by this converter instance.</param>
    /// <returns>true if the instance can convert the specified object type; otherwise, false.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == null || !typeToConvert.IsGenericType)
        {
            return false;
        }

        return typeof(ISelectExpandWrapper).IsAssignableFrom(typeToConvert);

        /* We can use the following codes to limit the compare.
         * But, use the above ISelectExpandWrapper can unblock the new type later.
        Type generaticType = typeToConvert.GetGenericTypeDefinition();
        if (generaticType == typeof(SelectSome<>) ||
            generaticType == typeof(SelectSomeAndInheritance<>) ||
            generaticType == typeof(SelectAllAndExpand<>) ||
            generaticType == typeof(SelectAll<>) ||
            generaticType == typeof(SelectExpandWrapper<>))
        {
            return true;
        }

        return false;
        */
    }

    /// <summary>
    /// Creates a converter for a specified type.
    /// </summary>
    /// <param name="type">The type handled by the converter.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>A converter for which T is compatible with typeToConvert.</returns>
    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        if (type == null || !type.IsGenericType)
        {
            return null;
        }

        // Since 'type' is tested in 'CanConvert()', it must be a generic type
        Type generaticType = type.GetGenericTypeDefinition();
        Type entityType = type.GetGenericArguments()[0];

        if (generaticType == typeof(SelectSome<>))
        {
            return (JsonConverter)Activator.CreateInstance(typeof(SelectSomeConverter<>).MakeGenericType(new Type[] { entityType }));
        }

        if (generaticType == typeof(SelectSomeAndInheritance<>))
        {
            return (JsonConverter)Activator.CreateInstance(typeof(SelectSomeAndInheritanceConverter<>).MakeGenericType(new Type[] { entityType }));
        }

        if (generaticType == typeof(SelectAll<>))
        {
            return (JsonConverter)Activator.CreateInstance(typeof(SelectAllConverter<>).MakeGenericType(new Type[] { entityType }));
        }

        if (generaticType == typeof(SelectAllAndExpand<>))
        {
            return (JsonConverter)Activator.CreateInstance(typeof(SelectAllAndExpandConverter<>).MakeGenericType(new Type[] { entityType }));
        }

        if (generaticType == typeof(SelectExpandWrapper<>))
        {
            return (JsonConverter)Activator.CreateInstance(typeof(SelectExpandWrapperConverter<>).MakeGenericType(new Type[] { entityType }));
        }

        return null;
    }
}
