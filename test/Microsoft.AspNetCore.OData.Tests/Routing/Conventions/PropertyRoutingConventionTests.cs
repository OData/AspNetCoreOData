// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class PropertyRoutingConventionTests
    {
        private static PropertyRoutingConvention PropertyConvention = ConventionHelpers.CreateConvention<PropertyRoutingConvention>();
        private static IEdmModel EdmModel = GetEdmModel();

        [Theory]
        [InlineData(typeof(CustomersController), true)]
        [InlineData(typeof(MeController), true)]
        [InlineData(typeof(UnknownController), false)]
        public void AppliesToControllerReturnsExpectedForController(Type controllerType, bool expected)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType);
            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);

            // Act
            bool actual = PropertyConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static TheoryDataSet<Type, string, string[]> PropertyRoutingConventionTestData
        {
            get
            {
                return new TheoryDataSet<Type, string, string[]>()
                {
                    // Get
                    {
                        typeof(CustomersController),
                        "GetName",
                        new[]
                        {
                            "/Customers({key})/Name",
                            "/Customers/{key}/Name",
                            "/Customers({key})/Name/$value",
                            "/Customers/{key}/Name/$value"
                        }
                    },
                    { typeof(MeController), "GetName", new[] { "/Me/Name", "/Me/Name/$value" } },
                    {
                        typeof(CustomersController),
                        "GetEmails",
                        new[]
                        {
                            "/Customers({key})/Emails",
                            "/Customers/{key}/Emails",
                            "/Customers({key})/Emails/$count",
                            "/Customers/{key}/Emails/$count"
                        }
                    },
                    { typeof(MeController), "GetEmails", new[] { "/Me/Emails", "/Me/Emails/$count" } },

                    // Get complex property
                    { typeof(CustomersController), "GetAddress", new[] { "/Customers({key})/Address", "/Customers/{key}/Address" } },
                    { typeof(MeController), "GetAddress", new[] { "/Me/Address" } },
                    {
                        typeof(CustomersController),
                        "GetLocations",
                        new[]
                        {
                            "/Customers({key})/Locations",
                            "/Customers/{key}/Locations",
                            "/Customers({key})/Locations/$count",
                            "/Customers/{key}/Locations/$count"
                        }
                    },
                    { typeof(MeController), "GetLocations", new[] { "/Me/Locations", "/Me/Locations/$count" } },

                    // Post
                    { typeof(CustomersController), "PostToEmails", new[] { "/Customers({key})/Emails", "/Customers/{key}/Emails" } },
                    { typeof(MeController), "PostToEmails", new[] { "/Me/Emails" } },

                    // Put, Patch, Delete
                    { typeof(CustomersController), "PutToName", new[] { "/Customers({key})/Name", "/Customers/{key}/Name" } },
                    { typeof(CustomersController), "PatchToName", new[] { "/Customers({key})/Name", "/Customers/{key}/Name" } },
                    { typeof(CustomersController), "DeleteToName", new[] { "/Customers({key})/Name", "/Customers/{key}/Name" } },
                    { typeof(MeController), "PutToName", new[] { "/Me/Name" } },
                    { typeof(MeController), "PatchToName", new[] { "/Me/Name" } },
                    { typeof(MeController), "DeleteToName", new[] { "/Me/Name" } },

                    // with type cast
                    {
                        typeof(CustomersController),
                        "GetSubAddressFromVipCustomer",
                        new[]
                        {
                            "/Customers({key})/NS.VipCustomer/SubAddress",
                            "/Customers/{key}/NS.VipCustomer/SubAddress"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "PutToLocationsOfUsAddress",
                        new[]
                        {
                            "/Customers({key})/Locations/NS.UsAddress",
                            "/Customers/{key}/Locations/NS.UsAddress"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "PatchToSubAddressOfCnAddressFromVipCustomer",
                        new[]
                        {
                            "/Customers({key})/NS.VipCustomer/SubAddress/NS.CnAddress",
                            "/Customers/{key}/NS.VipCustomer/SubAddress/NS.CnAddress"
                        }
                    },
                    {
                        typeof(MeController),
                        "PutToSubAddressOfCnAddressFromVipCustomer",
                        new[]
                        {
                            "/Me/NS.VipCustomer/SubAddress/NS.CnAddress"
                        }
                    },
                    {
                        typeof(MeController),
                        "PostToSubLocationsOfUsAddressFromVipCustomer",
                        new[]
                        {
                            "/Me/NS.VipCustomer/SubLocations/NS.UsAddress"
                        }
                    },
                    {
                        typeof(MeController),
                        "GetSubLocationsOfUsAddressFromVipCustomer",
                        new[]
                        {
                            "/Me/NS.VipCustomer/SubLocations/NS.UsAddress",
                            "/Me/NS.VipCustomer/SubLocations/NS.UsAddress/$count"
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(PropertyRoutingConventionTestData))]
        public void PropertyRoutingConventionTestDataRunsAsExpected(Type controllerType, string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType, actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            // Act
            bool returnValue = PropertyConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("PostToName")]
        [InlineData("Get")]
        [InlineData("PostToSubAddressOfUsAddressFromVipCustomer")]
        public void PropertyRoutingConventionDoesNothingForNotSupportedAction(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<AnotherCustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            // Act
            bool returnValue = PropertyConvention.AppliesToAction(context);
            Assert.False(returnValue);

            // Assert
            Assert.Empty(action.Selectors);
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // Address
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // CnAddess
            EdmComplexType cnAddress = new EdmComplexType("NS", "CnAddress", address);
            cnAddress.AddStructuralProperty("Postcode", EdmPrimitiveTypeKind.String);
            model.AddElement(cnAddress);

            // UsAddress
            EdmComplexType usAddress = new EdmComplexType("NS", "UsAddress", address);
            usAddress.AddStructuralProperty("Zipcode", EdmPrimitiveTypeKind.Int32);
            model.AddElement(usAddress);

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            customer.AddStructuralProperty("Emails", new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, false))));
            customer.AddStructuralProperty("Address", new EdmComplexTypeReference(address, false));
            customer.AddStructuralProperty("Locations", new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(address, false))));
            model.AddElement(customer);

            // VipCustomer
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            vipCustomer.AddStructuralProperty("SubAddress", new EdmComplexTypeReference(address, false));
            vipCustomer.AddStructuralProperty("SubLocations", new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(address, false))));
            model.AddElement(vipCustomer);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            container.AddEntitySet("Customers", customer);
            container.AddSingleton("Me", customer);
            model.AddElement(container);
            return model;
        }

        private class CustomersController
        {
            public void GetName(int key, CancellationToken cancellation)
            { }

            public void GetAddress(int key, CancellationToken cancellation)
            { }

            public void GetLocations(int key)
            { }

            public void GetEmails(int key)
            { }

            public void PutToName(int key)
            { }

            public void PatchToName(CancellationToken cancellation, int key)
            { }

            public void DeleteToName(int key)
            { }

            public void PostToEmails(int key)
            { }

            public void GetSubAddressFromVipCustomer(int key)
            { }

            public void PutToLocationsOfUsAddress(int key)
            { }

            // PATCH ~/Customers(1)/NS.VipCustomer/SubAddress/NS.CnAddress
            public void PatchToSubAddressOfCnAddressFromVipCustomer(int key)
            { }
        }

        private class MeController
        {
            public void GetName(CancellationToken cancellation)
            { }

            public void GetAddress()
            { }

            public void GetEmails()
            { }

            public void GetLocations()
            { }

            public void PutToName()
            { }

            public void PatchToName(CancellationToken cancellation)
            { }

            public void DeleteToName()
            { }

            public void PostToEmails()
            { }

            // Get ~/Me/NS.VipCustomer/SubAddress/CN.UsAddress
            public void GetSubLocationsOfUsAddressFromVipCustomer()
            { }

            // PATCH ~/Me/NS.VipCustomer/SubAddress/CN.CnAddress
            public void PutToSubAddressOfCnAddressFromVipCustomer()
            { }

            // Post ~/Me/NS.VipCustomer/SubAddress/CN.UsAddress
            public void PostToSubLocationsOfUsAddressFromVipCustomer()
            { }
        }

        private class AnotherCustomersController
        {
            public void PostToName(string keyLastName, string keyFirstName)
            { }

            public void Get(int key)
            { }

            // Post to non-collection is not allowed.
            public void PostToSubAddressOfUsAddressFromVipCustomer()
            { }
        }

        private class UnknownController
        { }
    }
}