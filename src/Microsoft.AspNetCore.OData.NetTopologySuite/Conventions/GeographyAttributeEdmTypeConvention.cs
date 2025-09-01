//-----------------------------------------------------------------------------
// <copyright file="GeographyAttributeEdmTypeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Attributes;
using Microsoft.AspNetCore.OData.NetTopologySuite.Common;
using Microsoft.AspNetCore.OData.NetTopologySuite.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Conventions.Attributes;
using Geometry = NetTopologySuite.Geometries.Geometry;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Conventions;

/// <summary>
/// A convention that applies to entity or complex types decorated with the <see cref="GeographyAttribute"/>.
/// When applied, all NetTopologySuite spatial properties on the type are mapped to the corresponding
/// OData geography Edm types (for example, <c>Point</c> → <c>Edm.GeographyPoint</c>,
/// <c>LineString</c> → <c>Edm.GeographyLineString</c>), rather than geometry types.
/// </summary>
public class GeographyAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeographyAttributeEdmTypeConvention"/> class.
    /// Configures the convention to match types annotated with <see cref="GeographyAttribute"/> and
    /// to map all NetTopologySuite spatial properties on those types to Edm.Geography* primitive kinds.
    /// </summary>
    public GeographyAttributeEdmTypeConvention()
        : base(attribute => attribute.GetType() == typeof(GeographyAttribute), allowMultiple: false)
    {

    }

    /// <inheritdoc/>
    public override void Apply(StructuralTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model, Attribute attribute)
    {
        if (edmTypeConfiguration == null)
        {
            throw Error.ArgumentNull(nameof(edmTypeConfiguration));
        }

        if (model == null)
        {
            throw Error.ArgumentNull(nameof(model));
        }

        foreach (PropertyConfiguration edmProperty in edmTypeConfiguration.Properties)
        {
            if (edmProperty.AddedExplicitly)
            {
                continue;
            }

            if (edmProperty is PrimitivePropertyConfiguration primitiveProperty
                && typeof(Geometry).IsAssignableFrom(edmProperty.RelatedClrType))
            {
                EdmPrimitiveTypeKind? targetPrimitiveTypeKind = EdmSpatialKindMapper.GetGeographyKindForClrType(edmProperty.RelatedClrType);
                if (targetPrimitiveTypeKind.HasValue)
                {
                    primitiveProperty.AsSpatial(targetPrimitiveTypeKind.Value);
                }
            }
            else if (edmProperty is CollectionPropertyConfiguration collectionProperty
                && typeof(Geometry).IsAssignableFrom(collectionProperty.RelatedClrType))
            {
                EdmPrimitiveTypeKind? elementGeographyKind =
                    EdmSpatialKindMapper.GetGeographyKindForClrType(collectionProperty.RelatedClrType);

                if (elementGeographyKind.HasValue)
                {
                    collectionProperty.AsSpatial(elementGeographyKind.Value);
                }
            }
        }
    }
}
