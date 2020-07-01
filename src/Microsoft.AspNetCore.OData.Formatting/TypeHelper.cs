// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Formatting
{
    internal static class TypeHelper
    {
        public static Type GetUnderlyingTypeOrSelf(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// Determine if a type is assignable from another type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <param name="fromType">The type to assign from.</param>
        /// <returns>True if the type is assignable; false otherwise.</returns>
        public static bool IsTypeAssignableFrom(Type clrType, Type fromType)
        {
            return clrType.IsAssignableFrom(fromType);
        }

        /// <summary>
        /// Return the reflected type from a member info.
        /// </summary>
        /// <param name="memberInfo">The member info to convert.</param>
        /// <returns>The reflected type from a member info.</returns>
        public static Type GetReflectedType(MemberInfo memberInfo)
        {
            return memberInfo.ReflectedType;
        }

        /// <summary>
        /// Determine if a type is a value type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a value type; false otherwise.</returns>
        public static bool IsValueType(Type clrType)
        {
            return clrType.IsValueType;
        }

        /// <summary>
        /// Determine if a type is an enumeration.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsEnum(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return underlyingTypeOrSelf.IsEnum;
        }

        /// <summary>
        /// Determine if a type is a generic type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a generic type; false otherwise.</returns>
        public static bool IsGenericType(this Type clrType)
        {
            return clrType.IsGenericType;
        }

        /// <summary>
        /// Determine if a type is a collection.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsCollection(Type clrType)
        {
            Type elementType;
            return TypeHelper.IsCollection(clrType, out elementType);
        }

        /// <summary>
        /// Determine if a type is a collection.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <param name="elementType">out: the element type of the collection.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsCollection(Type clrType, out Type elementType)
        {
            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            elementType = clrType;

            // see if this type should be ignored.
            if (clrType == typeof(string))
            {
                return false;
            }

            Type collectionInterface
                = clrType.GetInterfaces()
                    .Union(new[] { clrType })
                    .FirstOrDefault(
                        t => TypeHelper.IsGenericType(t)
                             && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();
                return true;
            }

            return false;
        }

        internal static Type GetTaskInnerTypeOrSelf(Type type)
        {
            if (IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return type.GetGenericArguments().First();
            }

            return type;
        }
    }
}
