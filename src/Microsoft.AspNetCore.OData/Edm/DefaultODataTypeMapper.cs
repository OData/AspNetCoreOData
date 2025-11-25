//-----------------------------------------------------------------------------
// <copyright file="DefaultODataTypeMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.Spatial;

namespace Microsoft.AspNetCore.OData.Edm;

/// <summary>
/// The default implementation for <see cref="IODataTypeMapper"/>.
/// </summary>
public class DefaultODataTypeMapper : IODataTypeMapper
{
    /// <summary>
    /// Creates a static instance for the Default type mapper.
    /// </summary>
    internal static DefaultODataTypeMapper Default = new DefaultODataTypeMapper();

    #region Default_PrimitiveTypeMapping
    /// <summary>
    /// The default mapping between Edm primitive type and Clr primitive type.
    /// Primitive types are cross Edm models.
    /// </summary>
    private static IDictionary<Type, IEdmPrimitiveTypeReference> ClrPrimitiveTypes
        = new Dictionary<Type, IEdmPrimitiveTypeReference>();

    /// <summary>
    /// Item1 --> non-nullable
    /// Item2 --> nullable
    /// </summary>
    private static IDictionary<IEdmPrimitiveType, (Type, Type)> EdmPrimitiveTypes
        = new Dictionary<IEdmPrimitiveType, (Type, Type)>();

    static DefaultODataTypeMapper()
    {
        BuildReferenceTypeMapping<string>(EdmPrimitiveTypeKind.String);
        BuildValueTypeMapping<bool>(EdmPrimitiveTypeKind.Boolean);
        BuildValueTypeMapping<byte>(EdmPrimitiveTypeKind.Byte);
        BuildValueTypeMapping<decimal>(EdmPrimitiveTypeKind.Decimal);
        BuildValueTypeMapping<double>(EdmPrimitiveTypeKind.Double);
        BuildValueTypeMapping<Guid>(EdmPrimitiveTypeKind.Guid);
        BuildValueTypeMapping<short>(EdmPrimitiveTypeKind.Int16);
        BuildValueTypeMapping<int>(EdmPrimitiveTypeKind.Int32);
        BuildValueTypeMapping<long>(EdmPrimitiveTypeKind.Int64);
        BuildValueTypeMapping<sbyte>(EdmPrimitiveTypeKind.SByte);
        BuildValueTypeMapping<float>(EdmPrimitiveTypeKind.Single);
        BuildReferenceTypeMapping<byte[]>(EdmPrimitiveTypeKind.Binary);
        BuildReferenceTypeMapping<Stream>(EdmPrimitiveTypeKind.Stream);
        BuildValueTypeMapping<DateTimeOffset>(EdmPrimitiveTypeKind.DateTimeOffset);
        BuildValueTypeMapping<TimeSpan>(EdmPrimitiveTypeKind.Duration);
        BuildValueTypeMapping<DateOnly>(EdmPrimitiveTypeKind.Date);
        BuildValueTypeMapping<TimeOnly>(EdmPrimitiveTypeKind.TimeOfDay);

        BuildReferenceTypeMapping<Geography>(EdmPrimitiveTypeKind.Geography);
        BuildReferenceTypeMapping<GeographyPoint>(EdmPrimitiveTypeKind.GeographyPoint);
        BuildReferenceTypeMapping<GeographyLineString>(EdmPrimitiveTypeKind.GeographyLineString);
        BuildReferenceTypeMapping<GeographyPolygon>(EdmPrimitiveTypeKind.GeographyPolygon);
        BuildReferenceTypeMapping<GeographyCollection>(EdmPrimitiveTypeKind.GeographyCollection);
        BuildReferenceTypeMapping<GeographyMultiLineString>(EdmPrimitiveTypeKind.GeographyMultiLineString);
        BuildReferenceTypeMapping<GeographyMultiPoint>(EdmPrimitiveTypeKind.GeographyMultiPoint);
        BuildReferenceTypeMapping<GeographyMultiPolygon>(EdmPrimitiveTypeKind.GeographyMultiPolygon);
        BuildReferenceTypeMapping<Geometry>(EdmPrimitiveTypeKind.Geometry);
        BuildReferenceTypeMapping<GeometryPoint>(EdmPrimitiveTypeKind.GeometryPoint);
        BuildReferenceTypeMapping<GeometryLineString>(EdmPrimitiveTypeKind.GeometryLineString);
        BuildReferenceTypeMapping<GeometryPolygon>(EdmPrimitiveTypeKind.GeometryPolygon);
        BuildReferenceTypeMapping<GeometryCollection>(EdmPrimitiveTypeKind.GeometryCollection);
        BuildReferenceTypeMapping<GeometryMultiLineString>(EdmPrimitiveTypeKind.GeometryMultiLineString);
        BuildReferenceTypeMapping<GeometryMultiPoint>(EdmPrimitiveTypeKind.GeometryMultiPoint);
        BuildReferenceTypeMapping<GeometryMultiPolygon>(EdmPrimitiveTypeKind.GeometryMultiPolygon);

        // non-standard mappings
        BuildReferenceTypeMapping<XElement>(EdmPrimitiveTypeKind.String, isStandard: false);
        BuildValueTypeMapping<ushort>(EdmPrimitiveTypeKind.Int32, isStandard: false);
        BuildValueTypeMapping<uint>(EdmPrimitiveTypeKind.Int64, isStandard: false);
        BuildValueTypeMapping<ulong>(EdmPrimitiveTypeKind.Int64, isStandard: false);
        BuildReferenceTypeMapping<char[]>(EdmPrimitiveTypeKind.String, isStandard: false);
        BuildValueTypeMapping<char>(EdmPrimitiveTypeKind.String, isStandard: false);
        BuildValueTypeMapping<DateTime>(EdmPrimitiveTypeKind.DateTimeOffset, isStandard: false);
    }
    #endregion

