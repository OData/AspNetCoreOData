// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Edm;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataPrimitiveSerializerTests
    {
        public static IEnumerable<object[]> NonEdmPrimitiveConversionData
        {
            get
            {
                return EdmPrimitiveHelperTests.ConvertPrimitiveValue_NonStandardPrimitives_Data.Select(data => new[] { data[1], data[0] });
            }
        }

        public static TheoryDataSet<DateTime> NonEdmPrimitiveConversionDateTime
        {
            get
            {
                DateTime dtUtc = new DateTime(2014, 12, 12, 1, 2, 3, DateTimeKind.Utc);
                DateTime dtLocal = new DateTime(2014, 12, 12, 1, 2, 3, DateTimeKind.Local);
                DateTime unspecified = new DateTime(2014, 12, 12, 1, 2, 3, DateTimeKind.Unspecified);
                return new TheoryDataSet<DateTime>
                {
                    { dtUtc },
                    { dtLocal },
                    { unspecified },
                };
            }
        }

        public static TheoryDataSet<object, string, string> NonEdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<object, string, string>
                {
                    { (char)'1', "Edm.String", "\"1\"" },
                    { (char[]) new char[] {'1' }, "Edm.String", "\"1\"" },
                    { (UInt16)1, "Edm.Int32", "1" },
                    { (UInt32)1, "Edm.Int64", "1" },
                    { (UInt64)1, "Edm.Int64", "1" },
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                    { new XElement(XName.Get("element","namespace")), "Edm.String", "\"<element xmlns=\\\"namespace\\\" />\"" },
                };
            }
        }

        public static TheoryDataSet<object, string, string> EdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<object, string, string>
                {
                    { "1", "Edm.String", "\"1\"" },
                    { true, "Edm.Boolean", "true" },
                    { (Byte)1, "Edm.Byte", "1" },
                    { (Decimal)1, "Edm.Decimal", "1" },
                    { (Double)1, "Edm.Double", "1.0" },
                    { (Guid)Guid.Empty, "Edm.Guid", "\"00000000-0000-0000-0000-000000000000\"" },
                    { (Int16)1, "Edm.Int16", "1" },
                    { (Int32)1, "Edm.Int32", "1" },
                    { (Int64)1, "Edm.Int64", "1" },
                    { (SByte)1, "Edm.SByte", "1" },
                    { (Single)1, "Edm.Single", "1" },
                    { new byte[] { 1 }, "Edm.Binary", "\"AQ==\"" },
                    { new TimeSpan(), "Edm.Duration", "\"PT0S\"" },
                    { new DateTimeOffset(), "Edm.DateTimeOffset", "\"0001-01-01T00:00:00Z\"" },
                    { new Date(2014, 10, 13), "Edm.Date", "\"2014-10-13\"" },
                    { new TimeOfDay(15, 38, 25, 109), "Edm.TimeOfDay", "\"15:38:25.1090000\"" },
                };
            }
        }

        [Fact]
        public void Property_ODataPayloadKind()
        {
            // Arrange
            var serializer = new ODataPrimitiveSerializer();

            // Act & Assert
            Assert.Equal(ODataPayloadKind.Property, serializer.ODataPayloadKind);
        }

        [Fact]
        public void WriteObject_Throws_RootElementNameMissing()
        {
            // Arrange
            ODataSerializerContext writeContext = new ODataSerializerContext();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(
                () => serializer.WriteObject(42, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), writeContext),
                "The 'RootElementName' property is required on 'ODataSerializerContext'. (Parameter 'writeContext')");
        }

        [Fact]
        public void WriteObject_Calls_CreateODataPrimitiveValue()
        {
            // Arrange
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = EdmCoreModel.Instance };
            Mock<ODataPrimitiveSerializer> serializer = new Mock<ODataPrimitiveSerializer>();
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataPrimitiveValue(
                    42, It.Is<IEdmPrimitiveTypeReference>(t => t.PrimitiveKind() == EdmPrimitiveTypeKind.Int32), writeContext))
                .Returns(new ODataPrimitiveValue(42)).Verifiable();

            // Act
            serializer.Object.WriteObject(42, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void CreateODataValue_PrimitiveValue()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(int).GetEdmPrimitiveTypeReference();
            var serializer = new ODataPrimitiveSerializer();

            // Act
            var odataValue = serializer.CreateODataValue(20, edmPrimitiveType, writeContext: null);

            // Assert
            Assert.NotNull(odataValue);
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.Equal(20, primitiveValue.Value);
        }

        [Fact]
        public void CreateODataValue_ReturnsODataNullValue_ForNullValue()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(string).GetEdmPrimitiveTypeReference();
            var serializer = new ODataPrimitiveSerializer();

            // Act
            var odataValue = serializer.CreateODataValue(null, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            Assert.IsType<ODataNullValue>(odataValue);
        }

        [Fact]
        public void CreateODataValue_ReturnsDateTimeOffset_ForDateTime_ByDefault()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(DateTime).GetEdmPrimitiveTypeReference();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            DateTime dt = new DateTime(2014, 10, 27);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(dt, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.Equal(new DateTimeOffset(dt), primitiveValue.Value);
        }

        [Theory]
        [InlineData("UTC")] // +0:00
        [InlineData("Pacific Standard Time")] // -8:00
        [InlineData("China Standard Time")] // +8:00
        public void CreateODataValue_ReturnsDateTimeOffsetMinValue_ForDateTimeMinValue(string timeZoneId)
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(DateTime).GetEdmPrimitiveTypeReference();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            DateTime dt = DateTime.MinValue;
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var request = RequestFactory.Create();
            ODataSerializerContext context = new ODataSerializerContext { Request = request, TimeZone = timeZone };

            // Act
            ODataValue odataValue = serializer.CreateODataValue(dt, edmPrimitiveType, context);

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);

            if (timeZone.BaseUtcOffset.Hours < 0)
            {
                Assert.Equal(new DateTimeOffset(dt, timeZone.GetUtcOffset(dt)), primitiveValue.Value);
            }
            else
            {
                Assert.Equal(DateTimeOffset.MinValue, primitiveValue.Value);
            }
        }

        [Theory]
        [InlineData("UTC")] // +0:00
        [InlineData("Pacific Standard Time")] // -8:00
        [InlineData("China Standard Time")] // +8:00
        public void CreateODataValue_ReturnsDateTimeOffsetMaxValue_ForDateTimeMaxValue(string timeZoneId)
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(DateTime).GetEdmPrimitiveTypeReference();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            DateTime dt = DateTime.MaxValue;
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var request = RequestFactory.Create();
            ODataSerializerContext context = new ODataSerializerContext { Request = request, TimeZone = timeZone };

            // Act
            ODataValue odataValue = serializer.CreateODataValue(dt, edmPrimitiveType, context);

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);

            if (timeZone.BaseUtcOffset.Hours > 0)
            {
                Assert.Equal(new DateTimeOffset(dt, timeZone.GetUtcOffset(dt)), primitiveValue.Value);
            }
            else
            {
                Assert.Equal(DateTimeOffset.MaxValue, primitiveValue.Value);
            }
        }

        [Theory]
        [MemberData(nameof(NonEdmPrimitiveConversionDateTime))]
        public void CreateODataValue_ReturnsDateTimeOffset_ForDateTime_WithDifferentTimeZone(DateTime value)
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(DateTime).GetEdmPrimitiveTypeReference();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var request = RequestFactory.Create(opt => opt.SetTimeZoneInfo(tzi));

            ODataSerializerContext context = new ODataSerializerContext { Request = request };

            DateTimeOffset expected = value.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(value, tzi.GetUtcOffset(value))
                : TimeZoneInfo.ConvertTime(new DateTimeOffset(value), tzi);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(value, edmPrimitiveType, context);

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.Equal(expected, primitiveValue.Value);
        }

        [Fact]
        public void CreateODataValue_ReturnsDate_ForDateTime()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(Date).GetEdmPrimitiveTypeReference();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            DateTime dt = new DateTime(2014, 10, 27);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(dt, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.IsType<Date>(primitiveValue.Value);
            Assert.Equal(new Date(dt.Year, dt.Month, dt.Day), primitiveValue.Value);
        }

        [Fact]
        public void CreateODataValue_ReturnsTimeOfDay_ForTimeSpan()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(TimeOfDay).GetEdmPrimitiveTypeReference();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            TimeSpan ts = new TimeSpan(0, 10, 11, 12, 13);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(ts, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.IsType<TimeOfDay>(primitiveValue.Value);
            Assert.Equal(new TimeOfDay(ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds), primitiveValue.Value);
        }

        [Theory]
        [MemberData(nameof(EdmPrimitiveData))]
        [MemberData(nameof(NonEdmPrimitiveData))]
        public void WriteObject_EdmPrimitives(object graph, string type, string value)
        {
            // Arrange
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            ODataSerializerContext writecontext = new ODataSerializerContext()
            {
                RootElementName = "PropertyName",
                Model = EdmCoreModel.Instance
            };

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };

            MemoryStream stream = new MemoryStream();
            ODataMessageWriter writer = new ODataMessageWriter(
                new ODataMessageWrapper(stream) as IODataResponseMessage, settings);

            string expect = "{\"@odata.context\":\"http://any/$metadata#" + type + "\",";
            if (type == "Edm.Null")
            {
                expect += "\"@odata.null\":" + value + "}";
            }
            else
            {
                expect += "\"value\":" + value + "}";
            }

            // Act
            serializer.WriteObject(graph, typeof(int), writer, writecontext);

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();
            Assert.Equal(expect, result);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(short).GetEdmPrimitiveTypeReference();
            ODataPrimitiveValue primitive = new ODataPrimitiveValue((short)1);

            // Act
            ODataPrimitiveSerializer.AddTypeNameAnnotationAsNeeded(primitive, edmPrimitiveType, ODataMetadataLevel.Full);

            // Assert
            ODataTypeAnnotation annotation = primitive.TypeAnnotation;
            Assert.NotNull(annotation); // Guard
            Assert.Equal("Edm.Int16", annotation.TypeName);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(0, true)]
        [InlineData("", true)]
        [InlineData(0.1D, true)]
        [InlineData(double.PositiveInfinity, false)]
        [InlineData(double.NegativeInfinity, false)]
        [InlineData(double.NaN, false)]
        [InlineData((short)1, false)]
        public void CanTypeBeInferredInJson(object value, bool expectedResult)
        {
            // Act
            bool actualResult = ODataPrimitiveSerializer.CanTypeBeInferredInJson(value);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void CreatePrimitive_ReturnsNull_ForNullValue()
        {
            // Act
            IEdmPrimitiveTypeReference edmPrimitiveType = typeof(int).GetEdmPrimitiveTypeReference();
            ODataValue value = ODataPrimitiveSerializer.CreatePrimitive(null, edmPrimitiveType, writeContext: null);

            // Assert
            Assert.Null(value);
        }

        [Theory]
        [MemberData(nameof(EdmPrimitiveData))]
        public void ConvertUnsupportedPrimitives_DoesntChangeStandardEdmPrimitives(object graph, string type, string value)
        {
            // Arrange & Act & Assert
            Assert.NotNull(type);
            Assert.NotNull(value);
            Assert.Equal(graph, ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph, timeZoneInfo: null));
        }

        [Theory]
        [MemberData(nameof(NonEdmPrimitiveConversionData))]
        public void ConvertUnsupportedPrimitives_NonStandardEdmPrimitives(object graph, object result)
        {
            // Arrange & Act & Assert
            Assert.Equal(result, ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph, timeZoneInfo: null));
        }

        [Theory]
        [MemberData(nameof(NonEdmPrimitiveConversionDateTime))]
        public void ConvertUnsupportedDateTime_NonStandardEdmPrimitives(DateTime graph)
        {
            // Arrange & Act
            TimeZoneInfo timeZone = TimeZoneInfo.Local;
            object value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph, timeZoneInfo: null);

            DateTimeOffset expected = graph.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(graph, timeZone.GetUtcOffset(graph))
                : TimeZoneInfo.ConvertTime(new DateTimeOffset(graph), timeZone);

            // Assert
            DateTimeOffset actual = Assert.IsType<DateTimeOffset>(value);
            Assert.Equal(new DateTimeOffset(graph), actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(NonEdmPrimitiveConversionDateTime))]
        public void ConvertUnsupportedDateTime_NonStandardEdmPrimitives_TimeZone(DateTime graph)
        {
            // Arrange
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

            DateTimeOffset expected = graph.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(graph, tzi.GetUtcOffset(graph))
                : TimeZoneInfo.ConvertTime(new DateTimeOffset(graph), tzi);

            // Act
            object value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph, tzi);

            // Assert
            DateTimeOffset actual = Assert.IsType<DateTimeOffset>(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, ODataMetadataLevel.Full, true)]
        [InlineData((short)1, ODataMetadataLevel.Full, false)]
        [InlineData((short)1, ODataMetadataLevel.Minimal, true)]
        [InlineData((short)1, ODataMetadataLevel.None, true)]
        public void ShouldSuppressTypeNameSerialization(object value, ODataMetadataLevel metadataLevel,
            bool expectedResult)
        {
            // Act
            bool actualResult = ODataPrimitiveSerializer.ShouldSuppressTypeNameSerialization(value, metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
