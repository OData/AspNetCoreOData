//-----------------------------------------------------------------------------
// <copyright file="EdmPrimitiveHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm
{
    internal static class EdmPrimitiveHelper
    {
        public static object ConvertPrimitiveValue(object value, Type type)
        {
            return ConvertPrimitiveValue(value, type, timeZoneInfo: null);
        }

        public static object ConvertPrimitiveValue(object value, Type type, TimeZoneInfo timeZoneInfo)
        {
            Contract.Assert(value != null);
            Contract.Assert(type != null);

            // if value is of the same type nothing to do here.
            if (value.GetType() == type || value.GetType() == Nullable.GetUnderlyingType(type))
            {
                return value;
            }

            if (type.IsInstanceOfType(value))
            {
                return value;
            }

            string str = value as string;

            if (type == typeof(char))
            {
                if (str == null || str.Length != 1)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeStringLengthOne));
                }

                return str[0];
            }
            else if (type == typeof(char?))
            {
                if (str == null || str.Length > 1)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeStringMaxLengthOne));
                }

                return str.Length > 0 ? str[0] : (char?)null;
            }
            else if (type == typeof(char[]))
            {
                if (str == null)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                }

                return str.ToCharArray();
            }
            else if (type == typeof(XElement))
            {
                if (str == null)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                }

                return XElement.Parse(str);
            }
            else
            {
                type = Nullable.GetUnderlyingType(type) ?? type;

                // Convert.ChangeType invalid cast from 'System.String' to 'System.Guid'
                if (type == typeof(Guid))
                {
                    if (str == null)
                    {
                        throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                    }

                    return Guid.Parse(str);
                }
                else if (TypeHelper.IsEnum(type))
                {
                    if (str == null)
                    {
                        throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                    }

                    return Enum.Parse(type, str);
                }
                else if (type == typeof(DateTime))
                {
                    if (value is DateTimeOffset)
                    {
                        DateTimeOffset dateTimeOffsetValue = (DateTimeOffset)value;
                        TimeZoneInfo timeZone = timeZoneInfo ?? TimeZoneInfo.Local;
                        dateTimeOffsetValue = TimeZoneInfo.ConvertTime(dateTimeOffsetValue, timeZone);
                        return dateTimeOffsetValue.DateTime;
                    }

                    if (value is Date)
                    {
                        Date dt = (Date)value;
                        return (DateTime)dt;
                    }

                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeDateTimeOffsetOrDate));
                }
                else if (type == typeof(TimeSpan))
                {
                    if (value is TimeOfDay)
                    {
                        TimeOfDay tod = (TimeOfDay)value;
                        return (TimeSpan)tod;
                    }

                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeTimeOfDay));
                }
                else if (type == typeof(bool))
                {
                    bool result;
                    if (str != null && Boolean.TryParse(str, out result))
                    {
                        return result;
                    }

                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeBoolean));
                }
                else
                {
                    try
                    {
                        // Note that we are not casting the return value to nullable<T> as even if we do it
                        // CLR would un-box it back to T.
                        return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                    }
                    catch (InvalidCastException)
                    {
                        throw new ValidationException(Error.Format(SRResources.PropertyCannotBeConverted, type));
                    }
                    catch (FormatException)
                    {
                        throw new ValidationException(Error.Format(SRResources.PropertyUnrecognizedFormat, type));
                    }
                    catch (OverflowException)
                    {
                        throw new ValidationException(Error.Format(SRResources.PropertyTypeOverflow, type));
                    }
                }
            }
        }
    }
}
