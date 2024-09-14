//-----------------------------------------------------------------------------
// <copyright file="JPageResultValueConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Results;
using Xunit;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson.Tests;

public class JPageResultValueConverterTests
{
    private static IList<PageCustomer> _customers = new List<PageCustomer>
    {
        new PageCustomer
        {
            Id = 1,
            Name = "XU"
        },
        new PageCustomer
        {
            Id = 2,
            Name = "WU"
        },
    };

    [Theory]
    [InlineData(typeof(PageResult), true)]
    [InlineData(typeof(PageResult<object>), true)]
    [InlineData(typeof(SelectExpandWrapper), false)]
    [InlineData(typeof(object), false)]
    public void CanConvertWorksForPageResult(Type type, bool expected)
    {
        // Arrange
        JPageResultValueConverter converter = new JPageResultValueConverter();

        // Act & Assert
        Assert.Equal(expected, converter.CanConvert(type));
    }

    [Fact]
    public void ReadJsonForPageResultThrowsNotImplementedException()
    {
        // Arrange
        JPageResultValueConverter converter = new JPageResultValueConverter();

        // Act
        Action test = () => converter.ReadJson(null, typeof(object), null, null);

        // Assert
        NotImplementedException exception = Assert.Throws<NotImplementedException>(test);
        Assert.Equal(SRResources.ReadPageResultNotImplemented, exception.Message);
    }

    [Fact]
    public void CanWritePageResultOnlyWithEnumerableToJsonUsingNewtonsoftJsonConverter()
    {
        // Arrange
        PageResult<PageCustomer> pageResult = new PageResult<PageCustomer>(_customers, null, null);
        JPageResultValueConverter converter = new JPageResultValueConverter();

        // Act
        string json = SerializeUtils.WriteJson(converter, pageResult);

        // Assert
        Assert.Equal("{\"items\":[{\"Id\":1,\"Name\":\"XU\"},{\"Id\":2,\"Name\":\"WU\"}]}", json);
    }

    [Fact]
    public void CanWritePageResultAllToJsonUsingNewtonsoftJsonConverter()
    {
        // Arrange
        Uri uri = new Uri("http://any");
        PageResult<PageCustomer> pageResult = new PageResult<PageCustomer>(_customers, uri, 4);
        JPageResultValueConverter converter = new JPageResultValueConverter();

        // Act
        string json = SerializeUtils.WriteJson(converter, pageResult, true);

        // Assert
        Assert.Equal(@"{
  ""items"": [
    {
      ""Id"": 1,
      ""Name"": ""XU""
    },
    {
      ""Id"": 2,
      ""Name"": ""WU""
    }
  ],
  ""nextpagelink"": ""http://any"",
  ""count"": 4
}", json);
    }
}

public class PageCustomer
{
    public int Id { get; set; }

    public string Name { get; set; }
}
