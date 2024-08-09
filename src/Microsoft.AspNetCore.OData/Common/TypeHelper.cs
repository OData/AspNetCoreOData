//-----------------------------------------------------------------------------
// <copyright file="TypeHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Common
{
    /// <summary>
    /// The type related helper methods.
    /// </summary>
    internal static class TypeHelper
    {
        /// <summary>
        /// Test whether the input type is <see cref="DynamicTypeWrapper"/>
        /// </summary>
        /// <param name="type">The test type.</param>
        /// <returns>true/false</returns>
        public static bool IsDynamicTypeWrapper(this Type type)
        {
            return (type != null && typeof(DynamicTypeWrapper).IsAssignableFrom(type));
        }

        public static bool IsDeltaSetWrapper(this Type type, out Type entityType) => IsTypeWrapper(typeof(DeltaSet<>), type, out entityType);

        public static bool IsSelectExpandWrapper(this Type type, out Type entityType) => IsTypeWrapper(typeof(SelectExpandWrapper<>), type, out entityType);

        public static bool IsComputeWrapper(this Type type, out Type entityType) => IsTypeWrapper(typeof(ComputeWrapper<>), type, out entityType);

        private static bool IsTypeWrapper(Type wrappedType, Type type, out Type entityType)
        {
            if (type == null)
            {
                entityType = null;
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == wrappedType)
            {
                entityType = type.GetGenericArguments()[0];
                return true;
            }

            return IsTypeWrapper(wrappedType, type.BaseType, out entityType);
        }

        /// <summary>
        /// Return the collection element type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The collection element type from a type.</returns>
        public static Type GetInnerElementType(Type clrType)
        {
            IsCollection(clrType, out Type elementType);
            Contract.Assert(elementType != null);

            return elementType;
        }

        /// <summary>
        /// Return the underlying type or itself.
        /// </summary>
        /// <param name="type">The input type.</param>
        /// <returns>The underlying type.</returns>
        public static Type GetUnderlyingTypeOrSelf(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// Determine if a type is a DateTime.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a DateTime; false otherwise.</returns>
        public static bool IsDateTime(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return Type.GetTypeCode(underlyingTypeOrSelf) == TypeCode.DateTime;
        }

#if NET6_0
        /// <summary>
        /// Determine if a type is a <see cref="DateOnly"/>.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a DateOnly; false otherwise.</returns>
        public static bool IsDateOnly(this Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return underlyingTypeOrSelf == typeof(DateOnly);
        }

        /// <summary>
        /// Determine if a type is a <see cref="TimeOnly"/>.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a TimeOnly; false otherwise.</returns>
        public static bool IsTimeOnly(this Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return underlyingTypeOrSelf == typeof(TimeOnly);
        }
#endif

        /// <summary>
        /// Determine if a type is a TimeSpan.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a TimeSpan; false otherwise.</returns>
        public static bool IsTimeSpan(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return underlyingTypeOrSelf == typeof(TimeSpan);
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
        /// Determine if a type is null-able.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is null-able; false otherwise.</returns>
        public static bool IsNullable(this Type clrType)
        {
            if (clrType == null)
            {
                return false;
            }

            if (clrType.IsValueType)
            {
                // value types are only nullable if they are Nullable<T>
                return clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            else
            {
                // reference types are always nullable
                return true;
            }
        }

        /// <summary>
        /// Return the type from a nullable type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The type from a nullable type.</returns>
        public static Type ToNullable(Type clrType)
        {
            if (IsNullable(clrType))
            {
                return clrType;
            }
            else
            {
                return typeof(Nullable<>).MakeGenericType(clrType);
            }
        }

        /// <summary>
        /// Determine if a type is a collection.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsCollection(Type clrType) => IsCollection(clrType, out _);

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
                throw Error.ArgumentNull(nameof(clrType));
            }

            elementType = clrType;

            // see if this type should be ignored.
            if (clrType == typeof(string))
            {
                return false;
            }

            // Since IDictionary<T,T> is a collection of KeyValuePair<T,T>
            if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                return false;
            }

            Type collectionInterface
                = clrType.GetInterfaces()
                    .Union(new[] { clrType })
                    .FirstOrDefault(
                        t => t.IsGenericType
                             && (t.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                             || t.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check whether the given type is a primitive type or known type.
        /// </summary>
        /// <param name="type">The type to validate.</param>
        /// <returns>True if type is primitive or known type, otherwise False.</returns>
        public static bool IsPrimitiveOrKnownType(Type type)
        {
            return type.IsPrimitive 
                   || type == typeof(string)
                   || type == typeof(Uri)
                   || type == typeof(DateTime)
#if NET6_0_OR_GREATER
        || type == typeof(DateOnly)
        || type == typeof(TimeOnly)
#endif
                   || type == typeof(DateTimeOffset)
                   || type == typeof(Guid)
                   || type == typeof(Decimal);
        }

        internal static bool IsDictionary(Type clrType)
        {
            if (clrType == null)
            {
                return false;
            }

            if (typeof(IDictionary).IsAssignableFrom(clrType))
            {
                return true;
            }

            if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                return true;
            }

            foreach (var interfaceType in clrType.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    return true;
                }
            }

            return false;
        }

        internal static IEdmTypeReference GetUntypedEdmType(Type clrType)
        {
            if (clrType == null)
            {
                throw Error.ArgumentNull(nameof(clrType));
            }

            return IsDictionary(clrType) ?
                    (IEdmTypeReference)EdmUntypedStructuredTypeReference.NullableTypeReference :
                    (TypeHelper.IsCollection(clrType) ?
                        (IEdmTypeReference)EdmUntypedHelpers.NullableUntypedCollectionReference :
                        (IEdmTypeReference)EdmUntypedStructuredTypeReference.NullableTypeReference);
        }

        /// <summary>
        /// Returns type of T if the type implements IEnumerable of T, otherwise, return null.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Type GetImplementedIEnumerableType(Type type)
        {
            // get inner type from Task<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                type = type.GetGenericArguments().First();
            }

            if (type.IsGenericType && type.IsInterface &&
                (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 type.GetGenericTypeDefinition() == typeof(IQueryable<>)))
            {
                // special case the IEnumerable<T>
                return GetInnerGenericType(type);
            }
            else
            {
                // for the rest of interfaces and strongly Type collections
                Type[] interfaces = type.GetInterfaces();
                foreach (Type interfaceType in interfaces)
                {
                    if (interfaceType.IsGenericType &&
                        (interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                         interfaceType.GetGenericTypeDefinition() == typeof(IQueryable<>)))
                    {
                        // special case the IEnumerable<T>
                        return GetInnerGenericType(interfaceType);
                    }
                }
            }

            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching all exceptions in this case is the right to do.")]
        // This code is copied from DefaultHttpControllerTypeResolver.GetControllerTypes.
        internal static IEnumerable<Type> GetLoadedTypes(IAssemblyResolver assembliesResolver)
        {
            List<Type> result = new List<Type>();

            if (assembliesResolver == null)
            {
                return result;
            }

            // Go through all assemblies referenced by the application and search for types matching a predicate
            IEnumerable<Assembly> assemblies = assembliesResolver.Assemblies;
            foreach (Assembly assembly in assemblies)
            {
                Type[] exportedTypes = null;
                if (assembly == null || assembly.IsDynamic)
                {
                    // can't call GetTypes on a null (or dynamic?) assembly
                    continue;
                }

                try
                {
                    exportedTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    continue;
                }

                if (exportedTypes != null)
                {
                    result.AddRange(exportedTypes.Where(t => t != null && t.IsVisible));
                }
            }

            return result;
        }

        internal static Type GetTaskInnerTypeOrSelf(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return type.GetGenericArguments().First();
            }

            return type;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching all exceptions in this case is the right to do.")]
        internal static bool TryGetInstance(Type type, object value, out object instance)
        {
            instance = null;

            // Trial to create an instance, using parsing
            var methodInfo = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, null, new[] { value.GetType(), type.MakeByRefType() }, null);
            if (methodInfo != null)
            {
                object[] parameters = new object[] { value, null };
                var result = (bool)methodInfo.Invoke(null, parameters);
                if (result)
                {
                    instance = parameters[1];
                    return true;
                }
            }

            try
            {
                // Trial to create an instance, using constructor
                instance = Activator.CreateInstance(type, args: value);
                if (instance != null)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // Proceed further
            }
            return false;
        }

        private static Type GetInnerGenericType(Type interfaceType)
        {
            // Getting the type T definition if the returning type implements IEnumerable<T>
            Type[] parameterTypes = interfaceType.GetGenericArguments();

            if (parameterTypes.Length == 1)
            {
                return parameterTypes[0];
            }

            return null;
        }
    }
}
