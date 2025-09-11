//-----------------------------------------------------------------------------
// <copyright file="GeographyAttributeEdmPropertyConvention.cs" company=".NET Foundation">
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
/// A convention that applies to individual properties decorated with the <see cref="GeographyAttribute"/>.
/// If the property’s CLR type is a NetTopologySuite spatial type, it is mapped to the corresponding
/// OData geography Edm type (for example, <c>Point</c> → <c>Edm.GeographyPoint</c>,
/// <c>LineString</c> → <c>Edm.GeographyLineString</c>).
/// </summary>
public class GeographyAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeographyAttributeEdmPropertyConvention"/> class.
    /// Configures the convention to match properties annotated with <see cref="GeographyAttribute"/> and
    /// to map NetTopologySuite spatial CLR types to their Edm.Geography* primitive kinds.
    /// </summary>
    public GeographyAttributeEdmPropertyConvention()
        : base(attribute => attribute.GetType() == typeof(GeographyAttribute), allowMultiple: false)
    {

    }

    /// <inheritdoc/>
    public override void Apply(PropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute, ODataConventionModelBuilder model)
    {
        if (edmProperty == null)
        {
            throw Error.ArgumentNull(nameof(edmProperty));
        }

        if (structuralTypeConfiguration == null)
        {
            throw Error.ArgumentNull(nameof(structuralTypeConfiguration));
        }

        if (model == null)
        {
            throw Error.ArgumentNull(nameof(model));
        }

        // If the property was added explicitly, we do not apply the convention.
        if (edmProperty.AddedExplicitly)
        {
            return;
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
