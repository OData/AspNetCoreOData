//-----------------------------------------------------------------------------
// <copyright file="ODataEnumSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization;

/// <summary>
/// Represents an <see cref="ODataSerializer"/> for serializing <see cref="IEdmEnumType" />'s.
/// </summary>
public class ODataEnumSerializer : ODataEdmTypeSerializer
{
    /// <summary>
    /// Initializes a new instance of <see cref="ODataEnumSerializer"/>.
    /// </summary>
    public ODataEnumSerializer(IODataSerializerProvider serializerProvider)
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
            throw Error.Argument(nameof(writeContext), SRResources.RootElementNameMissing, typeof(ODataSerializerContext).Name);
        }

        IEdmTypeReference edmType = writeContext.GetEdmType(graph, type);
        Contract.Assert(edmType != null);

        await messageWriter.WritePropertyAsync(this.CreateProperty(graph, edmType, writeContext.RootElementName, writeContext)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public sealed override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
    {
        if (!expectedType.IsEnum())
        {
            throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataEnumSerializer).Name, expectedType.FullName());
        }

        ODataEnumValue value = CreateODataEnumValue(graph, expectedType.AsEnum(), writeContext);
        if (value == null)
        {
            return ODataNullValueExtensions.NullValue;
        }

        return value;
    }

    /// <summary>
    /// Creates an <see cref="ODataEnumValue"/> for the object represented by <paramref name="graph"/>.
    /// </summary>
    /// <param name="graph">The enum value.</param>
    /// <param name="enumType">The EDM enum type of the value.</param>
    /// <param name="writeContext">The serializer write context.</param>
    /// <returns>The created <see cref="ODataEnumValue"/>.</returns>
    public virtual ODataEnumValue CreateODataEnumValue(object graph, IEdmEnumTypeReference enumType,
        ODataSerializerContext writeContext)
    {
        if (graph == null)
        {
            return null;
        }

        string value = null;
        if (TypeHelper.IsEnum(graph.GetType()))
        {
            value = graph.ToString();
        }
        else
        {
            if (graph.GetType() == typeof(EdmEnumObject))
            {
                value = ((EdmEnumObject)graph).Value;
            }
        }

        // Enum member supports model alias case. So, try to use the Edm member name to create Enum value.
        var memberMapAnnotation = writeContext?.Model.GetClrEnumMemberAnnotation(enumType.EnumDefinition());
        if (memberMapAnnotation != null)
        {
            Enum graphEnum = (Enum)graph;

            var edmEnumMember = memberMapAnnotation.GetEdmEnumMember(graphEnum);
            if (edmEnumMember != null)
            {
                value = edmEnumMember.Name;
            }
            // If the enum is a flags enum, we need to handle the case where multiple flags are set
            else if (enumType.EnumDefinition().IsFlags)
            {
                value = GetFlagsEnumValue(graphEnum, memberMapAnnotation);
            }
        }

        ODataEnumValue enumValue = new ODataEnumValue(value, enumType.FullName());

        ODataMetadataLevel metadataLevel = writeContext != null ? writeContext.MetadataLevel : ODataMetadataLevel.Minimal;
        AddTypeNameAnnotationAsNeeded(enumValue, enumType, metadataLevel);

        return enumValue;
    }

    internal static void AddTypeNameAnnotationAsNeeded(ODataEnumValue enumValue, IEdmEnumTypeReference enumType, ODataMetadataLevel metadataLevel)
    {
        // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
        // null when values should not be serialized. The TypeName property is different and should always be
        // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
        // to serialize the type name (a null value prevents serialization).

        Contract.Assert(enumValue != null);

        // Only add an annotation if we want to override ODataLib's default type name serialization behavior.
        if (ShouldAddTypeNameAnnotation(metadataLevel))
        {
            string typeName;

            // Provide the type name to serialize (or null to force it not to serialize).
            if (ShouldSuppressTypeNameSerialization(metadataLevel))
            {
                typeName = null;
            }
            else
            {
                typeName = enumType.FullName();
            }

            enumValue.TypeAnnotation = new ODataTypeAnnotation(typeName);
        }
    }

    private static bool ShouldAddTypeNameAnnotation(ODataMetadataLevel metadataLevel)
    {
        switch (metadataLevel)
        {
            case ODataMetadataLevel.Minimal:
                return false;
            case ODataMetadataLevel.Full:
            case ODataMetadataLevel.None:
            default:
                return true;
        }
    }

    private static bool ShouldSuppressTypeNameSerialization(ODataMetadataLevel metadataLevel)
    {
        Contract.Assert(metadataLevel != ODataMetadataLevel.Minimal);

        switch (metadataLevel)
        {
            case ODataMetadataLevel.None:
                return true;
            case ODataMetadataLevel.Full:
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the combined names of the flags set in a Flags enum value.
    /// </summary>
    /// <param name="graphEnum">The enum value.</param>
    /// <param name="memberMapAnnotation">The annotation containing the mapping of CLR enum members to EDM enum members.</param>
    /// <returns>A comma-separated string of the names of the flags that are set.</returns>
    private static string GetFlagsEnumValue(Enum graphEnum, ClrEnumMemberAnnotation memberMapAnnotation)
    {
        List<string> flagsList = new List<string>();

        // Convert the enum value to a long for bitwise operations
        long graphValue = Convert.ToInt64(graphEnum);

        // Iterate through all enum values
        foreach (Enum flag in Enum.GetValues(graphEnum.GetType()))
        {
            // Convert the current flag to a long
            long flagValue = Convert.ToInt64(flag);

            // Using bitwise operations to check if a flag is set, which is more efficient than Enum.HasFlag
            if (flagValue != 0 && (graphValue & flagValue) == flagValue)
            {
                IEdmEnumMember flagMember = memberMapAnnotation.GetEdmEnumMember(flag);
                if (flagMember != null)
                {
                    flagsList.Add(flagMember.Name);
                }
            }
        }

        return string.Join(", ", flagsList);
    }
}
