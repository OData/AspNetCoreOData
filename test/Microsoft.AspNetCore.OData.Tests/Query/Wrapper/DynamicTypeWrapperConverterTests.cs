// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Wrapper
{
    public class DynamicTypeWrapperConverterTests
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

        [Theory]
        [InlineData(typeof(AggregationWrapper), typeof(AggregationWrapperConverter))]
        [InlineData(typeof(ComputeWrapper<object>), typeof(ComputeWrapperConverter<object>))]
        [InlineData(typeof(EntitySetAggregationWrapper), typeof(EntitySetAggregationWrapperConverter))]
        [InlineData(typeof(FlatteningWrapper<object>), typeof(FlatteningWrapperConverter<object>))]
        [InlineData(typeof(GroupByWrapper), typeof(GroupByWrapperConverter))]
        [InlineData(typeof(NoGroupByAggregationWrapper), typeof(NoGroupByAggregationWrapperConverter))]
        [InlineData(typeof(NoGroupByWrapper), typeof(NoGroupByWrapperConverter))]
        [InlineData(typeof(object), null)]
        [InlineData(typeof(SelectExpandWrapper), null)]
        public void CreateConverterWorksForDynamicTypeWrapper(Type type, Type expected)
        {
            // Arrange
            JsonSerializerOptions options = new JsonSerializerOptions();
            DynamicTypeWrapperConverter converter = new DynamicTypeWrapperConverter();

            // Act
            JsonConverter typeConverter = converter.CreateConverter(type, options);

            // Assert
            if (expected == null)
            {
                Assert.Null(typeConverter);
            }
            else
            {
                Assert.Equal(expected, typeConverter.GetType());
            }
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

            JsonSerializerOptions options = new JsonSerializerOptions();
            DynamicTypeWrapperConverter converter = new DynamicTypeWrapperConverter();
            JsonConverter<T> typeConverter = converter.CreateConverter(typeof(T), options) as JsonConverter<T>;

            // Act
            string json = SerializeUtils.SerializeAsJson(jsonWriter => typeConverter.Write(jsonWriter, wrapper, options));

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

            JsonSerializerOptions options = new JsonSerializerOptions();
            ComputeWrapperConverter<GroupByWrapper> converter = new ComputeWrapperConverter<GroupByWrapper>();

            // Act
            string json = SerializeUtils.SerializeAsJson(jsonWriter => converter.Write(jsonWriter, computeWrapper, options));

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

            JsonSerializerOptions options = new JsonSerializerOptions();
            FlatteningWrapperConverter<object> converter = new FlatteningWrapperConverter<object>();

            // Act
            string json = SerializeUtils.SerializeAsJson(jsonWriter => converter.Write(jsonWriter, flatteningWrapper, options));

            // Assert
            Assert.Equal("{\"TestProp\":\"TestValue\"}", json);
        }
    }
}
