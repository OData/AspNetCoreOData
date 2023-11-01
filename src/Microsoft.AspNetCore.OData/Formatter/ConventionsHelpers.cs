//-----------------------------------------------------------------------------
// <copyright file="ConventionsHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter
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

            return ConvertValue(value, resourceContext.TimeZone, resourceContext.EdmModel);
        }

        public static object ConvertValue(object value, TimeZoneInfo timeZone, IEdmModel model)
        {
            Contract.Assert(value != null);

            Type type = value.GetType();
            if (TypeHelper.IsEnum(type))
            {
                value = new ODataEnumValue(value.ToString(), type.EdmFullName());
            }
            else
            {
                Contract.Assert(model.GetEdmPrimitiveTypeReference(type) != null);
                value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(value, timeZone);
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
                return string.Empty;
            }

            IEnumerable<IEdmProperty> keys = entityType.Key();
            if (keys.Count() == 1)
            {
                return GetUriRepresentationForKeyValue(keys.First(), resourceContext);
            }
            else
            {
                IEnumerable<string> keyValues =
                    keys.Select(key => string.Format(
                        CultureInfo.InvariantCulture, "{0}={1}", key.Name, GetUriRepresentationForKeyValue(key, resourceContext)));
                return string.Join(",", keyValues);
            }
        }

        // gets the primitive odata uri representation.
        public static string GetUriRepresentationForValue(object value)
        {
            return GetUriRepresentationForValue(value, TimeZoneInfo.Local);
        }

        public static string GetUriRepresentationForValue(object value, TimeZoneInfo timeZone)
        {
            Contract.Assert(value != null);

            Type type = value.GetType();
            if (TypeHelper.IsEnum(type))
            {
                value = new ODataEnumValue(value.ToString(), type.EdmFullName());
            }
            else
            {
                value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(value, timeZone);
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

            return GetUriRepresentationForValue(value, resourceContext.TimeZone);
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
                return obj.Name.GetHashCode(StringComparison.Ordinal);
            }
        }
    }
}
