//-----------------------------------------------------------------------------
// <copyright file="DeserializationHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    internal static class DeserializationHelpers
    {
        internal static void ApplyProperty(ODataProperty property, IEdmStructuredTypeReference resourceType, object resource,
            IODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmStructuredType structuredType = resourceType.StructuredDefinition();
            IEdmProperty edmProperty = structuredType == null ? null : structuredType.ResolveProperty(property.Name);

            bool isDynamicProperty = false;
            string propertyName = property.Name;
            if (edmProperty != null)
            {
                propertyName = readContext.Model.GetClrPropertyName(edmProperty);
            }
            else
            {
                isDynamicProperty = structuredType != null && structuredType.IsOpen;
            }

            if (!isDynamicProperty && edmProperty == null)
            {
                throw new ODataException(
                    Error.Format(SRResources.CannotDeserializeUnknownProperty, property.Name, resourceType.Definition));
            }

            // dynamic properties have null values
            IEdmTypeReference propertyType = edmProperty != null ? edmProperty.Type : null;

            EdmTypeKind propertyKind;
            object value = ConvertValue(property.Value, ref propertyType, deserializerProvider, readContext,
                out propertyKind);

            if (isDynamicProperty)
            {
                SetDynamicProperty(resource, resourceType, propertyKind, propertyName, value, propertyType,
                    readContext.Model);
            }
            else
            {
                SetDeclaredProperty(resource, propertyKind, propertyName, value, edmProperty, readContext);
            }
        }

        internal static void SetDynamicProperty(object resource, IEdmStructuredTypeReference resourceType,
            EdmTypeKind propertyKind, string propertyName, object propertyValue, IEdmTypeReference propertyType,
            IEdmModel model)
        {
            if (propertyKind == EdmTypeKind.Collection && propertyValue.GetType() != typeof(EdmComplexObjectCollection)
                && propertyValue.GetType() != typeof(EdmEnumObjectCollection))
            {
                SetDynamicCollectionProperty(resource, propertyName, propertyValue, propertyType.AsCollection(),
                    resourceType?.StructuredDefinition(), model);
            }
            else
            {
                SetDynamicProperty(resource, propertyName, propertyValue, resourceType.StructuredDefinition(),
                    model);
            }
        }

        internal static void SetDeclaredProperty(object resource, EdmTypeKind propertyKind, string propertyName,
            object propertyValue, IEdmProperty edmProperty, ODataDeserializerContext readContext)
        {
            if (propertyKind == EdmTypeKind.Collection)
            {
                SetCollectionProperty(resource, edmProperty, propertyValue, propertyName);
            }
            else
            {
                if (!readContext.IsNoClrType)
                {
                    if (propertyKind == EdmTypeKind.Primitive)
                    {
                        propertyValue = EdmPrimitiveHelper.ConvertPrimitiveValue(propertyValue,
                            GetPropertyType(resource, propertyName), readContext.TimeZone);
                    }
                }

                SetProperty(resource, propertyName, propertyValue);
            }
        }

        internal static void SetCollectionProperty(object resource, IEdmProperty edmProperty, object value, string propertyName, ODataDeserializerContext context = null)
        {
            Contract.Assert(edmProperty != null);

            SetCollectionProperty(resource, propertyName, edmProperty.Type.AsCollection(), value, clearCollection: false, context: context);
        }

        internal static void SetCollectionProperty(object resource, string propertyName,
            IEdmCollectionTypeReference edmPropertyType, object value, bool clearCollection, ODataDeserializerContext context = null)
        {
            if (value != null)
            {
                // If the setting value is a delta set, we don't need to create a new collection, just use it.
                if (value is IDeltaSet set)
                {
                    SetProperty(resource, propertyName, set);
                    return;
                }

                IEnumerable collection = value as IEnumerable;
                Contract.Assert(collection != null,
                    "SetCollectionProperty is always passed the result of ODataFeedDeserializer or ODataCollectionDeserializer");

                Type resourceType = resource.GetType();
                Type propertyType = GetPropertyType(resource, propertyName);

                if (propertyType == typeof(object))
                {
                    SetProperty(resource, propertyName, collection);
                    return;
                }

                Type elementType;
                if (!TypeHelper.IsCollection(propertyType, out elementType))
                {
                    string message = Error.Format(SRResources.PropertyIsNotCollection, propertyType.FullName, propertyName, resourceType.FullName);
                    throw new SerializationException(message);
                }

                IEnumerable newCollection;
                if (CanSetProperty(resource, propertyName) &&
                    CollectionDeserializationHelpers.TryCreateInstance(propertyType, edmPropertyType, elementType, out newCollection))
                {
                    // settable collections
                    collection.AddToCollection(newCollection, elementType, resourceType, propertyName, propertyType, context);
                    if (propertyType.IsArray)
                    {
                        newCollection = CollectionDeserializationHelpers.ToArray(newCollection, elementType);
                    }

                    SetProperty(resource, propertyName, newCollection);
                }
                else
                {
                    // get-only collections.
                    newCollection = GetProperty(resource, propertyName) as IEnumerable;
                    if (newCollection == null)
                    {
                        string message = Error.Format(SRResources.CannotAddToNullCollection, propertyName, resourceType.FullName);
                        throw new SerializationException(message);
                    }

                    if (clearCollection)
                    {
                        newCollection.Clear(propertyName, resourceType);
                    }

                    collection.AddToCollection(newCollection, elementType, resourceType, propertyName, propertyType, context);
                }
            }
        }

        internal static void SetDynamicCollectionProperty(object resource, string propertyName, object value,
            IEdmCollectionTypeReference edmPropertyType, IEdmStructuredType structuredType,
            IEdmModel model)
        {
            Contract.Assert(value != null);
            Contract.Assert(model != null);

            IEnumerable collection = value as IEnumerable;
            Contract.Assert(collection != null);

            Type resourceType = resource.GetType();
            Type elementType;
            Type propertyType;
            if (edmPropertyType.ElementType().IsUntyped())
            {
                elementType = typeof(object);
                propertyType = typeof(IList<object>);

                SetDynamicProperty(resource, propertyName, value, structuredType, model);
                return;
            }
            else
            {
                elementType = model.GetClrType(edmPropertyType.ElementType());
                propertyType = typeof(ICollection<>).MakeGenericType(elementType);
            }

            IEnumerable newCollection;
            if (CollectionDeserializationHelpers.TryCreateInstance(propertyType, edmPropertyType, elementType,
                out newCollection))
            {
                collection.AddToCollection(newCollection, elementType, resourceType, propertyName, propertyType);
                SetDynamicProperty(resource, propertyName, newCollection, structuredType, model);
            }
        }

        internal static void SetProperty(object resource, string propertyName, object value)
        {
            IDelta delta = resource as IDelta;
            if (delta == null)
            {
                resource.GetType().GetProperty(propertyName).SetValue(resource, value, index: null);
            }
            else
            {
                delta.TrySetPropertyValue(propertyName, value);
            }
        }

        internal static void SetDynamicProperty(object resource, string propertyName, object value,
            IEdmStructuredType structuredType, IEdmModel model)
        {
            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                delta.TrySetPropertyValue(propertyName, value);
            }
           // else if (resource is ODataObject oObject)
            else if (resource is EdmUntypedObject oObject)
            {
                oObject[propertyName] = value;
            }
            else
            {
                PropertyInfo propertyInfo = model.GetDynamicPropertyDictionary(structuredType);
                if (propertyInfo == null)
                {
                    return;
                }

                IDictionary<string, object> dynamicPropertyDictionary;
                object dynamicDictionaryObject = propertyInfo.GetValue(resource);
                if (dynamicDictionaryObject == null)
                {
                    if (!propertyInfo.CanWrite)
                    {
                        throw Error.InvalidOperation(SRResources.CannotSetDynamicPropertyDictionary, propertyName,
                            resource.GetType().FullName);
                    }

                    dynamicPropertyDictionary = new Dictionary<string, object>();
                    propertyInfo.SetValue(resource, dynamicPropertyDictionary);
                }
                else
                {
                    dynamicPropertyDictionary = (IDictionary<string, object>)dynamicDictionaryObject;
                }

                if (dynamicPropertyDictionary.ContainsKey(propertyName))
                {
                    throw Error.InvalidOperation(SRResources.DuplicateDynamicPropertyNameFound,
                        propertyName, structuredType.FullTypeName());
                }

                dynamicPropertyDictionary.Add(propertyName, value);
            }
        }

        internal static object ConvertValue(object oDataValue, ref IEdmTypeReference propertyType, IODataDeserializerProvider deserializerProvider,
            ODataDeserializerContext readContext, out EdmTypeKind typeKind)
        {
            if (oDataValue == null || oDataValue is ODataNullValue)
            {
                typeKind = EdmTypeKind.None;
                return null;
            }

            if (oDataValue is ODataEnumValue enumValue)
            {
                typeKind = EdmTypeKind.Enum;
                return ConvertEnumValue(enumValue, ref propertyType, deserializerProvider, readContext);
            }

            if (oDataValue is ODataCollectionValue collectionValue)
            {
                typeKind = EdmTypeKind.Collection;
                return ConvertCollectionValue(collectionValue, ref propertyType, deserializerProvider, readContext);
            }

            if (oDataValue is ODataResourceValue resourceValue)
            {
                return ConvertResourceValue(resourceValue, ref propertyType, deserializerProvider, readContext, out typeKind);
            }

            if (oDataValue is ODataUntypedValue untypedValue)
            {
                Contract.Assert(!String.IsNullOrEmpty(untypedValue.RawValue));

                if (untypedValue.RawValue.StartsWith("[", StringComparison.Ordinal) ||
                    untypedValue.RawValue.StartsWith("{", StringComparison.Ordinal))
                {
                    throw new ODataException(Error.Format(SRResources.InvalidODataUntypedValue, untypedValue.RawValue));
                }

                oDataValue = ConvertPrimitiveValue(untypedValue.RawValue);
            }

            if (oDataValue is ODataPrimitiveValue primitiveValue)
            {
                typeKind = EdmTypeKind.Primitive;
                return EdmPrimitiveHelper.ConvertPrimitiveValue(primitiveValue.Value, primitiveValue.Value.GetType());
            }

            typeKind = EdmTypeKind.Primitive;
            return oDataValue;
        }

        internal static Type GetPropertyType(object resource, string propertyName)
        {
            Contract.Assert(resource != null);
            Contract.Assert(propertyName != null);

            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                Type type;
                delta.TryGetPropertyType(propertyName, out type);
                return type;
            }
            else
            {
                PropertyInfo property = resource.GetType().GetProperty(propertyName);
                return property == null ? null : property.PropertyType;
            }
        }

        private static bool CanSetProperty(object resource, string propertyName)
        {
            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                return true;
            }
            else
            {
                PropertyInfo property = resource.GetType().GetProperty(propertyName);
                return property != null && property.GetSetMethod() != null;
            }
        }

        private static object GetProperty(object resource, string propertyName)
        {
            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                object value;
                delta.TryGetPropertyValue(propertyName, out value);
                return value;
            }
            else
            {
                PropertyInfo property = resource.GetType().GetProperty(propertyName);
                Contract.Assert(property != null, "ODataLib should have already verified that the property exists on the type.");
                return property.GetValue(resource, index: null);
            }
        }

        private static object ConvertCollectionValue(ODataCollectionValue collection,
            ref IEdmTypeReference valueType, IODataDeserializerProvider deserializerProvider,
            ODataDeserializerContext readContext)
        {
            // Be noted: If a declared property (propertyType != null) is untyped (or collection),
            // It should be never come here. Because for collection untyped, it goes to nested resource set.
            // ODL reads the value as ODataResourceSet in a ODataNestedResourceInfo.
            // So, if it comes here, the untyped value is odata.type annotated. for example, create a ODataProperty using ODataCollectionValue

            // 05/01/2024: new scenario, if we specify an instance annotation using the collection value as:
            // ""Magics@NS.StringCollection"":[""Skyline"",7,""Beaver""],
            // ODL generates 'ODataCollectionValue' without providing the 'TypeName' on ODataCollectionValue.
            // So the above assumption could not be correct again.
            IEdmCollectionTypeReference collectionType;
            if (valueType == null || valueType.IsUntyped())
            {
                string elementTypeName = GetCollectionElementTypeName(collection.TypeName, isNested: false);
                if (elementTypeName != null)
                {
                    IEdmModel model = readContext.Model;
                    IEdmSchemaType elementType = model.FindType(elementTypeName);
                    Contract.Assert(elementType != null);
                    collectionType =
                        new EdmCollectionTypeReference(
                            new EdmCollectionType(elementType.ToEdmTypeReference(isNullable: false)));
                }
                else
                {
                    // 05/01/2024: If we don't have the property type info meanwhile we don't have 'TypeName' on ODataCollectionValue,
                    // Let's use the "Collection(Edm.Untyped)" as the collection type.
                    collectionType = EdmUntypedHelpers.NullablePrimitiveUntypedCollectionReference;
                }

                valueType = collectionType;
            }
            else
            {
                collectionType = valueType as IEdmCollectionTypeReference;
                Contract.Assert(collectionType != null, "The type for collection must be a IEdmCollectionType.");
            }

            IODataEdmTypeDeserializer deserializer = deserializerProvider.GetEdmTypeDeserializer(collectionType);
            return deserializer.ReadInline(collection, collectionType, readContext);
        }

        private static object ConvertResourceValue(ODataResourceValue resourceValue,
            ref IEdmTypeReference valueType, IODataDeserializerProvider deserializerProvider,
            ODataDeserializerContext readContext, out EdmTypeKind typeKind)
        {
            ODataDeserializerContext nestedReadContext = new ODataDeserializerContext
            {
                Path = readContext.Path,
                Model = readContext.Model,
                Request = readContext.Request,
                TimeZone = readContext.TimeZone
            };

            IODataEdmTypeDeserializer deserializer;
            if (string.IsNullOrEmpty(resourceValue.TypeName))
            {
                // If we don't have the type name, treat it as untyped.
                valueType = EdmUntypedStructuredTypeReference.NullableTypeReference;
                nestedReadContext.ResourceType = typeof(EdmUntypedObject);
                deserializer = deserializerProvider.GetEdmTypeDeserializer(valueType);
                typeKind = EdmTypeKind.Complex;
                return deserializer.ReadInline(resourceValue, valueType, nestedReadContext);
            }

            // If we do have the type name, make sure we can resolve the type using type name
            IEdmType edmType = readContext.Model.FindType(resourceValue.TypeName);
            if (edmType == null)
            {
                throw new ODataException(Error.Format(SRResources.ResourceTypeNotInModel, resourceValue.TypeName));
            }

            valueType = edmType.ToEdmTypeReference(true);
            deserializer = deserializerProvider.GetEdmTypeDeserializer(valueType);

            IEdmStructuredTypeReference structuredType = valueType.AsStructured();
            typeKind = structuredType.IsEntity() ? EdmTypeKind.Entity : EdmTypeKind.Complex;
            Type clrType;
            if (readContext.IsNoClrType)
            {
                clrType = structuredType.IsEntity()
                    ? typeof(EdmEntityObject)
                    : typeof(EdmComplexObject);
            }
            else
            {
                clrType = readContext.Model.GetClrType(structuredType);
                if (clrType == null)
                {
                    throw new ODataException(
                        Error.Format(SRResources.MappingDoesNotContainResourceType, structuredType.FullName()));
                }
            }

            nestedReadContext.ResourceType = clrType;
            return deserializer.ReadInline(resourceValue, valueType, nestedReadContext);
        }

        private static object ConvertPrimitiveValue(string value)
        {
            if (String.CompareOrdinal(value, "null") == 0)
            {
                return null;
            }

            if (Int32.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int intValue))
            {
                return intValue;
            }

            // Todo: if it is Ieee754Compatible, parse decimal after double
            if (Decimal.TryParse(value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out decimal decimalValue))
            {
                return decimalValue;
            }

            if (Double.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out double doubleValue))
            {
                return doubleValue;
            }

            if (!value.StartsWith("\"", StringComparison.Ordinal) || !value.EndsWith("\"", StringComparison.Ordinal))
            {
                throw new ODataException(Error.Format(SRResources.InvalidODataUntypedValue, value));
            }

            return value.Substring(1, value.Length - 2);
        }

        private static object ConvertEnumValue(ODataEnumValue enumValue, ref IEdmTypeReference propertyType,
            IODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmEnumTypeReference edmEnumType;
            if (propertyType == null || propertyType.IsUntyped())
            {
                // dynamic enum property or untyped property
                Contract.Assert(!String.IsNullOrEmpty(enumValue.TypeName),
                    "ODataLib should have verified that dynamic enum value has a type name since we provided metadata.");
                IEdmModel model = readContext.Model;
                IEdmType edmType = model.FindType(enumValue.TypeName);
                Contract.Assert(edmType.TypeKind == EdmTypeKind.Enum, "ODataLib should have verified that enum value has a enum resource type.");
                edmEnumType = new EdmEnumTypeReference(edmType as IEdmEnumType, isNullable: true);
                propertyType = edmEnumType;
            }
            else
            {
                edmEnumType = propertyType.AsEnum();
            }

            IODataEdmTypeDeserializer deserializer = deserializerProvider.GetEdmTypeDeserializer(edmEnumType);
            return deserializer.ReadInline(enumValue, propertyType, readContext);
        }

        // The same logic from ODL to get the element type name in a collection.
        internal static string GetCollectionElementTypeName(string typeName, bool isNested)
        {
            const string CollectionTypeQualifier = "Collection";
            int collectionTypeQualifierLength = CollectionTypeQualifier.Length;

            // A collection type name must not be null, it has to start with "Collection(" and end with ")"
            // and must not be "Collection()"
            if (typeName != null &&
                typeName.StartsWith(CollectionTypeQualifier + "(", StringComparison.Ordinal) &&
                typeName[typeName.Length - 1] == ')' &&
                typeName.Length != collectionTypeQualifierLength + 2)
            {
                if (isNested)
                {
                    throw new ODataException(Error.Format(SRResources.NestedCollectionsNotSupported, typeName));
                }

                string innerTypeName = typeName.Substring(collectionTypeQualifierLength + 1,
                    typeName.Length - (collectionTypeQualifierLength + 2));

                // Check if it is not a nested collection and throw if it is
                GetCollectionElementTypeName(innerTypeName, true);

                return innerTypeName;
            }

            return null;
        }
    }
}
