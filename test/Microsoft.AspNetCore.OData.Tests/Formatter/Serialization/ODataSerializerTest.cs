// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataSerializerTest
    {
        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            // Arrange
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported).Object;

            // Act & Assert
            Assert.Equal(ODataPayloadKind.Unsupported, serializer.ODataPayloadKind);
        }

        [Fact]
        public void WriteObject_Throws_NotSupported()
        {
            // Arrange
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported) { CallBase = true }.Object;

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(
                () => serializer.WriteObject(graph: null, type: typeof(int), messageWriter: null, writeContext: null),
                "ODataSerializerProxy does not support WriteObject.");
        }
    }
}
