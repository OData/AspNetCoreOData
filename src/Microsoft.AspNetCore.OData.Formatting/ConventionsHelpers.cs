// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatting
{
    internal static class ConventionsHelpers
    {
        public static IEnumerable<KeyValuePair<string, object>> GetEntityKey(ResourceContext resourceContext)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(resourceContext.StructuredType != null);
            Contract.Assert(resourceContext.EdmObject != null);

            IEdmEntityType entityType = resourceContext.StructuredType as IEdmEntityType;
            if (entityType == null)
            {
                return Enumerable.Empty<KeyValuePair<string, object>>();
            }

            IEnumerable<IEdmStructuralProperty> keys = entityType.Key();
            return keys.Select(k => new KeyValuePair<string, object>(k.Name, GetKeyValue(k, resourceContext)));
        }

        private static object GetKeyValue(IEdmProperty key, ResourceContext resourceContext)
        {
            Contract.Assert(key != null);
            Contract.Assert(resourceContext != null);

            object value = resourceContext.GetPropertyValue(key.Name);
            if (value == null)
            {
                IEdmTypeReference edmType = resourceContext.EdmObject.GetEdmType();
                throw Error.InvalidOperation(SRResources.KeyValueCannotBeNull, key.Name, edmType.Definition);
            }

            return ConvertValue(value);
        }

        public static object ConvertValue(object value)
        {
            Contract.Assert(value != null);

            Type type = value.GetType();
            if (TypeHelper.IsEnum(type))
            {
                value = new ODataEnumValue(value.ToString(), type.EdmFullName());
            }
            else
            {
                Contract.Assert(EdmLibHelper.GetEdmPrimitiveTypeOrNull(type) != null);
                value = ConvertUnsupportedPrimitives(value);
            }

            return value;
        }

        public static string GetEntityKeyValue(ResourceContext resourceContext)
        {
            Contract.Assert(resourceContext != null);
            Contract.Assert(resourceContext.StructuredType != null);
            Contract.Assert(resourceContext.EdmObject != null);

            IEdmEntityType entityType = resourceContext.StructuredType as IEdmEntityType;
            if (entityType == null)
            {
                return String.Empty;
            }

            IEnumerable<IEdmProperty> keys = entityType.Key();
            if (keys.Count() == 1)
            {
                return GetUriRepresentationForKeyValue(keys.First(), resourceContext);
            }
            else
            {
                IEnumerable<string> keyValues =
                    keys.Select(key => String.Format(
                        CultureInfo.InvariantCulture, "{0}={1}", key.Name, GetUriRepresentationForKeyValue(key, resourceContext)));
                return String.Join(",", keyValues);
            }
        }

        public static bool IsValidStructuralProperty(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            // ignore any indexer properties.
            if (propertyInfo.GetIndexParameters().Any())
            {
                return false;
            }

            if (propertyInfo.CanRead)
            {
                // non-public getters are not valid properties
                MethodInfo publicGetter = propertyInfo.GetGetMethod();
                if (publicGetter != null && propertyInfo.PropertyType.IsValidStructuralPropertyType())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidStructuralPropertyType(this Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            Type elementType;

            return !(type.IsGenericTypeDefinition
                     || type.IsPointer
                     || type == typeof(object)
                     || (TypeHelper.IsCollection(type, out elementType) && elementType == typeof(object)));
        }

        // gets the primitive odata uri representation.
        public static string GetUriRepresentationForValue(object value)
        {
            Contract.Assert(value != null);

            Type type = value.GetType();
            if (TypeHelper.IsEnum(type))
            {
                value = new ODataEnumValue(value.ToString(), type.EdmFullName());
            }
            else
            {
                Contract.Assert(EdmLibHelper.GetEdmPrimitiveTypeOrNull(type) != null);
                value = ConvertUnsupportedPrimitives(value);
            }

            return ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4);
        }

        private static string GetUriRepresentationForKeyValue(IEdmProperty key, ResourceContext resourceContext)
        {
            Contract.Assert(key != null);
            Contract.Assert(resourceContext != null);

            object value = resourceContext.GetPropertyValue(key.Name);
            if (value == null)
            {
                IEdmTypeReference edmType = resourceContext.EdmObject.GetEdmType();
                throw Error.InvalidOperation(SRResources.KeyValueCannotBeNull, key.Name, edmType.Definition);
            }

            return GetUriRepresentationForValue(value);
        }

        internal static object ConvertUnsupportedPrimitives(object value)
        {
            if (value != null)
            {
                Type type = value.GetType();

                // Note that type cannot be a nullable type as value is not null and it is boxed.
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                        return new String((char)value, 1);

                    case TypeCode.UInt16:
                        return (int)(ushort)value;

                    case TypeCode.UInt32:
                        return (long)(uint)value;

                    case TypeCode.UInt64:
                        return checked((long)(ulong)value);

                    case TypeCode.DateTime:
                        DateTime dateTime = (DateTime)value;
                        return TimeZoneInfoHelper.ConvertToDateTimeOffset(dateTime);

                    default:
                        if (type == typeof(char[]))
                        {
                            return new String(value as char[]);
                        }
                        else if (type == typeof(XElement))
                        {
                            return ((XElement)value).ToString();
                        }

                        break;
                }
            }

            return value;
        }


        private class PropertyEqualityComparer : IEqualityComparer<PropertyInfo>
        {
            public static PropertyEqualityComparer Instance = new PropertyEqualityComparer();

            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                Contract.Assert(x != null);
                Contract.Assert(y != null);

                return x.Name == y.Name;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                Contract.Assert(obj != null);
                return obj.Name.GetHashCode();
            }
        }
    }
}

