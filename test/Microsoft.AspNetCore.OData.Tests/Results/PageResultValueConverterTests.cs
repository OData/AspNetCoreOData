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
using Microsoft.AspNetCore.OData.TestCommon;
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

    public static TheoryDataSet<string, JsonNamingPolicy> PageResultValueConverterData
    {
        get
        {
            return new TheoryDataSet<string, JsonNamingPolicy>
            {
                { "{\"items\":[{\"Id\":1,\"Name\":\"abc\"},{\"Id\":2,\"Name\":\"efg\"}],\"nextpagelink\":\"http://any\",\"count\":2}", null },
                { "{\"items\":[{\"id\":1,\"name\":\"abc\"},{\"id\":2,\"name\":\"efg\"}],\"nextpagelink\":\"http://any\",\"count\":2}", JsonNamingPolicy.CamelCase },
                { "{\"ITEMS\":[{\"ID\":1,\"NAME\":\"abc\"},{\"ID\":2,\"NAME\":\"efg\"}],\"NEXTPAGELINK\":\"http://any\",\"COUNT\":2}", JsonNamingPolicy.SnakeCaseUpper },
                { "{\"values\":[{\"Id\":1,\"Name\":\"abc\"},{\"Id\":2,\"Name\":\"efg\"}],\"next_page_link\":\"http://any\",\"count\":2}", new MyNamingPolicy() }
            };
        }
    }

    [Theory]
    [MemberData(nameof(PageResultValueConverterData))]
    public void PageResultValueConverter_CanSerializePageResultOfT_WithNamingPolicy(string expected, JsonNamingPolicy policy)
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

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = policy
        };

        PageResultValueConverter converterFactory = new PageResultValueConverter();
        Type type = typeof(PageResult<Customer>);
        PageResultConverter<Customer> typeConverter = converterFactory.CreateConverter(type, options) as PageResultConverter<Customer>;

        // Act
        string json = SerializeUtils.SerializeAsJson(jsonWriter => typeConverter.Write(jsonWriter, result, options));

        // Assert
        Assert.Equal(expected, json);
    }

    private class MyNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (name == "nextpagelink")
            {
                return "next_page_link";
            }

            if (name == "items")
            {
                return "values";
            }

            return name;
        }
    }

    private class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
