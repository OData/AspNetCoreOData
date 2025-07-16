//-----------------------------------------------------------------------------
// <copyright file="TypeHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Microsoft.AspNetCore.OData.Common;

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

    /// <summary>
    /// Determines whether the specified type is a <see cref="ComputeWrapper{T}"/> or a custom implementation
    /// that inherits from <see cref="DynamicTypeWrapper"/> and implements both <see cref="IComputeWrapper{T}"/>
    /// and <see cref="IGroupByWrapper{TContainer, TWrapper}"/>.
    /// </summary>
    /// <param name="typeToCheck">The type to check.</param>
    /// <param name="entityType">The entity type if the specified type is a <see cref="ComputeWrapper{T}"/> or a custom implementation; otherwise, null.</param>
    /// <returns>
    /// <c>true</c> if the specified type is a <see cref="ComputeWrapper{T}"/> or a custom implementation
    /// that meets the criteria; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsComputeWrapper(this Type typeToCheck, out Type entityType)
    {
        entityType = null;
        if (typeToCheck == null)
        {
            return false;
        }

        bool isComputeWrapper = false;

        if (typeToCheck.IsGenericType)
        {
            Type genericTypeDefinition = typeToCheck.GetGenericTypeDefinition();

            // Default implementation
            if (genericTypeDefinition == typeof(ComputeWrapper<>))
            {
                Debug.Assert(
                    typeof(DynamicTypeWrapper).IsAssignableFrom(genericTypeDefinition)
                    && genericTypeDefinition.ImplementsInterface(typeof(IComputeWrapper<>))
                    && genericTypeDefinition.ImplementsInterface(typeof(IGroupByWrapper<,>)),
                    "ComputeWrapper<T> must inherit from DynamicTypeWrapper and implement IComputeWrapper<T> and IGroupByWrapper<TContainer, TWrapper>");

                isComputeWrapper = true;
            }
            // Custom implementation
            // Must inherit from DynamicTypeWrapper
            // Must implement IComputeWrapper<T> and IGroupByWrapper<TContainer, TWrapper>
            else if (typeof(DynamicTypeWrapper).IsAssignableFrom(genericTypeDefinition)
                && genericTypeDefinition.ImplementsInterface(typeof(IComputeWrapper<>))
                && genericTypeDefinition.ImplementsInterface(typeof(IGroupByWrapper<,>)))
            {
                isComputeWrapper = true;
            }

            if (isComputeWrapper)
            {
                entityType = typeToCheck.GetGenericArguments()[0];
            }
        }

        return isComputeWrapper;
    }

    /// <summary>
    /// Determines whether the specified type is a <see cref="FlatteningWrapper{T}"/> or a custom implementation
    /// that inherits from <see cref="DynamicTypeWrapper"/> and implements both <see cref="IFlatteningWrapper{T}"/>
    /// and <see cref="IGroupByWrapper{TContainer, TWrapper}"/>.
    /// </summary>
    /// <param name="typeToCheck">The type to check.</param>
    /// <returns>
    /// <c>true</c> if the specified type is a <see cref="FlatteningWrapper{T}"/> or a custom implementation
    /// that meets the criteria; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsFlatteningWrapper(this Type typeToCheck)
    {
        if (typeToCheck == null)
        {
            return false;
        }

        if (typeToCheck.IsGenericType)
        {
            Type genericTypeDefinition = typeToCheck.GetGenericTypeDefinition();

            Func<bool> isFlatteningWrapperFunc = () => typeof(DynamicTypeWrapper).IsAssignableFrom(genericTypeDefinition)
                && genericTypeDefinition.ImplementsInterface(typeof(IFlatteningWrapper<>))
                && genericTypeDefinition.ImplementsInterface(typeof(IGroupByWrapper<,>));
            // Default implementation
            if (genericTypeDefinition == typeof(FlatteningWrapper<>))
            {
                Debug.Assert(
                    typeof(DynamicTypeWrapper).IsAssignableFrom(genericTypeDefinition)
                    && genericTypeDefinition.ImplementsInterface(typeof(IFlatteningWrapper<>))
                    && genericTypeDefinition.ImplementsInterface(typeof(IGroupByWrapper<,>)),
                    "FlatteningWrapper<T> must inherit from DynamicTypeWrapper and implement IFlatteningWrapper<T> and IGroupByWrapper<TContainer, TWrapper>");

                return true;
            }

            // Custom implementation
            // Must inherit from DynamicTypeWrapper
            // Must implement IFlatteningWrapper<T> and IGroupByWrapper<TContainer, TWrapper>
            return typeof(DynamicTypeWrapper).IsAssignableFrom(genericTypeDefinition)
                && genericTypeDefinition.ImplementsInterface(typeof(IFlatteningWrapper<>))
                && genericTypeDefinition.ImplementsInterface(typeof(IGroupByWrapper<,>));
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified type is a <see cref="GroupByWrapper"/> or a custom implementation
    /// that inherits from <see cref="DynamicTypeWrapper"/> and implements <see cref="IGroupByWrapper{TContainer, TWrapper}"/>.
    /// </summary>
    /// <param name="typeToCheck">The type to check.</param>
    /// <returns>
    /// <c>true</c> if the specified type is a <see cref="GroupByWrapper"/> or a custom implementation
    /// that meets the criteria; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsGroupByWrapper(this Type typeToCheck)
    {
        if (typeToCheck == null || typeToCheck.IsValueType || typeToCheck == typeof(string))
        {
            return false;
        }

        // Default implementation
        if (typeof(GroupByWrapper).IsAssignableFrom(typeToCheck))
        {
            Debug.Assert(
                typeof(DynamicTypeWrapper).IsAssignableFrom(typeToCheck)
                && typeToCheck.ImplementsInterface(typeof(IGroupByWrapper<,>)),
                "GroupByWrapper must inherit from DynamicTypeWrapper and implement IGroupByWrapper<TContainer, TWrapper>");

            return true;
        }

        // Custom implementation
        // Must inherit from DynamicTypeWrapper
        // Must implement IGroupByWrapper<TContainer, TWrapper>
        return typeof(DynamicTypeWrapper).IsAssignableFrom(typeToCheck) &&
            typeToCheck.ImplementsInterface(typeof(IGroupByWrapper<,>));
    }

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
    /// Determines whether the specified <see cref="Type"/> represents an <see cref="IAsyncEnumerable{T}"/> type.
    /// </summary>
    /// <param name="clrType">The <see cref="Type"/> to evaluate.</param>
    /// <returns>True if the type is an enumeration; false otherwise.</returns>
    public static bool IsAsyncEnumerableType(Type clrType)
    {
        if (clrType == null)
        {
            throw Error.ArgumentNull(nameof(clrType));
        }

        return 
            (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)) ||
            (clrType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)));
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

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
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

    /// <summary>
    /// Determines whether the specified type inherits from a given generic base type.
    /// </summary>
    /// <param name="typeToCheck">The type to examine.</param>
    /// <param name="genericBaseType">The open generic type definition to check against (e.g., typeof(Base&lt;&gt;)).</param>
    /// <returns><c>true</c> if <paramref name="typeToCheck"/> inherits from <paramref name="genericBaseType"/>; otherwise, <c>false</c>.</returns>
    public static bool InheritsFromGenericBase(this Type typeToCheck, Type genericBaseType)
    {
        if (typeToCheck == null || genericBaseType == null || !genericBaseType.IsGenericTypeDefinition)
            return false;

        Type baseType = typeToCheck.BaseType;

        while (baseType != null && baseType != typeof(object))
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericBaseType)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified target type implements the given interface type.
    /// </summary>
    /// <param name="targetType">The type to check for implementation of the interface.</param>
    /// <param name="interfaceType">The interface type to check against.</param>
    /// <returns>
    /// <c>true</c> if the target type implements the specified interface type; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method supports both generic and non-generic interfaces. For generic interfaces, it checks if the target type
    /// implements any interface that matches the generic type definition of the specified interface type.
    /// </remarks>
    public static bool ImplementsInterface(this Type targetType, Type interfaceType)
    {
        if (targetType == null || interfaceType == null)
        {
            return false;
        }

        if (interfaceType.IsGenericTypeDefinition) // Generic interface (e.g., I<>)
        {
            Type[] implementedInterfaces = targetType.GetInterfaces();
            for (int i = 0; i < implementedInterfaces.Length; i++)
            {
                Type implementedInterface = implementedInterfaces[i];

                if (implementedInterface.IsGenericType &&
                    implementedInterface.GetGenericTypeDefinition() == interfaceType)
                {
                    return true;
                }
            }
        }
        else // Non-generic interface
        {
            return interfaceType.IsAssignableFrom(targetType);
        }

        return false;
    }
}
