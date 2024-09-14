//-----------------------------------------------------------------------------
// <copyright file="JSingleResultValueConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Results;
using Xunit;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson.Tests;

public class JSingleResultValueConverterTests
{
    private static IQueryable<SingleCustomer> _customers = new List<SingleCustomer>
    {
        new SingleCustomer
        {
            Id = 1,
            Name = "XU"
        },
        new SingleCustomer
        {
            Id = 2,
            Name = "WU"
        },
    }.AsQueryable();

    [Theory]
    [InlineData(typeof(SingleResult), true)]
    [InlineData(typeof(SingleResult<object>), true)]
    [InlineData(typeof(SelectExpandWrapper), false)]
    [InlineData(typeof(object), false)]
    public void CanConvertWorksForSingleResult(Type type, bool expected)
    {
        // Arrange
        JSingleResultValueConverter converter = new JSingleResultValueConverter();

        // Act & Assert
        Assert.Equal(expected, converter.CanConvert(type));
    }

    [Fact]
    public void ReadJsonForSingleResultThrowsNotImplementedException()
    {
        // Arrange
        JSingleResultValueConverter converter = new JSingleResultValueConverter();

        // Act
        Action test = () => converter.ReadJson(null, typeof(object), null, null);

        // Assert
        NotImplementedException exception = Assert.Throws<NotImplementedException>(test);
        Assert.Equal(SRResources.ReadSingleResultNotImplemented, exception.Message);
    }

    [Fact]
    public void CanWriteSingleResultToJsonUsingNewtonsoftJsonConverter()
    {
        // Arrange
        SingleResult<SingleCustomer> pageResult = new SingleResult<SingleCustomer>(_customers);
        JSingleResultValueConverter converter = new JSingleResultValueConverter();

        // Act
        string json = SerializeUtils.WriteJson(converter, pageResult);

        // Assert
        Assert.Equal("{\"Id\":1,\"Name\":\"XU\"}", json);
    }
}

public class SingleCustomer
{
    public int Id { get; set; }

    public string Name { get; set; }
}
