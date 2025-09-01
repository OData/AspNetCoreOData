//-----------------------------------------------------------------------------
// <copyright file="GeographyAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Conventions;
using Microsoft.OData.Edm;
using Geometry = NetTopologySuite.Geometries.Geometry;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Attributes;

/// <summary>
/// Marks a property or CLR type so that NetTopologySuite geometry values are modeled as Edm geography types.
/// </summary>
/// <remarks>
/// - When applied to a property whose CLR type derives from <see cref="Geometry"/>,
///   the property is mapped to the corresponding <see cref="EdmPrimitiveTypeKind"/> in the Edm.Geography* family
///   (for example, Point → GeographyPoint).
/// - When applied to a class, all properties on the type whose CLR types derive from
///   <see cref="Geometry"/> are treated as Edm.Geography*.
/// The behavior is enforced by the conventions
/// <see cref="GeographyAttributeEdmPropertyConvention"/> and
/// <see cref="GeographyAttributeEdmTypeConvention"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class GeographyAttribute : Attribute
{
}
