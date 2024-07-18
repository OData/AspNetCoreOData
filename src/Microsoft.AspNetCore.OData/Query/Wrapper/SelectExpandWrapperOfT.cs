//-----------------------------------------------------------------------------
// <copyright file="SelectExpandWrapperOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    /* Entityframework requires that the two different type initializers for a given type in the same query have the
same set of properties in the same order.

A ~/People?$select=Name&$expand=Friend results in a select expression that has two SelectExpandWrapper<Person>
expressions, one for the root level person and the second for the expanded Friend person.
The first wrapper has the Container property set (contains Name and Friend values) where as the second wrapper
has the Instance property set as it contains all the properties of the expanded person.

The below four classes workaround that entity framework limitation by defining a separate type for each
property selection combination possible. */
    /// <summary>
    /// Represents a container class that contains properties that are either selected or expanded using $select and $expand.
    /// </summary>
    /// <typeparam name="TElement">The element being selected and expanded.</typeparam>
    internal class SelectExpandWrapper<TElement> : SelectExpandWrapper
    {
        /// <summary>
        /// Gets or sets the instance of the element being selected and expanded.
        /// </summary>
        public TElement Instance
        {
            get { return (TElement)UntypedInstance; }
            set { UntypedInstance = value; }
        }

        /// <summary>
        /// Gets the instance value.
        /// </summary>
        public override object InstanceValue => Instance;

        protected override Type GetElementType()
        {
            return UntypedInstance == null ? typeof(TElement) : UntypedInstance.GetType();
        }
    }

    internal class SelectExpandWrapperConverter<TEntity> : JsonConverter<SelectExpandWrapper<TEntity>>
    {
        public override SelectExpandWrapper<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(SelectExpandWrapper<>).Name));
        }

        public override void Write(Utf8JsonWriter writer, SelectExpandWrapper<TEntity> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToDictionary(SelectExpandWrapperConverter.MapperProvider), options);
        }
    }
}
