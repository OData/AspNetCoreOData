// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Wrapper
{
    public class SelectExpandWrapperConverterTests
    {
        private static IEdmModel _edmModel = GetEdmModel();

        [Theory]
        [InlineData(typeof(SelectAllAndExpand<object>), true)]
        [InlineData(typeof(SelectAll<object>), true)]
        [InlineData(typeof(SelectExpandWrapper<object>), true)]
        [InlineData(typeof(SelectSomeAndInheritance<object>), true)]
        [InlineData(typeof(SelectSome<object>), true)]
        [InlineData(typeof(SelectExpandWrapper), false)]
        [InlineData(typeof(FlatteningWrapper<object>), false)]
        [InlineData(typeof(NoGroupByWrapper), false)]
        [InlineData(typeof(object), false)]
        [InlineData(null, false)]
        public void CanConvertWorksForSelectExpandWrapper(Type type, bool expected)
        {
            // Arrange
            SelectExpandWrapperConverter converter = new SelectExpandWrapperConverter();

            // Act & Assert
            Assert.Equal(expected, converter.CanConvert(type));
        }

        [Theory]
        [InlineData(typeof(SelectAllAndExpand<object>), typeof(SelectAllAndExpandConverter<object>))]
        [InlineData(typeof(SelectAll<object>), typeof(SelectAllConverter<object>))]
        [InlineData(typeof(SelectSomeAndInheritance<object>), typeof(SelectSomeAndInheritanceConverter<object>))]
        [InlineData(typeof(SelectSome<object>), typeof(SelectSomeConverter<object>))]
        [InlineData(typeof(SelectExpandWrapper<object>), typeof(SelectExpandWrapperConverter<object>))]
        [InlineData(typeof(SelectExpandWrapper), null)]
        [InlineData(typeof(FlatteningWrapper<object>), null)]
        [InlineData(typeof(NoGroupByWrapper), null)]
        [InlineData(typeof(object), null)]
        [InlineData(null, null)]
        public void CreateConverterWorksForSelectExpandWrapper(Type type, Type expected)
        {
            // Arrange
            JsonSerializerOptions options = new JsonSerializerOptions();
            SelectExpandWrapperConverter converter = new SelectExpandWrapperConverter();

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
        public void SelectExpandWrapperConverter_Works_SelectExpandWrapper()
        {
            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterRead<SelectExpandWrapper<SelectExpandWrapperEntity>>();

            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterWrite<SelectExpandWrapper<SelectExpandWrapperEntity>>();
        }

        [Fact]
        public void SelectSomeAndInheritanceWrapperConverter_Works_SelectSomeAndInheritance()
        {
            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterRead<SelectSomeAndInheritance<SelectExpandWrapperEntity>>();

            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterWrite<SelectSomeAndInheritance<SelectExpandWrapperEntity>>();
        }

        [Fact]
        public void SelectAllWrapperConverter_Works_SelectAll()
        {
            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterRead<SelectAll<SelectExpandWrapperEntity>>();

            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterWrite<SelectAll<SelectExpandWrapperEntity>>();
        }

        [Fact]
        public void SelectAllAndExpandWrapperConverter_Works_SelectAllAndExpand()
        {
            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterRead<SelectAllAndExpand<SelectExpandWrapperEntity>>();

            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterWrite<SelectAllAndExpand<SelectExpandWrapperEntity>>();
        }

        [Fact]
        public void SelectSomeWrapperConverter_Works_SelectSome()
        {
            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterRead<SelectSome<SelectExpandWrapperEntity>>();

            // Arrange & Act & Assert
            TestSelectExpandWrapperConverterWrite<SelectSome<SelectExpandWrapperEntity>>();
        }

        internal static void TestSelectExpandWrapperConverterRead<T>() where T : SelectExpandWrapper
        {
            // Arrange
            JsonSerializerOptions options = new JsonSerializerOptions();
            SelectExpandWrapperConverter converter = new SelectExpandWrapperConverter();
            JsonConverter<T> typeConverter = converter.CreateConverter(typeof(T), options) as JsonConverter<T>;

            try
            {
                // Act
                ReadOnlySpan<byte> jsonReadOnlySpan = Encoding.UTF8.GetBytes("any");
                Utf8JsonReader reader = new Utf8JsonReader(jsonReadOnlySpan);
                typeConverter.Read(ref reader, typeof(object), options);
            }
            catch (NotImplementedException ex)
            {
                // Assert
                Assert.Equal($"'{typeof(T).Name}' is internal and should never be deserialized into.", ex.Message);
            }
        }

        internal static void TestSelectExpandWrapperConverterWrite<T>() where T : SelectExpandWrapper
        {
            // Arrange
            T wrapper = (T)Activator.CreateInstance(typeof(T));
            MockPropertyContainer container = new MockPropertyContainer();

            wrapper.Container = new MockPropertyContainer();
            wrapper.Model = _edmModel;
            wrapper.UseInstanceForProperties = true;

            SelectExpandWrapper<SelectExpandWrapperEntity> selectExpandWrapper = wrapper as SelectExpandWrapper<SelectExpandWrapperEntity>;
            Assert.NotNull(selectExpandWrapper);
            selectExpandWrapper.Instance = new SelectExpandWrapperEntity
            {
                Name = "abc"
            };

            JsonSerializerOptions options = new JsonSerializerOptions();
            SelectExpandWrapperConverter converterFactory = new SelectExpandWrapperConverter();
            JsonConverter<T> typeConverter = converterFactory.CreateConverter(typeof(T), options) as JsonConverter<T>;

            // Act
            string json = SerializeUtils.SerializeAsJson(jsonWriter => typeConverter.Write(jsonWriter, wrapper, options));

            // Assert
            Assert.Equal("{\"Name\":\"abc\"}", json);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<SelectExpandWrapperEntity>();
            return builder.GetEdmModel();
        }

        private class SelectExpandWrapperEntity
        {
            public string Name { get; set; }
        }
    }
}
