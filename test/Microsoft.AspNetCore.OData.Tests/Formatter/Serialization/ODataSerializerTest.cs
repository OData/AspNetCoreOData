//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
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
        public async Task WriteObjectAsync_Throws_NotSupported()
        {
            // Arrange
            ODataSerializer serializer = new Mock<ODataSerializer>(ODataPayloadKind.Unsupported) { CallBase = true }.Object;

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<NotSupportedException>(
                () => serializer.WriteObjectAsync(graph: null, type: typeof(int), messageWriter: null, writeContext: null),
                "ODataSerializerProxy does not support WriteObject.");
        }
    }
}
