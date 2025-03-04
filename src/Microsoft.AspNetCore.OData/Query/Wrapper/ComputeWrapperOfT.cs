//-----------------------------------------------------------------------------
// <copyright file="ComputeWrapperOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query.Wrapper;

[JsonConverter(typeof(DynamicTypeWrapperConverter))]
internal class ComputeWrapper<T> : GroupByWrapper, IEdmEntityObject
{
    public T Instance { get; set; }

    /// <summary>
    /// The Edm Model associated with the wrapper.
    /// </summary>
    public IEdmModel Model { get; set; }

    public override Dictionary<string, object> Values
    {
        get
        {
            EnsureValues();
            return base.Values;
        }
    }

    private bool _merged;
    private void EnsureValues()
    {
        if (!this._merged)
        {
            // Base properties available via Instance can be real OData properties or generated in previous transformations

            var instanceContainer = this.Instance as DynamicTypeWrapper;
            if (instanceContainer != null)
            {
                // Add properties generated in previous transformations to the collection
                base.Values.MergeWithReplace(instanceContainer.Values);
            }
            else
            {
                // Add real OData properties to the collection
                // We need to use injected Model to real property names
                var edmType = GetEdmType() as IEdmStructuredTypeReference;

                if (edmType is IEdmComplexTypeReference t)
                {
                    _typedEdmStructuredObject = _typedEdmStructuredObject ??
                    new TypedEdmComplexObject(Instance, t, Model);
                }
                else
                {
                    _typedEdmStructuredObject = _typedEdmStructuredObject ??
                    new TypedEdmEntityObject(Instance, edmType as IEdmEntityTypeReference, Model);
                }

                var props = edmType.DeclaredStructuralProperties().Where(p => p.Type.IsPrimitive()).Select(p => p.Name);
                foreach (var propertyName in props)
                {
                    object value;
                    if (_typedEdmStructuredObject.TryGetPropertyValue(propertyName, out value))
                    {
                        base.Values[propertyName] = value;
                    }
                }
            }

            this._merged = true;
        }
    }

    private TypedEdmStructuredObject _typedEdmStructuredObject;

    public IEdmTypeReference GetEdmType()
    {
        return Model.GetEdmTypeReference(typeof(T));
    }
}

internal class ComputeWrapperConverter<T> : JsonConverter<ComputeWrapper<T>>
{
    public override ComputeWrapper<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(ComputeWrapper<>).Name));
    }

    public override void Write(Utf8JsonWriter writer, ComputeWrapper<T> value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            JsonSerializer.Serialize(writer, value.Values, options);
        }
    }
}
