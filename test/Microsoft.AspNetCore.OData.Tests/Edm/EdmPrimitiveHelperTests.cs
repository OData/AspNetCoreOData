//-----------------------------------------------------------------------------
// <copyright file="EdmPrimitiveHelperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm;

public class EdmPrimitiveHelperTests
{
    private enum TestEnum
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3
    };

    private struct TestStruct
    {
        public int X { get; set; }
    }

    public static TheoryDataSet<object, object, Type> ConvertPrimitiveValue_NonStandardPrimitives_Data
        => new TheoryDataSet<object, object, Type>
            {
                { 1, 1, typeof(int) },
                { "1", (char)'1', typeof(char) },
                { "1", (char?)'1', typeof(char?) },
                { "123", (char[]) new char[] {'1', '2', '3' }, typeof(char[]) },
                { (int)1 , (ushort)1, typeof(ushort)},
                { (int?)1, (ushort?)1,  typeof(ushort?) },
                { (long)1, (uint)1,  typeof(uint) },
                { (long?)1, (uint?)1, typeof(uint?) },
                { (long)1 , (ulong)1, typeof(ulong) },
                { (long?)1 ,(ulong?)1, typeof(ulong?) },
                //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                { "<element xmlns=\"namespace\" />" ,(XElement)new XElement(XName.Get("element","namespace")), typeof(XElement)}
            };

    public static TheoryDataSet<object, object, Type> ConvertPrimitiveValue_NonStandardPrimitives_ExtraData
        => new TheoryDataSet<object, object, Type>
            {
                { "", (char?)null, typeof(char?) },
                { "Two", TestEnum.Two, typeof(TestEnum) },
                { "True", true, typeof(bool) },
                { "true", true, typeof(bool) },
                { "  True  ", true, typeof(bool) },
                { "False", false, typeof(bool) },
                { "false", false, typeof(bool) },
                { "  False  ", false, typeof(bool) },
                { TestEnum.Two, (double)2.0, typeof(double) }
            };

    public static TheoryDataSet<object, Type, string> ConvertThrow_NonStandardPrimitives_Data
        => new TheoryDataSet<object, Type, string>
            {
                { 9, typeof(char), "The value must be a string with a length of 1." },
                { "", typeof(char), "The value must be a string with a length of 1." },
                { "123", typeof(char), "The value must be a string with a length of 1." },
                { 9, typeof(char?), "The value must be a string with a maximum length of 1." },
                { "123", typeof(char?), "The value must be a string with a maximum length of 1." },
                { 123, typeof(XElement), "The value must be a string." },
                { 9, typeof(char[]), "The value must be a string." },
                { 9, typeof(XElement), "The value must be a string." },
                { 9, typeof(TestEnum), "The value must be a string." },
                { 9, typeof(DateTime), "The value must be a DateTimeOffset or Date." },
                { 9, typeof(TimeSpan), "The value must be a Edm.TimeOfDay." },
                { "", typeof(bool), "The value must be a boolean." },
                { "0", typeof(bool), "The value must be a boolean." },
                { 0, typeof(bool), "The value must be a boolean." },
                { 1024, typeof(byte), "The value has a value that is out of range of type System.Byte." },
                { "data", typeof(long),  "The value has a format that is not recognized by type System.Int64." },
                { "data", typeof(TestStruct),  "The value cannot be converted to type Microsoft.AspNetCore.OData.Tests.Edm.EdmPrimitiveHelperTests+TestStruct." },
                { new TestStruct(), typeof(int),  "The value cannot be converted to type System.Int32." }
            };

    public static TheoryDataSet<DateTimeOffset> ConvertDateTime_NonStandardPrimitives_Data
        => new TheoryDataSet<DateTimeOffset>
            {
                DateTimeOffset.Parse("2014-12-12T01:02:03Z"),
                DateTimeOffset.Parse("2014-12-12T01:02:03-8:00"),
                DateTimeOffset.Parse("2014-12-12T01:02:03+8:00"),
            };

    [Theory]
    [MemberData(nameof(ConvertPrimitiveValue_NonStandardPrimitives_Data))]
    [MemberData(nameof(ConvertPrimitiveValue_NonStandardPrimitives_ExtraData))] 
    public void ConvertPrimitiveValue_NonStandardPrimitives(object valueToConvert, object result, Type conversionType)
    {
        // Arrange & Act
        object actual = EdmPrimitiveHelper.ConvertPrimitiveValue(valueToConvert, conversionType);

        // Assert
        if (result == null)
        {
            Assert.Equal(result, actual);
        }
        else
        {
            Assert.Equal(result.GetType(), actual.GetType());
            Assert.Equal(result.ToString(), actual.ToString());
        }
    }

    [Theory]
    [MemberData(nameof(ConvertDateTime_NonStandardPrimitives_Data))]
    public void ConvertDateTimeValue_NonStandardPrimitives_DefaultTimeZoneInfo(DateTimeOffset valueToConvert)
    {
        // Arrange & Act
        object actual = EdmPrimitiveHelper.ConvertPrimitiveValue(valueToConvert, typeof(DateTime));

        // Assert
        DateTime dt = Assert.IsType<DateTime>(actual);
        Assert.Equal(valueToConvert.LocalDateTime, dt);
    }

    [Theory]
    [MemberData(nameof(ConvertDateTime_NonStandardPrimitives_Data))]
    public void ConvertDateTimeValue_NonStandardPrimitives_CustomTimeZoneInfo(DateTimeOffset valueToConvert)
    {
        // Arrange & Act
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        object actual = EdmPrimitiveHelper.ConvertPrimitiveValue(valueToConvert, typeof(DateTime), timeZone);

        // Assert
        DateTime dt = Assert.IsType<DateTime>(actual);
        Assert.Equal(TimeZoneInfo.ConvertTime(valueToConvert, timeZone).DateTime, dt);
    }

    [Theory]
    [MemberData(nameof(ConvertThrow_NonStandardPrimitives_Data))]
    public void ConvertPrimitiveValue_Throws(object valueToConvert, Type conversionType, string exception)
    {
        // Arrange & Act & Assert
        ExceptionAssert.Throws<ValidationException>(
            () => EdmPrimitiveHelper.ConvertPrimitiveValue(valueToConvert, conversionType),
            exception);
    }
}
