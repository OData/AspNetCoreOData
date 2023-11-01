//-----------------------------------------------------------------------------
// <copyright file="FromODataBodyAttributeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class FromODataBodyAttributeTests
    {
        [Fact]
        public void Ctor_ConfigureODataBodyModelBinder()
        {
            // Arrange
            FromODataBodyAttribute attribute = new FromODataBodyAttribute();

            // Act & Assert
            Assert.Equal(typeof(ODataBodyModelBinder), attribute.BinderType);
        }
    }
}
