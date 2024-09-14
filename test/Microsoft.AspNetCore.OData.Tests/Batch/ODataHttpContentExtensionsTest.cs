//-----------------------------------------------------------------------------
// <copyright file="ODataHttpContentExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch;

public class ODataHttpContentExtensionsTest
{
    [Fact]
    public async Task GetODataMessageReaderAsync_NullContent_Throws()
    {
        // Arrange & Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => ODataHttpContentExtensions.GetODataMessageReaderAsync(null, new ODataMessageReaderSettings(), CancellationToken.None),
            "content");
    }

    [Fact]
    public async Task GetODataMessageReaderAsync_ReturnsMessageReader()
    {
        // Arrange
        StringContent content = new StringContent("foo", Encoding.UTF8, "multipart/mixed");

        // Act & Assert
        Assert.NotNull(await content.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None));
    }
}
