//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;

public class ODataDeserializerTest
{
    [Fact]
    public void Ctor_SetsProperty_ODataPayloadKind()
    {
        // Arrange
        Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Unsupported);

        // Act & Assert
        Assert.Equal(ODataPayloadKind.Unsupported, deserializer.Object.ODataPayloadKind);
    }

    [Fact]
    public async Task ReadAsync_Throws_NotSupported()
    {
        // Arrange
        Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Resource) { CallBase = true };

        // Act & Assert
        await ExceptionAssert.ThrowsAsync<NotSupportedException>(
            () => deserializer.Object.ReadAsync(messageReader: null, type: null, readContext: null),
            "'ODataDeserializerProxy' does not support Read.");
    }
}
