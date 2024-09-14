//-----------------------------------------------------------------------------
// <copyright file="PropertyHelperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Common;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Commons;

public class PropertyHelperTests
{
    [Fact]
    public void GetProperties_Returns_PropertyHelpers()
    {
        // Arrange
        MyProps props = new MyProps
        {
            IntProp = 42,
            StringProp = "abc"
        };

        // Act
        PropertyHelper[] properties = PropertyHelper.GetProperties(props);

        // Assert
        Assert.Equal(2, properties.Length);
        Assert.Collection(properties,
            e =>
            {
                Assert.Equal("IntProp", e.Name);
                Assert.Equal(42, e.GetValue(props));
            },
            e =>
            {
                Assert.Equal("StringProp", e.Name);
                Assert.Equal("abc", e.GetValue(props));
            });
    }

    private class MyProps
    {
        public int IntProp { get; set; }
        public string StringProp { get; set; }
    }
}
