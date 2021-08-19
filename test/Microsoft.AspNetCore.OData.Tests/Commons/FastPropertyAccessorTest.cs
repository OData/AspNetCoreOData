//-----------------------------------------------------------------------------
// <copyright file="FastPropertyAccessorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Common;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class FastPropertyAccessorTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Property()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new FastPropertyAccessor<MyProps>(null), "property");
        }

        [Fact]
        public void Ctor_ThrowsArgument_Property()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => new FastPropertyAccessor<MyProps>(typeof(MyProps).GetProperty("DoubleProp")),
                "property", "The PropertyInfo provided must have public 'get' and 'set' accessor methods.");
        }

        [Fact]
        public void GetterWorksForValueType()
        {
            // Arrange
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("IntProp"));
            mine.IntProp = 4;

            // Act & Assert
            Assert.Equal(4, accessor.GetValue(mine));
        }

        [Fact]
        public void SetterWorksForValueType()
        {
            // Arrange
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("IntProp"));
            mine.IntProp = 4;

            // Act
            accessor.SetValue(mine, 3);

            // Assert
            Assert.Equal(3, accessor.GetValue(mine));
            Assert.Equal(3, mine.IntProp);
        }

        [Fact]
        public void Getter_ThrowsArgumentNull_Instance()
        {
            // Arrange & Act & Assert
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("StringProp"));
            ExceptionAssert.ThrowsArgumentNull(() => accessor.GetValue(null), "instance");
        }

        [Fact]
        public void GetterWorksForReferenceType()
        {
            // Arrange
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("StringProp"));
            mine.StringProp = "*4";

            // Act & Assert
            Assert.Equal("*4", accessor.GetValue(mine));
        }

        [Fact]
        public void Setter_ThrowsArgumentNull_Instance()
        {
            // Arrange & Act & Assert
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("StringProp"));
            ExceptionAssert.ThrowsArgumentNull(() => accessor.SetValue(null, 5), "instance");
        }

        [Fact]
        public void SetterWorksForReferenceType()
        {
            // Arrange
            var mine = new MyProps();
            var accessor = new FastPropertyAccessor<MyProps>(mine.GetType().GetProperty("StringProp"));
            mine.StringProp = "*4";

            // Act
            accessor.SetValue(mine, "#3");

            // Assert
            Assert.Equal("#3", accessor.GetValue(mine));
            Assert.Equal("#3", mine.StringProp);
        }

        [Fact]
        public void Copy_ThrowsArgumentNull_FromAndTo()
        {
            // Arrange
            var accessor = new FastPropertyAccessor<MyProps>(typeof(MyProps).GetProperty("StringProp"));

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => accessor.Copy(null, null), "from");

            // Act & Assert
            MyProps mine = new MyProps();
            ExceptionAssert.ThrowsArgumentNull(() => accessor.Copy(mine, null), "to");
        }

        public class MyProps
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }

            public double DoubleProp { get; }
        }
    }
}
