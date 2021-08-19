//-----------------------------------------------------------------------------
// <copyright file="PageResultOfTTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Results
{
    public class PageResultOfTTests
    {
        [Fact]
        public void CtorPageResultOfT_ThrowsArgumentNull_Items()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new PageResult<Customer>(null, null, null), "items");
        }

        [Fact]
        public void CtorPageResultOfT_SetProperties()
        {
            // Arrange
            IEnumerable<Customer> customers = Enumerable.Empty<Customer>();
            Uri nextPageLink = new Uri("http://any");
            long? count = 1;

            // Act
            PageResult<Customer> result = new PageResult<Customer>(customers, nextPageLink, count);

            // Assert
            Assert.Same(result.Items, customers);
            Assert.Same(nextPageLink, result.NextPageLink);
            Assert.Equal(1, result.Count);
        }

        [Fact]
        public void CtorPageResultOfT_ThrowsArgumentOutOfRange_NegativeCount()
        {
            // Arrange
            IEnumerable<Customer> customers = Enumerable.Empty<Customer>();
            Uri nextPageLink = new Uri("http://any");
            long? count = -1;

            // Act
            Action test = () => new PageResult<Customer>(customers, nextPageLink, count);

            // Assert
            ArgumentOutOfRangeException exception = ExceptionAssert.Throws<ArgumentOutOfRangeException>(test);
            Assert.Contains("Value must be greater than or equal to 0. (Parameter 'value')\r\nActual value was -1.", exception.Message);
        }

        [Fact]
        public void ToDictionaryPageResultOfT_Works()
        {
            // Arrange
            IEnumerable<Customer> customers = new Customer[]
            {
                new Customer(),
                new Customer()
            };
            Uri nextPageLink = new Uri("http://any");
            long? count = 2;

            // Act
            PageResult<Customer> result = new PageResult<Customer>(customers, nextPageLink, count);
            IDictionary<string, object> dics = result.ToDictionary();

            // Assert
            Assert.Equal(3, dics.Count);
            Assert.Collection(dics,
                e =>
                {
                    Assert.Equal("items", e.Key);
                    Assert.Same(customers, e.Value);
                },
                e =>
                {
                    Assert.Equal("nextpagelink", e.Key);
                    Assert.Equal("http://any", e.Value);
                },
                e =>
                {
                    Assert.Equal("count", e.Key);
                    Assert.Equal((long)2, e.Value);
                });
        }

        private class Customer
        {
        }
    }
}
