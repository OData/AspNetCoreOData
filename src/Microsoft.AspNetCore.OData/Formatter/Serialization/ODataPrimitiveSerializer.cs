//-----------------------------------------------------------------------------
// <copyright file="ODataPrimitiveSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing <see cref="IEdmPrimitiveType" />'s.
    /// </summary>
    public class ODataPrimitiveSerializer : ODataEdmTypeSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataPrimitiveSerializer"/>.
        /// </summary>
        public ODataPrimitiveSerializer()
            : base(ODataPayloadKind.Property)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataPrimitiveSerializer"/>.
        /// </summary>
        public ODataPrimitiveSerializer(IODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.Property, serializerProvider)
        {
        }

        /// <inheritdoc/>
        public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull(nameof(messageWriter));
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull(nameof(writeContext));
            }

            if (writeContext.RootElementName == null)
            {
                throw Error.Argument("writeContext", SRResources.RootElementNameMissing, typeof(ODataSerializerContext).Name);
            }

            IEdmTypeReference edmType = writeContext.GetEdmType(graph, type);
            Contract.Assert(edmType != null);

            ODataProperty property = this.CreateProperty(graph, edmType, writeContext.RootElementName, writeContext);

            if (writeContext.InstanceAnnotations != null)
            {
                ODataSerializerHelper.AppendInstanceAnnotations(writeContext.InstanceAnnotations,
                    property.InstanceAnnotations, writeContext, SerializerProvider);
            }

            await messageWriter.WritePropertyAsync(property).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public sealed override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
        {
            if (!expectedType.IsPrimitive())
            {
                throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataPrimitiveSerializer), expectedType.FullName());
            }

            ODataPrimitiveValue value = CreateODataPrimitiveValue(graph, expectedType.AsPrimitive(), writeContext);
            if (value == null)
            {
                return ODataNullValueExtensions.NullValue;
            }

            return value;
        }

        /// <summary>
        /// Creates an <see cref="ODataPrimitiveValue"/> for the object represented by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The primitive value.</param>
        /// <param name="primitiveType">The EDM primitive type of the value.</param>
        /// <param name="writeContext">The serializer write context.</param>
        /// <returns>The created <see cref="ODataPrimitiveValue"/>.</returns>
        public virtual ODataPrimitiveValue CreateODataPrimitiveValue(object graph, IEdmPrimitiveTypeReference primitiveType,
            ODataSerializerContext writeContext)
        {
            return CreatePrimitive(graph, primitiveType, writeContext);
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataPrimitiveValue primitive, IEdmPrimitiveTypeReference primitiveType,
            ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).
            Contract.Assert(primitive != null);

            object value = primitive.Value;
            string typeName = null; // Set null to force the type name not to serialize.

            // Provide the type name to serialize.
            if (!ShouldSuppressTypeNameSerialization(value, metadataLevel))
            {
                typeName = primitiveType.FullName();
            }

            if (typeName != null)
            {
                primitive.TypeAnnotation = new ODataTypeAnnotation(typeName);
            }
        }

        internal static ODataPrimitiveValue CreatePrimitive(object value, IEdmPrimitiveTypeReference primitiveType,
            ODataSerializerContext writeContext)
        {
            if (value == null)
            {
                return null;
            }

            object supportedValue = ConvertPrimitiveValue(value, primitiveType, writeContext?.TimeZone);
            ODataPrimitiveValue primitive = new ODataPrimitiveValue(supportedValue);

            if (writeContext != null)
            {
                AddTypeNameAnnotationAsNeeded(primitive, primitiveType, writeContext.MetadataLevel);
            }

            return primitive;
        }

        internal static object ConvertPrimitiveValue(object value, IEdmPrimitiveTypeReference primitiveType, TimeZoneInfo timeZoneInfo)
        {
            if (value == null)
            {
                return null;
            }

            Type type = value.GetType();

            // Return values for supported primitive values. 
            if (type == typeof(string)
                || type == typeof(int)
                || type == typeof(bool)
                || type == typeof(double)
                || type == typeof(Guid))
            {
                return value;
            }

            if (primitiveType != null && primitiveType.IsDate() && TypeHelper.IsDateTime(type))
            {
                Date dt = (DateTime)value;
                return dt;
            }

            if (primitiveType != null && primitiveType.IsTimeOfDay() && TypeHelper.IsTimeSpan(type))
            {
                TimeOfDay tod = (TimeSpan)value;
                return tod;
            }

#if NET6_0
            // Since ODL doesn't support "DateOnly", we have to use Date defined in ODL.
            if (primitiveType != null && primitiveType.IsDate() && TypeHelper.IsDateOnly(type))
            {
                DateOnly dateOnly = (DateOnly)value;
                return new Date(dateOnly.Year, dateOnly.Month, dateOnly.Day);
            }

            // Since ODL doesn't support "TimeOnly", we have to use TimeOfDay defined in ODL.
            if (primitiveType != null && primitiveType.IsTimeOfDay() && TypeHelper.IsTimeOnly(type))
            {
                TimeOnly timeOnly = (TimeOnly)value;
                return new TimeOfDay(timeOnly.Hour, timeOnly.Minute, timeOnly.Second, timeOnly.Millisecond);
            }
#endif

            return ConvertUnsupportedPrimitives(value, timeZoneInfo);
        }

        internal static object ConvertUnsupportedPrimitives(object value, TimeZoneInfo timeZoneInfo)
        {
            if (value != null)
            {
                Type type = value.GetType();

                // Note that type cannot be a nullable type as value is not null and it is boxed.
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                        return new string((char)value, 1);

                    case TypeCode.UInt16:
                        return (int)(ushort)value;

                    case TypeCode.UInt32:
                        return (long)(uint)value;

                    case TypeCode.UInt64:
                        return checked((long)(ulong)value);

                    case TypeCode.DateTime:
                        DateTime dateTime = (DateTime)value;
                        return TimeZoneInfoHelper.ConvertToDateTimeOffset(dateTime, timeZoneInfo);

                    default:
                        if (type == typeof(char[]))
                        {
                            return new string(value as char[]);
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

        internal static bool CanTypeBeInferredInJson(object value)
        {
            Contract.Assert(value != null);

            TypeCode typeCode = Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                // The type for a Boolean, Int32 or String can always be inferred in JSON.
                case TypeCode.Boolean:
                case TypeCode.Int32:
                case TypeCode.String:
                    return true;
                // The type for a Double can be inferred in JSON ...
                case TypeCode.Double:
                    double doubleValue = (double)value;
                    // ... except for NaN or Infinity (positive or negative).
                    if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                default:
                    return false;
            }
        }

        internal static bool ShouldSuppressTypeNameSerialization(object value, ODataMetadataLevel metadataLevel)
        {
            // For dynamic properties in minimal metadata level, the type name always appears as declared property.
            if (metadataLevel != ODataMetadataLevel.Full)
            {
                return true;
            }

            return CanTypeBeInferredInJson(value);
        }
    }
}