    #region IODataTypeMapper.GetPrimitiveType
    /// <summary>
    /// Gets the corresponding Edm primitive type <see cref="IEdmPrimitiveTypeReference"/> for a given <see cref="Type"/> type.
    /// </summary>
    /// <param name="clrType">The given CLR type.</param>
    /// <returns>Null or the Edm primitive type.</returns>
    public virtual IEdmPrimitiveTypeReference GetEdmPrimitiveType(Type clrType)
    {
        if (clrType == null)
        {
            return null;
        }

        return ClrPrimitiveTypes.TryGetValue(clrType, out IEdmPrimitiveTypeReference primitive) ? primitive : null;
    }

    /// <summary>
    /// Gets the corresponding <see cref="Type"/> type for a given Edm primitive type <see cref="IEdmPrimitiveTypeReference"/>.
    /// </summary>
    /// <param name="primitiveType">The given Edm primitive type.</param>
    /// <param name="nullable">The nullable or not.</param>
    /// <returns>Null or the CLR type.</returns>
    public virtual Type GetClrPrimitiveType(IEdmPrimitiveType primitiveType, bool nullable)
    {
        if (primitiveType == null)
        {
            return null;
        }

        if (EdmPrimitiveTypes.TryGetValue(primitiveType, out (Type, Type) types))
        {
            if (nullable)
            {
                return types.Item2;
            }
            else
            {
                return types.Item1;
            }
        }

        return null;
    }
    #endregion

    /// <summary>
    /// The cache used to hold the type mapping between <see cref="Type"/> and <see cref="IEdmTypeReference"/>.
    /// </summary>
    private ConcurrentDictionary<IEdmModel, TypeCacheItem> _cache = new ConcurrentDictionary<IEdmModel, TypeCacheItem>();

    #region ClrType -> EdmType
    /// <summary>
    /// Gets the corresponding Edm type <see cref="IEdmTypeReference"/> for the given CLR type <see cref="Type"/>.
    /// </summary>
    /// <param name="edmModel">The given Edm model.</param>
    /// <param name="clrType">The given CLR type.</param>
    /// <returns>Null or the corresponding Edm type reference.</returns>
    public virtual IEdmTypeReference GetEdmTypeReference(IEdmModel edmModel, Type clrType)
    {
        if (clrType == null)
        {
            throw Error.ArgumentNull(nameof(clrType));
        }

        IEdmPrimitiveTypeReference primitiveType = GetEdmPrimitiveType(clrType);
        if (primitiveType != null)
        {
            return primitiveType;
        }

        if (edmModel == null)
        {
            throw Error.ArgumentNull(nameof(edmModel));
        }

        TypeCacheItem map = _cache.GetOrAdd(edmModel, d => new TypeCacheItem());

        // Search from cache
        if (map.TryFindEdmType(clrType, out IEdmTypeReference edmTypeRef))
        {
            return edmTypeRef;
        }

        // Not in the cache, let's build the Edm type reference.
        IEdmType edmType = GetEdmType(edmModel, clrType, testCollections: true);
        if (edmType != null)
        {
            bool isNullable = clrType.IsNullable();
            edmTypeRef = edmType.ToEdmTypeReference(isNullable);
        }
        else
        {
            edmTypeRef = null;
        }

        map.AddClrToEdmMap(clrType, edmTypeRef);
        return edmTypeRef;
    }

