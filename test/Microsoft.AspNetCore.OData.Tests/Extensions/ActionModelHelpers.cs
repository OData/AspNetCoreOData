// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    internal static class ActionModelHelpers
    {
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
