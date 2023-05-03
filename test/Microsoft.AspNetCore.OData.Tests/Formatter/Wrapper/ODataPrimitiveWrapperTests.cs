//-----------------------------------------------------------------------------
// <copyright file="ODataPrimitiveWrapperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Wrapper
{
    public class ODataPrimitiveWrapperTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_PrimitiveValue()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataPrimitiveWrapper(null), "value");
        }

        [Fact]
        public void Ctor_Sets_CorrectProperties()
        {
            // Arrange & Act & Assert
            ODataPrimitiveValue oDataPrimitiveValue = new ODataPrimitiveValue(42);

            // Act
            ODataPrimitiveWrapper wrapper = new ODataPrimitiveWrapper(oDataPrimitiveValue);

            // Assert
            Assert.Same(oDataPrimitiveValue, wrapper.Value);
        }
    }
}
