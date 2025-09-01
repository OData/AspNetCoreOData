//-----------------------------------------------------------------------------
// <copyright file="ODataNetTopologySuiteEdmTypeMappingProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Providers;
using Geometry = NetTopologySuite.Geometries.Geometry;
using GeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using LineString = NetTopologySuite.Geometries.LineString;
using MultiLineString = NetTopologySuite.Geometries.MultiLineString;
using MultiPoint = NetTopologySuite.Geometries.MultiPoint;
using MultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Providers;

/// <summary>
/// Maps NetTopologySuite (NTS) spatial CLR types to OData Edm primitive spatial types and vice versa.
/// </summary>
/// <remarks>
/// - CLR to Edm: returns Edm.Geometry* kinds for NTS types (e.g., Point → Edm.GeometryPoint).
///   Per-property geography mapping can be enabled via conventions that set the target Edm kind.
/// - Edm to CLR: resolves both Edm.Geometry* and Edm.Geography* kinds to the corresponding NTS types,
///   since NTS represents both with the same CLR types.
/// - Nullability: when mapping Edm to CLR, the provider ensures the returned CLR type is compatible
///   with the Edm nullability (reference types are always nullable).
/// </remarks>
public class ODataNetTopologySuiteEdmTypeMappingProvider : IEdmTypeMappingProvider
{
    private static readonly EdmCoreModel _coreModel = EdmCoreModel.Instance;

    private static readonly Dictionary<Type, IEdmPrimitiveType> _spatialToEdmTypesMapping =
        new[]
        {
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Point), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(LineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Polygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(MultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(MultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(MultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geometry), GetPrimitiveType(EdmPrimitiveTypeKind.Geometry)),
        }
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    private static readonly Dictionary<IEdmPrimitiveType, Type> _edmToSpatialTypesMapping =
        new[]
        {
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint), typeof(Point)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint), typeof(Point)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString), typeof(LineString)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString), typeof(LineString)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon), typeof(Polygon)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon), typeof(Polygon)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint), typeof(MultiPoint)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint), typeof(MultiPoint)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString), typeof(MultiLineString)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString), typeof(MultiLineString)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon), typeof(MultiPolygon)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon), typeof(MultiPolygon)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection), typeof(GeometryCollection)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection), typeof(GeometryCollection)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.Geometry), typeof(Geometry)),
                new KeyValuePair<IEdmPrimitiveType, Type>(GetPrimitiveType(EdmPrimitiveTypeKind.Geography), typeof(Geometry)),
        }
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    /// <inheritdoc/>
    public bool TryGetEdmType(Type clrType, out IEdmPrimitiveType primitiveType)
    {
        if (clrType.Equals(typeof(Point)))
        {

        }
        return _spatialToEdmTypesMapping.TryGetValue(clrType, out primitiveType);
    }

    /// <inheritdoc/>
    public bool TryGetClrType(IEdmTypeReference edmTypeReference, out Type clrType)
    {
        clrType = null;

        if (edmTypeReference == null || !edmTypeReference.IsPrimitive())
        {
            return false;
        }

        foreach (KeyValuePair<IEdmPrimitiveType, Type> kvp in _edmToSpatialTypesMapping)
        {
            if (edmTypeReference.Definition.IsEquivalentTo(kvp.Key) && (!edmTypeReference.IsNullable || IsNullable(kvp.Value)))
            {
                clrType = kvp.Value;
                return true;
            }
        }

        return false;
    }

    private static IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind primitiveKind)
    {
        return _coreModel.GetPrimitiveType(primitiveKind);
    }

    private static bool IsNullable(Type type)
    {
        Debug.Assert(type != null, "Type should not be null.");

        if (!type.IsValueType)
        {
            return true; // Reference types are nullable
        }

        return Nullable.GetUnderlyingType(type) != null; // Nullable value types
    }
}
