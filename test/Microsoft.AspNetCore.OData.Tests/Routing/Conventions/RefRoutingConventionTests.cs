// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class RefRoutingConventionTests
    {
        private static IEdmModel _edmModel = GetModel();
        private static RefRoutingConvention _refConvention = ConventionHelpers.CreateConvention<RefRoutingConvention>();

        [Fact]
        public void SplitRefActionName_WorksAsExpected()
        {
            // Arranges & Act & Assert
            string actionMethodName = "CreateRef";
            string method = RefRoutingConvention.SplitRefActionName(actionMethodName, out string httpMethod, out string prop, out string declaring);
            Assert.Equal("Post,Patch", httpMethod);
            Assert.Equal("CreateRef", method);
            Assert.Null(prop);
            Assert.Null(declaring);

            // Arranges & Act & Assert
            actionMethodName = "GetRefToPropertyFromAbc";
            method = RefRoutingConvention.SplitRefActionName(actionMethodName, out httpMethod, out prop, out declaring);
            Assert.Equal("Get", httpMethod);
            Assert.Equal("GetRef", method);
            Assert.Equal("Property", prop);
            Assert.Equal("Abc", declaring);

            // Arranges & Act & Assert
            actionMethodName = "CreateRefFromAbcToProperty";
            method = RefRoutingConvention.SplitRefActionName(actionMethodName, out httpMethod, out prop, out declaring);
            Assert.Null(method);
        }

        public static TheoryDataSet<string, string[]> RefConventionTestData
        {
            get
            {
                return new TheoryDataSet<string, string[]>()
                {
                    // Bound to single
                    {
                        "CreateRefToOrder",
                        new[]
                        {
                            "/RefCustomers({key})/Order/$ref",
                            "/RefCustomers/{key}/Order/$ref"
                        }
                    },
                    {
                        "DeleteRefToOrder",
                        new[]
                        {
                            "/RefCustomers({key})/Order/$ref",
                            "/RefCustomers/{key}/Order/$ref"
                        }
                    },
                    {
                        "DeleteRefToOrders",
                        new[]
                        {
                            "/RefCustomers({key})/Orders({relatedKey})/$ref",
                            "/RefCustomers({key})/Orders/{relatedKey}/$ref",
                            "/RefCustomers/{key}/Orders({relatedKey})/$ref",
                            "/RefCustomers/{key}/Orders/{relatedKey}/$ref"
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(RefConventionTestData))]
        public void SingletonRoutingConventionTestDataRunsAsExpected(string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<RefCustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, _edmModel, controller);
            context.Action = action;

            // Act
            _refConvention.AppliesToAction(context);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        public static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<RefCustomer>("RefCustomers");
            builder.EntitySet<RefOrder>("RefOrders");

            return builder.GetEdmModel();
        }

        private class RefCustomer
        {
            public int Id { get; set; }
            public RefOrder Order { get; set; }
            public RefOrder[] Orders { get; set; }
        }

        private class RefOrder
        {
            public int Id { get; set; }
        }

        private class RefCustomersController
        {
            [HttpPost]
            public string CreateRefToOrder(int key, string navigationProperty)
            {
                return string.Format(CultureInfo.InvariantCulture, "CreateRef({0})({1})", key, navigationProperty);
            }

            public string DeleteRefToOrder(int key, string navigationProperty)
            {
                return string.Format(CultureInfo.InvariantCulture, "DeleteRef({0})({1})", key, navigationProperty);
            }

            public string DeleteRefToOrders(int key, int relatedKey, string navigationProperty)
            {
                return string.Format(CultureInfo.InvariantCulture, "DeleteRef({0})({1})({2})", key, relatedKey, navigationProperty);
            }
        }
    }
}