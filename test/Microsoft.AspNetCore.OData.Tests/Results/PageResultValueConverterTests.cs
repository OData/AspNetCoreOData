//-----------------------------------------------------------------------------
// <copyright file="PageResultValueConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Results;

public class PageResultValueConverterTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(PageResult<object>), true)]
    public void CanConvert_WorksForPageResultValueConverter(Type type, bool expected)
    {
        // Arrange
        PageResultValueConverter converter = new PageResultValueConverter();

        // Act & Assert
        Assert.Equal(expected, converter.CanConvert(type));
    }

    [Fact]
    public void CreateConverter_WorksForPageResultValueConverter()
    {
        // Arrange
        JsonSerializerOptions options = new JsonSerializerOptions();
        PageResultValueConverter converter = new PageResultValueConverter();

        // Act & Assert
        Type type = typeof(PageResult<object>);
        JsonConverter typeConverter = converter.CreateConverter(type, options);
        Assert.Equal(typeof(PageResultConverter<object>), typeConverter.GetType());

        // Act & Assert
        type = typeof(IEnumerable<object>);
        typeConverter = converter.CreateConverter(type, options);
        Assert.Null(typeConverter);
    }

    [Fact]
    public void PageResultValueConverter_CanSerializePageResultOfT()
    {
        // Arrange & Act & Assert
        IEnumerable<Customer> customers = new Customer[]
        {
            new Customer { Id = 1, Name = "abc" },
            new Customer { Id = 2, Name = "efg" },
        };
        Uri nextPageLink = new Uri("http://any");
        long? count = 2;
        PageResult<Customer> result = new PageResult<Customer>(customers, nextPageLink, count);

        JsonSerializerOptions options = new JsonSerializerOptions();
        PageResultValueConverter converterFactory = new PageResultValueConverter();
        Type type = typeof(PageResult<Customer>);
        PageResultConverter<Customer> typeConverter = converterFactory.CreateConverter(type, options) as PageResultConverter<Customer>;

        // Act
        string json = SerializeUtils.SerializeAsJson(jsonWriter => typeConverter.Write(jsonWriter, result, options));

        // Assert
        Assert.Equal("{\"items\":[{\"Id\":1,\"Name\":\"abc\"},{\"Id\":2,\"Name\":\"efg\"}],\"nextpagelink\":\"http://any\",\"count\":2}", json);
    }

    private class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
