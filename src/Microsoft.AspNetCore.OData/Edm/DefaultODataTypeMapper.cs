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
    /// Creates a static instance for the default type mapper.
    /// </summary>
    internal static readonly DefaultODataTypeMapper Default;

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
        Default = new DefaultODataTypeMapper();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultODataTypeMapper"/>.
    /// </summary>
    /// <remarks>
    /// The constructor wires up three buckets of primitive mappings:
    /// - Standard primitives: 1:1 CLR-to-Edm mappings that require no runtime conversion.
    /// - Spatial primitives: Microsoft.Spatial geography/geometry types to Edm spatial kinds.
    /// - Non-standard primitives: CLR types that do not have an Edm counterpart and are represented
    ///   by another Edm primitive (for example, <see cref="char"/> → <see cref="EdmPrimitiveTypeKind.String"/>,
    ///   <see cref="uint"/> → <see cref="EdmPrimitiveTypeKind.Int64"/>).
    /// Non-standard mappings are intentionally not added to reverse Edm → CLR lookup so they don't leak into the model
    /// and are instead normalized at query-time.
    /// </remarks>
    protected DefaultODataTypeMapper()
    {
        // Default primitive type mappings
        RegisterDefaultMappings();
        // Spatial mappings
        RegisterSpatialMappings();
        // Non-standard mappings
        RegisterNonStandardMappings();
    }
    #endregion

    /// <summary>
    /// Registers the standard CLR-to-Edm primitive mappings.
    /// </summary>
    /// <remarks>
    /// “Standard” means the CLR type has a direct Edm primitive equivalent and can round-trip without
    /// normalization or provider-specific conversions during query translation.
    /// Override to add/remove mappings or to change the defaults for a custom stack.
    /// </remarks>
    protected virtual void RegisterDefaultMappings()
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
        BuildValueTypeMapping<Date>(EdmPrimitiveTypeKind.Date);
        BuildValueTypeMapping<TimeOfDay>(EdmPrimitiveTypeKind.TimeOfDay);
    }

    /// <summary>
    /// Registers non-standard CLR primitive mappings that are represented by another Edm primitive.
    /// </summary>
    /// <remarks>
    /// Examples of non-standard CLR types and their Edm representations:
    /// - <see cref="ushort"/> → <see cref="EdmPrimitiveTypeKind.Int32"/>
    /// - <see cref="uint"/>, <see cref="ulong"/> → <see cref="EdmPrimitiveTypeKind.Int64"/>
    /// - <see cref="char"/>, <see cref="char"/>[], <see cref="XElement"/> → <see cref="EdmPrimitiveTypeKind.String"/>
    /// - <see cref="DateTime"/> → <see cref="EdmPrimitiveTypeKind.DateTimeOffset"/>
    /// - <see cref="DateOnly"/> → <see cref="EdmPrimitiveTypeKind.Date"/>
    /// - <see cref="TimeOnly"/> → <see cref="EdmPrimitiveTypeKind.TimeOfDay"/>
    /// These mappings are used for CLR → Edm lookups. They are not registered in the reverse Edm → CLR table:
    /// the query binder normalizes such values at runtime,
    /// ensuring providers (e.g., EF Core) receive supported primitives.
    /// </remarks>
    protected virtual void RegisterNonStandardMappings()
    {
        BuildReferenceTypeMapping<XElement>(EdmPrimitiveTypeKind.String, isStandard: false);
        BuildValueTypeMapping<ushort>(EdmPrimitiveTypeKind.Int32, isStandard: false);
        BuildValueTypeMapping<uint>(EdmPrimitiveTypeKind.Int64, isStandard: false);
        BuildValueTypeMapping<ulong>(EdmPrimitiveTypeKind.Int64, isStandard: false);
        BuildReferenceTypeMapping<char[]>(EdmPrimitiveTypeKind.String, isStandard: false);
        BuildValueTypeMapping<char>(EdmPrimitiveTypeKind.String, isStandard: false);
        BuildValueTypeMapping<DateTime>(EdmPrimitiveTypeKind.DateTimeOffset, isStandard: false);
        BuildValueTypeMapping<DateOnly>(EdmPrimitiveTypeKind.Date, isStandard: false);
        BuildValueTypeMapping<TimeOnly>(EdmPrimitiveTypeKind.TimeOfDay, isStandard: false);
    }

    /// <summary>
    /// Registers geography/geometry CLR types to Edm spatial primitive kinds.
    /// </summary>
    /// <remarks>
    /// This mapping assumes Microsoft.Spatial abstractions are used for Edm spatial types on the wire.
    /// If your application introduces an alternate spatial stack, override this method to register the desired
    /// CLR spatial types to Edm geography/geometry kinds.
    /// </remarks>
    protected virtual void RegisterSpatialMappings()
    {
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
    }

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


    /// <summary>
    /// Adds a CLR-to-EDM primitive mapping entry.
    /// </summary>
    /// <typeparam name="T">The CLR type being mapped.</typeparam>
    /// <param name="primitiveKind">The Edm primitive kind to map to.</param>
    /// <param name="isStandard">
    /// Whether this is a “standard” mapping (true) or a “non-standard” normalization mapping (false).
    /// Standard mappings are also registered for reverse Edm → CLR lookup; non-standard mappings are not,
    /// so the CLR non-standard type will not appear in Edm → CLR resolutions and will be normalized at bind-time.
    /// </param>
    protected virtual void BuildTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind, bool isStandard)
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
                // And since we make the order un-changeable, it means 'nullable' coming first.
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

    /// <summary>
    /// Adds a value-type CLR → Edm mapping for both nullable and non-nullable variants.
    /// </summary>
    /// <typeparam name="T">The non-nullable CLR value type (struct).</typeparam>
    /// <param name="primitiveKind">The Edm primitive kind to map to.</param>
    /// <param name="isStandard">
    /// Whether this is a “standard” mapping (true) or a “non-standard” normalization mapping (false).
    /// See <see cref="BuildTypeMapping{T}(EdmPrimitiveTypeKind, bool)"/> for behavior differences.
    /// </param>
    /// <remarks>
    /// Ordering matters: nullable is registered first so the reverse Edm → CLR cache can correctly store both
    /// nullable and non-nullable standard targets.
    /// </remarks>
    protected virtual void BuildValueTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind, bool isStandard = true)
        where T : struct
    {
        // Do not change the order for the nullable or non-nullable. Put nullable ahead of non-nullable.
        // By design: non-nullable will overwrite the item1.

        BuildTypeMapping<T?>(primitiveKind, isStandard);
        BuildTypeMapping<T>(primitiveKind, isStandard);
    }

    /// <summary>
    /// Adds a reference-type CLR → Edm mapping.
    /// </summary>
    /// <typeparam name="T">The CLR reference type (class).</typeparam>
    /// <param name="primitiveKind">The Edm primitive kind to map to.</param>
    /// <param name="isStandard">
    /// Whether this is a “standard” mapping (true) or a “non-standard” normalization mapping (false).
    /// See <see cref="BuildTypeMapping{T}(EdmPrimitiveTypeKind, bool)"/> for behavior differences.
    /// </param>
    protected virtual void BuildReferenceTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind, bool isStandard = true)
        where T : class
    {
        BuildTypeMapping<T>(primitiveKind, isStandard);
    }
}
