//-----------------------------------------------------------------------------
// <copyright file="ODataEnumDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization;

/// <summary>
/// Represents an <see cref="ODataDeserializer"/> that can read OData enum types.
/// </summary>
public class ODataEnumDeserializer : ODataEdmTypeDeserializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataEnumDeserializer"/> class.
    /// </summary>
    public ODataEnumDeserializer()
        : base(ODataPayloadKind.Property)
    {
    }

    /// <inheritdoc />
    public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
    {
        if (messageReader == null)
        {
            throw Error.ArgumentNull(nameof(messageReader));
        }

        if (type == null)
        {
            throw Error.ArgumentNull(nameof(type));
        }

        if (readContext == null)
        {
            throw Error.ArgumentNull(nameof(readContext));
        }

        IEdmTypeReference edmType = readContext.GetEdmType(type);
        Contract.Assert(edmType != null);

        ODataProperty property = await messageReader.ReadPropertyAsync(edmType).ConfigureAwait(false);
        return ReadInline(property, edmType, readContext);
    }

    /// <inheritdoc />
    public override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
    {
        if (item == null)
        {
            return null;
        }

        if (readContext == null)
        {
            throw Error.ArgumentNull(nameof(readContext));
        }

        ODataProperty property = item as ODataProperty;
        if (property != null)
        {
            item = property.Value;
        }

        IEdmEnumTypeReference enumTypeReference = edmType.AsEnum();
        ODataEnumValue enumValue = item as ODataEnumValue;
        if (readContext.IsNoClrType)
        {
            Contract.Assert(edmType.TypeKind() == EdmTypeKind.Enum);
            return new EdmEnumObject(enumTypeReference, enumValue.Value);
        }

        IEdmEnumType enumType = enumTypeReference.EnumDefinition();

        Type clrType = readContext.Model.GetClrType(edmType);

        // Enum member supports model alias case. So, try to use the Edm member name to retrieve the Enum value.
        var memberMapAnnotation = readContext.Model.GetClrEnumMemberAnnotation(enumType);
        if (memberMapAnnotation != null)
        {
            if (enumValue != null)
            {
                IEdmEnumMember enumMember = enumType.Members.FirstOrDefault(m => m.Name == enumValue.Value);
                if (enumMember != null)
                {
                    var clrMember = memberMapAnnotation.GetClrEnumMember(enumMember);
                    if (clrMember != null)
                    {
                        return clrMember;
                    }
                }
                else if (enumType.IsFlags)
                {
                    var result = ReadFlagsEnumValue(enumValue, enumType, clrType, memberMapAnnotation);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
        }

        return EnumDeserializationHelpers.ConvertEnumValue(item, clrType);
    }

    /// <summary>
    /// Reads the value of a flags enum.
    /// </summary>
    /// <param name="enumValue">The OData enum value.</param>
    /// <param name="enumType">The EDM enum type.</param>
    /// <param name="clrType">The EDM enum CLR type.</param>
    /// <param name="memberMapAnnotation">The annotation containing the mapping of CLR enum members to EDM enum members.</param>
    /// <returns>The deserialized flags enum value.</returns>
    private static object ReadFlagsEnumValue(ODataEnumValue enumValue, IEdmEnumType enumType, Type clrType, ClrEnumMemberAnnotation memberMapAnnotation)
    {
        long result = 0;
        clrType = TypeHelper.GetUnderlyingTypeOrSelf(clrType);

        ReadOnlySpan<char> source = enumValue.Value.AsSpan().Trim();
        int start = 0;
        while (start < source.Length)
        {
            // Find the end of the current value.
            int end = start;
            while (end < source.Length && source[end] != ',')
            {
                end++;
            }

            // Extract the current value.
            ReadOnlySpan<char> currentValue = source[start..end].Trim();

            bool parsed = Enum.TryParse(clrType, currentValue, true, out object enumMemberParsed);
            if (parsed)
            {
                result |= Convert.ToInt64((Enum)enumMemberParsed);
            }
            else
            {
                // If the value is not a valid enum member, try to match it with the EDM enum member name.
                // This is needed for model alias case.
                // For example,
                // - if the enum member is defined as "Friday" and the value is "fri", we need to match them.
                // - if the enum member is defined as "FullTime" and the value is "Full Time", we need to match them.
                // - if the enum member is defined as "PartTime" and the value is "part time", we need to match them.
                parsed = false;
                foreach (IEdmEnumMember enumMember in enumType.Members)
                {
                    // Check if the current value matches the enum member name.
                    parsed = currentValue.Equals(enumMember.Name.AsSpan(), StringComparison.InvariantCultureIgnoreCase);
                    if (parsed)
                    {
                        Enum clrEnumMember = memberMapAnnotation.GetClrEnumMember(enumMember);
                        result |= Convert.ToInt64(clrEnumMember);
                        break;
                    }
                }
            }

            // If still not valid, return null.
            if (!parsed)
            {
                return null;
            }

            // Move to the next value.
            start = end + 1;
        }

        return result == 0 ? null : Enum.ToObject(clrType, result);
    }
}
