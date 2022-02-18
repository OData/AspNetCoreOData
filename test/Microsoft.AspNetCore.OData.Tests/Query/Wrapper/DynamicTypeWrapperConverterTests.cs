//-----------------------------------------------------------------------------
// <copyright file="DynamicTypeWrapperConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text;
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
        [InlineData(null, false)]
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
        [InlineData(null, null)]
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
        public void AggregationWrapperConverter_Works_AggregationWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterRead<AggregationWrapper>();

            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterWrite<AggregationWrapper>();
        }

        [Fact]
        public void EntitySetAggregationWrapperConverter_Works_EntitySetAggregationWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterRead<EntitySetAggregationWrapper>();

            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterWrite<EntitySetAggregationWrapper>();
        }

        [Fact]
        public void GroupByWrapperWrapperConverter_Works_GroupByWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterRead<GroupByWrapper>();

            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterWrite<GroupByWrapper>();
        }

        [Fact]
        public void NoGroupByAggregationWrapperConverter_Works_NoGroupByAggregationWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterRead<NoGroupByAggregationWrapper>();

            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterWrite<NoGroupByAggregationWrapper>();
        }

        [Fact]
        public void NoGroupByWrapperWrapperConverter_Works_NoGroupByWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterRead<NoGroupByWrapper>();

            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterWrite<NoGroupByWrapper>();
        }

        internal static void TestDynamicTypeWrapperConverterRead<T>() where T : GroupByWrapper
        {
            // Arrange
            JsonSerializerOptions options = new JsonSerializerOptions();
            DynamicTypeWrapperConverter converter = new DynamicTypeWrapperConverter();
            JsonConverter<T> typeConverter = converter.CreateConverter(typeof(T), options) as JsonConverter<T>;

            try
            {
                // Act
                ReadOnlySpan<byte> jsonReadOnlySpan = Encoding.UTF8.GetBytes("any");
                Utf8JsonReader reader = new Utf8JsonReader(jsonReadOnlySpan);
                typeConverter.Read(ref reader, typeof(AggregationWrapper), options);
            }
            catch (NotImplementedException ex)
            {
                // Assert
                Assert.Equal($"'{typeof(T).Name}' is internal and should never be deserialized into.", ex.Message);
            }
        }

        internal static void TestDynamicTypeWrapperConverterWrite<T>() where T : GroupByWrapper
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
        public void ComputeWrapperOfTypeConverter_Works_ComputeWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterRead<ComputeWrapper<object>>();

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
        public void FlatteningWrapperOfTypeConverter_Works_FlatteningWrapper()
        {
            // Arrange & Act & Assert
            TestDynamicTypeWrapperConverterRead<FlatteningWrapper<object>>();

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
