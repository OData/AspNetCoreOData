// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Abstracts.Annotations;
using Microsoft.AspNetCore.OData.Abstracts.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.Spatial;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// 
    /// </summary>
    public class ODataTypeMappingProvider : IODataTypeMappingProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private ConcurrentDictionary<Type, IEdmTypeReference> _cache = new ConcurrentDictionary<Type, IEdmTypeReference>();

        /// <summary>
        /// the built-in mapping between Edm primitive type and the corresponding CLR type
        /// </summary>
        private IDictionary<Type, Type> _nonStandardPrimitiveTypes;

        /// <summary>
        /// The registered assembly resolver.
        /// </summary>
        private IAssemblyResolver _assemblyResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataTypeMappingProvider"/> class.
        /// </summary>
        /// <param name="resolver">The registered assembly resolver.</param>
        public ODataTypeMappingProvider(IAssemblyResolver resolver)
        {
            _assemblyResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public virtual Type MapTo(Type clrType)
        {
            return _nonStandardPrimitiveTypes.TryGetValue(clrType, out Type type) ? type : clrType;
        }

        /// <summary>
        /// Gets the corresponding Edm primitive type for the given CLR type.
        /// </summary>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>Null or the Edm primitive type.</returns>
        public virtual IEdmPrimitiveTypeReference GetEdmPrimitiveType(Type clrType)
        {
            if (_cache.TryGetValue(clrType, out IEdmTypeReference edmType))
            {
                if (edmType.IsPrimitive())
                {
                    return edmType.AsPrimitive();
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm primitive type.
        /// </summary>
        /// <param name="edmPrimitiveType">The given Edm primitive type.</param>
        /// <returns>Null or the CLR type.</returns>
        public virtual Type GetClrPrimitiveType(IEdmPrimitiveTypeReference edmPrimitiveType)
        {
            return _cache
                .Where(kvp => edmPrimitiveType.Definition.IsEquivalentTo(kvp.Value.Definition) && (!edmPrimitiveType.IsNullable || kvp.Key.IsNullable()))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm type.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="edmType">The Edm type.</param>
        /// <returns>Null or the CLR type.</returns>
        public virtual Type GetClrType(IEdmModel model, IEdmTypeReference edmType)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (edmType == null)
            {
                throw new ArgumentNullException(nameof(edmType));
            }

            Type clrType = _cache.Where(kvp => edmType.Definition.IsEquivalentTo(kvp.Value.Definition) && (!edmType.IsNullable || kvp.Key.IsNullable()))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
            if (clrType != null)
            {
                return clrType;
            }

            clrType = GetClrType(model, edmType.Definition);
            if (clrType != null && TypeHelper.IsEnum(clrType) && edmType.IsNullable)
            {
                clrType = TypeHelper.ToNullable(clrType);
            }

            _cache[clrType] = edmType;
            return clrType;
        }

        /// <summary>
        /// Gets the Edm type for the given Clr Type.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="clrType">The given Clr type.</param>
        /// <returns>Null or the Edm type.</returns>
        public virtual IEdmTypeReference GetEdmType(IEdmModel model, Type clrType)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            IEdmTypeReference edmType;
            if (_cache.TryGetValue(clrType, out edmType))
            {
                return edmType;
            }

            edmType = GetEdmType(model, clrType, testCollections: true);
            _cache[clrType] = edmType;
            return edmType;
        }

        private IEdmTypeReference GetEdmType(IEdmModel edmModel, Type clrType, bool testCollections)
        {
            Contract.Assert(edmModel != null);
            Contract.Assert(clrType != null);

            IEdmPrimitiveTypeReference primitiveType = GetEdmPrimitiveType(clrType);
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
                        //Type entityType;
                        //if (IsSelectExpandWrapper(elementClrType, out entityType))
                        //{
                        //    elementClrType = entityType;
                        //}

                        //if (IsComputeWrapper(elementClrType, out entityType))
                        //{
                        //    elementClrType = entityType;
                        //}

                        IEdmTypeReference elementType = GetEdmType(edmModel, elementClrType, testCollections: false);
                        if (elementType != null)
                        {
                            return new EdmCollectionTypeReference(new EdmCollectionType(elementType));
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

                if (returnType != null)
                {
                    return returnType.ToEdmTypeReference(clrType.IsNullable());
                }

                if (clrType.BaseType != null)
                {
                    // go up the inheritance tree to see if we have a mapping defined for the base type.
                    return GetEdmType(edmModel, clrType.BaseType, testCollections);
                }

                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmModel"></param>
        /// <param name="edmType"></param>
        /// <returns></returns>
        private Type GetClrType(IEdmModel edmModel, IEdmType edmType)
        {
            Contract.Assert(edmModel != null);
            Contract.Assert(edmType != null);

            IEdmSchemaType edmSchemaType = edmType as IEdmSchemaType;
            if(edmSchemaType == null)
            {
                return null;
            }

            ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmSchemaType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            string typeName = edmSchemaType.FullName();
            IEnumerable<Type> matchingTypes = GetMatchingTypes(typeName);

            if (matchingTypes.Count() > 1)
            {
                throw Error.Argument("edmTypeReference", SRResources.MultipleMatchingClrTypesForEdmType,
                    typeName, String.Join(",", matchingTypes.Select(type => type.AssemblyQualifiedName)));
            }

            Type clrType = matchingTypes.SingleOrDefault();
            edmModel.SetAnnotationValue(edmSchemaType, new ClrTypeAnnotation(clrType));
            return clrType;
        }

        private static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => TypeHelper.IsGenericType(t) && t.GetGenericTypeDefinition() == interfaceType;
            return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        private IEnumerable<Type> GetMatchingTypes(string edmFullName)
            => TypeHelper.GetLoadedTypes(_assemblyResolver).Where(t => t.IsPublic && t.EdmFullName() == edmFullName);

        //private static KeyValuePair<Type, IEdmPrimitiveType> BuildTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind)
        //    => new KeyValuePair<Type, IEdmPrimitiveType>(typeof(T), EdmCoreModel.Instance.GetPrimitiveType(primitiveKind));

        private void InitPrimitiveTypes()
        {
            foreach ((Type type, EdmPrimitiveTypeKind kind) in new[]
                {
                    (typeof(string), EdmPrimitiveTypeKind.String),
                    (typeof(bool), EdmPrimitiveTypeKind.Boolean),
                    (typeof(bool?), EdmPrimitiveTypeKind.Boolean),
                    (typeof(byte), EdmPrimitiveTypeKind.Byte),
                    (typeof(byte?), EdmPrimitiveTypeKind.Byte),
                    (typeof(decimal), EdmPrimitiveTypeKind.Decimal),
                    (typeof(decimal?), EdmPrimitiveTypeKind.Decimal),
                    (typeof(double), EdmPrimitiveTypeKind.Double),
                    (typeof(double?), EdmPrimitiveTypeKind.Double),
                    (typeof(Guid), EdmPrimitiveTypeKind.Guid),
                    (typeof(Guid?), EdmPrimitiveTypeKind.Guid),
                    (typeof(short), EdmPrimitiveTypeKind.Int16),
                    (typeof(short?), EdmPrimitiveTypeKind.Int16),
                    (typeof(int), EdmPrimitiveTypeKind.Int32),
                    (typeof(int?), EdmPrimitiveTypeKind.Int32),
                    (typeof(long), EdmPrimitiveTypeKind.Int64),
                    (typeof(long?), EdmPrimitiveTypeKind.Int64),
                    (typeof(sbyte), EdmPrimitiveTypeKind.SByte),
                    (typeof(sbyte?), EdmPrimitiveTypeKind.SByte),
                    (typeof(float), EdmPrimitiveTypeKind.Single),
                    (typeof(float?), EdmPrimitiveTypeKind.Single),
                    (typeof(byte[]), EdmPrimitiveTypeKind.Binary),
                    (typeof(Stream), EdmPrimitiveTypeKind.Stream),
                    (typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset),
                    (typeof(DateTimeOffset?), EdmPrimitiveTypeKind.DateTimeOffset),
                    (typeof(TimeSpan), EdmPrimitiveTypeKind.Duration),
                    (typeof(TimeSpan?), EdmPrimitiveTypeKind.Duration),
                    (typeof(Date), EdmPrimitiveTypeKind.Date),
                    (typeof(Date?), EdmPrimitiveTypeKind.Date),
                    (typeof(TimeOfDay), EdmPrimitiveTypeKind.TimeOfDay),
                    (typeof(TimeOfDay?), EdmPrimitiveTypeKind.TimeOfDay),
                    (typeof(Geography), EdmPrimitiveTypeKind.Geography),
                    (typeof(GeographyPoint), EdmPrimitiveTypeKind.GeographyPoint),
                    (typeof(GeographyLineString), EdmPrimitiveTypeKind.GeographyLineString),
                    (typeof(GeographyPolygon), EdmPrimitiveTypeKind.GeographyPolygon),
                    (typeof(GeographyCollection), EdmPrimitiveTypeKind.GeographyCollection),
                    (typeof(GeographyMultiLineString), EdmPrimitiveTypeKind.GeographyMultiLineString),
                    (typeof(GeographyMultiPoint), EdmPrimitiveTypeKind.GeographyMultiPoint),
                    (typeof(GeographyMultiPolygon), EdmPrimitiveTypeKind.GeographyMultiPolygon),
                    (typeof(Geometry), EdmPrimitiveTypeKind.Geometry),
                    (typeof(GeometryPoint), EdmPrimitiveTypeKind.GeometryPoint),
                    (typeof(GeometryLineString), EdmPrimitiveTypeKind.GeometryLineString),
                    (typeof(GeometryPolygon), EdmPrimitiveTypeKind.GeometryPolygon),
                    (typeof(GeometryCollection), EdmPrimitiveTypeKind.GeometryCollection),
                    (typeof(GeometryMultiLineString), EdmPrimitiveTypeKind.GeometryMultiLineString),
                    (typeof(GeometryMultiPoint), EdmPrimitiveTypeKind.GeometryMultiPoint),
                    (typeof(GeometryMultiPolygon), EdmPrimitiveTypeKind.GeometryMultiPolygon),
                    // below are non-standard
                    (typeof(XElement), EdmPrimitiveTypeKind.String),
                    (typeof(ushort), EdmPrimitiveTypeKind.Int32),
                    (typeof(ushort?), EdmPrimitiveTypeKind.Int32),
                    (typeof(uint), EdmPrimitiveTypeKind.Int64),
                    (typeof(uint?), EdmPrimitiveTypeKind.Int64),
                    (typeof(ulong), EdmPrimitiveTypeKind.Int64),
                    (typeof(ulong?), EdmPrimitiveTypeKind.Int64),
                    (typeof(char[]), EdmPrimitiveTypeKind.String),
                    (typeof(char), EdmPrimitiveTypeKind.String),
                    (typeof(char?), EdmPrimitiveTypeKind.String),
                    (typeof(DateTime), EdmPrimitiveTypeKind.DateTimeOffset),
                    (typeof(DateTime?), EdmPrimitiveTypeKind.DateTimeOffset),
                })
            {
                _cache[type] = EdmCoreModel.Instance.GetPrimitive(kind, type.IsNullable());
            }

            _nonStandardPrimitiveTypes = new Dictionary<Type, Type>
            {
                { typeof(XElement), typeof(string) },
                { typeof(ushort), typeof(int) },
                { typeof(ushort?), typeof(int?) },
                { typeof(uint), typeof(long) },
                { typeof(uint?), typeof(long?) },
                { typeof(ulong), typeof(long) },
                { typeof(ulong?), typeof(long?) },
                { typeof(char[]), typeof(string) },
                { typeof(char), typeof(string) },
                { typeof(char?), typeof(string) },
                { typeof(DateTime), typeof(DateTimeOffset) },
                { typeof(DateTime?), typeof(DateTimeOffset?) }
            };
        }

        private static KeyValuePair<Type, IEdmPrimitiveTypeReference> BuildTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind)
            => new KeyValuePair<Type, IEdmPrimitiveTypeReference>(typeof(T), EdmCoreModel.Instance.GetPrimitive(primitiveKind, typeof(T).IsNullable()));
    }
}
