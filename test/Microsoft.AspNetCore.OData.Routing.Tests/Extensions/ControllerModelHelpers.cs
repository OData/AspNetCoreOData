// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNetCore.OData.Routing.Tests.Extensions
{
    public static class ControllerModelHelpers
    {
        public static ControllerModel BuildControllerModel<T>(params string[] actions)
        {
            return BuildControllerModel(typeof(T), actions);
        }

        public static ControllerModel BuildControllerModel(Type controllerType, params string[] actions)
        {
            object[] controllerAttributes = controllerType.GetCustomAttributes(inherit: true);
            ControllerModel controller = new ControllerModel(controllerType.GetTypeInfo(), controllerAttributes);

            foreach (var actionName in actions)
            {
                MethodInfo methodInfo = controllerType.GetMethod(actionName);
                Assert.NotNull(methodInfo);

                object[] attributes = methodInfo.GetCustomAttributes(inherit: true);
                ActionModel actionModel = new ActionModel(methodInfo, attributes);
                controller.Actions.Add(actionModel);
            }

            return controller;
        }
    }
}
