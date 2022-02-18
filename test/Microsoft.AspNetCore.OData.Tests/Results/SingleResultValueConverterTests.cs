//-----------------------------------------------------------------------------
// <copyright file="SingleResultValueConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Results
{
    public class SingleResultValueConverterTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(SingleResult<object>), true)]
        public void CanConvert_WorksForSingleResultValueConverter(Type type, bool expected)
        {
            // Arrange
            SingleResultValueConverter converter = new SingleResultValueConverter();

            // Act & Assert
            Assert.Equal(expected, converter.CanConvert(type));
        }

        [Fact]
        public void CreateConverter_WorksForSingleResultValueConverter()
        {
            // Arrange
            JsonSerializerOptions options = new JsonSerializerOptions();
            SingleResultValueConverter converter = new SingleResultValueConverter();

            // Act & Assert
            Type type = typeof(SingleResult<object>);
            JsonConverter typeConverter = converter.CreateConverter(type, options);
            Assert.Equal(typeof(SingleResultConverter<object>), typeConverter.GetType());

            // Act & Assert
            type = typeof(IEnumerable<object>);
            typeConverter = converter.CreateConverter(type, options);
            Assert.Null(typeConverter);
        }

        [Fact]
        public void SingleResultValueConverter_CanSerializeSingleResultOfT()
        {
            // Arrange & Act & Assert
            IEnumerable<Customer> customers = new Customer[]
            {
                new Customer { Id = 1, Name = "abc" },
                new Customer { Id = 2, Name = "efg" }
            };

            SingleResult<Customer> result = new SingleResult<Customer>(customers.AsQueryable());

            JsonSerializerOptions options = new JsonSerializerOptions();
            SingleResultValueConverter converterFactory = new SingleResultValueConverter();
            Type type = typeof(SingleResult<Customer>);
            SingleResultConverter<Customer> typeConverter = converterFactory.CreateConverter(type, options) as SingleResultConverter<Customer>;

            // Act
            string json = SerializeUtils.SerializeAsJson(jsonWriter => typeConverter.Write(jsonWriter, result, options));

            // Assert
            Assert.Equal("{\"Id\":1,\"Name\":\"abc\"}", json);
        }

        private class Customer
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
