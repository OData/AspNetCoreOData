//-----------------------------------------------------------------------------
// <copyright file="ActionModelHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    internal static class ActionModelHelpers
    {
        /// <summary>
        /// Build the actio nmodel using the method info.
        /// </summary>
        /// <param name="methodInfo">The input method info.</param>
        /// <returns>The action model.</returns>
        public static ActionModel BuildActionModel(this MethodInfo methodInfo)
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

            return actionModel;
        }

        /// <summary>
        /// Copied from ASP.NET Core and make some changes.
        /// Returns <c>true</c> if the <paramref name="methodInfo"/> is an action. Otherwise <c>false</c>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/>.</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/>.</param>
        /// <returns><c>true</c> if the <paramref name="methodInfo"/> is an action. Otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Override this method to provide custom logic to determine which methods are considered actions.
        /// </remarks>
        public static bool IsAction(this MethodInfo methodInfo)
        {
            Assert.NotNull(methodInfo);

            // The SpecialName bit is set to flag members that are treated in a special way by some compilers
            // (such as property accessors and operator overloading methods).
            if (methodInfo.IsSpecialName)
            {
                return false;
            }

            if (methodInfo.IsDefined(typeof(NonActionAttribute)))
            {
                return false;
            }

            // Overridden methods from Object class, e.g. Equals(Object), GetHashCode(), etc., are not valid.
            if (methodInfo.GetBaseDefinition().DeclaringType == typeof(object))
            {
                return false;
            }

            // Dispose method implemented from IDisposable is not valid
            if (IsIDisposableMethod(methodInfo))
            {
                return false;
            }

            if (methodInfo.IsStatic)
            {
                return false;
            }

            if (methodInfo.IsAbstract)
            {
                return false;
            }

            if (methodInfo.IsConstructor)
            {
                return false;
            }

            if (methodInfo.IsGenericMethod)
            {
                return false;
            }

            return methodInfo.IsPublic;
        }

        private static bool IsIDisposableMethod(MethodInfo methodInfo)
        {
            // Find where the method was originally declared
            var baseMethodInfo = methodInfo.GetBaseDefinition();
            var declaringTypeInfo = baseMethodInfo.DeclaringType.GetTypeInfo();

            return
                (typeof(IDisposable).GetTypeInfo().IsAssignableFrom(declaringTypeInfo) &&
                 declaringTypeInfo.GetRuntimeInterfaceMap(typeof(IDisposable)).TargetMethods[0] == baseMethodInfo);
        }
    }
}
