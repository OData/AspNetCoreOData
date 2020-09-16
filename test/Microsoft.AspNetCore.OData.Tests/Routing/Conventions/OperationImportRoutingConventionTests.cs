// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class OperationImportRoutingConventionTests
    {
        private static OperationImportRoutingConvention ImportConvention = ConventionHelpers.CreateConvention<OperationImportRoutingConvention>();
        private static IEdmModel EdmModel = GetEdmModel();

        [Theory]
        [InlineData(typeof(ODataOperationImportController), true)]
        [InlineData(typeof(UnknownController), false)]
        public void AppliesToControllerReturnsExpectedForController(Type controllerType, bool expected)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType);
            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);

            // Act
            bool actual = ImportConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static TheoryDataSet<MethodInfo, string[]> ImportRoutingConventionTestData
        {
            get
            {
                Type controller = typeof(ODataOperationImportController);
                MethodInfo method1 = controller.GetMethod("CalcByRating", new Type[] { typeof(int) });
                MethodInfo method2 = controller.GetMethod("CalcByRating", new Type[] { typeof(string) });
                return new TheoryDataSet<MethodInfo, string[]>()
                {
                    {
                        method1, new[] { "CalcByRating(order={order})" }
                    },
                    {
                        method2, new[] { "CalcByRating(name={name})" }
                    },
                    {
                        controller.GetMethod("CalcByRatingAction"), new[] { "CalcByRatingAction" }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ImportRoutingConventionTestData))]
        public void OperationImportRoutingConventionResolveFunctionOverloadAsExpected(MethodInfo method, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModelByMethodInfo<ODataOperationImportController>(method);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            // Act
            ImportConvention.AppliesToAction(context);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("OtherFunctionImport")]
        [InlineData("OtherActionImport")]
        public void OperationImportRoutingConventionDoesNothingForNotSupportedAction(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<ODataOperationImportController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            // Act
            bool returnValue = ImportConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            Assert.Empty(action.Selectors);
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference stringType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunction calcByRating1 = new EdmFunction("NS", "CalcByRating", returnType, false, entitySetPathExpression: null, isComposable: false);
            calcByRating1.AddParameter("order", intType);
            model.AddElement(calcByRating1);
            container.AddFunctionImport(calcByRating1);

            EdmFunction calcByRating2 = new EdmFunction("NS", "CalcByRating", returnType, false, entitySetPathExpression: null, isComposable: false);
            calcByRating2.AddParameter("name", stringType);
            model.AddElement(calcByRating2);
            container.AddFunctionImport(calcByRating2);

            EdmAction calcByRatingAction = new EdmAction("NS", "CalcByRatingAction", returnType, false, null);
            calcByRatingAction.AddParameter("name", stringType);
            model.AddElement(calcByRatingAction);
            container.AddActionImport(calcByRatingAction);

            model.AddElement(container);
            return model;
        }

        private class ODataOperationImportController
        {
            [HttpGet]
            public void CalcByRating(int order)
            { }

            [HttpGet]
            public void CalcByRating(string name)
            { }

            [HttpPost]
            public void CalcByRatingAction(ODataActionParameters parameters)
            { }

            [HttpGet]
            public void OtherFunctionImport(string name)
            { }

            [HttpPost]
            public void OtherActionImport(ODataActionParameters parameters)
            { }
        }

        private class UnknownController
        { }
    }
}