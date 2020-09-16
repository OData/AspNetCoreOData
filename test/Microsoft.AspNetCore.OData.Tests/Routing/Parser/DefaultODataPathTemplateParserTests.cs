// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Parser
{
    public class DefaultODataPathTemplateParserTests
    {
        private static IEdmModel EdmModel = GetEdmModel();

        //[Theory]
        //[InlineData("$metadata")]
        //[InlineData("Customers")]
        //[InlineData("Customers({key})")]
        //[InlineData("Me")]
        //[InlineData("RateByInfoImport(order={order},name={name})")]
        //public void CreateFirstSegmentWorksAsExpected(string identifier)
        //{
        //    IList<ODataSegmentTemplate> path = new List<ODataSegmentTemplate>();

        //    // Act
        //   // DefaultODataPathTemplateParser.CreateFirstSegment(identifier, EdmModel, path);
        //}

        [Theory]
        [InlineData("Customers", "GetCustomers")]
        [InlineData("Customers({key})/Orders", "GetOrdersOfACustomer")]
        [InlineData("Customers({key})", "GetCustomer")]
        [InlineData("VipCustomer", "GetVipCustomer")] // Singleton
        [InlineData("VipCustomer/Orders", "GetOrdersOfVipCustomer")] // Singleton/Navigation
        [InlineData("VipCustomer", "GetVipCustomerWithPrefix")] // Singleton
        [InlineData("VipCustomer/Name", "GetVipCustomerNameWithPrefix")] // Singleton/property
        [InlineData("VipCustomer/Orders", "GetVipCustomerOrdersWithPrefix")] // Singleton/Navigation
        [InlineData("VipCustomer", "GetCustomer")]
        [InlineData("VipCustomer/Orders", "GetOrdersOfACustomer")]
        public void ParseODataUriTemplateWorksAsExpected(string template, string expectedActionName)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(model, template, null);

            // Assert
            Assert.NotNull(expectedActionName);
            Assert.NotNull(path);
        }

        private static IEdmModel GetEdmModel()
        {
            var model = new EdmModel();
            var customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);

            var vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(customer);
            model.AddElement(vipCustomer);

            var order = new EdmEntityType("NS", "Order");
            order.AddKeys(order.AddStructuralProperty("OrderId", EdmPrimitiveTypeKind.Int32));
            model.AddElement(order);

            customer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Orders",
                Target = order,
                TargetMultiplicity = EdmMultiplicity.Many
            });

            var entityContainer = new EdmEntityContainer("NS", "Default");
            entityContainer.AddEntitySet("Customers", customer);
            entityContainer.AddSingleton("VipCustomer", customer);
            model.AddElement(entityContainer);

            return model;
        }
    }
}
