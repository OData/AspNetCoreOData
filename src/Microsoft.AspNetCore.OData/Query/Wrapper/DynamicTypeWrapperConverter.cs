// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    /// <summary>
    /// Supports converting <see cref="DynamicTypeWrapper"/> types by using a factory pattern.
    /// </summary>
    internal class DynamicTypeWrapperConverter : JsonConverterFactory
    {
        /// <summary>
        /// determines whether the converter instance can convert the specified object type.
        /// </summary>
        /// <param name="typeToConvert">The type of the object to check whether it can be converted by this converter instance.</param>
        /// <returns>true if the instance can convert the specified object type; otherwise, false.</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == null)
            {
                return false;
            }

            return typeof(DynamicTypeWrapper).IsAssignableFrom(typeToConvert);
        }

        /// <summary>
        /// Creates a converter for a specified type.
        /// </summary>
        /// <param name="type">The type handled by the converter.</param>
        /// <param name="options">The serialization options to use.</param>
        /// <returns>A converter for which T is compatible with typeToConvert.</returns>
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            if (type == null)
            {
                return null;
            }

            if (type.IsGenericType)
            {
                // Since 'type' is tested in 'CanConvert()', it must be a generic type
                Type generaticType = type.GetGenericTypeDefinition();
                Type elementType = type.GetGenericArguments()[0];

                if (generaticType == typeof(ComputeWrapper<>))
                {
                    return (JsonConverter)Activator.CreateInstance(typeof(ComputeWrapperConverter<>).MakeGenericType(new Type[] { elementType }));
                }

                if (generaticType == typeof(FlatteningWrapper<>))
                {
                    return (JsonConverter)Activator.CreateInstance(typeof(FlatteningWrapperConverter<>).MakeGenericType(new Type[] { elementType }));
                }
            }
            else
            {
                if (type == typeof(AggregationWrapper))
                {
                    return (JsonConverter)Activator.CreateInstance(typeof(AggregationWrapperConverter));
                }

                if (type == typeof(EntitySetAggregationWrapper))
                {
                    return (JsonConverter)Activator.CreateInstance(typeof(EntitySetAggregationWrapperConverter));
                }

                if (type == typeof(GroupByWrapper))
                {
                    return (JsonConverter)Activator.CreateInstance(typeof(GroupByWrapperConverter));
                }

                if (type == typeof(NoGroupByAggregationWrapper))
                {
                    return (JsonConverter)Activator.CreateInstance(typeof(NoGroupByAggregationWrapperConverter));
                }

                if (type == typeof(NoGroupByWrapper))
                {
                    return (JsonConverter)Activator.CreateInstance(typeof(NoGroupByWrapperConverter));
                }
            }

            return null;
        }
    }
}
