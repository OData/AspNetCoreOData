// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataRawValueSerializerTests
    {
        [Theory]
        [InlineData(5)]
        [InlineData(5u)]
        [InlineData(5L)]
        [InlineData(5f)]
        [InlineData(5d)]
        [InlineData("test")]
        [InlineData(false)]
        [InlineData('t')]
        public void SerializesPrimitiveTypes(object value)
        {
            // Arrange
            ODataRawValueSerializer serializer = new ODataRawValueSerializer();
            Mock<IODataRequestMessage> mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(mockRequest.Object);

            // Act
            serializer.WriteObject(value, value.GetType(), messageWriter, null);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            // Assert
            Assert.Equal(value.ToString(), result, ignoreCase: true);
        }

        [Fact]
        public void SerializesNullablePrimitiveTypes()
        {
            // Arrange
            int? value = 5;
            ODataRawValueSerializer serializer = new ODataRawValueSerializer();
            Mock<IODataRequestMessage> mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(mockRequest.Object);

            // Act
            serializer.WriteObject(value, value.GetType(), messageWriter, null);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);

            // Assert
            Assert.Equal(value.ToString(), reader.ReadToEnd());
        }

        public static TheoryDataSet<object, DateTimeOffset> DateTimeTestData
        {
            get
            {
                // Because Xunit uses the test data for a unique Id, we'll use a stable time
                // derived from UtcNow() to allow a test discovery pass and run work
                // for a 24 hour period.
                DateTime dt1 = DateTime.Today;
                DateTimeOffset dto1 = new DateTimeOffset(dt1).ToLocalTime();

                DateTime dt2 = DateTime.Today.AddDays(1);
                DateTimeOffset dto2 = new DateTimeOffset(dt2).ToLocalTime();

                return new TheoryDataSet<object, DateTimeOffset>
                {
                    { dt1, dto1 },
                    { new DateTime?(dt2), dto2}
                };
            }
        }

        [Theory]
        [MemberData(nameof(DateTimeTestData))]
        public void SerializesDateTimeTypes(object value, DateTimeOffset expect)
        {
            // Arrange
            ODataRawValueSerializer serializer = new ODataRawValueSerializer();
            Mock<IODataRequestMessage> mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(mockRequest.Object);

            // Act
            serializer.WriteObject(value, value.GetType(), messageWriter, null);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);

            // Assert
            Assert.Equal(expect, DateTimeOffset.Parse(reader.ReadToEnd()));
        }

        [Fact]
        public void SerializesEnumType()
        {
            // Arrange
            ODataRawValueSerializer serializer = new ODataRawValueSerializer();
            Mock<IODataRequestMessage> mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(mockRequest.Object);
            object value = Color.Red | Color.Blue;

            // Act
            serializer.WriteObject(value, value.GetType(), messageWriter, null);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            // Assert
            Assert.Equal(value.ToString(), result, ignoreCase: true);
        }

        [Fact]
        public void SerializesReturnedCountValue()
        {
            // Arrange
            var serializer = new ODataRawValueSerializer();
            var mockRequest = new Mock<IODataRequestMessage>();
            Stream stream = new MemoryStream();
            mockRequest.Setup(r => r.GetStream()).Returns(stream);
            var messageWriter = new ODataMessageWriter(mockRequest.Object);
            HttpRequest request = RequestFactory.Create(opt => opt.AddModel(EdmCoreModel.Instance));
            request.ODataFeature().Path = new ODataPath(CountSegment.Instance);
            var context = new ODataSerializerContext { Request = request };

            // Act
            serializer.WriteObject(5, null, messageWriter, context);
            stream.Seek(0, SeekOrigin.Begin);
            TextReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            // Assert
            Assert.Equal("5", result);
        }

        private enum Color
        {
            Red,
            Blue
        }
    }
}