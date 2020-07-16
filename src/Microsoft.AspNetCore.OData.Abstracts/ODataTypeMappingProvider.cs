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
    /// The default implementation for <see cref="IODataTypeMappingProvider"/>.
    /// </summary>
    public class ODataTypeMappingProvider : IODataTypeMappingProvider
    {

        private ConcurrentDictionary<IEdmModel, ConcurrentDictionary<Type, IEdmTypeReference>> _cache
            = new ConcurrentDictionary<IEdmModel, ConcurrentDictionary<Type, IEdmTypeReference>>();

        /// <summary>
        /// The mapping from <see cref="Type"/> to <see cref="IEdmTypeReference"/>.
        /// </summary>
        private ConcurrentDictionary<Type, IEdmTypeReference> _clrToEdmTypeReference = new ConcurrentDictionary<Type, IEdmTypeReference>();


        /// <summary>
        /// The mapping from <see cref="IEdmType"/> to <see cref="Type"/>.
        /// </summary>
        private ConcurrentDictionary<IEdmType, Type> _edmTypeToClr = new ConcurrentDictionary<IEdmType, Type>();

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

           // InitPrimitiveTypes();
        }

        /// <summary>
        /// Maps a CLR type to standard CLR type.
        /// </summary>
        /// <param name="clrType">The potential non-standard CLR type.</param>
        /// <returns>The standard CLR type or the input CLR type itself.</returns>
        public virtual Type MapTo(Type clrType)
        {
            if (clrType == null)
            {
                return null;
            }

            //if (_clrToEdmTypeReference.TryGetValue(clrType, out IEdmTypeReference edmType))
            //{
            //    if (_edmTypeToClr.TryGetValue(edmType.Definition, out Type newType))
            //    {
            //        if (clrType.IsNullable())
            //        {
            //            return TypeHelper.ToNullable(newType);
            //        }

            //        return newType;
            //    }
            //}

            IEdmPrimitiveTypeReference edmTypeRef = GetEdmPrimitiveType(clrType);
            if (edmTypeRef != null)
            {
                return GetClrPrimitiveType(edmTypeRef);
            }

            return clrType;
        }

        /// <summary>
        /// Gets the corresponding Edm primitive type for the given CLR type.
        /// </summary>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>Null or the Edm primitive type.</returns>
        public virtual IEdmPrimitiveTypeReference GetEdmPrimitiveType(Type clrType)
        {
            return BuiltInPrimitiveTypes.TryGetValue(clrType, out IEdmPrimitiveTypeReference primitive) ? primitive : null;
        }

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm primitive type.
        /// </summary>
        /// <param name="edmPrimitiveType">The given Edm primitive type.</param>
        /// <returns>Null or the CLR type.</returns>
        public virtual Type GetClrPrimitiveType(IEdmPrimitiveTypeReference edmPrimitiveType)
        {
            return BuiltInPrimitiveTypes
                .Where(kvp => edmPrimitiveType.Definition.IsEquivalentTo(kvp.Value.Definition) && (!edmPrimitiveType.IsNullable || kvp.Key.IsNullable()))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
        }

        

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm type reference.
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

            if (edmType.IsPrimitive())
            {
                return GetClrPrimitiveType((IEdmPrimitiveTypeReference)edmType);
            }

            Type clrType = GetClrTypeInCache(model, edmType);
            if (clrType != null)
            {
                return clrType;
            }


            IEdmType edmTypeDefinition = edmType.Definition;
            //if (_edmTypeToClr.TryGetValue(edmTypeDefinition, out clrType))
            //{
            //    if (edmType.IsNullable && (clrType.IsValueType || clrType.IsEnum))
            //    {
            //        return TypeHelper.ToNullable(clrType);
            //    }
            //    else
            //    {
            //        return clrType;
            //    }
            //}

            // If not found, find the CLR type from the model.
            clrType = FindClrType(model, edmTypeDefinition);

            //_edmTypeToClr[edmType.Definition] = clrType; // could be null
            AddTypeMapping(model, clrType, edmType);

            if (clrType != null && edmType.IsNullable && clrType.IsEnum)
            {
                clrType = TypeHelper.ToNullable(clrType);
            }

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

            return FindEdmType(model, clrType, testCollections: true);
        }

        private Type GetClrTypeInCache(IEdmModel model, IEdmTypeReference edmType)
        {
            if (edmType.IsPrimitive())
            {
                return GetClrPrimitiveType((IEdmPrimitiveTypeReference)edmType);
            }

            if (!_cache.TryGetValue(model, out ConcurrentDictionary<Type, IEdmTypeReference> map))
            {
                map = new ConcurrentDictionary<Type, IEdmTypeReference>();
                _cache[model] = map;
            }

            return map
                .Where(kvp => edmType.Definition.IsEquivalentTo(kvp.Value.Definition) && (!edmType.IsNullable || kvp.Key.IsNullable()))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
        }

        private void AddTypeMapping(IEdmModel model, Type clrType, IEdmTypeReference edmType)
        {
            if (!_cache.TryGetValue(model, out ConcurrentDictionary<Type, IEdmTypeReference> map))
            {
                map = new ConcurrentDictionary<Type, IEdmTypeReference>();
                _cache[model] = map;
            }

            map[clrType] = edmType;
        }

        private IEdmTypeReference GetEdmTypeInCache(IEdmModel model, Type clrType)
        {
            IEdmPrimitiveTypeReference edmPrimitiveTypeReference = GetEdmPrimitiveType(clrType);
            if (edmPrimitiveTypeReference != null)
            {
                return edmPrimitiveTypeReference;
            }

            if (!_cache.TryGetValue(model, out ConcurrentDictionary<Type, IEdmTypeReference> map))
            {
                map = new ConcurrentDictionary<Type, IEdmTypeReference>();
                _cache[model] = map;
            }

            return map.TryGetValue(clrType, out IEdmTypeReference edmType) ? edmType : null;
        }

        private IEdmTypeReference FindEdmType(IEdmModel model, Type clrType, bool testCollections)
        {
            Contract.Assert(model != null);
            Contract.Assert(clrType != null);


            IEdmTypeReference edmTypeRef = GetEdmTypeInCache(model, clrType);
            if (edmTypeRef != null)
            {
                return edmTypeRef;
            }

            //IEdmTypeReference edmType;
            //if (_clrToEdmTypeReference.TryGetValue(clrType, out edmType))
            //{
            //    return edmType;
            //}

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

                    IEdmTypeReference elementType = FindEdmType(model, elementClrType, testCollections: false);
                    if (elementType != null)
                    {
                        edmTypeRef = new EdmCollectionTypeReference(new EdmCollectionType(elementType));
                        AddTypeMapping(model, clrType, edmTypeRef);
                        return edmTypeRef;
                    }
                }
            }

            Type backupClrType = clrType;
            bool isNullable = backupClrType.IsNullable();

            Type underlyingType = TypeHelper.GetUnderlyingTypeOrSelf(clrType);
            if (TypeHelper.IsEnum(underlyingType))
            {
                clrType = underlyingType;
            }

            // search for the ClrTypeAnnotation and return it if present
            IEdmType returnType =
                model
                .SchemaElements
                .OfType<IEdmType>()
                .Select(edmType => new { EdmType = edmType, Annotation = model.GetAnnotationValue<ClrTypeAnnotation>(edmType) })
                .Where(tuple => tuple.Annotation != null && tuple.Annotation.ClrType == clrType)
                .Select(tuple => tuple.EdmType)
                .SingleOrDefault();

            // default to the EdmType with the same name as the ClrType name
            returnType = returnType ?? model.FindType(clrType.EdmFullName());

            if (returnType != null)
            {
                edmTypeRef = returnType.ToEdmTypeReference(isNullable);

                AddTypeMapping(model, backupClrType, edmTypeRef);
                return edmTypeRef;
            }

            if (clrType.BaseType != null)
            {
                // go up the inheritance tree to see if we have a mapping defined for the base type.
                return FindEdmType(model, clrType.BaseType, testCollections);
            }

            AddTypeMapping(model, backupClrType, null);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmModel"></param>
        /// <param name="edmType"></param>
        /// <returns></returns>
        private Type FindClrType(IEdmModel edmModel, IEdmType edmType)
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
            EdmCoreModel coreModel = EdmCoreModel.Instance;

            // be noted: don't change the order. we put the nullable after non-nullable to make sure
            // _edmTypeToClr only contains the non-nullable CLR type.
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
                })
            {
                IEdmPrimitiveTypeReference primitiveType = coreModel.GetPrimitive(kind, type.IsNullable());
                _clrToEdmTypeReference[type] = primitiveType;

                if (!_edmTypeToClr.ContainsKey(primitiveType.Definition))
                {
                    _edmTypeToClr[primitiveType.Definition] = type;
                }
            }

            // below are non-standard
            foreach ((Type type, EdmPrimitiveTypeKind kind) in new[]
            {
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
                (typeof(DateTime?), EdmPrimitiveTypeKind.DateTimeOffset)
            })
            {
                _clrToEdmTypeReference[type] = coreModel.GetPrimitive(kind, type.IsNullable());
            }
        }

        private static IDictionary<Type, IEdmPrimitiveTypeReference> BuiltInPrimitiveTypes = new[]
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
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        private static KeyValuePair<Type, IEdmPrimitiveTypeReference> BuildTypeMapping<T>(EdmPrimitiveTypeKind primitiveKind)
            => new KeyValuePair<Type, IEdmPrimitiveTypeReference>(typeof(T), EdmCoreModel.Instance.GetPrimitive(primitiveKind, typeof(T).IsNullable()));
    }
}
