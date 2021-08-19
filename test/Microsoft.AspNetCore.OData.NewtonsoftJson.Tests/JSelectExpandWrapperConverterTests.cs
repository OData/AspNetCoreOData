//-----------------------------------------------------------------------------
// <copyright file="JSelectExpandWrapperConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.NewtonsoftJson.Tests.Models;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson.Tests
{
    public class JSelectExpandWrapperConverterTests
    {
        private static IEdmModel _edmModel = GetEdmModel();

        [Theory]
        [InlineData(typeof(SelectAllAndExpand<object>), true)]
        [InlineData(typeof(SelectAll<object>), true)]
        [InlineData(typeof(SelectExpandWrapper<object>), true)]
        [InlineData(typeof(SelectSomeAndInheritance<object>), true)]
        [InlineData(typeof(SelectSome<object>), true)]
        [InlineData(typeof(SelectExpandWrapper), true)]
        [InlineData(typeof(FlatteningWrapper<object>), false)]
        [InlineData(typeof(NoGroupByWrapper), false)]
        [InlineData(typeof(object), false)]
        public void CanConvertWorksForSelectExpandWrapper(Type type, bool expected)
        {
            // Arrange
            JSelectExpandWrapperConverter converter = new JSelectExpandWrapperConverter();

            // Act & Assert
            Assert.Equal(expected, converter.CanConvert(type));
        }

        [Fact]
        public void ReadJsonThrowsNotImplementedException()
        {
            // Arrange
            JSelectExpandWrapperConverter converter = new JSelectExpandWrapperConverter();

            // Act
            Action test = () => converter.ReadJson(null, typeof(object), null, null);

            // Assert
            NotImplementedException exception = Assert.Throws<NotImplementedException>(test);
            Assert.Equal(SRResources.ReadSelectExpandWrapperNotImplemented, exception.Message);
        }

        [Fact]
        public void CanWriteSelectAllToJsonUsingNewtonsoftJsonConverter()
        {
            // Arrange
            SelectAll<Customer> selectAll = new SelectAll<Customer>();
            selectAll.Container = null;
            selectAll.Model = _edmModel;
            selectAll.UseInstanceForProperties = true;
            selectAll.Instance = new Customer
            {
                Id = 2,
                Name = "abc",
                Location = new Address
                {
                    Street = "37TH PL",
                    City = "Reond"
                }
            };

            JSelectExpandWrapperConverter converter = new JSelectExpandWrapperConverter();

            // Act
            string json = SerializeUtils.WriteJson(converter, selectAll);

            // Assert
            Assert.Equal("{\"Id\":2,\"Name\":\"abc\",\"Location\":{\"Street\":\"37TH PL\",\"City\":\"Reond\"}}", json);
        }

        [Fact]
        public void CanWriteSelectAllAndExpandToJsonUsingNewtonsoftJsonConverter()
        {
            // Arrange
            SelectAll<Order> expandOrder = new SelectAll<Order>();
            expandOrder.Model = _edmModel;
            expandOrder.UseInstanceForProperties = true;
            expandOrder.Instance = new Order
            {
                Id = 21,
                Title = "new Order21"
            };

            SelectAllAndExpand<Customer> selectExpand = new SelectAllAndExpand<Customer>();
            MockPropertyContainer container = new MockPropertyContainer();
            container.Properties["Orders"] = expandOrder; // expanded
            selectExpand.Container = container;
            selectExpand.Model = _edmModel;
            selectExpand.UseInstanceForProperties = true;
            selectExpand.Instance = new Customer
            {
                Id = 2,
                Name = "abc",
                Location = new Address
                {
                    Street = "37TH PL",
                    City = "Reond"
                }
            };

            JSelectExpandWrapperConverter converter = new JSelectExpandWrapperConverter();

            // Act
            string json = SerializeUtils.WriteJson(converter, selectExpand, true);

            // Assert
            Assert.Equal(@"{
  ""Orders"": {
    ""Id"": 21,
    ""Title"": ""new Order21""
  },
  ""Id"": 2,
  ""Name"": ""abc"",
  ""Location"": {
    ""Street"": ""37TH PL"",
    ""City"": ""Reond""
  }
}", json);
        }

        [Fact]
        public void CanWriteSelectSomeWrapperToJsonUsingNewtonsoftJsonConverter()
        {
            // Arrange
            SelectSome<Customer> selectSome = new SelectSome<Customer>();
            MockPropertyContainer container = new MockPropertyContainer();
            container.Properties["Name"] = "sam";
            selectSome.Container = container;
            selectSome.Model = _edmModel;

            JSelectExpandWrapperConverter converter = new JSelectExpandWrapperConverter();

            // Act
            string json = SerializeUtils.WriteJson(converter, selectSome);

            // Assert
            Assert.Equal("{\"Name\":\"sam\"}", json);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<Customer>();
            return builder.GetEdmModel();
        }
    }
}
