//-----------------------------------------------------------------------------
// <copyright file="RequestPreferenceHelpersTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class RequestPreferenceHelpersTests
    {
        [Fact]
        public void RequestPrefersMaxPageSize_ReturnsFalse_WithPreferHeader()
        {
            // Arrange
            HeaderDictionary headers = new HeaderDictionary();

            // Act & Assert
            Assert.False(RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out _));
        }

        [Theory]
        [InlineData("maxpagesize=5")]
        [InlineData("odata.maxpagesize=5")]
        public void RequestPrefersMaxPageSize_ReturnsPageSize_WithPreferValue(string preferValue)
        {
            // Arrange
            HeaderDictionary headers = new HeaderDictionary(
                new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Prefer", preferValue }
                });

            // Act & Assert
            Assert.True(RequestPreferenceHelpers.RequestPrefersMaxPageSize(headers, out int pageSize));
            Assert.Equal(5, pageSize);
        }
    }
}
