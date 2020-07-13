// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Abstracts.Annotations;
using Microsoft.AspNetCore.OData.Abstracts.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query.Container
{
    internal class JsonPropertyNameMapper : IPropertyMapper
    {
        private IEdmModel _model;
        private IEdmStructuredType _type;

        public JsonPropertyNameMapper(IEdmModel model, IEdmStructuredType type)
        {
            _model = model;
            _type = type;
        }

        public string MapProperty(string propertyName)
        {
            IEdmProperty property = _type.Properties().Single(s => s.Name == propertyName);
            PropertyInfo info = GetPropertyInfo(property);
            JsonPropertyNameAttribute jsonProperty = GetJsonProperty(info);
            if (jsonProperty != null && !String.IsNullOrWhiteSpace(jsonProperty.Name))
            {
                return jsonProperty.Name;
            }
            else
            {
                return property.Name;
            }
        }

        private PropertyInfo GetPropertyInfo(IEdmProperty property)
        {
            ClrPropertyInfoAnnotation clrPropertyAnnotation = _model.GetAnnotationValue<ClrPropertyInfoAnnotation>(property);
            if (clrPropertyAnnotation != null)
            {
                return clrPropertyAnnotation.ClrPropertyInfo;
            }

            ClrTypeAnnotation clrTypeAnnotation = _model.GetAnnotationValue<ClrTypeAnnotation>(property.DeclaringType);
            Contract.Assert(clrTypeAnnotation != null);

            PropertyInfo info = clrTypeAnnotation.ClrType.GetProperty(property.Name);
            Contract.Assert(info != null);

            return info;
        }

        private static JsonPropertyNameAttribute GetJsonProperty(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), inherit: false)
                   .OfType<JsonPropertyNameAttribute>().SingleOrDefault();
        }
    }
}
