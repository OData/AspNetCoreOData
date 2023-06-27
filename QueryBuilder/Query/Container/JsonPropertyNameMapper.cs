using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataQueryBuilder.Query.Container;

namespace ODataQueryBuilder.Query.Container
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

            JsonIgnoreAttribute jsonIgnore = GetJsonIgnore(info);
            if (jsonIgnore != null)
            {
                return null;
            }

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

        private static JsonIgnoreAttribute GetJsonIgnore(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: false)
                   .OfType<JsonIgnoreAttribute>().SingleOrDefault();
        }
    }
}
