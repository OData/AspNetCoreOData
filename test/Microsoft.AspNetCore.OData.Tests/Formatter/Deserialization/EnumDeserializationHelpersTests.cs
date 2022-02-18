//-----------------------------------------------------------------------------
// <copyright file="EnumDeserializationHelpersTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class EnumDeserializationHelpersTests
    {
        [Fact]
        public void ConvertEnumValue_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => EnumDeserializationHelpers.ConvertEnumValue(null, null), "value");
            ExceptionAssert.ThrowsArgumentNull(() => EnumDeserializationHelpers.ConvertEnumValue(42, null), "type");
        }

        [Fact]
        public void ConvertEnumValue_Returns_ForPocoEnum()
        {
            // Arrange & Act & Assert
            Assert.Equal(EnumColor.Blue, EnumDeserializationHelpers.ConvertEnumValue(EnumColor.Blue, typeof(EnumColor)));
            Assert.Equal(EnumColor.Blue, EnumDeserializationHelpers.ConvertEnumValue(EnumColor.Blue, typeof(EnumColor?)));
        }

        [Fact]
        public void ConvertEnumValue_ThrowsValidationException_NonODataEnumValue()
        {
            // Arrange & Act & Assert
            ExceptionAssert.Throws<ValidationException>(
                () => EnumDeserializationHelpers.ConvertEnumValue(42, typeof(EnumColor)),
                "The value with type 'Int32' must have type 'ODataEnumValue'.");
        }

        [Fact]
        public void ConvertEnumValue_ThrowsValidationException_NonEnumType()
        {
            // Arrange & Act & Assert
            ODataEnumValue enumValue = new ODataEnumValue("Red");

            ExceptionAssert.Throws<InvalidOperationException>(
                () => EnumDeserializationHelpers.ConvertEnumValue(enumValue, typeof(int)),
                "The type 'Int32' must be an enum or Nullable<T> where T is an enum type.");
        }

        [Fact]
        public void ConvertEnumValue_Returns_ForODataEnumValue()
        {
            // Arrange & Act & Assert
            ODataEnumValue enumValue = new ODataEnumValue("Red");
            Assert.Equal(EnumColor.Red, EnumDeserializationHelpers.ConvertEnumValue(enumValue, typeof(EnumColor)));

            // Arrange & Act & Assert
            enumValue = new ODataEnumValue("Green");
            Assert.Equal(EnumColor.Green, EnumDeserializationHelpers.ConvertEnumValue(enumValue, typeof(EnumColor)));
        }

        public enum EnumColor
        {
            Red,
            Blue,
            Green
        }
    }
}
