// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// Expose functionality to convert an function parameter value into a CLR object.
    /// </summary>
    internal static class ODataModelBinderConverter
    {
        private static readonly MethodInfo EnumTryParseMethod = typeof(Enum).GetMethods()
            .Single(m => m.Name == "TryParse" && m.GetParameters().Length == 2);

        private static readonly MethodInfo CastMethodInfo = typeof(Enumerable).GetMethod("Cast");

        /// <summary>
        /// Convert an OData value into a CLR object.
        /// </summary>
        /// <param name="graph">The given object.</param>
        /// <param name="edmTypeReference">The EDM type of the given object.</param>
        /// <param name="clrType">The CLR type of the given object.</param>
        /// <param name="parameterName">The parameter name of the given object.</param>
        /// <param name="readContext">The <see cref="ODataDeserializerContext"/> use to convert.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <returns>The converted object.</returns>
        public static object Convert(object graph, IEdmTypeReference edmTypeReference,
            Type clrType, string parameterName, ODataDeserializerContext readContext,
            IServiceProvider requestContainer)
        {
            if (graph == null || graph is ODataNullValue)
            {
                return null;
            }

            // collection of primitive, enum
            ODataCollectionValue collectionValue = graph as ODataCollectionValue;
            if (collectionValue != null)
            {
                return ConvertCollection(collectionValue, edmTypeReference, clrType, parameterName, readContext,
                    requestContainer);
            }

            // enum value
            ODataEnumValue enumValue = graph as ODataEnumValue;
            if (enumValue != null)
            {
                IEdmEnumTypeReference edmEnumType = edmTypeReference.AsEnum();
                Contract.Assert(edmEnumType != null);

                ODataDeserializerProvider deserializerProvider =
                    requestContainer.GetRequiredService<ODataDeserializerProvider>();

                ODataEnumDeserializer deserializer =
                    (ODataEnumDeserializer)deserializerProvider.GetEdmTypeDeserializer(edmEnumType);

                return deserializer.ReadInline(enumValue, edmEnumType, readContext);
            }

            // primitive value
            if (edmTypeReference.IsPrimitive())
            {
                ConstantNode node = graph as ConstantNode;
                return EdmPrimitiveHelper.ConvertPrimitiveValue(node != null ? node.Value : graph, clrType, readContext?.TimeZone);
            }

            // Resource, ResourceSet, Entity Reference or collection of entity reference
            return ConvertResourceOrResourceSet(graph, edmTypeReference, readContext);
        }

        internal static object ConvertTo(string valueString, Type type, TimeZoneInfo timeZone)
        {
            if (valueString == null)
            {
                return null;
            }

            if (TypeHelper.IsNullable(type) && String.Equals(valueString, "null", StringComparison.Ordinal))
            {
                return null;
            }

            // TODO 1668: ODL beta1's ODataUriUtils.ConvertFromUriLiteral does not support converting uri literal
            // to ODataEnumValue, but beta1's ODataUriUtils.ConvertToUriLiteral supports converting ODataEnumValue
            // to uri literal.
            if (TypeHelper.IsEnum(type))
            {
                string[] values = valueString.Split(new[] { '\'' }, StringSplitOptions.None);
                if (values.Length == 3 && String.IsNullOrEmpty(values[2]))
                {
                    // Remove the type name if the enum value is a fully qualified literal.
                    valueString = values[1];
                }

                Type enumType = TypeHelper.GetUnderlyingTypeOrSelf(type);
                object[] parameters = new[] { valueString, Enum.ToObject(enumType, 0) };
                bool isSuccessful = (bool)EnumTryParseMethod.MakeGenericMethod(enumType).Invoke(null, parameters);

                if (!isSuccessful)
                {
                    throw Error.InvalidOperation(SRResources.ModelBinderUtil_ValueCannotBeEnum, valueString, type.Name);
                }

                return parameters[1];
            }

            // The logic of "public static object ConvertFromUriLiteral(string value, ODataVersion version);" treats
            // the date value string (for example: 2015-01-02) as DateTimeOffset literal, and return a DateTimeOffset
            // object. However, the logic of
            // "object ConvertFromUriLiteral(string value, ODataVersion version, IEdmModel model, IEdmTypeReference typeReference);"
            // can return the correct Date object.
            if (type == typeof(Date) || type == typeof(Date?))
            {
                EdmCoreModel model = EdmCoreModel.Instance;
                IEdmPrimitiveTypeReference dateTypeReference = type.GetEdmPrimitiveTypeReference();
                return ODataUriUtils.ConvertFromUriLiteral(valueString, ODataVersion.V4, model, dateTypeReference);
            }

            object value;
            try
            {
                value = ODataUriUtils.ConvertFromUriLiteral(valueString, ODataVersion.V4);
            }
            catch
            {
                if (type == typeof(string))
                {
                    return valueString;
                }

                throw;
            }

            bool isNonStandardEdmPrimitive;
            type.IsNonstandardEdmPrimitive(out isNonStandardEdmPrimitive);

            if (isNonStandardEdmPrimitive)
            {
                // shall we get the timezone?
                return EdmPrimitiveHelper.ConvertPrimitiveValue(value, type, timeZone);
            }
            else
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                return System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
        }

        private static object ConvertCollection(ODataCollectionValue collectionValue,
            IEdmTypeReference edmTypeReference, Type clrType, string parameterName,
            ODataDeserializerContext readContext, IServiceProvider requestContainer)
        {
            Contract.Assert(collectionValue != null);

            IEdmCollectionTypeReference collectionType = edmTypeReference as IEdmCollectionTypeReference;
            Contract.Assert(collectionType != null);

            ODataDeserializerProvider deserializerProvider =
                requestContainer.GetRequiredService<ODataDeserializerProvider>();
            ODataCollectionDeserializer deserializer =
                (ODataCollectionDeserializer)deserializerProvider.GetEdmTypeDeserializer(collectionType);

            object value = deserializer.ReadInline(collectionValue, collectionType, readContext);
            if (value == null)
            {
                return null;
            }

            IEnumerable collection = value as IEnumerable;
            Contract.Assert(collection != null);

            Type elementType;
            if (!TypeHelper.IsCollection(clrType, out elementType))
            {
                // EdmEntityCollectionObject and EdmComplexCollectionObject are collection types.
                throw new ODataException(String.Format(CultureInfo.InvariantCulture,
                    SRResources.ParameterTypeIsNotCollection, parameterName, clrType));
            }

            IEnumerable newCollection;
            if (CollectionDeserializationHelpers.TryCreateInstance(clrType, collectionType, elementType,
                out newCollection))
            {
                collection.AddToCollection(newCollection, elementType, parameterName, clrType);
                if (clrType.IsArray)
                {
                    newCollection = CollectionDeserializationHelpers.ToArray(newCollection, elementType);
                }

                return newCollection;
            }

            return null;
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        internal static object ConvertResourceOrResourceSet(object oDataValue, IEdmTypeReference edmTypeReference,
            ODataDeserializerContext readContext)
        {
            string valueString = oDataValue as string;
            Contract.Assert(valueString != null);

            if (edmTypeReference.IsNullable && String.Equals(valueString, "null", StringComparison.Ordinal))
            {
                return null;
            }

            HttpRequest request = readContext.Request;
            ODataMessageReaderSettings oDataReaderSettings = request.GetReaderSettings();

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(valueString)))
            {
                stream.Seek(0, SeekOrigin.Begin);

                // Do we need to dispose it?
                IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(stream, null, request.GetODataContentIdMapping());
                using (
                    ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage,
                        oDataReaderSettings, readContext.Model))
                {
                    if (edmTypeReference.IsCollection())
                    {
                        return ConvertResourceSet(oDataMessageReader, edmTypeReference, readContext);
                    }
                    else
                    {
                        return ConvertResource(oDataMessageReader, edmTypeReference, readContext);
                    }
                }
            }
        }

        private static object ConvertResourceSet(ODataMessageReader oDataMessageReader,
            IEdmTypeReference edmTypeReference, ODataDeserializerContext readContext)
        {
            IEdmCollectionTypeReference collectionType = edmTypeReference.AsCollection();

            EdmEntitySet tempEntitySet = null;
            if (collectionType.ElementType().IsEntity())
            {
                tempEntitySet = new EdmEntitySet(readContext.Model.EntityContainer, "temp",
                    collectionType.ElementType().AsEntity().EntityDefinition());
            }

            // TODO: Sam xu, can we use the parameter-less overload
            ODataReader odataReader = oDataMessageReader.CreateODataUriParameterResourceSetReader(tempEntitySet,
                collectionType.ElementType().AsStructured().StructuredDefinition());
            ODataResourceSetWrapper resourceSet =
                odataReader.ReadResourceOrResourceSet() as ODataResourceSetWrapper;

            ODataDeserializerProvider deserializerProvider = readContext.Request.GetDeserializerProvider();

            ODataResourceSetDeserializer resourceSetDeserializer =
                (ODataResourceSetDeserializer)deserializerProvider.GetEdmTypeDeserializer(collectionType);

            object result = resourceSetDeserializer.ReadInline(resourceSet, collectionType, readContext);
            IEnumerable enumerable = result as IEnumerable;
            if (enumerable != null)
            {
                if (readContext.IsNoClrType)
                {
                    return enumerable.ConvertToEdmObject(collectionType);
                }
                else
                {
                    IEdmTypeReference elementTypeReference = collectionType.ElementType();

                    Type elementClrType = readContext.Model.GetClrType(elementTypeReference);
                    IEnumerable castedResult =
                        CastMethodInfo.MakeGenericMethod(elementClrType)
                            .Invoke(null, new object[] { enumerable }) as IEnumerable;
                    return castedResult;
                }
            }

            return null;
        }

        private static object ConvertResource(ODataMessageReader oDataMessageReader, IEdmTypeReference edmTypeReference,
            ODataDeserializerContext readContext)
        {
            EdmEntitySet tempEntitySet = null;
            if (edmTypeReference.IsEntity())
            {
                IEdmEntityTypeReference entityType = edmTypeReference.AsEntity();
                tempEntitySet = new EdmEntitySet(readContext.Model.EntityContainer, "temp",
                    entityType.EntityDefinition());
            }

            // TODO: Sam xu, can we use the parameter-less overload
            ODataReader resourceReader = oDataMessageReader.CreateODataUriParameterResourceReader(tempEntitySet,
                edmTypeReference.ToStructuredType());

            object item = resourceReader.ReadResourceOrResourceSet();

            ODataResourceWrapper topLevelResource = item as ODataResourceWrapper;
            Contract.Assert(topLevelResource != null);

            ODataDeserializerProvider deserializerProvider = readContext.Request.GetDeserializerProvider();

            ODataResourceDeserializer entityDeserializer =
                (ODataResourceDeserializer)deserializerProvider.GetEdmTypeDeserializer(edmTypeReference);
            return entityDeserializer.ReadInline(topLevelResource, edmTypeReference, readContext);
        }
    }
}
