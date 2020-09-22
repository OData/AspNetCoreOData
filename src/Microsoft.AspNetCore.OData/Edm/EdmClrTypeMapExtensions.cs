// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.Spatial;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// The extensions used to map between C# types and Edm types.
    /// </summary>
    internal static class EdmClrTypeMapExtensions
    {
        #region PrimitiveTypeMapping
        /// <summary>
        /// The mapping between Edm primitive type and Clr primitive type.
        /// </summary>
        private static IDictionary<Type, IEdmPrimitiveTypeReference> _builtInPrimitiveTypes = new[]
        {
            BuildTypeMapping<string>(EdmPrimitiveTypeKind.String),
            BuildTypeMapping<bool>(EdmPrimitiveTypeKind.Boolean),
            BuildTypeMapping<bool?>(EdmPrimitiveTypeKind.Boolean),
            BuildTypeMapping<byte>(EdmPrimitiveTypeKind.Byte),
            BuildTypeMapping<byte?>(EdmPrimitiveTypeKind.Byte),
            BuildTypeMapping<decimal>(EdmPrimitiveTypeKind.Decimal),
            BuildTypeMapping<decimal?>(EdmPrimitiveTypeKind.Decimal),
            BuildTypeMapping<double>(EdmPrimitiveTypeKind.Double),
            BuildTypeMapping<double?>(EdmPrimitiveTypeKind.Double),
            BuildTypeMapping<Guid>(EdmPrimitiveTypeKind.Guid),
            BuildTypeMapping<Guid?>(EdmPrimitiveTypeKind.Guid),
            BuildTypeMapping<short>(EdmPrimitiveTypeKind.Int16),
            BuildTypeMapping<short?>(EdmPrimitiveTypeKind.Int16),
            BuildTypeMapping<int>(EdmPrimitiveTypeKind.Int32),
            BuildTypeMapping<int?>(EdmPrimitiveTypeKind.Int32),
            BuildTypeMapping<long>(EdmPrimitiveTypeKind.Int64),
            BuildTypeMapping<long?>(EdmPrimitiveTypeKind.Int64),
            BuildTypeMapping<sbyte>(EdmPrimitiveTypeKind.SByte),
            BuildTypeMapping<sbyte?>(EdmPrimitiveTypeKind.SByte),
            BuildTypeMapping<float>(EdmPrimitiveTypeKind.Single),
            BuildTypeMapping<float?>(EdmPrimitiveTypeKind.Single),
            BuildTypeMapping<byte[]>(EdmPrimitiveTypeKind.Binary),
            BuildTypeMapping<Stream>(EdmPrimitiveTypeKind.Stream),
            BuildTypeMapping<DateTimeOffset>(EdmPrimitiveTypeKind.DateTimeOffset),
            BuildTypeMapping<DateTimeOffset?>(EdmPrimitiveTypeKind.DateTimeOffset),
            BuildTypeMapping<TimeSpan>(EdmPrimitiveTypeKind.Duration),
            BuildTypeMapping<TimeSpan?>(EdmPrimitiveTypeKind.Duration),
            BuildTypeMapping<Date>(EdmPrimitiveTypeKind.Date),
            BuildTypeMapping<Date?>(EdmPrimitiveTypeKind.Date),
            BuildTypeMapping<TimeOfDay>(EdmPrimitiveTypeKind.TimeOfDay),
            BuildTypeMapping<TimeOfDay?>(EdmPrimitiveTypeKind.TimeOfDay),
            BuildTypeMapping<Geography>(EdmPrimitiveTypeKind.Geography),
            BuildTypeMapping<GeographyPoint>(EdmPrimitiveTypeKind.GeographyPoint),
            BuildTypeMapping<GeographyLineString>(EdmPrimitiveTypeKind.GeographyLineString),
            BuildTypeMapping<GeographyPolygon>(EdmPrimitiveTypeKind.GeographyPolygon),
            BuildTypeMapping<GeographyCollection>(EdmPrimitiveTypeKind.GeographyCollection),
            BuildTypeMapping<GeographyMultiLineString>(EdmPrimitiveTypeKind.GeographyMultiLineString),
            BuildTypeMapping<GeographyMultiPoint>(EdmPrimitiveTypeKind.GeographyMultiPoint),
            BuildTypeMapping<GeographyMultiPolygon>(EdmPrimitiveTypeKind.GeographyMultiPolygon),
            BuildTypeMapping<Geometry>(EdmPrimitiveTypeKind.Geometry),
            BuildTypeMapping<GeometryPoint>(EdmPrimitiveTypeKind.GeometryPoint),
            BuildTypeMapping<GeometryLineString>(EdmPrimitiveTypeKind.GeometryLineString),
            BuildTypeMapping<GeometryPolygon>(EdmPrimitiveTypeKind.GeometryPolygon),
            BuildTypeMapping<GeometryCollection>(EdmPrimitiveTypeKind.GeometryCollection),
            BuildTypeMapping<GeometryMultiLineString>(EdmPrimitiveTypeKind.GeometryMultiLineString),
            BuildTypeMapping<GeometryMultiPoint>(EdmPrimitiveTypeKind.GeometryMultiPoint),
            BuildTypeMapping<GeometryMultiPolygon>(EdmPrimitiveTypeKind.GeometryMultiPolygon),

            // non-standard mappings
            BuildTypeMapping<XElement>(EdmPrimitiveTypeKind.String),
            BuildTypeMapping<ushort>(EdmPrimitiveTypeKind.Int32),
            BuildTypeMapping<ushort?>(EdmPrimitiveTypeKind.Int32),
            BuildTypeMapping<uint>(EdmPrimitiveTypeKind.Int64),
            BuildTypeMapping<uint?>(EdmPrimitiveTypeKind.Int64),
            BuildTypeMapping<ulong>(EdmPrimitiveTypeKind.Int64),
            BuildTypeMapping<ulong?>(EdmPrimitiveTypeKind.Int64),
            BuildTypeMapping<char[]>(EdmPrimitiveTypeKind.String),
            BuildTypeMapping<char>(EdmPrimitiveTypeKind.String),
            BuildTypeMapping<char?>(EdmPrimitiveTypeKind.String),
            BuildTypeMapping<DateTime>(EdmPrimitiveTypeKind.DateTimeOffset),
            BuildTypeMapping<DateTime?>(EdmPrimitiveTypeKind.DateTimeOffset)
        }
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Gets the corresponding Edm primitive type for the given CLR type.
        /// </summary>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>Null or the Edm primitive type.</returns>
        public static IEdmPrimitiveTypeReference GetEdmPrimitiveTypeReference(this Type clrType)
        {
            return _builtInPrimitiveTypes.TryGetValue(clrType, out IEdmPrimitiveTypeReference primitive) ? primitive : null;
        }

        /// <summary>
        /// Gets the corresponding Edm primitive type for the given CLR type.
        /// </summary>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>Null or the Edm primitive type.</returns>
        public static IEdmPrimitiveType GetEdmPrimitiveType(this Type clrType)
        {
            return _builtInPrimitiveTypes.TryGetValue(clrType, out IEdmPrimitiveTypeReference primitive) ?
                (IEdmPrimitiveType)primitive.Definition : null;
        }

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm primitive type.
        /// </summary>
        /// <param name="edmPrimitiveType">The given Edm primitive type.</param>
        /// <returns>Null or the CLR type.</returns>
        public static Type GetClrPrimitiveType(this IEdmPrimitiveTypeReference edmPrimitiveType)
        {
            return _builtInPrimitiveTypes
                .Where(kvp => edmPrimitiveType.Definition.IsEquivalentTo(kvp.Value.Definition) && (!edmPrimitiveType.IsNullable || IsNullable(kvp.Key)))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
        }

        /// <summary>
        /// Figures out if the given clr type is nonstandard edm primitive like uint, ushort, char[] etc.
        /// and returns the corresponding clr type to which we map like uint => long.
        /// </summary>
        /// <param name="clrType">The potential non-standard CLR type.</param>
        /// <param name="isNonstandardEdmPrimitive">A boolean value out to indicate whether the input CLR type is standard OData primitive type.</param>
        /// <returns>The standard CLR type or the input CLR type itself.</returns>
        public static Type IsNonstandardEdmPrimitive(this Type clrType, out bool isNonstandardEdmPrimitive)
        {
            IEdmPrimitiveTypeReference edmType = clrType?.GetEdmPrimitiveTypeReference();
            if (edmType == null)
            {
                isNonstandardEdmPrimitive = false;
                return clrType;
            }

            Type reverseLookupClrType = edmType.GetClrPrimitiveType();
            isNonstandardEdmPrimitive = (clrType != reverseLookupClrType);

            return reverseLookupClrType;
        }
        #endregion

        #region ClrType -> EdmType

        /// <summary>
        /// Gets the Edm type reference from the CLR type.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>null or the Edm type reference.</returns>
        public static IEdmTypeReference GetEdmTypeReference(this IEdmModel edmModel, Type clrType)
        {
            IEdmType edmType = edmModel.GetEdmType(clrType);
            if (edmType != null)
            {
                bool isNullable = IsNullable(clrType);
                return edmType.ToEdmTypeReference(isNullable);
            }

            return null;
        }

        /// <summary>
        /// Gets the Edm type from the CLR type.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>null or the Edm type.</returns>
        public static IEdmType GetEdmType(this IEdmModel edmModel, Type clrType)
        {
            if (edmModel == null)
            {
                throw new ArgumentNullException(nameof(edmModel));
            }

            if (clrType == null)
            {
                throw new ArgumentNullException(nameof(clrType));
            }

            return GetEdmType(edmModel, clrType, testCollections: true);
        }

        private static IEdmType GetEdmType(IEdmModel edmModel, Type clrType, bool testCollections)
        {
            Contract.Assert(edmModel != null);
            Contract.Assert(clrType != null);

            IEdmPrimitiveType primitiveType = clrType.GetEdmPrimitiveType();
            if (primitiveType != null)
            {
                return primitiveType;
            }
            else
            {
                if (testCollections)
                {
                    Type enumerableOfT = ExtractGenericInterface(clrType, typeof(IEnumerable<>));
                    if (enumerableOfT != null)
                    {
                        Type elementClrType = enumerableOfT.GetGenericArguments()[0];

                        // IEnumerable<SelectExpandWrapper<T>> is a collection of T.
                        Type entityType;
                        if (IsSelectExpandWrapper(elementClrType, out entityType))
                        {
                            elementClrType = entityType;
                        }

                        if (IsComputeWrapper(elementClrType, out entityType))
                        {
                            elementClrType = entityType;
                        }

                        IEdmType elementType = GetEdmType(edmModel, elementClrType, testCollections: false);
                        if (elementType != null)
                        {
                            return new EdmCollectionType(elementType.ToEdmTypeReference(IsNullable(elementClrType)));
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
        /// Gets the corresponding CLR type for a given Edm type reference.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="edmTypeReference">The Edm type reference.</param>
        /// <returns>Null or the CLR type.</returns>
        public static Type GetClrType(this IEdmModel edmModel, IEdmTypeReference edmTypeReference)
        {
            return edmModel.GetClrType(edmTypeReference, AssemblyResolverHelper.Default);
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
            if (edmTypeReference == null)
            {
                throw new ArgumentNullException(nameof(edmTypeReference));
            }

            if (edmTypeReference.IsPrimitive())
            {
                return GetClrPrimitiveType((IEdmPrimitiveTypeReference)edmTypeReference);
            }
            else
            {
                Type clrType = edmModel.GetClrType(edmTypeReference.Definition, assembliesResolver);
                if (clrType != null  && clrType.IsEnum && edmTypeReference.IsNullable)
                {
                    return TypeHelper.ToNullable(clrType);
                }

                return clrType;
            }
        }

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm type reference.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="edmType">The Edm type.</param>
        /// <returns>Null or the CLR type.</returns>
        internal static Type GetClrType(this IEdmModel edmModel, IEdmType edmType)
        {
            return edmModel.GetClrType(edmType, AssemblyResolverHelper.Default);
        }

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm type reference.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="edmType">The Edm type.</param>
        /// <param name="assembliesResolver">The assembly resolver.</param>
        /// <returns>Null or the CLR type.</returns>
        internal static Type GetClrType(this IEdmModel edmModel, IEdmType edmType, IAssemblyResolver assembliesResolver)
        {
            if (edmType == null)
            {
                throw new ArgumentNullException(nameof(edmType));
            }

            IEdmSchemaType edmSchemaType = edmType as IEdmSchemaType;
            Contract.Assert(edmSchemaType != null);

            ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmSchemaType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            string typeName = edmSchemaType.FullName();
            IEnumerable<Type> matchingTypes = GetMatchingTypes(typeName, assembliesResolver);

            if (matchingTypes.Count() > 1)
            {
                throw Error.Argument("edmTypeReference", SRResources.MultipleMatchingClrTypesForEdmType,
                    typeName, string.Join(",", matchingTypes.Select(type => type.AssemblyQualifiedName)));
            }

            Type type = matchingTypes.SingleOrDefault();
            if (type == null)
            {
                return null;
            }

            edmModel.SetAnnotationValue(edmSchemaType, new ClrTypeAnnotation(matchingTypes.SingleOrDefault()));
            return matchingTypes.SingleOrDefault();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal static ClrTypeCache GetTypeMappingCache(this IEdmModel model)
        {
            Contract.Assert(model != null);

            ClrTypeCache typeMappingCache = model.GetAnnotationValue<ClrTypeCache>(model);
            if (typeMappingCache == null)
            {
                typeMappingCache = new ClrTypeCache();
                model.SetAnnotationValue(model, typeMappingCache);
            }

            return typeMappingCache;
        }

        private static IEnumerable<Type> GetMatchingTypes(string edmFullName, IAssemblyResolver assembliesResolver)
        {
            return TypeHelper.GetLoadedTypes(assembliesResolver).Where(t => t.IsPublic && t.EdmFullName() == edmFullName);
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

        // TODO (workitem 336): Support nested types and anonymous types.
        private static string MangleClrTypeName(Type type)
        {
            Contract.Assert(type != null);

            if (!TypeHelper.IsGenericType(type))
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

        private static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => TypeHelper.IsGenericType(t) && t.GetGenericTypeDefinition() == interfaceType;
            return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        private static bool IsSelectExpandWrapper(Type type, out Type entityType) => IsTypeWrapper(typeof(SelectExpandWrapper<>), type, out entityType);

        internal static bool IsComputeWrapper(Type type, out Type entityType) => IsTypeWrapper(typeof(ComputeWrapper<>), type, out entityType);

        private static bool IsTypeWrapper(Type wrappedType, Type type, out Type entityType)
        {
            if (type == null)
            {
                entityType = null;
                return false;
            }

            if (TypeHelper.IsGenericType(type) && type.GetGenericTypeDefinition() == wrappedType)
            {
                entityType = type.GetGenericArguments()[0];
                return true;
            }

            return IsTypeWrapper(wrappedType, type.BaseType, out entityType);
        }

        private static KeyValuePair<Type, IEdmPrimitiveTypeReference> BuildTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind)
            => new KeyValuePair<Type, IEdmPrimitiveTypeReference>(typeof(T), EdmCoreModel.Instance.GetPrimitive(primitiveKind, IsNullable<T>()));

        /// <summary>
        /// Check the input type is nullable type or not.
        /// </summary>
        /// <param name="type">The input CLR type.</param>
        /// <returns>True/False.</returns>
        private static bool IsNullable(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Check the input type is nullable or not.
        /// </summary>
        /// <typeparam name="T">The test CRL type.</typeparam>
        /// <returns>True/False.</returns>
        private static bool IsNullable<T>()
        {
            Type type = typeof(T);
            return IsNullable(type);
        }
    }
}
