// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    internal static class ControllerModelHelpers
    {
        public static ControllerModel BuildControllerModel<T>(params string[] actions)
        {
            return BuildControllerModel(typeof(T), actions);
        }

        public static ControllerModel BuildControllerModel(Type controllerType, params string[] actions)
        {
            object[] controllerAttributes = controllerType.GetCustomAttributes(inherit: true);

            TypeInfo typeInfo = controllerType.GetTypeInfo();
            ControllerModel controller = new ControllerModel(controllerType.GetTypeInfo(), controllerAttributes);

            controller.ControllerName =
                typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) ?
                    typeInfo.Name.Substring(0, typeInfo.Name.Length - "Controller".Length) :
                    typeInfo.Name;

            foreach (var actionName in actions)
            {
                MethodInfo methodInfo = controllerType.GetMethod(actionName);
                Assert.NotNull(methodInfo);

                object[] attributes = methodInfo.GetCustomAttributes(inherit: true);
                ActionModel actionModel = new ActionModel(methodInfo, attributes);

                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    object[] paramAttributes = parameterInfo.GetCustomAttributes(inherit: true);
                    ParameterModel parameterModel = new ParameterModel(parameterInfo, paramAttributes)
                    {
                        ParameterName = parameterInfo.Name,
                    };

                    actionModel.Parameters.Add(parameterModel);
                }

                controller.Actions.Add(actionModel);
            }

            return controller;
        }

        public static ControllerModel BuildControllerModelByMethodInfo<T>(params MethodInfo[] methodInfos)
        {
            return BuildControllerModelByMethodInfo(typeof(T), methodInfos);
        }

        public static ControllerModel BuildControllerModelByMethodInfo(Type controllerType, params MethodInfo[] methodInfos)
        {
            object[] controllerAttributes = controllerType.GetCustomAttributes(inherit: true);

            TypeInfo typeInfo = controllerType.GetTypeInfo();
            ControllerModel controller = new ControllerModel(controllerType.GetTypeInfo(), controllerAttributes);

            controller.ControllerName =
                typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) ?
                    typeInfo.Name.Substring(0, typeInfo.Name.Length - "Controller".Length) :
                    typeInfo.Name;

            foreach (var methodInfo in methodInfos)
            {
                object[] attributes = methodInfo.GetCustomAttributes(inherit: true);
                ActionModel actionModel = new ActionModel(methodInfo, attributes);

                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    object[] paramAttributes = parameterInfo.GetCustomAttributes(inherit: true);
                    ParameterModel parameterModel = new ParameterModel(parameterInfo, paramAttributes)
                    {
                        ParameterName = parameterInfo.Name,
                    };

                    actionModel.Parameters.Add(parameterModel);
                }

                controller.Actions.Add(actionModel);
            }

            return controller;
        }

        public static ControllerModel BuildControllerModelWithAllActions<T>()
        {
            return BuildControllerModelWithAllActions(typeof(T));
        }

        public static ControllerModel BuildControllerModelWithAllActions(Type controllerType)
        {
            object[] controllerAttributes = controllerType.GetCustomAttributes(inherit: true);

            TypeInfo typeInfo = controllerType.GetTypeInfo();
            ControllerModel controller = new ControllerModel(controllerType.GetTypeInfo(), controllerAttributes);

            controller.ControllerName =
                typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) ?
                    typeInfo.Name.Substring(0, typeInfo.Name.Length - "Controller".Length) :
                    typeInfo.Name;

            foreach (var methodInfo in controllerType.GetMethods())
            {
                if (!methodInfo.IsAction())
                {
                    continue;
                }

                object[] attributes = methodInfo.GetCustomAttributes(inherit: true);
                ActionModel actionModel = new ActionModel(methodInfo, attributes);

                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    object[] paramAttributes = parameterInfo.GetCustomAttributes(inherit: true);
                    ParameterModel parameterModel = new ParameterModel(parameterInfo, paramAttributes)
                    {
                        ParameterName = parameterInfo.Name,
                    };

                    actionModel.Parameters.Add(parameterModel);
                }

                controller.Actions.Add(actionModel);
            }

            return controller;
        }
    }
}
