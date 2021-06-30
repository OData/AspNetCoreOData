// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ODataOutputFormatterHelperTests
    {
        [Fact]
        public void BuildSerializerContext_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataOutputFormatterHelper.BuildSerializerContext(null), "request");
        }

        [Fact]
        public async Task WriteToStreamAsync_ThrowsArgumentNull_Model()
        {
            // Arrange & Act & Assert
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => ODataOutputFormatterHelper.WriteToStreamAsync(null, null, null, ODataVersion.V4, null, null, null, null, null),
                "The request must have an associated EDM model. Consider registering Edm model calling AddOData().");
        }

        [Fact]
        public void GetSerializer_ThrowsSerializationException_NonEdmType()
        {
            // Arrange
            Mock<IEdmObject> mock = new Mock<IEdmObject>();
            mock.Setup(s => s.GetEdmType()).Returns((IEdmTypeReference)null);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(() => ODataOutputFormatterHelper.GetSerializer(type: null, mock.Object, null, null),
                "The EDM type of the object of type 'Castle.Proxies.IEdmObjectProxy' is null. The EDM type of an 'IEdmObject' cannot be null.");
        }

        [Fact]
        public void GetSerializer_ThrowsSerializationException_TypeCannotBeSerialized()
        {
            // Arrange
            Type intType = typeof(int);
            HttpRequest request = new DefaultHttpContext().Request;
            Mock<IODataSerializerProvider> provider = new Mock<IODataSerializerProvider>();
            provider.Setup(s => s.GetODataPayloadSerializer(intType, request)).Returns((IODataSerializer)null);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => ODataOutputFormatterHelper.GetSerializer(intType, null, request, provider.Object),
                "'Int32' cannot be serialized using the OData output formatter.");
        }

        [Fact]
        public void GetSerializer_ThrowsSerializationException_TypeCannotBeSerialized_NonClrType()
        {
            // Arrange
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);
            Mock<IEdmObject> mock = new Mock<IEdmObject>();
            mock.Setup(s => s.GetEdmType()).Returns(intType);

            Mock<IODataSerializerProvider> provider = new Mock<IODataSerializerProvider>();
            provider.Setup(s => s.GetEdmTypeSerializer(intType)).Returns((IODataEdmTypeSerializer)null);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => ODataOutputFormatterHelper.GetSerializer(null, mock.Object, null, provider.Object),
                "'[Edm.Int32 Nullable=False]' cannot be serialized using the OData output formatter.");
        }
    }
}