//-----------------------------------------------------------------------------
// <copyright file="EdmClrTypeMapExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Edm;

/// <summary>
/// The extensions used to map between C# types and Edm types.
/// </summary>
internal static class EdmClrTypeMapExtensions
{
    /// <summary>
    /// Gets the corresponding Edm primitive type <see cref="IEdmPrimitiveTypeReference"/> for a given <see cref="Type"/> type.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="clrType">The given CLR type.</param>
    /// <returns>Null or the Edm primitive type.</returns>
    public static IEdmPrimitiveTypeReference GetEdmPrimitiveTypeReference(this IEdmModel edmModel, Type clrType)
    {
        if (edmModel == null || edmModel is EdmCoreModel)
        {
            return DefaultODataTypeMapper.Default.GetEdmPrimitiveType(clrType);
        }

        return edmModel.GetTypeMapper().GetEdmPrimitiveType(clrType);
    }

    /// <summary>
    /// Gets the corresponding CLR type for a given Edm primitive type.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="edmPrimitiveType">The given Edm primitive type.</param>
    /// <returns>Null or the CLR type.</returns>
    public static Type GetClrPrimitiveType(this IEdmModel edmModel, IEdmPrimitiveTypeReference edmPrimitiveType)
    {
        if (edmPrimitiveType == null)
        {
            return null;
        }

        if (edmModel == null || edmModel is EdmCoreModel)
        {
            return DefaultODataTypeMapper.Default.GetClrPrimitiveType(edmPrimitiveType.PrimitiveDefinition(), edmPrimitiveType.IsNullable);
        }

        return edmModel.GetTypeMapper().GetPrimitiveType(edmPrimitiveType);
    }

    /// <summary>
    /// Figures out if the given clr type is nonstandard edm primitive like uint, ushort, char[] etc.
    /// and returns the corresponding clr type to which we map like uint => long.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="clrType">The potential non-standard CLR type.</param>
    /// <param name="isNonstandardEdmPrimitive">A boolean value out to indicate whether the input CLR type is standard OData primitive type.</param>
    /// <returns>The standard CLR type or the input CLR type itself.</returns>
    public static Type IsNonstandardEdmPrimitive(this IEdmModel edmModel, Type clrType, out bool isNonstandardEdmPrimitive)
    {
        IEdmPrimitiveTypeReference edmType = edmModel.GetEdmPrimitiveTypeReference(clrType);
        if (edmType == null)
        {
            isNonstandardEdmPrimitive = false;
            return clrType;
        }

        Type reverseLookupClrType = edmModel.GetClrPrimitiveType(edmType);
        isNonstandardEdmPrimitive = (clrType != reverseLookupClrType);

        return reverseLookupClrType;
    }

    /// <summary>
    /// Gets the Edm type reference from the CLR type.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="clrType">The given CLR type.</param>
    /// <returns>null or the Edm type reference.</returns>
    public static IEdmTypeReference GetEdmTypeReference(this IEdmModel edmModel, Type clrType)
    {
        return edmModel.GetTypeMapper().GetEdmTypeReference(edmModel, clrType);
    }

    /// <summary>
    /// Gets the Edm type from the CLR type.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="clrType">The given CLR type.</param>
    /// <returns>null or the Edm type.</returns>
    public static IEdmType GetEdmType(this IEdmModel edmModel, Type clrType)
    {
        return edmModel.GetEdmTypeReference(clrType)?.Definition;
    }

    /// <summary>
    /// Gets the corresponding CLR type for a given Edm type reference.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="edmTypeReference">The Edm type reference.</param>
    /// <returns>Null or the CLR type.</returns>
    public static Type GetClrType(this IEdmModel edmModel, IEdmTypeReference edmTypeReference)
    {
        return edmModel.GetTypeMapper().GetClrType(edmModel, edmTypeReference, AssemblyResolverHelper.Default);
    }

    /// <summary>
    /// Gets the corresponding CLR type for a given Edm type reference.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="edmTypeReference">The Edm type reference.</param>
    /// <param name="assembliesResolver">The assembly resolver.</param>
    /// <returns>Null or the CLR type.</returns>
    public static Type GetClrType(this IEdmModel edmModel, IEdmTypeReference edmTypeReference, IAssemblyResolver assembliesResolver)
    {
        return edmModel.GetTypeMapper().GetClrType(edmModel, edmTypeReference, assembliesResolver);
    }

    /// <summary>
    /// Gets the corresponding CLR type for a given Edm type.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="edmType">The Edm type.</param>
    /// <returns>Null or the CLR type.</returns>
    public static Type GetClrType(this IEdmModel edmModel, IEdmType edmType)
    {
        return edmModel.GetClrType(edmType, AssemblyResolverHelper.Default);
    }

    /// <summary>
    /// Gets the corresponding CLR type for a given Edm type.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="edmType">The Edm type.</param>
    /// <param name="assembliesResolver">The assembly resolver.</param>
    /// <returns>Null or the CLR type.</returns>
    public static Type GetClrType(this IEdmModel edmModel, IEdmType edmType, IAssemblyResolver assembliesResolver)
    {
        return edmModel.GetTypeMapper().GetClrType(edmModel, edmType, true, assembliesResolver);
    }

    internal static string EdmFullName(this Type clrType)
    {
        return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", clrType.Namespace, clrType.EdmName());
    }

    // Mangle the invalid EDM literal Type.FullName (System.Collections.Generic.IEnumerable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
    // to a valid EDM literal (the C# type name IEnumerable<int>).
    internal static string EdmName(this Type clrType)
    {
        // We cannot use just Type.Name here as it doesn't work for generic types.
        return MangleClrTypeName(clrType);
    }

    // TODO (work item 336): Support nested types and anonymous types.
    private static string MangleClrTypeName(Type type)
    {
        Contract.Assert(type != null);

        if (!type.IsGenericType)
        {
            return type.Name;
        }
        else
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}Of{1}",
                type.Name.Replace('`', '_'),
                String.Join("_", type.GetGenericArguments().Select(t => MangleClrTypeName(t))));
        }
    }
}
