// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Xunit;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson.Tests
{
    public class JDynamicTypeWrapperConverterTests
    {
        [Theory]
        [InlineData(typeof(AggregationWrapper), true)]
        [InlineData(typeof(ComputeWrapper<object>), true)]
        [InlineData(typeof(EntitySetAggregationWrapper), true)]
        [InlineData(typeof(FlatteningWrapper<object>), true)]
        [InlineData(typeof(GroupByWrapper), true)]
        [InlineData(typeof(NoGroupByAggregationWrapper), true)]
        [InlineData(typeof(NoGroupByWrapper), true)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(SelectExpandWrapper), false)]
        public void CanConvertWorksForDynamicTypeWrapper(Type type, bool expected)
        {
            // Arrange
            DynamicTypeWrapperConverter converter = new DynamicTypeWrapperConverter();

            // Act & Assert
            Assert.Equal(expected, converter.CanConvert(type));
        }

        [Fact]
        public void ReadJsonForDynamicTypeWrapperThrowsNotImplementedException()
        {
            // Arrange
            JDynamicTypeWrapperConverter converter = new JDynamicTypeWrapperConverter();

            // Act
            Action test = () => converter.ReadJson(null, typeof(object), null, null);

            // Assert
            NotImplementedException exception = Assert.Throws<NotImplementedException>(test);
            Assert.Equal(SRResources.ReadDynamicTypeWrapperNotImplemented, exception.Message);
        }

        [Fact]
        public void AggregationWrapperConverterCanSerializeAggregationWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverter<AggregationWrapper>();
        }

        [Fact]
        public void EntitySetAggregationWrapperConverterCanSerializeEntitySetAggregationWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverter<EntitySetAggregationWrapper>();
        }

        [Fact]
        public void GroupByWrapperWrapperConverterCanSerializeGroupByWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverter<GroupByWrapper>();
        }

        [Fact]
        public void NoGroupByAggregationWrapperConverterCanSerializeNoGroupByAggregationWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverter<NoGroupByWrapper>();
        }

        [Fact]
        public void NoGroupByWrapperWrapperConverterCanSerializeNoGroupByWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverter<NoGroupByWrapper>();
        }

        internal static void TestDynamicTypeWrapperConverter<T>() where T : GroupByWrapper
        {
            // Arrange
            T wrapper = (T)Activator.CreateInstance(typeof(T));

            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = "TestProp",
                Value = "TestValue"
            };

            JDynamicTypeWrapperConverter converter = new JDynamicTypeWrapperConverter();

            // Act
            string json = SerializeUtils.WriteJson(converter, wrapper);

            // Assert
            Assert.Equal("{\"TestProp\":\"TestValue\"}", json);
        }

        [Fact]
        public void ComputeWrapperOfTypeConverterCanSerializeGroupByWrapper()
        {
            // Arrange
            GroupByWrapper wrapper = new GroupByWrapper();
            wrapper.GroupByContainer = new AggregationPropertyContainer()
            {
                Name = "TestProp",
                Value = "TestValue"
            };

            ComputeWrapper<GroupByWrapper> computeWrapper = new ComputeWrapper<GroupByWrapper>
            {
                Instance = wrapper
            };

            JDynamicTypeWrapperConverter converter = new JDynamicTypeWrapperConverter();

            // Act
            string json = SerializeUtils.WriteJson(converter, computeWrapper);

            // Assert
            Assert.Equal("{\"TestProp\":\"TestValue\"}", json);
        }

        [Fact]
        public void FlatteningWrapperOfTypeConverterCanSerializeGroupByWrapper()
        {
            // Arrange
            FlatteningWrapper<object> flatteningWrapper = new FlatteningWrapper<object>
            {
                GroupByContainer = new AggregationPropertyContainer()
                {
                    Name = "TestProp",
                    Value = "TestValue"
                }
            };

            JDynamicTypeWrapperConverter converter = new JDynamicTypeWrapperConverter();

            // Act
            string json = SerializeUtils.WriteJson(converter, flatteningWrapper);

            // Assert
            Assert.Equal("{\"TestProp\":\"TestValue\"}", json);
        }
    }
}
