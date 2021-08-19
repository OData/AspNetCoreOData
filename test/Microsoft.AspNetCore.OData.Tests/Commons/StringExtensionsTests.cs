//-----------------------------------------------------------------------------
// <copyright file="StringExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Common;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("PostTo", "Post")]
        [InlineData("PUTTO", "Put")]
        [InlineData("PATCHTO", "Patch")]
        [InlineData("DELETETO", "Delete")]
        [InlineData("Get", "Get")]
        public void NormalizeHttpMethod_Returns_MethodExpected(string method, string expected)
        {
            // Arrange
            string actual = method.NormalizeHttpMethod();

            // Act & Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("{any}", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("{any", false)]
        [InlineData("any}", false)]
        public void IsValidTemplateLiteral_Returns_BooleanExpected(string literal, bool expected)
        {
            // Arrange
            bool actual = literal.IsValidTemplateLiteral();

            // Act & Assert
            Assert.Equal(expected, actual);
        }
    }
}
