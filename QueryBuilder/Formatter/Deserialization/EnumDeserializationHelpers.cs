using System;
using System.ComponentModel.DataAnnotations;
using ODataQueryBuilder.Common;
using Microsoft.OData;

namespace ODataQueryBuilder.Formatter.Deserialization
{
    internal static class EnumDeserializationHelpers
    {
        public static object ConvertEnumValue(object value, Type type)
        {
            if (value == null)
            {
                throw Error.ArgumentNull(nameof(value));
            }

            if (type == null)
            {
                throw Error.ArgumentNull(nameof(type));
            }

            Type enumType = TypeHelper.GetUnderlyingTypeOrSelf(type);

            // if value is of the requested type nothing to do here.
            if (value.GetType() == enumType)
            {
                return value;
            }

            ODataEnumValue enumValue = value as ODataEnumValue;

            if (enumValue == null)
            {
                throw new ValidationException(Error.Format(SRResources.PropertyMustBeEnum, value.GetType().Name, "ODataEnumValue"));
            }

            if (!TypeHelper.IsEnum(enumType))
            {
                throw Error.InvalidOperation(Error.Format(SRResources.TypeMustBeEnumOrNullableEnum, type.Name));
            }

            return Enum.Parse(enumType, enumValue.Value);
        }
    }
}