    private IEdmType GetEdmType(IEdmModel edmModel, Type clrType, bool testCollections)
    {
        Contract.Assert(edmModel != null);
        Contract.Assert(clrType != null);

        IEdmPrimitiveTypeReference primitiveType = GetEdmPrimitiveType(clrType);
        if (primitiveType != null)
        {
            return primitiveType.Definition;
        }
        else
        {
            if (testCollections)
            {
                Type entityType;
                if (clrType.IsDeltaSetWrapper(out entityType))
                {
                    IEdmType elementType = GetEdmType(edmModel, entityType, testCollections: false);
                    if (elementType != null)
                    {
                        return new EdmCollectionType(elementType.ToEdmTypeReference(entityType.IsNullable()));
                    }
                }

                Type enumerableOfT = ExtractGenericInterface(clrType, typeof(IEnumerable<>));
                Type asyncEnumerableOfT = ExtractGenericInterface(clrType, typeof(IAsyncEnumerable<>));

                if (enumerableOfT != null || asyncEnumerableOfT != null)
                {
                    Type elementClrType = null;
                        
                    if (enumerableOfT != null)
                    {
                        elementClrType = enumerableOfT.GetGenericArguments()[0];
                    }
                    else
                    {
                        elementClrType = asyncEnumerableOfT.GetGenericArguments()[0];
                    }

                    // IEnumerable<SelectExpandWrapper<T>> is a collection of T.
                    if (elementClrType.IsSelectExpandWrapper(out entityType))
                    {
                        elementClrType = entityType;
                    }

                    if (elementClrType.IsComputeWrapper(out entityType))
                    {
                        elementClrType = entityType;
                    }

                    IEdmType elementType = GetEdmType(edmModel, elementClrType, testCollections: false);
                    if (elementType != null)
                    {
                        return new EdmCollectionType(elementType.ToEdmTypeReference(elementClrType.IsNullable()));
                    }
                }
            }

            Type underlyingType = TypeHelper.GetUnderlyingTypeOrSelf(clrType);
            if (TypeHelper.IsEnum(underlyingType))
            {
                clrType = underlyingType;
            }

            // search for the ClrTypeAnnotation and return it if present
            IEdmType returnType =
                edmModel
                .SchemaElements
                .OfType<IEdmType>()
                .Select(edmType => new { EdmType = edmType, Annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmType) })
                .Where(tuple => tuple.Annotation != null && tuple.Annotation.ClrType == clrType)
                .Select(tuple => tuple.EdmType)
                .SingleOrDefault();

            // default to the EdmType with the same name as the ClrType name
            returnType = returnType ?? edmModel.FindType(clrType.EdmFullName());

            if (clrType.BaseType != null)
            {
                // go up the inheritance tree to see if we have a mapping defined for the base type.
                returnType = returnType ?? GetEdmType(edmModel, clrType.BaseType, testCollections);
            }

            return returnType;
        }
    }
    #endregion

    #region EdmType -> ClrType
    /// <summary>
    /// Gets the corresponding <see cref="Type"/> for a given Edm type <see cref="IEdmType"/>.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="edmType">The Edm type.</param>
    /// <param name="nullable">The nullable or not.</param>
    /// <param name="assembliesResolver">The assembly resolver. if it's null, will use the default resolver.</param>
    /// <returns>Null or the CLR type.</returns>
    public virtual Type GetClrType(IEdmModel edmModel, IEdmType edmType, bool nullable, IAssemblyResolver assembliesResolver)
    {
        if (edmType == null)
        {
            throw Error.ArgumentNull(nameof(edmType));
        }

        if (edmType.TypeKind == EdmTypeKind.Primitive)
        {
            return GetClrPrimitiveType((IEdmPrimitiveType)edmType, nullable);
        }

        if (edmModel == null)
        {
            throw Error.ArgumentNull(nameof(edmModel));
        }

        assembliesResolver = assembliesResolver ?? AssemblyResolverHelper.Default;

        // Let's search from cache
        TypeCacheItem map = _cache.GetOrAdd(edmModel, d => new TypeCacheItem());
        if (map.TryFindClrType(edmType, nullable, out Type clrType))
        {
            return clrType;
        }

        // If not cached, find the CLR type from the model.
        clrType = FindClrType(edmModel, edmType, assembliesResolver);

        if (clrType != null && nullable && clrType.IsEnum)
        {
            clrType = TypeHelper.ToNullable(clrType);
        }

        map.AddEdmToClrMap(edmType, nullable, clrType);

        return clrType;
    }

    /// <summary>
    /// Finds the corresponding CLR type for a given Edm type reference.
    /// </summary>
    /// <param name="edmModel">The Edm model.</param>
    /// <param name="edmType">The Edm type.</param>
    /// <param name="assembliesResolver">The assembly resolver.</param>
    /// <returns>Null or the CLR type.</returns>
    internal static Type FindClrType(IEdmModel edmModel, IEdmType edmType, IAssemblyResolver assembliesResolver)
    {
        if (edmModel == null)
        {
            throw Error.ArgumentNull(nameof(edmModel));
        }

        if (edmType == null)
        {
            throw Error.ArgumentNull(nameof(edmType));
        }

        if (assembliesResolver == null)
        {
            throw Error.ArgumentNull(nameof(assembliesResolver));
        }

        IEdmSchemaType edmSchemaType = edmType as IEdmSchemaType;
        if (edmSchemaType == null)
        {
            return null;
        }

        // by default, retrieve it from Clr type annotation.
        ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmSchemaType);
        if (annotation != null)
        {
            return annotation.ClrType;
        }

        string typeName = edmSchemaType.FullName();
        IEnumerable<Type> matchingTypes = GetMatchingTypes(typeName, assembliesResolver);

        if (matchingTypes.Count() > 1)
        {
            throw Error.InvalidOperation(SRResources.MultipleMatchingClrTypesForEdmType,
                typeName, string.Join(",", matchingTypes.Select(type => type.AssemblyQualifiedName)));
        }

        Type clrType = matchingTypes.SingleOrDefault();

        // TODO: shall we save it back to model because we will cache it?
        // I think we should not save it back to model, since we will cache it
        // edmModel.SetAnnotationValue(edmSchemaType, new ClrTypeAnnotation(clrType));
        return clrType;
    }
    #endregion

    private static Type ExtractGenericInterface(Type queryType, Type interfaceType)
    {
        Func<Type, bool> matchesInterface = t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType;
        return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
    }

    private static IEnumerable<Type> GetMatchingTypes(string edmFullName, IAssemblyResolver assembliesResolver)
        => TypeHelper.GetLoadedTypes(assembliesResolver).Where(t => t.IsPublic && t.EdmFullName() == edmFullName);

    private static void BuildTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind, bool isStandard)
    {
        Type type = typeof(T);
        bool isNullable = type.IsNullable();
        IEdmPrimitiveTypeReference edmPrimitiveTypeReference = EdmCoreModel.Instance.GetPrimitive(primitiveKind, isNullable);
        ClrPrimitiveTypes[type] = edmPrimitiveTypeReference;

        if (isStandard)
        {
            IEdmPrimitiveType primitiveType = edmPrimitiveTypeReference.PrimitiveDefinition();
            if (isNullable)
            {
                // for nullable, for example System.String, we don't have non-nullable string.
                // so, let's save it for both.
                // And since we make the order un-changable, it means 'nullable' coming first.
                EdmPrimitiveTypes[primitiveType] = (type, type);
            }
            else
            {
                if (EdmPrimitiveTypes.ContainsKey(primitiveType))
                {
                    (Type _, Type edmType2) = EdmPrimitiveTypes[primitiveType];
                    EdmPrimitiveTypes[primitiveType] = (type, edmType2);
                }
                else
                {
                    EdmPrimitiveTypes[primitiveType] = (type, null);
                }
            }
        }
    }

    private static void BuildValueTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind, bool isStandard = true)
        where T : struct
    {
        // Do not change the order for the nullable or non-nullable. Put nullable ahead of non-nullable.
        // By design: non-nullable will overwrite the item1.

        BuildTypeMapping<T?>(primitiveKind, isStandard);
        BuildTypeMapping<T>(primitiveKind, isStandard);
    }

    private static void BuildReferenceTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind, bool isStandard = true)
        where T : class
    {
        BuildTypeMapping<T>(primitiveKind, isStandard);
    }
}
