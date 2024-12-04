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
using Microsoft.AspNetCore.OData.Extensions;
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

        bool enablePropertyNameCaseInsensitive = 
            readContext.Request != null &&
            readContext.Request.ODataOptions() != null &&
            readContext.Request.ODataOptions().RouteOptions != null &&
            readContext.Request.ODataOptions().RouteOptions.EnablePropertyNameCaseInsensitive;

        IEdmEnumType enumType = enumTypeReference.EnumDefinition();

        Type clrType = readContext.Model.GetClrType(edmType);

        // Enum member supports model alias case. So, try to use the Edm member name to retrieve the Enum value.
        var memberMapAnnotation = readContext.Model.GetClrEnumMemberAnnotation(enumType);
        if (memberMapAnnotation != null)
        {
            if (enumValue != null)
            {
                IEdmEnumMember enumMember = GetEnumMember(enumType, enumValue.Value, enablePropertyNameCaseInsensitive);

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
                    var result = ReadFlagsEnumValue(enumValue, enumType, clrType, enablePropertyNameCaseInsensitive, memberMapAnnotation);
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
    /// <param name="enablePropertyNameCaseInsensitive">The value indicating whether to enable case insensitive for the property name in conventional routing.</param>
    /// <param name="memberMapAnnotation">The annotation containing the mapping of CLR enum members to EDM enum members.</param>
    /// <returns>The deserialized flags enum value.</returns>
    private object ReadFlagsEnumValue(ODataEnumValue enumValue, IEdmEnumType enumType, Type clrType, bool enablePropertyNameCaseInsensitive, ClrEnumMemberAnnotation memberMapAnnotation)
    {
        long result = 0;
        Type clrEnumType = TypeHelper.GetUnderlyingTypeOrSelf(clrType);

        // For flags enum, we need to split the value and convert it to the enum value.
        string[] values = enumValue.Value.Split(',');
        foreach (string value in values)
        {
            Enum clrEnumMember = null;
            IEdmEnumMember enumMember = GetEnumMember(enumType, value.Trim(), enablePropertyNameCaseInsensitive);

            if (enumMember != null)
            {
                clrEnumMember = memberMapAnnotation.GetClrEnumMember(enumMember);
            }
            else if (Enum.TryParse(clrEnumType, value, true, out var enumMemberParsed))
            {
                clrEnumMember = (Enum)enumMemberParsed;
            }
            else
            {
                return null;
            }

            if (clrEnumMember != null)
            {
                result |= Convert.ToInt64(clrEnumMember);
            }
        }

        return result == 0 ? null : Enum.ToObject(clrEnumType, result);
    }

    private IEdmEnumMember GetEnumMember(IEdmEnumType enumType, string value, bool enablePropertyNameCaseInsensitive)
    {
        StringComparison comparison = enablePropertyNameCaseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        return enumType.Members.FirstOrDefault(m => m.Name.Equals(value, comparison));
    }
}
