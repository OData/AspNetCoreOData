//-----------------------------------------------------------------------------
// <copyright file="DynamicTypeWrapperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Wrapper
{
    public class DynamicTypeWrapperTests
    {
        [Fact]
        public void CanSetProperty()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new GroupByWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            var actual = wrapper.Values[propName];

            Assert.Equal(expectedValue, actual);
        }

        [Fact]
        public void CanTryGetProperty()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new GroupByWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            object actual;
            Assert.True(wrapper.TryGetPropertyValue(propName, out actual));

            Assert.Equal(expectedValue, actual);
        }

        [Fact]
        public void CanEqualWrappers()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new GroupByWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            var wrapper2 = new GroupByWrapper();
            wrapper2.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            Assert.Equal(wrapper, wrapper2);
        }

        [Fact]
        public void GetHashCodeEqualForEqualWrappers()
        {
            var expectedValue = "TestValue";
            var propName = "TestProp";
            var wrapper = new GroupByWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            var wrapper2 = new GroupByWrapper();
            wrapper2.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = propName,
                Value = expectedValue
            };

            Assert.Equal(wrapper.GetHashCode(), wrapper2.GetHashCode());
        }
    }
}
